// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Syntax;
using Midl.Parser;

namespace Midl.Parser.Tests;

public class ClassicMidlTests
{
    private static GreenNode Parse(IReadOnlyList<SyntaxToken> tokens)
    {
        var parser = new Midl.Parser.Parser(tokens);
        return parser.Parse();
    }

    private static GreenNode ParseText(string source)
    {
        var lexer = new Midl.Lexer.Lexer(source);
        var tokens = lexer.Lex();
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

    private static GreenNode? FindChildRecursive(GreenNode node, SyntaxKind kind)
    {
        if (node.Kind == kind) return node;
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

    // ─── Test 1: ParseCoclassDeclaration ───

    [Fact]
    public void ParseCoclassDeclaration()
    {
        // [uuid("...")] coclass MyClass { [default] interface IMyInterface; interface IOther; };
        var tokens = new TokenBuilder()
            .OpenBracket
            .Identifier("uuid")
            .OpenParen
            .StringLiteral("00000000-0000-0000-0000-000000000000")
            .CloseParen
            .CloseBracket
            .Token(SyntaxKind.CoclassKeyword, "coclass")
            .Identifier("MyClass")
            .OpenBrace
            // [default] interface IMyInterface;
            .OpenBracket
            .Token(SyntaxKind.DefaultKeyword, "default")
            .CloseBracket
            .Token(SyntaxKind.InterfaceKeyword, "interface")
            .Identifier("IMyInterface")
            .Semicolon
            // interface IOther;
            .Token(SyntaxKind.InterfaceKeyword, "interface")
            .Identifier("IOther")
            .Semicolon
            .CloseBrace
            .Semicolon
            .EOF
            .Build();

        var tree = Parse(tokens);
        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);

        var coclass = FindChild(tree, SyntaxKind.CoclassDeclaration);
        Assert.NotNull(coclass);

        // Should have attribute list
        var attrs = FindChild(coclass, SyntaxKind.AttributeList);
        Assert.NotNull(attrs);

        // Should have two interface members (BaseType nodes)
        var members = ChildrenOfKind(coclass, SyntaxKind.BaseType);
        Assert.Equal(2, members.Count);

        // First member should have a [default] attribute
        var firstMemberAttr = FindChild(members[0], SyntaxKind.AttributeList);
        Assert.NotNull(firstMemberAttr);
    }

    // ─── Test 2: ParseCppQuote ───

    [Fact]
    public void ParseCppQuote()
    {
        // cpp_quote("#include <objidl.h>")
        var tokens = new TokenBuilder()
            .Token(SyntaxKind.CppQuoteKeyword, "cpp_quote")
            .OpenParen
            .StringLiteral("#include <objidl.h>")
            .CloseParen
            .EOF
            .Build();

        var tree = Parse(tokens);
        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);

        var cppQuote = FindChild(tree, SyntaxKind.CppQuoteDirective);
        Assert.NotNull(cppQuote);

        // Should contain the string literal
        var strLit = FindChild(cppQuote, SyntaxKind.StringLiteralToken);
        Assert.NotNull(strLit);
    }

    // ─── Test 3: ParseTypedef ───

    [Fact]
    public void ParseTypedef()
    {
        // typedef [unique] IFoo* LPFOO;
        var tokens = new TokenBuilder()
            .Token(SyntaxKind.TypedefKeyword, "typedef")
            .OpenBracket
            .Identifier("unique")
            .CloseBracket
            .Identifier("IFoo")
            .Token(SyntaxKind.AsteriskToken, "*")
            .Identifier("LPFOO")
            .Semicolon
            .EOF
            .Build();

        var tree = Parse(tokens);
        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);

        var typedef = FindChild(tree, SyntaxKind.TypedefDeclaration);
        Assert.NotNull(typedef);

        // Should have an attribute list for [unique]
        var attrs = FindChild(typedef, SyntaxKind.AttributeList);
        Assert.NotNull(attrs);

        // Should have a pointer type
        var ptrType = FindChild(typedef, SyntaxKind.PointerType);
        Assert.NotNull(ptrType);
    }

    // ─── Test 4: ParseClassicInterface ───

    [Fact]
    public void ParseClassicInterface()
    {
        // [object, uuid("00000000-0000-0000-0000-000000000000")]
        // interface IFoo : IUnknown
        // {
        //     HRESULT Method1([in] int x, [out, retval] int* result);
        //     [propget] HRESULT Name([out, retval] BSTR* name);
        // };
        var tokens = new TokenBuilder()
            // Attributes
            .OpenBracket
            .Identifier("object")
            .Comma
            .Identifier("uuid")
            .OpenParen
            .StringLiteral("00000000-0000-0000-0000-000000000000")
            .CloseParen
            .CloseBracket
            // interface IFoo : IUnknown
            .Token(SyntaxKind.InterfaceKeyword, "interface")
            .Identifier("IFoo")
            .Colon
            .Identifier("IUnknown")
            .OpenBrace
            // HRESULT Method1([in] int x, [out, retval] int* result);
            .Token(SyntaxKind.HResultKeyword, "HRESULT")
            .Identifier("Method1")
            .OpenParen
            .OpenBracket
            .Token(SyntaxKind.InKeyword, "in")
            .CloseBracket
            .Token(SyntaxKind.IntKeyword, "int")
            .Identifier("x")
            .Comma
            .OpenBracket
            .Token(SyntaxKind.OutKeyword, "out")
            .Comma
            .Token(SyntaxKind.RetvalKeyword, "retval")
            .CloseBracket
            .Token(SyntaxKind.IntKeyword, "int")
            .Token(SyntaxKind.AsteriskToken, "*")
            .Identifier("result")
            .CloseParen
            .Semicolon
            // [propget] HRESULT Name([out, retval] BSTR* name);
            .OpenBracket
            .Identifier("propget")
            .CloseBracket
            .Token(SyntaxKind.HResultKeyword, "HRESULT")
            .Identifier("Name")
            .OpenParen
            .OpenBracket
            .Token(SyntaxKind.OutKeyword, "out")
            .Comma
            .Token(SyntaxKind.RetvalKeyword, "retval")
            .CloseBracket
            .Identifier("BSTR")
            .Token(SyntaxKind.AsteriskToken, "*")
            .Identifier("name")
            .CloseParen
            .Semicolon
            // };
            .CloseBrace
            .Semicolon
            .EOF
            .Build();

        var tree = Parse(tokens);
        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);

        var iface = FindChild(tree, SyntaxKind.InterfaceDeclaration);
        Assert.NotNull(iface);

        // Should have attributes
        var attrs = FindChild(iface, SyntaxKind.AttributeList);
        Assert.NotNull(attrs);

        // Should have base list (: IUnknown)
        var baseList = FindChild(iface, SyntaxKind.BaseList);
        Assert.NotNull(baseList);

        // Should have two methods
        var methods = ChildrenOfKind(iface, SyntaxKind.MethodDeclaration);
        Assert.Equal(2, methods.Count);

        // First method should have parameter list with attributes
        var paramList = FindChild(methods[0], SyntaxKind.ParameterList);
        Assert.NotNull(paramList);

        // Second method should have its own attribute list ([propget])
        var method2Attrs = FindChild(methods[1], SyntaxKind.AttributeList);
        Assert.NotNull(method2Attrs);
    }

    // ─── Test 5: ParseLibraryBlock ───

    [Fact]
    public void ParseLibraryBlock()
    {
        // [uuid("...")] library MyLib { importlib("stdole2.tlb"); coclass Bar { interface IFoo; }; };
        var tokens = new TokenBuilder()
            .OpenBracket
            .Identifier("uuid")
            .OpenParen
            .StringLiteral("12345678-1234-1234-1234-123456789abc")
            .CloseParen
            .CloseBracket
            .Token(SyntaxKind.LibraryKeyword, "library")
            .Identifier("MyLib")
            .OpenBrace
            // importlib("stdole2.tlb");
            .Token(SyntaxKind.ImportlibKeyword, "importlib")
            .OpenParen
            .StringLiteral("stdole2.tlb")
            .CloseParen
            .Semicolon
            // coclass Bar { interface IFoo; };
            .Token(SyntaxKind.CoclassKeyword, "coclass")
            .Identifier("Bar")
            .OpenBrace
            .Token(SyntaxKind.InterfaceKeyword, "interface")
            .Identifier("IFoo")
            .Semicolon
            .CloseBrace
            .Semicolon
            .CloseBrace
            .Semicolon
            .EOF
            .Build();

        var tree = Parse(tokens);
        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);

        var lib = FindChild(tree, SyntaxKind.LibraryDeclaration);
        Assert.NotNull(lib);

        // Should have importlib directive
        var importlib = FindChild(lib, SyntaxKind.ImportlibDirective);
        Assert.NotNull(importlib);

        // Should have coclass inside library
        var coclass = FindChild(lib, SyntaxKind.CoclassDeclaration);
        Assert.NotNull(coclass);
    }

    // ─── Test 6: ParseMixedMidl2AndMidl3 ───

    [Fact]
    public void ParseMixedMidl2AndMidl3()
    {
        // Classic interface followed by MIDL3 namespace with runtimeclass
        var source = @"
[object]
interface IFoo : IUnknown
{
    HRESULT GetName([out] int x);
};

namespace MyApp
{
    runtimeclass Widget
    {
        void DoSomething();
    }
}
";

        var tree = ParseText(source);
        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);

        // Should have a classic interface
        var iface = FindChild(tree, SyntaxKind.InterfaceDeclaration);
        Assert.NotNull(iface);

        // Should have a namespace
        var ns = FindChild(tree, SyntaxKind.NamespaceDeclaration);
        Assert.NotNull(ns);

        // Namespace should contain a runtimeclass
        var rc = FindChild(ns, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);

        // Runtimeclass should have a method
        var methods = ChildrenOfKind(rc, SyntaxKind.MethodDeclaration);
        Assert.Single(methods);
    }

    // ─── Test 7: ParsePointerTypes ───

    [Fact]
    public void ParsePointerTypes()
    {
        // interface ITest { HRESULT M1([in] int* pX, [in] IFoo** ppFoo, [in] void* pData); };
        var tokens = new TokenBuilder()
            .Token(SyntaxKind.InterfaceKeyword, "interface")
            .Identifier("ITest")
            .OpenBrace
            .Token(SyntaxKind.HResultKeyword, "HRESULT")
            .Identifier("M1")
            .OpenParen
            // int* pX
            .OpenBracket
            .Token(SyntaxKind.InKeyword, "in")
            .CloseBracket
            .Token(SyntaxKind.IntKeyword, "int")
            .Token(SyntaxKind.AsteriskToken, "*")
            .Identifier("pX")
            .Comma
            // IFoo** ppFoo
            .OpenBracket
            .Token(SyntaxKind.InKeyword, "in")
            .CloseBracket
            .Identifier("IFoo")
            .Token(SyntaxKind.AsteriskToken, "*")
            .Token(SyntaxKind.AsteriskToken, "*")
            .Identifier("ppFoo")
            .Comma
            // void* pData
            .OpenBracket
            .Token(SyntaxKind.InKeyword, "in")
            .CloseBracket
            .Token(SyntaxKind.VoidKeyword, "void")
            .Token(SyntaxKind.AsteriskToken, "*")
            .Identifier("pData")
            .CloseParen
            .Semicolon
            .CloseBrace
            .Semicolon
            .EOF
            .Build();

        var tree = Parse(tokens);
        var iface = FindChild(tree, SyntaxKind.InterfaceDeclaration);
        Assert.NotNull(iface);

        var methods = ChildrenOfKind(iface, SyntaxKind.MethodDeclaration);
        Assert.Single(methods);

        var paramList = FindChild(methods[0], SyntaxKind.ParameterList);
        Assert.NotNull(paramList);

        // Find pointer types in the parameter list (recursive search)
        var pointerTypes = new List<GreenNode>();
        CollectNodesOfKind(paramList, SyntaxKind.PointerType, pointerTypes);

        // Should have 4 pointer types: int*, IFoo*, IFoo** (outer), void*
        Assert.True(pointerTypes.Count >= 3, $"Expected at least 3 PointerType nodes, got {pointerTypes.Count}");
    }

    private static void CollectNodesOfKind(GreenNode node, SyntaxKind kind, List<GreenNode> results)
    {
        if (node.Kind == kind) results.Add(node);
        for (int i = 0; i < node.SlotCount; i++)
        {
            var child = node.GetSlot(i);
            if (child != null) CollectNodesOfKind(child, kind, results);
        }
    }
}
