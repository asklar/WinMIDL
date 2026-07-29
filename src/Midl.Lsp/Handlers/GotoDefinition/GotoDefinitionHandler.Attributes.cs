// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.Syntax;

namespace Midl.Lsp.Handlers;

public static partial class GotoDefinitionHandler
{
    /// <summary>
    /// If the cursor is inside [...] attribute brackets, try to navigate to the attribute type definition.
    /// Returns an LSP Location (or command) on success, null if the cursor isn't in an attribute context.
    /// </summary>
    internal static object? TryResolveAttributeDefinition(
        string sourceText, IReadOnlyList<SyntaxToken> tokens, int offset,
        SyntaxToken identifierToken, Compilation compilation,
        int[] lineStarts, string uri)
    {
        if (!IsInsideAttributeBrackets(sourceText, offset))
            return null;

        var attrName = identifierToken.Text;
        var attrSymbol = ResolveAttributeSymbol(compilation, attrName);
        if (attrSymbol is null)
            return null;

        // Local attribute defined in current source
        if (!attrSymbol.IsExternal && attrSymbol.SourceFile is null)
        {
            int defOffset = FindAttributeDefinitionOffset(sourceText, attrSymbol.Name);
            if (defOffset >= 0)
            {
                var (defLine, defChar) = PositionHelper.OffsetToLineChar(lineStarts, defOffset);
                var (defEndLine, defEndChar) = PositionHelper.OffsetToLineChar(lineStarts, defOffset + attrSymbol.Name.Length);
                return MakeLocation(uri, defLine, defChar, defEndLine, defEndChar);
            }

            // Fallback: use SourcePosition from binding
            if (attrSymbol.SourcePosition >= 0)
            {
                var (defLine, defChar) = PositionHelper.OffsetToLineChar(lineStarts, attrSymbol.SourcePosition);
                var (defEndLine, defEndChar) = PositionHelper.OffsetToLineChar(lineStarts, attrSymbol.SourcePosition + attrSymbol.Name.Length);
                return MakeLocation(uri, defLine, defChar, defEndLine, defEndChar);
            }
        }

        // Imported .idl attribute
        if (attrSymbol.SourceFile != null && File.Exists(attrSymbol.SourceFile))
        {
            var otherText = File.ReadAllText(attrSymbol.SourceFile);
            int defOffset = FindAttributeDefinitionOffset(otherText, attrSymbol.Name);
            if (defOffset >= 0)
            {
                var otherLineStarts = PositionHelper.BuildLineStarts(otherText);
                var (defLine, defChar) = PositionHelper.OffsetToLineChar(otherLineStarts, defOffset);
                var (defEndLine, defEndChar) = PositionHelper.OffsetToLineChar(otherLineStarts, defOffset + attrSymbol.Name.Length);
                var otherUri = "file:///" + attrSymbol.SourceFile.Replace('\\', '/');
                return MakeLocation(otherUri, defLine, defChar, defEndLine, defEndChar);
            }
        }

        // External SDK/WinMD attribute
        if (attrSymbol.IsExternal && attrSymbol.SourceWinMdPath != null)
        {
            if (File.Exists(attrSymbol.SourceWinMdPath))
                return WinMdNavigator.CreateWinMdDefinitionLocation(attrSymbol);

            if (attrSymbol.SourceWinMdPath == "<built-in>")
            {
                var winmd = WinMdNavigator.FindWindowsWinMd();
                if (winmd != null)
                    return WinMdNavigator.CreateWinMdDefinitionLocation(
                        new WinMdNavigator.BuiltInSymbolProxy(attrSymbol, winmd));
            }
        }

        return null;
    }

    /// <summary>
    /// Determines whether the given offset in the source text is inside [...] attribute brackets.
    /// Walks backward from the offset, tracking bracket nesting, to find an unmatched '['.
    /// </summary>
    internal static bool IsInsideAttributeBrackets(string sourceText, int offset)
    {
        int depth = 0;
        for (int i = offset - 1; i >= 0; i--)
        {
            char c = sourceText[i];
            if (c == ']') depth++;
            else if (c == '[')
            {
                if (depth == 0) return true;
                depth--;
            }
            // Stop at statement/block boundaries
            else if (c == '{' || c == '}' || c == ';')
                return false;
        }
        return false;
    }

    /// <summary>
    /// Resolves an attribute name to its <see cref="AttributeTypeSymbol"/> in the compilation.
    /// Tries: exact name, name+"Attribute", PascalCase+"Attribute", FQN from WinMD providers,
    /// and AttributeNameOverride matching.
    /// </summary>
    internal static AttributeTypeSymbol? ResolveAttributeSymbol(Compilation compilation, string name)
    {
        // Search locally defined types first
        foreach (var type in compilation.GetAllTypes())
        {
            if (type is AttributeTypeSymbol at)
            {
                if (at.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return at;
                // Match short name (strip Attribute suffix)
                if (at.Name.EndsWith("Attribute", StringComparison.Ordinal) && at.Name.Length > 9 &&
                    at.Name[..^9].Equals(name, StringComparison.OrdinalIgnoreCase))
                    return at;
                if (at.AttributeNameOverride != null &&
                    at.AttributeNameOverride.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return at;
            }
        }

        // Try resolving from lazy WinMD providers
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
            var symbol = compilation.GetTypeByName(candidate);
            if (symbol is AttributeTypeSymbol at)
                return at;
        }

        // Also scan all lazy type names for attribute matches
        foreach (var fqn in compilation.GetAllLazyTypeNames())
        {
            if (!fqn.EndsWith("Attribute", StringComparison.Ordinal))
                continue;

            var simpleName = fqn;
            var lastDot = fqn.LastIndexOf('.');
            if (lastDot >= 0) simpleName = fqn[(lastDot + 1)..];

            // Match by simple name or short name
            if (simpleName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                simpleName.Equals(name + "Attribute", StringComparison.OrdinalIgnoreCase) ||
                (simpleName.EndsWith("Attribute", StringComparison.Ordinal) &&
                 simpleName[..^9].Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                var symbol = compilation.GetTypeByName(fqn);
                if (symbol is AttributeTypeSymbol at)
                {
                    // Also check AttributeNameOverride
                    if (at.AttributeNameOverride != null &&
                        at.AttributeNameOverride.Equals(name, StringComparison.OrdinalIgnoreCase))
                        return at;
                    return at;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the offset of "attribute Name" in source text, returning the offset of Name.
    /// </summary>
    private static int FindAttributeDefinitionOffset(string sourceText, string name)
    {
        string pattern = "attribute " + name;
        int searchFrom = 0;
        while (true)
        {
            int idx = sourceText.IndexOf(pattern, searchFrom, StringComparison.Ordinal);
            if (idx < 0) break;
            int afterPattern = idx + pattern.Length;
            if (afterPattern < sourceText.Length &&
                (char.IsLetterOrDigit(sourceText[afterPattern]) || sourceText[afterPattern] == '_'))
            {
                searchFrom = idx + 1;
                continue;
            }
            return idx + "attribute ".Length;
        }
        return -1;
    }
}
