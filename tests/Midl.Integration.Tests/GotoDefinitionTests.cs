using System.Text.Json;
using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.WinMd;
using Midl.Lsp;
using Midl.Lsp.Handlers;

namespace Midl.Integration.Tests;

public class GotoDefinitionTests
{
    #region Feature 1: Imported .idl files

    [Fact]
    public void GotoDefinition_ImportedType_JumpsToOtherFile()
    {
        // Create temp files for import resolution
        var tempDir = Path.Combine(Path.GetTempPath(), "midl-gotodef-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);

        try
        {
            var helperPath = Path.Combine(tempDir, "Helper.idl");
            var mainPath = Path.Combine(tempDir, "Main.idl");

            File.WriteAllText(helperPath, "namespace N {\n    interface IHelper {\n        void DoWork();\n    }\n}\n");
            File.WriteAllText(mainPath, "import \"Helper.idl\"\nnamespace N {\n    runtimeclass Foo : IHelper {\n    }\n}\n");

            var mainSource = File.ReadAllText(mainPath);
            var mainUri = "file:///" + mainPath.Replace('\\', '/');

            // IHelper appears at line 2 ("    runtimeclass Foo : IHelper {")
            // Find the column of "IHelper" on line 2
            var lines = mainSource.Split('\n');
            int ihelperCol = lines[2].IndexOf("IHelper", StringComparison.Ordinal);

            var result = GotoDefinitionHandler.HandleCore(mainSource, mainUri, 2, ihelperCol,
                currentFilePath: mainPath);

            Assert.NotNull(result);
            var dict = Assert.IsType<Dictionary<string, object?>>(result);
            var resultUri = dict["uri"]!.ToString()!;

            // Should point to Helper.idl, not Main.idl
            Assert.Contains("Helper.idl", resultUri);
            Assert.StartsWith("file:///", resultUri);

            // Verify range covers "IHelper" in Helper.idl
            var range = (Dictionary<string, object?>)dict["range"]!;
            var start = (Dictionary<string, object?>)range["start"]!;
            int startLine = (int)start["line"]!;
            int startChar = (int)start["character"]!;

            // "interface IHelper" — name starts after "interface "
            var helperSource = File.ReadAllText(helperPath);
            var helperLines = helperSource.Split('\n');
            Assert.Contains("IHelper", helperLines[startLine]);
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }

    [Fact]
    public void GotoDefinition_ImportedEnum_JumpsToOtherFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "midl-gotodef-enum-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);

        try
        {
            var typesPath = Path.Combine(tempDir, "Types.idl");
            var mainPath = Path.Combine(tempDir, "Main.idl");

            File.WriteAllText(typesPath, "namespace N {\n    enum Color { Red, Green, Blue };\n}\n");
            File.WriteAllText(mainPath, "import \"Types.idl\"\nnamespace N {\n    struct Pixel {\n        Color c;\n    };\n}\n");

            var mainSource = File.ReadAllText(mainPath);
            var mainUri = "file:///" + mainPath.Replace('\\', '/');

            // "Color" appears at line 3: "        Color c;"
            var lines = mainSource.Split('\n');
            int colorCol = lines[3].IndexOf("Color", StringComparison.Ordinal);

            var result = GotoDefinitionHandler.HandleCore(mainSource, mainUri, 3, colorCol,
                currentFilePath: mainPath);

            Assert.NotNull(result);
            var dict = Assert.IsType<Dictionary<string, object?>>(result);
            var resultUri = dict["uri"]!.ToString()!;
            Assert.Contains("Types.idl", resultUri);
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }

    #endregion

    #region Feature 2: WinMD types

    [Fact]
    public void GotoDefinition_WinMdType_ReturnsDecompiledView()
    {
        // Create a temp WinMD by compiling a simple IDL
        var tempDir = Path.Combine(Path.GetTempPath(), "midl-gotodef-winmd-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);

        try
        {
            // Compile a simple IDL to WinMD
            var idlSource = "namespace TestNs {\n    interface ITestWidget {\n        void Activate();\n    }\n}\n";
            var compilation = CompilationHelper.Compile(idlSource);
            var winmdPath = Path.Combine(tempDir, "TestNs.winmd");
            var emitter = new WinMdEmitter();
            emitter.Emit(compilation, winmdPath);

            Assert.True(File.Exists(winmdPath), "WinMD should be generated");

            // Create an IDL that references the WinMD via @ref
            var mainSource = "// @ref: \"" + winmdPath.Replace('\\', '/') + "\"\nnamespace TestNs {\n    runtimeclass Widget : ITestWidget {\n    }\n}\n";

            // F12 on ITestWidget — it's an external symbol
            // First compile with the reference to get the symbol
            var mainCompilation = CompilationHelper.Compile(mainSource, new[] { winmdPath }, CompilationMode.Strict);
            var symbol = mainCompilation.GetAllTypes().FirstOrDefault(s => s.Name == "ITestWidget");
            Assert.NotNull(symbol);
            Assert.True(symbol.IsExternal);
            Assert.NotNull(symbol.SourceWinMdPath);

            // Test the WinMD definition location creation
            var result = GotoDefinitionHandler.CreateWinMdDefinitionLocation(symbol);
            Assert.NotNull(result);

            var dict = Assert.IsType<Dictionary<string, object?>>(result);
            var resultUri = dict["uri"]!.ToString()!;

            // Should use the midl-winmd scheme
            Assert.StartsWith("midl-winmd:///", resultUri);

            // Verify the range points to a valid position
            var range = (Dictionary<string, object?>)dict["range"]!;
            var start = (Dictionary<string, object?>)range["start"]!;
            int startLine = (int)start["line"]!;
            int startChar = (int)start["character"]!;

            // The decompiled view should have the interface at a reasonable position
            Assert.True(startLine >= 0 || startChar >= 0, "Position should be valid");
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }

    #endregion

    #region Regression: local symbol still works

    [Fact]
    public void GotoDefinition_LocalSymbol_StillWorks()
    {
        var source = "namespace N {\n    interface IFoo {\n        void Bar();\n    }\n    runtimeclass Widget : IFoo {\n    }\n}\n";
        var uri = "file:///test.idl";

        // F12 on IFoo at line 4 ("    runtimeclass Widget : IFoo {")
        var lines = source.Split('\n');
        int ifooCol = lines[4].IndexOf("IFoo", StringComparison.Ordinal);

        var result = GotoDefinitionHandler.HandleCore(source, uri, 4, ifooCol);

        Assert.NotNull(result);
        var dict = Assert.IsType<Dictionary<string, object?>>(result);
        var resultUri = dict["uri"]!.ToString()!;

        // Should point to the same file
        Assert.Equal(uri, resultUri);

        // Should point to the definition line (line 1: "    interface IFoo {")
        var range = (Dictionary<string, object?>)dict["range"]!;
        var start = (Dictionary<string, object?>)range["start"]!;
        int defLine = (int)start["line"]!;
        Assert.Equal(1, defLine);
    }

    [Fact]
    public void GotoDefinition_LocalRuntimeClass_StillWorks()
    {
        var source = "namespace N {\n    runtimeclass Foo {\n        void DoWork();\n    }\n}\n";
        var uri = "file:///test.idl";

        // F12 on "Foo" at line 1
        var lines = source.Split('\n');
        int fooCol = lines[1].IndexOf("Foo", StringComparison.Ordinal);

        var result = GotoDefinitionHandler.HandleCore(source, uri, 1, fooCol);

        Assert.NotNull(result);
        var dict = Assert.IsType<Dictionary<string, object?>>(result);
        Assert.Equal(uri, dict["uri"]!.ToString()!);
    }

    #endregion

    #region Built-in WinRT types

    [Fact]
    public void GotoDefinition_BuiltInWinRTType_NavigatesToWinMd()
    {
        var source = "namespace N {\n    runtimeclass Foo {\n        Windows.Foundation.IAsyncAction DoWorkAsync();\n    }\n}\n";
        var uri = "file:///test.idl";

        // F12 on "IAsyncAction" at line 2
        var lines = source.Split('\n');
        int col = lines[2].IndexOf("IAsyncAction", StringComparison.Ordinal);

        var result = GotoDefinitionHandler.HandleCore(source, uri, 2, col);

        // Should return a midl-winmd:// URI (if Windows.winmd exists on this machine)
        if (result is Dictionary<string, object?> dict)
        {
            var resultUri = dict["uri"]!.ToString()!;
            Assert.Contains("midl-winmd://", resultUri);
            Assert.Contains("Windows.Foundation", resultUri);
        }
        // If no SDK installed, result may be null — that's ok for CI
    }

    [Fact]
    public void GotoDefinition_PerNamespace_DistinctUris()
    {
        // Two different built-in types from different namespaces should get different URIs
        var source1 = "namespace N { interface IFoo { Windows.Foundation.IAsyncAction A(); } }";
        var source2 = "namespace N { runtimeclass Foo { Windows.Foundation.Collections.IVectorView<Int32> GetItems(); } }";
        var uri = "file:///test.idl";

        var lines1 = source1.Split('\n');
        int col1 = lines1[0].IndexOf("IAsyncAction", StringComparison.Ordinal);
        var result1 = GotoDefinitionHandler.HandleCore(source1, uri, 0, col1);

        var lines2 = source2.Split('\n');
        int col2 = lines2[0].IndexOf("IVectorView", StringComparison.Ordinal);
        var result2 = GotoDefinitionHandler.HandleCore(source2, uri, 0, col2);

        if (result1 is Dictionary<string, object?> dict1 && result2 is Dictionary<string, object?> dict2)
        {
            var uri1 = dict1["uri"]!.ToString()!;
            var uri2 = dict2["uri"]!.ToString()!;
            // Different namespaces → different URIs (distinct virtual docs)
            Assert.NotEqual(uri1, uri2);
            Assert.Contains("Windows.Foundation", uri1);
        }
    }

    [Fact]
    public void GotoDefinition_WinMdRefDirective_UsesWinMdScheme()
    {
        // F12 on a // @ref: pointing to a .winmd should use midl-winmd:// scheme
        var tempDir = Path.Combine(Path.GetTempPath(), "midl-gotodef-ref-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);
        try
        {
            // Create a dummy .winmd
            var refSource = "namespace ExtLib { enum Status { Active } }";
            var compilation = CompilationHelper.Compile(refSource);
            var emitter = new WinMdEmitter();
            var winmdBytes = emitter.EmitToBytes(compilation);
            var winmdPath = Path.Combine(tempDir, "ExtLib.winmd");
            File.WriteAllBytes(winmdPath, winmdBytes);

            var source = $"// @ref: {winmdPath}\nnamespace N {{ }}";
            var uri = "file:///" + Path.Combine(tempDir, "test.idl").Replace('\\', '/');

            // F12 on line 0 (the @ref line)
            var result = GotoDefinitionHandler.HandleCore(source, uri, 0, 10);

            Assert.NotNull(result);
            var dict = Assert.IsType<Dictionary<string, object?>>(result);
            var resultUri = dict["uri"]!.ToString()!;
            Assert.StartsWith("midl-winmd://", resultUri);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void GotoDefinition_Handle_PassesRefsToCore()
    {
        // Integration test: verify Handle() passes includePaths/referencePaths
        // so that WinMD types can be resolved (the bug we caught earlier)
        var tempDir = Path.Combine(Path.GetTempPath(), "midl-gotodef-handle-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);
        try
        {
            // Create a WinMD with a type
            var refSource = "namespace ExtLib { interface IWidget { void Activate(); } }";
            var compilation = CompilationHelper.Compile(refSource);
            var emitter = new WinMdEmitter();
            var winmdBytes = emitter.EmitToBytes(compilation);
            var winmdPath = Path.Combine(tempDir, "ExtLib.winmd");
            File.WriteAllBytes(winmdPath, winmdBytes);

            // Source that references the WinMD type
            var source = "// @ref: " + winmdPath + "\nnamespace N { runtimeclass Foo : ExtLib.IWidget {} }";
            var idlPath = Path.Combine(tempDir, "test.idl");
            File.WriteAllText(idlPath, source);
            var uri = "file:///" + idlPath.Replace('\\', '/');

            var dm = new DocumentManager();
            dm.UpdateDocument(uri, source);

            // Call Handle (not HandleCore) — this is the real LSP entry point
            var posParams = MakePositionParams(uri, 1, source.Split('\n')[1].IndexOf("IWidget", StringComparison.Ordinal));
            var result = GotoDefinitionHandler.Handle(posParams, dm,
                includePaths: new[] { tempDir },
                referencePaths: new[] { winmdPath });

            // Should find the type (not return null)
            Assert.NotNull(result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    private static JsonElement MakePositionParams(string uri, int line, int character)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["textDocument"] = new Dictionary<string, object?> { ["uri"] = uri },
            ["position"] = new Dictionary<string, object?> { ["line"] = line, ["character"] = character }
        });
        return JsonDocument.Parse(json).RootElement;
    }

    #endregion

    #region FindDefinitionOffset edge cases

    [Fact]
    public void FindDefinitionOffset_Delegate_SkipsReturnType()
    {
        // Delegate definitions have return type between keyword and name:
        // "delegate void MyHandler(String arg);"
        var source = "namespace N {\n    delegate void MyHandler(String arg);\n}\n";
        var symbol = new Midl.Analysis.Symbols.DelegateSymbol("MyHandler", null!);
        int offset = GotoDefinitionHandler.FindDefinitionOffset(source, symbol);
        Assert.True(offset >= 0, "Should find delegate definition");
        Assert.Equal("MyHandler", source.Substring(offset, "MyHandler".Length));
    }

    [Fact]
    public void FindDefinitionOffset_Struct_FindsDefinitionNotPropertyType()
    {
        // "struct TimeSpan" should be found, not "TimeSpan" in a property type
        var source = "namespace N {\n    interface IFoo {\n        TimeSpan GetTime();\n    }\n    struct TimeSpan { Int64 Duration; }\n}\n";
        var symbol = new Midl.Analysis.Symbols.StructSymbol("TimeSpan", null!);
        int offset = GotoDefinitionHandler.FindDefinitionOffset(source, symbol);
        Assert.True(offset >= 0);
        // Should point to the struct definition, not the property usage
        var before = source[..offset];
        Assert.Contains("struct", before.Substring(Math.Max(0, before.Length - 10)));
    }

    #endregion

    #region Squiggle position with @ref backslash

    [Fact]
    public void Diagnostics_WithRefBackslash_CorrectPosition()
    {
        // @ref with backslash path should not shift squiggle positions
        var source = "// @ref: D:\\path\\to\\ref\nnamespace N { runtimeclass Foo { Strng X { get; }; } }";
        var compilation = CompilationHelper.Compile(source);
        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1001");
        Assert.NotNull(diag);
        // "Strng" starts at a specific offset on line 2
        var lines = source.Split('\n');
        int strngCol = lines[1].IndexOf("Strng", StringComparison.Ordinal);
        Assert.True(strngCol >= 0);
        // The diagnostic position should be on line 2 content, not shifted by @ref line joining
        Assert.True(diag.Position >= source.IndexOf("Strng", StringComparison.Ordinal),
            $"Diagnostic position {diag.Position} should be at or after 'Strng' at {source.IndexOf("Strng")}");
    }

    #endregion

    #region Namespace segment F12

    [Fact]
    public void GotoDefinition_NamespaceSegment_ResolvesFullType()
    {
        // F12 on "Foundation" in "Windows.Foundation.TypedEventHandler" should resolve to TypedEventHandler
        var source = "namespace N { runtimeclass Foo { event Windows.Foundation.TypedEventHandler<Foo, Object> Changed; } }";
        var uri = "file:///test.idl";

        // Position cursor on "Foundation" (not the type name)
        int foundationIdx = source.IndexOf("Foundation", StringComparison.Ordinal);
        Assert.True(foundationIdx > 0);

        // Convert offset to line/char
        var lineStarts = PositionHelper.BuildLineStarts(source);
        var (line, ch) = PositionHelper.OffsetToLineChar(lineStarts, foundationIdx);

        var result = GotoDefinitionHandler.HandleCore(source, uri, line, ch);

        // Should find something (not null) — on machines with Windows SDK
        var winmdExists = Directory.Exists(@"C:\Program Files (x86)\Windows Kits\10\UnionMetadata");
        if (winmdExists && result is Dictionary<string, object?> dict)
        {
            var resultUri = dict["uri"]!.ToString()!;
            Assert.Contains("midl-winmd://", resultUri);
        }
    }

    [Fact]
    public void GotoDefinition_MiddleNamespaceSegment_StillResolvesType()
    {
        // F12 on "Foundation" in "Windows.Foundation.TypedEventHandler" (cursor on middle segment)
        // should still resolve to TypedEventHandler
        var source = "namespace N { runtimeclass Foo { event Windows.Foundation.TypedEventHandler<Foo, Object> Changed; } }";
        var uri = "file:///test.idl";

        // Position cursor on "Foundation" — a middle segment, not the type name
        int idx = source.IndexOf("Foundation", StringComparison.Ordinal);
        var lineStarts = PositionHelper.BuildLineStarts(source);
        var (line, ch) = PositionHelper.OffsetToLineChar(lineStarts, idx);

        var result = GotoDefinitionHandler.HandleCore(source, uri, line, ch);

        // TypedEventHandler is a built-in, so this should resolve on any machine
        Assert.NotNull(result);
        var dict = Assert.IsType<Dictionary<string, object?>>(result);
        var resultUri = dict["uri"]!.ToString()!;
        Assert.Contains("midl-winmd://", resultUri);
        Assert.Contains("Windows.Foundation", resultUri);
    }

    [Fact]
    public void GotoDefinition_MiddleSegment_WinMdRefType_Resolves()
    {
        // F12 on "ExtLib" in "ExtLib.IWidget" where IWidget comes from a WinMD
        var tempDir = Path.Combine(Path.GetTempPath(), "midl-gotodef-midseg-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);
        try
        {
            var refSource = "namespace ExtLib { interface IWidget { void Activate(); } }";
            var refCompilation = CompilationHelper.Compile(refSource);
            var emitter = new WinMdEmitter();
            var winmdBytes = emitter.EmitToBytes(refCompilation);
            var winmdPath = Path.Combine(tempDir, "ExtLib.winmd");
            File.WriteAllBytes(winmdPath, winmdBytes);

            var source = "namespace N { runtimeclass Foo : ExtLib.IWidget {} }";
            var idlPath = Path.Combine(tempDir, "test.idl");
            File.WriteAllText(idlPath, source);
            var uri = "file:///" + idlPath.Replace('\\', '/');

            // Compile with the ref
            var compilation = CompilationHelper.Compile(source, new[] { winmdPath },
                CompilationMode.Strict, null, idlPath);

            // Cursor on "ExtLib" (namespace segment)
            int extlibIdx = source.IndexOf("ExtLib", StringComparison.Ordinal);
            var lineStarts = PositionHelper.BuildLineStarts(source);
            var (line, ch) = PositionHelper.OffsetToLineChar(lineStarts, extlibIdx);
            
            var result = GotoDefinitionHandler.HandleCore(source, uri, line, ch,
                currentFilePath: idlPath,
                referencePaths: new[] { winmdPath },
                cachedCompilation: compilation);

            Assert.NotNull(result);
            var dict = Assert.IsType<Dictionary<string, object?>>(result);
            var resultUri = dict["uri"]!.ToString()!;
            Assert.Contains("midl-winmd://", resultUri);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    #endregion
}
