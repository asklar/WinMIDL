// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Syntax;

namespace Midl.Parser;

public sealed partial class Parser
{
    /// <summary>
    /// Parse: [attrs] interface Name [requires Base1, Base2] { members... }
    /// </summary>
    private GreenNode ParseInterfaceDeclaration(GreenNode? attributes)
    {
        var children = RentChildren();
        if (attributes != null) children.Add(attributes);

        children.Add(Eat(SyntaxKind.InterfaceKeyword));
        children.Add(EatIdentifierOrKeyword());

        // Optional requires clause
        if (At(SyntaxKind.RequiresKeyword) || AtContextual("requires"))
        {
            children.Add(ParseRequiresClause());
        }

        // Optional base list with colon
        if (At(SyntaxKind.ColonToken))
        {
            children.Add(ParseBaseList());
        }

        // Forward declaration: interface IFoo;
        if (At(SyntaxKind.SemicolonToken))
        {
            children.Add(EatToken());
            return MakeNode(SyntaxKind.InterfaceDeclaration, children);
        }

        children.Add(Eat(SyntaxKind.OpenBraceToken));

        // Parse interface members (methods, properties, events — no constructors)
        while (!At(SyntaxKind.CloseBraceToken) && !At(SyntaxKind.EndOfFileToken))
        {
            var before = _position;
            var member = ParseInterfaceMember();
            if (member != null) children.Add(member);
            if (_position == before)
            {
                var skip = SkipToMemberStart();
                if (skip != null) children.Add(skip);
                if (_position == before) children.Add(EatToken());
            }
        }

        children.Add(Eat(SyntaxKind.CloseBraceToken));

        // Optional trailing semicolon (classic MIDL: interface IFoo { ... };)
        if (At(SyntaxKind.SemicolonToken))
            children.Add(EatToken());

        return MakeNode(SyntaxKind.InterfaceDeclaration, children);
    }

    /// <summary>Parse: requires Type, Type, ...</summary>
    private GreenNode ParseRequiresClause()
    {
        var children = RentChildren();
        // Eat "requires" — it might be RequiresKeyword or an identifier
        children.Add(EatToken());

        children.Add(ParseTypeReference());
        while (At(SyntaxKind.CommaToken))
        {
            children.Add(EatToken()); // comma
            children.Add(ParseTypeReference());
        }

        return MakeNode(SyntaxKind.RequiresClause, children);
    }

    /// <summary>Parse a single interface member (method, property, or event).</summary>
    private GreenNode? ParseInterfaceMember()
    {
        // Leading attributes
        var attributes = RentChildren();
        while (At(SyntaxKind.OpenBracketToken))
        {
            attributes.Add(ParseAttributeList());

            // Versioned block
            if (At(SyntaxKind.OpenBraceToken))
            {
                return ParseInterfaceVersionedBlock(attributes);
            }
        }

        GreenNode? attrNode = attributes.Count > 0
            ? MakeNode(SyntaxKind.AttributeList, attributes)
            : null;

        // Event
        if (At(SyntaxKind.EventKeyword))
        {
            return ParseEventDeclaration(attrNode);
        }

        // Method or property (no constructors in interfaces)
        return ParseInterfaceMethodOrProperty(attrNode);
    }

    /// <summary>Parse a versioned block inside an interface.</summary>
    private GreenNode ParseInterfaceVersionedBlock(List<GreenNode?> attributes)
    {
        var children = RentChildren();
        foreach (var attr in attributes) children.Add(attr);

        children.Add(Eat(SyntaxKind.OpenBraceToken));

        while (!At(SyntaxKind.CloseBraceToken) && !At(SyntaxKind.EndOfFileToken))
        {
            var before = _position;
            var member = ParseInterfaceMember();
            if (member != null) children.Add(member);
            if (_position == before)
            {
                var skip = SkipToMemberStart();
                if (skip != null) children.Add(skip);
                if (_position == before) children.Add(EatToken());
            }
        }

        children.Add(Eat(SyntaxKind.CloseBraceToken));
        return MakeNode(SyntaxKind.VersionedBlock, children);
    }

    /// <summary>Parse a method or property in an interface.</summary>
    private GreenNode ParseInterfaceMethodOrProperty(GreenNode? attributes)
    {
        var returnType = ParseTypeReference();

        // Classic MIDL2 may place a calling convention between the return type and the name.
        var callingConvention = TryParseCallingConvention();

        var name = EatIdentifierOrKeyword();

        if (At(SyntaxKind.OpenParenToken))
        {
            // Method
            return ParseMethodDeclaration(attributes, null, returnType, name, callingConvention);
        }

        // Property
        return ParsePropertyDeclaration(attributes, null, returnType, name);
    }
}
