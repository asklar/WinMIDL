using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis.Tests;

public class SmokeTests
{
    [Fact]
    public void ProjectLoads()
    {
        Assert.True(true, "Analysis test project loaded successfully");
    }
}

public class BinderTests
{
    #region Namespace Tests

    [Fact]
    public void BindSimpleNamespace()
    {
        var compilation = CompilationHelper.Compile("""
            namespace Foo
            {
                [default_interface] runtimeclass Bar { }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var fooNs = compilation.GlobalNamespace.ChildNamespaces["Foo"];
        Assert.NotNull(fooNs);
        Assert.Equal("Foo", fooNs.Name);

        var bar = fooNs.Members.OfType<RuntimeClassSymbol>().Single();
        Assert.Equal("Bar", bar.Name);
        Assert.Equal("Foo.Bar", bar.FullName);
    }

    [Fact]
    public void NestedNamespaces()
    {
        var compilation = CompilationHelper.Compile("""
            namespace A.B
            {
                [default_interface] runtimeclass X { }
            }
            namespace A.B.C
            {
                [default_interface] runtimeclass Y { }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var aNs = compilation.GlobalNamespace.ChildNamespaces["A"];
        Assert.NotNull(aNs);
        Assert.Equal("A", aNs.Name);

        var bNs = aNs.ChildNamespaces["B"];
        Assert.NotNull(bNs);
        Assert.Equal("B", bNs.Name);
        Assert.Equal("A.B", bNs.FullName);

        var x = bNs.Members.OfType<RuntimeClassSymbol>().Single();
        Assert.Equal("X", x.Name);
        Assert.Equal("A.B.X", x.FullName);

        var cNs = bNs.ChildNamespaces["C"];
        Assert.NotNull(cNs);
        Assert.Equal("C", cNs.Name);
        Assert.Equal("A.B.C", cNs.FullName);

        var y = cNs.Members.OfType<RuntimeClassSymbol>().Single();
        Assert.Equal("Y", y.Name);
        Assert.Equal("A.B.C.Y", y.FullName);
    }

    #endregion

    #region RuntimeClass Tests

    [Fact]
    public void BindRuntimeClassWithMembers()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                delegate void MyHandler(String message);

                runtimeclass Widget
                {
                    Widget();
                    void DoWork(Int32 count);
                    String Title { get; set; };
                    event MyHandler Changed;
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var widget = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "Widget");

        // Constructor
        var ctor = widget.Members.OfType<ConstructorSymbol>().Single();
        Assert.Equal("Widget", ctor.Name);
        Assert.Empty(ctor.Parameters);

        // Method
        var method = widget.Members.OfType<MethodSymbol>().Single();
        Assert.Equal("DoWork", method.Name);
        Assert.Equal("void", method.ReturnType.Name);
        Assert.True(method.ReturnType.IsBuiltIn);
        Assert.Single(method.Parameters);
        Assert.Equal("count", method.Parameters[0].Name);
        Assert.Equal("Int32", method.Parameters[0].Type.Name);

        // Property
        var prop = widget.Members.OfType<PropertySymbol>().Single();
        Assert.Equal("Title", prop.Name);
        Assert.Equal("String", prop.Type.Name);
        Assert.True(prop.HasGetter);
        Assert.True(prop.HasSetter);

        // Event
        var evt = widget.Members.OfType<EventSymbol>().Single();
        Assert.Equal("Changed", evt.Name);
        Assert.Equal("MyHandler", evt.DelegateType.Name);
    }

    [Fact]
    public void BindRuntimeClassWithModifiers()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                static runtimeclass Helpers
                {
                    static Int32 Compute();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var helpers = ns.Members.OfType<RuntimeClassSymbol>().Single();
        Assert.True(helpers.IsStatic);

        var method = helpers.Members.OfType<MethodSymbol>().Single();
        Assert.True(method.IsStatic);
    }

    [Fact]
    public void BindRuntimeClassReadOnlyProperty()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass Item
                {
                    String Name { get; };
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var item = ns.Members.OfType<RuntimeClassSymbol>().Single();
        var prop = item.Members.OfType<PropertySymbol>().Single();
        Assert.True(prop.HasGetter);
        Assert.False(prop.HasSetter);
    }

    #endregion

    #region Interface Tests

    [Fact]
    public void BindInterface()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                interface IWidget
                {
                    void Activate();
                    String Name { get; };
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var iface = ns.Members.OfType<InterfaceSymbol>().Single();
        Assert.Equal("IWidget", iface.Name);
        Assert.Equal("TestApp.IWidget", iface.FullName);

        var method = iface.Members.OfType<MethodSymbol>().Single();
        Assert.Equal("Activate", method.Name);
        Assert.Equal("void", method.ReturnType.Name);

        var prop = iface.Members.OfType<PropertySymbol>().Single();
        Assert.Equal("Name", prop.Name);
    }

    #endregion

    #region Struct Tests

    [Fact]
    public void BindStruct()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                struct Point
                {
                    Single X;
                    Single Y;
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var s = ns.Members.OfType<StructSymbol>().Single();
        Assert.Equal("Point", s.Name);
        Assert.Equal(2, s.Fields.Count);
        Assert.Equal("X", s.Fields[0].Name);
        Assert.Equal("Single", s.Fields[0].Type.Name);
        Assert.Equal("Y", s.Fields[1].Name);
        Assert.Equal("Single", s.Fields[1].Type.Name);
    }

    #endregion

    #region Enum Tests

    [Fact]
    public void BindEnum()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                enum Color
                {
                    Red,
                    Green,
                    Blue,
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var e = ns.Members.OfType<EnumSymbol>().Single();
        Assert.Equal("Color", e.Name);
        Assert.Equal(3, e.EnumMembers.Count);
        Assert.Equal("Red", e.EnumMembers[0].Name);
        Assert.Equal("Green", e.EnumMembers[1].Name);
        Assert.Equal("Blue", e.EnumMembers[2].Name);
    }

    #endregion

    #region Type Resolution Tests

    [Fact]
    public void ResolveQualifiedType()
    {
        var compilation = CompilationHelper.Compile("""
            namespace Windows.Foundation
            {
                [default_interface] runtimeclass Uri { }
            }
            namespace MyApp
            {
                runtimeclass Browser
                {
                    Windows.Foundation.Uri GetUri();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var myApp = compilation.GlobalNamespace.ChildNamespaces["MyApp"];
        var browser = myApp.Members.OfType<RuntimeClassSymbol>().Single();
        var method = browser.Members.OfType<MethodSymbol>().Single();

        Assert.Equal("Windows.Foundation.Uri", method.ReturnType.Name);
        Assert.NotNull(method.ReturnType.ResolvedSymbol);
        Assert.Equal("Uri", method.ReturnType.ResolvedSymbol!.Name);
        Assert.IsType<RuntimeClassSymbol>(method.ReturnType.ResolvedSymbol);
    }

    [Fact]
    public void ResolveShorthandType()
    {
        var compilation = CompilationHelper.Compile("""
            namespace Windows.Foundation.Collections
            {
                interface IVector
                {
                }
            }
            namespace MyApp
            {
                runtimeclass Library
                {
                    IVector GetItems();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var myApp = compilation.GlobalNamespace.ChildNamespaces["MyApp"];
        var lib = myApp.Members.OfType<RuntimeClassSymbol>().Single();
        var method = lib.Members.OfType<MethodSymbol>().Single();

        Assert.Equal("IVector", method.ReturnType.Name);
        Assert.NotNull(method.ReturnType.ResolvedSymbol);
        Assert.Equal("IVector", method.ReturnType.ResolvedSymbol!.Name);
        Assert.Equal("Windows.Foundation.Collections.IVector",
            method.ReturnType.ResolvedSymbol.FullName);
    }

    #endregion

    #region Diagnostic Tests

    [Fact]
    public void DiagnosticForUndefinedType()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass Widget
                {
                    NoSuchType GetThing();
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics);
        Assert.Equal("MIDL1001", diag.Code);
        Assert.Contains("NoSuchType", diag.Message);
        Assert.Equal(DiagnosticSeverity.Error, diag.Severity);
    }

    [Fact]
    public void DiagnosticForDuplicateDefinition()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [default_interface] runtimeclass Widget { }
                [default_interface] runtimeclass Widget { }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics);
        Assert.Equal("MIDL1002", diag.Code);
        Assert.Contains("Widget", diag.Message);
    }

    #endregion

    #region Compilation API Tests

    [Fact]
    public void GetTypeByNameFindsType()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass Widget { }
                interface IWidget { }
            }
            """);

        var widget = compilation.GetTypeByName("TestApp.Widget");
        Assert.NotNull(widget);
        Assert.IsType<RuntimeClassSymbol>(widget);

        var iWidget = compilation.GetTypeByName("TestApp.IWidget");
        Assert.NotNull(iWidget);
        Assert.IsType<InterfaceSymbol>(iWidget);

        Assert.Null(compilation.GetTypeByName("TestApp.NonExistent"));
    }

    [Fact]
    public void GetAllTypesReturnsAll()
    {
        var compilation = CompilationHelper.Compile("""
            namespace A
            {
                runtimeclass X { }
            }
            namespace B
            {
                struct Y { }
            }
            """);

        var types = compilation.GetAllTypes().ToList();
        Assert.Equal(2, types.Count);
        Assert.Contains(types, t => t.Name == "X");
        Assert.Contains(types, t => t.Name == "Y");
    }

    #endregion
}
