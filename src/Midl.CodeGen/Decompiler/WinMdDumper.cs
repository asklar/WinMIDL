// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;

namespace Midl.CodeGen.Decompiler;

/// <summary>
/// Produces a canonical, sorted metadata dump of all public WinRT types in a .winmd file.
/// Useful for debugging and automated comparison.
/// </summary>
public static class WinMdDumper
{
    private const int TdWindowsRuntime = 0x4000;

    public static string Dump(string winmdPath)
    {
        var bytes = File.ReadAllBytes(winmdPath);
        return Dump(bytes);
    }

    public static string Dump(byte[] winmdBytes)
    {
        using var peReader = new PEReader(new MemoryStream(winmdBytes));
        var reader = peReader.GetMetadataReader(MetadataReaderOptions.None);
        return DumpFromReader(reader);
    }

    private static string DumpFromReader(MetadataReader reader)
    {
        var sb = new StringBuilder();
        var sigProvider = new DumperSignatureTypeProvider(reader);
        var types = new List<(string FullName, TypeDefinitionHandle Handle)>();

        foreach (var handle in reader.TypeDefinitions)
        {
            var td = reader.GetTypeDefinition(handle);
            var name = reader.GetString(td.Name);
            if (name == "<Module>") continue;

            var attrs = td.Attributes;
            if (((int)attrs & TdWindowsRuntime) == 0) continue;
            if ((attrs & TypeAttributes.VisibilityMask) != TypeAttributes.Public) continue;

            var ns = reader.GetString(td.Namespace);
            var fullName = string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";
            types.Add((fullName, handle));
        }

        types.Sort((a, b) => string.Compare(a.FullName, b.FullName, StringComparison.Ordinal));

        for (int t = 0; t < types.Count; t++)
        {
            if (t > 0) sb.AppendLine();
            DumpType(reader, types[t].Handle, types[t].FullName, sb, sigProvider);
        }

        return sb.ToString();
    }

    private static void DumpType(MetadataReader reader, TypeDefinitionHandle handle,
        string fullName, StringBuilder sb, DumperSignatureTypeProvider sigProvider)
    {
        var td = reader.GetTypeDefinition(handle);
        var kind = GetTypeKind(reader, td);

        sb.AppendLine($"[{kind}] {fullName}");

        // Base type
        var baseType = GetBaseTypeName(reader, td.BaseType);
        if (baseType != null)
            sb.AppendLine($"  Base: {baseType}");

        // Flags
        var flagStr = FormatFlags(td.Attributes);
        sb.AppendLine($"  Flags: {flagStr}");

        // Interfaces
        var interfaces = new List<string>();
        foreach (var ifaceImplHandle in td.GetInterfaceImplementations())
        {
            var ifaceImpl = reader.GetInterfaceImplementation(ifaceImplHandle);
            var ifaceName = GetEntityName(reader, ifaceImpl.Interface);
            var modifiers = GetInterfaceModifiers(reader, ifaceImplHandle);
            var entry = string.IsNullOrEmpty(modifiers) ? ifaceName : $"{ifaceName} [{modifiers}]";
            interfaces.Add(entry);
        }
        if (interfaces.Count > 0)
        {
            interfaces.Sort(StringComparer.Ordinal);
            sb.AppendLine("  Interfaces:");
            foreach (var iface in interfaces)
                sb.AppendLine($"    {iface}");
        }

        // Fields
        var fields = new List<string>();
        foreach (var fieldHandle in td.GetFields())
        {
            var field = reader.GetFieldDefinition(fieldHandle);
            var fieldName = reader.GetString(field.Name);
            var fieldType = DecodeFieldType(reader, field);
            fields.Add($"{fieldType} {fieldName}");
        }
        if (fields.Count > 0)
        {
            fields.Sort(StringComparer.Ordinal);
            sb.AppendLine("  Fields:");
            foreach (var f in fields)
                sb.AppendLine($"    {f}");
        }

        // Methods
        var methods = new List<string>();
        foreach (var methodHandle in td.GetMethods())
        {
            var method = reader.GetMethodDefinition(methodHandle);
            var methodName = reader.GetString(method.Name);
            var sig = method.DecodeSignature(sigProvider, null);
            var returnType = sig.ReturnType;
            var parameters = FormatParameters(reader, method, sig);
            methods.Add($"{returnType} {methodName}({parameters})");
        }
        if (methods.Count > 0)
        {
            methods.Sort(StringComparer.Ordinal);
            sb.AppendLine("  Methods:");
            foreach (var m in methods)
                sb.AppendLine($"    {m}");
        }

        // Properties
        var properties = new List<string>();
        foreach (var propHandle in td.GetProperties())
        {
            var prop = reader.GetPropertyDefinition(propHandle);
            var propName = reader.GetString(prop.Name);
            var propSig = prop.DecodeSignature(sigProvider, null);
            var propType = propSig.ReturnType;
            var accessors = GetPropertyAccessors(reader, prop);
            properties.Add($"{propType} {propName} {{ {accessors} }}");
        }
        if (properties.Count > 0)
        {
            properties.Sort(StringComparer.Ordinal);
            sb.AppendLine("  Properties:");
            foreach (var p in properties)
                sb.AppendLine($"    {p}");
        }

        // Events
        var events = new List<string>();
        foreach (var eventHandle in td.GetEvents())
        {
            var evt = reader.GetEventDefinition(eventHandle);
            var evtName = reader.GetString(evt.Name);
            var delegateType = GetEntityName(reader, evt.Type);
            events.Add($"{delegateType} {evtName}");
        }
        if (events.Count > 0)
        {
            events.Sort(StringComparer.Ordinal);
            sb.AppendLine("  Events:");
            foreach (var e in events)
                sb.AppendLine($"    {e}");
        }

        // Custom attributes
        var customAttrs = new List<string>();
        foreach (var attrHandle in td.GetCustomAttributes())
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            var formatted = FormatCustomAttribute(reader, attr);
            if (formatted != null)
                customAttrs.Add(formatted);
        }
        if (customAttrs.Count > 0)
        {
            customAttrs.Sort(StringComparer.Ordinal);
            sb.AppendLine("  CustomAttributes:");
            foreach (var a in customAttrs)
                sb.AppendLine($"    {a}");
        }
    }

    private static string GetTypeKind(MetadataReader reader, TypeDefinition td)
    {
        var baseType = GetBaseTypeName(reader, td.BaseType);
        return baseType switch
        {
            "System.Enum" => "Enum",
            "System.ValueType" => "Struct",
            "System.MulticastDelegate" => "Delegate",
            "System.Attribute" => "Attribute",
            _ when (td.Attributes & TypeAttributes.Interface) != 0 => "Interface",
            _ => "RuntimeClass",
        };
    }

    private static string FormatFlags(TypeAttributes attrs)
    {
        var flags = new List<string>();
        if ((attrs & TypeAttributes.VisibilityMask) == TypeAttributes.Public) flags.Add("Public");
        if ((attrs & TypeAttributes.VisibilityMask) == TypeAttributes.NotPublic) flags.Add("NotPublic");
        if ((attrs & TypeAttributes.Abstract) != 0) flags.Add("Abstract");
        if ((attrs & TypeAttributes.Sealed) != 0) flags.Add("Sealed");
        if ((attrs & TypeAttributes.Interface) != 0) flags.Add("Interface");
        if (((int)attrs & TdWindowsRuntime) != 0) flags.Add("WindowsRuntime");
        if ((attrs & TypeAttributes.SpecialName) != 0) flags.Add("SpecialName");
        if ((attrs & TypeAttributes.SpecialName) != 0) flags.Add("SpecialName");
        flags.Sort(StringComparer.Ordinal);
        return string.Join(", ", flags);
    }

    private static string GetInterfaceModifiers(MetadataReader reader, InterfaceImplementationHandle handle)
    {
        var ifaceImpl = reader.GetInterfaceImplementation(handle);
        var mods = new List<string>();
        foreach (var attrHandle in ifaceImpl.GetCustomAttributes())
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            var attrName = GetAttributeTypeName(reader, attr.Constructor);
            if (attrName == "DefaultAttribute") mods.Add("Default");
            else if (attrName == "OverridableAttribute") mods.Add("Overridable");
            else if (attrName == "ProtectedAttribute") mods.Add("Protected");
            else if (attrName == "StaticAttribute") mods.Add("Static");
        }
        mods.Sort(StringComparer.Ordinal);
        return string.Join(", ", mods);
    }

    private static string GetPropertyAccessors(MetadataReader reader, PropertyDefinition prop)
    {
        var accessors = prop.GetAccessors();
        var parts = new List<string>();
        if (!accessors.Getter.IsNil) parts.Add("get;");
        if (!accessors.Setter.IsNil) parts.Add("set;");
        return string.Join(" ", parts);
    }

    private static string? FormatCustomAttribute(MetadataReader reader, CustomAttribute attr)
    {
        var attrName = GetAttributeTypeName(reader, attr.Constructor);
        if (attrName == null) return null;

        // Strip "Attribute" suffix for readability
        if (attrName.EndsWith("Attribute"))
            attrName = attrName[..^"Attribute".Length];

        var args = DecodeAttributeArguments(reader, attr);
        if (args.Count > 0)
            return $"[{attrName}({string.Join(", ", args)})]";
        return $"[{attrName}]";
    }

    private static List<string> DecodeAttributeArguments(MetadataReader reader, CustomAttribute attr)
    {
        var args = new List<string>();
        try
        {
            var blob = reader.GetBlobReader(attr.Value);
            if (blob.Length < 2) return args;
            blob.ReadUInt16(); // prolog

            var attrName = GetAttributeTypeName(reader, attr.Constructor);
            if (attrName == "GuidAttribute" && blob.RemainingBytes >= 16)
            {
                uint a = blob.ReadUInt32();
                ushort b = blob.ReadUInt16();
                ushort c = blob.ReadUInt16();
                byte d = blob.ReadByte();
                byte e = blob.ReadByte();
                byte f = blob.ReadByte();
                byte g = blob.ReadByte();
                byte h = blob.ReadByte();
                byte ii = blob.ReadByte();
                byte j = blob.ReadByte();
                byte k = blob.ReadByte();
                var guid = new Guid(a, b, c, d, e, f, g, h, ii, j, k);
                args.Add(guid.ToString("D"));
            }
            else if (attrName == "ContractVersionAttribute")
            {
                // Try string + uint variant first
                try
                {
                    var contractName = ReadSerializedString(ref blob);
                    if (contractName != null && blob.RemainingBytes >= 4)
                    {
                        var version = blob.ReadUInt32();
                        args.Add(contractName);
                        args.Add(version.ToString());
                    }
                }
                catch
                {
                    // Might be uint-only variant — reset not possible, skip
                }
            }
            else if (attrName is "ThreadingAttribute" or "MarshalingBehaviorAttribute" or "ActivatableAttribute")
            {
                if (blob.RemainingBytes >= 4)
                {
                    var val = blob.ReadUInt32();
                    args.Add(val.ToString());
                }
            }
            else if (attrName == "VersionAttribute")
            {
                if (blob.RemainingBytes >= 4)
                {
                    var val = blob.ReadUInt32();
                    args.Add(val.ToString());
                }
            }
        }
        catch
        {
            // Best-effort decoding
        }
        return args;
    }

    private static string? ReadSerializedString(ref BlobReader blob)
    {
        if (blob.RemainingBytes == 0) return null;
        var marker = blob.ReadByte();
        if (marker == 0xFF) return null;
        if (marker == 0x00) return string.Empty;

        int length = marker;
        if ((marker & 0x80) != 0)
        {
            if ((marker & 0xC0) == 0x80)
                length = ((marker & 0x3F) << 8) | blob.ReadByte();
            else
                length = ((marker & 0x1F) << 24) | (blob.ReadByte() << 16) | (blob.ReadByte() << 8) | blob.ReadByte();
        }

        if (blob.RemainingBytes < length) return null;
        var bytes = blob.ReadBytes(length);
        return Encoding.UTF8.GetString(bytes);
    }

    #region Name resolution helpers

    private static string DecodeFieldType(MetadataReader reader, FieldDefinition field)
    {
        var sigProvider = new DumperSignatureTypeProvider(reader);
        var blob = reader.GetBlobReader(field.Signature);
        blob.ReadSignatureHeader();
        return DecodeTypeFromBlob(reader, ref blob);
    }

    private static string DecodeTypeFromBlob(MetadataReader reader, ref BlobReader blob)
    {
        int typeCode = blob.ReadCompressedInteger();
        if (typeCode is 0x11 or 0x12)
        {
            int coded = blob.ReadCompressedInteger();
            int tag = coded & 0x3;
            int index = coded >> 2;
            if (tag == 0)
                return GetTypeDefFullName(reader, MetadataTokens.TypeDefinitionHandle(index));
            if (tag == 1)
                return GetTypeRefFullName(reader, MetadataTokens.TypeReferenceHandle(index));
            return "Object";
        }
        return typeCode switch
        {
            0x01 => "void",
            0x02 => "Boolean",
            0x03 => "Char16",
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
    }

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
            HandleKind.TypeSpecification => DecodeTypeSpec(reader, handle),
            _ => string.Empty,
        };
    }

    private static string DecodeTypeSpec(MetadataReader reader, EntityHandle handle)
    {
        var spec = reader.GetTypeSpecification((TypeSpecificationHandle)handle);
        return spec.DecodeSignature(new DumperSignatureTypeProvider(reader), null);
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

    private static string FormatParameters(MetadataReader reader, MethodDefinition method,
        MethodSignature<string> sig)
    {
        var parts = new List<string>();
        var paramHandles = method.GetParameters();
        var parameters = new List<(int Seq, string Name)>();
        foreach (var ph in paramHandles)
        {
            try
            {
                var p = reader.GetParameter(ph);
                if (p.SequenceNumber > 0)
                    parameters.Add((p.SequenceNumber, reader.GetString(p.Name)));
            }
            catch { }
        }

        for (int i = 0; i < sig.ParameterTypes.Length; i++)
        {
            var typeName = sig.ParameterTypes[i];
            var match = parameters.FirstOrDefault(p => p.Seq == i + 1);
            var paramName = match.Name ?? $"param{i + 1}";
            parts.Add($"{typeName} {paramName}");
        }

        return string.Join(", ", parts);
    }

    #endregion

    /// <summary>
    /// ISignatureTypeProvider that decodes types to full name strings for dump output.
    /// </summary>
    private sealed class DumperSignatureTypeProvider : ISignatureTypeProvider<string, object?>
    {
        private readonly MetadataReader _reader;
        public DumperSignatureTypeProvider(MetadataReader reader) => _reader = reader;

        public string GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode switch
        {
            PrimitiveTypeCode.Void => "void",
            PrimitiveTypeCode.Boolean => "Boolean",
            PrimitiveTypeCode.Char => "Char16",
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
            => GetTypeDefFullName(reader, handle);

        public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
            => GetTypeRefFullName(reader, handle);

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
}
