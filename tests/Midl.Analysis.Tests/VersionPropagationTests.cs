using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis.Tests;

public class VersionPropagationTests
{
    [Fact]
    public void ContractPropagatedToSynthesizedInterface()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [contractversion(3)]
                apicontract TestContract { }

                [contract(TestContract, 1)]
                runtimeclass Widget
                {
                    void DoSomething();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var widget = ns.Members.OfType<RuntimeClassSymbol>().Single();

        Assert.NotNull(widget.DefaultInterface);
        Assert.Equal("TestContract", widget.DefaultInterface.ContractName);
        Assert.Equal(1, widget.DefaultInterface.ContractVersion);
    }

    [Fact]
    public void MemberInheritsClassContract()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [contractversion(3)]
                apicontract TestContract { }

                [contract(TestContract, 1)]
                runtimeclass Widget
                {
                    void DoSomething();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var widget = ns.Members.OfType<RuntimeClassSymbol>().Single();

        var method = widget.Members.OfType<MethodSymbol>().Single();
        Assert.Equal("TestContract", method.ContractName);
        Assert.Equal(1, method.ContractVersion);
    }

    [Fact]
    public void VersionedBlockOverridesClassContract()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [contractversion(3)]
                apicontract TestContract { }

                [contract(TestContract, 1)]
                runtimeclass Widget
                {
                    void MethodA();

                    [contract(TestContract, 2)] {
                        void MethodB();
                    }
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var widget = ns.Members.OfType<RuntimeClassSymbol>().Single();

        var methodA = widget.Members.OfType<MethodSymbol>().First(m => m.Name == "MethodA");
        Assert.Equal("TestContract", methodA.ContractName);
        Assert.Equal(1, methodA.ContractVersion);

        var methodB = widget.Members.OfType<MethodSymbol>().First(m => m.Name == "MethodB");
        Assert.Equal("TestContract", methodB.ContractName);
        Assert.Equal(2, methodB.ContractVersion);
    }

    [Fact]
    public void InterfaceVersionValidation()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [contractversion(3)]
                apicontract TestContract { }

                [contract(TestContract, 1)]
                interface IEarly { }

                [contract(TestContract, 2)]
                runtimeclass Widget : IEarly
                {
                    void DoSomething();
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1014");
        Assert.Contains("IEarly", diag.Message);
        Assert.Contains("version 1", diag.Message);
        Assert.Contains("version 2", diag.Message);
    }

    [Fact]
    public void ContractFamilyValidation()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [contractversion(3)]
                apicontract ContractA { }

                [contractversion(3)]
                apicontract ContractB { }

                [contract(ContractB, 1)]
                interface IOther { }

                [contract(ContractA, 1)]
                runtimeclass Widget : IOther
                {
                    void DoSomething();
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1015");
        Assert.Contains("IOther", diag.Message);
        Assert.Contains("ContractB", diag.Message);
        Assert.Contains("Widget", diag.Message);
    }

    [Fact]
    public void EnumMemberVersionValidation()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [contractversion(5)]
                apicontract TestContract { }

                [contract(TestContract, 3)]
                enum Colors
                {
                    [contract(TestContract, 2)]
                    Red = 0,
                    Green = 1,
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1017");
        Assert.Contains("Red", diag.Message);
        Assert.Contains("version 2", diag.Message);
        Assert.Contains("version 3", diag.Message);
    }

    [Fact]
    public void ValidVersionsProduceNoDiagnostic()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [contractversion(5)]
                apicontract TestContract { }

                [contract(TestContract, 1)]
                interface IWidget
                {
                    void Foo();
                }

                [contract(TestContract, 1)]
                runtimeclass Widget : IWidget
                {
                    void DoSomething();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
    }

    [Fact]
    public void UndefinedContractDiagnostic()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [contract(NonExistent, 1)]
                runtimeclass Widget
                {
                    void DoSomething();
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1016");
        Assert.Contains("NonExistent", diag.Message);
    }
}
