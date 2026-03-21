using Midl.Analysis;
using Midl.Analysis.Symbols;

namespace Midl.Analysis.Tests;

public class InterfaceSynthesisTests
{
    [Fact]
    public void SynthesizeDefaultInterface()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass C
                {
                    void M();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "C");

        Assert.NotNull(rc.DefaultInterface);
        Assert.Equal("IC", rc.DefaultInterface!.Name);
        Assert.True(rc.DefaultInterface.IsExclusive);
        Assert.True(rc.DefaultInterface.IsSynthesized);
        Assert.Equal("C", rc.DefaultInterface.ExclusiveToClass);

        var method = rc.DefaultInterface.Members.OfType<MethodSymbol>().Single();
        Assert.Equal("M", method.Name);
    }

    [Fact]
    public void SynthesizeStaticInterface()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass C
                {
                    static void S();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "C");

        Assert.NotNull(rc.StaticsInterface);
        Assert.Equal("ICStatics", rc.StaticsInterface!.Name);
        Assert.True(rc.StaticsInterface.IsExclusive);
        Assert.True(rc.StaticsInterface.IsSynthesized);

        var method = rc.StaticsInterface.Members.OfType<MethodSymbol>().Single();
        Assert.Equal("S", method.Name);
    }

    [Fact]
    public void SynthesizeFactoryInterface()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [default_interface]
                runtimeclass C
                {
                    C(Int32 x);
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "C");

        Assert.NotNull(rc.FactoryInterface);
        Assert.Equal("ICFactory", rc.FactoryInterface!.Name);
        Assert.True(rc.FactoryInterface.IsExclusive);
        Assert.True(rc.FactoryInterface.IsSynthesized);

        var factoryMethod = rc.FactoryInterface.Members.OfType<MethodSymbol>().Single();
        Assert.Equal("CreateInstance", factoryMethod.Name);
        Assert.Single(factoryMethod.Parameters);
    }

    [Fact]
    public void SynthesizeDefaultActivation()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [default_interface]
                runtimeclass C
                {
                    C();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "C");

        Assert.True(rc.HasDefaultActivation);
    }

    [Fact]
    public void SynthesizeProtectedInterface()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                unsealed runtimeclass C
                {
                    protected void P();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "C");

        Assert.NotNull(rc.ProtectedInterface);
        Assert.Equal("ICProtected", rc.ProtectedInterface!.Name);
        Assert.True(rc.ProtectedInterface.IsExclusive);
        Assert.True(rc.ProtectedInterface.IsSynthesized);

        var method = rc.ProtectedInterface.Members.OfType<MethodSymbol>().Single();
        Assert.Equal("P", method.Name);
    }

    [Fact]
    public void SynthesizeOverridableInterface()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                unsealed runtimeclass C
                {
                    overridable void O();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "C");

        Assert.NotNull(rc.OverridesInterface);
        Assert.Equal("ICOverrides", rc.OverridesInterface!.Name);
        Assert.True(rc.OverridesInterface.IsExclusive);
        Assert.True(rc.OverridesInterface.IsSynthesized);

        var method = rc.OverridesInterface.Members.OfType<MethodSymbol>().Single();
        Assert.Equal("O", method.Name);
    }

    [Fact]
    public void DefaultInterfaceIsFirst()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass C
                {
                    void M();
                    static void S();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "C");

        // The instance interface (IC) should be the default, not ICStatics
        Assert.NotNull(rc.DefaultInterface);
        Assert.Equal("IC", rc.DefaultInterface!.Name);
    }

    [Fact]
    public void AutoNumberOnCollision()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                interface IC { }

                runtimeclass C
                {
                    void M();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "C");

        // IC already exists, so the synthesized interface should be IC2
        Assert.NotNull(rc.DefaultInterface);
        Assert.Equal("IC2", rc.DefaultInterface!.Name);
        Assert.True(rc.DefaultInterface.IsExclusive);
    }

    [Fact]
    public void ExclusiveToSetOnSynthesized()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass Widget
                {
                    void DoWork();
                    static void Helper();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "Widget");

        // All synthesized interfaces should have ExclusiveToClass set
        Assert.NotNull(rc.DefaultInterface);
        Assert.True(rc.DefaultInterface!.IsExclusive);
        Assert.Equal("Widget", rc.DefaultInterface.ExclusiveToClass);

        Assert.NotNull(rc.StaticsInterface);
        Assert.True(rc.StaticsInterface!.IsExclusive);
        Assert.Equal("Widget", rc.StaticsInterface.ExclusiveToClass);
    }

    [Fact]
    public void NoSynthesisForExplicitInterfaces()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                interface IFoo
                {
                    void DoSomething();
                }

                runtimeclass C : IFoo
                {
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "C");

        // No loose members, so no synthesis needed
        Assert.Null(rc.DefaultInterface);
        Assert.Null(rc.FactoryInterface);
        Assert.Null(rc.StaticsInterface);
        Assert.Empty(rc.ImplementedInterfaces);
    }

    [Fact]
    public void MixedMembersAndExplicit()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                interface IFoo
                {
                    void Original();
                }

                runtimeclass C : IFoo
                {
                    void Extra();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "C");

        // IC is synthesized for the loose Extra() member
        Assert.NotNull(rc.DefaultInterface);
        Assert.Equal("IC", rc.DefaultInterface!.Name);
        var method = rc.DefaultInterface.Members.OfType<MethodSymbol>().Single();
        Assert.Equal("Extra", method.Name);

        // IFoo is the first in base list, so treated as BaseClass by the binder
        Assert.NotNull(rc.BaseClass);
        Assert.Equal("IFoo", rc.BaseClass!.Name);
    }

    [Fact]
    public void StaticAndInstanceSeparated()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass C
                {
                    void InstanceMethod();
                    static void StaticMethod();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "C");

        // Should have both IC and ICStatics
        Assert.NotNull(rc.DefaultInterface);
        Assert.Equal("IC", rc.DefaultInterface!.Name);
        var instanceMethod = rc.DefaultInterface.Members.OfType<MethodSymbol>().Single();
        Assert.Equal("InstanceMethod", instanceMethod.Name);

        Assert.NotNull(rc.StaticsInterface);
        Assert.Equal("ICStatics", rc.StaticsInterface!.Name);
        var staticMethod = rc.StaticsInterface.Members.OfType<MethodSymbol>().Single();
        Assert.Equal("StaticMethod", staticMethod.Name);

        // ImplementedInterfaces should have both
        Assert.Equal(2, rc.ImplementedInterfaces.Count);
    }
}
