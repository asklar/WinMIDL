// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Lexer;

namespace Midl.Analysis.Tests;

/// <summary>
/// A macro-expanded line used to be left out of the source map entirely: the preprocessor
/// flushed a segment before the line and started a new one after it, so the line itself was
/// a hole. Any diagnostic landing on that line resolved to nothing, and the raw expanded
/// offset leaked out — reported as line 16, column 87743 of a 15-line file, which an editor
/// silently drops. The result was a line with a real error and no squiggle.
///
/// Analysis has to run on the expanded text, so the fix is provenance: the expanded line
/// must map back to where it came from.
/// </summary>
public class MacroExpansionSourceMapTests
{
    private static string WriteHeader(string dir, string name, string content)
    {
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, name);
        File.WriteAllText(path, content);
        return path;
    }

    private static string TempDir() =>
        Path.Combine(Path.GetTempPath(), "midl-macromap-" + Guid.NewGuid().ToString("N"));

    /// <summary>Offsets in the expanded text that no segment covers.</summary>
    private static List<(int Start, int End)> FindGaps(Preprocessor pp, int expandedLength)
    {
        var gaps = new List<(int, int)>();
        var ordered = pp.SourceMapSegments.OrderBy(s => s.ExpandedStart).ToList();
        int covered = 0;
        foreach (var s in ordered)
        {
            if (s.ExpandedStart > covered) gaps.Add((covered, s.ExpandedStart));
            covered = Math.Max(covered, s.ExpandedEnd);
        }
        if (covered < expandedLength) gaps.Add((covered, expandedLength));
        return gaps;
    }

    [Fact]
    public void AMacroExpandedLineIsCoveredByTheSourceMap()
    {
        var dir = TempDir();
        try
        {
            WriteHeader(dir, "helper.h", "#define UNUSED_THING 1\n");
            var source = "#define BAR  x\n#include \"helper.h\"\n\nInt32 BAR y;\n";

            var pp = new Preprocessor(null, new[] { dir }, Path.Combine(dir, "main.idl"));
            var expanded = pp.Process(source);

            var gaps = FindGaps(pp, expanded.Length);
            Assert.True(gaps.Count == 0,
                "the expanded text should be fully covered; gaps at " +
                string.Join(", ", gaps.Select(g => $"[{g.Start}..{g.End})")));
        }
        finally { Directory.Delete(dir, recursive: true); }
    }

    [Fact]
    public void APositionOnAMacroExpandedLineResolvesToThatLine()
    {
        var dir = TempDir();
        try
        {
            WriteHeader(dir, "helper.h", "#define UNUSED_THING 1\n");
            var mainPath = Path.Combine(dir, "main.idl");
            // Line 3 (0-based) is the macro line.
            var source = "#define BAR  x\n#include \"helper.h\"\n\nInt32 BAR y;\n";

            var pp = new Preprocessor(null, new[] { dir }, mainPath);
            var expanded = pp.Process(source);

            var map = new SourceMap();
            foreach (var s in pp.SourceMapSegments)
                map.AddSegment(s.ExpandedStart, s.ExpandedEnd, s.FilePath, s.OriginalStart, s.IsConstant);
            map.SortSegments();

            // Find the trailing 'y' in the expanded text — the token a parser would complain
            // about — and check it maps back onto the macro line of the original file.
            int expandedY = expanded.LastIndexOf('y');
            Assert.True(expandedY >= 0, "expanded text should still contain the trailing token");

            var resolved = map.Resolve(expandedY);
            Assert.True(resolved.HasValue, "a position on the expanded line must resolve");

            Assert.Equal(mainPath, resolved!.Value.filePath);
            Assert.True(resolved.Value.originalOffset < source.Length,
                $"offset {resolved.Value.originalOffset} is past the end of a {source.Length}-char file");

            int line = source[..resolved.Value.originalOffset].Count(c => c == '\n');
            Assert.Equal(3, line);
        }
        finally { Directory.Delete(dir, recursive: true); }
    }

    [Fact]
    public void TextBeforeAMacroOnTheSameLineStillMapsExactly()
    {
        var dir = TempDir();
        try
        {
            WriteHeader(dir, "helper.h", "#define UNUSED_THING 1\n");
            var mainPath = Path.Combine(dir, "main.idl");
            var source = "#define BAR  x\n#include \"helper.h\"\n\nInt32 BAR y;\n";

            var pp = new Preprocessor(null, new[] { dir }, mainPath);
            var expanded = pp.Process(source);

            var map = new SourceMap();
            foreach (var s in pp.SourceMapSegments)
                map.AddSegment(s.ExpandedStart, s.ExpandedEnd, s.FilePath, s.OriginalStart, s.IsConstant);
            map.SortSegments();

            // "Int32" precedes the macro, so it is unaffected by expansion and must map
            // character-for-character rather than collapsing to the start of the line.
            int expandedInt32 = expanded.LastIndexOf("Int32", StringComparison.Ordinal);
            var resolved = map.Resolve(expandedInt32);

            Assert.True(resolved.HasValue);
            Assert.Equal(source.IndexOf("Int32", StringComparison.Ordinal), resolved!.Value.originalOffset);
        }
        finally { Directory.Delete(dir, recursive: true); }
    }

    /// <summary>
    /// The reported case, end to end: two macros on one line, one expanding to nothing and
    /// one to an identifier, producing a duplicate name that the parser must reject.
    /// </summary>
    [Fact]
    public void TheReportedCaseProducesADiagnosticInsideTheFile()
    {
        var dir = TempDir();
        try
        {
            WriteHeader(dir, "helper.h", "#define UNUSED_THING 1\n");
            var mainPath = Path.Combine(dir, "main.idl");
            var source = """
                #define FOO
                #define BAR  x
                #include "helper.h"

                namespace N{

                    runtimeclass C {
                        Int32 Y;
                        Int32 FOO BAR x;
                        Int32 X;
                        C();
                    }
                }
                """;
            File.WriteAllText(mainPath, source);

            var c = CompilationHelper.Compile(source, referencePaths: null, mode: CompilationMode.Strict,
                includePaths: new[] { dir }, currentFilePath: mainPath);

            var parseErrors = c.Diagnostics.Where(d => d.Code is "MIDL1100" or "MIDL1102").ToList();
            Assert.True(parseErrors.Count > 0, "'Int32  x x;' is not valid and must be reported");

            foreach (var d in parseErrors)
            {
                int pos = d.OriginalPosition ?? d.Position;
                Assert.True(pos >= 0 && pos < source.Length,
                    $"{d.Code} at {pos} is outside a {source.Length}-char file (raw position {d.Position})");

                int line = source[..pos].Count(ch => ch == '\n');
                Assert.Equal(8, line);
            }
        }
        finally { Directory.Delete(dir, recursive: true); }
    }
}
