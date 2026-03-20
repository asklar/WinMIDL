using Midl.Syntax;

namespace Midl.Parser;

public sealed partial class Parser
{
    /// <summary>Parse: [attr1, attr2(args), ...]</summary>
    private GreenNode ParseAttributeList()
    {
        var children = new List<GreenNode?>();
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
        var children = new List<GreenNode?>();

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
        var children = new List<GreenNode?>();
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
        var children = new List<GreenNode?>();

        // Handle various argument forms
        switch (Current.Kind)
        {
            case SyntaxKind.StringLiteralToken:
                children.Add(EatToken());
                break;

            case SyntaxKind.NumericLiteralToken:
                children.Add(EatToken());
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
                    children.Add(ParseDottedName());
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

    /// <summary>Parse a parenthesized attribute argument: (expr)</summary>
    private GreenNode ParseParenthesizedAttrArg()
    {
        var children = new List<GreenNode?>();
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
