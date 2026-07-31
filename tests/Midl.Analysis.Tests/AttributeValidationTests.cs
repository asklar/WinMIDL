// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis.Tests;

public class AttributeValidationTests
{
    [Fact]
    public void ValidThreadingBoth()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [threading(both), activatable, default_interface]
                runtimeclass C { }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
    }

    [Fact]
    public void InvalidThreadingValue()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [threading(invalid), activatable, default_interface]
                runtimeclass C { }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics);
        Assert.Equal("MIDL1020", diag.Code);
        Assert.Contains("invalid", diag.Message);
    }

    [Fact]
    public void ValidMarshalingAgile()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [marshaling_behavior(agile), activatable, default_interface]
                runtimeclass C { }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
    }

    [Fact]
    public void InvalidMarshalingValue()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [marshaling_behavior(bogus), activatable, default_interface]
                runtimeclass C { }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics);
        Assert.Equal("MIDL1021", diag.Code);
        Assert.Contains("bogus", diag.Message);
    }

    [Fact]
    public void InvalidDeprecationType()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [deprecated("msg", bogus, C, 1), default_interface]
                runtimeclass C { }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics);
        Assert.Equal("MIDL1026", diag.Code);
        Assert.Contains("bogus", diag.Message);
    }

    [Fact]
    public void ThreadingOnNonActivatable()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [threading(both)]
                static runtimeclass C { }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics);
        Assert.Equal("MIDL1027", diag.Code);
        Assert.Contains("threading", diag.Message);
    }

    [Fact]
    public void DefaultInterfaceOnEmptyClass()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [default_interface]
                runtimeclass E { }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single();

        Assert.NotNull(rc.DefaultInterface);
        Assert.Equal("IE", rc.DefaultInterface!.Name);
        Assert.True(rc.DefaultInterface.IsExclusive);
        Assert.True(rc.DefaultInterface.IsSynthesized);
        Assert.Equal("N.E", rc.DefaultInterface.ExclusiveToClass);
        Assert.Empty(rc.DefaultInterface.Members);
    }

    [Fact]
    public void ValidAttributesNoDiagnostic()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [threading(both), marshaling_behavior(agile), activatable, default_interface]
                runtimeclass C { }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
    }

    [Fact]
    public void AttributeUsageOnlyOnAttribute()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [attributeusage(target_all), default_interface]
                runtimeclass C { }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1029");
        Assert.Contains("attributeusage", diag.Message);
    }

    [Fact]
    public void AttributeUsageOnAttribute()
    {
        // The parser doesn't produce AttributeDeclaration nodes yet,
        // so 'attribute' is treated as an unknown declaration and skipped.
        // This test verifies that [attributeusage] does NOT produce a
        // diagnostic when the containing type is not bound as a runtimeclass/interface/etc.
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [attributeusage(target_all)]
                attribute MyAttr { }
            }
            """);

        // No MIDL1029 because the attribute declaration is skipped by the binder;
        // the attributeusage attribute is not on a non-attribute type.
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1029");
    }

    // ─── Attribute target restriction tests ───────────────────────────

    [Fact]
    public void ContractversionOnRuntimeclass_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [contractversion(1), default_interface]
                runtimeclass C { }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1029");
        Assert.Contains("contractversion", diag.Message);
        Assert.Contains("target_apicontract", diag.Message);
    }

    [Fact]
    public void ContractversionOnApicontract_NoError()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [contractversion(1)]
                apicontract MyContract { }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1029");
    }

    [Fact]
    public void UuidOnRuntimeclass_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [uuid("12345678-1234-1234-1234-123456789abc"), default_interface]
                runtimeclass C { }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1029");
        Assert.Contains("uuid", diag.Message);
    }

    [Fact]
    public void UuidOnInterface_NoError()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [uuid("12345678-1234-1234-1234-123456789abc")]
                interface IFoo
                {
                    void Bar();
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1029");
    }

    [Fact]
    public void ThreadingOnInterface_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [threading(both)]
                interface IFoo
                {
                    void Bar();
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1029");
        Assert.Contains("threading", diag.Message);
        Assert.Contains("target_runtimeclass", diag.Message);
    }

    [Fact]
    public void FlagsOnRuntimeclass_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [flags, default_interface]
                runtimeclass C { }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1029");
        Assert.Contains("flags", diag.Message);
        Assert.Contains("target_enum", diag.Message);
    }

    [Fact]
    public void OverloadOnProperty_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [default_interface]
                runtimeclass C
                {
                    [overload("X")]
                    String Name { get; };
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1029");
        Assert.Contains("overload", diag.Message);
        Assert.Contains("target_method", diag.Message);
    }

    [Fact]
    public void DefaultInterfaceOnInterface_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [default_interface]
                interface IFoo
                {
                    void Bar();
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1029");
        Assert.Contains("default_interface", diag.Message);
        Assert.Contains("target_runtimeclass", diag.Message);
    }

    [Fact]
    public void ExclusivetoOnRuntimeclass_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [exclusiveto(C), default_interface]
                runtimeclass C { }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1029");
        Assert.Contains("exclusiveto", diag.Message);
        Assert.Contains("target_interface", diag.Message);
    }

    [Fact]
    public void ActivatableOnInterface_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [activatable(1)]
                interface IFoo
                {
                    void Bar();
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1029");
        Assert.Contains("activatable", diag.Message);
        Assert.Contains("target_runtimeclass", diag.Message);
    }

    [Fact]
    public void NoexceptOnRuntimeclass_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [noexcept, default_interface]
                runtimeclass C { }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1029");
        Assert.Contains("noexcept", diag.Message);
        Assert.Contains("target_method", diag.Message);
    }

    [Fact]
    public void ComposableOnInterface_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [composable(IFoo, public, 1)]
                interface IFoo
                {
                    void Bar();
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1029");
        Assert.Contains("composable", diag.Message);
        Assert.Contains("target_runtimeclass", diag.Message);
    }

    [Fact]
    public void MethodNameOnRuntimeclass_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [method_name("Foo"), default_interface]
                runtimeclass C { }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1029");
        Assert.Contains("method_name", diag.Message);
        Assert.Contains("target_method", diag.Message);
    }

    [Fact]
    public void DeprecatedOnMethod_NoError()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [contractversion(1)]
                apicontract MyContract { }

                [default_interface]
                runtimeclass C
                {
                    [deprecated("old", deprecate, MyContract, 1)]
                    void DoThing();
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1029");
    }

    [Fact]
    public void DeprecatedOnEnum_NoError()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [contractversion(1)]
                apicontract MyContract { }

                [deprecated("old enum", deprecate, MyContract, 1)]
                enum E
                {
                    A = 0
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1029");
    }

    [Fact]
    public void ExperimentalOnStruct_NoError()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [experimental]
                struct S
                {
                    Int32 X;
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1029");
    }

    [Fact]
    public void UnknownAttributeFoobar_ProducesMIDL1065()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [foobar, default_interface]
                runtimeclass C { }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1065");
        Assert.Contains("foobar", diag.Message);
    }

    [Fact]
    public void ComposableAndActivatable_ProducesMIDL1025()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [exclusiveto(C)]
                interface ICFactory
                {
                    C Create();
                }
                [composable(ICFactory, public, 1)]
                [activatable]
                unsealed runtimeclass C
                {
                    overridable void DoWork();
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1025");
        Assert.Equal(DiagnosticSeverity.Error, diag.Severity);
    }

    [Fact]
    public void NoExceptOnProperty_ProducesTargetError()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [default_interface]
                runtimeclass C
                {
                    [noexcept]
                    String Name { get; };
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1029");
        Assert.Contains("noexcept", diag.Message);
        Assert.Contains("target_method", diag.Message);
    }

    [Fact]
    public void ContractArgNotApiContract_ProducesMIDL1075()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [contractversion(1)]
                apicontract RealContract { }

                [contract(NotAnApiContract, 1), default_interface]
                runtimeclass C { }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1075");
        Assert.Equal(DiagnosticSeverity.Error, diag.Severity);
        Assert.Contains("NotAnApiContract", diag.Message);
        Assert.Contains("C", diag.Message);
    }

    [Fact]
    public void ContractArgIsValidApiContract_NoError()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [contractversion(1)]
                apicontract ValidContract { }

                [contract(ValidContract, 1), default_interface]
                runtimeclass C { }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1075");
    }
}
