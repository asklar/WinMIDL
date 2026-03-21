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
        Assert.Equal("E", rc.DefaultInterface.ExclusiveToClass);
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

        var diag = Assert.Single(compilation.Diagnostics);
        Assert.Equal("MIDL1028", diag.Code);
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

        // No MIDL1028 because the attribute declaration is skipped by the binder;
        // the attributeusage attribute is not on a non-attribute type.
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1028");
    }
}
