// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Syntax;

namespace Midl.Parser;

public sealed partial class Parser
{
    /// <summary>
    /// Parse a type reference: simple name, qualified name (A.B.C),
    /// generic (IVector&lt;String&gt;), array (UInt8[]), nullable (IReference&lt;Int32&gt;).
    /// </summary>
    private GreenNode ParseTypeReference()
    {
        var type = ParseBaseTypeReference();

        // Check for pointer suffix: *, **
        while (At(SyntaxKind.AsteriskToken))
        {
            var star = EatToken();
            type = MakeNode(SyntaxKind.PointerType, type, star);
        }

        // Check for array suffix: []
        if (At(SyntaxKind.OpenBracketToken) && Peek(1).Kind == SyntaxKind.CloseBracketToken)
        {
            var open = EatToken();
            var close = EatToken();
            type = MakeNode(SyntaxKind.ArrayType, type, open, close);
        }

        return type;
    }

    /// <summary>Parse the base type reference (before any array suffix).</summary>
    private GreenNode ParseBaseTypeReference()
    {
        // Start with simple name (identifier or keyword)
        if (!IsIdentifierOrKeyword(Current.Kind))
        {
            // Error: expected type name
            return MakeNode(SyntaxKind.TypeReference, CreateMissingToken(SyntaxKind.IdentifierToken));
        }

        var firstName = EatToken();

        // Check for generic: Name<...>
        if (At(SyntaxKind.LessThanToken))
        {
            return ParseGenericName(firstName);
        }

        // Check for dotted name: A.B.C or A.B<C>
        if (At(SyntaxKind.DotToken))
        {
            return ParseQualifiedTypeReference(firstName);
        }

        // Simple type reference
        return MakeNode(SyntaxKind.TypeReference, firstName);
    }

    /// <summary>Parse a qualified type reference: A.B.C or A.B.C&lt;T&gt;</summary>
    private GreenNode ParseQualifiedTypeReference(SyntaxToken firstName)
    {
        var parts = RentChildren();
        parts.Add(firstName);

        while (At(SyntaxKind.DotToken))
        {
            parts.Add(EatToken()); // dot

            if (!IsIdentifierOrKeyword(Current.Kind))
            {
                parts.Add(CreateMissingToken(SyntaxKind.IdentifierToken));
                break;
            }

            var name = EatToken();

            // Check if the last segment is generic
            if (At(SyntaxKind.LessThanToken))
            {
                parts.Add(ParseGenericName(name));
                break; // generic ends the qualified name
            }
            else
            {
                parts.Add(name);
            }
        }

        if (parts.Count == 1)
        {
            var result = MakeNode(SyntaxKind.TypeReference, parts[0]);
            _childListPool.Push(parts);
            return result;
        }

        return MakeNode(SyntaxKind.QualifiedName, parts);
    }

    /// <summary>Parse a generic name: Name&lt;T1, T2, ...&gt;</summary>
    private GreenNode ParseGenericName(SyntaxToken name)
    {
        var children = RentChildren();
        children.Add(name);
        children.Add(ParseTypeArgumentList());
        return MakeNode(SyntaxKind.GenericName, children);
    }

    /// <summary>Parse a type argument list: &lt;Type, Type, ...&gt;</summary>
    private GreenNode ParseTypeArgumentList()
    {
        var children = RentChildren();
        children.Add(Eat(SyntaxKind.LessThanToken));

        children.Add(ParseTypeReference());
        while (At(SyntaxKind.CommaToken))
        {
            children.Add(EatToken()); // comma
            children.Add(ParseTypeReference());
        }

        // Handle nested generics: >> should be split into two > tokens
        if (At(SyntaxKind.GreaterThanGreaterThanToken))
        {
            var gg = EatToken();
            // Create a synthetic > for closing this generic argument list
            var syntheticGt = new SyntaxToken(SyntaxKind.GreaterThanToken, ">",
                leadingTrivia: gg.LeadingTrivia);
            children.Add(syntheticGt);
            // Push back a second > for the outer generic to consume
            _pendingToken = new SyntaxToken(SyntaxKind.GreaterThanToken, ">",
                trailingTrivia: gg.TrailingTrivia);
        }
        else
        {
            children.Add(Eat(SyntaxKind.GreaterThanToken));
        }

        return MakeNode(SyntaxKind.TypeArgumentList, children);
    }
}
