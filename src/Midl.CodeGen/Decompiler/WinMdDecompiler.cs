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

    public string Decompile(string winmdPath, string? namespaceFilter = null)
    {
        var bytes = File.ReadAllBytes(winmdPath);
        return Decompile(bytes, namespaceFilter);
    }

    public string Decompile(byte[] winmdBytes, string? namespaceFilter = null)
    {
        using var peReader = new PEReader(new MemoryStream(winmdBytes));
        var reader = peReader.GetMetadataReader();
        return DecompileFromReader(reader, namespaceFilter);
    }

    private string DecompileFromReader(MetadataReader reader, string? namespaceFilter = null)
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
            if (namespaceFilter != null && !ns.Equals(namespaceFilter, StringComparison.Ordinal))
                continue;
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

    // Stores generic type parameter names for the currently decompiling type
    private string[]? _currentGenericParamNames;

    private void DecompileType(MetadataReader reader, TypeDefinitionHandle handle, StringBuilder sb, string indent)
    {
        var td = reader.GetTypeDefinition(handle);
        var rawName = reader.GetString(td.Name);

        // Clean up generic type names: TypedEventHandler`2 → TypedEventHandler<TSender, TResult>
        string name;
        var genericParams = td.GetGenericParameters();
        int backtickIdx = rawName.IndexOf('`');
        if (backtickIdx >= 0 && genericParams.Count > 0)
        {
            var baseName = rawName[..backtickIdx];
            _currentGenericParamNames = genericParams
                .Select(gp => reader.GetString(reader.GetGenericParameter(gp).Name)).ToArray();
            name = baseName + "<" + string.Join(", ", _currentGenericParamNames) + ">";
        }
        else
        {
            _currentGenericParamNames = null;
            name = rawName;
        }

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
                DecompileDelegate(reader, td, handle, name, sb, indent);
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
                else if (IsApiContract(reader, td, handle))
                    DecompileApiContract(reader, td, handle, name, sb, indent);
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
        // Emit all attributes (uuid, exclusiveto, deprecated, version, contract, etc.)
        EmitAttributes(reader, handle, sb, indent);

        // Emit uuid attribute if present (always emit separately for interfaces)
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

            var sig = method.DecodeSignature(new DecompilerSignatureTypeProvider(reader), _currentGenericParamNames);
            var returnType = MapToMidlType(sig.ReturnType);
            var parameters = FormatParameters(reader, method, sig);
            sb.AppendLine($"{indent}    {returnType} {methodName}({parameters});");
        }

        // Properties
        foreach (var propHandle in td.GetProperties())
        {
            var prop = reader.GetPropertyDefinition(propHandle);
            var propName = reader.GetString(prop.Name);
            var propSig = prop.DecodeSignature(new DecompilerSignatureTypeProvider(reader), _currentGenericParamNames);
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

    private void DecompileDelegate(MetadataReader reader, TypeDefinition td, TypeDefinitionHandle handle,
        string name, StringBuilder sb, string indent)
    {
        EmitAttributes(reader, handle, sb, indent);

        // Find Invoke method
        foreach (var methodHandle in td.GetMethods())
        {
            var method = reader.GetMethodDefinition(methodHandle);
            var methodName = reader.GetString(method.Name);
            if (methodName != "Invoke") continue;

            var sig = method.DecodeSignature(new DecompilerSignatureTypeProvider(reader), _currentGenericParamNames);
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
        // Emit all custom attributes
        EmitAttributes(reader, handle, sb, indent);

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

            var sig = method.DecodeSignature(new DecompilerSignatureTypeProvider(reader), _currentGenericParamNames);
            var parameters = FormatParameters(reader, method, sig);
            sb.AppendLine($"{indent}    {name}({parameters});");
        }

        // Collect names of static method stubs to skip (handled by EmitStaticMembers)
        var staticMethodNames = new HashSet<string>();
        foreach (var methodHandle in td.GetMethods())
        {
            var method = reader.GetMethodDefinition(methodHandle);
            if (method.Attributes.HasFlag(MethodAttributes.Static))
                staticMethodNames.Add(reader.GetString(method.Name));
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
            if (staticMethodNames.Contains($"get_{propName}") || staticMethodNames.Contains($"put_{propName}"))
                continue;
            var propSig = prop.DecodeSignature(new DecompilerSignatureTypeProvider(reader), _currentGenericParamNames);
            accessorNames.Add($"get_{propName}");
            accessorNames.Add($"put_{propName}");
            properties[propName] = (MapToMidlType(propSig.ReturnType), true, false);
        }
        foreach (var eventHandle in td.GetEvents())
        {
            var evt = reader.GetEventDefinition(eventHandle);
            var evtName = reader.GetString(evt.Name);
            if (staticMethodNames.Contains($"add_{evtName}"))
                continue;
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
            if (method.Attributes.HasFlag(MethodAttributes.Static)) continue;

            if (methodName.StartsWith("get_") && !accessorNames.Contains(methodName))
            {
                var propName = methodName[4..];
                accessorNames.Add(methodName);
                var sig = method.DecodeSignature(new DecompilerSignatureTypeProvider(reader), _currentGenericParamNames);
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
                var sig = method.DecodeSignature(new DecompilerSignatureTypeProvider(reader), _currentGenericParamNames);
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
                var sig = method.DecodeSignature(new DecompilerSignatureTypeProvider(reader), _currentGenericParamNames);
                var delegateType = sig.ParameterTypes.Length > 0 ? MapToMidlType(sig.ParameterTypes[0]) : "Object";
                events.TryAdd(evtName, delegateType);
            }
            else if (methodName.StartsWith("remove_") && !accessorNames.Contains(methodName))
            {
                accessorNames.Add(methodName);
                accessorNames.Add($"add_{methodName[7..]}");
            }
        }

        // Emit methods (skip constructors, accessors, and static stubs)
        foreach (var methodHandle in td.GetMethods())
        {
            var method = reader.GetMethodDefinition(methodHandle);
            var methodName = reader.GetString(method.Name);
            if (methodName == ".ctor") continue;
            if (accessorNames.Contains(methodName)) continue;
            if (method.Attributes.HasFlag(MethodAttributes.Static)) continue;

            var sig = method.DecodeSignature(new DecompilerSignatureTypeProvider(reader), _currentGenericParamNames);
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

        // Emit static members from [static()] interfaces
        EmitStaticMembers(reader, handle, sb, indent);

        sb.AppendLine($"{indent}}};");
    }

    private void EmitStaticMembers(MetadataReader reader, TypeDefinitionHandle classHandle, StringBuilder sb, string indent)
    {
        var td = reader.GetTypeDefinition(classHandle);
        foreach (var attrHandle in td.GetCustomAttributes())
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            var attrName = GetAttributeTypeName(reader, attr.Constructor);
            if (attrName != "StaticAttribute") continue;

            try
            {
                var blob = reader.GetBlobReader(attr.Value);
                blob.ReadUInt16(); // prolog
                var staticInterfaceName = ReadSerializedString(ref blob);
                if (staticInterfaceName == null) continue;

                var staticsTdHandle = FindTypeDefByFullName(reader, staticInterfaceName);
                if (staticsTdHandle == null) continue;

                var staticsTypeDef = reader.GetTypeDefinition(staticsTdHandle.Value);
                EmitMembersFromInterface(reader, staticsTypeDef, sb, indent, isStatic: true);
            }
            catch { /* best-effort attribute decoding */ }
        }
    }

    private static TypeDefinitionHandle? FindTypeDefByFullName(MetadataReader reader, string fullName)
    {
        foreach (var handle in reader.TypeDefinitions)
        {
            var td = reader.GetTypeDefinition(handle);
            var ns = reader.GetString(td.Namespace);
            var name = reader.GetString(td.Name);
            var thisFullName = string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";
            if (thisFullName == fullName)
                return handle;
        }
        return null;
    }

    private void EmitMembersFromInterface(MetadataReader reader, TypeDefinition td, StringBuilder sb, string indent, bool isStatic)
    {
        var prefix = isStatic ? "static " : "";
        var accessorNames = new HashSet<string>();
        var properties = new Dictionary<string, (string Type, bool HasGet, bool HasSet)>();
        var events = new Dictionary<string, string>();

        // Collect from PropertyMap/EventMap
        foreach (var propHandle in td.GetProperties())
        {
            var prop = reader.GetPropertyDefinition(propHandle);
            var propName = reader.GetString(prop.Name);
            var propSig = prop.DecodeSignature(new DecompilerSignatureTypeProvider(reader), _currentGenericParamNames);
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

        // Detect by naming convention
        foreach (var methodHandle in td.GetMethods())
        {
            var method = reader.GetMethodDefinition(methodHandle);
            var methodName = reader.GetString(method.Name);
            if (methodName == ".ctor") continue;

            if (methodName.StartsWith("get_") && !accessorNames.Contains(methodName))
            {
                var propName = methodName[4..];
                accessorNames.Add(methodName);
                var sig = method.DecodeSignature(new DecompilerSignatureTypeProvider(reader), _currentGenericParamNames);
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
                var sig = method.DecodeSignature(new DecompilerSignatureTypeProvider(reader), _currentGenericParamNames);
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
                var sig = method.DecodeSignature(new DecompilerSignatureTypeProvider(reader), _currentGenericParamNames);
                var delegateType = sig.ParameterTypes.Length > 0 ? MapToMidlType(sig.ParameterTypes[0]) : "Object";
                events.TryAdd(evtName, delegateType);
            }
            else if (methodName.StartsWith("remove_") && !accessorNames.Contains(methodName))
            {
                accessorNames.Add(methodName);
                accessorNames.Add($"add_{methodName[7..]}");
            }
        }

        // Emit methods
        foreach (var methodHandle in td.GetMethods())
        {
            var method = reader.GetMethodDefinition(methodHandle);
            var methodName = reader.GetString(method.Name);
            if (methodName == ".ctor") continue;
            if (accessorNames.Contains(methodName)) continue;

            var sig = method.DecodeSignature(new DecompilerSignatureTypeProvider(reader), _currentGenericParamNames);
            var returnType = MapToMidlType(sig.ReturnType);
            var parameters = FormatParameters(reader, method, sig);
            sb.AppendLine($"{indent}    {prefix}{returnType} {methodName}({parameters});");
        }

        // Emit properties
        foreach (var (propName, (propType, hasGet, hasSet)) in properties)
        {
            if (hasGet && hasSet)
                sb.AppendLine($"{indent}    {prefix}{propType} {propName};");
            else if (hasGet)
                sb.AppendLine($"{indent}    {prefix}{propType} {propName} {{ get; }};");
            else
                sb.AppendLine($"{indent}    {prefix}{propType} {propName} {{ set; }};");
        }

        // Emit events
        foreach (var (evtName, delegateType) in events)
        {
            sb.AppendLine($"{indent}    {prefix}event {delegateType} {evtName};");
        }
    }

    #endregion

    #region Attribute helpers

    /// <summary>
    /// Reads all well-known custom attributes from a type and emits them as MIDL3 syntax.
    /// Handles: threading, marshaling_behavior, composable, activatable, static,
    /// deprecated, exclusiveto, webhosthidden, version, contract, contractversion.
    /// GuidAttribute and FlagsAttribute are handled separately.
    /// </summary>
    private void EmitAttributes(MetadataReader reader, TypeDefinitionHandle handle, StringBuilder sb, string indent)
    {
        var td = reader.GetTypeDefinition(handle);
        foreach (var attrHandle in td.GetCustomAttributes())
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            var attrName = GetAttributeTypeName(reader, attr.Constructor);
            if (attrName == null) continue;

            switch (attrName)
            {
                case "ThreadingAttribute":
                    EmitThreadingAttribute(reader, attr, sb, indent);
                    break;
                case "MarshalingBehaviorAttribute":
                    EmitMarshalingBehaviorAttribute(reader, attr, sb, indent);
                    break;
                case "ComposableAttribute":
                    EmitComposableAttribute(reader, attr, sb, indent);
                    break;
                case "ActivatableAttribute":
                    EmitActivatableAttribute(reader, attr, sb, indent);
                    break;
                case "StaticAttribute":
                    EmitStaticAttribute(reader, attr, sb, indent);
                    break;
                case "DeprecatedAttribute":
                    EmitDeprecatedAttribute(reader, attr, sb, indent);
                    break;
                case "ExclusiveToAttribute":
                    EmitExclusiveToAttribute(reader, attr, sb, indent);
                    break;
                case "WebHostHiddenAttribute":
                    sb.AppendLine($"{indent}[webhosthidden]");
                    break;
                case "VersionAttribute":
                    EmitVersionAttribute(reader, attr, sb, indent);
                    break;
                case "ContractVersionAttribute":
                    EmitContractVersionAttribute(reader, attr, sb, indent);
                    break;
            }
        }
    }

    private static void EmitThreadingAttribute(MetadataReader reader, CustomAttribute attr, StringBuilder sb, string indent)
    {
        var blob = reader.GetBlobReader(attr.Value);
        if (blob.Length < 6) return; // prolog(2) + int32(4)
        blob.ReadUInt16(); // prolog
        var value = blob.ReadInt32();
        var model = value switch { 1 => "sta", 2 => "mta", 3 => "both", _ => null };
        if (model != null)
            sb.AppendLine($"{indent}[threading({model})]");
    }

    private static void EmitMarshalingBehaviorAttribute(MetadataReader reader, CustomAttribute attr, StringBuilder sb, string indent)
    {
        var blob = reader.GetBlobReader(attr.Value);
        if (blob.Length < 6) return;
        blob.ReadUInt16(); // prolog
        var value = blob.ReadInt32();
        var behavior = value switch { 0 => "none", 1 => "standard", 2 => "agile", _ => null };
        if (behavior != null)
            sb.AppendLine($"{indent}[marshaling_behavior({behavior})]");
    }

    private static void EmitComposableAttribute(MetadataReader reader, CustomAttribute attr, StringBuilder sb, string indent)
    {
        try
        {
            var blob = reader.GetBlobReader(attr.Value);
            if (blob.Length < 2) return;
            blob.ReadUInt16(); // prolog
            var factoryType = ReadSerializedString(ref blob);
            if (factoryType != null && blob.RemainingBytes >= 8)
            {
                var access = blob.ReadInt32();
                var version = blob.ReadUInt32();
                var accessStr = access == 2 ? "protected" : "public";
                sb.AppendLine($"{indent}[composable({GetShortName(factoryType)}, {accessStr}, {version})]");
            }
        }
        catch { /* best-effort attribute decoding */ }
    }

    private static void EmitActivatableAttribute(MetadataReader reader, CustomAttribute attr, StringBuilder sb, string indent)
    {
        try
        {
            int paramCount = GetCtorParameterCount(reader, attr.Constructor);
            var blob = reader.GetBlobReader(attr.Value);
            if (blob.Length < 2) return;
            blob.ReadUInt16(); // prolog

            if (paramCount >= 3)
            {
                // Factory variant: (Type factoryType, UInt32 version, String platform)
                var factoryType = ReadSerializedString(ref blob);
                if (factoryType != null && blob.RemainingBytes >= 4)
                {
                    var version = blob.ReadUInt32();
                    sb.AppendLine($"{indent}[activatable({GetShortName(factoryType)}, {version})]");
                }
            }
            else
            {
                // Simple variant: (UInt32 version, String platform)
                if (blob.RemainingBytes >= 4)
                {
                    var version = blob.ReadUInt32();
                    sb.AppendLine($"{indent}[activatable({version})]");
                }
            }
        }
        catch { /* best-effort */ }
    }

    private static void EmitStaticAttribute(MetadataReader reader, CustomAttribute attr, StringBuilder sb, string indent)
    {
        try
        {
            var blob = reader.GetBlobReader(attr.Value);
            if (blob.Length < 2) return;
            blob.ReadUInt16(); // prolog
            var staticType = ReadSerializedString(ref blob);
            if (staticType != null && blob.RemainingBytes >= 4)
            {
                var version = blob.ReadUInt32();
                sb.AppendLine($"{indent}[static({GetShortName(staticType)}, {version})]");
            }
        }
        catch { /* best-effort */ }
    }

    private static void EmitDeprecatedAttribute(MetadataReader reader, CustomAttribute attr, StringBuilder sb, string indent)
    {
        try
        {
            var blob = reader.GetBlobReader(attr.Value);
            if (blob.Length < 2) return;
            blob.ReadUInt16(); // prolog
            var message = ReadSerializedString(ref blob);
            if (message != null && blob.RemainingBytes >= 8)
            {
                var deprecationType = blob.ReadInt32();
                var version = blob.ReadUInt32();
                var depTypeStr = deprecationType == 1 ? "remove" : "deprecate";
                sb.AppendLine($"{indent}[deprecated(\"{message}\", {depTypeStr}, {version})]");
            }
        }
        catch { /* best-effort */ }
    }

    private static void EmitExclusiveToAttribute(MetadataReader reader, CustomAttribute attr, StringBuilder sb, string indent)
    {
        try
        {
            var blob = reader.GetBlobReader(attr.Value);
            if (blob.Length < 2) return;
            blob.ReadUInt16(); // prolog
            var typeName = ReadSerializedString(ref blob);
            if (typeName != null)
                sb.AppendLine($"{indent}[exclusiveto({GetShortName(typeName)})]");
        }
        catch { /* best-effort */ }
    }

    private static void EmitVersionAttribute(MetadataReader reader, CustomAttribute attr, StringBuilder sb, string indent)
    {
        var blob = reader.GetBlobReader(attr.Value);
        if (blob.Length < 6) return;
        blob.ReadUInt16(); // prolog
        var version = blob.ReadUInt32();
        if (version != 0x00010000) // skip default version to keep output clean
        {
            var display = Midl.Analysis.VersionEncoding.Display(version);
            sb.AppendLine($"{indent}[version({display})]");
        }
    }

    private void EmitContractVersionAttribute(MetadataReader reader, CustomAttribute attr, StringBuilder sb, string indent)
    {
        try
        {
            int paramCount = GetCtorParameterCount(reader, attr.Constructor);
            var blob = reader.GetBlobReader(attr.Value);
            if (blob.Length < 2) return;
            blob.ReadUInt16(); // prolog

            if (paramCount >= 2)
            {
                // Named variant: (String contractName, UInt32 version)
                var contractName = ReadSerializedString(ref blob);
                if (contractName != null && blob.RemainingBytes >= 4)
                {
                    var version = blob.ReadUInt32();
                    var display = Midl.Analysis.VersionEncoding.Display(version);
                    sb.AppendLine($"{indent}[contract({contractName}, {display})]");
                }
            }
            else
            {
                // Version-only variant: (UInt32 version)
                if (blob.RemainingBytes >= 4)
                {
                    var version = blob.ReadUInt32();
                    var display = Midl.Analysis.VersionEncoding.Display(version);
                    sb.AppendLine($"{indent}[contractversion({display})]");
                }
            }
        }
        catch { /* best-effort */ }
    }

    /// <summary>
    /// Gets the parameter count from a constructor's MemberRef signature.
    /// </summary>
    private static int GetCtorParameterCount(MetadataReader reader, EntityHandle ctorHandle)
    {
        if (ctorHandle.Kind == HandleKind.MemberReference)
        {
            var memberRef = reader.GetMemberReference((MemberReferenceHandle)ctorHandle);
            var sigBlob = reader.GetBlobReader(memberRef.Signature);
            sigBlob.ReadSignatureHeader(); // calling convention
            return sigBlob.ReadCompressedInteger(); // parameter count
        }
        return -1;
    }

    /// <summary>
    /// Detects whether a type definition represents an apicontract.
    /// ApiContracts are sealed types with ContractVersionAttribute (uint-only variant) and no methods.
    /// </summary>
    private static bool IsApiContract(MetadataReader reader, TypeDefinition td, TypeDefinitionHandle handle)
    {
        if (!td.Attributes.HasFlag(TypeAttributes.Sealed)) return false;

        // Must have no methods
        bool hasMethods = false;
        foreach (var _ in td.GetMethods()) { hasMethods = true; break; }
        if (hasMethods) return false;

        // Must have ContractVersionAttribute with uint-only constructor (1 param)
        foreach (var attrHandle in td.GetCustomAttributes())
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            var attrName = GetAttributeTypeName(reader, attr.Constructor);
            if (attrName == "ContractVersionAttribute")
            {
                int paramCount = GetCtorParameterCount(reader, attr.Constructor);
                if (paramCount == 1) return true;
            }
        }
        return false;
    }

    #endregion

    #region ApiContract

    private void DecompileApiContract(MetadataReader reader, TypeDefinition td, TypeDefinitionHandle handle,
        string name, StringBuilder sb, string indent)
    {
        EmitAttributes(reader, handle, sb, indent);
        sb.AppendLine($"{indent}apicontract {name}");
        sb.AppendLine($"{indent}{{");
        sb.AppendLine($"{indent}}};");
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
            // Strip generic arity suffix (e.g., IAsyncOperation`1 → IAsyncOperation)
            int backtick = name.IndexOf('`');
            if (backtick >= 0) name = name[..backtick];
            return string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";
        }

        public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
        {
            var tr = reader.GetTypeReference(handle);
            var ns = reader.GetString(tr.Namespace);
            var name = reader.GetString(tr.Name);
            int backtick = name.IndexOf('`');
            if (backtick >= 0) name = name[..backtick];
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
        public string GetGenericTypeParameter(object? genericContext, int index)
        {
            if (genericContext is string[] names && index < names.Length)
                return names[index];
            return index switch { 0 => "T", 1 => "U", 2 => "V", 3 => "W", _ => $"T{index}" };
        }
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
