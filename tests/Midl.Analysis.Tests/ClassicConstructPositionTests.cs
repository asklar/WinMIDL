// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.Analysis.Diagnostics;

namespace Midl.Analysis.Tests;

/// <summary>
/// Classic-construct diagnostics (MIDL2001) must point at the construct that triggered
/// them, not at the first construct of that kind in the file.
///
/// <c>FindKeywordPosition</c> resolved positions with <c>_posMap.GetPositionOfFirst(text)</c>,
/// a text lookup that always returns the *first* matching token. Every <c>cpp_quote</c>
/// in a file therefore reported at the first <c>cpp_quote</c> — 13 stacked diagnostics on
/// one line, none on the other twelve.
/// </summary>
public class ClassicConstructPositionTests
{
    private static List<Diagnostic> Classic(string source, string code = "MIDL2001") =>
        CompilationHelper.Compile(source, referencePaths: null, mode: CompilationMode.Strict,
                includePaths: null, currentFilePath: null)
            .Diagnostics.Where(d => d.Code == code).ToList();

    private static int Pos(Diagnostic d) => d.OriginalPosition ?? d.Position;

    [Fact]
    public void EachCppQuote_ReportsAtItsOwnPosition()
    {
        const string source = """
            cpp_quote("alpha")
            cpp_quote("beta")
            cpp_quote("gamma")
            namespace N { runtimeclass C { } }
            """;

        var diags = Classic(source).Where(d => d.Message.Contains("cpp_quote")).ToList();
        Assert.Equal(3, diags.Count);

        // Three distinct positions, one per directive.
        var positions = diags.Select(Pos).OrderBy(p => p).ToList();
        Assert.Equal(3, positions.Distinct().Count());

        var expected = new List<int>();
        for (int i = source.IndexOf("cpp_quote", StringComparison.Ordinal); i >= 0;
             i = source.IndexOf("cpp_quote", i + 1, StringComparison.Ordinal))
        {
            expected.Add(i);
        }
        Assert.Equal(expected, positions);
    }

    [Fact]
    public void CppQuote_SquiggleCoversTheKeyword()
    {
        const string source = """
            cpp_quote("alpha")
            cpp_quote("beta")
            namespace N { runtimeclass C { } }
            """;

        foreach (var d in Classic(source).Where(d => d.Message.Contains("cpp_quote")))
        {
            Assert.Equal("cpp_quote", source.Substring(Pos(d), d.Length));
        }
    }

    [Fact]
    public void EachTypedef_ReportsAtItsOwnPosition()
    {
        const string source = """
            typedef struct { long a; } First;
            typedef struct { long b; } Second;
            namespace N { runtimeclass C { } }
            """;

        var diags = Classic(source).Where(d => d.Message.Contains("typedef")).ToList();
        Assert.Equal(2, diags.Count);
        Assert.Equal(2, diags.Select(Pos).Distinct().Count());

        foreach (var d in diags)
            Assert.Equal("typedef", source.Substring(Pos(d), d.Length));
    }

    [Fact]
    public void CppQuote_AfterLeadingBlockComment_IsNotReportedInTheComment()
    {
        // The real-world shape that produced squiggles over license prose.
        const string source = """
            /*
            This program is distributed in the hope that it will be useful,
            but WITHOUT ANY WARRANTY.
            */

            cpp_quote("/*")
            cpp_quote("This program is distributed in the hope that it will be useful,")
            cpp_quote("*/")

            namespace N { runtimeclass C { } }
            """;

        var commentEnd = source.IndexOf("*/", StringComparison.Ordinal) + 2;

        var diags = Classic(source).Where(d => d.Message.Contains("cpp_quote")).ToList();
        Assert.Equal(3, diags.Count);

        foreach (var d in diags)
        {
            Assert.True(Pos(d) >= commentEnd,
                $"cpp_quote diagnostic at {Pos(d)} lands inside the leading block comment "
                + $"(ends at {commentEnd}); squiggles \"{source.Substring(Pos(d), d.Length)}\"");
            Assert.Equal("cpp_quote", source.Substring(Pos(d), d.Length));
        }

        Assert.Equal(3, diags.Select(Pos).Distinct().Count());
    }
}
