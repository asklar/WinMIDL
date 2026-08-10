// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.Analysis.Diagnostics;

namespace Midl.Analysis.Tests;

/// <summary>
/// Position coverage for MIDL1047 (import file not found).
///
/// The position was hardcoded to 0, so the squiggle always painted the first N
/// characters of the file (N = length of the import path). In any file with a
/// license header that lands inside the leading block comment — a squiggle over
/// unrelated prose describing a problem on a completely different line.
///
/// These assert the exact offset. The pre-existing ImportNotFoundErrors test
/// checked only the code and message, which is why a hardcoded 0 went unnoticed.
/// </summary>
public class ImportNotFoundPositionTests
{
    private static Diagnostic ImportDiag(string source)
    {
        var c = CompilationHelper.Compile(source, referencePaths: null, mode: CompilationMode.Strict,
            includePaths: null, currentFilePath: @"C:\fake\test.idl");
        return Assert.Single(c.Diagnostics, d => d.Code == "MIDL1047");
    }

    private static int Pos(Diagnostic d) => d.OriginalPosition ?? d.Position;

    [Fact]
    public void ImportNotFound_PointsAtThePathText()
    {
        const string source = """
            import "nonexistent.idl";
            namespace N { runtimeclass C { } }
            """;

        var d = ImportDiag(source);

        Assert.Equal(source.IndexOf("nonexistent.idl", StringComparison.Ordinal), Pos(d));
        Assert.Equal("nonexistent.idl".Length, d.Length);
        Assert.Equal("nonexistent.idl", source.Substring(Pos(d), d.Length));
    }

    [Fact]
    public void ImportNotFound_AfterLeadingBlockComment_DoesNotSquiggleTheComment()
    {
        // The real-world shape: a license header, then the import.
        const string source = """
            /*
            This file is a part of the Example project.
            Copyright 2006-2010 contributors.
                This program is distributed in the hope that it will be useful,
                but WITHOUT ANY WARRANTY.
            */

            import "wtypes.idl";

            namespace N { runtimeclass C { } }
            """;

        var d = ImportDiag(source);

        var commentStart = source.IndexOf("/*", StringComparison.Ordinal);
        var commentEnd = source.IndexOf("*/", StringComparison.Ordinal) + 2;

        Assert.False(Pos(d) >= commentStart && Pos(d) < commentEnd,
            $"MIDL1047 at {Pos(d)} lands inside the block comment [{commentStart}..{commentEnd}) — "
            + $"squiggles \"{source.Substring(Pos(d), Math.Min(d.Length, source.Length - Pos(d)))}\"");

        Assert.Equal(source.IndexOf("wtypes.idl", StringComparison.Ordinal), Pos(d));
        Assert.Equal("wtypes.idl", source.Substring(Pos(d), d.Length));
    }

    [Fact]
    public void ImportNotFound_AfterLineComments_PointsAtThePath()
    {
        const string source = """
            // leading comment
            // another line

            import "missing.idl";
            namespace N { runtimeclass C { } }
            """;

        var d = ImportDiag(source);

        Assert.Equal(source.IndexOf("missing.idl", StringComparison.Ordinal), Pos(d));
        Assert.Equal("missing.idl", source.Substring(Pos(d), d.Length));
    }

    [Fact]
    public void MultipleMissingImports_EachGetsItsOwnPosition()
    {
        const string source = """
            import "alpha.idl";
            import "beta.idl";
            import "gamma.idl";
            namespace N { runtimeclass C { } }
            """;

        var c = CompilationHelper.Compile(source, referencePaths: null, mode: CompilationMode.Strict,
            includePaths: null, currentFilePath: @"C:\fake\test.idl");
        var diags = c.Diagnostics.Where(d => d.Code == "MIDL1047").ToList();

        Assert.Equal(3, diags.Count);

        foreach (var name in new[] { "alpha.idl", "beta.idl", "gamma.idl" })
        {
            var d = Assert.Single(diags, x => x.Message.Contains(name));
            Assert.Equal(source.IndexOf(name, StringComparison.Ordinal), Pos(d));
            Assert.Equal(name, source.Substring(Pos(d), d.Length));
        }

        // Distinct positions — not all collapsed onto offset 0.
        Assert.Equal(3, diags.Select(Pos).Distinct().Count());
    }

    [Fact]
    public void ImportNotFound_IndentedImport_PointsAtThePathNotTheIndent()
    {
        const string source = "\r\n\r\n    import \"deep.idl\";\r\nnamespace N { runtimeclass C { } }";

        var d = ImportDiag(source);

        Assert.Equal(source.IndexOf("deep.idl", StringComparison.Ordinal), Pos(d));
        Assert.Equal("deep.idl", source.Substring(Pos(d), d.Length));
    }
}
