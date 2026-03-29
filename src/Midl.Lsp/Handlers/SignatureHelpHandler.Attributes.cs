using Midl.Analysis;
using Midl.Analysis.Symbols;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Attribute-argument signature help — shows parameter hints when typing
/// inside <c>[attrName(</c> brackets.
/// </summary>
public static partial class SignatureHelpHandler
{
    /// <summary>
    /// Well-known attribute signatures.  Some attributes have multiple overload
    /// forms (e.g. <c>activatable</c>), so each entry maps to a list of signatures.
    /// </summary>
    private static readonly Dictionary<string, List<(string Label, string[] Params)>> AttributeSignatures =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["composable"] =
            [
                ("composable(IFactory, access, ContractName, version)",
                    new[] { "IFactory — factory interface type", "access — public or protected", "ContractName — API contract name", "version — contract version number" })
            ],
            ["activatable"] =
            [
                ("activatable(ContractName, version)",
                    new[] { "ContractName — API contract name", "version — contract version number" }),
                ("activatable(IFactory, ContractName, version)",
                    new[] { "IFactory — factory interface type", "ContractName — API contract name", "version — contract version number" })
            ],
            ["static"] =
            [
                ("static(IStatics, ContractName, version)",
                    new[] { "IStatics — static interface type", "ContractName — API contract name", "version — contract version number" })
            ],
            ["deprecated"] =
            [
                ("deprecated(\"message\", deprecate|remove, ContractName, version)",
                    new[] { "\"message\" — deprecation message", "deprecate|remove — deprecation type", "ContractName — API contract name", "version — contract version number" })
            ],
            ["contract"] =
            [
                ("contract(ContractName, version)",
                    new[] { "ContractName — API contract name", "version — contract version number" })
            ],
            ["contractversion"] =
            [
                ("contractversion(version)",
                    new[] { "version — contract version number" })
            ],
            ["version"] =
            [
                ("version(version)",
                    new[] { "version — version number" })
            ],
            ["threading"] =
            [
                ("threading(both|sta|mta)",
                    new[] { "model — both, sta, or mta" })
            ],
            ["marshaling_behavior"] =
            [
                ("marshaling_behavior(agile|standard|none)",
                    new[] { "behavior — agile, standard, or none" })
            ],
            ["exclusiveto"] =
            [
                ("exclusiveto(ClassName)",
                    new[] { "ClassName — target runtime class name" })
            ],
            ["attributeusage"] =
            [
                ("attributeusage(target...)",
                    new[] { "target — target_runtimeclass, target_interface, target_enum, etc." })
            ],
            ["attributename"] =
            [
                ("attributename(\"shortname\")",
                    new[] { "\"shortname\" — short name for the attribute" })
            ],
            ["overload"] =
            [
                ("overload(\"GroupName\")",
                    new[] { "\"GroupName\" — overload group name" })
            ],
            ["method_name"] =
            [
                ("method_name(\"Name\")",
                    new[] { "\"Name\" — projected method name" })
            ],
            ["return_name"] =
            [
                ("return_name(\"Name\")",
                    new[] { "\"Name\" — return value parameter name" })
            ],
            ["contentproperty"] =
            [
                ("contentproperty(\"PropertyName\")",
                    new[] { "\"PropertyName\" — content property name" })
            ],
            ["uuid"] =
            [
                ("uuid(\"00000000-0000-0000-0000-000000000000\")",
                    new[] { "\"GUID\" — interface GUID in registry format" })
            ],
            ["constructor_name"] =
            [
                ("constructor_name(\"Namespace.IFactory\", GUID)",
                    new[] { "\"Name\" — fully-qualified factory interface name", "GUID — interface identifier" })
            ],
            ["interface_name"] =
            [
                ("interface_name(\"Namespace.IFoo\", GUID)",
                    new[] { "\"Name\" — fully-qualified instance interface name", "GUID — interface identifier" })
            ],
            ["static_name"] =
            [
                ("static_name(\"Namespace.IFooStatics\", GUID)",
                    new[] { "\"Name\" — fully-qualified static interface name", "GUID — interface identifier" })
            ],
        };

    /// <summary>
    /// If the open-paren at <paramref name="openParenPos"/> is inside an attribute
    /// bracket (<c>[attrName(</c>), build the signature-help response.
    /// </summary>
    private static object? TryBuildAttributeSignature(string sourceText, int openParenPos, int commaCount, Compilation? cachedCompilation = null)
    {
        // Walk backward from the open paren to find the attribute name,
        // then verify there is an unmatched '[' before it.
        int nameEnd = openParenPos;

        // Skip whitespace between name and '('
        int pos = openParenPos - 1;
        while (pos >= 0 && char.IsWhiteSpace(sourceText[pos]))
            pos--;

        if (pos < 0) return null;

        // Collect the identifier
        int identEnd = pos + 1;
        while (pos >= 0 && (char.IsLetterOrDigit(sourceText[pos]) || sourceText[pos] == '_'))
            pos--;

        if (identEnd == pos + 1) return null; // no identifier found

        string attrName = sourceText[(pos + 1)..identEnd];

        // Now check that there is an unmatched '[' before the identifier
        if (!IsInsideAttributeBracket(sourceText, pos))
            return null;

        // Try resolving from compilation first (works for SDK + user-defined attrs)
        if (cachedCompilation != null)
        {
            var dynamicSig = TryBuildDynamicAttributeSignature(cachedCompilation, attrName);
            if (dynamicSig is not null)
                return BuildSignatureResponse(dynamicSig, commaCount);
        }

        // Fall back to well-known signatures (richer descriptions like "public or protected")
        if (AttributeSignatures.TryGetValue(attrName, out var sigs))
        {
            return BuildSignatureResponse(sigs, commaCount);
        }

        // Last resort: try compiling without refs for local attrs
        var userSig = TryBuildUserDefinedAttributeSignature(sourceText, attrName, cachedCompilation);
        if (userSig is not null)
        {
            return BuildSignatureResponse(userSig, commaCount);
        }

        return null;
    }

    /// <summary>
    /// Resolve attribute signature dynamically from a compilation (with SDK refs).
    /// Works for SDK attributes, user-defined attributes, and lazy WinMD providers.
    /// </summary>
    private static List<(string Label, string[] Params)>? TryBuildDynamicAttributeSignature(
        Compilation compilation, string attrName)
    {
        // Check eagerly-loaded types
        foreach (var type in compilation.GetAllTypes())
        {
            if (type is AttributeTypeSymbol attr && MatchesAttributeName(attr, attrName))
                return BuildSignatureFromFields(attr);
        }

        // Check lazy WinMD providers (SDK attrs like NoExceptionAttribute)
        foreach (var fullName in compilation.GetAllLazyTypeNames())
        {
            if (!fullName.EndsWith("Attribute", StringComparison.Ordinal)) continue;
            if (!fullName.StartsWith("Windows.Foundation.Metadata.", StringComparison.Ordinal)) continue;
            var symbol = compilation.GetTypeByName(fullName);
            if (symbol is AttributeTypeSymbol attr && MatchesAttributeName(attr, attrName))
                return BuildSignatureFromFields(attr);
        }

        return null;
    }

    private static bool MatchesAttributeName(AttributeTypeSymbol attr, string name)
    {
        if (name.Equals(attr.AttributeNameOverride, StringComparison.OrdinalIgnoreCase)) return true;
        if (name.Equals(attr.Name, StringComparison.OrdinalIgnoreCase)) return true;
        if (attr.Name.EndsWith("Attribute", StringComparison.Ordinal) && attr.Name.Length > 9 &&
            name.Equals(attr.Name[..^9], StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    private static List<(string Label, string[] Params)>? BuildSignatureFromFields(AttributeTypeSymbol attr)
    {
        if (attr.Fields.Count == 0) return null;

        var displayName = attr.AttributeNameOverride ?? attr.Name;
        var paramDescs = attr.Fields
            .Select(f => $"{f.Name} — {f.Type.DisplayName}")
            .ToArray();
        var label = $"{displayName}({string.Join(", ", attr.Fields.Select(f => f.Name))})";

        return [(label, paramDescs)];
    }

    /// <summary>
    /// Checks whether position <paramref name="pos"/> is inside an unmatched '[' bracket.
    /// </summary>
    private static bool IsInsideAttributeBracket(string sourceText, int pos)
    {
        int depth = 0;
        for (int i = pos; i >= 0; i--)
        {
            char c = sourceText[i];
            if (c == ']') depth++;
            else if (c == '[')
            {
                if (depth > 0) depth--;
                else return true; // unmatched '[' found
            }
        }
        return false;
    }

    /// <summary>
    /// Compile the source and try to resolve <paramref name="attrName"/> as an
    /// <see cref="AttributeTypeSymbol"/> to build a dynamic signature from its fields.
    /// </summary>
    private static List<(string Label, string[] Params)>? TryBuildUserDefinedAttributeSignature(
        string sourceText, string attrName, Compilation? cachedCompilation = null)
    {
        Compilation compilation;
        if (cachedCompilation != null)
        {
            compilation = cachedCompilation;
        }
        else
        {
            try
            {
                compilation = CompilationHelper.Compile(sourceText);
            }
            catch
            {
                return null;
            }
        }

        // Search all types for a matching AttributeTypeSymbol
        foreach (var type in compilation.GetAllTypes())
        {
            if (type is not AttributeTypeSymbol attr) continue;

            var displayName = attr.AttributeNameOverride ?? attr.Name;
            var strippedName = attr.Name.EndsWith("Attribute", StringComparison.Ordinal) && attr.Name.Length > 9
                ? attr.Name[..^9]
                : attr.Name;

            if (!attrName.Equals(displayName, StringComparison.OrdinalIgnoreCase) &&
                !attrName.Equals(strippedName, StringComparison.OrdinalIgnoreCase) &&
                !attrName.Equals(attr.Name, StringComparison.OrdinalIgnoreCase))
                continue;

            if (attr.Fields.Count == 0) return null;

            var paramDescs = attr.Fields
                .Select(f => $"{f.Name} — {f.Type.DisplayName}")
                .ToArray();

            var label = $"{displayName}({string.Join(", ", attr.Fields.Select(f => f.Name))})";

            return [(label, paramDescs)];
        }

        return null;
    }

    /// <summary>
    /// Build the LSP SignatureHelp response dictionary from a list of signature overloads.
    /// </summary>
    private static object BuildSignatureResponse(
        List<(string Label, string[] Params)> signatures, int commaCount)
    {
        var sigObjects = signatures.Select(sig => (object)new Dictionary<string, object?>
        {
            ["label"] = sig.Label,
            ["parameters"] = sig.Params.Select(p =>
                (object)new Dictionary<string, object?> { ["label"] = p }).ToArray()
        }).ToArray();

        // Pick the best matching signature: prefer one where commaCount < param count
        int activeSignature = 0;
        for (int i = 0; i < signatures.Count; i++)
        {
            if (commaCount < signatures[i].Params.Length)
            {
                activeSignature = i;
                break;
            }
        }

        return new Dictionary<string, object?>
        {
            ["signatures"] = sigObjects,
            ["activeSignature"] = activeSignature,
            ["activeParameter"] = Math.Min(commaCount, signatures[activeSignature].Params.Length - 1)
        };
    }
}
