using Midl.Analysis;

namespace Midl.Analysis.Tests;

public class VersionValidationTests
{
    [Fact]
    public void ContractVersion_NegativeValue_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [contractversion(-1)]
                apicontract MyContract { }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1107");
        Assert.Contains("contractversion", diag.Message);
        Assert.Contains("-1", diag.Message);
    }

    [Fact]
    public void ContractVersion_PositiveValue_NoError()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [contractversion(1)]
                apicontract MyContract { }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1107");
    }

    [Fact]
    public void ContractVersion_Zero_NoError()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [contractversion(0)]
                apicontract MyContract { }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1107");
    }

    [Fact]
    public void Version_NegativeValue_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [version(-5), default_interface]
                runtimeclass C { }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1107");
        Assert.Contains("version", diag.Message);
        Assert.Contains("-5", diag.Message);
    }

    [Fact]
    public void Version_PositiveValue_NoError()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [version(1), default_interface]
                runtimeclass C { }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1107");
    }

    [Fact]
    public void Contract_NegativeVersion_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [contractversion(1)]
                apicontract MyContract { }

                [contract(MyContract, -1), default_interface]
                runtimeclass C { }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1107");
        Assert.Contains("contract", diag.Message);
    }

    [Fact]
    public void Contract_VersionLiteral_NoError()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [contractversion(1)]
                apicontract MyContract { }

                [contract(MyContract, 1.0), default_interface]
                runtimeclass C { }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1107");
    }

    [Fact]
    public void ContractVersion_FloatValue_NoError()
    {
        // [contractversion(1.0)] — valid x.y version literal (binder encodes as major << 16 | minor)
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [contractversion(1.0)]
                apicontract MyContract { }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1107");
    }

    [Fact]
    public void Version_FloatValue_NoError()
    {
        // [version(1.0)] — valid x.y version literal
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [version(1.0), default_interface]
                runtimeclass C { }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1107");
    }

    [Fact]
    public void InvalidVersionArg_SquiggleOnArg_NotAttrName()
    {
        // "version" is at column 23 in the source (0-based)
        var source = "namespace N { [contractversion(1.0)] apicontract C {} [contract(C, version)] runtimeclass R : Windows.Foundation.IStringable { R(); } }";
        var compilation = CompilationHelper.Compile(source);

        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1107");
        Assert.NotNull(diag);
        // Position should be on "version" (the arg), not "contract" (the attr name)
        var snippet = source[diag.Position..(diag.Position + diag.Length)];
        Assert.Equal("version", snippet);
    }

    [Fact]
    public void InvalidThreadingModel_SquiggleOnArg()
    {
        var source = "namespace N { [threading(banana), default_interface] runtimeclass R { R(); } }";
        var compilation = CompilationHelper.Compile(source);

        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1020");
        Assert.NotNull(diag);
        var snippet = source[diag.Position..(diag.Position + diag.Length)];
        Assert.Equal("banana", snippet);
    }

    [Fact]
    public void ContractExceedsMax_SquiggleOnVersionArg()
    {
        var source = "namespace N { [contractversion(1.55)] apicontract Foo{} [contract(Foo, 2)] runtimeclass R : Windows.Foundation.IStringable { R(); } }";
        var compilation = CompilationHelper.Compile(source);
        
        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1093");
        Assert.NotNull(diag);
        var snippet = source[diag.Position..(diag.Position + diag.Length)];
        Assert.Equal("2", snippet);
    }

    [Fact]
    public void ContractExceedsMax_MultiLine_SquiggleOnVersionArg()
    {
        var source = "namespace N {\n    [contractversion(1.55)]\n    apicontract Foo{}\n    [threading(both)]\n    [contract(Foo, 2)]\n    runtimeclass R : Windows.Foundation.IStringable\n    {\n        R();\n    }\n}";
        var compilation = CompilationHelper.Compile(source);
        
        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1093");
        Assert.NotNull(diag);
        Assert.True(diag.Position >= source.IndexOf("[contract"),
            $"Squiggle at pos {diag.Position} should be inside [contract(Foo, 2)]");
        Assert.True(diag.Position < source.IndexOf("runtimeclass"),
            $"Squiggle at pos {diag.Position} should be before 'runtimeclass'");
    }

    [Fact]
    public void InvalidMarshalingBehavior_SquiggleOnArg()
    {
        var source = "namespace N { [marshaling_behavior(banana), default_interface] runtimeclass R { R(); } }";
        var compilation = CompilationHelper.Compile(source);
        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1021");
        Assert.NotNull(diag);
        var snippet = source[diag.Position..(diag.Position + diag.Length)];
        Assert.Equal("banana", snippet);
    }

    [Fact]
    public void InvalidDeprecationType_SquiggleOnArg()
    {
        var source = "namespace N { [deprecated(\"old\", banana, \"C\", 1)] interface IFoo { void F(); } }";
        var compilation = CompilationHelper.Compile(source);
        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1026");
        Assert.NotNull(diag);
        var snippet = source[diag.Position..(diag.Position + diag.Length)];
        Assert.Equal("banana", snippet);
    }

    [Fact]
    public void WrongTarget_SquiggleOnAttrName()
    {
        var source = "namespace N { [threading(both)] struct S { Int32 X; }; }";
        var compilation = CompilationHelper.Compile(source);
        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1029");
        Assert.NotNull(diag);
        var snippet = source[diag.Position..(diag.Position + diag.Length)];
        Assert.Equal("threading", snippet);
    }
}
