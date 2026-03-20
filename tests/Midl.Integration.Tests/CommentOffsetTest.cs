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
}
