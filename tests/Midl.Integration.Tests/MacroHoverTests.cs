// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Midl.Analysis;
using Midl.Lsp;
using Midl.Lsp.Handlers;
using Midl.Lsp.Protocol;

namespace Midl.Integration.Tests;

/// <summary>
/// A preprocessor macro is expanded away before the binder ever sees it, but the editor
/// still shows the original text. Until now that left a macro looking like an ordinary
/// identifier: `#define FOO` followed by `Int32 FOO x;` hovered as plain text, giving no
/// hint that FOO expands to nothing and that the line therefore reads `Int32 x;`.
///
/// That is precisely the case that makes a file confusing to read, so hover has to say so.
/// </summary>
public class MacroHoverTests
{
    private const string Uri = "file:///macros.idl";

    private static DocumentManager Docs(string source)
    {
        var dm = new DocumentManager();
        dm.OpenDocument(Uri, source);
        return dm;
    }

    private static string? HoverAt(string source, int line, int character)
    {
        var json = "{\"textDocument\":{\"uri\":\"" + Uri + "\"},\"position\":{\"line\":" +
                   line + ",\"character\":" + character + "}}";
        var result = HoverHandler.Handle(JsonSerializer.Deserialize<JsonElement>(json), Docs(source));

        return result is null
            ? null
            : JsonSerializer.Serialize(result, LspJsonContext.Default.HoverResult);
    }

    // -----------------------------------------------------------------------
    // Object-like macros
    // -----------------------------------------------------------------------

    private const string EmptyMacro = """
        #define FOO

        namespace N
        {
            runtimeclass C
            {
                Int32 FOO x;
            }
        }
        """;

    [Fact]
    public void AMacroThatExpandsToNothingSaysSo()
    {
        // Line 6 is "        Int32 FOO x;" — character 14 is inside FOO.
        var json = HoverAt(EmptyMacro, 6, 14);

        Assert.NotNull(json);
        Assert.Contains("macro", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("FOO", json);
    }

    [Fact]
    public void AMacroHoverReportsAnEmptyExpansionExplicitly()
    {
        // "expands to nothing" is the whole point — an empty code span would read as a
        // rendering glitch rather than as information.
        var json = HoverAt(EmptyMacro, 6, 14);

        Assert.NotNull(json);
        Assert.Contains("nothing", json, StringComparison.OrdinalIgnoreCase);
    }

    private const string ValueMacro = """
        #define WIDTH 1920

        namespace N
        {
            runtimeclass C
            {
                Int32 WIDTH;
            }
        }
        """;

    [Fact]
    public void AMacroWithAValueShowsTheValue()
    {
        var json = HoverAt(ValueMacro, 6, 14);

        Assert.NotNull(json);
        Assert.Contains("1920", json);
    }

    // -----------------------------------------------------------------------
    // Function-like macros
    // -----------------------------------------------------------------------

    private const string FunctionMacro = """
        #define SQUARE(x) ((x) * (x))

        namespace N
        {
            runtimeclass C
            {
                Int32 SQUARE;
            }
        }
        """;

    [Fact]
    public void AFunctionLikeMacroShowsItsParameters()
    {
        var json = HoverAt(FunctionMacro, 6, 14);

        Assert.NotNull(json);
        Assert.Contains("SQUARE", json);
        Assert.Contains("x", json);
    }

    // -----------------------------------------------------------------------
    // Guards — hover must not start claiming everything is a macro.
    // -----------------------------------------------------------------------

    [Fact]
    public void AMacroDefinedInSourceIsNotCalledPredefined()
    {
        // Regression: IsBuiltIn was derived from "has no file path", but an editor compiles
        // in-memory text with no path at all — so every macro in the document claimed to be
        // predefined by the compiler, which is the opposite of the truth.
        var json = HoverAt(EmptyMacro, 6, 14);

        Assert.NotNull(json);
        Assert.DoesNotContain("Predefined", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("line 1", json);
    }

    [Fact]
    public void ACompilerPredefinedMacroSaysSo()
    {
        const string source = """
            namespace N
            {
                runtimeclass C
                {
                    Int32 MIDL_PASS;
                }
            }
            """;

        // MIDL_PASS is predefined by the preprocessor, so it has no definition site here.
        var json = HoverAt(source, 4, 14);

        Assert.NotNull(json);
        Assert.Contains("Predefined", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Defined on line", json);
    }

    [Fact]
    public void AnOrdinaryIdentifierIsNotReportedAsAMacro()
    {
        // "C" is a runtimeclass, not a macro.
        var json = HoverAt(EmptyMacro, 4, 18);

        if (json is not null)
            Assert.DoesNotContain("macro", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AKeywordIsStillDocumentedAsAKeyword()
    {
        // Keyword docs must keep priority over the macro lookup.
        var json = HoverAt(EmptyMacro, 4, 4);

        Assert.NotNull(json);
        Assert.Contains("runtimeclass", json);
        Assert.DoesNotContain("macro", json, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// A macro defined in an #include carries an offset into <em>that</em> file. Reported
    /// against the open document's line starts it yields a confident, wrong line number —
    /// so the definition site is verified before it is claimed.
    /// </summary>
    [Fact]
    public void AMacroFromAnIncludeDoesNotClaimALineInThisFile()
    {
        var dir = Path.Combine(Path.GetTempPath(), "midl-macro-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "header.h"), "#define FROM_HEADER 42\n");

            const string source = """
                #include "header.h"

                namespace N
                {
                    runtimeclass C
                    {
                        Int32 FROM_HEADER;
                    }
                }
                """;

            var compilation = CompilationHelper.Compile(
                source, referencePaths: null, mode: CompilationMode.Compat,
                includePaths: new[] { dir }, currentFilePath: null);

            Assert.True(compilation.Macros.ContainsKey("FROM_HEADER"),
                "the include should have been resolved for this test to mean anything");

            var json = HoverAtWith(source, 6, 16, compilation);

            Assert.NotNull(json);
            Assert.Contains("header.h", json);
            // Line 1 of this document is the #include, not a #define.
            Assert.DoesNotContain("Defined on line", json);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    private static string? HoverAtWith(string source, int line, int character, Compilation compilation)
    {
        var json = "{\"textDocument\":{\"uri\":\"" + Uri + "\"},\"position\":{\"line\":" +
                   line + ",\"character\":" + character + "}}";
        var result = HoverHandler.Handle(JsonSerializer.Deserialize<JsonElement>(json), Docs(source), compilation);

        return result is null
            ? null
            : JsonSerializer.Serialize(result, LspJsonContext.Default.HoverResult);
    }
}
