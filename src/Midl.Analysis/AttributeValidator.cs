// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis;

/// <summary>
/// Validates attribute arguments are semantically correct.
/// Runs after binding and interface synthesis.
/// </summary>
public partial class AttributeValidator
{
    private readonly List<Diagnostic> _diagnostics;
    private readonly Compilation _compilation;
    private HashSet<string> _sdkResolvedAttrNames = new(StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> ValidThreadingModels =
        new(StringComparer.OrdinalIgnoreCase) { "both", "sta", "mta" };

    private static readonly HashSet<string> ValidMarshalingBehaviors =
        new(StringComparer.OrdinalIgnoreCase) { "agile", "standard", "none" };

    private static readonly HashSet<string> ValidComposableAccess =
        new(StringComparer.OrdinalIgnoreCase) { "public", "protected" };

    private static readonly HashSet<string> ValidDeprecationTypes =
        new(StringComparer.OrdinalIgnoreCase) { "deprecate", "remove" };

    /// <summary>
    /// Centralized map of well-known WinRT attribute names to their valid target kinds.
    /// Attributes not listed here have no target restriction (valid everywhere).
    /// Used by both validation and autocomplete.
    /// </summary>
    public static readonly Dictionary<string, HashSet<string>> BuiltInAttributeTargets =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["contractversion"] = new() { "target_apicontract" },
            ["version"] = new() { "target_runtimeclass", "target_interface", "target_delegate", "target_enum", "target_struct", "target_field" },
            ["contract"] = new() { "target_runtimeclass", "target_interface", "target_delegate", "target_enum", "target_struct", "target_field", "target_interfaceimpl", "target_method", "target_event", "target_parameter", "target_property" },
            ["uuid"] = new() { "target_interface", "target_delegate" },
            ["threading"] = new() { "target_runtimeclass" },
            ["marshaling_behavior"] = new() { "target_runtimeclass" },
            ["activatable"] = new() { "target_runtimeclass" },
            ["composable"] = new() { "target_runtimeclass" },
            ["static"] = new() { "target_runtimeclass" },
            ["deprecated"] = new() { "target_runtimeclass", "target_interface", "target_delegate", "target_enum", "target_struct", "target_method", "target_property", "target_event", "target_field" },
            ["exclusiveto"] = new() { "target_interface" },
            ["default_interface"] = new() { "target_runtimeclass" },
            ["webhosthidden"] = new() { "target_runtimeclass", "target_interface", "target_delegate", "target_enum", "target_struct" },
            ["experimental"] = new() { "target_runtimeclass", "target_interface", "target_delegate", "target_enum", "target_struct" },
            ["flags"] = new() { "target_enum" },
            ["overload"] = new() { "target_method" },
            ["default_overload"] = new() { "target_method" },
            ["method_name"] = new() { "target_method" },
            ["return_name"] = new() { "target_method" },
            ["remote_async"] = new() { "target_method" },
            ["noexcept"] = new() { "target_method" },
            ["attributeusage"] = new() { "target_attribute" },
            ["allowmultiple"] = new() { "target_attribute" },
            ["attributename"] = new() { "target_attribute" },
            ["contentproperty"] = new() { "target_runtimeclass" },
            ["allowforweb"] = new() { "target_runtimeclass" },
            ["constructor_name"] = new() { "target_runtimeclass" },
            ["interface_name"] = new() { "target_runtimeclass" },
            ["static_name"] = new() { "target_runtimeclass" },
        };

    /// <summary>All well-known built-in attribute names recognized by the MIDL compiler.</summary>
    // Attributes recognized by the compiler. This includes classic MIDL compat
    // attributes and WinRT attributes that the compiler/binder explicitly handles.
    // Additional SDK attributes (e.g. noexcept, contentproperty) are resolved
    // dynamically from WinMD providers at validation time.
    private static readonly HashSet<string> BuiltinAttributes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // WinRT attributes the compiler/binder explicitly processes
            "uuid", "contract", "contractversion", "version",
            "threading", "marshaling_behavior",
            "activatable", "composable", "static",
            "deprecated", "experimental",
            "exclusiveto", "webhosthidden",
            "default", "overridable", "protected",
            "default_interface", "flags",
            // Attribute meta-attributes
            "attributeusage", "attributename", "allowmultiple",
            // Member-level attributes the binder processes
            "method_name", "overload", "default_overload",
            "return_name", "remote_async", "noexcept",
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
            "annotation", "async", "eventadd", "eventremove",
            "switch_type", "switch_is", "case", "default_case",
            "ignore", "transmit_as", "represent_as", "wire_marshal",
            "first_is", "last_is", "byte_count", "ms_union",
            "handle", "error_status_t", "heap", "idempotent",
            "broadcast", "maybe", "message", "notify", "reflect",
            "disable_consistency_check", "implicit_handle", "explicit_handle",
            "cpp_quote", "midl_pragma",
#if FEATURE_STAGING
            "feature", "feature_name",
#endif
            "internal",
            // XAML/composition attributes passed through to WinMD emission
            "contentproperty", "inputproperty",
            "allowforweb",
            "bindable", "create_instance",
            // Interface naming directives
            "constructor_name", "interface_name", "static_name",
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
                // Also index by short name (strip "Attribute" suffix) — standard convention
                if (at.Name.EndsWith("Attribute", StringComparison.Ordinal) && at.Name.Length > 9)
                    attrTypes[at.Name[..^9]] = at;
                if (at.AttributeNameOverride != null)
                    attrTypes[at.AttributeNameOverride] = at;
            }
            if (type is ApiContractSymbol)
            {
                apiContracts.Add(type.FullName);
                apiContracts.Add(type.Name);
            }
        }

        // Preload SDK attribute types from lazy WinMD providers
        // so we can validate attributes like [noexcept] (NoExceptionAttribute).
        // Also preload apicontract types from referenced WinMDs so
        // [contract(Windows.Foundation.UniversalApiContract, ...)] validates.
        foreach (var fullName in _compilation.GetAllLazyTypeNames())
        {
            if (fullName.EndsWith("Attribute", StringComparison.Ordinal) &&
                fullName.StartsWith("Windows.Foundation.Metadata.", StringComparison.Ordinal))
            {
                if (attrTypes.Values.Any(a => a.FullName == fullName))
                    continue; // already loaded

                var symbol = _compilation.GetTypeByName(fullName);
                if (symbol is AttributeTypeSymbol sdkAttr)
                {
                    attrTypes[sdkAttr.Name] = sdkAttr;
                    if (sdkAttr.Name.EndsWith("Attribute", StringComparison.Ordinal) && sdkAttr.Name.Length > 9)
                        attrTypes[sdkAttr.Name[..^9]] = sdkAttr;
                    if (sdkAttr.AttributeNameOverride != null)
                        attrTypes[sdkAttr.AttributeNameOverride] = sdkAttr;
                }
            }
            else if (fullName.EndsWith("ApiContract", StringComparison.Ordinal) ||
                     fullName.EndsWith("Contract", StringComparison.Ordinal))
            {
                // Resolve candidate — will be an ApiContractSymbol if it really is one.
                var symbol = _compilation.GetTypeByName(fullName);
                if (symbol is ApiContractSymbol)
                {
                    apiContracts.Add(symbol.FullName);
                    apiContracts.Add(symbol.Name);
                }
            }
        }

        // Track which attr names are SDK-resolved (to avoid duplicate target errors)
        _sdkResolvedAttrNames = new HashSet<string>(attrTypes.Keys, StringComparer.OrdinalIgnoreCase);

        foreach (var type in _compilation.GetAllTypes())
        {
            // Types reached through an import report at offsets in their own file.
            int diagStart = _diagnostics.Count;
            switch (type)
            {
                case RuntimeClassSymbol rc:
                    ValidateRuntimeClass(rc);
                    ValidateUnknownAttributes(rc.Attributes, attrTypes);
                    ValidateBuiltInAttributeTargets(rc.Attributes, "target_runtimeclass");
                    ValidateCustomAttributeInstances(rc.Attributes, rc.Name, "target_runtimeclass", attrTypes);
                    ValidateContractAndFeatureArgs(rc.Attributes, rc.Name, apiContracts);
                    ValidateMemberAttributes(rc.Members, attrTypes, apiContracts);
                    break;
                case InterfaceSymbol iface:
                    ValidateUnknownAttributes(iface.Attributes, attrTypes);
                    ValidateBuiltInAttributeTargets(iface.Attributes, "target_interface");
                    ValidateCustomAttributeInstances(iface.Attributes, iface.Name, "target_interface", attrTypes);
                    ValidateContractAndFeatureArgs(iface.Attributes, iface.Name, apiContracts);
                    ValidateDeprecatedOnAttributes(iface.Attributes);
                    if (!iface.IsSynthesized)
                        ValidateMemberAttributes(iface.Members, attrTypes, apiContracts);
                    break;
                case EnumSymbol en:
                    ValidateUnknownAttributes(en.Attributes, attrTypes);
                    ValidateBuiltInAttributeTargets(en.Attributes, "target_enum");
                    ValidateCustomAttributeInstances(en.Attributes, en.Name, "target_enum", attrTypes);
                    ValidateContractAndFeatureArgs(en.Attributes, en.Name, apiContracts);
                    ValidateDeprecatedOnAttributes(en.Attributes);
                    foreach (var m in en.EnumMembers)
                    {
                        ValidateBuiltInAttributeTargets(m.Attributes, "target_field");
                        ValidateCustomAttributeInstances(m.Attributes, m.Name, "target_field", attrTypes);
                        ValidateContractAndFeatureArgs(m.Attributes, m.Name, apiContracts);
                        ValidateDeprecatedOnAttributes(m.Attributes);
                    }
                    break;
                case StructSymbol st:
                    ValidateUnknownAttributes(st.Attributes, attrTypes);
                    ValidateBuiltInAttributeTargets(st.Attributes, "target_struct");
                    ValidateCustomAttributeInstances(st.Attributes, st.Name, "target_struct", attrTypes);
                    ValidateContractAndFeatureArgs(st.Attributes, st.Name, apiContracts);
                    ValidateDeprecatedOnAttributes(st.Attributes);
                    break;
                case DelegateSymbol del:
                    ValidateUnknownAttributes(del.Attributes, attrTypes);
                    ValidateBuiltInAttributeTargets(del.Attributes, "target_delegate");
                    ValidateCustomAttributeInstances(del.Attributes, del.Name, "target_delegate", attrTypes);
                    ValidateContractAndFeatureArgs(del.Attributes, del.Name, apiContracts);
                    ValidateDeprecatedOnAttributes(del.Attributes);
                    break;
                case ApiContractSymbol ac:
                    ValidateUnknownAttributes(ac.Attributes, attrTypes);
                    ValidateBuiltInAttributeTargets(ac.Attributes, "target_apicontract");
                    break;
                case AttributeTypeSymbol:
                    break;
            }
            DiagnosticSourceTagger.Tag(_diagnostics, diagStart, type.SourceFile);
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

    /// <summary>
    /// Validates that well-known built-in attributes are used on appropriate targets.
    /// </summary>
    private void ValidateBuiltInAttributeTargets(List<AttributeData> attributes, string targetKind)
    {
        foreach (var attr in attributes)
        {
            if (BuiltInAttributeTargets.TryGetValue(attr.Name, out var validTargets) &&
                !validTargets.Contains(targetKind))
            {
                // Only report if the SDK path won't also catch it (avoid duplicates)
                // SDK-resolved attrs are validated by ValidateCustomAttributeInstances
                if (!_sdkResolvedAttrNames.Contains(attr.Name))
                {
                    _diagnostics.Add(DiagnosticDescriptors.AttributeWrongTarget(
                        attr.Name, targetKind, string.Join(", ", validTargets), attr.Position));
                }
            }
        }
    }

    private void ValidateThreadingModel(AttributeData attr)
    {
        if (attr.Arguments.Count != 1)
        {
            _diagnostics.Add(DiagnosticDescriptors.AttributeArgCountMismatch(
                "threading", 1, attr.Arguments.Count, attr.Position));
            return;
        }
        var value = attr.GetIdentifier(0);
        if (value != null && !ValidThreadingModels.Contains(value))
        {
            var argPos = attr.Arguments[0].Position;
            var argLen = attr.Arguments[0].Length;
            _diagnostics.Add(DiagnosticDescriptors.InvalidThreadingModel(value,
                argPos > 0 ? argPos : attr.Position, argLen > 0 ? argLen : value.Length));
        }
    }

    private void ValidateMarshalingBehavior(AttributeData attr)
    {
        if (attr.Arguments.Count != 1)
        {
            _diagnostics.Add(DiagnosticDescriptors.AttributeArgCountMismatch(
                "marshaling_behavior", 1, attr.Arguments.Count, attr.Position));
            return;
        }
        var value = attr.GetIdentifier(0);
        if (value != null && !ValidMarshalingBehaviors.Contains(value))
        {
            var arg = attr.Arguments[0];
            _diagnostics.Add(DiagnosticDescriptors.InvalidMarshalingBehavior(value,
                arg.Position > 0 ? arg.Position : attr.Position));
        }
    }

    private void ValidateComposableAccess(AttributeData attr)
    {
        // composable(type, access, version) — access is arg index 1
        var access = attr.GetIdentifier(1);
        if (access != null && !ValidComposableAccess.Contains(access))
        {
            var arg = attr.Arguments[1];
            _diagnostics.Add(DiagnosticDescriptors.InvalidComposableAccess(access,
                arg.Position > 0 ? arg.Position : attr.Position));
        }
    }

    private void ValidateDeprecationType(AttributeData attr)
    {
        // deprecated("msg", type, contract, version) — type is arg index 1
        var depType = attr.GetIdentifier(1);
        if (depType != null && !ValidDeprecationTypes.Contains(depType))
        {
            var arg = attr.Arguments[1];
            _diagnostics.Add(DiagnosticDescriptors.InvalidDeprecationType(depType,
                arg.Position > 0 ? arg.Position : attr.Position));
        }
    }

    /// <summary>
    /// Validates [deprecated] attribute args on any attribute list (types, members, fields).
    /// </summary>
    private void ValidateDeprecatedOnAttributes(List<AttributeData> attributes)
    {
        foreach (var attr in attributes)
        {
            if (attr.Name.Equals("deprecated", StringComparison.OrdinalIgnoreCase))
                ValidateDeprecationType(attr);
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
            !string.Equals(factorySymbol.ExclusiveToClass, rc.FullName, StringComparison.Ordinal))
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
            !string.Equals(ifaceSymbol.ExclusiveToClass, rc.FullName, StringComparison.Ordinal))
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
            if (BuiltinAttributes.Contains(attr.Name))
                continue;
            if (attrTypes.ContainsKey(attr.Name))
                continue;

            // Try resolving from SDK/WinMD lazy providers (e.g. NoExceptionAttribute → "noexcept")
            if (TryResolveSdkAttribute(attr.Name, attrTypes))
                continue;

            _diagnostics.Add(DiagnosticDescriptors.UnknownAttribute(attr.Name, attr.Position));
        }
    }

    /// <summary>
    /// Attempts to find an attribute type in the SDK lazy providers by trying
    /// common naming patterns: "name", "NameAttribute", "Windows.Foundation.Metadata.NameAttribute".
    /// </summary>
    private bool TryResolveSdkAttribute(string name, Dictionary<string, AttributeTypeSymbol> attrTypes)
    {
        var candidates = new[]
        {
            name,
            name + "Attribute",
            char.ToUpper(name[0]) + name[1..] + "Attribute",
            "Windows.Foundation.Metadata." + name + "Attribute",
            "Windows.Foundation.Metadata." + char.ToUpper(name[0]) + name[1..] + "Attribute",
        };

        foreach (var candidate in candidates)
        {
            var symbol = _compilation.GetTypeByName(candidate);
            if (symbol is AttributeTypeSymbol at)
            {
                // Cache it for future lookups
                attrTypes[name] = at;
                if (at.AttributeNameOverride != null)
                    attrTypes[at.AttributeNameOverride] = at;
                return true;
            }
        }

        return false;
    }

    /// <summary>Validate custom attributes on members (methods, properties, events, constructors).</summary>
    private void ValidateMemberAttributes(
        IReadOnlyList<ISymbol> members,
        Dictionary<string, AttributeTypeSymbol> attrTypes,
        HashSet<string> apiContracts)
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
            ValidateBuiltInAttributeTargets(attrs, targetKind);
            ValidateCustomAttributeInstances(attrs, name!, targetKind, attrTypes);
            ValidateContractAndFeatureArgs(attrs, name!, apiContracts);
            ValidateDeprecatedOnAttributes(attrs);
        }
    }

    private void ValidateContractAndFeatureArgs(
        List<AttributeData> attributes, string ownerName,
        HashSet<string> apiContracts)
    {
        foreach (var attr in attributes)
        {
            var lowerName = attr.Name.ToLowerInvariant();

            if (lowerName == "contract")
            {
                // [contract] requires exactly 2 args: (ContractName, version)
                if (attr.Arguments.Count != 2)
                {
                    _diagnostics.Add(DiagnosticDescriptors.AttributeArgCountMismatch(
                        "contract", 2, attr.Arguments.Count, attr.Position));
                    continue;
                }
                var contractRef = attr.GetTypeName(0) ?? attr.GetIdentifier(0);
                if (contractRef != null && !apiContracts.Contains(contractRef))
                {
                    _diagnostics.Add(DiagnosticDescriptors.ContractArgNotApiContract(
                        ownerName, contractRef, attr.Position));
                }
            }
#if FEATURE_STAGING
            else if (lowerName == "feature")
            {
                var featureRef = attr.GetTypeName(0) ?? attr.GetIdentifier(0);
                if (featureRef != null && !_compilation.FeatureNames.ContainsKey(featureRef))
                {
                    _diagnostics.Add(DiagnosticDescriptors.FeatureArgNotFeatureName(
                        ownerName, featureRef, attr.Position));
                }
            }
#endif
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

            bool hasHardcodedValidation = BuiltInAttributeTargets.ContainsKey(attr.Name);

            // Validate target — skip if already checked by ValidateBuiltInAttributeTargets
            // to avoid duplicate MIDL1029 errors
            if (!hasHardcodedValidation &&
                attrType.ValidTargets.Count > 0 &&
                !attrType.ValidTargets.Any(t => t.Equals(targetKind, StringComparison.OrdinalIgnoreCase)))
            {
                _diagnostics.Add(DiagnosticDescriptors.AttributeWrongTarget(
                    attr.Name, targetKind, string.Join(", ", attrType.ValidTargets), attr.Position));
            }

            // Skip arg count/type validation for attrs with hardcoded validators
            // (they do their own arg validation — avoids duplicate MIDL1030 errors)
            if (hasHardcodedValidation)
                continue;

            // Validate argument count
            if (attrType.Fields.Count > 0 && attr.Arguments.Count != attrType.Fields.Count)
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
                    var argPos = arg.Position > 0 ? arg.Position : attr.Position;
                    if (!IsArgTypeCompatible(arg.Kind, field.Type.Name))
                    {
                        _diagnostics.Add(DiagnosticDescriptors.AttributeArgumentTypeMismatch(
                            attr.Name, i, field.Name, field.Type.DisplayName,
                            ArgKindDisplayName(arg.Kind), argPos));
                    }
                    else if (arg.Kind == ArgKind.Integer)
                    {
                        // Validate numeric range for integer fields
                        var rangeError = ValidateNumericRange(arg.IntValue, field.Type.Name);
                        if (rangeError != null)
                        {
                            _diagnostics.Add(DiagnosticDescriptors.AttributeArgumentOutOfRange(
                                attr.Name, i, field.Name, field.Type.DisplayName,
                                arg.IntValue, rangeError, argPos));
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

