// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#if WITH_COMPAT
using Midl.Syntax;

namespace Midl.Parser;

public sealed partial class Parser
{
    /// <summary>Parse: [attrs] coclass Name { [attrs] interface IFoo; ... };</summary>
    private GreenNode ParseCoclassDeclaration(GreenNode? attributes)
    {
        var children = RentChildren();
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
        var children = RentChildren();

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
        var children = RentChildren();
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
        var children = RentChildren();
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
        var children = RentChildren();
        if (attributes != null) children.Add(attributes);

        children.Add(Eat(SyntaxKind.TypedefKeyword));

        // Optional attributes after typedef (e.g., typedef [unique] IFoo* LPFOO;)
        while (At(SyntaxKind.OpenBracketToken))
        {
            children.Add(ParseAttributeList());
        }

        // Check for struct/union/enum inline definitions
        // For typedef: the inner type may be a forward declaration OR a full definition,
        // followed by an alias name and semicolon.
        if (At(SyntaxKind.StructKeyword) || At(SyntaxKind.EnumKeyword) || At(SyntaxKind.UnionKeyword)
            || At(SyntaxKind.InterfaceKeyword))
        {
            // Peek ahead to determine the pattern:
            // typedef enum Name { ... } AliasName;  → full definition + alias
            // typedef enum Name AliasName;           → forward decl + alias (no body)
            // typedef struct Name AliasName;          → forward decl + alias
            // typedef struct { ... } AliasName;       → anonymous; the alias is the only name
            var keyword = EatToken(); // enum/struct/union/interface keyword
            children.Add(keyword);

            // The tag is optional. "typedef struct {" is anonymous, and eating the brace as a
            // name is what turned one declaration into a cascade of recovery errors.
            if (!At(SyntaxKind.OpenBraceToken) && IsIdentifierOrKeyword(Current.Kind))
            {
                var typeName = EatIdentifierOrKeyword(); // the type name
                children.Add(typeName);
            }

            if (At(SyntaxKind.OpenBraceToken))
            {
                // Full definition: parse the body { ... }
                children.Add(EatToken()); // {
                
                if (keyword.Kind == SyntaxKind.EnumKeyword)
                {
                    while (!At(SyntaxKind.CloseBraceToken) && !At(SyntaxKind.EndOfFileToken))
                    {
                        var before = _position;
                        children.Add(ParseEnumMember());
                        if (_position == before) children.Add(EatToken());
                    }
                }
                else
                {
                    // struct/union body — consume until }
                    int depth = 1;
                    while (depth > 0 && !At(SyntaxKind.EndOfFileToken))
                    {
                        if (At(SyntaxKind.OpenBraceToken)) depth++;
                        if (At(SyntaxKind.CloseBraceToken)) depth--;
                        if (depth > 0) children.Add(EatToken());
                    }
                }
                children.Add(Eat(SyntaxKind.CloseBraceToken));
            }
            // else: forward declaration — no body, just the type name

            ParseTypedefDeclarators(children);

            return MakeNode(SyntaxKind.TypedefDeclaration, children);
        }
        else
        {
            // Simple typedef: typedef Type Name;
            children.Add(ParseTypeReference());
        }

        ParseTypedefDeclarators(children);

        return MakeNode(SyntaxKind.TypedefDeclaration, children);
    }

    /// <summary>
    /// Parse the declarator list that closes a typedef, and the semicolon.
    ///
    /// Classic MIDL declares several names at once and decorates each independently:
    /// <c>typedef long MyLong, *PMyLong;</c> and <c>typedef long Row[10];</c>. Stopping after
    /// the first bare name left the comma or bracket unexpected, and the parser did not
    /// recover — one declaration cost the rest of the file.
    /// </summary>
    private void ParseTypedefDeclarators(List<GreenNode?> children)
    {
        while (true)
        {
            // Pointer declarator: *PName, **PPName
            while (At(SyntaxKind.AsteriskToken))
                children.Add(EatToken());

            if (!IsIdentifierOrKeyword(Current.Kind)) break;
            children.Add(EatIdentifierOrKeyword());

            // Array declarator: [10], [], [SIZE]
            while (At(SyntaxKind.OpenBracketToken))
            {
                children.Add(EatToken());
                while (!At(SyntaxKind.CloseBracketToken) && !At(SyntaxKind.SemicolonToken)
                       && !At(SyntaxKind.EndOfFileToken))
                {
                    children.Add(EatToken());
                }
                if (At(SyntaxKind.CloseBracketToken)) children.Add(EatToken());
            }

            if (!At(SyntaxKind.CommaToken)) break;
            children.Add(EatToken());
        }

        if (At(SyntaxKind.SemicolonToken))
            children.Add(EatToken());
    }

    /// <summary>Parse: cpp_quote("string")</summary>
    private GreenNode ParseCppQuoteDirective(GreenNode? attributes = null)
    {
        var children = RentChildren();
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
        var children = RentChildren();
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
#endif
