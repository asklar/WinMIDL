using Midl.Syntax;

namespace Midl.Parser;

public sealed partial class Parser
{
    /// <summary>Parse an expression (for enum initializers and attribute arguments).</summary>
    private GreenNode ParseExpression()
    {
        return ParseBinaryExpression(0);
    }

    /// <summary>Parse a binary expression with precedence climbing.</summary>
    private GreenNode ParseBinaryExpression(int minPrecedence)
    {
        var left = ParseUnaryExpression();

        while (true)
        {
            int prec = GetBinaryPrecedence(Current.Kind);
            if (prec <= minPrecedence) break;

            var op = EatToken();
            var right = ParseBinaryExpression(prec);
            left = MakeNode(SyntaxKind.BinaryExpression, left, op, right);
        }

        return left;
    }

    /// <summary>Parse a unary expression: +, -, ~, !, or primary.</summary>
    private GreenNode ParseUnaryExpression()
    {
        if (AtAny(SyntaxKind.PlusToken, SyntaxKind.MinusToken,
                  SyntaxKind.TildeToken, SyntaxKind.ExclamationToken))
        {
            var op = EatToken();
            var operand = ParseUnaryExpression();
            return MakeNode(SyntaxKind.UnaryExpression, op, operand);
        }
        return ParsePrimaryExpression();
    }

    /// <summary>Parse a primary expression: literal, identifier, parenthesized.</summary>
    private GreenNode ParsePrimaryExpression()
    {
        switch (Current.Kind)
        {
            case SyntaxKind.NumericLiteralToken:
                return MakeNode(SyntaxKind.NumericLiteralExpression, EatToken());

            case SyntaxKind.StringLiteralToken:
                return MakeNode(SyntaxKind.StringLiteralExpression, EatToken());

            case SyntaxKind.GuidLiteralToken:
                return MakeNode(SyntaxKind.GuidExpression, EatToken());

            case SyntaxKind.OpenParenToken:
            {
                var open = EatToken();
                var expr = ParseExpression();
                var close = Eat(SyntaxKind.CloseParenToken);
                return MakeNode(SyntaxKind.ParenthesizedExpression, open, expr, close);
            }

            default:
                if (IsIdentifierOrKeyword(Current.Kind))
                {
                    // Could be a dotted name like MyEnum.Value
                    var name = ParseDottedName();
                    return MakeNode(SyntaxKind.IdentifierExpression, name);
                }
                // Error: unexpected token in expression, return missing token
                return MakeNode(SyntaxKind.IdentifierExpression, CreateMissingToken(SyntaxKind.IdentifierToken));
        }
    }

    /// <summary>Get the precedence of a binary operator (higher = binds tighter).</summary>
    private static int GetBinaryPrecedence(SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.PipePipeToken => 1,
            SyntaxKind.AmpersandAmpersandToken => 2,
            SyntaxKind.PipeToken => 3,
            SyntaxKind.CaretToken => 4,
            SyntaxKind.AmpersandToken => 5,
            SyntaxKind.EqualsEqualsToken or SyntaxKind.ExclamationEqualsToken => 6,
            SyntaxKind.LessThanToken or SyntaxKind.GreaterThanToken
                or SyntaxKind.LessThanEqualsToken or SyntaxKind.GreaterThanEqualsToken => 7,
            SyntaxKind.LessThanLessThanToken or SyntaxKind.GreaterThanGreaterThanToken => 8,
            SyntaxKind.PlusToken or SyntaxKind.MinusToken => 9,
            SyntaxKind.AsteriskToken or SyntaxKind.SlashToken or SyntaxKind.PercentToken => 10,
            _ => 0
        };
    }
}
