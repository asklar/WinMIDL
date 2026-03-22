using Midl.Syntax;

namespace Midl.Parser;

public sealed partial class Parser
{
    /// <summary>Parse a full compilation unit: imports, declarations, EOF.</summary>
    private GreenNode ParseCompilationUnit()
    {
        var children = RentChildren();

        // Parse import directives
        while (At(SyntaxKind.ImportKeyword))
        {
            children.Add(ParseImportDirective());
        }

        // Parse declarations until EOF
        while (!At(SyntaxKind.EndOfFileToken))
        {
            var before = _position;
            var decl = ParseDeclaration();
            if (decl != null)
            {
                children.Add(decl);
            }

            // Safety: if we didn't make progress, skip a token to avoid infinite loop
            if (_position == before)
            {
                var skipped = SkipToDeclarationStart();
                if (skipped != null) children.Add(skipped);
                if (_position == before)
                {
                    // Still stuck — eat one token to guarantee progress
                    children.Add(EatToken());
                }
            }
        }

        children.Add(Eat(SyntaxKind.EndOfFileToken));
        return MakeNode(SyntaxKind.CompilationUnit, children);
    }

    /// <summary>Parse: import "file.idl";</summary>
    private GreenNode ParseImportDirective()
    {
        var importKeyword = Eat(SyntaxKind.ImportKeyword);
        var path = Eat(SyntaxKind.StringLiteralToken);
        var semicolon = Eat(SyntaxKind.SemicolonToken);
        return MakeNode(SyntaxKind.ImportDirective, importKeyword, path, semicolon);
    }

    /// <summary>Parse a single declaration, dispatching based on leading attributes and current token.</summary>
    private GreenNode? ParseDeclaration()
    {
        // Parse any leading attribute lists
        var attributes = RentChildren();
        while (At(SyntaxKind.OpenBracketToken))
        {
            attributes.Add(ParseAttributeList());
        }

        // Parse modifiers
        var modifiers = RentChildren();
        while (IsModifier(Current.Kind))
        {
            modifiers.Add(EatToken());
        }

        GreenNode? attrNode = attributes.Count > 0
            ? MakeNode(SyntaxKind.AttributeList, attributes)
            : null;

        GreenNode? modNode = modifiers.Count > 0
            ? MakeNode(SyntaxKind.ModifierList, modifiers)
            : null;

        return Current.Kind switch
        {
            SyntaxKind.NamespaceKeyword => ParseNamespaceDeclaration(attrNode),
            SyntaxKind.RuntimeclassKeyword => ParseRuntimeClassDeclaration(attrNode, modNode),
            SyntaxKind.InterfaceKeyword when modNode != null => ReportModifiersOnNonClass(attrNode, modNode, "interface"),
            SyntaxKind.InterfaceKeyword => ParseInterfaceDeclaration(attrNode),
            SyntaxKind.DelegateKeyword when modNode != null => ReportModifiersOnNonClass(attrNode, modNode, "delegate"),
            SyntaxKind.DelegateKeyword => ParseDelegateDeclaration(attrNode),
            SyntaxKind.StructKeyword when modNode != null => ReportModifiersOnNonClass(attrNode, modNode, "struct"),
            SyntaxKind.StructKeyword => ParseStructDeclaration(attrNode),
            SyntaxKind.EnumKeyword when modNode != null => ReportModifiersOnNonClass(attrNode, modNode, "enum"),
            SyntaxKind.EnumKeyword => ParseEnumDeclaration(attrNode),
            SyntaxKind.ApicontractKeyword => ParseApiContractDeclaration(attrNode),
            SyntaxKind.AttributeKeyword => ParseAttributeDeclaration(attrNode),
            SyntaxKind.DeclareKeyword => ParseDeclareBlock(attrNode),
            SyntaxKind.ImportKeyword => ParseImportDirective(),
#if WITH_COMPAT
            SyntaxKind.CoclassKeyword => ParseCoclassDeclaration(attrNode),
            SyntaxKind.LibraryKeyword => ParseLibraryDeclaration(attrNode),
            SyntaxKind.TypedefKeyword => ParseTypedefDeclaration(attrNode),
            SyntaxKind.CppQuoteKeyword => ParseCppQuoteDirective(attrNode),
            SyntaxKind.UnionKeyword => ParseUnionDeclaration(attrNode),
            SyntaxKind.DispinterfaceKeyword => ParseDispinterfaceDeclaration(attrNode),
#if WINDOWS_INTERNAL
            // feature_name is a Velocity/Feature staging declaration (not a keyword)
            _ when Current.Kind == SyntaxKind.IdentifierToken && Current.Text == "feature_name"
                => ParseFeatureNameDeclaration(),
#endif
#endif
            _ => HandleUnexpectedDeclaration(attrNode, modNode)
        };
    }

    /// <summary>Handle tokens that don't start a recognized declaration.</summary>
    private GreenNode? HandleUnexpectedDeclaration(GreenNode? attrNode, GreenNode? modNode)
    {
        var parts = RentChildren();
        if (attrNode != null) parts.Add(attrNode);
        if (modNode != null) parts.Add(modNode);

        var skipped = SkipToDeclarationStart();
        if (skipped != null) parts.Add(skipped);

        if (parts.Count > 0)
            return MakeNode(SyntaxKind.SkippedTokens, parts);

        return null;
    }

#if WITH_COMPAT && WINDOWS_INTERNAL
    /// <summary>Parse: feature_name Name = {Stage, ValidInAllBranches}; — Velocity/Feature staging declaration.</summary>
    private GreenNode ParseFeatureNameDeclaration()
    {
        var children = RentChildren();
        children.Add(EatToken()); // feature_name (identifier)
        children.Add(EatIdentifierOrKeyword()); // Name

        if (At(SyntaxKind.EqualsToken))
        {
            children.Add(EatToken()); // =
            if (At(SyntaxKind.OpenBraceToken))
            {
                children.Add(EatToken()); // {
                children.Add(EatIdentifierOrKeyword()); // Stage (e.g. AlwaysEnabled)
                children.Add(Eat(SyntaxKind.CommaToken)); // ,
                // TRUE/FALSE are lexed as TrueKeyword/FalseKeyword, not identifiers
                if (At(SyntaxKind.TrueKeyword) || At(SyntaxKind.FalseKeyword))
                    children.Add(EatToken());
                else
                    children.Add(EatIdentifierOrKeyword());
                children.Add(Eat(SyntaxKind.CloseBraceToken)); // }
            }
        }

        if (At(SyntaxKind.SemicolonToken))
            children.Add(EatToken());

        return MakeNode(SyntaxKind.DeclareBlock, children);
    }
#endif

    /// <summary>Parse: declare { declarations... }</summary>
    private GreenNode ParseDeclareBlock(GreenNode? attributes)
    {
        var children = RentChildren();
        if (attributes != null) children.Add(attributes);

        children.Add(Eat(SyntaxKind.DeclareKeyword));
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
        return MakeNode(SyntaxKind.DeclareBlock, children);
    }

    /// <summary>Report an error when modifiers (static, unsealed) are used on non-runtimeclass declarations.</summary>
    private GreenNode? ReportModifiersOnNonClass(GreenNode? attrNode, GreenNode modNode, string keyword)
    {
        // Convert modifiers to SkippedTokens so SyntaxErrorCollector reports MIDL1100
        var modChildren = RentChildren();
        for (int i = 0; i < modNode.SlotCount; i++)
            modChildren.Add(modNode.GetSlot(i));
        var skippedMods = MakeNode(SyntaxKind.SkippedTokens, modChildren);

        // Parse the declaration normally
        var decl = keyword switch
        {
            "interface" => ParseInterfaceDeclaration(attrNode),
            "delegate" => ParseDelegateDeclaration(attrNode),
            "struct" => ParseStructDeclaration(attrNode),
            "enum" => ParseEnumDeclaration(attrNode),
            _ => HandleUnexpectedDeclaration(attrNode, modNode)
        };

        if (decl == null) return skippedMods;

        // Prepend SkippedTokens to the declaration children
        var wrappedChildren = RentChildren();
        wrappedChildren.Add(skippedMods);
        for (int i = 0; i < decl.SlotCount; i++)
            wrappedChildren.Add(decl.GetSlot(i));
        return MakeNode(decl.Kind, wrappedChildren);
    }
}
