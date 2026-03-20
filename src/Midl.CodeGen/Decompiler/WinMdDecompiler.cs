using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;

namespace Midl.CodeGen.Decompiler;

/// <summary>
/// Decompiles a WinRT metadata (.winmd) file back into MIDL3 IDL syntax.
/// </summary>
public class WinMdDecompiler
{
    private const int TdWindowsRuntime = 0x4000;

    public string Decompile(string winmdPath)
    {
        var bytes = File.ReadAllBytes(winmdPath);
        return Decompile(bytes);
    }

    public string Decompile(byte[] winmdBytes)
    {
        using var peReader = new PEReader(new MemoryStream(winmdBytes));
        var reader = peReader.GetMetadataReader();
        return DecompileFromReader(reader);
    }

    private string DecompileFromReader(MetadataReader reader)
    {
        var sb = new StringBuilder();
        var typesByNamespace = new Dictionary<string, List<TypeDefinitionHandle>>();

        foreach (var handle in reader.TypeDefinitions)
        {
            var td = reader.GetTypeDefinition(handle);
            var name = reader.GetString(td.Name);
            if (name == "<Module>") continue;

            var attrs = td.Attributes;
            // Skip non-WinRT types
            if (((int)attrs & TdWindowsRuntime) == 0) continue;

            // Skip non-public synthesized interfaces (I{ClassName} with ExclusiveTo)
            if ((attrs & TypeAttributes.VisibilityMask) != TypeAttributes.Public)
                continue;

            var ns = reader.GetString(td.Namespace);
            if (!typesByNamespace.TryGetValue(ns, out var list))
            {
                list = new List<TypeDefinitionHandle>();
                typesByNamespace[ns] = list;
            }
            list.Add(handle);
        }

        bool first = true;
        foreach (var (ns, types) in typesByNamespace.OrderBy(kv => kv.Key))
        {
            if (!first) sb.AppendLine();
            first = false;

            sb.AppendLine($"namespace {ns}");
            sb.AppendLine("{");

            for (int i = 0; i < types.Count; i++)
            {
                if (i > 0) sb.AppendLine();
                DecompileType(reader, types[i], sb, indent: "    ");
            }

            sb.AppendLine("}");
        }

        return sb.ToString();
    }

    private void DecompileType(MetadataReader reader, TypeDefinitionHandle handle, StringBuilder sb, string indent)
    {
        var td = reader.GetTypeDefinition(handle);
        var name = reader.GetString(td.Name);
        var baseTypeName = GetBaseTypeName(reader, td.BaseType);

        switch (baseTypeName)
        {
            case "System.Enum":
                DecompileEnum(reader, td, handle, name, sb, indent);
                break;
            case "System.ValueType":
                DecompileStruct(reader, td, name, sb, indent);
                break;
            case "System.MulticastDelegate":
                DecompileDelegate(reader, td, name, sb, indent);
                break;
            case "System.Attribute":
                // Skip WinRT attribute types in decompiled output
                break;
            default:
                if ((td.Attributes & TypeAttributes.Interface) != 0)
                {
                    // Skip non-public (exclusive/synthesized) interfaces — they'll be re-synthesized
                    if ((td.Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NotPublic)
                        break;
                    DecompileInterface(reader, td, handle, name, sb, indent);
                }
                else
                    DecompileRuntimeClass(reader, td, handle, name, sb, indent);
                break;
        }
    }

    #region Enum

    private void DecompileEnum(MetadataReader reader, TypeDefinition td, TypeDefinitionHandle handle,
        string name, StringBuilder sb, string indent)
    {
        bool isFlags = HasCustomAttribute(reader, handle, "System", "FlagsAttribute");
        if (isFlags)
            sb.AppendLine($"{indent}[flags]");

        sb.AppendLine($"{indent}enum {name}");
        sb.AppendLine($"{indent}{{");

        var fields = td.GetFields().Select(f => reader.GetFieldDefinition(f)).ToList();
        var members = fields.Where(f => reader.GetString(f.Name) != "value__").ToList();
        for (int i = 0; i < members.Count; i++)
        {
            var fieldName = reader.GetString(members[i].Name);
            var comma = i < members.Count - 1 ? "," : ",";
            sb.AppendLine($"{indent}    {fieldName}{comma}");
        }

        sb.AppendLine($"{indent}}};");
    }

    #endregion

    #region Struct

    private void DecompileStruct(MetadataReader reader, TypeDefinition td,
        string name, StringBuilder sb, string indent)
    {
        sb.AppendLine($"{indent}struct {name}");
        sb.AppendLine($"{indent}{{");

        foreach (var fieldHandle in td.GetFields())
        {
            var field = reader.GetFieldDefinition(fieldHandle);
            var fieldName = reader.GetString(field.Name);
            var typeName = DecodeFieldType(reader, field);
            sb.AppendLine($"{indent}    {typeName} {fieldName};");
        }

        sb.AppendLine($"{indent}}};");
    }

    #endregion

    #region Interface

    private void DecompileInterface(MetadataReader reader, TypeDefinition td, TypeDefinitionHandle handle,
        string name, StringBuilder sb, string indent)
    {
        // Emit uuid attribute if present
        var guid = ReadGuidAttribute(reader, handle);
        if (guid.HasValue)
            sb.AppendLine($"{indent}[uuid({FormatGuid(guid.Value)})]");

        // Check for requires
        var requires = new List<string>();
        foreach (var ifaceImplHandle in td.GetInterfaceImplementations())
        {
            var ifaceImpl = reader.GetInterfaceImplementation(ifaceImplHandle);
            var ifaceName = GetEntityName(reader, ifaceImpl.Interface);
            if (!string.IsNullOrEmpty(ifaceName))
                requires.Add(GetShortName(ifaceName));
        }

        var requiresClause = requires.Count > 0 ? $" requires {string.Join(", ", requires)}" : "";
        sb.AppendLine($"{indent}interface {name}{requiresClause}");
        sb.AppendLine($"{indent}{{");

        // Collect accessor method names to skip
        var accessorNames = new HashSet<string>();
        foreach (var propHandle in td.GetProperties())
        {
            var prop = reader.GetPropertyDefinition(propHandle);
            var propName = reader.GetString(prop.Name);
            accessorNames.Add($"get_{propName}");
            accessorNames.Add($"put_{propName}");
        }
        foreach (var eventHandle in td.GetEvents())
        {
            var evt = reader.GetEventDefinition(eventHandle);
            var evtName = reader.GetString(evt.Name);
            accessorNames.Add($"add_{evtName}");
            accessorNames.Add($"remove_{evtName}");
        }

        // Methods
        foreach (var methodHandle in td.GetMethods())
        {
            var method = reader.GetMethodDefinition(methodHandle);
            var methodName = reader.GetString(method.Name);
            if (accessorNames.Contains(methodName)) continue;

            var sig = method.DecodeSignature(new DecompilerSignatureTypeProvider(reader), null);
            var returnType = MapToMidlType(sig.ReturnType);
            var parameters = FormatParameters(reader, method, sig);
            sb.AppendLine($"{indent}    {returnType} {methodName}({parameters});");
        }

        // Properties
        foreach (var propHandle in td.GetProperties())
        {
            var prop = reader.GetPropertyDefinition(propHandle);
            var propName = reader.GetString(prop.Name);
            var propSig = prop.DecodeSignature(new DecompilerSignatureTypeProvider(reader), null);
            var propType = MapToMidlType(propSig.ReturnType);
            sb.AppendLine($"{indent}    {propType} {propName};");
        }

        // Events
        foreach (var eventHandle in td.GetEvents())
        {
            var evt = reader.GetEventDefinition(eventHandle);
            var evtName = reader.GetString(evt.Name);
            var delegateType = GetEntityName(reader, evt.Type);
            sb.AppendLine($"{indent}    event {GetShortName(delegateType)} {evtName};");
        }

        sb.AppendLine($"{indent}}};");
    }

    #endregion

    #region Delegate

    private void DecompileDelegate(MetadataReader reader, TypeDefinition td,
        string name, StringBuilder sb, string indent)
    {
        // Find Invoke method
        foreach (var methodHandle in td.GetMethods())
        {
            var method = reader.GetMethodDefinition(methodHandle);
            var methodName = reader.GetString(method.Name);
            if (methodName != "Invoke") continue;

            var sig = method.DecodeSignature(new DecompilerSignatureTypeProvider(reader), null);
            var returnType = MapToMidlType(sig.ReturnType);
            var parameters = FormatParameters(reader, method, sig);
            sb.AppendLine($"{indent}delegate {returnType} {name}({parameters});");
            return;
        }

        // Fallback
        sb.AppendLine($"{indent}delegate void {name}();");
    }

    #endregion

    #region RuntimeClass

    private void DecompileRuntimeClass(MetadataReader reader, TypeDefinition td, TypeDefinitionHandle handle,
        string name, StringBuilder sb, string indent)
    {
        // Check for attributes to emit
        EmitContractAttribute(reader, handle, sb, indent);

        // Check if sealed/static
        bool isSealed = td.Attributes.HasFlag(TypeAttributes.Sealed);
        bool isStatic = td.Attributes.HasFlag(TypeAttributes.Abstract);

        var keyword = isStatic ? "static runtimeclass" : "runtimeclass";
        if (!isSealed && !isStatic) keyword = "unsealed runtimeclass";

        sb.AppendLine($"{indent}{keyword} {name}");
        sb.AppendLine($"{indent}{{");

        // Constructors
        foreach (var methodHandle in td.GetMethods())
        {
            var method = reader.GetMethodDefinition(methodHandle);
            var methodName = reader.GetString(method.Name);
            if (methodName != ".ctor") continue;

            var sig = method.DecodeSignature(new DecompilerSignatureTypeProvider(reader), null);
            var parameters = FormatParameters(reader, method, sig);
            sb.AppendLine($"{indent}    {name}({parameters});");
        }

        // Detect properties and events by naming convention (runtimeclass TypeDefs may not have PropertyMap)
        var accessorNames = new HashSet<string>();
        var properties = new Dictionary<string, (string Type, bool HasGet, bool HasSet)>();
        var events = new Dictionary<string, string>();

        // First: check PropertyMap/EventMap (may be populated)
        foreach (var propHandle in td.GetProperties())
        {
            var prop = reader.GetPropertyDefinition(propHandle);
            var propName = reader.GetString(prop.Name);
            var propSig = prop.DecodeSignature(new DecompilerSignatureTypeProvider(reader), null);
            accessorNames.Add($"get_{propName}");
            accessorNames.Add($"put_{propName}");
            properties[propName] = (MapToMidlType(propSig.ReturnType), true, false);
        }
        foreach (var eventHandle in td.GetEvents())
        {
            var evt = reader.GetEventDefinition(eventHandle);
            var evtName = reader.GetString(evt.Name);
            var delegateType = GetEntityName(reader, evt.Type);
            accessorNames.Add($"add_{evtName}");
            accessorNames.Add($"remove_{evtName}");
            events[evtName] = GetShortName(delegateType);
        }

        // Second: detect by naming convention (for methods not covered by PropertyMap)
        foreach (var methodHandle in td.GetMethods())
        {
            var method = reader.GetMethodDefinition(methodHandle);
            var methodName = reader.GetString(method.Name);
            if (methodName == ".ctor") continue;

            if (methodName.StartsWith("get_") && !accessorNames.Contains(methodName))
            {
                var propName = methodName[4..];
                accessorNames.Add(methodName);
                var sig = method.DecodeSignature(new DecompilerSignatureTypeProvider(reader), null);
                var propType = MapToMidlType(sig.ReturnType);
                if (properties.ContainsKey(propName))
                    properties[propName] = (propType, true, properties[propName].HasSet);
                else
                    properties[propName] = (propType, true, false);
            }
            else if (methodName.StartsWith("put_") && !accessorNames.Contains(methodName))
            {
                var propName = methodName[4..];
                accessorNames.Add(methodName);
                var sig = method.DecodeSignature(new DecompilerSignatureTypeProvider(reader), null);
                var propType = sig.ParameterTypes.Length > 0 ? MapToMidlType(sig.ParameterTypes[0]) : "Object";
                if (properties.ContainsKey(propName))
                    properties[propName] = (properties[propName].Type, properties[propName].HasGet, true);
                else
                    properties[propName] = (propType, false, true);
            }
            else if (methodName.StartsWith("add_") && !accessorNames.Contains(methodName))
            {
                var evtName = methodName[4..];
                accessorNames.Add(methodName);
                accessorNames.Add($"remove_{evtName}");
                var sig = method.DecodeSignature(new DecompilerSignatureTypeProvider(reader), null);
                var delegateType = sig.ParameterTypes.Length > 0 ? MapToMidlType(sig.ParameterTypes[0]) : "Object";
                events.TryAdd(evtName, delegateType);
            }
            else if (methodName.StartsWith("remove_") && !accessorNames.Contains(methodName))
            {
                accessorNames.Add(methodName);
                accessorNames.Add($"add_{methodName[7..]}");
            }
        }

        // Emit methods (skip constructors and accessors)
        foreach (var methodHandle in td.GetMethods())
        {
            var method = reader.GetMethodDefinition(methodHandle);
            var methodName = reader.GetString(method.Name);
            if (methodName == ".ctor") continue;
            if (accessorNames.Contains(methodName)) continue;

            var sig = method.DecodeSignature(new DecompilerSignatureTypeProvider(reader), null);
            var returnType = MapToMidlType(sig.ReturnType);
            var parameters = FormatParameters(reader, method, sig);
            sb.AppendLine($"{indent}    {returnType} {methodName}({parameters});");
        }

        // Emit properties (idiomatic MIDL3 syntax)
        foreach (var (propName, (propType, hasGet, hasSet)) in properties)
        {
            if (hasGet && hasSet)
                sb.AppendLine($"{indent}    {propType} {propName};");
            else if (hasGet)
                sb.AppendLine($"{indent}    {propType} {propName} {{ get; }};");
            else
                sb.AppendLine($"{indent}    {propType} {propName} {{ set; }};");
        }

        // Emit events (idiomatic MIDL3 syntax)
        foreach (var (evtName, delegateType) in events)
        {
            sb.AppendLine($"{indent}    event {delegateType} {evtName};");
        }

        sb.AppendLine($"{indent}}};");
    }

    #endregion

    #region Attribute helpers

    private void EmitContractAttribute(MetadataReader reader, TypeDefinitionHandle handle, StringBuilder sb, string indent)
    {
        var td = reader.GetTypeDefinition(handle);
        foreach (var attrHandle in td.GetCustomAttributes())
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            var attrName = GetAttributeTypeName(reader, attr.Constructor);
            if (attrName == "ContractVersionAttribute")
            {
                // Try to read contract name and version from blob
                var blob = reader.GetBlobReader(attr.Value);
                if (blob.Length >= 2)
                {
                    blob.ReadUInt16(); // prolog
                    // First parameter could be string (contract name) or uint (version)
                    // For the named variant: string contractName, uint version
                    try
                    {
                        var contractName = ReadSerializedString(ref blob);
                        if (contractName != null && blob.RemainingBytes >= 4)
                        {
                            var version = blob.ReadUInt32();
                            sb.AppendLine($"{indent}[contract({contractName}, {version})]");
                            return;
                        }
                    }
                    catch
                    {
                        // Fall through — might be the uint-only variant
                    }
                }
            }
        }
    }

    private static Guid? ReadGuidAttribute(MetadataReader reader, TypeDefinitionHandle handle)
    {
        var td = reader.GetTypeDefinition(handle);
        foreach (var attrHandle in td.GetCustomAttributes())
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            var ctorName = GetAttributeTypeName(reader, attr.Constructor);
            if (ctorName != "GuidAttribute") continue;

            var blob = reader.GetBlobReader(attr.Value);
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

    private static bool HasCustomAttribute(MetadataReader reader, TypeDefinitionHandle handle, string ns, string attrTypeName)
    {
        var td = reader.GetTypeDefinition(handle);
        foreach (var attrHandle in td.GetCustomAttributes())
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            if (attr.Constructor.Kind == HandleKind.MemberReference)
            {
                var memberRef = reader.GetMemberReference((MemberReferenceHandle)attr.Constructor);
                if (memberRef.Parent.Kind == HandleKind.TypeReference)
                {
                    var typeRef = reader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
                    if (reader.GetString(typeRef.Name) == attrTypeName &&
                        reader.GetString(typeRef.Namespace) == ns)
                        return true;
                }
            }
        }
        return false;
    }

    private static string? GetAttributeTypeName(MetadataReader reader, EntityHandle ctorHandle)
    {
        if (ctorHandle.Kind == HandleKind.MemberReference)
        {
            var memberRef = reader.GetMemberReference((MemberReferenceHandle)ctorHandle);
            if (memberRef.Parent.Kind == HandleKind.TypeReference)
            {
                var typeRef = reader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
                return reader.GetString(typeRef.Name);
            }
        }
        return null;
    }

    private static string? ReadSerializedString(ref BlobReader blob)
    {
        if (blob.RemainingBytes == 0) return null;
        var marker = blob.ReadByte();
        if (marker == 0xFF) return null;
        if (marker == 0x00) return string.Empty;

        // The marker is actually the first byte of a compressed length
        int length = marker;
        if ((marker & 0x80) != 0)
        {
            if ((marker & 0xC0) == 0x80)
            {
                length = ((marker & 0x3F) << 8) | blob.ReadByte();
            }
            else
            {
                length = ((marker & 0x1F) << 24) | (blob.ReadByte() << 16) | (blob.ReadByte() << 8) | blob.ReadByte();
            }
        }

        if (blob.RemainingBytes < length) return null;
        var bytes = blob.ReadBytes(length);
        return Encoding.UTF8.GetString(bytes);
    }

    #endregion

    #region Type resolution helpers

    private static string? GetBaseTypeName(MetadataReader reader, EntityHandle baseTypeHandle)
    {
        if (baseTypeHandle.IsNil) return null;
        return baseTypeHandle.Kind switch
        {
            HandleKind.TypeReference => GetTypeRefFullName(reader, (TypeReferenceHandle)baseTypeHandle),
            HandleKind.TypeDefinition => GetTypeDefFullName(reader, (TypeDefinitionHandle)baseTypeHandle),
            _ => null,
        };
    }

    private static string GetTypeRefFullName(MetadataReader reader, TypeReferenceHandle handle)
    {
        var typeRef = reader.GetTypeReference(handle);
        var ns = reader.GetString(typeRef.Namespace);
        var name = reader.GetString(typeRef.Name);
        return string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";
    }

    private static string GetTypeDefFullName(MetadataReader reader, TypeDefinitionHandle handle)
    {
        var typeDef = reader.GetTypeDefinition(handle);
        var ns = reader.GetString(typeDef.Namespace);
        var name = reader.GetString(typeDef.Name);
        return string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";
    }

    private static string GetEntityName(MetadataReader reader, EntityHandle handle)
    {
        if (handle.IsNil) return string.Empty;
        return handle.Kind switch
        {
            HandleKind.TypeReference => GetTypeRefFullName(reader, (TypeReferenceHandle)handle),
            HandleKind.TypeDefinition => GetTypeDefFullName(reader, (TypeDefinitionHandle)handle),
            _ => string.Empty,
        };
    }

    private static string GetShortName(string fullName)
    {
        var dot = fullName.LastIndexOf('.');
        return dot >= 0 ? fullName[(dot + 1)..] : fullName;
    }

    private static string FormatGuid(Guid guid)
    {
        return guid.ToString("D");
    }

    #endregion

    #region Signature helpers

    private static string DecodeFieldType(MetadataReader reader, FieldDefinition field)
    {
        var blob = reader.GetBlobReader(field.Signature);
        blob.ReadSignatureHeader();
        return DecodeType(reader, ref blob);
    }

    private static string DecodeType(MetadataReader reader, ref BlobReader blob)
    {
        int typeCode = blob.ReadCompressedInteger();
        return typeCode switch
        {
            0x11 or 0x12 => DecodeTypeDefOrRefEncoded(reader, ref blob),
            _ => MapElementTypeToMidl(typeCode),
        };
    }

    private static string DecodeTypeDefOrRefEncoded(MetadataReader reader, ref BlobReader blob)
    {
        int coded = blob.ReadCompressedInteger();
        int tag = coded & 0x3;
        int index = coded >> 2;

        if (tag == 0)
            return GetShortName(GetTypeDefFullName(reader, MetadataTokens.TypeDefinitionHandle(index)));
        if (tag == 1)
            return GetShortName(GetTypeRefFullName(reader, MetadataTokens.TypeReferenceHandle(index)));

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

    private static string MapToMidlType(string typeName) => typeName switch
    {
        "System.Guid" or "Guid" => "Guid",
        "System.Object" => "Object",
        _ when typeName.Contains('.') => GetShortName(typeName),
        _ => typeName,
    };

    private static string FormatParameters(MetadataReader reader, MethodDefinition method,
        MethodSignature<string> sig)
    {
        var parts = new List<string>();
        
        // Safely collect parameters — some methods may have no parameter rows
        var paramHandles = method.GetParameters();
        var parameters = new List<(int Seq, string Name)>();
        foreach (var ph in paramHandles)
        {
            try
            {
                var p = reader.GetParameter(ph);
                if (p.SequenceNumber > 0) // Skip return value (sequence 0)
                    parameters.Add((p.SequenceNumber, reader.GetString(p.Name)));
            }
            catch
            {
                // Skip invalid parameter handles
            }
        }

        for (int i = 0; i < sig.ParameterTypes.Length; i++)
        {
            var typeName = MapToMidlType(sig.ParameterTypes[i]);
            var match = parameters.FirstOrDefault(p => p.Seq == i + 1);
            var paramName = match.Name ?? $"param{i + 1}";
            parts.Add($"{typeName} {paramName}");
        }

        return string.Join(", ", parts);
    }

    #endregion

    /// <summary>
    /// ISignatureTypeProvider that decodes types to string names for decompilation.
    /// </summary>
    private sealed class DecompilerSignatureTypeProvider : ISignatureTypeProvider<string, object?>
    {
        private readonly MetadataReader _reader;

        public DecompilerSignatureTypeProvider(MetadataReader reader) => _reader = reader;

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
        public string GetTypeFromSpecification(MetadataReader reader, object? genericContext,
            TypeSpecificationHandle handle, byte rawTypeKind)
        {
            var spec = reader.GetTypeSpecification(handle);
            return spec.DecodeSignature(this, genericContext);
        }
    }

    #region Synthesis detection helpers

    /// <summary>
    /// Check if an interface name matches the auto-synthesis pattern for a given runtimeclass.
    /// </summary>
    private static bool IsSynthesizedInterfaceName(string interfaceName, string className)
    {
        return interfaceName == $"I{className}"
            || interfaceName == $"I{className}Factory"
            || interfaceName == $"I{className}Statics"
            || interfaceName == $"I{className}Protected"
            || interfaceName == $"I{className}Overrides"
            || (interfaceName.StartsWith($"I{className}") && interfaceName.Length > className.Length + 1
                && char.IsDigit(interfaceName[$"I{className}".Length]));
    }

    /// <summary>
    /// Get the runtimeclass name from an ExclusiveToAttribute on an interface.
    /// </summary>
    private static string? GetExclusiveToClassName(MetadataReader reader, TypeDefinitionHandle handle)
    {
        var td = reader.GetTypeDefinition(handle);
        foreach (var attrHandle in td.GetCustomAttributes())
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            var attrTypeName = GetAttributeTypeName(reader, attr.Constructor);
            if (attrTypeName is "ExclusiveToAttribute")
            {
                // Decode the blob to get the type name
                try
                {
                    var blobReader = reader.GetBlobReader(attr.Value);
                    blobReader.ReadUInt16(); // prolog
                    var typeName = blobReader.ReadSerializedString();
                    if (typeName != null)
                    {
                        // Extract short name from fully qualified
                        var lastDot = typeName.LastIndexOf('.');
                        return lastDot >= 0 ? typeName[(lastDot + 1)..] : typeName;
                    }
                }
                catch { }
            }
        }
        return null;
    }

    #endregion
}
