// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.Analysis.Symbols;

namespace Midl.Analysis.Tests;

public class NamedInterfaceTests
{
    [Fact]
    public void ConstructorName_SetsFactoryInterfaceName()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [default_interface]
                [constructor_name("TestApp.IWidgetFactory", 4e1a77a0-e6b1-4670-a970-22e5e5a6eed6)]
                runtimeclass Widget
                {
                    Widget(Int32 x);
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "Widget");

        Assert.NotNull(rc.FactoryInterface);
        Assert.Equal("IWidgetFactory", rc.FactoryInterface!.Name);
        Assert.Equal(Guid.Parse("4e1a77a0-e6b1-4670-a970-22e5e5a6eed6"), rc.FactoryInterface.Uuid);
        Assert.True(rc.FactoryInterface.IsExclusive);
        Assert.True(rc.FactoryInterface.IsSynthesized);
    }

    [Fact]
    public void InterfaceName_SetsDefaultInterfaceName()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [interface_name("TestApp.IWidget", b5f2e28a-1a3c-4c77-b9e7-dd4a8c1a7b5e)]
                runtimeclass Widget
                {
                    void DoWork();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "Widget");

        Assert.NotNull(rc.DefaultInterface);
        Assert.Equal("IWidget", rc.DefaultInterface!.Name);
        Assert.Equal(Guid.Parse("b5f2e28a-1a3c-4c77-b9e7-dd4a8c1a7b5e"), rc.DefaultInterface.Uuid);
        Assert.True(rc.DefaultInterface.IsExclusive);
        Assert.True(rc.DefaultInterface.IsSynthesized);
    }

    [Fact]
    public void StaticName_SetsStaticsInterfaceName()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [static_name("TestApp.IWidgetStatics", 8c9a1f3d-2b4e-4d6f-a1c8-3e7f9d2b5a4c)]
                runtimeclass Widget
                {
                    static void Helper();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "Widget");

        Assert.Single(rc.StaticsInterfaces);
        var statics = rc.StaticsInterfaces[0];
        Assert.Equal("IWidgetStatics", statics.Name);
        Assert.Equal(Guid.Parse("8c9a1f3d-2b4e-4d6f-a1c8-3e7f9d2b5a4c"), statics.Uuid);
        Assert.True(statics.IsExclusive);
        Assert.True(statics.IsSynthesized);
    }

    [Fact]
    public void NamedInterfaces_WithoutGuid_UsesNameOnly()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [interface_name("TestApp.IMyWidget")]
                runtimeclass Widget
                {
                    void DoWork();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "Widget");

        Assert.NotNull(rc.DefaultInterface);
        Assert.Equal("IMyWidget", rc.DefaultInterface!.Name);
        Assert.Null(rc.DefaultInterface.Uuid);
    }

    [Fact]
    public void AllThreeNamedInterfaces_Together()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [default_interface]
                [constructor_name("TestApp.IWidgetFactory", 11111111-1111-1111-1111-111111111111)]
                [interface_name("TestApp.IWidget", 22222222-2222-2222-2222-222222222222)]
                [static_name("TestApp.IWidgetStatics", 33333333-3333-3333-3333-333333333333)]
                runtimeclass Widget
                {
                    Widget(Int32 x);
                    void DoWork();
                    static void Helper();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "Widget");

        Assert.NotNull(rc.FactoryInterface);
        Assert.Equal("IWidgetFactory", rc.FactoryInterface!.Name);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), rc.FactoryInterface.Uuid);

        Assert.NotNull(rc.DefaultInterface);
        Assert.Equal("IWidget", rc.DefaultInterface!.Name);
        Assert.Equal(Guid.Parse("22222222-2222-2222-2222-222222222222"), rc.DefaultInterface.Uuid);

        Assert.Single(rc.StaticsInterfaces);
        Assert.Equal("IWidgetStatics", rc.StaticsInterfaces[0].Name);
        Assert.Equal(Guid.Parse("33333333-3333-3333-3333-333333333333"), rc.StaticsInterfaces[0].Uuid);
    }

    [Fact]
    public void NamedInterface_SimpleNameExtracted()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [interface_name("ICustomName")]
                runtimeclass Widget
                {
                    void DoWork();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "Widget");

        Assert.NotNull(rc.DefaultInterface);
        Assert.Equal("ICustomName", rc.DefaultInterface!.Name);
    }

    [Fact]
    public void NamedInterfaces_PropertiesStored()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [constructor_name("TestApp.IFooFactory", 11111111-1111-1111-1111-111111111111)]
                [interface_name("TestApp.IFoo", 22222222-2222-2222-2222-222222222222)]
                [static_name("TestApp.IFooStatics", 33333333-3333-3333-3333-333333333333)]
                runtimeclass Foo
                {
                    void M();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "Foo");

        // Verify the properties are stored on the symbol
        Assert.Equal("TestApp.IFooFactory", rc.FactoryInterfaceName);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), rc.FactoryInterfaceGuid);
        Assert.Equal("TestApp.IFoo", rc.DefaultInterfaceName);
        Assert.Equal(Guid.Parse("22222222-2222-2222-2222-222222222222"), rc.DefaultInterfaceGuid);
        Assert.Equal("TestApp.IFooStatics", rc.StaticsInterfaceName);
        Assert.Equal(Guid.Parse("33333333-3333-3333-3333-333333333333"), rc.StaticsInterfaceGuid);
    }

    [Fact]
    public void WithoutNamedInterfaces_DefaultNamesUsed()
    {
        // Verify that without the attributes, default naming still works
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [default_interface]
                runtimeclass Widget
                {
                    Widget(Int32 x);
                    void DoWork();
                    static void Helper();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "Widget");

        Assert.NotNull(rc.DefaultInterface);
        Assert.Equal("IWidget", rc.DefaultInterface!.Name);
        Assert.Null(rc.DefaultInterface.Uuid);

        Assert.NotNull(rc.FactoryInterface);
        Assert.Equal("IWidgetFactory", rc.FactoryInterface!.Name);
        Assert.Null(rc.FactoryInterface.Uuid);

        Assert.Single(rc.StaticsInterfaces);
        Assert.Equal("IWidgetStatics", rc.StaticsInterfaces[0].Name);
        Assert.Null(rc.StaticsInterfaces[0].Uuid);
    }
}
