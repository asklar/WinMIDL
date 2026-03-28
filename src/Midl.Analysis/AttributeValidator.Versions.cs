using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis;

/// <summary>
/// Version argument validation for [contractversion], [version], and [contract] attributes.
/// </summary>
public partial class AttributeValidator
{
    /// <summary>
    /// Validates that version arguments on [contractversion], [version], and [contract]
    /// are non-negative integers (or valid x.y version literals).
    /// </summary>
    internal void ValidateVersionArguments()
    {
        foreach (var type in _compilation.GetAllTypes())
        {
            var attributes = type switch
            {
                ApiContractSymbol ac => ac.Attributes,
                Symbols.RuntimeClassSymbol rc => rc.Attributes,
                Symbols.InterfaceSymbol iface => iface.Attributes,
                Symbols.EnumSymbol en => en.Attributes,
                Symbols.StructSymbol st => st.Attributes,
                Symbols.DelegateSymbol del => del.Attributes,
                _ => null,
            };

            if (attributes is null) continue;

            ValidateVersionArgumentsOnAttributes(attributes);

            // Also validate member-level attributes
            if (type is Symbols.RuntimeClassSymbol rc2)
                ValidateVersionArgumentsOnMembers(rc2.Members);
            else if (type is Symbols.InterfaceSymbol iface2 && !iface2.IsSynthesized)
                ValidateVersionArgumentsOnMembers(iface2.Members);
            else if (type is Symbols.EnumSymbol en2)
            {
                foreach (var m in en2.EnumMembers)
                    ValidateVersionArgumentsOnAttributes(m.Attributes);
            }
        }
    }

    private void ValidateVersionArgumentsOnMembers(IReadOnlyList<Symbols.ISymbol> members)
    {
        foreach (var member in members)
        {
            var attrs = member switch
            {
                Symbols.MethodSymbol m => m.Attributes,
                Symbols.PropertySymbol p => p.Attributes,
                Symbols.EventSymbol e => e.Attributes,
                Symbols.ConstructorSymbol c => c.Attributes,
                _ => null,
            };

            if (attrs is not null)
                ValidateVersionArgumentsOnAttributes(attrs);
        }
    }

    private void ValidateVersionArgumentsOnAttributes(List<AttributeData> attributes)
    {
        foreach (var attr in attributes)
        {
            var lowerName = attr.Name.ToLowerInvariant();

            switch (lowerName)
            {
                case "contractversion":
                    ValidateVersionArg(attr, 0, "contractversion");
                    break;

                case "version":
                    ValidateVersionArg(attr, 0, "version");
                    break;

                case "contract":
                    // [contract(Name, N)] — version is arg index 1
                    ValidateVersionArg(attr, 1, "contract");
                    break;
            }
        }
    }

    /// <summary>
    /// Validates a single version argument on an attribute.
    /// </summary>
    private void ValidateVersionArg(AttributeData attr, int argIndex, string attrName)
    {
        if (argIndex >= attr.Arguments.Count)
            return;

        var arg = attr.Arguments[argIndex];

        switch (arg.Kind)
        {
            case ArgKind.Integer:
                if (arg.IntValue < 0)
                {
                    _diagnostics.Add(DiagnosticDescriptors.InvalidVersionValue(
                        attrName, arg.IntValue.ToString(), attr.Position));
                }
                else if (arg.IntValue > uint.MaxValue)
                {
                    _diagnostics.Add(DiagnosticDescriptors.InvalidVersionValue(
                        attrName, arg.IntValue.ToString(), attr.Position));
                }
                break;

            case ArgKind.Float:
                // x.y version literals (e.g., 1.0, 2.5) are valid — the binder encodes them as (major << 16) | minor.
                // Only reject negative float values.
                if (arg.FloatValue < 0)
                {
                    _diagnostics.Add(DiagnosticDescriptors.InvalidVersionValue(
                        attrName, arg.StringValue ?? arg.FloatValue.ToString(), attr.Position));
                }
                break;

            case ArgKind.String:
                // String values are never valid for version arguments
                _diagnostics.Add(DiagnosticDescriptors.InvalidVersionValue(
                    attrName, $"\"{arg.StringValue}\"", attr.Position));
                break;
        }
    }
}
