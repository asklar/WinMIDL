// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Midl.CodeGen.Comparison;

/// <summary>
/// Structurally compares two WinRT metadata (.winmd) files using MetadataReader.
/// Ignores token values, string heap offsets, method RVAs, and exact attribute blob encoding.
/// </summary>
public class WinMdComparer
{
    public ComparisonResult Compare(string expectedPath, string actualPath)
    {
        if (!File.Exists(expectedPath))
            return new ComparisonResult { AreEqual = false, Differences = { $"Expected file not found: {expectedPath}" } };
        if (!File.Exists(actualPath))
            return new ComparisonResult { AreEqual = false, Differences = { $"Actual file not found: {actualPath}" } };

        return Compare(File.ReadAllBytes(expectedPath), File.ReadAllBytes(actualPath));
    }

    public ComparisonResult Compare(byte[] expected, byte[] actual)
    {
        var result = new ComparisonResult();

        using var expectedPe = new PEReader(new MemoryStream(expected));
        using var actualPe = new PEReader(new MemoryStream(actual));

        var expectedReader = expectedPe.GetMetadataReader();
        var actualReader = actualPe.GetMetadataReader();

        var expectedTypes = GetTypeInfos(expectedReader);
        var actualTypes = GetTypeInfos(actualReader);

        // Compare type counts
        if (expectedTypes.Count != actualTypes.Count)
        {
            result.Differences.Add(
                $"TypeDef count differs: expected {expectedTypes.Count}, actual {actualTypes.Count}");
        }

        var expectedNames = expectedTypes.Keys.OrderBy(k => k).ToList();
        var actualNames = actualTypes.Keys.OrderBy(k => k).ToList();

        // Types present in expected but missing from actual
        foreach (var name in expectedNames.Except(actualNames))
            result.Differences.Add($"Missing type: {name}");

        // Types present in actual but not in expected
        foreach (var name in actualNames.Except(expectedNames))
            result.Differences.Add($"Extra type: {name}");

        // Compare common types
        foreach (var name in expectedNames.Intersect(actualNames))
        {
            var exp = expectedTypes[name];
            var act = actualTypes[name];
            CompareTypeInfo(name, exp, act, result);
        }

        result.AreEqual = result.Differences.Count == 0;
        return result;
    }

    public int CountTypes(byte[] winmdBytes)
    {
        using var pe = new PEReader(new MemoryStream(winmdBytes));
        var reader = pe.GetMetadataReader();
        return GetTypeInfos(reader).Count;
    }

    public int CountTypes(string winmdPath)
    {
        if (!File.Exists(winmdPath))
            return 0;
        return CountTypes(File.ReadAllBytes(winmdPath));
    }

    private static void CompareTypeInfo(string typeName, TypeInfo expected, TypeInfo actual, ComparisonResult result)
    {
        // Flags
        if (expected.Flags != actual.Flags)
            result.Differences.Add($"Type '{typeName}' flags differ: expected {expected.Flags}, actual {actual.Flags}");

        // Base type
        if (expected.BaseType != actual.BaseType)
            result.Differences.Add($"Type '{typeName}' base type differs: expected '{expected.BaseType}', actual '{actual.BaseType}'");

        // Methods
        CompareNamedItems(typeName, "method", expected.Methods, actual.Methods, result);

        // Properties
        CompareStringLists(typeName, "property", expected.Properties, actual.Properties, result);

        // Events
        CompareStringLists(typeName, "event", expected.Events, actual.Events, result);

        // Interface implementations
        CompareStringLists(typeName, "interface impl", expected.InterfaceImpls, actual.InterfaceImpls, result);

        // Custom attributes (just check type names exist)
        CompareStringLists(typeName, "custom attribute", expected.CustomAttributes, actual.CustomAttributes, result);
    }

    private static void CompareNamedItems(string typeName, string kind,
        List<string> expected, List<string> actual, ComparisonResult result)
    {
        var expectedSorted = expected.OrderBy(x => x).ToList();
        var actualSorted = actual.OrderBy(x => x).ToList();

        foreach (var item in expectedSorted.Except(actualSorted))
            result.Differences.Add($"Type '{typeName}' missing {kind}: {item}");

        foreach (var item in actualSorted.Except(expectedSorted))
            result.Differences.Add($"Type '{typeName}' extra {kind}: {item}");
    }

    private static void CompareStringLists(string typeName, string kind,
        List<string> expected, List<string> actual, ComparisonResult result)
    {
        var expectedSorted = expected.OrderBy(x => x).ToList();
        var actualSorted = actual.OrderBy(x => x).ToList();

        foreach (var item in expectedSorted.Except(actualSorted))
            result.Differences.Add($"Type '{typeName}' missing {kind}: {item}");

        foreach (var item in actualSorted.Except(expectedSorted))
            result.Differences.Add($"Type '{typeName}' extra {kind}: {item}");
    }

    private static Dictionary<string, TypeInfo> GetTypeInfos(MetadataReader reader)
    {
        var types = new Dictionary<string, TypeInfo>();

        foreach (var handle in reader.TypeDefinitions)
        {
            var td = reader.GetTypeDefinition(handle);
            var name = reader.GetString(td.Name);
            if (name == "<Module>") continue;

            var ns = reader.GetString(td.Namespace);
            var fullName = string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";

            var info = new TypeInfo
            {
                Flags = td.Attributes,
                BaseType = ResolveEntityName(reader, td.BaseType),
                Methods = GetMethodSignatures(reader, td),
                Properties = GetPropertyNames(reader, td),
                Events = GetEventNames(reader, td),
                InterfaceImpls = GetInterfaceImplNames(reader, handle),
                CustomAttributes = GetCustomAttributeTypeNames(reader, handle)
            };

            types[fullName] = info;
        }

        return types;
    }

    private static List<string> GetMethodSignatures(MetadataReader reader, TypeDefinition td)
    {
        var methods = new List<string>();
        foreach (var mh in td.GetMethods())
        {
            var method = reader.GetMethodDefinition(mh);
            var methodName = reader.GetString(method.Name);
            var paramCount = method.GetParameters().Count;

            // Decode return type
            var sig = method.DecodeSignature(new SimpleSignatureProvider(), null);
            var returnType = sig.ReturnType;

            methods.Add($"{returnType} {methodName}({paramCount} params)");
        }
        return methods;
    }

    private static List<string> GetPropertyNames(MetadataReader reader, TypeDefinition td)
    {
        var props = new List<string>();
        foreach (var ph in td.GetProperties())
        {
            var prop = reader.GetPropertyDefinition(ph);
            props.Add(reader.GetString(prop.Name));
        }
        return props;
    }

    private static List<string> GetEventNames(MetadataReader reader, TypeDefinition td)
    {
        var events = new List<string>();
        foreach (var eh in td.GetEvents())
        {
            var evt = reader.GetEventDefinition(eh);
            events.Add(reader.GetString(evt.Name));
        }
        return events;
    }

    private static List<string> GetInterfaceImplNames(MetadataReader reader, TypeDefinitionHandle tdHandle)
    {
        var impls = new List<string>();
        var td = reader.GetTypeDefinition(tdHandle);
        foreach (var ih in td.GetInterfaceImplementations())
        {
            var impl = reader.GetInterfaceImplementation(ih);
            impls.Add(ResolveEntityName(reader, impl.Interface));
        }
        return impls;
    }

    private static List<string> GetCustomAttributeTypeNames(MetadataReader reader, TypeDefinitionHandle tdHandle)
    {
        var attrs = new List<string>();
        foreach (var attrHandle in reader.GetCustomAttributes(tdHandle))
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            var ctorName = ResolveAttributeConstructorTypeName(reader, attr.Constructor);
            if (ctorName != null)
                attrs.Add(ctorName);
        }
        return attrs;
    }

    private static string? ResolveAttributeConstructorTypeName(MetadataReader reader, EntityHandle constructor)
    {
        if (constructor.Kind == HandleKind.MemberReference)
        {
            var memberRef = reader.GetMemberReference((MemberReferenceHandle)constructor);
            return ResolveEntityName(reader, memberRef.Parent);
        }
        if (constructor.Kind == HandleKind.MethodDefinition)
        {
            var methodDef = reader.GetMethodDefinition((MethodDefinitionHandle)constructor);
            var declaringType = methodDef.GetDeclaringType();
            var td = reader.GetTypeDefinition(declaringType);
            var ns = reader.GetString(td.Namespace);
            var name = reader.GetString(td.Name);
            return string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";
        }
        return null;
    }

    private static string ResolveEntityName(MetadataReader reader, EntityHandle handle)
    {
        if (handle.IsNil) return "";

        switch (handle.Kind)
        {
            case HandleKind.TypeReference:
                var typeRef = reader.GetTypeReference((TypeReferenceHandle)handle);
                var trNs = reader.GetString(typeRef.Namespace);
                var trName = reader.GetString(typeRef.Name);
                return string.IsNullOrEmpty(trNs) ? trName : $"{trNs}.{trName}";

            case HandleKind.TypeDefinition:
                var typeDef = reader.GetTypeDefinition((TypeDefinitionHandle)handle);
                var tdNs = reader.GetString(typeDef.Namespace);
                var tdName = reader.GetString(typeDef.Name);
                return string.IsNullOrEmpty(tdNs) ? tdName : $"{tdNs}.{tdName}";

            case HandleKind.TypeSpecification:
                var typeSpec = reader.GetTypeSpecification((TypeSpecificationHandle)handle);
                try
                {
                    return typeSpec.DecodeSignature(new SimpleSignatureProvider(), null);
                }
                catch
                {
                    return "<TypeSpec>";
                }

            default:
                return $"<{handle.Kind}>";
        }
    }

    private class TypeInfo
    {
        public TypeAttributes Flags { get; init; }
        public string BaseType { get; init; } = "";
        public List<string> Methods { get; init; } = new();
        public List<string> Properties { get; init; } = new();
        public List<string> Events { get; init; } = new();
        public List<string> InterfaceImpls { get; init; } = new();
        public List<string> CustomAttributes { get; init; } = new();
    }

    /// <summary>
    /// Minimal ISignatureTypeProvider for decoding type signatures into readable strings.
    /// </summary>
    private class SimpleSignatureProvider : ISignatureTypeProvider<string, object?>
    {
        public string GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode switch
        {
            PrimitiveTypeCode.Void => "void",
            PrimitiveTypeCode.Boolean => "Boolean",
            PrimitiveTypeCode.Char => "Char",
            PrimitiveTypeCode.SByte => "SByte",
            PrimitiveTypeCode.Byte => "Byte",
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
            PrimitiveTypeCode.IntPtr => "IntPtr",
            PrimitiveTypeCode.UIntPtr => "UIntPtr",
            _ => typeCode.ToString()
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
        public string GetArrayType(string elementType, ArrayShape shape) => $"{elementType}[{shape.Rank}]";
        public string GetByReferenceType(string elementType) => $"{elementType}&";
        public string GetPointerType(string elementType) => $"{elementType}*";
        public string GetGenericInstantiation(string genericType, System.Collections.Immutable.ImmutableArray<string> typeArguments)
            => $"{genericType}<{string.Join(", ", typeArguments)}>";
        public string GetGenericMethodParameter(object? genericContext, int index) => $"!!{index}";
        public string GetGenericTypeParameter(object? genericContext, int index) => $"!{index}";
        public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) => unmodifiedType;
        public string GetPinnedType(string elementType) => elementType;
        public string GetTypeFromSpecification(MetadataReader reader, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
        {
            var spec = reader.GetTypeSpecification(handle);
            return spec.DecodeSignature(this, genericContext);
        }
        public string GetFunctionPointerType(MethodSignature<string> signature) => "fnptr";
    }
}

public class ComparisonResult
{
    public bool AreEqual { get; set; }
    public List<string> Differences { get; init; } = new();
}
