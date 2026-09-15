// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Syntax;
using Midl.Parser;

namespace Midl.Parser.Tests;

public class ParserCompletenessTests
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

    private static void CollectNodesOfKind(GreenNode node, SyntaxKind kind, List<GreenNode> results)
    {
        if (node.Kind == kind) results.Add(node);
        for (int i = 0; i < node.SlotCount; i++)
        {
            var child = node.GetSlot(i);
            if (child != null) CollectNodesOfKind(child, kind, results);
        }
    }

    // --- Test 1: ParsePartialRuntimeClass ---

    [Fact]
    public void ParsePartialRuntimeClass()
    {
        // partial runtimeclass C { void M(); }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.PartialKeyword)
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("C")
            .OpenBrace
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

        var modList = FindChild(rc, SyntaxKind.ModifierList);
        Assert.NotNull(modList);
        var partial = FindChild(modList, SyntaxKind.PartialKeyword);
        Assert.NotNull(partial);
    }

    // --- Test 2: ParseStaticRuntimeClass ---

    [Fact]
    public void ParseStaticRuntimeClass()
    {
        // static runtimeclass C { static void M(); }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.StaticKeyword)
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("C")
            .OpenBrace
            .Keyword(SyntaxKind.StaticKeyword)
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

        var modList = FindChild(rc, SyntaxKind.ModifierList);
        Assert.NotNull(modList);
        var staticMod = FindChild(modList, SyntaxKind.StaticKeyword);
        Assert.NotNull(staticMod);

        var methods = ChildrenOfKind(rc, SyntaxKind.MethodDeclaration);
        Assert.Single(methods);
    }

    // --- Test 3: ParsePropertySetAddition ---

    [Fact]
    public void ParsePropertySetAddition()
    {
        // runtimeclass C { String Name { get; }; String Name { set; }; }
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
            .Semicolon
            .Keyword(SyntaxKind.StringKeyword)
            .Identifier("Name")
            .OpenBrace
            .Keyword(SyntaxKind.SetKeyword)
            .Semicolon
            .CloseBrace
            .Semicolon
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);

        var props = ChildrenOfKind(rc, SyntaxKind.PropertyDeclaration);
        Assert.True(props.Count >= 2, $"Expected at least 2 PropertyDeclaration nodes, got {props.Count}");
    }

    // --- Test 4: ParseDefaultOnBaseList ---

    [Fact]
    public void ParseDefaultOnBaseList()
    {
        // runtimeclass C : [default] IFoo { }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("C")
            .Colon
            .OpenBracket
            .Token(SyntaxKind.DefaultKeyword, "default")
            .CloseBracket
            .Identifier("IFoo")
            .OpenBrace
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);

        // The [default] attribute should be captured somewhere in the tree
        var attr = FindChildRecursive(tree, SyntaxKind.AttributeList);
        Assert.NotNull(attr);
    }

    // --- Test 5: ParseDeclareBlock ---

    [Fact]
    public void ParseDeclareBlock()
    {
        // declare { interface IVector<String>; }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.DeclareKeyword)
            .OpenBrace
            .Keyword(SyntaxKind.InterfaceKeyword)
            .Identifier("IVector")
            .LessThan
            .Keyword(SyntaxKind.StringKeyword)
            .GreaterThan
            .Semicolon
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        var declareBlock = FindChild(tree, SyntaxKind.DeclareBlock);
        Assert.NotNull(declareBlock);
    }

    [Fact]
    public void ParseDeclareBlock_ParametricInterfaceSpecializations()
    {
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.DeclareKeyword)
            .OpenBrace
            .Keyword(SyntaxKind.InterfaceKeyword)
            .Identifier("IVector")
            .LessThan
            .Identifier("IFoo")
            .Token(SyntaxKind.AsteriskToken, "*")
            .GreaterThan
            .Semicolon
            .Keyword(SyntaxKind.InterfaceKeyword)
            .Identifier("IMap")
            .LessThan
            .Identifier("Key")
            .Comma
            .Identifier("Value")
            .Token(SyntaxKind.AsteriskToken, "*")
            .GreaterThan
            .Semicolon
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        var declareBlock = FindChild(tree, SyntaxKind.DeclareBlock);
        Assert.NotNull(declareBlock);

        var generics = new List<GreenNode>();
        CollectNodesOfKind(tree, SyntaxKind.GenericName, generics);
        Assert.True(generics.Count >= 2, $"Expected parametric names to fold into GenericName nodes, got {generics.Count}");

        var missing = new List<GreenNode>();
        CollectNodesOfKind(tree, SyntaxKind.MissingToken, missing);
        Assert.Empty(missing);

        var skipped = new List<GreenNode>();
        CollectNodesOfKind(tree, SyntaxKind.SkippedTokens, skipped);
        Assert.Empty(skipped);
    }

    // --- Test 6: ParseAttributeDefinition ---

    [Fact]
    public void ParseAttributeDefinition()
    {
        // [attributeusage(target_runtimeclass)] attribute MyAttr { Int32 Level; }
        // The parser captures the [attributeusage] attribute list in the tree
        var tokens = new TokenBuilder()
            .OpenBracket
            .Identifier("attributeusage")
            .OpenParen
            .Identifier("target_runtimeclass")
            .CloseParen
            .CloseBracket
            .Keyword(SyntaxKind.AttributeKeyword)
            .Identifier("MyAttr")
            .OpenBrace
            .Keyword(SyntaxKind.Int32Keyword)
            .Identifier("Level")
            .Semicolon
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        // Verify the [attributeusage] attribute is captured
        var attr = FindChildRecursive(tree, SyntaxKind.Attribute);
        Assert.NotNull(attr);
    }

    // --- Test 7: ParseContractOnEnumMember ---

    [Fact]
    public void ParseContractOnEnumMember()
    {
        // enum E { A, [contract(C, 2)] B }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.EnumKeyword)
            .Identifier("E")
            .OpenBrace
            .Identifier("A")
            .Comma
            .OpenBracket
            .Identifier("contract")
            .OpenParen
            .Identifier("C")
            .Comma
            .NumericLiteral("2")
            .CloseParen
            .CloseBracket
            .Identifier("B")
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        var enumDecl = FindChild(tree, SyntaxKind.EnumDeclaration);
        Assert.NotNull(enumDecl);

        var members = ChildrenOfKind(enumDecl, SyntaxKind.EnumMemberDeclaration);
        Assert.True(members.Count >= 2, $"Expected at least 2 enum members, got {members.Count}");

        // Second member should have an attribute
        var attrOnMember = FindChild(members[1], SyntaxKind.AttributeList);
        Assert.NotNull(attrOnMember);
    }

    // --- Test 8: ParseEmptyClassWithDefaultInterface ---

    [Fact]
    public void ParseEmptyClassWithDefaultInterface()
    {
        // [default_interface] runtimeclass Empty { }
        var tokens = new TokenBuilder()
            .OpenBracket
            .Identifier("default_interface")
            .CloseBracket
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("Empty")
            .OpenBrace
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);

        var attrList = FindChild(rc, SyntaxKind.AttributeList);
        Assert.NotNull(attrList);
    }

    // --- Test 9: ParseRefConstParam ---

    [Fact]
    public void ParseRefConstParam()
    {
        // runtimeclass C { Double Calc(ref const Windows.Foundation.Rect value); }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("C")
            .OpenBrace
            .Keyword(SyntaxKind.DoubleKeyword)
            .Identifier("Calc")
            .OpenParen
            .Keyword(SyntaxKind.RefKeyword)
            .Token(SyntaxKind.ConstKeyword, "const")
            .Identifier("Windows")
            .Dot
            .Identifier("Foundation")
            .Dot
            .Identifier("Rect")
            .Identifier("value")
            .CloseParen
            .Semicolon
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);

        // Verify both ref and const keywords are captured in the tree
        var refToken = FindChildRecursive(tree, SyntaxKind.RefKeyword);
        Assert.NotNull(refToken);
        var constToken = FindChildRecursive(tree, SyntaxKind.ConstKeyword);
        Assert.NotNull(constToken);
    }

    // --- Test 10: ParseArrayParam ---

    [Fact]
    public void ParseArrayParam()
    {
        // runtimeclass C { void SetBytes(UInt8[] bytes); }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("C")
            .OpenBrace
            .Keyword(SyntaxKind.VoidKeyword)
            .Identifier("SetBytes")
            .OpenParen
            .Token(SyntaxKind.UInt8Keyword, "UInt8")
            .OpenBracket
            .CloseBracket
            .Identifier("bytes")
            .CloseParen
            .Semicolon
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);

        var method = FindChild(rc, SyntaxKind.MethodDeclaration);
        Assert.NotNull(method);

        var arrayType = FindChildRecursive(method, SyntaxKind.ArrayType);
        Assert.NotNull(arrayType);
    }

    // --- Test 11: ParseOutArrayParam ---

    [Fact]
    public void ParseOutArrayParam()
    {
        // runtimeclass C { void GetBytes(out UInt8[] bytes); }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("C")
            .OpenBrace
            .Keyword(SyntaxKind.VoidKeyword)
            .Identifier("GetBytes")
            .OpenParen
            .Keyword(SyntaxKind.OutKeyword)
            .Token(SyntaxKind.UInt8Keyword, "UInt8")
            .OpenBracket
            .CloseBracket
            .Identifier("bytes")
            .CloseParen
            .Semicolon
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);

        var method = FindChild(rc, SyntaxKind.MethodDeclaration);
        Assert.NotNull(method);

        var arrayType = FindChildRecursive(method, SyntaxKind.ArrayType);
        Assert.NotNull(arrayType);

        var outKeyword = FindChildRecursive(method, SyntaxKind.OutKeyword);
        Assert.NotNull(outKeyword);
    }

    // --- Test 12: ParseRefArrayParam ---

    [Fact]
    public void ParseRefArrayParam()
    {
        // runtimeclass C { void ReadBytes(ref UInt8[] bytes); }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("C")
            .OpenBrace
            .Keyword(SyntaxKind.VoidKeyword)
            .Identifier("ReadBytes")
            .OpenParen
            .Keyword(SyntaxKind.RefKeyword)
            .Token(SyntaxKind.UInt8Keyword, "UInt8")
            .OpenBracket
            .CloseBracket
            .Identifier("bytes")
            .CloseParen
            .Semicolon
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);

        var method = FindChild(rc, SyntaxKind.MethodDeclaration);
        Assert.NotNull(method);

        var arrayType = FindChildRecursive(method, SyntaxKind.ArrayType);
        Assert.NotNull(arrayType);

        var refKeyword = FindChildRecursive(method, SyntaxKind.RefKeyword);
        Assert.NotNull(refKeyword);
    }

    // --- Test 13: ParseProtectedMember ---

    [Fact]
    public void ParseProtectedMember()
    {
        // unsealed runtimeclass C { protected void DoWork(); }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.UnsealedKeyword)
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("C")
            .OpenBrace
            .Keyword(SyntaxKind.ProtectedKeyword)
            .Keyword(SyntaxKind.VoidKeyword)
            .Identifier("DoWork")
            .OpenParen
            .CloseParen
            .Semicolon
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);

        // Class should have unsealed modifier
        var classModList = FindChild(rc, SyntaxKind.ModifierList);
        Assert.NotNull(classModList);
        var unsealed = FindChild(classModList, SyntaxKind.UnsealedKeyword);
        Assert.NotNull(unsealed);

        // Method should have protected modifier
        var method = FindChild(rc, SyntaxKind.MethodDeclaration);
        Assert.NotNull(method);
        var methodModList = FindChild(method, SyntaxKind.ModifierList);
        Assert.NotNull(methodModList);
        var protectedMod = FindChild(methodModList, SyntaxKind.ProtectedKeyword);
        Assert.NotNull(protectedMod);
    }

    // --- Test 14: ParseOverridableMember ---

    [Fact]
    public void ParseOverridableMember()
    {
        // unsealed runtimeclass C { overridable void OnChanged(); }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.UnsealedKeyword)
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("C")
            .OpenBrace
            .Keyword(SyntaxKind.OverridableKeyword)
            .Keyword(SyntaxKind.VoidKeyword)
            .Identifier("OnChanged")
            .OpenParen
            .CloseParen
            .Semicolon
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);

        var method = FindChild(rc, SyntaxKind.MethodDeclaration);
        Assert.NotNull(method);

        var methodModList = FindChild(method, SyntaxKind.ModifierList);
        Assert.NotNull(methodModList);
        var overridable = FindChild(methodModList, SyntaxKind.OverridableKeyword);
        Assert.NotNull(overridable);
    }

    // --- Test 15: ParseEventWithGenericDelegate ---

    [Fact]
    public void ParseEventWithGenericDelegate()
    {
        // runtimeclass C { event Windows.Foundation.TypedEventHandler<C, Object> Changed; }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("C")
            .OpenBrace
            .Keyword(SyntaxKind.EventKeyword)
            .Identifier("Windows")
            .Dot
            .Identifier("Foundation")
            .Dot
            .Identifier("TypedEventHandler")
            .LessThan
            .Identifier("C")
            .Comma
            .Keyword(SyntaxKind.ObjectKeyword)
            .GreaterThan
            .Identifier("Changed")
            .Semicolon
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);

        var evt = FindChild(rc, SyntaxKind.EventDeclaration);
        Assert.NotNull(evt);

        var genericName = FindChildRecursive(evt, SyntaxKind.GenericName);
        Assert.NotNull(genericName);
    }

    // --- Test 16: ParseMultipleInterfaceRequires ---

    [Fact]
    public void ParseMultipleInterfaceRequires()
    {
        // interface ICombo requires IText, IList { void Do(); }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.InterfaceKeyword)
            .Identifier("ICombo")
            .Identifier("requires")
            .Identifier("IText")
            .Comma
            .Identifier("IList")
            .OpenBrace
            .Keyword(SyntaxKind.VoidKeyword)
            .Identifier("Do")
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

        // Verify multiple required interfaces
        var typeRefs = ChildrenOfKind(requires, SyntaxKind.TypeReference);
        Assert.True(typeRefs.Count >= 2, $"Expected at least 2 required interfaces, got {typeRefs.Count}");

        var methods = ChildrenOfKind(iface, SyntaxKind.MethodDeclaration);
        Assert.Single(methods);
    }

    // --- Test 17: ParseNestedGenerics ---

    [Fact]
    public void ParseNestedGenerics()
    {
        // runtimeclass C { Windows.Foundation.IAsyncOperation<IVector<String> > GetAsync(); }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("C")
            .OpenBrace
            .Identifier("Windows")
            .Dot
            .Identifier("Foundation")
            .Dot
            .Identifier("IAsyncOperation")
            .LessThan
            .Identifier("IVector")
            .LessThan
            .Keyword(SyntaxKind.StringKeyword)
            .GreaterThan
            .GreaterThan
            .Identifier("GetAsync")
            .OpenParen
            .CloseParen
            .Semicolon
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);

        var method = FindChild(rc, SyntaxKind.MethodDeclaration);
        Assert.NotNull(method);

        // Should have nested generic types
        var genericNames = new List<GreenNode>();
        CollectNodesOfKind(tree, SyntaxKind.GenericName, genericNames);
        Assert.True(genericNames.Count >= 2, $"Expected at least 2 GenericName nodes for nested generics, got {genericNames.Count}");
    }

    // --- Test 18: ParseContractVersionedBlock ---

    [Fact]
    public void ParseContractVersionedBlock()
    {
        // [contract(UAC, 1)] runtimeclass C { void A(); [contract(UAC, 2)] { void B(); } }
        var tokens = new TokenBuilder()
            .OpenBracket
            .Identifier("contract")
            .OpenParen
            .Identifier("UAC")
            .Comma
            .NumericLiteral("1")
            .CloseParen
            .CloseBracket
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("C")
            .OpenBrace
            .Keyword(SyntaxKind.VoidKeyword)
            .Identifier("A")
            .OpenParen
            .CloseParen
            .Semicolon
            .OpenBracket
            .Identifier("contract")
            .OpenParen
            .Identifier("UAC")
            .Comma
            .NumericLiteral("2")
            .CloseParen
            .CloseBracket
            .OpenBrace
            .Keyword(SyntaxKind.VoidKeyword)
            .Identifier("B")
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

        // Should have a direct method A
        var methods = ChildrenOfKind(rc, SyntaxKind.MethodDeclaration);
        Assert.Single(methods);

        // Should have a versioned block
        var vb = FindChild(rc, SyntaxKind.VersionedBlock);
        Assert.NotNull(vb);

        // Versioned block should contain method B
        var vbMethods = ChildrenOfKind(vb, SyntaxKind.MethodDeclaration);
        Assert.Single(vbMethods);
    }

    // --- Test 19: ParseConstructorOverloads ---

    [Fact]
    public void ParseConstructorOverloads()
    {
        // runtimeclass C { C(); C(Int32 x); C(Int32 x, String y); }
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.RuntimeclassKeyword)
            .Identifier("C")
            .OpenBrace
            .Identifier("C")
            .OpenParen
            .CloseParen
            .Semicolon
            .Identifier("C")
            .OpenParen
            .Keyword(SyntaxKind.Int32Keyword)
            .Identifier("x")
            .CloseParen
            .Semicolon
            .Identifier("C")
            .OpenParen
            .Keyword(SyntaxKind.Int32Keyword)
            .Identifier("x")
            .Comma
            .Keyword(SyntaxKind.StringKeyword)
            .Identifier("y")
            .CloseParen
            .Semicolon
            .CloseBrace
            .EOF
            .Build();

        var tree = Parse(tokens);

        var rc = FindChild(tree, SyntaxKind.RuntimeClassDeclaration);
        Assert.NotNull(rc);

        var ctors = ChildrenOfKind(rc, SyntaxKind.ConstructorDeclaration);
        Assert.True(ctors.Count >= 3, $"Expected at least 3 constructors, got {ctors.Count}");
    }

    // --- Test 20: ParseImportDirective ---

    [Fact]
    public void ParseImportDirective()
    {
        // import "Windows.Foundation.idl";
        var tokens = new TokenBuilder()
            .Keyword(SyntaxKind.ImportKeyword)
            .StringLiteral("Windows.Foundation.idl")
            .Semicolon
            .EOF
            .Build();

        var tree = Parse(tokens);

        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
        var import = FindChild(tree, SyntaxKind.ImportDirective);
        Assert.NotNull(import);

        // Verify string literal is present
        var strLit = FindChild(import, SyntaxKind.StringLiteralToken);
        Assert.NotNull(strLit);
    }
}
