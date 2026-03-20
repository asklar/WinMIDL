using System.Text.Json;
using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;
using Midl.CodeGen.WinMd;
using Midl.Lsp;
using Midl.Lsp.Handlers;

namespace Midl.Integration.Tests;

public class LspHandlerTests
{
    private static DocumentManager CreateDocManager(string uri, string source)
    {
        var dm = new DocumentManager();
        dm.OpenDocument(uri, source);
        return dm;
    }

    private static JsonElement MakeTextDocParams(string uri)
    {
        return JsonDocument.Parse($$"""
            { "textDocument": { "uri": "{{uri}}" } }
            """).RootElement;
    }

    private static JsonElement MakePositionParams(string uri, int line, int character)
    {
        return JsonDocument.Parse($$"""
            {
                "textDocument": { "uri": "{{uri}}" },
                "position": { "line": {{line}}, "character": {{character}} }
            }
            """).RootElement;
    }

    #region Completion Tests

    [Fact]
    public void CompletionIncludesKeywords()
    {
        var uri = "file:///test.midl";
        var dm = CreateDocManager(uri, "namespace Foo { }");
        // Position NOT inside brackets — should return keywords
        var result = CompletionHandler.Handle(MakePositionParams(uri, 0, 16), dm);

        var json = JsonSerializer.Serialize(result);

        Assert.Contains("\"namespace\"", json);
        Assert.Contains("\"runtimeclass\"", json);
        Assert.Contains("\"interface\"", json);
        Assert.Contains("\"String\"", json);
        Assert.Contains("\"Int32\"", json);
    }

    [Fact]
    public void CompletionInAttributeBracket_ShowsOnlyAttributes()
    {
        var uri = "file:///test.midl";
        var source = "namespace TestApp {\n    [\n    runtimeclass Foo { }\n}";
        //                                            ^ cursor at line 1, char 5 (inside '[')
        var dm = CreateDocManager(uri, source);
        var result = CompletionHandler.Handle(MakePositionParams(uri, 1, 5), dm);

        var json = JsonSerializer.Serialize(result);

        // Should include attributes applicable to runtimeclass
        Assert.Contains("activatable", json);
        Assert.Contains("composable", json);
        Assert.Contains("threading", json);
        Assert.Contains("contract", json);
        Assert.Contains("uuid", json);

        // Should NOT include keywords/types (those are for non-attribute context)
        Assert.DoesNotContain("\"namespace\"", json);
        Assert.DoesNotContain("\"Int32\"", json);
        Assert.DoesNotContain("\"IVector\"", json);
    }

    [Fact]
    public void CompletionInAttributeBracket_Interface_ShowsInterfaceAttrs()
    {
        var uri = "file:///test.midl";
        var source = "namespace N {\n    [\n    interface IFoo { }\n}";
        var dm = CreateDocManager(uri, source);
        var result = CompletionHandler.Handle(MakePositionParams(uri, 1, 5), dm);

        var json = JsonSerializer.Serialize(result);

        // Interface-specific attributes
        Assert.Contains("exclusiveto", json);
        Assert.Contains("uuid", json);
        Assert.Contains("contract", json);

        // Runtimeclass-only attributes should NOT appear
        Assert.DoesNotContain("activatable", json);
        Assert.DoesNotContain("composable", json);
    }

    [Fact]
    public void CompletionInAttributeBracket_Enum_ShowsFlagsAttr()
    {
        var uri = "file:///test.midl";
        var source = "namespace N {\n    [\n    enum Color { Red, Green }\n}";
        var dm = CreateDocManager(uri, source);
        var result = CompletionHandler.Handle(MakePositionParams(uri, 1, 5), dm);

        var json = JsonSerializer.Serialize(result);
        Assert.Contains("flags", json);
        Assert.DoesNotContain("activatable", json);
    }

    [Fact]
    public void CompletionInAttributeBracket_IncludesUserDefinedAttrs()
    {
        var uri = "file:///test.midl";
        var source = """
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyTag
                {
                    Int32 Level;
                };

                []
                runtimeclass Foo { }
            }
            """;
        // Cursor at line 8, char 5 — inside '[]' before runtimeclass Foo
        var dm = CreateDocManager(uri, source);
        var result = CompletionHandler.Handle(MakePositionParams(uri, 8, 5), dm);

        var json = JsonSerializer.Serialize(result);
        Assert.Contains("MyTag", json);
    }

    [Fact]
    public void CompletionOnImportLine_ShowsIdlFiles()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"midl_import_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            // Create sibling IDL files
            File.WriteAllText(Path.Combine(tempDir, "Helper.idl"), "namespace H { }");
            File.WriteAllText(Path.Combine(tempDir, "Types.idl"), "namespace T { }");
            File.WriteAllText(Path.Combine(tempDir, "NotIdl.txt"), "hello");

            var uri = "file:///" + Uri.EscapeDataString(Path.Combine(tempDir, "Widget.idl")).Replace("\\", "/");
            var source = "import \"\"\nnamespace W { }";
            var dm = CreateDocManager(uri, source);

            // Cursor at line 0, char 8 — inside import ""
            var result = CompletionHandler.Handle(MakePositionParams(uri, 0, 8), dm,
                includePaths: new[] { tempDir });

            var json = JsonSerializer.Serialize(result);

            // Should include .idl files
            Assert.Contains("Helper.idl", json);
            Assert.Contains("Types.idl", json);

            // Should NOT include non-.idl files
            Assert.DoesNotContain("NotIdl.txt", json);

            // Should NOT include keywords
            Assert.DoesNotContain("\"runtimeclass\"", json);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void CompletionOnImportLine_FiltersPartialTyping()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"midl_import_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "Helper.idl"), "namespace H { }");
            File.WriteAllText(Path.Combine(tempDir, "Types.idl"), "namespace T { }");

            var uri = "file:///" + Uri.EscapeDataString(Path.Combine(tempDir, "Widget.idl")).Replace("\\", "/");
            var source = "import \"Hel\"\nnamespace W { }";
            var dm = CreateDocManager(uri, source);

            var result = CompletionHandler.Handle(MakePositionParams(uri, 0, 11), dm,
                includePaths: new[] { tempDir });

            var json = JsonSerializer.Serialize(result);
            Assert.Contains("Helper.idl", json);
            Assert.DoesNotContain("Types.idl", json);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void CompletionIncludesExternalTypes()
    {
        // Create a reference .winmd with an external type
        var refSource = """
            namespace ExtLib
            {
                enum ExtStatus { Idle, Active }
            }
            """;
        var refCompilation = CompilationHelper.Compile(refSource);
        Assert.Empty(refCompilation.Diagnostics);

        var emitter = new WinMdEmitter();
        var winmdBytes = emitter.EmitToBytes(refCompilation);

        var tempPath = Path.Combine(Path.GetTempPath(), $"lsptest_{Guid.NewGuid():N}.winmd");
        try
        {
            File.WriteAllBytes(tempPath, winmdBytes);

            // Compile app source with reference, to verify external types resolve
            var appSource = """
                namespace MyApp
                {
                    runtimeclass Widget { Widget(); ExtLib.ExtStatus GetStatus(); }
                }
                """;
            var compilation = CompilationHelper.Compile(appSource, new[] { tempPath });
            var extType = compilation.GetTypeByName("ExtLib.ExtStatus");

            Assert.NotNull(extType);
            Assert.True(extType!.IsExternal);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    #endregion

    #region Hover Tests

    [Fact]
    public void HoverOnKeywordReturnsDoc()
    {
        var uri = "file:///test.midl";
        var source = "runtimeclass Foo { }";
        var dm = CreateDocManager(uri, source);

        // Position on "runtimeclass" keyword (line 0, char 0)
        var result = HoverHandler.Handle(MakePositionParams(uri, 0, 0), dm);
        Assert.NotNull(result);

        var json = JsonSerializer.Serialize(result);
        Assert.Contains("runtimeclass", json);
        Assert.Contains("Windows Runtime class", json);
    }

    [Fact]
    public void HoverOnTypeReturnsSymbolInfo()
    {
        var uri = "file:///test.midl";
        var source = """
            namespace N
            {
                runtimeclass Foo
                {
                }
            }
            """;
        var dm = CreateDocManager(uri, source);

        // Find line/char of "Foo" in "runtimeclass Foo"
        var lines = source.Split('\n');
        int fooLine = -1, fooChar = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            int idx = lines[i].IndexOf("runtimeclass Foo");
            if (idx >= 0)
            {
                fooLine = i;
                fooChar = idx + "runtimeclass ".Length; // position on "Foo"
                break;
            }
        }
        Assert.True(fooLine >= 0);

        var result = HoverHandler.Handle(MakePositionParams(uri, fooLine, fooChar), dm);
        Assert.NotNull(result);

        var json = JsonSerializer.Serialize(result);
        Assert.Contains("N.Foo", json);
        Assert.Contains("RuntimeClass", json);
    }

    #endregion

    #region Document Symbol Tests

    [Fact]
    public void DocumentSymbolsReturnsOutline()
    {
        var uri = "file:///test.midl";
        var source = """
            namespace MyLib
            {
                runtimeclass Widget { void DoWork(); }
                interface IFoo { void Bar(); }
            }
            """;
        var dm = CreateDocManager(uri, source);

        var result = DocumentSymbolHandler.Handle(MakeTextDocParams(uri), dm);
        Assert.NotNull(result);

        var json = JsonSerializer.Serialize(result);
        // Verify namespace and type names appear in the outline
        Assert.Contains("\"MyLib\"", json);
        Assert.Contains("\"Widget\"", json);
        Assert.Contains("\"IFoo\"", json);
        Assert.Contains("\"DoWork\"", json);
        Assert.Contains("\"Bar\"", json);
    }

    #endregion

    #region Diagnostics Conversion Tests

    [Fact]
    public void DiagnosticsPublisherConvertsOffsets()
    {
        // Verify PositionHelper correctly converts offsets to line/column.
        var source = "namespace Foo {\n  runtimeclass";
        var lineStarts = PositionHelper.BuildLineStarts(source);

        // Offset 15 is the '\n' + start of line 1
        // "namespace Foo {" is 15 chars (indices 0-14), '\n' at 14
        // Actually let's compute: "namespace Foo {\n" — 'n' at 0, '\n' at 15
        // Line 0 starts at offset 0, line 1 starts at offset 16
        // Offset 15 is the '\n' which is still on line 0

        // Position 0 should be (0, 0)
        var (line0, char0) = PositionHelper.OffsetToLineChar(lineStarts, 0);
        Assert.Equal(0, line0);
        Assert.Equal(0, char0);

        // First char of second line
        var (line1, char1) = PositionHelper.OffsetToLineChar(lineStarts, 16);
        Assert.Equal(1, line1);
        Assert.Equal(0, char1);

        // "runtimeclass" starts at offset 18 (line 1, char 2)
        var (line2, char2) = PositionHelper.OffsetToLineChar(lineStarts, 18);
        Assert.Equal(1, line2);
        Assert.Equal(2, char2);

        // Verify roundtrip
        int offset = PositionHelper.LineCharToOffset(lineStarts, 1, 2);
        Assert.Equal(18, offset);
    }

    #endregion

    #region Semantic Token Tests

    [Fact]
    public void SemanticTokensClassifiesKeyword()
    {
        var source = "namespace TestApp { }";
        var (data, tokenTypes, _) = SemanticTokensHandler.HandleCore(source);

        Assert.True(data.Length >= 5);

        int keywordIdx = Array.IndexOf(tokenTypes, "keyword");
        Assert.True(keywordIdx >= 0);

        // "namespace" is at line 0, char 0, length 9, type = keyword
        Assert.Equal(0, data[0]); // deltaLine
        Assert.Equal(0, data[1]); // deltaChar
        Assert.Equal(9, data[2]); // length
        Assert.Equal(keywordIdx, data[3]); // keyword type
    }

    [Fact]
    public void SemanticTokensClassifiesType()
    {
        var source = """
            namespace TestApp
            {
                runtimeclass Widget
                {
                    String GetName();
                }
            }
            """;
        var (data, tokenTypes, _) = SemanticTokensHandler.HandleCore(source);
        Assert.True(data.Length > 0);

        // "String" is classified as keyword (built-in type keyword)
        int keywordIdx = Array.IndexOf(tokenTypes, "keyword");
        Assert.True(keywordIdx >= 0);

        // Walk the delta-encoded data to find a token matching "String" (length 6, keyword type)
        bool foundString = false;
        int line = 0, col = 0;
        for (int i = 0; i + 4 < data.Length; i += 5)
        {
            int deltaLine = data[i];
            int deltaChar = data[i + 1];
            int length = data[i + 2];
            int tokenType = data[i + 3];

            if (deltaLine > 0) { line += deltaLine; col = deltaChar; }
            else { col += deltaChar; }

            if (length == 6 && tokenType == keywordIdx)
            {
                var lineStarts = PositionHelper.BuildLineStarts(source);
                int offset = PositionHelper.LineCharToOffset(lineStarts, line, col);
                if (offset + 6 <= source.Length && source.Substring(offset, 6) == "String")
                {
                    foundString = true;
                    break;
                }
            }
        }
        Assert.True(foundString, "String should be classified as keyword token type");
    }

    #endregion

    #region Formatting Tests

    [Fact]
    public void FormattingFixesIndentation()
    {
        var input = "namespace MyLib{\nruntimeclass Widget{\nvoid DoWork();\n}\n}";
        var formatted = FormattingHandler.Format(input);

        Assert.Contains("namespace MyLib", formatted);
        Assert.Contains("    runtimeclass Widget", formatted);
        Assert.Contains("        void DoWork();", formatted);
    }

    #endregion

    #region Find References Tests

    [Fact]
    public void FindReferencesFindsUsages()
    {
        var source = """
            namespace TestApp
            {
                interface IFoo
                {
                    void DoWork();
                }
                runtimeclass Bar : IFoo
                {
                }
            }
            """;
        var uri = "file:///test.midl";

        // Find the line/char of "IFoo" in the interface declaration
        var lines = source.Split('\n');
        int targetLine = -1, targetChar = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            int idx = lines[i].IndexOf("interface IFoo");
            if (idx >= 0)
            {
                targetLine = i;
                targetChar = idx + "interface ".Length; // position on "IFoo"
                break;
            }
        }
        Assert.True(targetLine >= 0, "Could not find IFoo declaration");

        var refs = FindReferencesHandler.HandleCore(source, uri, targetLine, targetChar);

        // Should find at least 2 references: the declaration and the usage in Bar's base list
        Assert.True(refs.Count >= 2, $"Expected at least 2 references, found {refs.Count}");

        var json = JsonSerializer.Serialize(refs);
        // Both occurrences of "IFoo" should be found
        Assert.Contains(uri, json);
    }

    #endregion
}
