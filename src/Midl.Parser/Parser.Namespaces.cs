// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.CompilerServices;
using Midl.Syntax;

namespace Midl.Parser;

public sealed partial class Parser
{
    /// <summary>Parse: namespace A.B.C { declarations... }</summary>
    private GreenNode ParseNamespaceDeclaration(GreenNode? attributes)
    {
        var children = RentChildren();
        if (attributes != null) children.Add(attributes);

        children.Add(Eat(SyntaxKind.NamespaceKeyword));
        children.Add(ParseDottedName());
        children.Add(Eat(SyntaxKind.OpenBraceToken));

        while (!At(SyntaxKind.CloseBraceToken) && !At(SyntaxKind.EndOfFileToken))
        {
            var before = _position;
            var decl = ParseDeclaration();
            if (decl != null) children.Add(decl);
            if (_position == before)
            {
                var skip = SkipToDeclarationStart();
                if (skip != null) children.Add(skip);
                if (_position == before) children.Add(EatToken());
            }
        }

        children.Add(Eat(SyntaxKind.CloseBraceToken));
        return MakeNode(SyntaxKind.NamespaceDeclaration, children);
    }

    /// <summary>Parse a dotted name like A.B.C (identifier separated by dots).</summary>
    private GreenNode ParseDottedName()
    {
        var parts = RentChildren();
        parts.Add(EatIdentifierOrKeyword());

        while (At(SyntaxKind.DotToken))
        {
            parts.Add(EatToken()); // dot
            parts.Add(EatIdentifierOrKeyword());
        }

        if (parts.Count == 1)
        {
            var single = parts[0]!;
            _childListPool.Push(parts);
            return single;
        }

        return MakeNode(SyntaxKind.DottedName, parts);
    }

    /// <summary>
    /// Eat an identifier token, or if the current token is a keyword that can also be
    /// used as a name (like type keywords), consume it as an identifier.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxToken EatIdentifierOrKeyword()
    {
        if (At(SyntaxKind.IdentifierToken) || IsTypeKeyword(Current.Kind) || IsNameableKeyword(Current.Kind))
        {
            return EatToken();
        }
        return Eat(SyntaxKind.IdentifierToken);
    }

    /// <summary>Check if a kind is a built-in type keyword that can appear as a type name.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsTypeKeyword(SyntaxKind kind)
    {
        return kind is SyntaxKind.BooleanKeyword
            or SyntaxKind.CharKeyword
            or SyntaxKind.Int16Keyword
            or SyntaxKind.Int32Keyword
            or SyntaxKind.Int64Keyword
            or SyntaxKind.UInt8Keyword
            or SyntaxKind.UInt16Keyword
            or SyntaxKind.UInt32Keyword
            or SyntaxKind.UInt64Keyword
            or SyntaxKind.SingleKeyword
            or SyntaxKind.DoubleKeyword
            or SyntaxKind.StringKeyword
            or SyntaxKind.GuidKeyword
            or SyntaxKind.ObjectKeyword
            or SyntaxKind.VoidKeyword
            // Classic MIDL C types
            or SyntaxKind.IntKeyword
            or SyntaxKind.ShortKeyword
            or SyntaxKind.LongKeyword
            or SyntaxKind.FloatKeyword
            or SyntaxKind.ByteKeyword
            or SyntaxKind.HyperKeyword
            or SyntaxKind.UnsignedKeyword
            or SyntaxKind.SignedKeyword
            or SyntaxKind.SmallKeyword
            or SyntaxKind.WcharTKeyword
            or SyntaxKind.HandleTKeyword
            or SyntaxKind.ErrorStatusTKeyword
            or SyntaxKind.HResultKeyword;
    }

    /// <summary>Keywords that can also appear as identifiers in name positions.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsNameableKeyword(SyntaxKind kind)
    {
        return kind is SyntaxKind.EventKeyword
            or SyntaxKind.GetKeyword
            or SyntaxKind.SetKeyword
            or SyntaxKind.RequiresKeyword
            or SyntaxKind.ContractKeyword
            // Classic MIDL keywords usable as names (e.g. in attributes)
            or SyntaxKind.InKeyword
            or SyntaxKind.OutKeyword
            or SyntaxKind.RetvalKeyword
            or SyntaxKind.DefaultKeyword
            or SyntaxKind.CaseKeyword
            // 'static' can appear as both a member modifier and an attribute name
            or SyntaxKind.StaticKeyword;
    }
}
