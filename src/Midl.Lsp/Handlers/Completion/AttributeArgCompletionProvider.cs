// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.Analysis.Symbols;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Provides contextual completions for attribute arguments: [attrName(arg0, arg1|)].
/// For example, [contract(| → offers API contract names, [threading(| → offers both/sta/mta.
/// </summary>
internal static class AttributeArgCompletionProvider
{
    /// <summary>
    /// Detects if the cursor is inside an attribute argument list [attrName(args|)].
    /// Returns the attribute name (lowercase) and argument position (0-based comma count), or null.
    /// </summary>
    internal static (string attrName, int argPosition)? DetectAttributeArgContext(string text, int offset)
    {
        // Walk backward from cursor to find an unmatched '(' while tracking commas and nested parens
        int parenDepth = 0;
        int commaCount = 0;
        int openParenPos = -1;

        for (int i = offset - 1; i >= 0; i--)
        {
            char c = text[i];
            if (c == ')') parenDepth++;
            else if (c == '(')
            {
                if (parenDepth > 0) { parenDepth--; continue; }
                openParenPos = i;
                break;
            }
            else if (c == ',' && parenDepth == 0)
            {
                commaCount++;
            }
            // Stop at line boundaries or closing brackets that indicate we're not in an attr arg
            else if (c == ']' || c == '}' || c == ';')
            {
                return null;
            }
        }

        if (openParenPos < 0)
            return null;

        // Extract the identifier before '(' — skip whitespace
        int identEnd = openParenPos;
        int identStart = identEnd - 1;
        while (identStart >= 0 && char.IsWhiteSpace(text[identStart]))
            identStart--;

        if (identStart < 0)
            return null;

        // Collect identifier chars (letters, digits, underscore)
        int identActualEnd = identStart + 1;
        while (identStart >= 0 && (char.IsLetterOrDigit(text[identStart]) || text[identStart] == '_'))
            identStart--;
        identStart++;

        if (identStart >= identActualEnd)
            return null;

        string attrName = text[identStart..identActualEnd];

        // Now verify we're inside an attribute bracket: look for unmatched '[' before the identifier
        int bracketDepth = 0;
        for (int i = identStart - 1; i >= 0; i--)
        {
            char c = text[i];
            if (char.IsWhiteSpace(c)) continue;
            if (c == ']') bracketDepth++;
            else if (c == '[')
            {
                if (bracketDepth > 0) { bracketDepth--; continue; }
                // Found unmatched '[' — we're in an attribute context
                return (attrName.ToLowerInvariant(), commaCount);
            }
            else if (c == ',')
            {
                // Could be after another attribute: [attr1, attrName(|
                continue;
            }
            else if (c == ')')
            {
                // Previous attribute with args: [attr1(...), attrName(|
                // Skip the previous parens
                int depth = 1;
                i--;
                while (i >= 0 && depth > 0)
                {
                    if (text[i] == ')') depth++;
                    else if (text[i] == '(') depth--;
                    i--;
                }
                // Now i should be just before the previous attr name; continue scanning
                continue;
            }
            else
            {
                // Some other character — could be part of a previous attr identifier, skip it
                while (i >= 0 && (char.IsLetterOrDigit(text[i]) || text[i] == '_'))
                    i--;
                // Don't need i++ here — the for loop's i-- will advance past
                // the character before the identifier we just skipped
                continue;
            }
        }

        return null;
    }

    /// <summary>
    /// Builds completion items for the detected attribute argument context.
    /// </summary>
    internal static List<object>? BuildAttributeArgCompletions(
        string attrName, int argPosition, Compilation? compilation)
    {
        return attrName switch
        {
            "contract" => BuildContractArgCompletions(argPosition, compilation),
            "composable" => BuildComposableArgCompletions(argPosition, compilation),
            "static" => BuildStaticArgCompletions(argPosition, compilation),
            "activatable" => BuildActivatableArgCompletions(argPosition, compilation),
            "exclusiveto" => BuildExclusiveToArgCompletions(argPosition, compilation),
            "threading" => BuildFixedChoiceCompletions(argPosition, 0, ["both", "sta", "mta"]),
            "marshaling_behavior" => BuildFixedChoiceCompletions(argPosition, 0, ["agile", "standard", "none"]),
            "attributeusage" => BuildFixedChoiceCompletions(argPosition, 0,
                ["target_runtimeclass", "target_interface", "target_enum", "target_struct",
                 "target_delegate", "target_method", "target_property", "target_event", "target_parameter"]),
            "deprecated" => BuildDeprecatedArgCompletions(argPosition, compilation),
            _ => null,
        };
    }

    private static List<object>? BuildContractArgCompletions(int argPosition, Compilation? compilation)
    {
        // contract(apicontractName, version)
        return argPosition switch
        {
            0 => BuildApiContractCompletions(compilation),
            _ => null, // version — no completion
        };
    }

    private static List<object>? BuildComposableArgCompletions(int argPosition, Compilation? compilation)
    {
        // composable(IFactory, public|protected, ContractName, version)
        return argPosition switch
        {
            0 => BuildInterfaceCompletions(compilation),
            1 => BuildItems(["public", "protected"], CompletionHandler.KindKeyword, "Access modifier"),
            2 => BuildApiContractCompletions(compilation),
            _ => null,
        };
    }

    private static List<object>? BuildStaticArgCompletions(int argPosition, Compilation? compilation)
    {
        // static(IStatics, ContractName, version)
        return argPosition switch
        {
            0 => BuildInterfaceCompletions(compilation),
            1 => BuildApiContractCompletions(compilation),
            _ => null,
        };
    }

    private static List<object>? BuildActivatableArgCompletions(int argPosition, Compilation? compilation)
    {
        // activatable(ContractName | IFactory, ContractName | version, version)
        return argPosition switch
        {
            0 => BuildCombinedCompletions(BuildApiContractCompletions(compilation), BuildInterfaceCompletions(compilation)),
            1 => BuildCombinedCompletions(BuildApiContractCompletions(compilation), null),
            _ => null,
        };
    }

    private static List<object>? BuildExclusiveToArgCompletions(int argPosition, Compilation? compilation)
    {
        // exclusiveto(ClassName)
        return argPosition switch
        {
            0 => BuildRuntimeClassCompletions(compilation),
            _ => null,
        };
    }

    private static List<object>? BuildDeprecatedArgCompletions(int argPosition, Compilation? compilation)
    {
        // deprecated("message", deprecate|remove, ContractName, version)
        return argPosition switch
        {
            1 => BuildItems(["deprecate", "remove"], CompletionHandler.KindKeyword, "Deprecation type"),
            2 => BuildApiContractCompletions(compilation),
            _ => null, // arg 0 is a string, arg 3 is version
        };
    }

    private static List<object>? BuildFixedChoiceCompletions(int argPosition, int targetArg, string[] choices)
    {
        if (argPosition != targetArg)
            return null;
        return BuildItems(choices, CompletionHandler.KindEnumMember, "Attribute argument");
    }

    private static List<object> BuildItems(string[] labels, int kind, string detail)
    {
        var items = new List<object>();
        foreach (var label in labels)
        {
            items.Add(new Dictionary<string, object?>
            {
                ["label"] = label,
                ["kind"] = kind,
                ["detail"] = detail,
                ["insertText"] = label,
            });
        }
        return items;
    }

    private static List<object> BuildApiContractCompletions(Compilation? compilation)
    {
        var items = new List<object>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (compilation != null)
        {
            // Local api contracts
            foreach (var type in compilation.GetAllTypes())
            {
                if (type is ApiContractSymbol contract && seen.Add(contract.Name))
                {
                    items.Add(new Dictionary<string, object?>
                    {
                        ["label"] = contract.Name,
                        ["kind"] = CompletionHandler.KindKeyword,
                        ["detail"] = $"API contract: {contract.FullName}",
                        ["insertText"] = contract.Name,
                    });
                }
            }

            // SDK/WinMD lazy providers
            foreach (var fullName in compilation.GetAllLazyTypeNames())
            {
                if (string.IsNullOrEmpty(fullName)) continue;
                // API contracts typically end with "Contract" or "ApiContract"
                if (!fullName.Contains("Contract", StringComparison.Ordinal)) continue;

                var simpleName = fullName.Contains('.')
                    ? fullName[(fullName.LastIndexOf('.') + 1)..]
                    : fullName;
                if (!seen.Add(simpleName)) continue;

                try
                {
                    var symbol = compilation.GetTypeByName(fullName);
                    if (symbol is ApiContractSymbol sdkContract)
                    {
                        items.Add(new Dictionary<string, object?>
                        {
                            ["label"] = sdkContract.Name,
                            ["kind"] = CompletionHandler.KindKeyword,
                            ["detail"] = $"(SDK) API contract: {sdkContract.FullName}",
                            ["insertText"] = sdkContract.Name,
                        });
                    }
                }
                catch { }
            }
        }

        return items;
    }

    private static List<object> BuildInterfaceCompletions(Compilation? compilation)
    {
        var items = new List<object>();
        if (compilation == null) return items;

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var type in compilation.GetAllTypes())
        {
            if (type is InterfaceSymbol iface && seen.Add(iface.Name))
            {
                items.Add(new Dictionary<string, object?>
                {
                    ["label"] = iface.Name,
                    ["kind"] = CompletionHandler.KindInterface,
                    ["detail"] = $"Interface: {iface.FullName}",
                    ["insertText"] = iface.Name,
                });
            }
        }
        return items;
    }

    private static List<object> BuildRuntimeClassCompletions(Compilation? compilation)
    {
        var items = new List<object>();
        if (compilation == null) return items;

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var type in compilation.GetAllTypes())
        {
            if (type is RuntimeClassSymbol rc && seen.Add(rc.Name))
            {
                items.Add(new Dictionary<string, object?>
                {
                    ["label"] = rc.Name,
                    ["kind"] = CompletionHandler.KindClass,
                    ["detail"] = $"Runtime class: {rc.FullName}",
                    ["insertText"] = rc.Name,
                });
            }
        }
        return items;
    }

    private static List<object>? BuildCombinedCompletions(List<object>? a, List<object>? b)
    {
        if (a == null && b == null) return null;
        var result = new List<object>();
        if (a != null) result.AddRange(a);
        if (b != null) result.AddRange(b);
        return result.Count > 0 ? result : null;
    }
}
