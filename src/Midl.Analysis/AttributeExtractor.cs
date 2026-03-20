using Midl.Syntax;

namespace Midl.Analysis;

/// <summary>
/// Walks an AttributeList green node and produces a list of AttributeData.
/// Positions are computed from green tree FullWidth accumulation (not text-based lookup)
/// to ensure each attribute usage gets the correct source position.
/// </summary>
public static class AttributeExtractor
{
    /// <summary>Extract all attributes from an AttributeList green node.</summary>
    /// <param name="attrListNode">The AttributeList green node.</param>
    /// <param name="baseOffset">Absolute source offset of the attrListNode start.</param>
    public static List<AttributeData> Extract(GreenNode? attrListNode, int baseOffset = 0)
    {
        var result = new List<AttributeData>();
        if (attrListNode == null) return result;

        ExtractFromNode(attrListNode, result, baseOffset);
        return result;
    }

    private static void ExtractFromNode(GreenNode node, List<AttributeData> result, int offset)
    {
        int childOffset = offset;
        for (int i = 0; i < node.SlotCount; i++)
        {
            var slot = node.GetSlot(i);
            if (slot == null) continue;

            if (slot.Kind == SyntaxKind.AttributeList)
            {
                ExtractFromNode(slot, result, childOffset);
            }
            else if (slot.Kind == SyntaxKind.Attribute)
            {
                var attr = ExtractAttribute(slot, childOffset);
                if (attr != null)
                    result.Add(attr);
            }

            childOffset += slot.FullWidth;
        }
    }

    private static AttributeData? ExtractAttribute(GreenNode attrNode, int nodeOffset)
    {
        // First child is the name (SimpleName, DottedName, or IdentifierToken)
        var nameNode = attrNode.GetSlot(0);
        if (nameNode == null) return null;

        var name = ExtractName(nameNode);
        if (string.IsNullOrEmpty(name)) return null;

        // Compute position of name node's text by skipping its leading trivia
        int position = nodeOffset + GetLeadingTriviaWidth(nameNode);

        var data = new AttributeData { Name = name, Position = position };

        // Find AttributeArgumentList child
        int childOffset = nodeOffset;
        for (int j = 0; j < attrNode.SlotCount; j++)
        {
            var child = attrNode.GetSlot(j);
            if (child == null) continue;

            if (child.Kind == SyntaxKind.AttributeArgumentList)
                ExtractArguments(child, data.Arguments);

            childOffset += child.FullWidth;
        }

        return data;
    }

    /// <summary>Get the width of leading trivia for the first token in a node.</summary>
    private static int GetLeadingTriviaWidth(GreenNode node)
    {
        var first = FindFirstToken(node);
        return first?.LeadingTrivia.Sum(t => t.FullWidth) ?? 0;
    }

    /// <summary>
    /// Compute the absolute source offset of a target green node within a root tree.
    /// Uses reference equality to find the target.
    /// </summary>
    public static int? FindNodeOffset(GreenNode root, GreenNode target, int currentOffset = 0)
    {
        if (ReferenceEquals(root, target)) return currentOffset;
        int offset = currentOffset;
        for (int i = 0; i < root.SlotCount; i++)
        {
            var slot = root.GetSlot(i);
            if (slot == null) continue;
            var result = FindNodeOffset(slot, target, offset);
            if (result.HasValue) return result;
            offset += slot.FullWidth;
        }
        return null;
    }

    private static void ExtractArguments(GreenNode argListNode, List<AttributeArgValue> arguments)
    {
        for (int i = 0; i < argListNode.SlotCount; i++)
        {
            var slot = argListNode.GetSlot(i);
            if (slot == null) continue;

            if (slot.Kind == SyntaxKind.AttributeArgument)
            {
                var argValue = ExtractArgValue(slot);
                if (argValue != null)
                    arguments.Add(argValue);
            }
        }
    }

    private static AttributeArgValue? ExtractArgValue(GreenNode argNode)
    {
        // An AttributeArgument has one main child: a literal token, identifier, dotted name, or expression
        for (int i = 0; i < argNode.SlotCount; i++)
        {
            var slot = argNode.GetSlot(i);
            if (slot == null) continue;

            if (slot is SyntaxToken token)
            {
                return token.Kind switch
                {
                    SyntaxKind.StringLiteralToken => new AttributeArgValue
                    {
                        Kind = ArgKind.String,
                        StringValue = StripQuotes(token.Text)
                    },
                    SyntaxKind.NumericLiteralToken => new AttributeArgValue
                    {
                        Kind = ArgKind.Integer,
                        IntValue = ParseNumeric(token.Text)
                    },
                    SyntaxKind.GuidLiteralToken => new AttributeArgValue
                    {
                        Kind = ArgKind.Guid,
                        GuidValue = Guid.TryParse(StripQuotes(token.Text), out var g) ? g : default,
                        StringValue = StripQuotes(token.Text)
                    },
                    SyntaxKind.IdentifierToken => new AttributeArgValue
                    {
                        Kind = ArgKind.Identifier,
                        StringValue = token.Text
                    },
                    // Keyword tokens used as identifiers in attribute args
                    _ when IsKeywordIdentifier(token.Kind) => new AttributeArgValue
                    {
                        Kind = ArgKind.Identifier,
                        StringValue = token.Text
                    },
                    _ => new AttributeArgValue
                    {
                        Kind = ArgKind.Expression,
                        StringValue = token.Text
                    }
                };
            }

            // DottedName → TypeName (e.g. Windows.Foundation.UniversalApiContract)
            if (slot.Kind == SyntaxKind.DottedName)
            {
                return new AttributeArgValue
                {
                    Kind = ArgKind.TypeName,
                    StringValue = ExtractName(slot)
                };
            }

            // SimpleName → Identifier
            if (slot.Kind == SyntaxKind.SimpleName)
            {
                return new AttributeArgValue
                {
                    Kind = ArgKind.Identifier,
                    StringValue = ExtractName(slot)
                };
            }

            // QualifiedName → TypeName
            if (slot.Kind == SyntaxKind.QualifiedName)
            {
                return new AttributeArgValue
                {
                    Kind = ArgKind.TypeName,
                    StringValue = ExtractName(slot)
                };
            }

            // ParenthesizedExpression or other → Expression
            return new AttributeArgValue
            {
                Kind = ArgKind.Expression,
                StringValue = CollectText(slot)
            };
        }

        return null;
    }

    private static string ExtractName(GreenNode node)
    {
        if (node is SyntaxToken t) return t.Text;

        if (node.Kind == SyntaxKind.DottedName || node.Kind == SyntaxKind.QualifiedName)
        {
            var parts = new List<string>();
            for (int i = 0; i < node.SlotCount; i += 2)
            {
                if (node.GetSlot(i) is SyntaxToken tok)
                    parts.Add(tok.Text);
            }
            return string.Join(".", parts);
        }

        if (node.Kind == SyntaxKind.SimpleName)
        {
            var inner = node.GetSlot(0);
            if (inner is SyntaxToken st) return st.Text;
        }

        // Fallback: collect first token text
        var first = FindFirstToken(node);
        return first?.Text ?? "";
    }

    private static SyntaxToken? FindFirstToken(GreenNode node)
    {
        if (node is SyntaxToken t) return t;
        for (int i = 0; i < node.SlotCount; i++)
        {
            var slot = node.GetSlot(i);
            if (slot == null) continue;
            var result = FindFirstToken(slot);
            if (result != null) return result;
        }
        return null;
    }

    private static string StripQuotes(string text)
    {
        if (text.Length >= 2 && text[0] == '"' && text[^1] == '"')
            return text[1..^1];
        return text;
    }

    private static long ParseNumeric(string text)
    {
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
            long.TryParse(text.AsSpan(2), System.Globalization.NumberStyles.HexNumber, null, out var hex))
            return hex;
        if (long.TryParse(text, out var val))
            return val;
        return 0;
    }

    private static bool IsKeywordIdentifier(SyntaxKind kind) =>
        kind is SyntaxKind.ContractKeyword or SyntaxKind.RequiresKeyword
            or SyntaxKind.FlagsKeyword or SyntaxKind.EventKeyword
            or SyntaxKind.GetKeyword or SyntaxKind.SetKeyword;

    private static string CollectText(GreenNode node)
    {
        if (node is SyntaxToken t) return t.Text;
        var parts = new List<string>();
        for (int i = 0; i < node.SlotCount; i++)
        {
            var slot = node.GetSlot(i);
            if (slot != null)
                parts.Add(CollectText(slot));
        }
        return string.Join("", parts);
    }
}
