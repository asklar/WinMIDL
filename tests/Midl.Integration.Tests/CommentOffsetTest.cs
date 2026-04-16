using Midl.Analysis;
using Xunit;

public class CommentOffsetTest
{
    [Fact]
    public void DiagnosticPositionsAccountForComments()
    {
        // "// test\r\n\r\n" = 11 chars of comment/whitespace before "namespace{"
        var source = "// test\r\n\r\nnamespace{\r\n    interface foo\r\n    {\r\n        float x();\r\n    }\r\n}";
        var compilation = CompilationHelper.Compile(source);

        foreach (var d in compilation.Diagnostics)
        {
            // No diagnostic should point into the comment area (positions 0-10)
            // The comment is "// test\r\n" (9 chars) + "\r\n" (2 chars) = 11
            // "namespace" starts at position 11
            if (d.Code == "MIDL1102") // Expected 'identifier' for missing namespace name
            {
                Assert.True(d.Position >= 11,
                    $"Diagnostic {d.Code} at pos {d.Position} points into comment area. " +
                    $"Message: {d.Message}. Should be >= 11 (after comment).");
            }
        }
    }

    [Fact]
    public void DiagnosticsNeverPointIntoLeadingComments()
    {
        // Larger comment block followed by invalid code
        var source = "/* big\r\n   multi-line\r\n   comment */\r\nruntimeclass Foo { }";
        var compilation = CompilationHelper.Compile(source);

        var commentEnd = source.IndexOf("*/") + 4; // past "*/" and "\r\n"
        foreach (var d in compilation.Diagnostics)
        {
            // MIDL1003 (namespace required) should point at "Foo", not into the comment
            if (d.Code == "MIDL1003")
            {
                Assert.True(d.Position >= commentEnd,
                    $"Diagnostic {d.Code} at pos {d.Position} points into comment. Should be >= {commentEnd}");
            }
        }
    }

    [Fact]
    public void DiagnosticAfterInlineComment_PointsToCorrectColumn()
    {
        // No macros — just a comment before a syntax error.
        // The diagnostic must NOT point into the comment.
        var source = "namespace N { interface IFoo { Int32 /* comment */ (); }; }";
        var compilation = CompilationHelper.Compile(source);

        var commentStart = source.IndexOf("/* comment */");
        var commentEnd = commentStart + "/* comment */".Length;

        var diags = compilation.Diagnostics;
        Assert.True(diags.Count > 0, "Expected at least one diagnostic");

        foreach (var d in diags)
        {
            var pos = d.OriginalPosition ?? d.Position;
            Assert.False(
                pos >= commentStart && pos < commentEnd,
                $"Diagnostic {d.Code} at pos {pos} points into comment [{commentStart}..{commentEnd}). " +
                $"Message: {d.Message}");
        }
    }

    [Fact]
    public void DiagnosticAfterMacro_NOTHING_Before_PointsToOriginalX()
    {
        // NOTHING (7 chars) → "" shrinks the expanded line.
        // MIDL3015 (PascalCase) on 'x' must point to x's position in the original source.
        var source = "#define NOTHING \nnamespace N { runtimeclass C { NOTHING Int32 x; } }";
        var compilation = CompilationHelper.Compile(source, referencePaths: null,
            mode: CompilationMode.Strict, includePaths: null, currentFilePath: null,
            defines: null, emitQualityHints: true);

        var xPos = source.IndexOf(" x;") + 1;
        var naming = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL3015");
        Assert.NotNull(naming);
        var pos = naming.OriginalPosition ?? naming.Position;
        Assert.Equal(xPos, pos);
    }

    [Fact]
    public void DiagnosticAfterMacro_NOTHING_After_PointsToOriginalX()
    {
        // NOTHING after the type name — same column-shift test.
        var source = "#define NOTHING \nnamespace N { runtimeclass C { Int32 NOTHING x; } }";
        var compilation = CompilationHelper.Compile(source, referencePaths: null,
            mode: CompilationMode.Strict, includePaths: null, currentFilePath: null,
            defines: null, emitQualityHints: true);

        var xPos = source.IndexOf(" x;") + 1;
        var naming = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL3015");
        Assert.NotNull(naming);
        var pos = naming.OriginalPosition ?? naming.Position;
        Assert.Equal(xPos, pos);
    }

    [Fact]
    public void DiagnosticAfterMacro_MYTYPE_PointsToOriginalX()
    {
        // MYTYPE (6 chars) → Int32 (5 chars): line shrinks by 1.
        // MIDL3015 on 'x' must point to x in the original source, not 1 char before.
        var source = "#define MYTYPE Int32\nnamespace N { runtimeclass C { MYTYPE x; } }";
        var compilation = CompilationHelper.Compile(source, referencePaths: null,
            mode: CompilationMode.Strict, includePaths: null, currentFilePath: null,
            defines: null, emitQualityHints: true);

        var xPos = source.IndexOf(" x;") + 1;
        var naming = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL3015");
        Assert.NotNull(naming);
        var pos = naming.OriginalPosition ?? naming.Position;
        Assert.Equal(xPos, pos);
    }

    [Fact]
    public void DiagnosticRemap_DuplicateToken_MatchesCorrectOccurrence()
    {
        // Two identically-named members on the same line after a macro expansion.
        // The remap must match each diagnostic to the correct occurrence, not just
        // the nearest one.
        //
        // Original: "... { NOTHING Int32 x; Int32 y; }"
        // Expanded: "...  {  Int32 x; Int32 y; }"
        // Both x and y get MIDL3015 — each must point to the correct one.
        var source = "#define NOTHING \nnamespace N { runtimeclass C { NOTHING Int32 x; Int32 y; } }";
        var compilation = CompilationHelper.Compile(source, referencePaths: null,
            mode: CompilationMode.Strict, includePaths: null, currentFilePath: null,
            defines: null, emitQualityHints: true);

        var xPos = source.IndexOf(" x;") + 1;
        var yPos = source.IndexOf(" y;") + 1;

        var namingDiags = compilation.Diagnostics.Where(d => d.Code == "MIDL3015").ToList();
        Assert.True(namingDiags.Count >= 2, $"Expected 2+ MIDL3015 diagnostics, got {namingDiags.Count}");

        var xDiag = namingDiags.FirstOrDefault(d => d.Message.Contains("'x'"));
        var yDiag = namingDiags.FirstOrDefault(d => d.Message.Contains("'y'"));
        Assert.NotNull(xDiag);
        Assert.NotNull(yDiag);

        Assert.Equal(xPos, xDiag.OriginalPosition ?? xDiag.Position);
        Assert.Equal(yPos, yDiag.OriginalPosition ?? yDiag.Position);
    }
}
