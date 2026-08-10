// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.Analysis.Diagnostics;

namespace Midl.Analysis.Tests;

/// <summary>
/// Attribution and positioning of diagnostics raised while binding <c>import</c>ed files.
///
/// Diagnostics from an imported IDL used to be attributed to the *importing* document,
/// carrying offsets computed against the imported file. Painting those offsets onto the
/// main document scattered squiggles across unrelated text — inside comments, mid-word,
/// spanning line breaks.
///
/// Root cause: only the preprocessor's SourceMap (which knows <c>#include</c>) resolved
/// SourceFile. <c>import</c> is a MIDL-level construct the source map never sees, so
/// <c>sourceMap.Resolve(diag.Position)</c> reinterpreted an imported-file offset as a
/// main-file offset and confidently attributed it to the main file.
/// </summary>
public class ImportedDiagnosticAttributionTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "midl-imp-" + Guid.NewGuid().ToString("N"));

    public ImportedDiagnosticAttributionTests() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        try { Directory.Delete(_dir, recursive: true); } catch { }
    }

    /// <summary>
    /// The offset the LSP actually paints. DiagnosticsPublisher prefers
    /// <c>OriginalPosition</c> whenever it is set, so a test that asserts on
    /// <c>Position</c> alone can pass while the squiggle lands somewhere else
    /// (or fail while the squiggle is correct).
    /// </summary>
    private static int Painted(Diagnostic d) => d.OriginalPosition ?? d.Position;

    private string Write(string name, string content)
    {
        var p = Path.Combine(_dir, name);
        File.WriteAllText(p, content);
        return p;
    }

    private Compilation CompileMain(string mainSource, string mainName = "main.idl")
    {
        var mainPath = Write(mainName, mainSource);
        return CompilationHelper.Compile(mainSource, referencePaths: null, mode: CompilationMode.Strict,
            includePaths: new[] { _dir }, currentFilePath: mainPath);
    }

    [Fact]
    public void ImportedDiagnostics_AreAttributedToTheImportedFile()
    {
        var depPath = Write("dep.idl", """
            [pointer_default(unique)]
            interface IImported { HRESULT Foo(); }
            """);

        const string main = """
            import "dep.idl";
            namespace N { runtimeclass C { } }
            """;

        var c = CompileMain(main);

        var classic = c.Diagnostics.Where(d => d.Code == "MIDL2003").ToList();
        Assert.NotEmpty(classic);

        foreach (var d in classic)
        {
            Assert.NotNull(d.SourceFile);
            Assert.Equal(Path.GetFullPath(depPath), Path.GetFullPath(d.SourceFile!));
        }
    }

    [Fact]
    public void ImportedDiagnostics_HavePositionsValidInTheImportedFile()
    {
        var depSource = """
            [pointer_default(unique)]
            interface IImported { HRESULT Foo(); }
            """;
        var depPath = Write("dep.idl", depSource);

        var c = CompileMain("""
            import "dep.idl";
            namespace N { runtimeclass C { } }
            """);

        var d = c.Diagnostics.First(x => x.Code == "MIDL2003");
        var pos = d.OriginalPosition ?? d.Position;

        Assert.InRange(pos, 0, depSource.Length - 1);
        // Must actually point at the offending construct.
        Assert.Equal(depSource.IndexOf("pointer_default", StringComparison.Ordinal), pos);
    }

    [Fact]
    public void ImportedDiagnostics_DoNotLandInTheImportingDocument()
    {
        // The importing file is deliberately shorter than the imported one, and its
        // leading block comment is where stray offsets used to land.
        var depSource = new string('\n', 40) + """
            [pointer_default(unique)]
            interface IImported { HRESULT Foo(); }
            """;
        Write("dep.idl", depSource);

        const string main = """
            /*
            License header that must never be squiggled.
            */
            import "dep.idl";
            namespace N { runtimeclass C { } }
            """;

        var c = CompileMain(main);

        var commentEnd = main.IndexOf("*/", StringComparison.Ordinal) + 2;
        var mainPath = Path.Combine(_dir, "main.idl");

        foreach (var d in c.Diagnostics)
        {
            bool isMain = d.SourceFile == null ||
                string.Equals(Path.GetFullPath(d.SourceFile), Path.GetFullPath(mainPath),
                    StringComparison.OrdinalIgnoreCase);
            if (!isMain) continue;

            var pos = d.OriginalPosition ?? d.Position;
            Assert.False(pos < commentEnd,
                $"{d.Code} attributed to the main file at {pos}, inside its license comment. "
                + $"Message: {d.Message}");
        }
    }

    [Fact]
    public void MainFileDiagnostics_KeepNullOrMainSourceFile()
    {
        // Regression guard: fixing imports must not push main-file diagnostics elsewhere.
        var mainPath = Write("solo.idl", "x");
        const string main = """
            namespace N { runtimeclass c { } }
            """;
        var c = CompilationHelper.Compile(main, referencePaths: null, mode: CompilationMode.Strict,
            includePaths: new[] { _dir }, currentFilePath: mainPath);

        Assert.NotEmpty(c.Diagnostics);
        foreach (var d in c.Diagnostics)
        {
            Assert.True(d.SourceFile == null ||
                string.Equals(Path.GetFullPath(d.SourceFile), Path.GetFullPath(mainPath),
                    StringComparison.OrdinalIgnoreCase),
                $"{d.Code} attributed to unexpected file {d.SourceFile}");
        }
    }

    [Fact]
    public void TransitiveImports_AreAttributedToTheirOwnFile()
    {
        var innerPath = Write("inner.idl", """
            [pointer_default(unique)]
            interface IInner { HRESULT Deep(); }
            """);
        Write("outer.idl", """
            import "inner.idl";
            interface IOuter { HRESULT Mid(); }
            """);

        var c = CompileMain("""
            import "outer.idl";
            namespace N { runtimeclass C { } }
            """);

        var d = Assert.Single(c.Diagnostics.Where(x => x.Code == "MIDL2003")
            .GroupBy(x => x.SourceFile).Select(g => g.Key));

        Assert.NotNull(d);
        Assert.Equal(Path.GetFullPath(innerPath), Path.GetFullPath(d!));
    }

    /// <summary>
    /// The importing document must keep reporting its <em>own</em> classic constructs,
    /// at their real offsets, no matter how large the imported files are.
    ///
    /// <c>_strictTokenIndex</c> is a monotonic cursor into <c>_posMap</c>. Binding an
    /// imported tree swaps <c>_posMap</c> but used to leave the cursor pointing at
    /// whatever index the (much longer) imported token stream had reached. Every
    /// subsequent <c>FindToken</c> against the main file's shorter stream then started
    /// past the end and returned -1, so the main document's strict-mode diagnostics
    /// were reported at offset 0 (a squiggle on unrelated text) or dropped outright.
    /// </summary>
    [Fact]
    public void MainFileStrictDiagnostics_SurviveALargeImport()
    {
        // Long enough that the imported token cursor runs well past the main file's stream.
        var big = string.Concat(Enumerable.Range(0, 40).Select(i =>
            $"interface IBig{i} {{ HRESULT M{i}([in] long a{i}, [in] short b{i}); }}\n"));
        Write("big.idl", big);

        const string main = """
            import "big.idl";

            namespace Probe
            {
                runtimeclass Widget
                {
                    void Go(long mainLongToken);
                }
            }
            """;

        var comp = CompileMain(main);
        var mainPath = Path.Combine(_dir, "main.idl");

        var mine = comp.Diagnostics
            .Where(d => d.SourceFile == null || d.SourceFile == mainPath)
            .Where(d => d.Code == "MIDL2005" && d.Message.Contains("'long'"))
            .ToList();

        Assert.True(mine.Count > 0,
            "The main file's own lowercase 'long' was dropped once a large file was imported. " +
            $"({comp.Diagnostics.Count} diagnostics total, none attributed to main.idl.)");

        // Assert the exact offset, not merely "non-zero": a position-0 fallback is a
        // squiggle painted on text that is certainly wrong, which is worse than silence.
        int expected = main.IndexOf("long mainLongToken", StringComparison.Ordinal);
        Assert.All(mine, d => Assert.Equal(expected, Painted(d)));
    }

    /// <summary>
    /// The same source must produce the same main-file diagnostics whether or not an
    /// unrelated file is imported. Imports may only <em>add</em> diagnostics.
    /// </summary>
    [Fact]
    public void ImportingAFile_DoesNotChangeTheMainFilesOwnDiagnostics()
    {
        var body = """

            namespace Probe
            {
                runtimeclass Widget
                {
                    void Go(long a, short b, float c);
                }
            }
            """;

        var withoutImport = CompileMain(body, "solo.idl");
        var soloPath = Path.Combine(_dir, "solo.idl");
        var soloOwn = withoutImport.Diagnostics
            .Where(d => d.SourceFile == null || d.SourceFile == soloPath)
            .Select(d => (d.Code, Position: Painted(d)))
            .OrderBy(x => x.Position).ThenBy(x => x.Code).ToList();

        Assert.NotEmpty(soloOwn);

        Write("big.idl", string.Concat(Enumerable.Range(0, 40).Select(i =>
            $"interface IBig{i} {{ HRESULT M{i}([in] long a{i}, [in] short b{i}); }}\n")));

        // Same body, byte for byte, so every offset in it is unchanged.
        var withImport = CompileMain("import \"big.idl\";" + body);
        var mainPath = Path.Combine(_dir, "main.idl");
        int shift = "import \"big.idl\";".Length;
        var mainOwn = withImport.Diagnostics
            .Where(d => d.SourceFile == null || d.SourceFile == mainPath)
            .Select(d => (d.Code, Position: Painted(d) - shift))
            .OrderBy(x => x.Position).ThenBy(x => x.Code).ToList();

        Assert.Equal(soloOwn, mainOwn);
    }

    /// <summary>
    /// Type resolution is a <em>deferred</em> pass: <c>TrackTypeRef</c> queues every
    /// reference during binding and <c>TypeResolver.ResolveAll</c> runs after all trees
    /// (including imported ones) have been bound.
    ///
    /// That put "undefined type" diagnostics outside the import loop's tagging range, so
    /// they kept a null SourceFile and were attributed to the importing document — at an
    /// offset that only means something in the imported file. Importing a classic system
    /// IDL painted hundreds of these across unrelated text.
    /// </summary>
    [Fact]
    public void UndefinedTypeDiagnostics_AreAttributedToTheFileThatUsesTheType()
    {
        var usesPath = Write("uses.idl",
            "interface IUses { HRESULT M([in] NoSuchType_Xyz count); }\n");

        // The main file never mentions the type.
        const string main = """
            import "uses.idl";

            namespace Probe
            {
                runtimeclass Widget
                {
                    Int32 Value;
                }
            }
            """;

        var comp = CompileMain(main);
        var mainPath = Path.Combine(_dir, "main.idl");

        var undefined = comp.Diagnostics
            .Where(d => d.Message.Contains("NoSuchType_Xyz"))
            .ToList();

        Assert.NotEmpty(undefined);
        Assert.All(undefined, d => Assert.Equal(usesPath, d.SourceFile));
        Assert.DoesNotContain(undefined, d => d.SourceFile == null || d.SourceFile == mainPath);
    }

    /// <summary>
    /// And the offset must be meaningful in the file it is attributed to — pointing at the
    /// type name itself, not at whatever text happens to sit at that offset elsewhere.
    /// </summary>
    [Fact]
    public void UndefinedTypeDiagnostics_PointAtTheTypeInTheImportedFile()
    {
        const string usesSource = "interface IUses { HRESULT M([in] NoSuchType_Xyz count); }\n";
        var usesPath = Write("uses.idl", usesSource);

        var comp = CompileMain("import \"uses.idl\";\n\nnamespace Probe { struct S { Int32 A; }; }\n");

        var d = comp.Diagnostics.First(x => x.Message.Contains("NoSuchType_Xyz"));

        int expected = usesSource.IndexOf("NoSuchType_Xyz", StringComparison.Ordinal);
        Assert.Equal(expected, Painted(d));
    }

    /// <summary>
    /// The importing file's own undefined types must still be reported against it.
    /// </summary>
    [Fact]
    public void UndefinedTypesInTheMainFile_AreStillReportedAgainstTheMainFile()
    {
        Write("uses.idl", "interface IUses { HRESULT M([in] NoSuchType_Xyz count); }\n");

        const string main = """
            import "uses.idl";

            namespace Probe
            {
                struct S { MainOnlyBogusType A; };
            }
            """;

        var comp = CompileMain(main);
        var mainPath = Path.Combine(_dir, "main.idl");

        var mine = comp.Diagnostics.Where(d => d.Message.Contains("MainOnlyBogusType")).ToList();

        Assert.NotEmpty(mine);
        Assert.All(mine, d => Assert.True(d.SourceFile == null || d.SourceFile == mainPath,
            $"expected main.idl (or null), got {d.SourceFile}"));
        Assert.All(mine, d => Assert.Equal(main.IndexOf("MainOnlyBogusType", StringComparison.Ordinal), Painted(d)));
    }
}
