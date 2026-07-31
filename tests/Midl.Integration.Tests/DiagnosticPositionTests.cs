// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.Analysis.Diagnostics;

namespace Midl.Integration.Tests;

public class DiagnosticPositionTests
{
    [Fact]
    public void NamespaceRequiredPositionOnTypeName()
    {
        var source = "runtimeclass Foo { }";
        var compilation = CompilationHelper.Compile(source);
        var diag = compilation.Diagnostics.First(d => d.Code == "MIDL1003");
        Assert.True(diag.Position > 0, $"Expected non-zero position for {diag.Code}: {diag.Message}");
        Assert.Equal("Foo", source.Substring(diag.Position, diag.Length));
    }

    [Fact]
    public void UndefinedTypePositionOnUsage()
    {
        var source = "namespace N { runtimeclass C { void M(BadType x); } }";
        var compilation = CompilationHelper.Compile(source);
        var diag = compilation.Diagnostics.First(d => d.Code == "MIDL1001");
        Assert.True(diag.Position > 0, $"Expected non-zero position for {diag.Code}: {diag.Message}");
        Assert.Equal("BadType", source.Substring(diag.Position, diag.Length));
    }

    [Fact]
    public void LowercaseTypePositionOnKeyword()
    {
        var source = "namespace N { runtimeclass C { int Foo(); } }";
        var compilation = CompilationHelper.Compile(source);
        var diag = compilation.Diagnostics.First(d => d.Code == "MIDL2005");
        Assert.True(diag.Position > 0, $"Expected non-zero position for {diag.Code}: {diag.Message}");
        Assert.Equal("int", source.Substring(diag.Position, diag.Length));
    }

    [Fact]
    public void ClassicConstructPositionOnKeyword()
    {
        var source = "coclass C { }";
        var compilation = CompilationHelper.Compile(source, CompilationMode.Strict);
        var diag = compilation.Diagnostics.First(d => d.Code == "MIDL2001");
        Assert.Equal(0, diag.Position); // "coclass" is the very first token
        Assert.Equal("coclass", source.Substring(diag.Position, diag.Length));
    }

    [Fact]
    public void AllDiagnosticsHaveNonZeroPosition()
    {
        // Sources where all diagnostics should point to non-start-of-file tokens
        var inputs = new[]
        {
            ("namespace N { struct Empty { }; }", "MIDL1009"),
            ("namespace N { runtimeclass C { void M(Int32 result); } }", "MIDL1013"),
            ("namespace N { runtimeclass C { void M(BadType x); } }", "MIDL1001"),
            ("namespace N { runtimeclass Foo { } runtimeclass foo { } }", "MIDL1010"),
        };

        foreach (var (source, expectedCode) in inputs)
        {
            var compilation = CompilationHelper.Compile(source);
            var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == expectedCode);
            Assert.NotNull(diag);
            Assert.True(diag.Position > 0,
                $"Expected non-zero position for {diag.Code}: {diag.Message} in source: {source}");
        }
    }
}
