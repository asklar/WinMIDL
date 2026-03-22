using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis.Tests;

public class AttributeExtractionTests
{
    [Fact]
    public void ExtractContractAttribute()
    {
        var compilation = CompilationHelper.Compile("""
            namespace Windows.Foundation
            {
                [contractversion(10)]
                apicontract UniversalApiContract {};
            }
            namespace TestApp
            {
                [contract(Windows.Foundation.UniversalApiContract, 5), default_interface]
                runtimeclass Widget { }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var widget = ns.Members.OfType<RuntimeClassSymbol>().Single();

        Assert.Equal("Windows.Foundation.UniversalApiContract", widget.ContractName);
        Assert.Equal((5 << 16), widget.ContractVersion);
    }

    [Fact]
    public void ExtractContractVersionOnApiContract()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [contractversion(4)]
                apicontract TestContract { }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var contract = ns.Members.OfType<ApiContractSymbol>().Single();

        Assert.Equal((4 << 16), contract.ContractVersion);
    }

    [Fact]
    public void ExtractVersionAttribute()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [version(1), default_interface]
                runtimeclass Widget { }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var widget = ns.Members.OfType<RuntimeClassSymbol>().Single();

        Assert.Equal(65536, widget.Version); // (1 << 16) = 65536
    }

    [Fact]
    public void ExtractThreadingBoth()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [threading(both), default_interface]
                runtimeclass Widget { }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var widget = ns.Members.OfType<RuntimeClassSymbol>().Single();

        Assert.Equal("both", widget.ThreadingModel);
    }

    [Fact]
    public void ExtractMarshalingAgile()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [marshaling_behavior(agile), default_interface]
                runtimeclass Widget { }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var widget = ns.Members.OfType<RuntimeClassSymbol>().Single();

        Assert.Equal("agile", widget.MarshalingBehavior);
    }

    [Fact]
    public void ExtractMethodName()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass Widget
                {
                    [method_name("DoWorkWithLabel")]
                    void DoWork(String s);
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var widget = ns.Members.OfType<RuntimeClassSymbol>().Single();
        var method = widget.Members.OfType<MethodSymbol>().Single();

        Assert.Equal("DoWorkWithLabel", method.MethodNameOverride);
    }

    [Fact]
    public void ExtractOverload()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass Widget
                {
                    [overload("DoWork")]
                    void DoWork(Int32 x);
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var widget = ns.Members.OfType<RuntimeClassSymbol>().Single();
        var method = widget.Members.OfType<MethodSymbol>().Single();

        Assert.Equal("DoWork", method.OverloadGroup);
    }

    [Fact]
    public void ExtractDefaultOverload()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass Widget
                {
                    [default_overload]
                    void DoWork(String s);
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var widget = ns.Members.OfType<RuntimeClassSymbol>().Single();
        var method = widget.Members.OfType<MethodSymbol>().Single();

        Assert.True(method.IsDefaultOverload);
    }

    [Fact]
    public void ExtractReturnName()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass Widget
                {
                    [return_name("count")]
                    Int32 GetCount();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var widget = ns.Members.OfType<RuntimeClassSymbol>().Single();
        var method = widget.Members.OfType<MethodSymbol>().Single();

        Assert.Equal("count", method.ReturnNameOverride);
    }

    [Fact]
    public void ExtractRemoteAsync()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass Widget
                {
                    [remote_async]
                    void DoAsync();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var widget = ns.Members.OfType<RuntimeClassSymbol>().Single();
        var method = widget.Members.OfType<MethodSymbol>().Single();

        Assert.True(method.IsRemoteAsync);
    }

    [Fact]
    public void ExtractDeprecated()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [deprecated("Use NewWidget", deprecate, TestContract, 2), default_interface]
                runtimeclass Widget { }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var widget = ns.Members.OfType<RuntimeClassSymbol>().Single();

        Assert.Equal("Use NewWidget", widget.DeprecatedMessage);
    }

    [Fact]
    public void ExtractUuid()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [uuid("12345678-1234-1234-1234-123456789abc")]
                interface IWidget
                {
                    void DoWork();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var iface = ns.Members.OfType<InterfaceSymbol>().Single();

        Assert.Equal(Guid.Parse("12345678-1234-1234-1234-123456789abc"), iface.Uuid);
    }

    [Fact]
    public void ExtractMultipleAttributes()
    {
        var compilation = CompilationHelper.Compile("""
            namespace Windows.Foundation
            {
                [contractversion(10)]
                apicontract UniversalApiContract {};
            }
            namespace TestApp
            {
                [contract(Windows.Foundation.UniversalApiContract, 1), threading(both), default_interface]
                runtimeclass Widget { }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var widget = ns.Members.OfType<RuntimeClassSymbol>().Single();

        Assert.Equal("Windows.Foundation.UniversalApiContract", widget.ContractName);
        Assert.Equal((1 << 16), widget.ContractVersion);
        Assert.Equal("both", widget.ThreadingModel);
    }

    [Fact]
    public void ExtractEnumMemberContract()
    {
        var compilation = CompilationHelper.Compile("""
            namespace Windows.Foundation
            {
                [contractversion(10)]
                apicontract UniversalApiContract {};
            }
            namespace TestApp
            {
                enum MyEnum
                {
                    Value1,
                    Value2,
                    [contract(Windows.Foundation.UniversalApiContract, 2)]
                    Value3,
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var myEnum = ns.Members.OfType<EnumSymbol>().Single();
        var value3 = myEnum.EnumMembers.First(m => m.Name == "Value3");

        Assert.Equal("Windows.Foundation.UniversalApiContract", value3.ContractName);
        Assert.Equal((2 << 16), value3.ContractVersion);
    }

    [Fact]
    public void AllAttributesStoredInList()
    {
        var compilation = CompilationHelper.Compile("""
            namespace Windows.Foundation
            {
                [contractversion(10)]
                apicontract UniversalApiContract {};
            }
            namespace TestApp
            {
                [contract(Windows.Foundation.UniversalApiContract, 3), threading(both), marshaling_behavior(agile), default_interface]
                runtimeclass Widget { }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var widget = ns.Members.OfType<RuntimeClassSymbol>().Single();

        Assert.Equal(4, widget.Attributes.Count);
        Assert.Contains(widget.Attributes, a => a.Name == "contract");
        Assert.Contains(widget.Attributes, a => a.Name == "threading");
        Assert.Contains(widget.Attributes, a => a.Name == "marshaling_behavior");
        Assert.Contains(widget.Attributes, a => a.Name == "default_interface");
    }
}
