using Midl.Analysis.Symbols;
using Midl.Syntax;

namespace Midl.Analysis;

// Partial class extension for doc comment extraction.
public partial class Binder
{
    /// <summary>
    /// Extract a DocComment from the leading trivia of a syntax node's first token.
    /// Concatenates consecutive /// lines, strips the /// prefix, and parses the XML.
    /// </summary>
    private static DocComment? ExtractDocComment(GreenNode node)
    {
        var firstToken = FindFirstToken(node);
        if (firstToken is null) return null;

        return ExtractDocCommentFromTrivia(firstToken.LeadingTrivia);
    }

    /// <summary>
    /// Extract a DocComment from a trivia array (typically leading trivia of a token).
    /// </summary>
    private static DocComment? ExtractDocCommentFromTrivia(GreenNode[] trivia)
    {
        // Collect all consecutive DocCommentTrivia nodes (they may be interleaved with whitespace/EOL)
        var docLines = new List<string>();
        foreach (var t in trivia)
        {
            if (t.Kind == SyntaxKind.DocCommentTrivia && t is SyntaxTrivia st)
            {
                docLines.Add(StripDocCommentPrefix(st.Text));
            }
        }

        if (docLines.Count == 0) return null;

        var xmlText = string.Join("\n", docLines);
        return DocComment.Parse(xmlText);
    }

    /// <summary>
    /// Strip the /// prefix (and optional leading space) from a doc comment line.
    /// </summary>
    private static string StripDocCommentPrefix(string text)
    {
        // Text starts with "///" — strip it, plus one optional space
        if (text.StartsWith("/// "))
            return text[4..];
        if (text.StartsWith("///"))
            return text[3..];
        return text;
    }

    /// <summary>
    /// Attach doc comment to a type symbol based on its declaration node.
    /// </summary>
    private static void AttachDocComment(ISymbol symbol, GreenNode declarationNode)
    {
        var doc = ExtractDocComment(declarationNode);
        if (doc != null)
            symbol.DocComment = doc;
    }

    /// <summary>
    /// Attach doc comment to a member symbol based on its declaration node.
    /// </summary>
    private static void AttachMemberDocComment(ISymbol symbol, GreenNode memberNode)
    {
        var doc = ExtractDocComment(memberNode);
        if (doc != null)
            symbol.DocComment = doc;
    }
}
