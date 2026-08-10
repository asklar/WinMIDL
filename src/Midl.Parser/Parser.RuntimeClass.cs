// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.CompilerServices;
using Midl.Syntax;

namespace Midl.Parser;

public sealed partial class Parser
{
    /// <summary>
    /// Parse: [attrs] [modifiers] runtimeclass Name [: Base, IFace] { members... }
    /// </summary>
    private GreenNode ParseRuntimeClassDeclaration(GreenNode? attributes, GreenNode? modifiers)
    {
        var children = RentChildren();
        if (attributes != null) children.Add(attributes);
        if (modifiers != null) children.Add(modifiers);

        children.Add(Eat(SyntaxKind.RuntimeclassKeyword));
        children.Add(EatIdentifierOrKeyword());

        // Optional base list: : Type, Type, ...
        if (At(SyntaxKind.ColonToken))
        {
            children.Add(ParseBaseList());
        }

        // Forward declaration: runtimeclass Foo;
        if (At(SyntaxKind.SemicolonToken))
        {
            children.Add(EatToken());
            return MakeNode(SyntaxKind.RuntimeClassDeclaration, children);
        }

        children.Add(Eat(SyntaxKind.OpenBraceToken));

        // Parse members
        while (!At(SyntaxKind.CloseBraceToken) && !At(SyntaxKind.EndOfFileToken))
        {
            var before = _position;
            var member = ParseClassMember();
            if (member != null) children.Add(member);
            if (_position == before)
            {
                var skip = SkipToMemberStart();
                if (skip != null) children.Add(skip);
                if (_position == before) children.Add(EatToken());
            }
        }

        children.Add(Eat(SyntaxKind.CloseBraceToken));
        return MakeNode(SyntaxKind.RuntimeClassDeclaration, children);
    }

    /// <summary>Parse a base list: : [default] Type, [default] Type, ...</summary>
    private GreenNode ParseBaseList()
    {
        var children = RentChildren();
        children.Add(Eat(SyntaxKind.ColonToken));
        children.Add(ParseBaseTypeEntry());

        while (At(SyntaxKind.CommaToken))
        {
            children.Add(EatToken()); // comma
            children.Add(ParseBaseTypeEntry());
        }

        return MakeNode(SyntaxKind.BaseList, children);
    }

    /// <summary>Parse a single base type entry, with optional leading [default] attribute.</summary>
    private GreenNode ParseBaseTypeEntry()
    {
        if (At(SyntaxKind.OpenBracketToken))
        {
            var inner = RentChildren();
            inner.Add(ParseAttributeList());
            inner.Add(ParseTypeReference());
            return MakeNode(SyntaxKind.BaseType, inner);
        }
        return ParseTypeReference();
    }

    /// <summary>Parse a single member inside a runtimeclass body.</summary>
    private GreenNode? ParseClassMember()
    {
        // Leading attributes
        var attributes = RentChildren();
        while (At(SyntaxKind.OpenBracketToken))
        {
            attributes.Add(ParseAttributeList());

            // Check if this is a versioned block: [contract(...)] { ... }
            if (At(SyntaxKind.OpenBraceToken))
            {
                return ParseVersionedBlock(attributes);
            }
        }

        GreenNode? attrNode = attributes.Count > 0
            ? MakeNode(SyntaxKind.AttributeList, attributes)
            : null;

        // Member modifiers
        var modifiers = RentChildren();
        while (IsModifier(Current.Kind))
        {
            modifiers.Add(EatToken());
        }
        GreenNode? modNode = modifiers.Count > 0
            ? MakeNode(SyntaxKind.ModifierList, modifiers)
            : null;

        // Event
        if (At(SyntaxKind.EventKeyword))
        {
            return ParseEventDeclaration(attrNode, modNode);
        }

        // Now we need to distinguish constructor, method, property
        // Constructor: Name(params); — current token is identifier and next is (
        // Method: ReturnType Name(params); — type followed by name then (
        // Property: Type Name; or Type Name { get; set; };

        return ParseMethodOrPropertyOrConstructor(attrNode, modNode);
    }

    /// <summary>Parse a versioned block: [contract(Name, Version)] { members... }</summary>
    private GreenNode ParseVersionedBlock(List<GreenNode?> attributes)
    {
        var children = RentChildren();
        foreach (var attr in attributes) children.Add(attr);

        children.Add(Eat(SyntaxKind.OpenBraceToken));

        while (!At(SyntaxKind.CloseBraceToken) && !At(SyntaxKind.EndOfFileToken))
        {
            var before = _position;
            var member = ParseClassMember();
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

    /// <summary>
    /// Distinguish and parse: constructor, method, or property.
    /// Constructor: Name(params);
    /// Method: ReturnType Name(params);
    /// Property: Type Name; or Type Name { get; set; };
    /// </summary>
    private GreenNode ParseMethodOrPropertyOrConstructor(GreenNode? attributes, GreenNode? modifiers)
    {
        // Check for constructor pattern: Identifier followed by (
        // A constructor has no return type, so if current is identifier and next is (
        // we treat it as a constructor
        if (IsIdentifierOrKeyword(Current.Kind) && Peek(1).Kind == SyntaxKind.OpenParenToken)
        {
            return ParseConstructorDeclaration(attributes, modifiers);
        }

        // Otherwise, parse the return type first
        var returnType = ParseTypeReference();

        // Then we expect a name
        if (!IsIdentifierOrKeyword(Current.Kind))
        {
            // The return type was valid (consumed by ParseTypeReference) but the
            // name token is bad. Return the return type as-is so its width is
            // preserved in the tree (needed for position tracking). The binder
            // ignores unknown member kinds. The outer loop will then call
            // SkipToMemberStart to handle the remaining bad tokens.
            return returnType;
        }

        var callingConvention = TryParseCallingConvention();

        var name = EatIdentifierOrKeyword();

        // Method: Name followed by (
        if (At(SyntaxKind.OpenParenToken))
        {
            return ParseMethodDeclaration(attributes, modifiers, returnType, name, callingConvention);
        }

        // Property: Name followed by ; or {
        return ParsePropertyDeclaration(attributes, modifiers, returnType, name);
    }

    /// <summary>Parse: [attrs] Name(params);</summary>
    private GreenNode ParseConstructorDeclaration(GreenNode? attributes, GreenNode? modifiers)
    {
        var children = RentChildren();
        if (attributes != null) children.Add(attributes);
        if (modifiers != null) children.Add(modifiers);

        children.Add(EatIdentifierOrKeyword()); // constructor name
        children.Add(ParseParameterList());
        children.Add(Eat(SyntaxKind.SemicolonToken));

        return MakeNode(SyntaxKind.ConstructorDeclaration, children);
    }

    /// <summary>Parse: [attrs] [mods] ReturnType [callconv] Name(params);</summary>
    private GreenNode ParseMethodDeclaration(GreenNode? attributes, GreenNode? modifiers,
        GreenNode returnType, SyntaxToken name, GreenNode? callingConvention = null)
    {
        var children = RentChildren();
        if (attributes != null) children.Add(attributes);
        if (modifiers != null) children.Add(modifiers);
        children.Add(returnType);
        // Kept in the tree so the width matches the source and strict mode can report it.
        if (callingConvention != null) children.Add(callingConvention);
        children.Add(name);
        children.Add(ParseParameterList());
        children.Add(Eat(SyntaxKind.SemicolonToken));

        return MakeNode(SyntaxKind.MethodDeclaration, children);
    }

    /// <summary>Parse: [attrs] [mods] Type Name; or Type Name { get; set; };</summary>
    private GreenNode ParsePropertyDeclaration(GreenNode? attributes, GreenNode? modifiers,
        GreenNode type, SyntaxToken name)
    {
        var children = RentChildren();
        if (attributes != null) children.Add(attributes);
        if (modifiers != null) children.Add(modifiers);
        children.Add(type);
        children.Add(name);

        if (At(SyntaxKind.OpenBraceToken))
        {
            children.Add(ParsePropertyAccessors());
        }

        children.Add(Eat(SyntaxKind.SemicolonToken));
        return MakeNode(SyntaxKind.PropertyDeclaration, children);
    }

    /// <summary>Parse: { get; set; }</summary>
    private GreenNode ParsePropertyAccessors()
    {
        var children = RentChildren();
        children.Add(Eat(SyntaxKind.OpenBraceToken));

        while (!At(SyntaxKind.CloseBraceToken) && !At(SyntaxKind.EndOfFileToken))
        {
            // Optional modifier before accessor: protected get; or protected set;
            if (At(SyntaxKind.ProtectedKeyword) &&
                (Peek(1).Kind is SyntaxKind.GetKeyword or SyntaxKind.SetKeyword))
            {
                children.Add(EatToken()); // protected
                children.Add(EatToken()); // get or set
                children.Add(Eat(SyntaxKind.SemicolonToken));
            }
            else if (At(SyntaxKind.GetKeyword) || At(SyntaxKind.SetKeyword))
            {
                children.Add(EatToken());
                children.Add(Eat(SyntaxKind.SemicolonToken));
            }
            else
            {
                // Error recovery: skip unexpected tokens
                children.Add(EatToken());
            }
        }

        children.Add(Eat(SyntaxKind.CloseBraceToken));
        return MakeNode(SyntaxKind.PropertyAccessors, children);
    }

    /// <summary>Parse: event DelegateType Name;</summary>
    private GreenNode ParseEventDeclaration(GreenNode? attributes, GreenNode? modifiers = null)
    {
        var children = RentChildren();
        if (attributes != null) children.Add(attributes);
        if (modifiers != null) children.Add(modifiers);

        children.Add(Eat(SyntaxKind.EventKeyword));
        children.Add(ParseTypeReference());
        children.Add(EatIdentifierOrKeyword());
        children.Add(Eat(SyntaxKind.SemicolonToken));

        return MakeNode(SyntaxKind.EventDeclaration, children);
    }

    /// <summary>Parse a parameter list: (param1, param2, ...)</summary>
    private GreenNode ParseParameterList()
    {
        var children = RentChildren();
        children.Add(Eat(SyntaxKind.OpenParenToken));

        if (!At(SyntaxKind.CloseParenToken))
        {
            children.Add(ParseParameter());
            while (At(SyntaxKind.CommaToken))
            {
                children.Add(EatToken()); // comma
                children.Add(ParseParameter());
            }
        }

        children.Add(Eat(SyntaxKind.CloseParenToken));
        return MakeNode(SyntaxKind.ParameterList, children);
    }

    /// <summary>
    /// Parse a single parameter: [attrs] [out | ref [const] | const] Type name
    ///
    /// Two unrelated uses of <c>const</c>:
    ///   MIDL3  <c>ref const T p</c> — parameter direction modifier
    ///   MIDL2  <c>const T p</c>     — C-style type qualifier
    ///
    /// Both are parsed here; the binder reports the classic form in strict mode. Parsing
    /// it unconditionally keeps a single misplaced qualifier from cascading into dozens of
    /// "Expected 'identifier'" errors that bury every real diagnostic in the file.
    /// </summary>
    private GreenNode ParseParameter()
    {
        var children = RentChildren();

        // Optional attributes
        while (At(SyntaxKind.OpenBracketToken))
        {
            children.Add(ParseAttributeList());
        }

        // Optional out/ref/ref const
        if (At(SyntaxKind.OutKeyword))
        {
            children.Add(EatToken());
        }
        else if (At(SyntaxKind.RefKeyword))
        {
            children.Add(EatToken());
            // ref const Type name
            if (At(SyntaxKind.ConstKeyword))
            {
                children.Add(EatToken());
            }
        }
        else if (At(SyntaxKind.ConstKeyword))
        {
            // Classic MIDL2 qualifier with no direction modifier in front of it.
            children.Add(EatToken());
        }

        children.Add(ParseTypeReference());
        children.Add(EatIdentifierOrKeyword());

        return MakeNode(SyntaxKind.Parameter, children);
    }

    /// <summary>
    /// Classic MIDL2 / C calling-convention keywords that may appear between a method's
    /// return type and its name (<c>error_status_t __stdcall logMessage(...)</c>).
    /// MIDL3 has no such modifier; the binder reports it in strict mode.
    /// </summary>
    private static bool IsCallingConventionText(string text) => text is
        "__stdcall" or "__cdecl" or "__fastcall" or "__thiscall" or "__vectorcall" or
        "STDMETHODCALLTYPE" or "STDAPICALLTYPE" or "STDMETHODVCALLTYPE" or
        "WINAPI" or "APIENTRY" or "CALLBACK";

    /// <summary>
    /// Consumes a calling-convention token when one sits between the return type and the
    /// method name. Requires a following identifier so a method genuinely *named*
    /// <c>__stdcall</c> still parses as a method name rather than a modifier.
    /// </summary>
    private GreenNode? TryParseCallingConvention()
    {
        if (!IsIdentifierOrKeyword(Current.Kind)) return null;
        if (!IsCallingConventionText(Current.Text)) return null;
        if (!IsIdentifierOrKeyword(Peek(1).Kind)) return null;

        // Re-kind the token so downstream code recognizes it structurally. Text and
        // trivia are carried over, so the tree's widths still match the source.
        var token = EatToken();
        return new SyntaxToken(SyntaxKind.CallingConventionToken, token.Text, token.Value,
            token.LeadingTrivia, token.TrailingTrivia);
    }

    /// <summary>Check if kind is an identifier or keyword usable as a name.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsIdentifierOrKeyword(SyntaxKind kind)
    {
        return kind == SyntaxKind.IdentifierToken || IsTypeKeyword(kind) || IsNameableKeyword(kind);
    }
}
