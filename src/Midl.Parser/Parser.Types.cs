// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Syntax;

namespace Midl.Parser;

public sealed partial class Parser
{
    /// <summary>Parse: [attrs] struct Name { fields... } or forward declaration: struct Name;</summary>
    private GreenNode ParseStructDeclaration(GreenNode? attributes)
    {
        var children = RentChildren();
        if (attributes != null) children.Add(attributes);

        children.Add(Eat(SyntaxKind.StructKeyword));
        children.Add(EatIdentifierOrKeyword());

        // Forward declaration: struct Name;
        if (At(SyntaxKind.SemicolonToken))
        {
            children.Add(EatToken());
            return MakeNode(SyntaxKind.StructDeclaration, children);
        }

        children.Add(Eat(SyntaxKind.OpenBraceToken));

        while (!At(SyntaxKind.CloseBraceToken) && !At(SyntaxKind.EndOfFileToken))
        {
            var before = _position;
            children.Add(ParseFieldDeclaration());
            if (_position == before)
            {
                var skip = SkipToMemberStart();
                if (skip != null) children.Add(skip);
                if (_position == before) children.Add(EatToken());
            }
        }

        children.Add(Eat(SyntaxKind.CloseBraceToken));

        // Optional trailing semicolon (classic MIDL)
        if (At(SyntaxKind.SemicolonToken))
            children.Add(EatToken());

        return MakeNode(SyntaxKind.StructDeclaration, children);
    }

    /// <summary>Parse a struct field: Type Name;</summary>
    private GreenNode ParseFieldDeclaration()
    {
        var children = RentChildren();

        // Optional attributes
        while (At(SyntaxKind.OpenBracketToken))
        {
            children.Add(ParseAttributeList());
        }

        children.Add(ParseTypeReference());
        children.Add(EatIdentifierOrKeyword());
        children.Add(Eat(SyntaxKind.SemicolonToken));

        return MakeNode(SyntaxKind.FieldDeclaration, children);
    }

    /// <summary>Parse: [attrs] enum Name { member, member, ... } or forward declaration: enum Name;</summary>
    private GreenNode ParseEnumDeclaration(GreenNode? attributes)
    {
        var children = RentChildren();
        if (attributes != null) children.Add(attributes);

        children.Add(Eat(SyntaxKind.EnumKeyword));
        children.Add(EatIdentifierOrKeyword());

        // Forward declaration: enum Name;
        if (At(SyntaxKind.SemicolonToken))
        {
            children.Add(EatToken());
            return MakeNode(SyntaxKind.EnumDeclaration, children);
        }

        children.Add(Eat(SyntaxKind.OpenBraceToken));

        while (!At(SyntaxKind.CloseBraceToken) && !At(SyntaxKind.EndOfFileToken))
        {
            var before = _position;
            children.Add(ParseEnumMember());
            if (_position == before)
            {
                var skip = SkipToMemberStart();
                if (skip != null) children.Add(skip);
                if (_position == before) children.Add(EatToken());
            }
        }

        children.Add(Eat(SyntaxKind.CloseBraceToken));

        // Optional trailing semicolon (classic MIDL)
        if (At(SyntaxKind.SemicolonToken))
            children.Add(EatToken());

        return MakeNode(SyntaxKind.EnumDeclaration, children);
    }

    /// <summary>Parse an enum member: [attrs] Name [= expr] [,]</summary>
    private GreenNode ParseEnumMember()
    {
        var children = RentChildren();

        // Optional attributes
        while (At(SyntaxKind.OpenBracketToken))
        {
            children.Add(ParseAttributeList());
        }

        children.Add(EatIdentifierOrKeyword());

        // Optional initializer
        if (At(SyntaxKind.EqualsToken))
        {
            children.Add(EatToken()); // =
            children.Add(ParseExpression());
        }

        // Optional trailing comma
        if (At(SyntaxKind.CommaToken))
        {
            children.Add(EatToken());
        }

        return MakeNode(SyntaxKind.EnumMemberDeclaration, children);
    }

    /// <summary>Parse: [attrs] delegate ReturnType Name(params);</summary>
    private GreenNode ParseDelegateDeclaration(GreenNode? attributes)
    {
        var children = RentChildren();
        if (attributes != null) children.Add(attributes);

        children.Add(Eat(SyntaxKind.DelegateKeyword));
        children.Add(ParseTypeReference());
        children.Add(EatIdentifierOrKeyword());
        children.Add(ParseParameterList());
        children.Add(Eat(SyntaxKind.SemicolonToken));

        return MakeNode(SyntaxKind.DelegateDeclaration, children);
    }

    /// <summary>Parse: [attrs] apicontract Name { }</summary>
    private GreenNode ParseApiContractDeclaration(GreenNode? attributes)
    {
        var children = RentChildren();
        if (attributes != null) children.Add(attributes);

        children.Add(Eat(SyntaxKind.ApicontractKeyword));
        children.Add(EatIdentifierOrKeyword());
        children.Add(Eat(SyntaxKind.OpenBraceToken));

        // API contracts typically have an empty body, but allow recovery
        while (!At(SyntaxKind.CloseBraceToken) && !At(SyntaxKind.EndOfFileToken))
        {
            children.Add(EatToken());
        }

        children.Add(Eat(SyntaxKind.CloseBraceToken));
        return MakeNode(SyntaxKind.ApiContractDeclaration, children);
    }

    /// <summary>Parse: [attrs] attribute Name { fields... }; or forward declaration: attribute Name;</summary>
    private GreenNode ParseAttributeDeclaration(GreenNode? attributes)
    {
        var children = RentChildren();
        if (attributes != null) children.Add(attributes);

        children.Add(Eat(SyntaxKind.AttributeKeyword));
        children.Add(EatIdentifierOrKeyword());

        // Forward declaration: attribute Name;
        if (At(SyntaxKind.SemicolonToken))
        {
            children.Add(EatToken());
            return MakeNode(SyntaxKind.AttributeDeclaration, children);
        }

        children.Add(Eat(SyntaxKind.OpenBraceToken));

        while (!At(SyntaxKind.CloseBraceToken) && !At(SyntaxKind.EndOfFileToken))
        {
            var before = _position;
            children.Add(ParseFieldDeclaration());
            if (_position == before)
            {
                var skip = SkipToMemberStart();
                if (skip != null) children.Add(skip);
                if (_position == before) children.Add(EatToken());
            }
        }

        children.Add(Eat(SyntaxKind.CloseBraceToken));

        // Optional trailing semicolon (classic MIDL)
        if (At(SyntaxKind.SemicolonToken))
            children.Add(EatToken());

        return MakeNode(SyntaxKind.AttributeDeclaration, children);
    }

    /// <summary>Parse: [attrs] union Name { arms... }; or union Name switch(type) { ... };</summary>
    private GreenNode ParseUnionDeclaration(GreenNode? attributes)
    {
        var children = RentChildren();
        if (attributes != null) children.Add(attributes);

        children.Add(Eat(SyntaxKind.UnionKeyword));
        children.Add(EatIdentifierOrKeyword());

        // Encapsulated union: union Name switch(type) { ... }
        if (At(SyntaxKind.SwitchKeyword))
        {
            children.Add(ParseSwitchBody());
        }
        else
        {
            children.Add(Eat(SyntaxKind.OpenBraceToken));

            while (!At(SyntaxKind.CloseBraceToken) && !At(SyntaxKind.EndOfFileToken))
            {
                var before = _position;
                children.Add(ParseUnionArm());
                if (_position == before)
                {
                    var skip = SkipToMemberStart();
                    if (skip != null) children.Add(skip);
                    if (_position == before) children.Add(EatToken());
                }
            }

            children.Add(Eat(SyntaxKind.CloseBraceToken));
        }

        // Optional trailing semicolon
        if (At(SyntaxKind.SemicolonToken))
            children.Add(EatToken());

        return MakeNode(SyntaxKind.UnionDeclaration, children);
    }

    /// <summary>Parse a union arm: [case(N)] type name; or [default] type name;</summary>
    private GreenNode ParseUnionArm()
    {
        var children = RentChildren();

        // Optional attributes like [case(1)] or [default]
        while (At(SyntaxKind.OpenBracketToken))
        {
            children.Add(ParseAttributeList());
        }

        children.Add(ParseTypeReference());
        children.Add(EatIdentifierOrKeyword());
        children.Add(Eat(SyntaxKind.SemicolonToken));

        return MakeNode(SyntaxKind.UnionArm, children);
    }

    /// <summary>Parse: switch(type varName) { case N: type name; ... default: type name; }</summary>
    private GreenNode ParseSwitchBody()
    {
        var children = RentChildren();
        children.Add(Eat(SyntaxKind.SwitchKeyword));
        children.Add(Eat(SyntaxKind.OpenParenToken));
        children.Add(ParseTypeReference());
        children.Add(EatIdentifierOrKeyword());
        children.Add(Eat(SyntaxKind.CloseParenToken));
        children.Add(Eat(SyntaxKind.OpenBraceToken));

        while (!At(SyntaxKind.CloseBraceToken) && !At(SyntaxKind.EndOfFileToken))
        {
            var before = _position;

            if (At(SyntaxKind.CaseKeyword))
            {
                var caseChildren = RentChildren();
                caseChildren.Add(EatToken()); // case
                caseChildren.Add(ParseExpression());
                caseChildren.Add(Eat(SyntaxKind.ColonToken));
                caseChildren.Add(ParseTypeReference());
                caseChildren.Add(EatIdentifierOrKeyword());
                caseChildren.Add(Eat(SyntaxKind.SemicolonToken));
                children.Add(MakeNode(SyntaxKind.CaseLabel, caseChildren));
            }
            else if (At(SyntaxKind.DefaultKeyword))
            {
                var defChildren = RentChildren();
                defChildren.Add(EatToken()); // default
                defChildren.Add(Eat(SyntaxKind.ColonToken));
                defChildren.Add(ParseTypeReference());
                defChildren.Add(EatIdentifierOrKeyword());
                defChildren.Add(Eat(SyntaxKind.SemicolonToken));
                children.Add(MakeNode(SyntaxKind.DefaultLabel, defChildren));
            }

            if (_position == before)
            {
                var skip = SkipToMemberStart();
                if (skip != null) children.Add(skip);
                if (_position == before) children.Add(EatToken());
            }
        }

        children.Add(Eat(SyntaxKind.CloseBraceToken));
        return MakeNode(SyntaxKind.SwitchBody, children);
    }
}
