// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
                ExtractArguments(child, data.Arguments, childOffset);

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

    private static void ExtractArguments(GreenNode argListNode, List<AttributeArgValue> arguments, int listOffset)
    {
        int childOffset = listOffset;
        for (int i = 0; i < argListNode.SlotCount; i++)
        {
            var slot = argListNode.GetSlot(i);
            if (slot == null) continue;

            if (slot.Kind == SyntaxKind.AttributeArgument)
            {
                var argValue = ExtractArgValue(slot, childOffset);
                if (argValue != null)
                    arguments.Add(argValue);
            }

            childOffset += slot.FullWidth;
        }
    }

    private static AttributeArgValue? ExtractArgValue(GreenNode argNode, int argOffset)
    {
        // An AttributeArgument has one main child: a literal token, identifier, dotted name, or expression
        int slotOffset = argOffset;
        for (int i = 0; i < argNode.SlotCount; i++)
        {
            var slot = argNode.GetSlot(i);
            if (slot == null) continue;

            if (slot is SyntaxToken token)
            {
                int tokenPos = slotOffset + (token.LeadingTrivia?.Sum(t => t.FullWidth) ?? 0);
                int tokenLen = token.Text.Length;

                return token.Kind switch
                {
                    SyntaxKind.StringLiteralToken => new AttributeArgValue
                    {
                        Kind = ArgKind.String,
                        StringValue = StripQuotes(token.Text),
                        Position = tokenPos, Length = tokenLen
                    },
                    SyntaxKind.NumericLiteralToken => new AttributeArgValue
                    {
                        Kind = ArgKind.Integer,
                        IntValue = ParseNumeric(token.Text),
                        StringValue = token.Text,
                        Position = tokenPos, Length = tokenLen
                    },
                    SyntaxKind.FloatLiteralToken => new AttributeArgValue
                    {
                        Kind = ArgKind.Float,
                        FloatValue = double.TryParse(token.Text, System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out var f) ? f : 0,
                        StringValue = token.Text,
                        Position = tokenPos, Length = tokenLen
                    },
                    SyntaxKind.GuidLiteralToken => new AttributeArgValue
                    {
                        Kind = ArgKind.Guid,
                        GuidValue = Guid.TryParse(StripQuotes(token.Text), out var g) ? g : default,
                        StringValue = StripQuotes(token.Text),
                        Position = tokenPos, Length = tokenLen
                    },
                    SyntaxKind.IdentifierToken => new AttributeArgValue
                    {
                        Kind = ArgKind.Identifier,
                        StringValue = token.Text,
                        Position = tokenPos, Length = tokenLen
                    },
                    // Keyword tokens used as identifiers in attribute args
                    _ when IsKeywordIdentifier(token.Kind) => new AttributeArgValue
                    {
                        Kind = ArgKind.Identifier,
                        StringValue = token.Text,
                        Position = tokenPos, Length = tokenLen
                    },
                    _ => new AttributeArgValue
                    {
                        Kind = ArgKind.Expression,
                        StringValue = token.Text,
                        Position = tokenPos, Length = tokenLen
                    }
                };
            }

            // DottedName → TypeName (e.g. Windows.Foundation.UniversalApiContract)
            if (slot.Kind == SyntaxKind.DottedName)
            {
                int namePos = slotOffset + GetLeadingTriviaWidth(slot);
                return new AttributeArgValue
                {
                    Kind = ArgKind.TypeName,
                    StringValue = ExtractName(slot),
                    Position = namePos, Length = slot.FullWidth - GetLeadingTriviaWidth(slot)
                };
            }

            // GuidExpression → GUID (unquoted: 12345678-1234-1234-1234-123456789abc)
            if (slot.Kind == SyntaxKind.GuidExpression)
            {
                var guidText = CollectText(slot).Replace(" ", "");
                int gPos = slotOffset + GetLeadingTriviaWidth(slot);
                return new AttributeArgValue
                {
                    Kind = ArgKind.Guid,
                    GuidValue = Guid.TryParse(guidText, out var g) ? g : default,
                    StringValue = guidText,
                    Position = gPos, Length = slot.FullWidth - GetLeadingTriviaWidth(slot)
                };
            }

            // SimpleName → Identifier
            if (slot.Kind == SyntaxKind.SimpleName)
            {
                int sPos = slotOffset + GetLeadingTriviaWidth(slot);
                return new AttributeArgValue
                {
                    Kind = ArgKind.Identifier,
                    StringValue = ExtractName(slot),
                    Position = sPos, Length = slot.FullWidth - GetLeadingTriviaWidth(slot)
                };
            }

            // QualifiedName → TypeName
            if (slot.Kind == SyntaxKind.QualifiedName)
            {
                int qPos = slotOffset + GetLeadingTriviaWidth(slot);
                return new AttributeArgValue
                {
                    Kind = ArgKind.TypeName,
                    StringValue = ExtractName(slot),
                    Position = qPos, Length = slot.FullWidth - GetLeadingTriviaWidth(slot)
                };
            }

            // ParenthesizedExpression or other → Expression
            {
                int ePos = slotOffset + GetLeadingTriviaWidth(slot);
                return new AttributeArgValue
                {
                    Kind = ArgKind.Expression,
                    StringValue = CollectText(slot),
                    Position = ePos, Length = slot.FullWidth - GetLeadingTriviaWidth(slot)
                };
            }
        }

        return null;
    }

    private static string ExtractName(GreenNode node)
    {
        if (node is SyntaxToken t) return t.Text;

        if (node.Kind == SyntaxKind.DottedName || node.Kind == SyntaxKind.QualifiedName)
        {
            System.Text.StringBuilder? sb = null;
            for (int i = 0; i < node.SlotCount; i += 2)
            {
                if (node.GetSlot(i) is SyntaxToken tok)
                {
                    if (sb == null)
                        sb = new System.Text.StringBuilder(tok.Text);
                    else
                        sb.Append('.').Append(tok.Text);
                }
            }
            return sb?.ToString() ?? "";
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
        bool negative = text.StartsWith('-');
        var s = negative ? text.AsSpan(1) : text.AsSpan();

        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
            long.TryParse(s[2..], System.Globalization.NumberStyles.HexNumber, null, out var hex))
            return negative ? -hex : hex;
        if (long.TryParse(text, out var val))
            return val;
        return 0;
    }

    private static bool IsKeywordIdentifier(SyntaxKind kind) =>
        kind is SyntaxKind.ContractKeyword or SyntaxKind.RequiresKeyword
            or SyntaxKind.FlagsKeyword or SyntaxKind.EventKeyword
            or SyntaxKind.GetKeyword or SyntaxKind.SetKeyword
            or SyntaxKind.ProtectedKeyword;

    private static string CollectText(GreenNode node)
    {
        if (node is SyntaxToken t) return t.Text;
        var sb = new System.Text.StringBuilder();
        CollectTextCore(node, sb);
        return sb.ToString();
    }

    private static void CollectTextCore(GreenNode node, System.Text.StringBuilder sb)
    {
        for (int i = 0; i < node.SlotCount; i++)
        {
            var slot = node.GetSlot(i);
            if (slot == null) continue;
            if (slot is SyntaxToken t)
                sb.Append(t.Text);
            else
                CollectTextCore(slot, sb);
        }
    }
}
