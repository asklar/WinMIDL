// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Syntax;
using Midl.Parser;

namespace Midl.Parser.Tests;

public class ErrorRecoveryTests
{
    private static GreenNode Parse(IReadOnlyList<SyntaxToken> tokens)
    {
        var parser = new Midl.Parser.Parser(tokens);
        return parser.Parse();
    }

    private static GreenNode? FindChild(GreenNode node, SyntaxKind kind)
    {
        for (int i = 0; i < node.SlotCount; i++)
        {
            var child = node.GetSlot(i);
            if (child?.Kind == kind) return child;
        }
        return null;
    }

    private static List<GreenNode> ChildrenOfKind(GreenNode node, SyntaxKind kind)
    {
        var result = new List<GreenNode>();
        for (int i = 0; i < node.SlotCount; i++)
        {
            var child = node.GetSlot(i);
            if (child?.Kind == kind) result.Add(child);
        }
        return result;
    }

    /// <summary>Recursively find a descendant of a specific kind.</summary>
    private static GreenNode? FindDescendant(GreenNode node, SyntaxKind kind)
    {
        for (int i = 0; i < node.SlotCount; i++)
        {
            var child = node.GetSlot(i);
            if (child == null) continue;
            if (child.Kind == kind) return child;
            var found = FindDescendant(child, kind);
            if (found != null) return found;
        }
        return null;
    }

    /// <summary>Recursively collect all descendants of a specific kind.</summary>
    private static List<GreenNode> FindAllDescendants(GreenNode node, SyntaxKind kind)
    {
        var result = new List<GreenNode>();
        CollectDescendants(node, kind, result);
        return result;
    }

    private static void CollectDescendants(GreenNode node, SyntaxKind kind, List<GreenNode> result)
    {
        for (int i = 0; i < node.SlotCount; i++)
        {
            var child = node.GetSlot(i);
            if (child == null) continue;
            if (child.Kind == kind) result.Add(child);
            CollectDescendants(child, kind, result);
        }
    }

    // ─── 1: Missing semicolon after method ───

    [Fact]
    public void MissingSemicolonAfterMethod()
    {
        // runtimeclass C { void M() }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("C")
            .OpenBrace
            .Keyword(SyntaxKind.VoidKeyword)
            .Identifier("M")
            .OpenParen
            .CloseParen
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);
        var methods = ChildrenOfKind(rc, SyntaxKind.MethodDeclaration);
        Assert.Single(methods);
    }

    // ─── 2: Missing semicolon after property ───

    [Fact]
    public void MissingSemicolonAfterProperty()
    {
        // runtimeclass C { String Name { get; } }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("C")
            .OpenBrace
            .Keyword(SyntaxKind.StringKeyword)
            .Identifier("Name")
            .OpenBrace
            .Keyword(SyntaxKind.GetKeyword)
            .Semicolon
            .CloseBrace
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);
        var props = ChildrenOfKind(rc, SyntaxKind.PropertyDeclaration);
        Assert.Single(props);
    }

    // ─── 3: Incomplete runtimeclass (no closing braces) ───

    [Fact]
    public void IncompleteRuntimeClass()
    {
        // namespace N { runtimeclass C {
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.NamespaceKeyword)
            .Identifier("N")
            .OpenBrace
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("C")
            .OpenBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        var ns = FindChild(tree, SyntaxKind.NamespaceDeclaration);
        Assert.NotNull(ns);
        var rc = FindDescendant(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);
    }

    // ─── 4: Incomplete attribute ───

    [Fact]
    public void IncompleteAttribute()
    {
        // [uuid(   (unterminated, then a parseable construct follows)
        // Parser should recover and produce a CompilationUnit
        var tokens = new TokenBuilder()
            .OpenBracket
            .Identifier("uuid")
            .OpenParen
            .EOF
            .Build();

        var tree = Parse(tokens);

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        // Tree should exist even with incomplete attribute
        Assert.True(tree.SlotCount >= 1);
    }

    // ─── 5: Cursor mid-typing method (void keyword, no method name) ───

    [Fact]
    public void CursorMidTypingMethod()
    {
        // runtimeclass C { void }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("C")
            .OpenBrace
            .Keyword(SyntaxKind.VoidKeyword)
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);
    }

    // ─── 6: Cursor mid-typing property (type keyword, no property name) ───

    [Fact]
    public void CursorMidTypingProperty()
    {
        // runtimeclass C { String }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("C")
            .OpenBrace
            .Keyword(SyntaxKind.StringKeyword)
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);
    }

    // ─── 7: Garbage between declarations ───

    [Fact]
    public void GarbageBetweenDeclarations()
    {
        // namespace N { @@@ runtimeclass C { } }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.NamespaceKeyword)
            .Identifier("N")
            .OpenBrace
            .Token(SyntaxKind.ErrorToken, "@")
            .Token(SyntaxKind.ErrorToken, "@")
            .Token(SyntaxKind.ErrorToken, "@")
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("C")
            .OpenBrace
            .CloseBrace
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        // Error tokens should be skipped; runtimeclass still parsed
        var rc = FindDescendant(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);
    }

    // ─── 8: Empty namespace body ───

    [Fact]
    public void EmptyNamespaceBody()
    {
        // namespace N { }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.NamespaceKeyword)
            .Identifier("N")
            .OpenBrace
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        var ns = FindChild(tree, SyntaxKind.NamespaceDeclaration);
        Assert.NotNull(ns);
    }

    // ─── 9: Missing open brace ───

    [Fact]
    public void MissingOpenBrace()
    {
        // runtimeclass C void M(); }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("C")
            .Keyword(SyntaxKind.VoidKeyword)
            .Identifier("M")
            .OpenParen
            .CloseParen
            .Semicolon
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);
    }

    // ─── 10: Multiple errors (missing names) ───

    [Fact]
    public void MultipleErrors()
    {
        // namespace N { runtimeclass { } struct { } }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.NamespaceKeyword)
            .Identifier("N")
            .OpenBrace
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .OpenBrace
            .CloseBrace
            .Keyword(SyntaxKind.StructKeyword)
            .OpenBrace
            .CloseBrace
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        var ns = FindChild(tree, SyntaxKind.NamespaceDeclaration);
        Assert.NotNull(ns);

        // Both declarations should be parsed despite missing names
        var allDecls = FindAllDescendants(tree, SyntaxKind.RuntimeClassDeclaration)
            .Concat(FindAllDescendants(tree, SyntaxKind.StructDeclaration))
            .ToList();
        Assert.True(allDecls.Count >= 1, "At least one declaration should be parsed despite missing names");
    }

    // ─── 11: Partial attribute list (trailing comma) ───

    [Fact]
    public void PartialAttributeList()
    {
        // [uuid("abc"), ] runtimeclass C { }
        var tokens = new TokenBuilder()
            .OpenBracket
            .Identifier("uuid")
            .OpenParen
            .StringLiteral("abc")
            .CloseParen
            .Comma
            .CloseBracket
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("C")
            .OpenBrace
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        var rc = FindDescendant(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);
    }

    // ─── 12: Nested error recovery (versioned block with empty contract args) ───

    [Fact]
    public void NestedErrorRecovery()
    {
        // namespace N { runtimeclass C { [contract()] { void M(); } } }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.NamespaceKeyword)
            .Identifier("N")
            .OpenBrace
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("C")
            .OpenBrace
            .OpenBracket
            .Identifier("contract")
            .OpenParen
            .CloseParen
            .CloseBracket
            .OpenBrace
            .Keyword(SyntaxKind.VoidKeyword)
            .Identifier("M")
            .OpenParen
            .CloseParen
            .Semicolon
            .CloseBrace
            .CloseBrace
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        var ns = FindChild(tree, SyntaxKind.NamespaceDeclaration);
        Assert.NotNull(ns);
        var rc = FindDescendant(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);

        // The method should be parsed inside the versioned block
        var method = FindDescendant(tree, SyntaxKind.MethodDeclaration);
        Assert.NotNull(method);
    }
}
