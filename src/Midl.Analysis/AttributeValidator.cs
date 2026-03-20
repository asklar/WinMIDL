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
        foreach (var type in _compilation.GetAllTypes())
        {
            switch (type)
            {
                case RuntimeClassSymbol rc:
                    ValidateRuntimeClass(rc);
                    break;
                case InterfaceSymbol iface:
                    ValidateAttributes(iface.Attributes, iface.Name, isRuntimeClass: false);
                    break;
                case EnumSymbol en:
                    ValidateAttributes(en.Attributes, en.Name, isRuntimeClass: false);
                    break;
                case StructSymbol st:
                    ValidateAttributes(st.Attributes, st.Name, isRuntimeClass: false);
                    break;
                case DelegateSymbol del:
                    ValidateAttributes(del.Attributes, del.Name, isRuntimeClass: false);
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
}
