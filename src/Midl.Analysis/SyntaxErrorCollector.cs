using Midl.Analysis.Diagnostics;
using Midl.Syntax;

namespace Midl.Analysis;

/// <summary>
/// Walks a green syntax tree and collects diagnostics from parser error nodes
/// (MissingToken, SkippedTokens, ErrorToken). These represent syntax errors
/// that the error-recovering parser preserved in the tree.
/// </summary>
public static class SyntaxErrorCollector
{
    /// <summary>
    /// Walk the entire green tree and collect syntax diagnostics.
    /// </summary>
    public static void CollectErrors(GreenNode root, IReadOnlyList<SyntaxToken> tokens, List<Diagnostic> diagnostics)
    {
        int position = 0;

        foreach (var token in tokens)
        {
            // Check leading trivia for skipped tokens
            foreach (var trivia in token.LeadingTrivia)
            {
                if (trivia.Kind == SyntaxKind.SkippedTokensTrivia && trivia is SyntaxTrivia st && st.Text.Trim().Length > 0)
                {
                    var text = st.Text.Trim();
                    diagnostics.Add(new Diagnostic(
                        DiagnosticSeverity.Error,
                        "MIDL1100",
                        $"Unexpected code '{text}'. Did you mean to put this inside a namespace or runtimeclass?",
                        position,
                        Math.Max(1, trivia.FullWidth)));
                }
                position += trivia.FullWidth;
            }

            // Check the token itself
            if (token.Kind == SyntaxKind.ErrorToken && token.Text.Length > 0)
            {
                var message = FormatErrorTokenMessage(token.Text);
                diagnostics.Add(new Diagnostic(
                    DiagnosticSeverity.Error,
                    "MIDL1101",
                    message,
                    position,
                    Math.Max(1, token.Text.Length)));
            }

            position += token.Text.Length;

            // Check trailing trivia for skipped tokens
            foreach (var trivia in token.TrailingTrivia)
            {
                if (trivia.Kind == SyntaxKind.SkippedTokensTrivia && trivia is SyntaxTrivia st2 && st2.Text.Trim().Length > 0)
                {
                    var text = st2.Text.Trim();
                    diagnostics.Add(new Diagnostic(
                        DiagnosticSeverity.Error,
                        "MIDL1100",
                        $"Unexpected code '{text}'. Did you mean to put this inside a namespace or runtimeclass?",
                        position,
                        Math.Max(1, trivia.FullWidth)));
                }
                position += trivia.FullWidth;
            }
        }

        // Walk the green tree to find MissingTokens (parser-synthesized)
        // Pass the token list so we can compute positions
        CollectMissingTokens(root, tokens, diagnostics);
    }

    private static void CollectMissingTokens(GreenNode node, IReadOnlyList<SyntaxToken> tokens, List<Diagnostic> diagnostics)
    {
        // Walk the tree, tracking cumulative position through FullWidth
        CollectMissingTokensRecursive(node, 0, diagnostics);
    }

    private static int CollectMissingTokensRecursive(GreenNode node, int position, List<Diagnostic> diagnostics)
    {
        if (node is SyntaxToken token)
        {
            if (token.Kind == SyntaxKind.MissingToken && token.Value is string expected)
            {
                diagnostics.Add(new Diagnostic(
                    DiagnosticSeverity.Error,
                    "MIDL1102",
                    $"Expected '{expected}'",
                    position,
                    1));
            }
            return position + token.FullWidth;
        }

        // SkippedTokens node anywhere in the tree — parser had to skip to recover
        if (node.Kind == SyntaxKind.SkippedTokens)
        {
            var text = CollectText(node).Trim();
            // Skip content that's purely semicolons (trailing semicolons after declarations)
            if (text.Length > 0 && text.Replace(";", "").Trim().Length > 0)
            {
                diagnostics.Add(new Diagnostic(
                    DiagnosticSeverity.Error,
                    "MIDL1100",
                    $"Unexpected code '{TruncateText(text)}'.",
                    position,
                    Math.Max(1, node.FullWidth)));
            }
            return position + node.FullWidth;
        }

        int pos = position;
        for (int i = 0; i < node.SlotCount; i++)
        {
            var slot = node.GetSlot(i);
            if (slot != null)
            {
                pos = CollectMissingTokensRecursive(slot, pos, diagnostics);
            }
        }
        return pos;
    }

    private static string TruncateText(string text)
    {
        return text.Length > 40 ? text[..37] + "..." : text;
    }

    private static string CollectText(GreenNode node)
    {
        if (node is SyntaxToken t) return t.Text;
        var parts = new List<string>();
        for (int i = 0; i < node.SlotCount; i++)
        {
            var slot = node.GetSlot(i);
            if (slot != null)
            {
                var text = CollectText(slot);
                if (text.Length > 0) parts.Add(text);
            }
        }
        return string.Join(" ", parts);
    }

    private static string FormatErrorTokenMessage(string text)
    {
        if (text is "@" or "#" or "$")
            return $"Unexpected character '{text}'. MIDL does not use this character.";

        return $"Unexpected character '{text}'";
    }
}
