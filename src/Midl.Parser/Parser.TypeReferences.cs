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

        // Classic MIDL/C spells some integer types with more than one word: unsigned long,
        // long long, unsigned __int64. Fold them into a single type reference in both modes.
        // Parsing them is not accepting them — strict mode still reports the whole type as a
        // classic construct — but leaving them unparsed turned one bad type into a cascade of
        // errors that buried everything real in the file.
        var folded = TryParseMultiWordCType(firstName);
        if (folded != null) return folded;

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

    /// <summary>
    /// Folds a multi-word C integer type into one <see cref="SyntaxKind.TypeReference"/>, or
    /// returns null when the tokens do not spell one.
    ///
    /// Only the words that genuinely form part of the type are consumed, so the parameter name
    /// in <c>unsigned long windowHandle</c> stays a name.
    /// </summary>
    private GreenNode? TryParseMultiWordCType(SyntaxToken firstName)
    {
        // "unsigned"/"signed" followed by the integer type they qualify. Bare "unsigned" is a
        // type on its own and is left to the normal path.
        if (firstName.Kind is SyntaxKind.UnsignedKeyword or SyntaxKind.SignedKeyword
            && IsCIntegerWord(Current))
        {
            var parts = RentChildren();
            parts.Add(firstName);

            bool wasLong = Current.Kind == SyntaxKind.LongKeyword;
            parts.Add(EatToken());

            // "unsigned long long"
            if (wasLong && Current.Kind == SyntaxKind.LongKeyword)
                parts.Add(EatToken());

            return MakeNode(SyntaxKind.TypeReference, parts);
        }

        // "long long"
        if (firstName.Kind == SyntaxKind.LongKeyword && Current.Kind == SyntaxKind.LongKeyword)
        {
            var parts = RentChildren();
            parts.Add(firstName);
            parts.Add(EatToken());
            return MakeNode(SyntaxKind.TypeReference, parts);
        }

        return null;
    }

    /// <summary>The integer words that may follow <c>unsigned</c> or <c>signed</c>.</summary>
    private static bool IsCIntegerWord(SyntaxToken token) =>
        token.Kind is SyntaxKind.CharKeyword
                   or SyntaxKind.ShortKeyword
                   or SyntaxKind.IntKeyword
                   or SyntaxKind.LongKeyword
                   or SyntaxKind.HyperKeyword
                   or SyntaxKind.SmallKeyword
        || (token.Kind == SyntaxKind.IdentifierToken && IsIntNWord(token.Text));

    /// <summary>The <c>__intN</c> family, which lexes as an identifier.</summary>
    private static bool IsIntNWord(string text) =>
        text is "__int8" or "__int16" or "__int32" or "__int64" or "__int3264";

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
