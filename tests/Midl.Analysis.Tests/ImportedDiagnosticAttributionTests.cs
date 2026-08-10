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
}
