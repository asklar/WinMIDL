// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Cli;

namespace Midl.Integration.Tests;

/// <summary>
/// Every CLI diagnostic must carry a <c>file(line,col):</c> prefix.
///
/// The prefix used to be emitted only when <c>Diagnostic.SourceFile</c> was set, and
/// SourceFile is populated only as a side effect of the source being rewritten — by the
/// preprocessor's SourceMap, or by the comment-stripping remap. A file that is never
/// rewritten produced diagnostics with a null SourceFile and no location at all.
///
/// That made the location depend on incidental properties of the input rather than on
/// anything meaningful:
/// <list type="bullet">
///   <item>a file with no comments and no directives lost its location entirely; and</item>
///   <item>because the preprocessor rejoins lines with <c>Environment.NewLine</c>, an
///         LF-terminated file counted as "rewritten" and kept its location, while the
///         same source with CRLF endings — the normal case on Windows — lost it.</item>
/// </list>
///
/// The driver knows the input path regardless, so it is used as the fallback.
/// </summary>
public class CliDiagnosticLocationTests : IDisposable
{
    private readonly string _tempDir;

    public CliDiagnosticLocationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "midl_cli_loc_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { }
    }

    /// <summary>Writes the content verbatim — no line-ending translation.</summary>
    private string WriteIdl(string content, string fileName)
    {
        var path = Path.Combine(_tempDir, fileName);
        File.WriteAllText(path, content);
        return path;
    }

    private static (int ExitCode, string StdOut, string StdErr) Run(params string[] args)
    {
        var options = CommandLineParser.Parse(args);
        var stdOut = new StringWriter();
        var stdErr = new StringWriter();
        var driver = new CompilationDriver(stdOut, stdErr);
        var exitCode = driver.Run(options);
        return (exitCode, stdOut.ToString(), stdErr.ToString());
    }

    /// <summary>Diagnostic lines only — skips banners and the "MIDL: N error(s)" summary.</summary>
    private static string[] DiagnosticLines(string stdErr) =>
        stdErr.Split('\n')
              .Select(l => l.TrimEnd('\r'))
              .Where(l => l.Contains("error MIDL") || l.Contains("warning MIDL"))
              .Where(l => !l.StartsWith("MIDL:"))
              .ToArray();

    private const string Code = "interface IFoo { HRESULT M([in] long x); }";

    public static TheoryData<string, string> LineEndings => new()
    {
        { "lf",   "\n" },
        { "crlf", "\r\n" },   // the normal case on Windows, and the one that regressed
        { "none", "" },
    };

    /// <summary>
    /// Whether a location is reported must not depend on the file's line endings.
    /// </summary>
    [Theory]
    [MemberData(nameof(LineEndings))]
    public void DiagnosticsCarryALocation_WhateverTheLineEndings(string name, string ending)
    {
        var idl = WriteIdl(Code + ending, $"{name}.idl");

        var diags = DiagnosticLines(Run("/nologo", "/winrt", "/no_def_idir", idl).StdErr);

        Assert.NotEmpty(diags);
        Assert.All(diags, d => Assert.Contains($"{name}.idl(", d));
    }

    /// <summary>
    /// Nor may it depend on whether the file happens to contain a comment: a comment makes
    /// the preprocessor rewrite the source, which is what used to set SourceFile.
    /// </summary>
    [Fact]
    public void AddingAComment_DoesNotChangeWhetherLocationsAreReported()
    {
        var without = WriteIdl(Code + "\r\n", "without.idl");
        var with = WriteIdl("// a comment\r\n" + Code + "\r\n", "with.idl");

        var withoutDiags = DiagnosticLines(Run("/nologo", "/winrt", "/no_def_idir", without).StdErr);
        var withDiags = DiagnosticLines(Run("/nologo", "/winrt", "/no_def_idir", with).StdErr);

        Assert.NotEmpty(withoutDiags);
        Assert.Equal(withDiags.Length, withoutDiags.Length);
        Assert.All(withoutDiags, d => Assert.Contains("without.idl(", d));
        Assert.All(withDiags, d => Assert.Contains("with.idl(", d));
    }

    /// <summary>
    /// The fallback must report the real offset, not a placeholder: <c>HRESULT</c> sits at
    /// column 18, and the comment shifts only the line. A location of (1,1) would point at
    /// the wrong token, which is worse than no location at all.
    /// </summary>
    [Fact]
    public void TheFallbackLocation_IsTheRealOffsetNotPositionZero()
    {
        var without = WriteIdl(Code + "\r\n", "without.idl");
        var with = WriteIdl("// a comment\r\n" + Code + "\r\n", "with.idl");

        var withoutHResult = Assert.Single(
            DiagnosticLines(Run("/nologo", "/winrt", "/no_def_idir", without).StdErr),
            d => d.Contains("MIDL2002"));
        var withHResult = Assert.Single(
            DiagnosticLines(Run("/nologo", "/winrt", "/no_def_idir", with).StdErr),
            d => d.Contains("MIDL2002"));

        Assert.Contains("without.idl(1,18):", withoutHResult);
        Assert.Contains("with.idl(2,18):", withHResult);
    }

    /// <summary>
    /// When the source <em>is</em> rewritten, the location must be computed from
    /// <c>OriginalPosition</c>, not <c>Position</c>.
    ///
    /// The preprocessor rejoins lines with <c>Environment.NewLine</c>, so an LF file is
    /// re-emitted as CRLF and every offset drifts by one character per preceding line.
    /// <c>Position</c> indexes that expanded text while the driver computes line/col
    /// against the original file, so using it puts the caret progressively further right
    /// the deeper into the file the diagnostic is.
    /// </summary>
    [Fact]
    public void WhenTheSourceIsRewritten_TheLocationComesFromTheOriginalOffset()
    {
        // LF endings: guarantees the rejoin rewrites the text. 'long' is on line 5.
        var source = string.Join("\n",
        [
            "namespace N",
            "{",
            "    runtimeclass W",
            "    {",
            "        void M(long x);",
            "    }",
            "}",
            "",
        ]);
        var idl = WriteIdl(source, "rewritten.idl");

        var lowercase = Assert.Single(
            DiagnosticLines(Run("/nologo", "/winrt", "/no_def_idir", idl).StdErr),
            d => d.Contains("MIDL2005"));

        // "        void M(" is 15 characters, so 'long' starts at column 16 of line 5.
        Assert.Contains("rewritten.idl(5,16):", lowercase);
    }
}
