using Midl.Analysis.Diagnostics;
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
    /// Strip doc comment prefixes from trivia text.
    /// Handles: /// , //! , /** ... */, /*! ... */
    /// </summary>
    private static string StripDocCommentPrefix(string text)
    {
        // Single-line: "/// " or "///"
        if (text.StartsWith("/// "))
            return text[4..];
        if (text.StartsWith("///"))
            return text[3..];
        // Single-line: "//! " or "//!"
        if (text.StartsWith("//! "))
            return text[4..];
        if (text.StartsWith("//!"))
            return text[3..];
        // Multi-line: /** ... */ or /*! ... */
        if ((text.StartsWith("/**") || text.StartsWith("/*!")) && text.EndsWith("*/"))
        {
            var inner = text[3..^2]; // strip /** and */
            // Split into lines and strip leading * on each
            var lines = inner.Split('\n');
            var sb = new System.Text.StringBuilder();
            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimStart();
                if (line.StartsWith("* "))
                    line = line[2..];
                else if (line.Length > 1 && line[0] == '*' && (line[1] == '@' || line[1] == '\\'))
                    line = line[1..]; // *@brief or *\brief — strip only the *
                else if (line == "*")
                    line = "";
                if (sb.Length > 0 && line.Length > 0)
                    sb.Append('\n');
                if (line.Length > 0)
                    sb.Append(line);
            }
            return sb.ToString();
        }
        return text;
    }

    /// <summary>
    /// Attach doc comment to a type symbol based on its declaration node.
    /// Emits MIDL3021 for any parse errors.
    /// </summary>
    private void AttachDocComment(ISymbol symbol, GreenNode declarationNode)
    {
        var doc = ExtractDocComment(declarationNode);
        if (doc != null)
        {
            symbol.DocComment = doc;
            EmitDocCommentErrors(doc, declarationNode);
        }
    }

    /// <summary>
    /// Attach doc comment to a member symbol based on its declaration node.
    /// Emits MIDL3021 for any parse errors.
    /// </summary>
    private void AttachMemberDocComment(ISymbol symbol, GreenNode memberNode)
    {
        var doc = ExtractDocComment(memberNode);
        if (doc != null)
        {
            symbol.DocComment = doc;
            EmitDocCommentErrors(doc, memberNode);
        }
    }

    private void EmitDocCommentErrors(DocComment doc, GreenNode node)
    {
        if (doc.ParseErrors.Count == 0) return;
        int pos = GetNodePosition(node);
        foreach (var error in doc.ParseErrors)
        {
            _diagnostics.Add(DiagnosticDescriptors.MalformedDocComment(error, pos));
        }
    }
}
