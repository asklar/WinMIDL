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
                    diagnostics.Add(new Diagnostic(
                        DiagnosticSeverity.Error,
                        "MIDL1100",
                        $"Unexpected token(s): '{st.Text.Trim()}'",
                        position,
                        Math.Max(1, trivia.FullWidth)));
                }
                position += trivia.FullWidth;
            }

            // Check the token itself
            if (token.Kind == SyntaxKind.ErrorToken && token.Text.Length > 0)
            {
                diagnostics.Add(new Diagnostic(
                    DiagnosticSeverity.Error,
                    "MIDL1101",
                    $"Unexpected character '{token.Text}'",
                    position,
                    Math.Max(1, token.Text.Length)));
            }

            position += token.Text.Length;

            // Check trailing trivia for skipped tokens
            foreach (var trivia in token.TrailingTrivia)
            {
                if (trivia.Kind == SyntaxKind.SkippedTokensTrivia && trivia is SyntaxTrivia st2 && st2.Text.Trim().Length > 0)
                {
                    diagnostics.Add(new Diagnostic(
                        DiagnosticSeverity.Error,
                        "MIDL1100",
                        $"Unexpected token(s): '{st2.Text.Trim()}'",
                        position,
                        Math.Max(1, trivia.FullWidth)));
                }
                position += trivia.FullWidth;
            }
        }

        // Note: We intentionally do NOT report MissingToken as errors here.
        // MissingToken nodes are the parser's way of recovering — they indicate
        // the parser expected something but continued. The semantic binder will
        // report meaningful errors for constructs that don't make sense.
        // Reporting every MissingToken would flood the user with noise.
    }
}
