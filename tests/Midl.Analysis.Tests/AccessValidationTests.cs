// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis.Tests;

public class AccessValidationTests
{
    [Fact]
    public void ProtectedOnSealedClassAllowed()
    {
        // Original midl.exe allows protected on sealed classes — we match that behavior
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                runtimeclass C
                {
                    protected void M();
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1033");
    }

    [Fact]
    public void OverridableOnSealedClassAllowed()
    {
        // Original midl.exe allows overridable on sealed classes — we match that behavior
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                runtimeclass C
                {
                    overridable void M();
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1033");
    }

    [Fact]
    public void ProtectedOnUnsealedClassOk()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                unsealed runtimeclass C
                {
                    protected void M();
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1033");
    }

    [Fact]
    public void OverridableOnUnsealedClassOk()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                unsealed runtimeclass C
                {
                    overridable void M();
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1033");
    }

    [Fact]
    public void RequiresUndefinedInterfaceErrors()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                interface I requires IUndefined
                {
                }
            }
            """);

        // MIDL1001 (undefined type) may also fire from TypeResolver;
        // we expect our more-specific MIDL1034 as well.
        Assert.Contains(compilation.Diagnostics, d => d.Code == "MIDL1034"
            && d.Message.Contains("IUndefined"));
    }

    [Fact]
    public void RequiresResolvedInterfaceOk()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                interface IA
                {
                }
                interface IB requires IA
                {
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1034");
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1038");
    }

    [Fact]
    public void RuntimeClassOnRequiresListErrors()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                runtimeclass C
                {
                }
                interface I requires C
                {
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1038");
        Assert.Contains("C", diag.Message);
        Assert.Contains("runtimeclass", diag.Message);
    }

    [Fact]
    public void BaseClassMustBeRuntimeclass()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                struct S
                {
                    Int32 X;
                }
                runtimeclass C : S
                {
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1035");
        Assert.Contains("S", diag.Message);
        Assert.Contains("not a runtimeclass", diag.Message);
    }

    [Fact]
    public void BaseClassMustBeUnsealed()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                runtimeclass Base
                {
                }
                runtimeclass Derived : Base
                {
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1036");
        Assert.Contains("Base", diag.Message);
        Assert.Contains("sealed runtimeclass", diag.Message);
    }

    [Fact]
    public void BaseClassUnsealedOk()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                unsealed runtimeclass Base
                {
                }
                runtimeclass Derived : Base
                {
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1035");
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1036");
    }

    [Fact]
    public void ProtectedPropertyOnSealedAllowed()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                runtimeclass C
                {
                    protected String Name { get; };
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1033");
    }

    [Fact]
    public void OverridableEventOnSealedAllowed()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                delegate void Handler();
                runtimeclass C
                {
                    overridable event Handler E;
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1033");
    }
}
