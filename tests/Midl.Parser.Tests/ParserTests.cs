using Midl.Syntax;
using Midl.Parser;

namespace Midl.Parser.Tests;

/// <summary>
/// Helper to build token lists for parser tests without a real lexer.
/// </summary>
internal class TokenBuilder
{
    private readonly List<SyntaxToken> _tokens = new();

    public TokenBuilder Token(SyntaxKind kind, string text)
    {
        _tokens.Add(new SyntaxToken(kind, text));
        return this;
    }

    public TokenBuilder Keyword(SyntaxKind kind) => Token(kind, KindToText(kind));
    public TokenBuilder Identifier(string name) => Token(SyntaxKind.IdentifierToken, name);
    public TokenBuilder StringLiteral(string value) => Token(SyntaxKind.StringLiteralToken, $"\"{value}\"");
    public TokenBuilder NumericLiteral(string value) => Token(SyntaxKind.NumericLiteralToken, value);
    public TokenBuilder GuidLiteral(string value) => Token(SyntaxKind.GuidLiteralToken, value);

    // Punctuation helpers
    public TokenBuilder OpenBrace => Token(SyntaxKind.OpenBraceToken, "{");
    public TokenBuilder CloseBrace => Token(SyntaxKind.CloseBraceToken, "}");
    public TokenBuilder OpenParen => Token(SyntaxKind.OpenParenToken, "(");
    public TokenBuilder CloseParen => Token(SyntaxKind.CloseParenToken, ")");
    public TokenBuilder OpenBracket => Token(SyntaxKind.OpenBracketToken, "[");
    public TokenBuilder CloseBracket => Token(SyntaxKind.CloseBracketToken, "]");
    public TokenBuilder Semicolon => Token(SyntaxKind.SemicolonToken, ";");
    public TokenBuilder Colon => Token(SyntaxKind.ColonToken, ":");
    public TokenBuilder Comma => Token(SyntaxKind.CommaToken, ",");
    public TokenBuilder Dot => Token(SyntaxKind.DotToken, ".");
    public new TokenBuilder Equals => Token(SyntaxKind.EqualsToken, "=");
    public TokenBuilder LessThan => Token(SyntaxKind.LessThanToken, "<");
    public TokenBuilder GreaterThan => Token(SyntaxKind.GreaterThanToken, ">");
    public TokenBuilder Pipe => Token(SyntaxKind.PipeToken, "|");
    public TokenBuilder EOF => Token(SyntaxKind.EndOfFileToken, "");

    public IReadOnlyList<SyntaxToken> Build() => _tokens;

    private static string KindToText(SyntaxKind kind) => kind switch
    {
        SyntaxKind.NamespaceKeyword => "namespace",
        SyntaxKind.RuntimeclassKeyword => "runtimeclass",
        SyntaxKind.InterfaceKeyword => "interface",
        SyntaxKind.DelegateKeyword => "delegate",
        SyntaxKind.StructKeyword => "struct",
        SyntaxKind.EnumKeyword => "enum",
        SyntaxKind.ApicontractKeyword => "apicontract",
        SyntaxKind.EventKeyword => "event",
        SyntaxKind.ImportKeyword => "import",
        SyntaxKind.DeclareKeyword => "declare",
        SyntaxKind.StaticKeyword => "static",
        SyntaxKind.PartialKeyword => "partial",
        SyntaxKind.UnsealedKeyword => "unsealed",
        SyntaxKind.ProtectedKeyword => "protected",
        SyntaxKind.OverridableKeyword => "overridable",
        SyntaxKind.VoidKeyword => "void",
        SyntaxKind.Int32Keyword => "Int32",
        SyntaxKind.UInt32Keyword => "UInt32",
        SyntaxKind.StringKeyword => "String",
        SyntaxKind.BooleanKeyword => "Boolean",
        SyntaxKind.DoubleKeyword => "Double",
        SyntaxKind.ObjectKeyword => "Object",
        SyntaxKind.GetKeyword => "get",
        SyntaxKind.SetKeyword => "set",
        SyntaxKind.OutKeyword => "out",
        SyntaxKind.RefKeyword => "ref",
        SyntaxKind.AttributeKeyword => "attribute",
        _ => kind.ToString()
    };
}

public class ParserTests
{
    private static GreenNode Parse(IReadOnlyList<SyntaxToken> tokens)
    {
        var parser = new Midl.Parser.Parser(tokens);
        return parser.Parse();
    }

    [Fact]
    public void ParseBaseList_WithDefaultAttribute()
    {
        // runtimeclass Foo : [default] IFoo, IBar {}
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("Foo")
            .Colon
            .OpenBracket
            .Token(SyntaxKind.DefaultKeyword, "default")
            .CloseBracket
            .Identifier("IFoo")
            .Comma
            .Identifier("IBar")
            .OpenBrace
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);
        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);

        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);

        var baseList = FindChild(rc!, SyntaxKind.BaseList);
        Assert.NotNull(baseList);

        // First entry should be a BaseType node wrapping [default] IFoo
        var baseType = FindChild(baseList!, SyntaxKind.BaseType);
        Assert.NotNull(baseType);

        // The BaseType should contain an AttributeList
        var attrList = FindChild(baseType!, SyntaxKind.AttributeList);
        Assert.NotNull(attrList);
    }

    private static GreenNode? GetSlot(GreenNode node, int index)
        => node.GetSlot(index);

    private static SyntaxKind ChildKind(GreenNode node, int index)
        => node.GetSlot(index)?.Kind ?? SyntaxKind.None;

    /// <summary>Find a child node of a specific kind (first occurrence, recursive one level).</summary>
    private static GreenNode? FindChild(GreenNode node, SyntaxKind kind)
    {
        for (int i = 0; i < node.SlotCount; i++)
        {
            var child = node.GetSlot(i);
            if (child?.Kind == kind) return child;
        }
        return null;
    }

    /// <summary>Collect all children of a specific kind.</summary>
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

    // ─── Test 1: Simple namespace with a runtimeclass ───

    [Fact]
    public void ParseNamespaceWithRuntimeClass()
    {
        // namespace MyApp { runtimeclass Widget { } }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.NamespaceKeyword)
            .Identifier("MyApp")
            .OpenBrace
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("Widget")
            .OpenBrace
            .CloseBrace
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        var ns = FindChild(tree, SyntaxKind.NamespaceDeclaration);
        Assert.NotNull(ns);
        var rc = FindChild(ns, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);
    }

    // ─── Test 2: RuntimeClass with constructor, method, property, event ───

    [Fact]
    public void ParseRuntimeClassWithMembers()
    {
        // runtimeclass Widget : BaseWidget {
        //     Widget(String name);
        //     void DoThing(Int32 x);
        //     String Name { get; set; };
        //     event EventHandler Changed;
        // }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("Widget")
            .Colon
            .Identifier("BaseWidget")
            .OpenBrace
            // Constructor
            .Identifier("Widget")
            .OpenParen
            .Keyword(SyntaxKind.StringKeyword)
            .Identifier("name")
            .CloseParen
            .Semicolon
            // Method
            .Keyword(SyntaxKind.VoidKeyword)
            .Identifier("DoThing")
            .OpenParen
            .Keyword(SyntaxKind.Int32Keyword)
            .Identifier("x")
            .CloseParen
            .Semicolon
            // Property
            .Keyword(SyntaxKind.StringKeyword)
            .Identifier("Name")
            .OpenBrace
            .Keyword(SyntaxKind.GetKeyword)
            .Semicolon
            .Keyword(SyntaxKind.SetKeyword)
            .Semicolon
            .CloseBrace
            .Semicolon
            // Event
            .Keyword(SyntaxKind.EventKeyword)
            .Identifier("EventHandler")
            .Identifier("Changed")
            .Semicolon
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);
        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);

        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);

        // Check base list
        var baseList = FindChild(rc, SyntaxKind.BaseList);
        Assert.NotNull(baseList);

        // Check members
        var ctors = ChildrenOfKind(rc, SyntaxKind.ConstructorDeclaration);
        Assert.Single(ctors);

        var methods = ChildrenOfKind(rc, SyntaxKind.MethodDeclaration);
        Assert.Single(methods);

        var props = ChildrenOfKind(rc, SyntaxKind.PropertyDeclaration);
        Assert.Single(props);

        var events = ChildrenOfKind(rc, SyntaxKind.EventDeclaration);
        Assert.Single(events);
    }

    // ─── Test 3: Interface with requires ───

    [Fact]
    public void ParseInterfaceWithRequires()
    {
        // interface IWidget requires IClosable, IStringable { void Close(); }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.InterfaceKeyword)
            .Identifier("IWidget")
            .Identifier("requires")  // contextual keyword
            .Identifier("IClosable")
            .Comma
            .Identifier("IStringable")
            .OpenBrace
            .Keyword(SyntaxKind.VoidKeyword)
            .Identifier("Close")
            .OpenParen
            .CloseParen
            .Semicolon
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);
        var iface = FindChild(tree, SyntaxKind.InterfaceDeclaration);
        Assert.NotNull(iface);

        var requires = FindChild(iface, SyntaxKind.RequiresClause);
        Assert.NotNull(requires);

        var methods = ChildrenOfKind(iface, SyntaxKind.MethodDeclaration);
        Assert.Single(methods);
    }

    // ─── Test 4: Struct and Enum ───

    [Fact]
    public void ParseStructAndEnum()
    {
        // struct Point { Int32 X; Int32 Y; }
        // enum Color { Red, Green = 1, Blue = 2, }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.StructKeyword)
            .Identifier("Point")
            .OpenBrace
            .Keyword(SyntaxKind.Int32Keyword)
            .Identifier("X")
            .Semicolon
            .Keyword(SyntaxKind.Int32Keyword)
            .Identifier("Y")
            .Semicolon
            .CloseBrace
            .Keyword(SyntaxKind.EnumKeyword)
            .Identifier("Color")
            .OpenBrace
            .Identifier("Red")
            .Comma
            .Identifier("Green")
            .Equals
            .NumericLiteral("1")
            .Comma
            .Identifier("Blue")
            .Equals
            .NumericLiteral("2")
            .Comma
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        var structDecl = FindChild(tree, SyntaxKind.StructDeclaration);
        Assert.NotNull(structDecl);
        var fields = ChildrenOfKind(structDecl, SyntaxKind.FieldDeclaration);
        Assert.Equal(2, fields.Count);

        var enumDecl = FindChild(tree, SyntaxKind.EnumDeclaration);
        Assert.NotNull(enumDecl);
        var members = ChildrenOfKind(enumDecl, SyntaxKind.EnumMemberDeclaration);
        Assert.Equal(3, members.Count);
    }

    // ─── Test 5: Delegate ───

    [Fact]
    public void ParseDelegate()
    {
        // delegate void EventHandler(Object sender, String args);
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.DelegateKeyword)
            .Keyword(SyntaxKind.VoidKeyword)
            .Identifier("EventHandler")
            .OpenParen
            .Keyword(SyntaxKind.ObjectKeyword)
            .Identifier("sender")
            .Comma
            .Keyword(SyntaxKind.StringKeyword)
            .Identifier("args")
            .CloseParen
            .Semicolon
            .EOF
            .Build();

        var tree = Parse(tokens);
        var del = FindChild(tree, SyntaxKind.DelegateDeclaration);
        Assert.NotNull(del);

        var paramList = FindChild(del, SyntaxKind.ParameterList);
        Assert.NotNull(paramList);
    }

    // ─── Test 6: Attribute lists with various arguments ───

    [Fact]
    public void ParseAttributeListWithArguments()
    {
        // [uuid("12345678-1234-1234-1234-123456789abc"), version(1)]
        // runtimeclass Foo { }
        var tokens = new TokenBuilder()
            .OpenBracket
            .Identifier("uuid")
            .OpenParen
            .StringLiteral("12345678-1234-1234-1234-123456789abc")
            .CloseParen
            .Comma
            .Identifier("version")
            .OpenParen
            .NumericLiteral("1")
            .CloseParen
            .CloseBracket
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("Foo")
            .OpenBrace
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);
        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);

        // The attribute list is wrapped around the runtimeclass
        // Find any AttributeList in the tree
        var attrList = FindChild(rc, SyntaxKind.AttributeList);
        Assert.NotNull(attrList);
    }

    // ─── Test 7: Versioned blocks ───

    [Fact]
    public void ParseVersionedBlock()
    {
        // runtimeclass Widget {
        //     [contract(WidgetContract, 1)] {
        //         void DoV1();
        //     }
        // }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("Widget")
            .OpenBrace
            .OpenBracket
            .Identifier("contract")
            .OpenParen
            .Identifier("WidgetContract")
            .Comma
            .NumericLiteral("1")
            .CloseParen
            .CloseBracket
            .OpenBrace
            .Keyword(SyntaxKind.VoidKeyword)
            .Identifier("DoV1")
            .OpenParen
            .CloseParen
            .Semicolon
            .CloseBrace
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);
        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);

        var vb = FindChild(rc, SyntaxKind.VersionedBlock);
        Assert.NotNull(vb);

        var methods = ChildrenOfKind(vb, SyntaxKind.MethodDeclaration);
        Assert.Single(methods);
    }

    // ─── Test 8: Error recovery — missing closing brace ───

    [Fact]
    public void ErrorRecovery_MissingClosingBrace()
    {
        // namespace MyApp { runtimeclass Widget {
        // (no closing braces) → should still produce a tree
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.NamespaceKeyword)
            .Identifier("MyApp")
            .OpenBrace
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("Widget")
            .OpenBrace
            .Keyword(SyntaxKind.VoidKeyword)
            .Identifier("DoThing")
            .OpenParen
            .CloseParen
            .Semicolon
            .EOF  // missing } }
            .Build();

        var tree = Parse(tokens);

        // Should produce a CompilationUnit regardless
        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);

        // Should have a namespace
        var ns = FindChild(tree, SyntaxKind.NamespaceDeclaration);
        Assert.NotNull(ns);

        // The runtimeclass should be present even without closing brace
        var rc = FindChild(ns, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);

        // The method should still be parsed
        var methods = ChildrenOfKind(rc, SyntaxKind.MethodDeclaration);
        Assert.Single(methods);
    }

    // ─── Test 9: Error recovery — incomplete declaration (mid-typing) ───

    [Fact]
    public void ErrorRecovery_IncompleteDeclaration()
    {
        // runtimeclass  (user stopped typing after keyword)
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .EOF  // incomplete
            .Build();

        var tree = Parse(tokens);
        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);

        // Should still produce a RuntimeClassDeclaration with missing tokens
        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);

        // Should have MissingToken children for name and braces
        bool hasMissing = false;
        for (int i = 0; i < rc.SlotCount; i++)
        {
            if (rc.GetSlot(i)?.Kind == SyntaxKind.MissingToken)
            {
                hasMissing = true;
                break;
            }
        }
        Assert.True(hasMissing, "Incomplete declaration should have MissingToken nodes");
    }

    // ─── Test 10: Error recovery — invalid token in attribute list ───

    [Fact]
    public void ErrorRecovery_InvalidTokenInAttributeList()
    {
        // [invalid $ stuff] runtimeclass Foo { }
        // The $ is an error token; parser should skip to ] and continue
        var tokens = new TokenBuilder()
            .OpenBracket
            .Identifier("invalid")
            .Token(SyntaxKind.ErrorToken, "$")
            .Identifier("stuff")
            .CloseBracket
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("Foo")
            .OpenBrace
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);
        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);

        // Should still find the runtimeclass after recovering
        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);
    }

    // ─── Additional structural tests ───

    [Fact]
    public void ParseImportDirective()
    {
        // import "windows.foundation.idl";
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.ImportKeyword)
            .StringLiteral("windows.foundation.idl")
            .Semicolon
            .EOF
            .Build();

        var tree = Parse(tokens);
        var import = FindChild(tree, SyntaxKind.ImportDirective);
        Assert.NotNull(import);
    }

    [Fact]
    public void ParseDottedNamespace()
    {
        // namespace Windows.UI.Xaml { }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.NamespaceKeyword)
            .Identifier("Windows")
            .Dot
            .Identifier("UI")
            .Dot
            .Identifier("Xaml")
            .OpenBrace
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);
        var ns = FindChild(tree, SyntaxKind.NamespaceDeclaration);
        Assert.NotNull(ns);

        // Check that dotted name is present
        var dottedName = FindChild(ns, SyntaxKind.DottedName);
        Assert.NotNull(dottedName);
    }

    [Fact]
    public void ParseGenericTypeReference()
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
    }

    [Fact]
    public void ParseApiContract()
    {
        // apicontract WidgetContract { }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.ApicontractKeyword)
            .Identifier("WidgetContract")
            .OpenBrace
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);
        var api = FindChild(tree, SyntaxKind.ApiContractDeclaration);
        Assert.NotNull(api);
    }

    [Fact]
    public void ParseStaticMethod()
    {
        // runtimeclass Foo { static Int32 Create(); }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("Foo")
            .OpenBrace
            .Keyword(SyntaxKind.StaticKeyword)
            .Keyword(SyntaxKind.Int32Keyword)
            .Identifier("Create")
            .OpenParen
            .CloseParen
            .Semicolon
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);
        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);

        var methods = ChildrenOfKind(rc, SyntaxKind.MethodDeclaration);
        Assert.Single(methods);
    }

    [Fact]
    public void ParseEnumWithBitwiseExpressions()
    {
        // enum Flags { A = 0x1, B = 0x2, C = A | B, }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.EnumKeyword)
            .Identifier("Flags")
            .OpenBrace
            .Identifier("A")
            .Equals
            .NumericLiteral("0x1")
            .Comma
            .Identifier("B")
            .Equals
            .NumericLiteral("0x2")
            .Comma
            .Identifier("C")
            .Equals
            .Identifier("A")
            .Pipe
            .Identifier("B")
            .Comma
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);
        var enumDecl = FindChild(tree, SyntaxKind.EnumDeclaration);
        Assert.NotNull(enumDecl);

        var members = ChildrenOfKind(enumDecl, SyntaxKind.EnumMemberDeclaration);
        Assert.Equal(3, members.Count);
    }

    [Fact]
    public void ParseEmptyInput()
    {
        var tokens = new TokenBuilder().EOF.Build();
        var tree = Parse(tokens);

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        // Should have just EOF
        Assert.True(tree.SlotCount >= 1);
    }

    [Fact]
    public void TreePreservesAllBytes()
    {
        // Every token's FullWidth should be accounted for in the tree
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.NamespaceKeyword)  // 9
            .Identifier("Foo")                      // 3
            .OpenBrace                               // 1
            .CloseBrace                              // 1
            .EOF                                     // 0
            .Build();

        var tree = Parse(tokens);

        // FullWidth of tree should equal sum of all token widths
        int expectedWidth = tokens.Sum(t => t.FullWidth);
        Assert.Equal(expectedWidth, tree.FullWidth);
    }
}
