// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;

namespace Midl.Analysis.Tests;

/// <summary>
/// A diagnostic's position must be an offset into the <em>original</em> text of the
/// <em>file it came from</em> — always, and by construction.
///
/// Positions are produced against preprocessed text. The main document has a
/// <see cref="SourceMap"/> that translates those offsets back to original files and
/// offsets, but every imported file was preprocessed with its segments discarded, so
/// nothing could translate them. Any macro expansion inside an imported file therefore
/// shifted every position after it.
///
/// These tests deliberately do <em>not</em> assert "the position is inside the file".
/// A wrong position that happens to land in bounds is just as wrong, and that is exactly
/// what the original report looked like: <c>Undefined type 'ULONG'</c> painted on a
/// <c>cpp_quote</c> line well inside the file. Each test below pins the exact offset of
/// the construct the diagnostic is about.
/// </summary>
public class CompilationSourceMapTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "midl-csm-" + Guid.NewGuid().ToString("N"));

    public CompilationSourceMapTests() => Directory.CreateDirectory(_dir);

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

    /// <summary>The offset the LSP and CLI actually paint.</summary>
    private static int Painted(Diagnostics.Diagnostic d) => d.OriginalPosition ?? d.Position;

    private const string MainThatImports =
        "import \"imported.idl\";\n\nnamespace Probe { struct S { Int32 A; }; }\n";

    /// <summary>
    /// A macro that expands to something longer shifts every offset after it in the
    /// preprocessed text. Without a source map for the imported file, the reported
    /// position is that shifted offset applied to the original file.
    /// </summary>
    [Fact]
    public void MacroExpansionInAnImport_DoesNotShiftPositionsAfterIt()
    {
        // WIDGET_T (8 chars) expands to 40, so everything after drifts by +32.
        const string imported = """
            #define WIDGET_T ThisIsAVeryLongExpandedTypeNameXXXXXXXX
            namespace Imported
            {
                struct Uses { WIDGET_T A; };
                struct Later { NoSuchTypeXyz B; };
            }
            """;
        var importedPath = Write("imported.idl", imported);

        var comp = CompileMain(MainThatImports);

        var d = Assert.Single(comp.Diagnostics, x => x.Message.Contains("NoSuchTypeXyz"));

        Assert.Equal(importedPath, d.SourceFile);
        Assert.Equal(imported.IndexOf("NoSuchTypeXyz", StringComparison.Ordinal), Painted(d));
    }

    /// <summary>
    /// The same, for a diagnostic raised during binding rather than in a deferred pass —
    /// the two travel through different code paths.
    /// </summary>
    [Fact]
    public void MacroExpansionInAnImport_DoesNotShiftBindingDiagnostics()
    {
        const string imported = """
            #define WIDGET_T ThisIsAVeryLongExpandedTypeNameXXXXXXXX
            namespace Imported
            {
                struct Uses { WIDGET_T A; };
                interface ILater
                {
                    HRESULT Go(Int32 x);
                }
            }
            """;
        var importedPath = Write("imported.idl", imported);

        var comp = CompileMain(MainThatImports);

        var d = Assert.Single(comp.Diagnostics, x => x.Code == "MIDL2002");

        Assert.Equal(importedPath, d.SourceFile);
        Assert.Equal(imported.IndexOf("HRESULT", StringComparison.Ordinal), Painted(d));
    }

    /// <summary>
    /// The main document's own <c>#include</c>s must resolve to the included file, not to
    /// the includer — the position is only meaningful in the file the text lives in. This
    /// worked before, and must keep working now that the map is assembled per unit.
    /// </summary>
    [Fact]
    public void DiagnosticsFromAnIncludedFile_NameTheIncludedFile()
    {
        const string inner = """
            namespace Inner
            {
                struct FromInclude { NoSuchTypeXyz A; };
            }
            """;
        var innerPath = Write("inner.h", inner);

        var comp = CompileMain("namespace Probe { struct S { Int32 A; }; }\n#include \"inner.h\"\n");

        var d = Assert.Single(comp.Diagnostics, x => x.Message.Contains("NoSuchTypeXyz"));

        Assert.Equal(innerPath, d.SourceFile);
        Assert.Equal(inner.IndexOf("NoSuchTypeXyz", StringComparison.Ordinal), Painted(d));
    }

    /// <summary>
    /// An imported file may itself <c>#include</c> another. A diagnostic about text that
    /// came from the included file must name that file, not the one that included it —
    /// the position is only meaningful in the file the text actually lives in.
    /// </summary>
    [Fact]
    public void DiagnosticsFromAFileIncludedByAnImport_NameTheIncludedFile()
    {
        const string inner = """
            namespace Inner
            {
                struct FromInclude { NoSuchTypeXyz A; };
            }
            """;
        var innerPath = Write("inner.h", inner);

        Write("imported.idl", """
            namespace Imported
            {
                struct Before { Int32 A; };
            }
            #include "inner.h"
            """);

        var comp = CompileMain(MainThatImports);

        var d = Assert.Single(comp.Diagnostics, x => x.Message.Contains("NoSuchTypeXyz"));

        Assert.Equal(innerPath, d.SourceFile);
        Assert.Equal(inner.IndexOf("NoSuchTypeXyz", StringComparison.Ordinal), Painted(d));
    }

    /// <summary>
    /// Nothing above may come at the cost of the main document, which already had a
    /// source map: its own macro expansions must still resolve to original offsets.
    /// </summary>
    [Fact]
    public void MacroExpansionInTheMainFile_StillResolvesToOriginalOffsets()
    {
        const string main = """
            #define WIDGET_T ThisIsAVeryLongExpandedTypeNameXXXXXXXX
            namespace Probe
            {
                struct Uses { WIDGET_T A; };
                struct Later { NoSuchTypeXyz B; };
            }
            """;

        var comp = CompileMain(main);

        var d = Assert.Single(comp.Diagnostics, x => x.Message.Contains("NoSuchTypeXyz"));

        Assert.Equal(main.IndexOf("NoSuchTypeXyz", StringComparison.Ordinal), Painted(d));
    }
}
