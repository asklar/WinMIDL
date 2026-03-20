using Midl.Syntax;

namespace Midl.Parser;

public sealed partial class Parser
{
    /// <summary>Parse: [attrs] coclass Name { [attrs] interface IFoo; ... };</summary>
    private GreenNode ParseCoclassDeclaration(GreenNode? attributes)
    {
        var children = new List<GreenNode?>();
        if (attributes != null) children.Add(attributes);

        children.Add(Eat(SyntaxKind.CoclassKeyword));
        children.Add(EatIdentifierOrKeyword());
        children.Add(Eat(SyntaxKind.OpenBraceToken));

        while (!At(SyntaxKind.CloseBraceToken) && !At(SyntaxKind.EndOfFileToken))
        {
            var before = _position;
            children.Add(ParseCoclassMember());
            if (_position == before)
            {
                var skip = SkipToMemberStart();
                if (skip != null) children.Add(skip);
                if (_position == before) children.Add(EatToken());
            }
        }

        children.Add(Eat(SyntaxKind.CloseBraceToken));

        // Optional trailing semicolon
        if (At(SyntaxKind.SemicolonToken))
            children.Add(EatToken());

        return MakeNode(SyntaxKind.CoclassDeclaration, children);
    }

    /// <summary>Parse a coclass member: [attrs] interface Name;</summary>
    private GreenNode ParseCoclassMember()
    {
        var children = new List<GreenNode?>();

        // Optional attributes like [default]
        while (At(SyntaxKind.OpenBracketToken))
        {
            children.Add(ParseAttributeList());
        }

        // interface Name; or dispinterface Name;
        if (At(SyntaxKind.InterfaceKeyword) || At(SyntaxKind.DispinterfaceKeyword))
        {
            children.Add(EatToken()); // interface or dispinterface
            children.Add(EatIdentifierOrKeyword()); // name
            children.Add(Eat(SyntaxKind.SemicolonToken));
        }

        return MakeNode(SyntaxKind.BaseType, children);
    }

    /// <summary>Parse: [attrs] library Name { ... };</summary>
    private GreenNode ParseLibraryDeclaration(GreenNode? attributes)
    {
        var children = new List<GreenNode?>();
        if (attributes != null) children.Add(attributes);

        children.Add(Eat(SyntaxKind.LibraryKeyword));
        children.Add(EatIdentifierOrKeyword());
        children.Add(Eat(SyntaxKind.OpenBraceToken));

        while (!At(SyntaxKind.CloseBraceToken) && !At(SyntaxKind.EndOfFileToken))
        {
            var before = _position;

            if (At(SyntaxKind.ImportlibKeyword))
            {
                children.Add(ParseImportlibDirective());
            }
            else
            {
                var decl = ParseDeclaration();
                if (decl != null) children.Add(decl);
            }

            if (_position == before)
            {
                var skip = SkipToDeclarationStart();
                if (skip != null) children.Add(skip);
                if (_position == before) children.Add(EatToken());
            }
        }

        children.Add(Eat(SyntaxKind.CloseBraceToken));

        // Optional trailing semicolon
        if (At(SyntaxKind.SemicolonToken))
            children.Add(EatToken());

        return MakeNode(SyntaxKind.LibraryDeclaration, children);
    }

    /// <summary>Parse: importlib("file.tlb");</summary>
    private GreenNode ParseImportlibDirective()
    {
        var children = new List<GreenNode?>();
        children.Add(Eat(SyntaxKind.ImportlibKeyword));
        children.Add(Eat(SyntaxKind.OpenParenToken));
        children.Add(Eat(SyntaxKind.StringLiteralToken));
        children.Add(Eat(SyntaxKind.CloseParenToken));
        children.Add(Eat(SyntaxKind.SemicolonToken));
        return MakeNode(SyntaxKind.ImportlibDirective, children);
    }

    /// <summary>Parse: typedef [attrs] Type Name; or typedef struct/union/enum { ... } Name;</summary>
    private GreenNode ParseTypedefDeclaration(GreenNode? attributes)
    {
        var children = new List<GreenNode?>();
        if (attributes != null) children.Add(attributes);

        children.Add(Eat(SyntaxKind.TypedefKeyword));

        // Optional attributes after typedef (e.g., typedef [unique] IFoo* LPFOO;)
        while (At(SyntaxKind.OpenBracketToken))
        {
            children.Add(ParseAttributeList());
        }

        // Check for struct/union/enum inline definitions
        if (At(SyntaxKind.StructKeyword))
        {
            children.Add(ParseStructDeclaration(null));
        }
        else if (At(SyntaxKind.UnionKeyword))
        {
            children.Add(ParseUnionDeclaration(null));
        }
        else if (At(SyntaxKind.EnumKeyword))
        {
            children.Add(ParseEnumDeclaration(null));
        }
        else
        {
            // Simple typedef: typedef Type Name;
            children.Add(ParseTypeReference());
        }

        // The alias name
        children.Add(EatIdentifierOrKeyword());
        children.Add(Eat(SyntaxKind.SemicolonToken));

        return MakeNode(SyntaxKind.TypedefDeclaration, children);
    }

    /// <summary>Parse: cpp_quote("string")</summary>
    private GreenNode ParseCppQuoteDirective(GreenNode? attributes = null)
    {
        var children = new List<GreenNode?>();
        if (attributes != null) children.Add(attributes);
        children.Add(Eat(SyntaxKind.CppQuoteKeyword));
        children.Add(Eat(SyntaxKind.OpenParenToken));
        children.Add(Eat(SyntaxKind.StringLiteralToken));
        children.Add(Eat(SyntaxKind.CloseParenToken));
        return MakeNode(SyntaxKind.CppQuoteDirective, children);
    }

    /// <summary>Parse: [attrs] dispinterface Name { properties: methods: };</summary>
    private GreenNode ParseDispinterfaceDeclaration(GreenNode? attributes)
    {
        var children = new List<GreenNode?>();
        if (attributes != null) children.Add(attributes);

        children.Add(Eat(SyntaxKind.DispinterfaceKeyword));
        children.Add(EatIdentifierOrKeyword());
        children.Add(Eat(SyntaxKind.OpenBraceToken));

        // Parse properties: and methods: sections
        while (!At(SyntaxKind.CloseBraceToken) && !At(SyntaxKind.EndOfFileToken))
        {
            var before = _position;

            // Check for section label: "properties:" or "methods:"
            if (IsIdentifierOrKeyword(Current.Kind) && Peek(1).Kind == SyntaxKind.ColonToken)
            {
                children.Add(EatToken()); // section name
                children.Add(EatToken()); // colon
            }
            else
            {
                var member = ParseInterfaceMember();
                if (member != null) children.Add(member);
            }

            if (_position == before)
            {
                var skip = SkipToMemberStart();
                if (skip != null) children.Add(skip);
                if (_position == before) children.Add(EatToken());
            }
        }

        children.Add(Eat(SyntaxKind.CloseBraceToken));

        // Optional trailing semicolon
        if (At(SyntaxKind.SemicolonToken))
            children.Add(EatToken());

        return MakeNode(SyntaxKind.DispinterfaceDeclaration, children);
    }
}
