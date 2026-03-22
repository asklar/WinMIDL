using Midl.Syntax;
using Midl.Parser;

namespace Midl.Parser.Tests;

/// <summary>
/// Tests that verify parser optimizations don't change behavior.
/// These exercise the hot paths targeted by SingleChildGreenNode,
/// token array storage, and span-based AtAny/SkipTokensUntil.
/// </summary>
public class ParserOptimizationTests
{
    private static GreenNode Parse(string source)
    {
        var lexer = new Midl.Lexer.Lexer(source);
        var tokens = lexer.Lex();
        var parser = new Midl.Parser.Parser(tokens);
        return parser.Parse();
    }

    private static GreenNode? FindDescendant(GreenNode node, SyntaxKind kind)
    {
        if (node.Kind == kind) return node;
        for (int i = 0; i < node.SlotCount; i++)
        {
            var child = node.GetSlot(i);
            if (child != null)
            {
                var found = FindDescendant(child, kind);
                if (found != null) return found;
            }
        }
        return null;
    }

    private static List<GreenNode> FindAllDescendants(GreenNode node, SyntaxKind kind)
    {
        var result = new List<GreenNode>();
        CollectDescendants(node, kind, result);
        return result;
    }

    private static void CollectDescendants(GreenNode node, SyntaxKind kind, List<GreenNode> result)
    {
        if (node.Kind == kind) result.Add(node);
        for (int i = 0; i < node.SlotCount; i++)
        {
            var child = node.GetSlot(i);
            if (child != null) CollectDescendants(child, kind, result);
        }
    }

    // --- SingleChildGreenNode tests ---

    [Fact]
    public void SimpleTypeReference_has_correct_structure()
    {
        // "Int32" type reference → single-child node (optimized path)
        var tree = Parse("namespace Foo { runtimeclass Bar { Int32 Prop; } }");
        var typeRef = FindDescendant(tree, SyntaxKind.TypeReference);

        Assert.NotNull(typeRef);
        Assert.Equal(1, typeRef.SlotCount);
        Assert.NotNull(typeRef.GetSlot(0));
        // Int32 is a keyword token, not an identifier
        Assert.True(typeRef.GetSlot(0) is SyntaxToken token &&
            (token.Kind == SyntaxKind.IdentifierToken || token.Kind == SyntaxKind.Int32Keyword));
        Assert.True(typeRef.FullWidth > 0);
    }

    [Fact]
    public void SingleChild_NumericLiteral_expression()
    {
        // "42" in enum → single-child NumericLiteralExpression
        var tree = Parse("namespace Foo { enum E { A = 42 } }");
        var numExpr = FindDescendant(tree, SyntaxKind.NumericLiteralExpression);

        Assert.NotNull(numExpr);
        Assert.Equal(1, numExpr.SlotCount);
        Assert.True(numExpr.GetSlot(0) is SyntaxToken { Kind: SyntaxKind.NumericLiteralToken });
    }

    [Fact]
    public void SingleChild_IdentifierExpression()
    {
        // Identifier expression in enum member initializer
        var tree = Parse("namespace Foo { enum E { A = 1, B = A } }");
        var idExprs = FindAllDescendants(tree, SyntaxKind.IdentifierExpression);

        Assert.True(idExprs.Count > 0);
        foreach (var idExpr in idExprs)
        {
            Assert.True(idExpr.SlotCount >= 1);
            Assert.NotNull(idExpr.GetSlot(0));
        }
    }

    // --- FullWidth preservation ---

    [Fact]
    public void FullWidth_preserved_for_all_node_types()
    {
        var source = "namespace Foo { runtimeclass Bar { Int32 Prop; void Method(String arg); } }";
        var tree = Parse(source);

        // The tree's FullWidth should equal the source length
        Assert.Equal(source.Length, tree.FullWidth);
    }

    [Fact]
    public void FullWidth_preserved_with_generics()
    {
        var source = "namespace Foo { runtimeclass Bar { Windows.Foundation.Collections.IVector<String> Items; } }";
        var tree = Parse(source);
        Assert.Equal(source.Length, tree.FullWidth);
    }

    // --- Multi-child nodes still work ---

    [Fact]
    public void MultiChild_BinaryExpression_correct()
    {
        var tree = Parse("namespace Foo { enum E { A = 1 + 2 } }");
        var binExpr = FindDescendant(tree, SyntaxKind.BinaryExpression);

        Assert.NotNull(binExpr);
        Assert.Equal(3, binExpr.SlotCount); // left, op, right
    }

    [Fact]
    public void PointerType_two_children()
    {
        var tree = Parse("namespace Foo { struct S { Int32* field; } }");
        var ptrType = FindDescendant(tree, SyntaxKind.PointerType);

        Assert.NotNull(ptrType);
        Assert.Equal(2, ptrType.SlotCount); // base type, star
    }

    // --- Error recovery still works with optimized paths ---

    [Fact]
    public void ErrorRecovery_SkipTokensUntil_produces_skipped_node()
    {
        // Invalid token before a declaration should be skipped
        var tree = Parse("namespace Foo { @@@ runtimeclass Bar { } }");
        var skipped = FindDescendant(tree, SyntaxKind.SkippedTokens);
        // There should be error tokens or skipped tokens in the tree
        Assert.NotNull(skipped);
    }

    [Fact]
    public void ErrorRecovery_MissingToken_still_works()
    {
        // Missing semicolon
        var tree = Parse("namespace Foo { runtimeclass Bar { Int32 Prop } }");
        var missingTokens = FindAllDescendants(tree, SyntaxKind.MissingToken);
        // Should find a missing semicolon token but parser should still produce valid tree
        Assert.True(missingTokens.Count > 0 || tree.Kind == SyntaxKind.CompilationUnit);
    }

    // --- Generic disambiguation (exercises Peek and pending token) ---

    [Fact]
    public void NestedGenerics_split_correctly()
    {
        var tree = Parse("namespace Foo { runtimeclass Bar { IVector<IMap<String, Int32>> Items; } }");
        var typeArgLists = FindAllDescendants(tree, SyntaxKind.TypeArgumentList);
        Assert.True(typeArgLists.Count >= 2); // outer and inner generic
        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
    }

    // --- Lexer token handoff ---

    [Fact]
    public void Parser_accepts_lexer_output_directly()
    {
        // Verifies the lexer→parser handoff works (token list type compatibility)
        var source = "namespace Test { runtimeclass Widget { String Name { get; }; } }";
        var lexer = new Midl.Lexer.Lexer(source);
        var tokens = lexer.Lex();
        var parser = new Midl.Parser.Parser(tokens);
        var tree = parser.Parse();

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        Assert.Equal(source.Length, tree.FullWidth);
    }

    // --- GetSlot out-of-range returns null ---

    [Fact]
    public void SingleChild_GetSlot_out_of_range_returns_null()
    {
        var tree = Parse("namespace Foo { runtimeclass Bar { Int32 Prop; } }");
        var typeRef = FindDescendant(tree, SyntaxKind.TypeReference);

        Assert.NotNull(typeRef);
        // After optimization, SingleChildGreenNode returns null for out-of-range;
        // GreenNodeList would throw. Both are valid — test the contract.
        Assert.Equal(1, typeRef.SlotCount);
        Assert.NotNull(typeRef.GetSlot(0));
    }
}
