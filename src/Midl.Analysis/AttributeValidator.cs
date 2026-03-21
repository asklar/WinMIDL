using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis;

/// <summary>
/// Validates attribute arguments are semantically correct.
/// Runs after binding and interface synthesis.
/// </summary>
public class AttributeValidator
{
    private readonly List<Diagnostic> _diagnostics;
    private readonly Compilation _compilation;

    private static readonly HashSet<string> ValidThreadingModels =
        new(StringComparer.OrdinalIgnoreCase) { "both", "sta", "mta" };

    private static readonly HashSet<string> ValidMarshalingBehaviors =
        new(StringComparer.OrdinalIgnoreCase) { "agile", "standard", "none" };

    private static readonly HashSet<string> ValidComposableAccess =
        new(StringComparer.OrdinalIgnoreCase) { "public", "protected" };

    private static readonly HashSet<string> ValidDeprecationTypes =
        new(StringComparer.OrdinalIgnoreCase) { "deprecate", "remove" };

    /// <summary>All well-known built-in attribute names recognized by the MIDL compiler.</summary>
    private static readonly HashSet<string> WellKnownAttributes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Type-level attributes
            "uuid", "contract", "contractversion", "version",
            "threading", "marshaling_behavior",
            "activatable", "composable", "static",
            "deprecated", "experimental",
            "exclusiveto", "webhosthidden",
            "default", "overridable", "protected",
            "default_interface", "flags",
            // Attribute meta-attributes
            "attributeusage", "attributename", "allowmultiple",
            // Member-level attributes
            "method_name", "overload", "default_overload",
            "return_name", "remote_async",
            // Classic MIDL attributes (compat)
            "object", "local", "pointer_default", "helpstring",
            "dual", "oleautomation", "nonextensible", "hidden",
            "restricted", "source", "propget", "propput", "propputref",
            "id", "in", "out", "retval", "optional", "defaultvalue",
            "size_is", "length_is", "max_is", "iid_is", "range",
            "call_as", "endpoint", "string", "unique", "ptr", "ref",
            "context_handle", "v1_enum", "async_uuid",
            "noncreatable", "aggregatable", "appobject",
            "entry", "dllname", "custom",
            // XAML/composition
            "contentproperty", "inputproperty",
            "bindable", "create_instance",
            "feature", "feature_name",
            "internal",
        };

    #pragma warning disable CS0649 // Field is never assigned
    private bool _currentClassIsStatic;
    #pragma warning restore CS0649

    public AttributeValidator(Compilation compilation, List<Diagnostic> diagnostics)
    {
        _compilation = compilation;
        _diagnostics = diagnostics;
    }

    public void Validate()
    {
        // Build lookup of user-defined attribute types
        var attrTypes = new Dictionary<string, AttributeTypeSymbol>(StringComparer.OrdinalIgnoreCase);
        // Build lookup of declared apicontract names (both full and simple names)
        var apiContracts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var type in _compilation.GetAllTypes())
        {
            if (type is AttributeTypeSymbol at)
            {
                attrTypes[at.Name] = at;
                if (at.AttributeNameOverride != null)
                    attrTypes[at.AttributeNameOverride] = at;
            }
            if (type is ApiContractSymbol)
            {
                apiContracts.Add(type.FullName);
                apiContracts.Add(type.Name);
            }
        }

        // Also use feature_name declarations from the compilation
        var featureNames = _compilation.FeatureNames;

        foreach (var type in _compilation.GetAllTypes())
        {
            switch (type)
            {
                case RuntimeClassSymbol rc:
                    ValidateRuntimeClass(rc);
                    ValidateUnknownAttributes(rc.Attributes, attrTypes);
                    ValidateCustomAttributeInstances(rc.Attributes, rc.Name, "target_runtimeclass", attrTypes);
                    ValidateContractAndFeatureArgs(rc.Attributes, rc.Name, apiContracts, featureNames);
                    ValidateMemberAttributes(rc.Members, attrTypes, apiContracts, featureNames);
                    break;
                case InterfaceSymbol iface:
                    ValidateAttributes(iface.Attributes, iface.Name, isRuntimeClass: false);
                    ValidateUnknownAttributes(iface.Attributes, attrTypes);
                    ValidateCustomAttributeInstances(iface.Attributes, iface.Name, "target_interface", attrTypes);
                    ValidateContractAndFeatureArgs(iface.Attributes, iface.Name, apiContracts, featureNames);
                    ValidateMemberAttributes(iface.Members, attrTypes, apiContracts, featureNames);
                    break;
                case EnumSymbol en:
                    ValidateAttributes(en.Attributes, en.Name, isRuntimeClass: false);
                    ValidateUnknownAttributes(en.Attributes, attrTypes);
                    ValidateCustomAttributeInstances(en.Attributes, en.Name, "target_enum", attrTypes);
                    ValidateContractAndFeatureArgs(en.Attributes, en.Name, apiContracts, featureNames);
                    foreach (var m in en.EnumMembers)
                    {
                        ValidateCustomAttributeInstances(m.Attributes, m.Name, "target_field", attrTypes);
                        ValidateContractAndFeatureArgs(m.Attributes, m.Name, apiContracts, featureNames);
                    }
                    break;
                case StructSymbol st:
                    ValidateAttributes(st.Attributes, st.Name, isRuntimeClass: false);
                    ValidateUnknownAttributes(st.Attributes, attrTypes);
                    ValidateCustomAttributeInstances(st.Attributes, st.Name, "target_struct", attrTypes);
                    ValidateContractAndFeatureArgs(st.Attributes, st.Name, apiContracts, featureNames);
                    break;
                case DelegateSymbol del:
                    ValidateAttributes(del.Attributes, del.Name, isRuntimeClass: false);
                    ValidateUnknownAttributes(del.Attributes, attrTypes);
                    ValidateCustomAttributeInstances(del.Attributes, del.Name, "target_delegate", attrTypes);
                    ValidateContractAndFeatureArgs(del.Attributes, del.Name, apiContracts, featureNames);
                    break;
                case AttributeTypeSymbol:
                    break;
            }
        }
    }

    private void ValidateRuntimeClass(RuntimeClassSymbol rc)
    {
        _currentClassIsStatic = rc.IsStatic;
        bool hasActivatable = false;
        bool hasComposable = false;

        foreach (var attr in rc.Attributes)
        {
            var name = attr.Name.ToLowerInvariant();
            if (name == "activatable") hasActivatable = true;
            if (name == "composable") hasComposable = true;
        }

        foreach (var attr in rc.Attributes)
        {
            switch (attr.Name.ToLowerInvariant())
            {
                case "threading":
                    ValidateThreadingModel(attr);
                    ValidateThreadingOnActivatableOrComposable(attr, hasActivatable, hasComposable);
                    break;

                case "marshaling_behavior":
                    ValidateMarshalingBehavior(attr);
                    ValidateMarshalingOnActivatableOrComposable(attr, hasActivatable, hasComposable);
                    break;

                case "composable":
                    ValidateComposableAccess(attr);
                    break;

                case "deprecated":
                    ValidateDeprecationType(attr);
                    break;

                case "activatable":
                    ValidateActivatableFactory(attr, rc);
                    break;

                case "static":
                    ValidateStaticInterface(attr, rc);
                    break;

                case "attributeusage":
                    _diagnostics.Add(DiagnosticDescriptors.AttributeUsageNotOnAttribute(rc.Name, attr.Position));
                    break;
            }
        }

        // Rule 6: Cannot mix composable and activatable
        if (hasComposable && hasActivatable)
        {
            var firstAttr = rc.Attributes.First(a =>
                a.Name.Equals("composable", StringComparison.OrdinalIgnoreCase) ||
                a.Name.Equals("activatable", StringComparison.OrdinalIgnoreCase));
            _diagnostics.Add(DiagnosticDescriptors.ComposableAndActivatable(rc.Name, firstAttr.Position));
        }
    }

    private void ValidateAttributes(List<AttributeData> attributes, string typeName, bool isRuntimeClass)
    {
        foreach (var attr in attributes)
        {
            switch (attr.Name.ToLowerInvariant())
            {
                case "attributeusage":
                    _diagnostics.Add(DiagnosticDescriptors.AttributeUsageNotOnAttribute(typeName, attr.Position));
                    break;
            }
        }
    }

    private void ValidateThreadingModel(AttributeData attr)
    {
        var value = attr.GetIdentifier(0);
        if (value != null && !ValidThreadingModels.Contains(value))
        {
            _diagnostics.Add(DiagnosticDescriptors.InvalidThreadingModel(value, attr.Position));
        }
    }

    private void ValidateMarshalingBehavior(AttributeData attr)
    {
        var value = attr.GetIdentifier(0);
        if (value != null && !ValidMarshalingBehaviors.Contains(value))
        {
            _diagnostics.Add(DiagnosticDescriptors.InvalidMarshalingBehavior(value, attr.Position));
        }
    }

    private void ValidateComposableAccess(AttributeData attr)
    {
        // composable(type, access, version) — access is arg index 1
        var access = attr.GetIdentifier(1);
        if (access != null && !ValidComposableAccess.Contains(access))
        {
            _diagnostics.Add(DiagnosticDescriptors.InvalidComposableAccess(access, attr.Position));
        }
    }

    private void ValidateDeprecationType(AttributeData attr)
    {
        // deprecated("msg", type, contract, version) — type is arg index 1
        var depType = attr.GetIdentifier(1);
        if (depType != null && !ValidDeprecationTypes.Contains(depType))
        {
            _diagnostics.Add(DiagnosticDescriptors.InvalidDeprecationType(depType, attr.Position));
        }
    }

    private void ValidateActivatableFactory(AttributeData attr, RuntimeClassSymbol rc)
    {
        // activatable(IFactory, ...) — factory is a type name at arg 0
        var factoryName = attr.GetTypeName(0) ?? attr.GetIdentifier(0);
        if (factoryName == null) return;

        // Check if factoryName is an interface that is exclusiveto this class
        var factorySymbol = FindInterface(factoryName, rc);
        if (factorySymbol == null) return;

        if (!factorySymbol.IsExclusive ||
            !string.Equals(factorySymbol.ExclusiveToClass, rc.Name, StringComparison.Ordinal))
        {
            _diagnostics.Add(DiagnosticDescriptors.ActivatableFactoryNotExclusiveTo(
                factoryName, rc.Name, attr.Position));
        }
    }

    private void ValidateStaticInterface(AttributeData attr, RuntimeClassSymbol rc)
    {
        // static(IStatics, ...) — interface is a type name at arg 0
        var ifaceName = attr.GetTypeName(0) ?? attr.GetIdentifier(0);
        if (ifaceName == null) return;

        var ifaceSymbol = FindInterface(ifaceName, rc);
        if (ifaceSymbol == null) return;

        if (!ifaceSymbol.IsExclusive ||
            !string.Equals(ifaceSymbol.ExclusiveToClass, rc.Name, StringComparison.Ordinal))
        {
            _diagnostics.Add(DiagnosticDescriptors.StaticInterfaceNotExclusiveTo(
                ifaceName, rc.Name, attr.Position));
        }
    }

    private void ValidateThreadingOnActivatableOrComposable(
        AttributeData attr, bool hasActivatable, bool hasComposable)
    {
        // Only warn if the class is explicitly static (no activation possible)
        // Non-static classes may be implicitly activatable
        if (!hasActivatable && !hasComposable && _currentClassIsStatic)
        {
            _diagnostics.Add(DiagnosticDescriptors.ThreadingOnNonActivatable("threading", attr.Position));
        }
    }

    private void ValidateMarshalingOnActivatableOrComposable(
        AttributeData attr, bool hasActivatable, bool hasComposable)
    {
        if (!hasActivatable && !hasComposable && _currentClassIsStatic)
        {
            _diagnostics.Add(DiagnosticDescriptors.ThreadingOnNonActivatable("marshaling_behavior", attr.Position));
        }
    }

    private InterfaceSymbol? FindInterface(string name, RuntimeClassSymbol rc)
    {
        // Look in the same namespace as the runtime class
        if (rc.ContainingSymbol is NamespaceSymbol ns)
        {
            foreach (var member in ns.Members)
            {
                if (member is InterfaceSymbol iface && iface.Name == name)
                    return iface;
            }
        }

        // Also check implemented (synthesized) interfaces
        foreach (var iface in rc.ImplementedInterfaces)
        {
            if (iface.Name == name)
                return iface;
        }

        return null;
    }

    /// <summary>
    /// Report a warning for attributes that are neither well-known built-in
    /// attributes nor user-defined custom attribute types.
    /// </summary>
    private void ValidateUnknownAttributes(
        List<AttributeData> attributes,
        Dictionary<string, AttributeTypeSymbol> attrTypes)
    {
        foreach (var attr in attributes)
        {
            if (!WellKnownAttributes.Contains(attr.Name) &&
                !attrTypes.ContainsKey(attr.Name))
            {
                _diagnostics.Add(DiagnosticDescriptors.UnknownAttribute(attr.Name, attr.Position));
            }
        }
    }

    /// <summary>Validate custom attributes on members (methods, properties, events, constructors).</summary>
    private void ValidateMemberAttributes(
        IReadOnlyList<ISymbol> members,
        Dictionary<string, AttributeTypeSymbol> attrTypes,
        HashSet<string> apiContracts,
        HashSet<string> featureNames)
    {
        foreach (var member in members)
        {
            var (attrs, targetKind, name) = member switch
            {
                MethodSymbol m => (m.Attributes, "target_method", m.Name),
                PropertySymbol p => (p.Attributes, "target_property", p.Name),
                EventSymbol e => (e.Attributes, "target_event", e.Name),
                ConstructorSymbol c => (c.Attributes, "target_method", c.Name),
                _ => (null, null, null),
            };

            if (attrs == null || targetKind == null) continue;

            ValidateUnknownAttributes(attrs, attrTypes);
            ValidateCustomAttributeInstances(attrs, name!, targetKind, attrTypes);
            ValidateContractAndFeatureArgs(attrs, name!, apiContracts, featureNames);
        }
    }

    private void ValidateContractAndFeatureArgs(
        List<AttributeData> attributes, string ownerName,
        HashSet<string> apiContracts, HashSet<string> featureNames)
    {
        foreach (var attr in attributes)
        {
            var lowerName = attr.Name.ToLowerInvariant();

            if (lowerName == "contract")
            {
                var contractRef = attr.GetTypeName(0) ?? attr.GetIdentifier(0);
                if (contractRef != null && !apiContracts.Contains(contractRef))
                {
                    _diagnostics.Add(DiagnosticDescriptors.ContractArgNotApiContract(
                        ownerName, contractRef, attr.Position));
                }
            }
            else if (lowerName == "feature")
            {
                var featureRef = attr.GetTypeName(0) ?? attr.GetIdentifier(0);
                if (featureRef != null && !featureNames.Contains(featureRef))
                {
                    _diagnostics.Add(DiagnosticDescriptors.FeatureArgNotFeatureName(
                        ownerName, featureRef, attr.Position));
                }
            }
        }
    }

    private void ValidateCustomAttributeInstances(
        List<AttributeData> attributes, string ownerName, string targetKind,
        Dictionary<string, AttributeTypeSymbol> attrTypes)
    {
        foreach (var attr in attributes)
        {
            if (!attrTypes.TryGetValue(attr.Name, out var attrType))
                continue;

            // Validate target
            if (attrType.ValidTargets.Count > 0 &&
                !attrType.ValidTargets.Any(t => t.Equals(targetKind, StringComparison.OrdinalIgnoreCase)))
            {
                _diagnostics.Add(DiagnosticDescriptors.AttributeWrongTarget(
                    attr.Name, targetKind, string.Join(", ", attrType.ValidTargets), attr.Position));
            }

            // Validate argument count
            if (attr.Arguments.Count != attrType.Fields.Count)
            {
                _diagnostics.Add(DiagnosticDescriptors.AttributeArgCountMismatch(
                    attr.Name, attrType.Fields.Count, attr.Arguments.Count, attr.Position));
            }
            else
            {
                // Validate argument types and ranges (only when count matches)
                for (int i = 0; i < attr.Arguments.Count; i++)
                {
                    var arg = attr.Arguments[i];
                    var field = attrType.Fields[i];
                    if (!IsArgTypeCompatible(arg.Kind, field.Type.Name))
                    {
                        _diagnostics.Add(DiagnosticDescriptors.AttributeArgumentTypeMismatch(
                            attr.Name, i, field.Name, field.Type.DisplayName,
                            ArgKindDisplayName(arg.Kind), attr.Position));
                    }
                    else if (arg.Kind == ArgKind.Integer)
                    {
                        // Validate numeric range for integer fields
                        var rangeError = ValidateNumericRange(arg.IntValue, field.Type.Name);
                        if (rangeError != null)
                        {
                            _diagnostics.Add(DiagnosticDescriptors.AttributeArgumentOutOfRange(
                                attr.Name, i, field.Name, field.Type.DisplayName,
                                arg.IntValue, rangeError, attr.Position));
                        }
                    }
                }
            }
        }
    }

    /// <summary>Check if an argument kind is compatible with the expected field type.</summary>
    private static bool IsArgTypeCompatible(ArgKind argKind, string fieldTypeName)
    {
        return fieldTypeName switch
        {
            "String" or "string" or "HSTRING" => argKind is ArgKind.String,
            "Int32" or "Int16" or "Int64" or "UInt8" or "UInt16" or "UInt32" or "UInt64"
                or "Char16" or "Char"
                or "int" or "long" or "short" or "byte" => argKind is ArgKind.Integer,
            "Single" or "Double" or "float" or "double" => argKind is ArgKind.Integer or ArgKind.Float,
            "Boolean" or "boolean" or "bool" => argKind is ArgKind.Identifier or ArgKind.Integer,
            "Guid" => argKind is ArgKind.Guid or ArgKind.String,
            // For enum types, type references, and unknown types: identifiers, type names,
            // integers (for enum underlying values), and expressions are all acceptable
            _ => argKind is ArgKind.Identifier or ArgKind.TypeName or ArgKind.Integer or ArgKind.Expression,
        };
    }

    private static string ArgKindDisplayName(ArgKind kind) => kind switch
    {
        ArgKind.String => "string literal",
        ArgKind.Integer => "numeric literal",
        ArgKind.Float => "float literal",
        ArgKind.Identifier => "identifier",
        ArgKind.TypeName => "type name",
        ArgKind.Guid => "GUID literal",
        ArgKind.Expression => "expression",
        _ => kind.ToString(),
    };

    /// <summary>
    /// Validate that an integer value fits within the range of the target type.
    /// Returns a human-readable range description on failure, or null if valid.
    /// </summary>
    private static string? ValidateNumericRange(long value, string typeName)
    {
        return typeName switch
        {
            "UInt8" or "byte" or "BYTE" when value < 0 || value > 255 => "0..255",
            "Int16" or "short" when value < short.MinValue || value > short.MaxValue => $"{short.MinValue}..{short.MaxValue}",
            "UInt16" or "WORD" when value < 0 || value > ushort.MaxValue => $"0..{ushort.MaxValue}",
            "Char16" or "Char" or "WCHAR" when value < 0 || value > ushort.MaxValue => $"0..{ushort.MaxValue}",
            "Int32" or "int" or "long" when value < int.MinValue || value > int.MaxValue => $"{int.MinValue}..{int.MaxValue}",
            "UInt32" or "DWORD" or "unsigned" when value < 0 || value > uint.MaxValue => $"0..{uint.MaxValue}",
            // Int64/UInt64: long already covers Int64 range; UInt64 can't be fully represented
            "UInt64" or "ULONGLONG" when value < 0 => "0..18446744073709551615",
            _ => null, // In range or type doesn't have a fixed range
        };
    }
}
