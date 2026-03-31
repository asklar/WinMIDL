using Midl.Syntax;

namespace Midl.Parser;

public sealed partial class Parser
{
    /// <summary>Parse: [attr1, attr2(args), ...]</summary>
    private GreenNode ParseAttributeList()
    {
        var children = RentChildren();
        children.Add(Eat(SyntaxKind.OpenBracketToken));

        // Error recovery: if we're already at ], produce an empty attribute list
        if (!At(SyntaxKind.CloseBracketToken))
        {
            children.Add(ParseAttribute());
            while (At(SyntaxKind.CommaToken))
            {
                children.Add(EatToken()); // comma
                if (At(SyntaxKind.CloseBracketToken)) break; // trailing comma
                children.Add(ParseAttribute());
            }
        }

        // Error recovery: skip unexpected tokens inside attribute list
        if (!At(SyntaxKind.CloseBracketToken) && !At(SyntaxKind.EndOfFileToken))
        {
            var skipped = SkipTokensUntil(SyntaxKind.CloseBracketToken, SyntaxKind.EndOfFileToken);
            if (skipped != null) children.Add(skipped);
        }

        children.Add(Eat(SyntaxKind.CloseBracketToken));
        return MakeNode(SyntaxKind.AttributeList, children);
    }

    /// <summary>Parse a single attribute: name or name(args)</summary>
    private GreenNode ParseAttribute()
    {
        var children = RentChildren();

        // Attribute name may be dotted (e.g. Windows.Foundation.Metadata.GuidAttribute)
        children.Add(ParseDottedName());

        // Optional argument list
        if (At(SyntaxKind.OpenParenToken))
        {
            children.Add(ParseAttributeArgumentList());
        }

        return MakeNode(SyntaxKind.Attribute, children);
    }

    /// <summary>Parse attribute arguments: (arg1, arg2, ...)</summary>
    private GreenNode ParseAttributeArgumentList()
    {
        var children = RentChildren();
        children.Add(Eat(SyntaxKind.OpenParenToken));

        if (!At(SyntaxKind.CloseParenToken))
        {
            children.Add(ParseAttributeArgument());
            while (At(SyntaxKind.CommaToken))
            {
                children.Add(EatToken()); // comma
                if (At(SyntaxKind.CloseParenToken)) break;
                children.Add(ParseAttributeArgument());
            }
        }

        // Error recovery: skip to ) if we got lost
        if (!At(SyntaxKind.CloseParenToken) && !At(SyntaxKind.EndOfFileToken))
        {
            var skipped = SkipTokensUntil(
                SyntaxKind.CloseParenToken,
                SyntaxKind.CloseBracketToken,
                SyntaxKind.EndOfFileToken);
            if (skipped != null) children.Add(skipped);
        }

        children.Add(Eat(SyntaxKind.CloseParenToken));
        return MakeNode(SyntaxKind.AttributeArgumentList, children);
    }

    /// <summary>Parse a single attribute argument: literal, identifier, dotted name, or nested expression.</summary>
    private GreenNode ParseAttributeArgument()
    {
        var children = RentChildren();

        // Handle various argument forms
        switch (Current.Kind)
        {
            case SyntaxKind.StringLiteralToken:
                children.Add(EatToken());
                break;

            case SyntaxKind.NumericLiteralToken:
            case SyntaxKind.FloatLiteralToken:
                // Check for unquoted GUID pattern: 12345678-1234-1234-1234-123456789abc
                // Note: 05fd4fdb lexes as NumericLiteral(05) + Identifier(fd4fdb) + MinusToken
                if (Current.Kind == SyntaxKind.NumericLiteralToken && IsGuidPatternFromHere())
                {
                    children.Add(ParseUnquotedGuid());
                }
                else
                {
                    children.Add(EatToken());
                }
                break;

            case SyntaxKind.GuidLiteralToken:
                children.Add(EatToken());
                break;

            case SyntaxKind.OpenParenToken:
                // Nested parenthesized expression
                children.Add(ParseParenthesizedAttrArg());
                break;

            default:
                // Identifier or dotted name, or keyword used as value
                if (IsIdentifierOrKeyword(Current.Kind))
                {
                    // Check for unquoted GUID starting with hex identifier: AAAAAAAA-BBBB-...
                    if (IsGuidPatternFromHere())
                    {
                        children.Add(ParseUnquotedGuid());
                    }
                    else
                    {
                        children.Add(ParseDottedName());
                    }
                }
                else
                {
                    // Unexpected token — eat it as an error
                    children.Add(EatToken());
                }
                break;
        }

        return MakeNode(SyntaxKind.AttributeArgument, children);
    }

    /// <summary>Check if a string is a valid GUID: 8hex-4hex-4hex-4hex-12hex.</summary>
    private static bool IsGuidFormat(string s)
    {
        if (s.Length != 36) return false;
        for (int i = 0; i < 36; i++)
        {
            if (i is 8 or 13 or 18 or 23)
            {
                if (s[i] != '-') return false;
            }
            else
            {
                if (!IsHexDigit(s[i])) return false;
            }
        }
        return true;

        static bool IsHexDigit(char c) =>
            (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
    }

    /// <summary>Check if tokens from current position form an unquoted GUID.</summary>
    private bool IsGuidPatternFromHere()
    {
        // Concatenate upcoming token texts (num, ident, minus) and check GUID format
        var sb = new System.Text.StringBuilder(40);
        for (int look = 0; look < 20; look++)
        {
            var t = Peek(look);
            if (t.Kind is SyntaxKind.NumericLiteralToken or SyntaxKind.IdentifierToken
                or SyntaxKind.MinusToken)
                sb.Append(t.Text);
            else
                break;
        }
        return IsGuidFormat(sb.ToString());
    }

    /// <summary>Consume tokens forming an unquoted GUID and produce a single GuidLiteralToken.</summary>
    private GreenNode ParseUnquotedGuid()
    {
        var parts = new List<string>();
        var consumed = RentChildren();
        // Consume until we hit comma, close paren, close bracket, or non-guid token
        while (!At(SyntaxKind.CloseParenToken) && !At(SyntaxKind.CloseBracketToken) &&
               !At(SyntaxKind.CommaToken) && !At(SyntaxKind.EndOfFileToken))
        {
            if (Current.Kind is SyntaxKind.NumericLiteralToken or SyntaxKind.IdentifierToken
                or SyntaxKind.MinusToken)
            {
                parts.Add(Current.Text);
                consumed.Add(EatToken());
            }
            else break;
        }
        var guidText = string.Join("", parts);
        // Create a composite node so FullWidth is correct
        return MakeNode(SyntaxKind.GuidExpression, consumed);
    }

    /// <summary>Parse a parenthesized attribute argument: (expr)</summary>
    private GreenNode ParseParenthesizedAttrArg()
    {
        var children = RentChildren();
        children.Add(Eat(SyntaxKind.OpenParenToken));

        // Consume everything until matching close paren, handling nesting
        int depth = 1;
        while (depth > 0 && !At(SyntaxKind.EndOfFileToken))
        {
            if (At(SyntaxKind.OpenParenToken)) depth++;
            if (At(SyntaxKind.CloseParenToken)) depth--;
            if (depth > 0)
            {
                children.Add(EatToken());
            }
        }

        children.Add(Eat(SyntaxKind.CloseParenToken));
        return MakeNode(SyntaxKind.ParenthesizedExpression, children);
    }
}
