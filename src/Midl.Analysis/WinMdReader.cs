using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Midl.Analysis.Symbols;

namespace Midl.Analysis;

/// <summary>
/// Reads WinRT types from .winmd files and converts them to MIDL symbols.
/// </summary>
public class WinMdReader
{
    private const int TdWindowsRuntime = 0x4000;

    /// <summary>Read all public WinRT types from a .winmd file path.</summary>
    public IReadOnlyList<ISymbol> ReadTypes(string winmdPath)
    {
        var bytes = File.ReadAllBytes(winmdPath);
        return ReadTypes(bytes, winmdPath);
    }

    /// <summary>Read all public WinRT types from raw bytes.</summary>
    public IReadOnlyList<ISymbol> ReadTypes(byte[] winmdBytes)
    {
        return ReadTypes(winmdBytes, null);
    }

    private IReadOnlyList<ISymbol> ReadTypes(byte[] winmdBytes, string? sourcePath)
    {
        var symbols = new List<ISymbol>();

        using var peReader = new PEReader(new MemoryStream(winmdBytes));
        var mdReader = peReader.GetMetadataReader();

        foreach (var typeDefHandle in mdReader.TypeDefinitions)
        {
            var typeDef = mdReader.GetTypeDefinition(typeDefHandle);

            // Skip <Module> and non-public types
            var name = mdReader.GetString(typeDef.Name);
            if (name == "<Module>") continue;

            var attrs = typeDef.Attributes;
            if ((attrs & TypeAttributes.VisibilityMask) != TypeAttributes.Public) continue;

            // Only process WinRT types
            if (((int)attrs & TdWindowsRuntime) == 0) continue;

            var ns = mdReader.GetString(typeDef.Namespace);
            var symbol = ReadTypeDefAsSymbol(mdReader, typeDef, typeDefHandle, name, ns, sourcePath);
            if (symbol != null)
                symbols.Add(symbol);
        }

        return symbols;
    }

    private static ISymbol? ReadTypeDefAsSymbol(
        MetadataReader mdReader, TypeDefinition typeDef, TypeDefinitionHandle handle,
        string name, string ns, string? sourcePath)
    {
        var baseTypeHandle = typeDef.BaseType;
        var baseTypeName = GetBaseTypeName(mdReader, baseTypeHandle);

        return baseTypeName switch
        {
            "System.Enum" => ReadEnum(mdReader, typeDef, handle, name, ns, sourcePath),
            "System.ValueType" => ReadStruct(mdReader, typeDef, name, ns, sourcePath),
            "System.MulticastDelegate" => ReadDelegate(mdReader, typeDef, name, ns, sourcePath),
            "System.Attribute" => ReadAttributeType(mdReader, typeDef, name, ns, sourcePath),
            _ when (typeDef.Attributes & TypeAttributes.Interface) != 0
                => ReadInterface(mdReader, typeDef, handle, name, ns, sourcePath),
            _ => ReadRuntimeClass(mdReader, typeDef, handle, name, ns, sourcePath),
        };
    }

    private static string? GetBaseTypeName(MetadataReader mdReader, EntityHandle baseTypeHandle)
    {
        if (baseTypeHandle.IsNil) return null;

        return baseTypeHandle.Kind switch
        {
            HandleKind.TypeReference => GetTypeRefFullName(mdReader, (TypeReferenceHandle)baseTypeHandle),
            HandleKind.TypeDefinition => GetTypeDefFullName(mdReader, (TypeDefinitionHandle)baseTypeHandle),
            _ => null,
        };
    }

    private static string GetTypeRefFullName(MetadataReader mdReader, TypeReferenceHandle handle)
    {
        var typeRef = mdReader.GetTypeReference(handle);
        var ns = mdReader.GetString(typeRef.Namespace);
        var name = mdReader.GetString(typeRef.Name);
        return string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";
    }

    private static string GetTypeDefFullName(MetadataReader mdReader, TypeDefinitionHandle handle)
    {
        var typeDef = mdReader.GetTypeDefinition(handle);
        var ns = mdReader.GetString(typeDef.Namespace);
        var name = mdReader.GetString(typeDef.Name);
        return string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";
    }

    #region Enum

    private static EnumSymbol ReadEnum(
        MetadataReader mdReader, TypeDefinition typeDef, TypeDefinitionHandle handle,
        string name, string ns, string? sourcePath)
    {
        // Create a placeholder namespace parent for FullName
        var nsSymbol = CreateNamespaceChain(ns);
        var symbol = new EnumSymbol(name, nsSymbol)
        {
            IsExternal = true,
            SourceWinMdPath = sourcePath,
        };

        // Read fields
        foreach (var fieldHandle in typeDef.GetFields())
        {
            var field = mdReader.GetFieldDefinition(fieldHandle);
            var fieldName = mdReader.GetString(field.Name);
            if (fieldName == "value__")
            {
                // Determine underlying type from the value__ field signature
                symbol.UnderlyingType = DecodeFieldUnderlyingType(mdReader, field);
                continue;
            }

            var member = new EnumMemberSymbol(fieldName, symbol)
            {
                IsExternal = true,
                SourceWinMdPath = sourcePath,
            };
            symbol.AddMember(member);
        }

        // Check for [Flags]
        symbol.IsFlags = HasCustomAttribute(mdReader, handle, "System", "FlagsAttribute");

        return symbol;
    }

    private static string DecodeFieldUnderlyingType(MetadataReader mdReader, FieldDefinition field)
    {
        var blob = mdReader.GetBlobReader(field.Signature);
        // Field signature: FIELD (0x06) followed by type
        var header = blob.ReadSignatureHeader();
        var typeCode = blob.ReadCompressedInteger();
        return MapElementTypeToMidl(typeCode);
    }

    #endregion

    #region Struct

    private static StructSymbol ReadStruct(
        MetadataReader mdReader, TypeDefinition typeDef,
        string name, string ns, string? sourcePath)
    {
        var nsSymbol = CreateNamespaceChain(ns);
        var symbol = new StructSymbol(name, nsSymbol)
        {
            IsExternal = true,
            SourceWinMdPath = sourcePath,
        };

        foreach (var fieldHandle in typeDef.GetFields())
        {
            var field = mdReader.GetFieldDefinition(fieldHandle);
            var fieldName = mdReader.GetString(field.Name);

            var typeName = DecodeFieldType(mdReader, field);
            var fieldSymbol = new FieldSymbol(fieldName, symbol)
            {
                Type = new TypeRef(typeName) { IsBuiltIn = IsBuiltInType(typeName) },
                IsExternal = true,
                SourceWinMdPath = sourcePath,
            };
            symbol.AddField(fieldSymbol);
        }

        return symbol;
    }

    private static string DecodeFieldType(MetadataReader mdReader, FieldDefinition field)
    {
        var blob = mdReader.GetBlobReader(field.Signature);
        var header = blob.ReadSignatureHeader();
        return DecodeType(mdReader, ref blob);
    }

    #endregion

    #region Interface

    private static InterfaceSymbol ReadInterface(
        MetadataReader mdReader, TypeDefinition typeDef, TypeDefinitionHandle handle,
        string name, string ns, string? sourcePath)
    {
        var nsSymbol = CreateNamespaceChain(ns);
        var symbol = new InterfaceSymbol(name, nsSymbol)
        {
            IsExternal = true,
            SourceWinMdPath = sourcePath,
        };

        // Read GUID
        symbol.Uuid = ReadGuidAttribute(mdReader, handle);

        // Read methods
        foreach (var methodHandle in typeDef.GetMethods())
        {
            var method = mdReader.GetMethodDefinition(methodHandle);
            var methodName = mdReader.GetString(method.Name);

            // Skip accessors — they will be represented by properties/events
            if (methodName.StartsWith("get_") || methodName.StartsWith("put_") ||
                methodName.StartsWith("add_") || methodName.StartsWith("remove_"))
                continue;

            var methodSymbol = ReadMethod(mdReader, method, methodName, symbol, sourcePath);
            symbol.AddMember(methodSymbol);
        }

        // Read properties
        foreach (var propHandle in typeDef.GetProperties())
        {
            var prop = mdReader.GetPropertyDefinition(propHandle);
            var propSymbol = ReadProperty(mdReader, prop, typeDef, symbol, sourcePath);
            symbol.AddMember(propSymbol);
        }

        // Read events
        foreach (var eventHandle in typeDef.GetEvents())
        {
            var evt = mdReader.GetEventDefinition(eventHandle);
            var evtSymbol = ReadEvent(mdReader, evt, symbol, sourcePath);
            symbol.AddMember(evtSymbol);
        }

        return symbol;
    }

    #endregion

    #region Delegate

    private static DelegateSymbol ReadDelegate(
        MetadataReader mdReader, TypeDefinition typeDef,
        string name, string ns, string? sourcePath)
    {
        var nsSymbol = CreateNamespaceChain(ns);
        var symbol = new DelegateSymbol(name, nsSymbol)
        {
            IsExternal = true,
            SourceWinMdPath = sourcePath,
        };

        // Find the Invoke method
        foreach (var methodHandle in typeDef.GetMethods())
        {
            var method = mdReader.GetMethodDefinition(methodHandle);
            var methodName = mdReader.GetString(method.Name);
            if (methodName != "Invoke") continue;

            var sig = method.DecodeSignature(new MidlSignatureTypeProvider(mdReader), null);

            symbol.ReturnType = new TypeRef(sig.ReturnType) { IsBuiltIn = IsBuiltInType(sig.ReturnType) };

            foreach (var paramHandle in method.GetParameters())
            {
                var param = mdReader.GetParameter(paramHandle);
                if (param.SequenceNumber == 0) continue; // return type pseudo-param
                var paramName = mdReader.GetString(param.Name);
                var paramType = param.SequenceNumber <= sig.ParameterTypes.Length
                    ? sig.ParameterTypes[param.SequenceNumber - 1]
                    : "Object";
                var paramSymbol = new ParameterSymbol(paramName, symbol)
                {
                    Type = new TypeRef(paramType) { IsBuiltIn = IsBuiltInType(paramType) },
                    IsExternal = true,
                    SourceWinMdPath = sourcePath,
                };
                symbol.Parameters.Add(paramSymbol);
            }
            break;
        }

        return symbol;
    }

    #endregion

    #region RuntimeClass

    private static RuntimeClassSymbol ReadRuntimeClass(
        MetadataReader mdReader, TypeDefinition typeDef, TypeDefinitionHandle handle,
        string name, string ns, string? sourcePath)
    {
        var nsSymbol = CreateNamespaceChain(ns);
        var symbol = new RuntimeClassSymbol(name, nsSymbol)
        {
            IsExternal = true,
            SourceWinMdPath = sourcePath,
        };

        // Read implemented interfaces
        foreach (var ifaceImplHandle in typeDef.GetInterfaceImplementations())
        {
            var ifaceImpl = mdReader.GetInterfaceImplementation(ifaceImplHandle);
            var ifaceName = GetEntityName(mdReader, ifaceImpl.Interface);
            if (!string.IsNullOrEmpty(ifaceName))
                symbol.Interfaces.Add(new TypeRef(ifaceName) { IsBuiltIn = false });
        }

        // Read GUID
        symbol.Uuid = ReadGuidAttribute(mdReader, handle);

        return symbol;
    }

    #endregion

    #region Member helpers

    private static MethodSymbol ReadMethod(
        MetadataReader mdReader, MethodDefinition method, string name,
        ISymbol parent, string? sourcePath)
    {
        var sig = method.DecodeSignature(new MidlSignatureTypeProvider(mdReader), null);
        var symbol = new MethodSymbol(name, parent)
        {
            ReturnType = new TypeRef(sig.ReturnType) { IsBuiltIn = IsBuiltInType(sig.ReturnType) },
            IsExternal = true,
            SourceWinMdPath = sourcePath,
        };

        foreach (var paramHandle in method.GetParameters())
        {
            var param = mdReader.GetParameter(paramHandle);
            if (param.SequenceNumber == 0) continue;
            var paramName = mdReader.GetString(param.Name);
            var paramType = param.SequenceNumber <= sig.ParameterTypes.Length
                ? sig.ParameterTypes[param.SequenceNumber - 1]
                : "Object";

            var direction = ParameterDirection.In;
            if ((param.Attributes & ParameterAttributes.Out) != 0)
                direction = ParameterDirection.Out;

            symbol.Parameters.Add(new ParameterSymbol(paramName, symbol)
            {
                Type = new TypeRef(paramType) { IsBuiltIn = IsBuiltInType(paramType) },
                Direction = direction,
                IsExternal = true,
                SourceWinMdPath = sourcePath,
            });
        }

        return symbol;
    }

    private static PropertySymbol ReadProperty(
        MetadataReader mdReader, PropertyDefinition prop,
        TypeDefinition containingType, ISymbol parent, string? sourcePath)
    {
        var propName = mdReader.GetString(prop.Name);
        var propSig = prop.DecodeSignature(new MidlSignatureTypeProvider(mdReader), null);

        var symbol = new PropertySymbol(propName, parent)
        {
            Type = new TypeRef(propSig.ReturnType) { IsBuiltIn = IsBuiltInType(propSig.ReturnType) },
            IsExternal = true,
            SourceWinMdPath = sourcePath,
        };

        // Determine getter/setter by checking for accessor methods
        var accessors = prop.GetAccessors();
        symbol.HasGetter = !accessors.Getter.IsNil;
        symbol.HasSetter = !accessors.Setter.IsNil;

        return symbol;
    }

    private static EventSymbol ReadEvent(
        MetadataReader mdReader, EventDefinition evt,
        ISymbol parent, string? sourcePath)
    {
        var eventName = mdReader.GetString(evt.Name);
        var delegateTypeName = GetEntityName(mdReader, evt.Type);

        return new EventSymbol(eventName, parent)
        {
            DelegateType = new TypeRef(delegateTypeName ?? "Object"),
            IsExternal = true,
            SourceWinMdPath = sourcePath,
        };
    }

    #endregion

    #region Attribute

    private static AttributeTypeSymbol ReadAttributeType(
        MetadataReader mdReader, TypeDefinition typeDef,
        string name, string ns, string? sourcePath)
    {
        var nsSymbol = CreateNamespaceChain(ns);
        var symbol = new AttributeTypeSymbol(name, nsSymbol)
        {
            IsExternal = true,
            SourceWinMdPath = sourcePath,
        };

        // Read fields from the .ctor parameters (attribute ctor params = attribute fields in WinRT)
        foreach (var methodHandle in typeDef.GetMethods())
        {
            var method = mdReader.GetMethodDefinition(methodHandle);
            var methodName = mdReader.GetString(method.Name);
            if (methodName != ".ctor") continue;

            foreach (var paramHandle in method.GetParameters())
            {
                var param = mdReader.GetParameter(paramHandle);
                if (param.SequenceNumber == 0) continue; // return type pseudo-param
                var paramName = mdReader.GetString(param.Name);

                var sig = method.DecodeSignature(new MidlSignatureTypeProvider(mdReader), null);
                var paramType = param.SequenceNumber <= sig.ParameterTypes.Length
                    ? sig.ParameterTypes[param.SequenceNumber - 1]
                    : "Object";

                var fieldSymbol = new FieldSymbol(paramName, symbol)
                {
                    Type = new TypeRef(paramType) { IsBuiltIn = IsBuiltInType(paramType) },
                    IsExternal = true,
                    SourceWinMdPath = sourcePath,
                };
                symbol.AddField(fieldSymbol);
            }
            break; // use the first .ctor
        }

        return symbol;
    }

    #endregion

    #region Type decoding

    private static string DecodeType(MetadataReader mdReader, ref BlobReader blob)
    {
        int typeCode = blob.ReadCompressedInteger();
        return typeCode switch
        {
            // TypeDef/TypeRef encoded
            0x11 or 0x12 => DecodeTypeDefOrRefEncoded(mdReader, ref blob),
            _ => MapElementTypeToMidl(typeCode),
        };
    }

    private static string DecodeTypeDefOrRefEncoded(MetadataReader mdReader, ref BlobReader blob)
    {
        int coded = blob.ReadCompressedInteger();
        int tag = coded & 0x3;
        int index = coded >> 2;

        if (tag == 0) // TypeDef
        {
            var handle = MetadataTokens.TypeDefinitionHandle(index);
            return GetTypeDefFullName(mdReader, handle);
        }
        else if (tag == 1) // TypeRef
        {
            var handle = MetadataTokens.TypeReferenceHandle(index);
            return GetTypeRefFullName(mdReader, handle);
        }

        return "Object";
    }

    private static string MapElementTypeToMidl(int elementType) => elementType switch
    {
        0x01 => "void",
        0x02 => "Boolean",
        0x03 => "Char",
        0x04 => "Int8",
        0x05 => "UInt8",
        0x06 => "Int16",
        0x07 => "UInt16",
        0x08 => "Int32",
        0x09 => "UInt32",
        0x0A => "Int64",
        0x0B => "UInt64",
        0x0C => "Single",
        0x0D => "Double",
        0x0E => "String",
        0x1C => "Object",
        _ => "Object",
    };

    private static string GetEntityName(MetadataReader mdReader, EntityHandle handle)
    {
        if (handle.IsNil) return string.Empty;
        return handle.Kind switch
        {
            HandleKind.TypeReference => GetTypeRefFullName(mdReader, (TypeReferenceHandle)handle),
            HandleKind.TypeDefinition => GetTypeDefFullName(mdReader, (TypeDefinitionHandle)handle),
            _ => string.Empty,
        };
    }

    #endregion

    #region Attribute helpers

    private static Guid? ReadGuidAttribute(MetadataReader mdReader, TypeDefinitionHandle handle)
    {
        var typeDef = mdReader.GetTypeDefinition(handle);
        foreach (var attrHandle in typeDef.GetCustomAttributes())
        {
            var attr = mdReader.GetCustomAttribute(attrHandle);
            var ctorName = GetAttributeTypeName(mdReader, attr.Constructor);
            if (ctorName != "GuidAttribute") continue;

            var blob = mdReader.GetBlobReader(attr.Value);
            if (blob.Length < 2 + 16) continue;
            blob.ReadUInt16(); // prolog

            uint a = blob.ReadUInt32();
            ushort b = blob.ReadUInt16();
            ushort c = blob.ReadUInt16();
            byte d = blob.ReadByte();
            byte e = blob.ReadByte();
            byte f = blob.ReadByte();
            byte g = blob.ReadByte();
            byte h = blob.ReadByte();
            byte i = blob.ReadByte();
            byte j = blob.ReadByte();
            byte k = blob.ReadByte();

            return new Guid(a, b, c, d, e, f, g, h, i, j, k);
        }
        return null;
    }

    private static bool HasCustomAttribute(MetadataReader mdReader, TypeDefinitionHandle handle, string ns, string attrTypeName)
    {
        var typeDef = mdReader.GetTypeDefinition(handle);
        foreach (var attrHandle in typeDef.GetCustomAttributes())
        {
            var attr = mdReader.GetCustomAttribute(attrHandle);
            var name = GetAttributeTypeName(mdReader, attr.Constructor);
            if (name == attrTypeName) return true;
        }
        return false;
    }

    private static string? GetAttributeTypeName(MetadataReader mdReader, EntityHandle ctorHandle)
    {
        if (ctorHandle.Kind == HandleKind.MemberReference)
        {
            var memberRef = mdReader.GetMemberReference((MemberReferenceHandle)ctorHandle);
            if (memberRef.Parent.Kind == HandleKind.TypeReference)
            {
                var typeRef = mdReader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
                return mdReader.GetString(typeRef.Name);
            }
        }
        return null;
    }

    #endregion

    #region Namespace helpers

    private static NamespaceSymbol CreateNamespaceChain(string ns)
    {
        if (string.IsNullOrEmpty(ns)) return new NamespaceSymbol("");

        var parts = ns.Split('.');
        var root = new NamespaceSymbol(parts[0]);
        var current = root;
        for (int i = 1; i < parts.Length; i++)
        {
            current = current.GetOrCreateChildNamespace(parts[i]);
        }
        return current;
    }

    private static bool IsBuiltInType(string name) => name switch
    {
        "Boolean" or "Char" or "Int16" or "Int32" or "Int64"
            or "UInt8" or "UInt16" or "UInt32" or "UInt64"
            or "Single" or "Double" or "String" or "Guid"
            or "Object" or "void" => true,
        _ => false,
    };

    #endregion

    /// <summary>
    /// ISignatureTypeProvider that decodes types to string names.
    /// </summary>
    private sealed class MidlSignatureTypeProvider : ISignatureTypeProvider<string, object?>
    {
        private readonly MetadataReader _reader;

        public MidlSignatureTypeProvider(MetadataReader reader) => _reader = reader;

        public string GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode switch
        {
            PrimitiveTypeCode.Void => "void",
            PrimitiveTypeCode.Boolean => "Boolean",
            PrimitiveTypeCode.Char => "Char",
            PrimitiveTypeCode.SByte => "Int8",
            PrimitiveTypeCode.Byte => "UInt8",
            PrimitiveTypeCode.Int16 => "Int16",
            PrimitiveTypeCode.UInt16 => "UInt16",
            PrimitiveTypeCode.Int32 => "Int32",
            PrimitiveTypeCode.UInt32 => "UInt32",
            PrimitiveTypeCode.Int64 => "Int64",
            PrimitiveTypeCode.UInt64 => "UInt64",
            PrimitiveTypeCode.Single => "Single",
            PrimitiveTypeCode.Double => "Double",
            PrimitiveTypeCode.String => "String",
            PrimitiveTypeCode.Object => "Object",
            PrimitiveTypeCode.IntPtr => "Int64",
            PrimitiveTypeCode.UIntPtr => "UInt64",
            _ => "Object",
        };

        public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
        {
            var td = reader.GetTypeDefinition(handle);
            var ns = reader.GetString(td.Namespace);
            var name = reader.GetString(td.Name);
            return string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";
        }

        public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
        {
            var tr = reader.GetTypeReference(handle);
            var ns = reader.GetString(tr.Namespace);
            var name = reader.GetString(tr.Name);
            return string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";
        }

        public string GetSZArrayType(string elementType) => $"{elementType}[]";
        public string GetArrayType(string elementType, ArrayShape shape) => $"{elementType}[]";
        public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments)
            => $"{genericType}<{string.Join(", ", typeArguments)}>";
        public string GetByReferenceType(string elementType) => elementType;
        public string GetPointerType(string elementType) => elementType;
        public string GetPinnedType(string elementType) => elementType;
        public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) => unmodifiedType;
        public string GetFunctionPointerType(MethodSignature<string> signature) => "Object";
        public string GetGenericMethodParameter(object? genericContext, int index) => $"!!{index}";
        public string GetGenericTypeParameter(object? genericContext, int index) => $"!{index}";
        public string GetTypeFromSpecification(MetadataReader reader, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
        {
            var spec = reader.GetTypeSpecification(handle);
            return spec.DecodeSignature(this, genericContext);
        }
    }
}
