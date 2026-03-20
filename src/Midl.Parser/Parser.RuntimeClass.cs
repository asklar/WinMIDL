using Midl.Syntax;

namespace Midl.Parser;

public sealed partial class Parser
{
    /// <summary>
    /// Parse: [attrs] [modifiers] runtimeclass Name [: Base, IFace] { members... }
    /// </summary>
    private GreenNode ParseRuntimeClassDeclaration(GreenNode? attributes, GreenNode? modifiers)
    {
        var children = new List<GreenNode?>();
        if (attributes != null) children.Add(attributes);
        if (modifiers != null) children.Add(modifiers);

        children.Add(Eat(SyntaxKind.RuntimeclassKeyword));
        children.Add(EatIdentifierOrKeyword());

        // Optional base list: : Type, Type, ...
        if (At(SyntaxKind.ColonToken))
        {
            children.Add(ParseBaseList());
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

    /// <summary>Parse a base list: : Type, Type, ...</summary>
    private GreenNode ParseBaseList()
    {
        var children = new List<GreenNode?>();
        children.Add(Eat(SyntaxKind.ColonToken));
        children.Add(ParseTypeReference());

        while (At(SyntaxKind.CommaToken))
        {
            children.Add(EatToken()); // comma
            children.Add(ParseTypeReference());
        }

        return MakeNode(SyntaxKind.BaseList, children);
    }

    /// <summary>Parse a single member inside a runtimeclass body.</summary>
    private GreenNode? ParseClassMember()
    {
        // Leading attributes
        var attributes = new List<GreenNode?>();
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
        var modifiers = new List<GreenNode?>();
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
            return ParseEventDeclaration(attrNode);
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
        var children = new List<GreenNode?>();
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
            // Error recovery: maybe this is a constructor after all, or malformed
            var parts = new List<GreenNode?>();
            if (attributes != null) parts.Add(attributes);
            if (modifiers != null) parts.Add(modifiers);
            parts.Add(returnType);
            return MakeNode(SyntaxKind.SkippedTokens, parts);
        }

        var name = EatIdentifierOrKeyword();

        // Method: Name followed by (
        if (At(SyntaxKind.OpenParenToken))
        {
            return ParseMethodDeclaration(attributes, modifiers, returnType, name);
        }

        // Property: Name followed by ; or {
        return ParsePropertyDeclaration(attributes, modifiers, returnType, name);
    }

    /// <summary>Parse: [attrs] Name(params);</summary>
    private GreenNode ParseConstructorDeclaration(GreenNode? attributes, GreenNode? modifiers)
    {
        var children = new List<GreenNode?>();
        if (attributes != null) children.Add(attributes);
        if (modifiers != null) children.Add(modifiers);

        children.Add(EatIdentifierOrKeyword()); // constructor name
        children.Add(ParseParameterList());
        children.Add(Eat(SyntaxKind.SemicolonToken));

        return MakeNode(SyntaxKind.ConstructorDeclaration, children);
    }

    /// <summary>Parse: [attrs] [mods] ReturnType Name(params);</summary>
    private GreenNode ParseMethodDeclaration(GreenNode? attributes, GreenNode? modifiers,
        GreenNode returnType, SyntaxToken name)
    {
        var children = new List<GreenNode?>();
        if (attributes != null) children.Add(attributes);
        if (modifiers != null) children.Add(modifiers);
        children.Add(returnType);
        children.Add(name);
        children.Add(ParseParameterList());
        children.Add(Eat(SyntaxKind.SemicolonToken));

        return MakeNode(SyntaxKind.MethodDeclaration, children);
    }

    /// <summary>Parse: [attrs] [mods] Type Name; or Type Name { get; set; };</summary>
    private GreenNode ParsePropertyDeclaration(GreenNode? attributes, GreenNode? modifiers,
        GreenNode type, SyntaxToken name)
    {
        var children = new List<GreenNode?>();
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
        var children = new List<GreenNode?>();
        children.Add(Eat(SyntaxKind.OpenBraceToken));

        while (!At(SyntaxKind.CloseBraceToken) && !At(SyntaxKind.EndOfFileToken))
        {
            if (At(SyntaxKind.GetKeyword) || At(SyntaxKind.SetKeyword))
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
    private GreenNode ParseEventDeclaration(GreenNode? attributes)
    {
        var children = new List<GreenNode?>();
        if (attributes != null) children.Add(attributes);

        children.Add(Eat(SyntaxKind.EventKeyword));
        children.Add(ParseTypeReference());
        children.Add(EatIdentifierOrKeyword());
        children.Add(Eat(SyntaxKind.SemicolonToken));

        return MakeNode(SyntaxKind.EventDeclaration, children);
    }

    /// <summary>Parse a parameter list: (param1, param2, ...)</summary>
    private GreenNode ParseParameterList()
    {
        var children = new List<GreenNode?>();
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

    /// <summary>Parse a single parameter: [attrs] [out/ref] Type name</summary>
    private GreenNode ParseParameter()
    {
        var children = new List<GreenNode?>();

        // Optional attributes
        while (At(SyntaxKind.OpenBracketToken))
        {
            children.Add(ParseAttributeList());
        }

        // Optional out/ref
        if (AtAny(SyntaxKind.OutKeyword, SyntaxKind.RefKeyword))
        {
            children.Add(EatToken());
        }

        children.Add(ParseTypeReference());
        children.Add(EatIdentifierOrKeyword());

        return MakeNode(SyntaxKind.Parameter, children);
    }

    /// <summary>Check if kind is an identifier or keyword usable as a name.</summary>
    private static bool IsIdentifierOrKeyword(SyntaxKind kind)
    {
        return kind == SyntaxKind.IdentifierToken || IsTypeKeyword(kind) || IsNameableKeyword(kind);
    }
}
