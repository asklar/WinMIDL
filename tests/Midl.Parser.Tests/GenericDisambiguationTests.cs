using Midl.Syntax;
using Midl.Parser;

namespace Midl.Parser.Tests;

public class GenericDisambiguationTests
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

    private static GreenNode? FindChildRecursive(GreenNode node, SyntaxKind kind)
    {
        for (int i = 0; i < node.SlotCount; i++)
        {
            var child = node.GetSlot(i);
            if (child == null) continue;
            if (child.Kind == kind) return child;
            var found = FindChildRecursive(child, kind);
            if (found != null) return found;
        }
        return null;
    }

    private static List<GreenNode> FindAllRecursive(GreenNode node, SyntaxKind kind)
    {
        var result = new List<GreenNode>();
        void Walk(GreenNode n)
        {
            if (n.Kind == kind) result.Add(n);
            for (int i = 0; i < n.SlotCount; i++)
            {
                var child = n.GetSlot(i);
                if (child != null) Walk(child);
            }
        }
        Walk(node);
        return result;
    }

    /// <summary>
    /// Task 4: IAsyncOperation&lt;IVector&lt;String&gt;&gt; should parse correctly with >> at end.
    /// </summary>
    [Fact]
    public void DoubleGreaterThanInGeneric()
    {
        // delegate void Handler(IAsyncOperation<IVector<String>> result);
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.DelegateKeyword)
            .Keyword(SyntaxKind.VoidKeyword)
            .Identifier("Handler")
            .OpenParen
            .Identifier("IAsyncOperation")
            .LessThan
            .Identifier("IVector")
            .LessThan
            .Keyword(SyntaxKind.StringKeyword)
            .Token(SyntaxKind.GreaterThanGreaterThanToken, ">>")
            .Identifier("result")
            .CloseParen
            .Semicolon
            .EOF
            .Build();

        var tree = Parse(tokens);
        var del = FindChild(tree, SyntaxKind.DelegateDeclaration);
        Assert.NotNull(del);

        // Should have two GenericName nodes (IAsyncOperation<...> and IVector<...>)
        var generics = FindAllRecursive(tree, SyntaxKind.GenericName);
        Assert.Equal(2, generics.Count);
    }

    /// <summary>
    /// Task 4: Triple nested generic IMap&lt;String, IVector&lt;IReference&lt;Int32&gt;&gt;&gt; parses.
    /// </summary>
    [Fact]
    public void TripleNestedGeneric()
    {
        // delegate void Handler(IMap<String, IVector<IReference<Int32>>> result);
        // Token stream uses >> for the inner two >>, and then a separate > for the outer
        // Actually with 3 levels: IMap< String, IVector< IReference< Int32 >>> result
        // The lexer would produce: IMap < String , IVector < IReference < Int32 >> > result
        // where >> closes IReference and IVector, then > closes IMap
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.DelegateKeyword)
            .Keyword(SyntaxKind.VoidKeyword)
            .Identifier("Handler")
            .OpenParen
            .Identifier("IMap")
            .LessThan
            .Keyword(SyntaxKind.StringKeyword)
            .Comma
            .Identifier("IVector")
            .LessThan
            .Identifier("IReference")
            .LessThan
            .Keyword(SyntaxKind.Int32Keyword)
            .Token(SyntaxKind.GreaterThanGreaterThanToken, ">>")
            .GreaterThan
            .Identifier("result")
            .CloseParen
            .Semicolon
            .EOF
            .Build();

        var tree = Parse(tokens);
        var del = FindChild(tree, SyntaxKind.DelegateDeclaration);
        Assert.NotNull(del);

        // Should have three GenericName nodes
        var generics = FindAllRecursive(tree, SyntaxKind.GenericName);
        Assert.Equal(3, generics.Count);
    }

    /// <summary>
    /// Single greater-than in generic still works.
    /// </summary>
    [Fact]
    public void SingleGreaterThanStillWorks()
    {
        // delegate void Handler(IVector<String> items);
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.DelegateKeyword)
            .Keyword(SyntaxKind.VoidKeyword)
            .Identifier("Handler")
            .OpenParen
            .Identifier("IVector")
            .LessThan
            .Keyword(SyntaxKind.StringKeyword)
            .GreaterThan
            .Identifier("items")
            .CloseParen
            .Semicolon
            .EOF
            .Build();

        var tree = Parse(tokens);
        var del = FindChild(tree, SyntaxKind.DelegateDeclaration);
        Assert.NotNull(del);

        var generics = FindAllRecursive(tree, SyntaxKind.GenericName);
        Assert.Single(generics);
    }

    /// <summary>
    /// Right shift in enum expressions still produces >> token (not split).
    /// </summary>
    [Fact]
    public void RightShiftInExpressionStillWorks()
    {
        // enum E { A = 1 >> 2, }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.EnumKeyword)
            .Identifier("E")
            .OpenBrace
            .Identifier("A")
            .Equals
            .NumericLiteral("1")
            .Token(SyntaxKind.GreaterThanGreaterThanToken, ">>")
            .NumericLiteral("2")
            .Comma
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);
        var enumDecl = FindChild(tree, SyntaxKind.EnumDeclaration);
        Assert.NotNull(enumDecl);

        // Should have a binary expression with >> operator
        var binExpr = FindChildRecursive(tree, SyntaxKind.BinaryExpression);
        Assert.NotNull(binExpr);
    }
}
