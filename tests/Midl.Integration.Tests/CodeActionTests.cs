using Midl.Analysis;
using Midl.Analysis.Diagnostics;

namespace Midl.Integration.Tests;

public class CodeActionTests
{
    private static string WrapInClass(string memberDecl) => $@"
namespace TestNs
{{
    runtimeclass TestClass
    {{
        {memberDecl}
    }}
}}";

    [Fact]
    public void FixLowercaseInt()
    {
        var compilation = CompilationHelper.Compile(WrapInClass("void M(int x);"));
        var diag = compilation.Diagnostics.First(d => d.Code == "MIDL2005" && d.Message.Contains("int"));
        Assert.NotNull(diag.Data);
        Assert.Equal("Int32", diag.Data["replacement"]);
    }

    [Fact]
    public void FixLowercaseBoolean()
    {
        var compilation = CompilationHelper.Compile(WrapInClass("void M(boolean x);"));
        var diag = compilation.Diagnostics.First(d => d.Code == "MIDL2005" && d.Message.Contains("boolean"));
        Assert.NotNull(diag.Data);
        Assert.Equal("Boolean", diag.Data["replacement"]);
    }

    [Fact]
    public void FixLowercaseDouble()
    {
        var compilation = CompilationHelper.Compile(WrapInClass("void M(double x);"));
        var diag = compilation.Diagnostics.First(d => d.Code == "MIDL2005" && d.Message.Contains("double"));
        Assert.NotNull(diag.Data);
        Assert.Equal("Double", diag.Data["replacement"]);
    }

    [Fact]
    public void FixLowercaseFloat()
    {
        var compilation = CompilationHelper.Compile(WrapInClass("void M(float x);"));
        var diag = compilation.Diagnostics.First(d => d.Code == "MIDL2005" && d.Message.Contains("float"));
        Assert.NotNull(diag.Data);
        Assert.Equal("Single", diag.Data["replacement"]);
    }

    [Fact]
    public void FixBSTR()
    {
        var compilation = CompilationHelper.Compile(WrapInClass("void M(BSTR x);"));
        var diag = compilation.Diagnostics.First(d => d.Code == "MIDL2005" && d.Message.Contains("BSTR"));
        Assert.NotNull(diag.Data);
        Assert.Equal("String", diag.Data["replacement"]);
    }

    [Fact]
    public void FixVARIANT()
    {
        var compilation = CompilationHelper.Compile(WrapInClass("void M(VARIANT x);"));
        var diag = compilation.Diagnostics.First(d => d.Code == "MIDL2005" && d.Message.Contains("VARIANT"));
        Assert.NotNull(diag.Data);
        Assert.Equal("Object", diag.Data["replacement"]);
    }

    [Fact]
    public void NoFixForValidCode()
    {
        var compilation = CompilationHelper.Compile(WrapInClass("void M(Int32 x);"));
        var fixable = compilation.Diagnostics.Where(d => d.Data is not null).ToList();
        Assert.Empty(fixable);
    }

    [Fact]
    public void ReplacementCoversCorrectSpan()
    {
        var source = WrapInClass("void M(int x);");
        var compilation = CompilationHelper.Compile(source);
        var diag = compilation.Diagnostics.First(d => d.Code == "MIDL2005" && d.Data is not null);
        Assert.Equal("int", source.Substring(diag.Position, diag.Length));
    }
}
