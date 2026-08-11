// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;

namespace Midl.Analysis.Tests;

/// <summary>
/// A diagnostic's column must not depend on the file's line-ending convention.
///
/// The preprocessor built its output with <c>StringBuilder.AppendLine</c>, which always
/// appends <c>Environment.NewLine</c> — CRLF on Windows — whatever the source used. An LF
/// file therefore became a CRLF buffer, one byte longer per line than the original it is
/// mapped from, while <see cref="SourceMap"/> maps linearly within a segment:
/// <c>OriginalStart + delta</c>. Every line crossed added one character of error.
///
/// It stayed invisible because Windows-authored IDLs and the Windows SDK headers are CRLF.
/// It appears on cross-platform repositories, files normalised by tooling, and checkouts
/// with <c>core.autocrlf=input</c>.
/// </summary>
public class LineEndingPositionTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "midl-le-" + Guid.NewGuid().ToString("N"));

    public LineEndingPositionTests() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        try { Directory.Delete(_dir, recursive: true); } catch { }
    }

    private static int Painted(Diagnostics.Diagnostic d) => d.OriginalPosition ?? d.Position;

    /// <summary>Body with a lowercase 'long' that strict mode reports.</summary>
    private const string BodyLf =
        "namespace N\n" +
        "{\n" +
        "    runtimeclass C\n" +
        "    {\n" +
        "        C();\n" +
        "        void M(long p);\n" +
        "    }\n" +
        "}\n";

    public static TheoryData<string, string> Cases => new()
    {
        // label, prefix before the body
        { "no directive", "" },
        { "define only", "#define FOO Bar\n" },
        { "define and use", "#define FOO Int32\n" },
        { "ifdef block", "#ifdef NOT_DEFINED\nstruct Hidden { Int32 A; };\n#endif\n" },
    };

    /// <summary>
    /// The same source, in LF and in CRLF, must report the offending token at the same
    /// line and column. Asserted against the offset of the token in each file's own text,
    /// so it is the real position rather than merely a consistent one.
    /// </summary>
    [Theory]
    [MemberData(nameof(Cases))]
    public void ColumnsAreTheSameForLfAndCrlf(string label, string prefix)
    {
        var lf = prefix + BodyLf;
        var crlf = lf.Replace("\n", "\r\n");

        var lfDiag = SingleLowercaseLongDiagnostic(lf, "lf.idl");
        var crlfDiag = SingleLowercaseLongDiagnostic(crlf, "crlf.idl");

        var (lfLine, lfCol) = LineAndColumn(lf, Painted(lfDiag));
        var (crlfLine, crlfCol) = LineAndColumn(crlf, Painted(crlfDiag));

        var (expectedLine, expectedCol) = LineAndColumn(lf, lf.IndexOf("long p", StringComparison.Ordinal));

        Assert.True(expectedLine == lfLine && expectedCol == lfCol,
            $"[{label}] LF reported ({lfLine},{lfCol}), expected ({expectedLine},{expectedCol})");
        Assert.True(lfLine == crlfLine && lfCol == crlfCol,
            $"[{label}] LF reported ({lfLine},{lfCol}) but CRLF reported ({crlfLine},{crlfCol})");
    }

    private Diagnostics.Diagnostic SingleLowercaseLongDiagnostic(string source, string name)
    {
        var path = Path.Combine(_dir, name);
        File.WriteAllText(path, source);
        var comp = CompilationHelper.Compile(source, referencePaths: null, mode: CompilationMode.Strict,
            includePaths: new[] { _dir }, currentFilePath: path);
        return Assert.Single(comp.Diagnostics, d => d.Code == "MIDL2005" && d.Message.Contains("'long'"));
    }

    private static (int Line, int Col) LineAndColumn(string text, int offset)
    {
        int line = 1, col = 1;
        for (int i = 0; i < offset && i < text.Length; i++)
        {
            if (text[i] == '\n') { line++; col = 1; }
            else if (text[i] != '\r') { col++; }
        }
        return (line, col);
    }
}
