// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
        Assert.Equal("TestApp.C", rc.DefaultInterface.ExclusiveToClass);

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

        Assert.Single(rc.StaticsInterfaces);
        var staticsIface = rc.StaticsInterfaces[0];
        Assert.Equal("ICStatics", staticsIface.Name);
        Assert.True(staticsIface.IsExclusive);
        Assert.True(staticsIface.IsSynthesized);

        var method = staticsIface.Members.OfType<MethodSymbol>().Single();
        Assert.Equal("S", method.Name);
    }

    [Fact]
    public void SynthesizeVersionedInterfaces()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [contractversion(3)]
                apicontract TestContract {}

                [contract(TestContract, 1)]
                runtimeclass Service
                {
                    void V1();

                    [contract(TestContract, 2)]
                    {
                        void V2();
                    }

                    [contract(TestContract, 3)]
                    {
                        void V3();
                        static void S3();
                    }
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var runtimeClass = ns.Members.OfType<RuntimeClassSymbol>().Single();
        var instanceInterfaces = runtimeClass.ImplementedInterfaces
            .Where(iface => !runtimeClass.StaticsInterfaces.Contains(iface))
            .ToList();

        Assert.Equal(new[] { "IService", "IService2", "IService3" },
            instanceInterfaces.Select(iface => iface.Name));
        Assert.Equal(new[] { "V1", "V2", "V3" },
            instanceInterfaces.SelectMany(iface => iface.Members).Select(member => member.Name));
        Assert.Equal(new int?[] { 1 << 16, 2 << 16, 3 << 16 },
            instanceInterfaces.Select(iface => iface.ContractVersion));

        var statics = Assert.Single(runtimeClass.StaticsInterfaces);
        Assert.Equal("IServiceStatics", statics.Name);
        Assert.Equal(3 << 16, statics.ContractVersion);
        Assert.Equal("S3", Assert.Single(statics.Members).Name);
    }

    [Fact]
    public void SynthesizeVersionedModifierInterfaces()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [contractversion(2)]
                apicontract TestContract {}

                [contract(TestContract, 1)]
                unsealed runtimeclass Widget
                {
                    void InstanceV1();
                    overridable void OverrideV1();

                    [contract(TestContract, 2)]
                    {
                        protected void ProtectedV2();
                        overridable void OverrideV2();
                    }
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var runtimeClass = ns.Members.OfType<RuntimeClassSymbol>().Single();

        Assert.Equal(new[] { "IWidgetProtected" },
            runtimeClass.ProtectedInterfaces.Select(iface => iface.Name));
        Assert.Equal(2 << 16,
            Assert.Single(runtimeClass.ProtectedInterfaces).ContractVersion);
        Assert.Equal("ProtectedV2",
            Assert.Single(Assert.Single(runtimeClass.ProtectedInterfaces).Members).Name);

        Assert.Equal(new[] { "IWidgetOverrides", "IWidgetOverrides2" },
            runtimeClass.OverridesInterfaces.Select(iface => iface.Name));
        Assert.Equal(new int?[] { 1 << 16, 2 << 16 },
            runtimeClass.OverridesInterfaces.Select(iface => iface.ContractVersion));
        Assert.Equal(new[] { "OverrideV1", "OverrideV2" },
            runtimeClass.OverridesInterfaces.SelectMany(iface => iface.Members).Select(member => member.Name));
    }

    [Theory]
    [InlineData("C", "Review.C")]
    [InlineData("Review.C", "C")]
    public void EquivalentContractNamesShareInterface(
        string classContract,
        string memberContract)
    {
        var compilation = CompilationHelper.Compile($$"""
            namespace Review
            {
                [contractversion(1)]
                apicontract C {}

                [contract({{classContract}}, 1)]
                runtimeclass Widget
                {
                    void A();
                    [contract({{memberContract}}, 1)]
                    {
                        void B();
                    }
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["Review"];
        var runtimeClass = ns.Members.OfType<RuntimeClassSymbol>().Single();
        var instanceInterfaces = runtimeClass.ImplementedInterfaces
            .Where(iface => !runtimeClass.FactoryInterfaces.Contains(iface)
                && !runtimeClass.StaticsInterfaces.Contains(iface)
                && !runtimeClass.ProtectedInterfaces.Contains(iface)
                && !runtimeClass.OverridesInterfaces.Contains(iface))
            .ToList();

        var iface = Assert.Single(instanceInterfaces);
        Assert.Equal(new[] { "A", "B" }, iface.Members.Select(member => member.Name));
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
    public void SynthesizeProtectedInterface_SealedClass()
    {
        // Original midl.exe emits IClassProtected even on sealed classes
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass C
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
        // When all members are protected, the protected interface becomes the default
        Assert.Same(rc.ProtectedInterface, rc.DefaultInterface);
    }

    [Fact]
    public void SynthesizeProtectedAndDefault_SealedClass()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass C
                {
                    void A();
                    protected void B();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "C");

        Assert.NotNull(rc.DefaultInterface);
        Assert.Equal("IC", rc.DefaultInterface!.Name);
        Assert.Single(rc.DefaultInterface.Members.OfType<MethodSymbol>());

        Assert.NotNull(rc.ProtectedInterface);
        Assert.Equal("ICProtected", rc.ProtectedInterface!.Name);
        Assert.Single(rc.ProtectedInterface.Members.OfType<MethodSymbol>());
        Assert.NotSame(rc.DefaultInterface, rc.ProtectedInterface);
    }

    [Fact]
    public void SynthesizeOverridableInterface_SealedClass()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass C
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
        Assert.Equal("TestApp.Widget", rc.DefaultInterface.ExclusiveToClass);

        Assert.Single(rc.StaticsInterfaces);
        Assert.True(rc.StaticsInterfaces[0].IsExclusive);
        Assert.Equal("TestApp.Widget", rc.StaticsInterfaces[0].ExclusiveToClass);
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
        Assert.Empty(rc.StaticsInterfaces);
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

        // IFoo is in the base list but is an interface — after reclassification it
        // moves into Interfaces (the class still derives from Object).
        Assert.Null(rc.BaseClass);
        Assert.Contains(rc.Interfaces, i => i.Name == "IFoo");
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

        Assert.Single(rc.StaticsInterfaces);
        Assert.Equal("ICStatics", rc.StaticsInterfaces[0].Name);
        var staticMethod = rc.StaticsInterfaces[0].Members.OfType<MethodSymbol>().Single();
        Assert.Equal("StaticMethod", staticMethod.Name);

        // ImplementedInterfaces should have both
        Assert.Equal(2, rc.ImplementedInterfaces.Count);
    }
}
