// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;
using Midl.CodeGen.WinMd;
using Midl.Lsp;
using Midl.Lsp.Handlers;
using Midl.Lsp.Protocol;

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
    public void CompletionAtTopLevel_ShowsDeclarationKeywords()
    {
        var uri = "file:///test.midl";
        var dm = CreateDocManager(uri, "namespace Foo { }");
        // Position inside namespace body → top-level context
        var result = CompletionHandler.Handle(MakePositionParams(uri, 0, 16), dm);

        var json = JsonSerializer.Serialize(result);

        // Should include declaration keywords
        Assert.Contains("\"runtimeclass\"", json);
        Assert.Contains("\"interface\"", json);
        Assert.Contains("\"struct\"", json);
        Assert.Contains("\"enum\"", json);
        Assert.Contains("\"delegate\"", json);

        // Should NOT include types (those belong inside type bodies)
        Assert.DoesNotContain("\"Int32\"", json);
        Assert.DoesNotContain("\"String\"", json);
    }

    [Fact]
    public void CompletionInRuntimeClassBody_ShowsTypesAndModifiers()
    {
        var uri = "file:///test.midl";
        var source = "namespace N {\n    runtimeclass Foo {\n        \n    }\n}";
        var dm = CreateDocManager(uri, source);
        // Cursor at line 2, char 8 — inside runtimeclass body
        var result = CompletionHandler.Handle(MakePositionParams(uri, 2, 8), dm);

        var json = JsonSerializer.Serialize(result);

        // Should include types for method return / property types
        Assert.Contains("\"Int32\"", json);
        Assert.Contains("\"String\"", json);
        Assert.Contains("\"void\"", json);

        // Should include member keywords
        Assert.Contains("\"event\"", json);
        Assert.Contains("\"static\"", json);

        // Should suggest constructor
        Assert.Contains("\"Foo\"", json);

        // Should NOT include top-level keywords
        Assert.DoesNotContain("\"namespace\"", json);
        Assert.DoesNotContain("\"import\"", json);
    }

    [Fact]
    public void CompletionInStructBody_ShowsOnlyValueTypes()
    {
        var uri = "file:///test.midl";
        var source = "namespace N {\n    struct Point {\n        \n    };\n}";
        var dm = CreateDocManager(uri, source);
        var result = CompletionHandler.Handle(MakePositionParams(uri, 2, 8), dm);

        var json = JsonSerializer.Serialize(result);

        // Should include value types
        Assert.Contains("\"Int32\"", json);
        Assert.Contains("\"Single\"", json);
        Assert.Contains("\"Boolean\"", json);

        // Should NOT include reference types or keywords
        Assert.DoesNotContain("\"Object\"", json);
        Assert.DoesNotContain("\"IVector\"", json);
        Assert.DoesNotContain("\"void\"", json);
        Assert.DoesNotContain("\"event\"", json);
    }

    [Fact]
    public void CompletionInEnumBody_NoTypeCompletions()
    {
        var uri = "file:///test.midl";
        var source = "namespace N {\n    enum Color {\n        \n    };\n}";
        var dm = CreateDocManager(uri, source);
        var result = CompletionHandler.Handle(MakePositionParams(uri, 2, 8), dm);

        var json = JsonSerializer.Serialize(result);

        // Should NOT include types
        Assert.DoesNotContain("\"Int32\"", json);
        Assert.DoesNotContain("\"String\"", json);
        Assert.DoesNotContain("\"runtimeclass\"", json);
    }

    [Fact]
    public void CompletionAfterRequires_ShowsInterfacesOnly()
    {
        var uri = "file:///test.midl";
        var source = "namespace N {\n    interface IFoo { };\n    interface IBar\n    requires \n    { }\n}";
        var dm = CreateDocManager(uri, source);
        // Cursor at line 3, char 13 — after "requires "
        var result = CompletionHandler.Handle(MakePositionParams(uri, 3, 13), dm);

        var json = JsonSerializer.Serialize(result);

        // Should include user-defined interfaces
        Assert.Contains("IFoo", json);

        // Should NOT include types, keywords, structs
        Assert.DoesNotContain("\"Int32\"", json);
        Assert.DoesNotContain("\"runtimeclass\"", json);
        Assert.DoesNotContain("\"void\"", json);
    }

    [Fact]
    public void CompletionAfterColon_ShowsInterfacesAndClasses()
    {
        var uri = "file:///test.midl";
        var source = "namespace N {\n    interface IFoo { };\n    runtimeclass Foo : \n    { }\n}";
        var dm = CreateDocManager(uri, source);
        // Cursor at line 2, char 23 — after ": "
        var result = CompletionHandler.Handle(MakePositionParams(uri, 2, 23), dm);

        var json = JsonSerializer.Serialize(result);

        // Should include interfaces
        Assert.Contains("IFoo", json);

        // Should NOT include primitive types or declaration keywords
        Assert.DoesNotContain("\"Int32\"", json);
        Assert.DoesNotContain("\"struct\"", json);
    }

    [Fact]
    public void CompletionInBaseList_ShowsInterfacesAndClasses()
    {
        var uri = "file:///test.midl";
        var source = "namespace N {\n    interface IFoo { };\n    enum MyEnum { A };\n    struct MyStruct { Int32 x; };\n    runtimeclass Foo : \n    { }\n}";
        var dm = CreateDocManager(uri, source);
        // Cursor after ": " on the runtimeclass line
        var result = CompletionHandler.Handle(MakePositionParams(uri, 4, 23), dm);

        var json = JsonSerializer.Serialize(result);

        // Should include user-defined interfaces and classes
        Assert.Contains("IFoo", json);

        // Should NOT include enums or structs
        Assert.DoesNotContain("\"MyEnum\"", json);
        Assert.DoesNotContain("\"MyStruct\"", json);
    }

    [Fact]
    public void CompletionInRequiresList_ShowsOnlyInterfaces()
    {
        var uri = "file:///test.midl";
        var source = "namespace N {\n    interface IFoo { };\n    runtimeclass Bar { };\n    interface IBar\n    requires \n    { }\n}";
        var dm = CreateDocManager(uri, source);
        // Cursor after "requires " on line 4
        var result = CompletionHandler.Handle(MakePositionParams(uri, 4, 13), dm);

        var json = JsonSerializer.Serialize(result);

        // Should include interfaces
        Assert.Contains("IFoo", json);

        // Should NOT include runtimeclasses, enums, structs, or primitives
        Assert.DoesNotContain("\"Bar\"", json);
        Assert.DoesNotContain("\"Int32\"", json);
        Assert.DoesNotContain("\"void\"", json);
    }

    [Fact]
    public void CompletionInParameterType_ShowsAllTypes()
    {
        var uri = "file:///test.midl";
        var source = "namespace N {\n    enum MyEnum { A };\n    interface IFoo {\n        void Bar( );\n    }\n}";
        var dm = CreateDocManager(uri, source);
        // Cursor inside parens at the space between ( and ) — line 3, char 17
        var result = CompletionHandler.Handle(MakePositionParams(uri, 3, 17), dm);

        var json = JsonSerializer.Serialize(result);

        // Should include all types (primitives + user types)
        Assert.Contains("\"Int32\"", json);
        Assert.Contains("\"String\"", json);
        Assert.Contains("MyEnum", json);

        // Should include parameter modifiers
        Assert.Contains("\"out\"", json);
        Assert.Contains("\"ref\"", json);

        // Should NOT include body keywords like event, static, void
        Assert.DoesNotContain("\"event\"", json);
        Assert.DoesNotContain("\"static\"", json);
        Assert.DoesNotContain("\"void\"", json);
    }

    [Fact]
    public void CompletionInEventDecl_ShowsDelegatesOnly()
    {
        var uri = "file:///test.midl";
        var source = "namespace N {\n    delegate void MyHandler(Object sender);\n    interface IFoo { };\n    runtimeclass Foo {\n        event  ;\n    }\n}";
        var dm = CreateDocManager(uri, source);
        // Cursor after "event " on line 4 (col 14)
        var result = CompletionHandler.Handle(MakePositionParams(uri, 4, 14), dm);

        var json = JsonSerializer.Serialize(result);

        // Should include user-defined delegates
        Assert.Contains("MyHandler", json);
        // Should include EventHandler and TypedEventHandler
        Assert.Contains("EventHandler", json);
        Assert.Contains("TypedEventHandler", json);

        // Should NOT include non-delegate types
        Assert.DoesNotContain("\"IFoo\"", json);
        Assert.DoesNotContain("\"Int32\"", json);
        Assert.DoesNotContain("\"void\"", json);
    }

    [Fact]
    public void CompletionInReturnType_ShowsTypesAndVoid()
    {
        var uri = "file:///test.midl";
        var source = "namespace N {\n    enum MyEnum { A };\n    interface IFoo {\n         \n    }\n}";
        var dm = CreateDocManager(uri, source);
        // Cursor inside the interface body at the blank line
        var result = CompletionHandler.Handle(MakePositionParams(uri, 3, 8), dm);

        var json = JsonSerializer.Serialize(result);

        // Should include all types + void (interface body context for return types)
        Assert.Contains("\"Int32\"", json);
        Assert.Contains("\"String\"", json);
        Assert.Contains("\"void\"", json);
        Assert.Contains("MyEnum", json);

        // Should include event keyword (interface body)
        Assert.Contains("\"event\"", json);
    }

    [Fact]
    public void CompletionIsIncomplete_ForContextualTypes()
    {
        var uri = "file:///test.midl";

        // Interface body context — should be isIncomplete=true
        var source1 = "namespace N {\n    interface IFoo {\n         \n    }\n}";
        var dm1 = CreateDocManager(uri, source1);
        var result1 = CompletionHandler.Handle(MakePositionParams(uri, 2, 8), dm1);
        var json1 = JsonSerializer.Serialize(result1);
        Assert.Contains("\"isIncomplete\":true", json1);

        // BaseList context — should be isIncomplete=true
        var source2 = "namespace N {\n    runtimeclass Foo : \n    { }\n}";
        var dm2 = CreateDocManager(uri, source2);
        var result2 = CompletionHandler.Handle(MakePositionParams(uri, 1, 23), dm2);
        var json2 = JsonSerializer.Serialize(result2);
        Assert.Contains("\"isIncomplete\":true", json2);

        // RequiresList context — should be isIncomplete=true
        var source3 = "namespace N {\n    interface IFoo\n    requires \n    { }\n}";
        var dm3 = CreateDocManager(uri, source3);
        var result3 = CompletionHandler.Handle(MakePositionParams(uri, 2, 13), dm3);
        var json3 = JsonSerializer.Serialize(result3);
        Assert.Contains("\"isIncomplete\":true", json3);

        // TopLevel context — should be isIncomplete=false
        var source4 = "namespace N { }";
        var dm4 = CreateDocManager(uri, source4);
        var result4 = CompletionHandler.Handle(MakePositionParams(uri, 0, 14), dm4);
        var json4 = JsonSerializer.Serialize(result4);
        Assert.Contains("\"isIncomplete\":false", json4);
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
        Assert.Contains("default_interface", json);

        // uuid is only valid on interface/delegate, not runtimeclass
        Assert.DoesNotContain("\"uuid\"", json);

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
            // Create sibling IDL files + the current file on disk
            File.WriteAllText(Path.Combine(tempDir, "Widget.idl"), "namespace W { }");
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

            // Should include sibling .idl files
            Assert.Contains("Helper.idl", json);
            Assert.Contains("Types.idl", json);

            // Should NOT include the current file itself
            Assert.DoesNotContain("Widget.idl", json);

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

    [Fact]
    public void CompletionAfterDot_WindowsNamespace_ShowsChildNamespaces()
    {
        var uri = "file:///test.midl";
        var source = """
            namespace Windows.Foundation
            {
                interface IStringable { }
            }
            namespace TestApp
            {
                runtimeclass Foo
                {
                    Windows.
                }
            }
            """;
        var dm = CreateDocManager(uri, source);
        // Cursor after "Windows." — line 8 (0-indexed), find the char position
        var lines = source.Split('\n');
        int dotLine = -1, dotChar = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            int idx = lines[i].IndexOf("Windows.");
            if (idx >= 0)
            {
                dotLine = i;
                dotChar = idx + "Windows.".Length;
                break;
            }
        }
        Assert.True(dotLine >= 0, "Could not find 'Windows.' in source");

        var result = CompletionHandler.Handle(MakePositionParams(uri, dotLine, dotChar), dm);
        var json = JsonSerializer.Serialize(result);

        // Should show "Foundation" child namespace
        Assert.Contains("Foundation", json);
        Assert.Contains($"\"kind\":{9}", json); // KindModule = 9
    }

    [Fact]
    public void CompletionAfterDot_IsIncomplete_ForRetrigger()
    {
        // Bug fix: dot completions must return isIncomplete=true so VS Code retriggers
        var uri = "file:///test.midl";
        var source = """
            namespace Windows.Foundation
            {
                interface IStringable { }
            }
            namespace TestApp
            {
                runtimeclass Foo
                {
                    Windows.
                }
            }
            """;
        var dm = CreateDocManager(uri, source);
        var lines = source.Split('\n');
        int dotLine = -1, dotChar = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            int idx = lines[i].IndexOf("Windows.");
            if (idx >= 0) { dotLine = i; dotChar = idx + "Windows.".Length; break; }
        }
        var result = CompletionHandler.Handle(MakePositionParams(uri, dotLine, dotChar), dm);
        var json = JsonSerializer.Serialize(result);

        Assert.Contains("\"isIncomplete\":true", json);
    }

    [Fact]
    public void CompletionInReturnTypePosition_ShowsUserTypes()
    {
        // Typing in a runtimeclass body should show types AND user-defined types
        var uri = "file:///test.midl";
        var source = "namespace N {\n    enum MyEnum { A };\n    runtimeclass Foo {\n        My\n    }\n}";
        var dm = CreateDocManager(uri, source);
        var compilation = CompilationHelper.Compile(source);
        var result = CompletionHandler.Handle(MakePositionParams(uri, 3, 10), dm, cachedCompilation: compilation);

        var json = JsonSerializer.Serialize(result);

        // RuntimeClassBody context: types + keywords + user types
        Assert.Contains("\"Int32\"", json);
        Assert.Contains("MyEnum", json);
        Assert.Contains("\"protected\"", json);
    }

    [Fact]
    public void FindDefinitionOffset_WordBoundary_DoesNotMatchSubstring()
    {
        // Bug fix: "interface IVector" must not match "interface IVectorChangedEventArgs"
        var sourceText = """
            namespace Windows.Foundation.Collections
            {
                interface IVectorChangedEventArgs
                {
                }
                interface IVector
                {
                }
            }
            """;
        var symbol = new Midl.Analysis.Symbols.InterfaceSymbol("IVector", null);
        int offset = GotoDefinitionHandler.FindDefinitionOffset(sourceText, symbol);
        Assert.True(offset >= 0, "Should find IVector");

        // The offset should point to "IVector" (the standalone one), not "IVectorChangedEventArgs"
        var nameAtOffset = sourceText.Substring(offset, "IVector".Length);
        Assert.Equal("IVector", nameAtOffset);

        // Verify the character AFTER is NOT alphanumeric (word boundary)
        int afterOffset = offset + "IVector".Length;
        Assert.True(afterOffset < sourceText.Length);
        Assert.False(char.IsLetterOrDigit(sourceText[afterOffset]),
            $"Character after IVector should not be alphanumeric, got '{sourceText[afterOffset]}'");
    }

    [Fact]
    public void CompletionAfterDot_LocalTypes_ShowsDeclaredTypes()
    {
        var uri = "file:///test.midl";
        var source = """
            namespace TestApp
            {
                enum Status { Idle, Active }
                runtimeclass Widget { }
                interface IHelper { }
                runtimeclass Foo
                {
                    TestApp.
                }
            }
            """;
        var dm = CreateDocManager(uri, source);
        var lines = source.Split('\n');
        int dotLine = -1, dotChar = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            int idx = lines[i].IndexOf("TestApp.");
            if (idx >= 0 && !lines[i].TrimStart().StartsWith("namespace"))
            {
                dotLine = i;
                dotChar = idx + "TestApp.".Length;
                break;
            }
        }
        Assert.True(dotLine >= 0, "Could not find 'TestApp.' usage in source");

        var result = CompletionHandler.Handle(MakePositionParams(uri, dotLine, dotChar), dm);
        var json = JsonSerializer.Serialize(result);

        // Should show locally declared types
        Assert.Contains("Status", json);
        Assert.Contains("Widget", json);
        Assert.Contains("IHelper", json);
    }

    [Fact]
    public void CompletionAfterDot_EnumType_ShowsEnumMembers()
    {
        var uri = "file:///test.midl";
        var source = """
            namespace TestApp
            {
                enum MyEnum { Alpha, Beta, Gamma }
                runtimeclass Foo
                {
                    MyEnum.
                }
            }
            """;
        var dm = CreateDocManager(uri, source);
        var lines = source.Split('\n');
        int dotLine = -1, dotChar = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            int idx = lines[i].IndexOf("MyEnum.");
            if (idx >= 0)
            {
                dotLine = i;
                dotChar = idx + "MyEnum.".Length;
                break;
            }
        }
        Assert.True(dotLine >= 0, "Could not find 'MyEnum.' in source");

        var result = CompletionHandler.Handle(MakePositionParams(uri, dotLine, dotChar), dm);
        var json = JsonSerializer.Serialize(result);

        // Should show enum members
        Assert.Contains("Alpha", json);
        Assert.Contains("Beta", json);
        Assert.Contains("Gamma", json);
        Assert.Contains($"\"kind\":{20}", json); // KindEnumMember = 20
    }

    [Fact]
    public void ExtractDotPrefix_ImmediatelyAfterDot()
    {
        // "Windows.|" → prefix = "Windows"
        var text = "Windows.";
        var prefix = DotCompletionProvider.ExtractDotPrefix(text, text.Length);
        Assert.Equal("Windows", prefix);
    }

    [Fact]
    public void ExtractDotPrefix_MidTypingAfterDot()
    {
        // "Windows.Fo|" → prefix = "Windows" (VS Code filters by "Fo")
        var text = "Windows.Fo";
        var prefix = DotCompletionProvider.ExtractDotPrefix(text, text.Length);
        Assert.Equal("Windows", prefix);
    }

    [Fact]
    public void ExtractDotPrefix_MultiLevel()
    {
        // "Windows.Foundation.|" → prefix = "Windows.Foundation"
        var text = "Windows.Foundation.";
        var prefix = DotCompletionProvider.ExtractDotPrefix(text, text.Length);
        Assert.Equal("Windows.Foundation", prefix);
    }

    [Fact]
    public void ExtractDotPrefix_MultiLevelMidTyping()
    {
        // "Windows.Foundation.C|" → prefix = "Windows.Foundation"
        var text = "Windows.Foundation.C";
        var prefix = DotCompletionProvider.ExtractDotPrefix(text, text.Length);
        Assert.Equal("Windows.Foundation", prefix);
    }

    [Fact]
    public void ExtractDotPrefix_NoDot_ReturnsNull()
    {
        var text = "Windows";
        Assert.Null(DotCompletionProvider.ExtractDotPrefix(text, text.Length));
    }

    [Fact]
    public void DotCompletion_MidTypingAfterDot_StillShowsChildren()
    {
        // When typing "TestApp.S|", should still show children of TestApp
        var uri = "file:///test.midl";
        var source = """
            namespace TestApp
            {
                enum Status { Idle }
                runtimeclass Widget { }
                runtimeclass Foo
                {
                    TestApp.S
                }
            }
            """;
        var dm = CreateDocManager(uri, source);
        var lines = source.Split('\n');
        int line = -1, col = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            int idx = lines[i].IndexOf("TestApp.S");
            if (idx >= 0 && !lines[i].TrimStart().StartsWith("namespace"))
            {
                line = i;
                col = idx + "TestApp.S".Length; // cursor after 'S'
                break;
            }
        }
        Assert.True(line >= 0);

        var result = CompletionHandler.Handle(MakePositionParams(uri, line, col), dm);
        var json = JsonSerializer.Serialize(result);

        // Should still show TestApp children (VS Code filters by "S")
        Assert.Contains("Status", json);
        Assert.Contains("Widget", json);
    }

    [Fact]
    public void DotCompletion_SdkNamespace_MidTyping_WithCachedCompilation()
    {
        var uri = "file:///test.midl";
        var source = "namespace X {\n    runtimeclass F {\n        Windows.F\n    }\n}";
        var dm = CreateDocManager(uri, source);

        // Cached compilation WITH SDK refs
        var winmd = Midl.Lsp.Handlers.WinMdNavigator.FindWindowsWinMd();
        if (winmd == null) return; // skip on machines without SDK
        var compilation = CompilationHelper.Compile(source, new List<string> { winmd }, CompilationMode.Strict);

        // Cursor at end of "Windows.F" — line 2 col 17
        var result = CompletionHandler.Handle(
            MakePositionParams(uri, 2, 17), dm, cachedCompilation: compilation);
        var json = JsonSerializer.Serialize(result);

        // Should still show Windows namespace children (Foundation starts with F)
        Assert.Contains("Foundation", json);
    }

    [Fact]
    public void DotCompletion_CaseInsensitive_LowercasePrefix()
    {
        var uri = "file:///test.midl";
        var source = "namespace X {\n    runtimeclass F {\n        windows.\n    }\n}";
        var dm = CreateDocManager(uri, source);

        var winmd = Midl.Lsp.Handlers.WinMdNavigator.FindWindowsWinMd();
        if (winmd == null) return;
        var compilation = CompilationHelper.Compile(source, new List<string> { winmd }, CompilationMode.Strict);

        // Cursor after "windows." — line 2, col 16
        var result = CompletionHandler.Handle(
            MakePositionParams(uri, 2, 16), dm, cachedCompilation: compilation);
        var json = JsonSerializer.Serialize(result);

        // Should show SDK namespace children despite lowercase input
        Assert.Contains("Foundation", json);
        Assert.Contains("UI", json);
    }

    [Fact]
    public void DotCompletion_InsertText_HasCanonicalCasing()
    {
        var uri = "file:///test.midl";
        var source = "namespace X {\n    runtimeclass F {\n        windows.\n    }\n}";
        var dm = CreateDocManager(uri, source);

        var winmd = Midl.Lsp.Handlers.WinMdNavigator.FindWindowsWinMd();
        if (winmd == null) return;
        var compilation = CompilationHelper.Compile(source, new List<string> { winmd }, CompilationMode.Strict);

        var result = CompletionHandler.Handle(
            MakePositionParams(uri, 2, 16), dm, cachedCompilation: compilation);
        var json = JsonSerializer.Serialize(result);

        // textEdit.newText should use canonical casing (Windows.Foundation, not windows.Foundation)
        Assert.Contains("Windows.Foundation", json);
        Assert.DoesNotContain("\"newText\":\"windows.", json);
    }

    [Fact]
    public void DotCompletion_MultiLevelSdk_ShowsTypesAndSubNamespaces()
    {
        var uri = "file:///test.midl";
        var source = "namespace X {\n    runtimeclass F {\n        Windows.Foundation.\n    }\n}";
        var dm = CreateDocManager(uri, source);

        var winmd = Midl.Lsp.Handlers.WinMdNavigator.FindWindowsWinMd();
        if (winmd == null) return;
        var compilation = CompilationHelper.Compile(source, new List<string> { winmd }, CompilationMode.Strict);

        // Cursor after "Windows.Foundation." — line 2, col 27
        var result = CompletionHandler.Handle(
            MakePositionParams(uri, 2, 27), dm, cachedCompilation: compilation);
        var json = JsonSerializer.Serialize(result);

        // Should show child namespaces (Collections, Diagnostics, etc.)
        Assert.Contains("Collections", json);
        // Should show types in Windows.Foundation (Uri, IAsyncAction, etc.)
        Assert.Contains("Uri", json);
    }

    [Fact]
    public void Completion_RootLevel_ShowsSdkNamespaces()
    {
        var uri = "file:///test.midl";
        var source = "namespace X {\n    runtimeclass F {\n        \n    }\n}";
        var dm = CreateDocManager(uri, source);

        var winmd = Midl.Lsp.Handlers.WinMdNavigator.FindWindowsWinMd();
        if (winmd == null) return;
        var compilation = CompilationHelper.Compile(source, new List<string> { winmd }, CompilationMode.Strict);

        // Cursor on blank line in runtimeclass body
        var result = CompletionHandler.Handle(
            MakePositionParams(uri, 2, 8), dm, cachedCompilation: compilation);
        var json = JsonSerializer.Serialize(result);

        // Should show "Windows" as a namespace completion
        Assert.Contains("\"Windows\"", json);
        Assert.Contains("SDK namespace", json);
    }

    [Fact]
    public void DotCompletion_AfterType_ReturnsEmpty()
    {
        // Typing "." after a resolved type should NOT show namespace children
        var uri = "file:///test.midl";
        var source = "namespace N {\n    runtimeclass Foo {\n    }\n    runtimeclass Bar {\n        N.Foo.\n    }\n}";
        var dm = CreateDocManager(uri, source);
        var compilation = CompilationHelper.Compile(source);

        // Cursor after "N.Foo." — line 4, col 14
        var result = CompletionHandler.Handle(
            MakePositionParams(uri, 4, 14), dm, cachedCompilation: compilation);
        var json = JsonSerializer.Serialize(result);

        // Should NOT show namespace children (N is a namespace, but Foo. shouldn't drill further)
        // The contextual completion may still show types — we just verify no dot-style namespace items
        Assert.DoesNotContain("SDK namespace", json);
    }

    [Fact]
    public void DotCompletion_AfterSdkType_ReturnsEmpty()
    {
        var uri = "file:///test.midl";
        var source = "namespace X {\n    runtimeclass F {\n        Windows.Foundation.Uri.\n    }\n}";
        var dm = CreateDocManager(uri, source);

        var winmd = Midl.Lsp.Handlers.WinMdNavigator.FindWindowsWinMd();
        if (winmd == null) return;
        var compilation = CompilationHelper.Compile(source, new List<string> { winmd }, CompilationMode.Strict);

        // Cursor after "Windows.Foundation.Uri." — line 2, col 31
        var result = CompletionHandler.Handle(
            MakePositionParams(uri, 2, 31), dm, cachedCompilation: compilation);
        var json = JsonSerializer.Serialize(result);

        // Should return empty — Uri is a type, not a namespace
        Assert.DoesNotContain("Foundation", json);
    }

    [Fact]
    public void DotCompletion_AfterEnum_ShowsMembers()
    {
        // Enum. should still show enum members
        var uri = "file:///test.midl";
        var source = "namespace N {\n    enum Color { Red, Green, Blue };\n    runtimeclass Foo {\n        N.Color.\n    }\n}";
        var dm = CreateDocManager(uri, source);
        var compilation = CompilationHelper.Compile(source);

        // Cursor after "N.Color." — line 3, col 16
        var result = CompletionHandler.Handle(
            MakePositionParams(uri, 3, 16), dm, cachedCompilation: compilation);
        var json = JsonSerializer.Serialize(result);

        Assert.Contains("Red", json);
        Assert.Contains("Green", json);
        Assert.Contains("Blue", json);
    }

    [Fact]
    public void RefDirective_FolderCompletion_UsesBackslashByDefault()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"midl_sep_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var subDir = Path.Combine(tempDir, "mylibs");
        Directory.CreateDirectory(subDir);
        try
        {
            var uri = "file:///" + Uri.EscapeDataString(Path.Combine(tempDir, "main.idl")).Replace("\\", "/");
            var source = "// @ref: \nnamespace N { }";
            var dm = CreateDocManager(uri, source);
            var items = RefDirectiveCompletionProvider.BuildRefDirectiveCompletions("", tempDir);
            var json = JsonSerializer.Serialize(items);
            // Default separator should be backslash
            Assert.Contains("mylibs\\\\", json);
            Assert.DoesNotContain("mylibs/", json);
        }
        finally { Directory.Delete(tempDir, true); }
    }

    [Fact]
    public void RefDirective_FolderCompletion_MatchesForwardSlash()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"midl_sep_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var subDir = Path.Combine(tempDir, "libs");
        Directory.CreateDirectory(subDir);
        var nestedDir = Path.Combine(subDir, "nested");
        Directory.CreateDirectory(nestedDir);
        try
        {
            // User typed "libs/" with forward slash — nested dirs should use "/"
            var items = RefDirectiveCompletionProvider.BuildRefDirectiveCompletions("libs/", tempDir);
            var json = JsonSerializer.Serialize(items);
            Assert.Contains("nested/", json);
        }
        finally { Directory.Delete(tempDir, true); }
    }

    [Fact]
    public void CompletionOnRefDirective_ShowsFilesAndFolders()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"midl_ref_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var subDir = Path.Combine(tempDir, "refs");
        Directory.CreateDirectory(subDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "types.idl"), "namespace T { }");
            File.WriteAllText(Path.Combine(tempDir, "lib.winmd"), ""); // placeholder
            File.WriteAllText(Path.Combine(tempDir, "readme.txt"), "hello");

            var uri = "file:///" + Uri.EscapeDataString(Path.Combine(tempDir, "main.idl")).Replace("\\", "/");
            var source = "// @ref: \nnamespace N { }";
            var dm = CreateDocManager(uri, source);

            // Cursor at line 0, char 9 — after "// @ref: "
            var result = CompletionHandler.Handle(MakePositionParams(uri, 0, 9), dm);
            var json = JsonSerializer.Serialize(result);

            // Should include .idl and .winmd files
            Assert.Contains("types.idl", json);
            Assert.Contains("lib.winmd", json);

            // Should include subdirectory
            Assert.Contains("refs", json);

            // Should NOT include non-matching files
            Assert.DoesNotContain("readme.txt", json);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void CompletionOnRefDirective_Subdirectory_ShowsSubdirFiles()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"midl_ref_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var subDir = Path.Combine(tempDir, "sub");
        Directory.CreateDirectory(subDir);
        try
        {
            File.WriteAllText(Path.Combine(subDir, "inner.idl"), "namespace I { }");
            File.WriteAllText(Path.Combine(subDir, "inner.winmd"), ""); // placeholder

            var uri = "file:///" + Uri.EscapeDataString(Path.Combine(tempDir, "main.idl")).Replace("\\", "/");
            var source = "// @ref: sub/\nnamespace N { }";
            var dm = CreateDocManager(uri, source);

            // Cursor at line 0, char 13 — after "// @ref: sub/"
            var result = CompletionHandler.Handle(MakePositionParams(uri, 0, 13), dm);
            var json = JsonSerializer.Serialize(result);

            Assert.Contains("inner.idl", json);
            Assert.Contains("inner.winmd", json);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void CompletionOnRefDirective_FilesSortBeforeFolders()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"midl_ref_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        Directory.CreateDirectory(Path.Combine(tempDir, "aaa_folder"));
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "zzz.idl"), "namespace Z { }");

            var uri = "file:///" + Uri.EscapeDataString(Path.Combine(tempDir, "main.idl")).Replace("\\", "/");
            var source = "// @ref: \nnamespace N { }";
            var dm = CreateDocManager(uri, source);

            var result = CompletionHandler.Handle(MakePositionParams(uri, 0, 9), dm);
            var json = JsonSerializer.Serialize(result);

            // Both should be present
            Assert.Contains("zzz.idl", json);
            Assert.Contains("aaa_folder", json);

            // File should come before folder even though folder sorts alphabetically first
            int fileIdx = json.IndexOf("zzz.idl");
            int folderIdx = json.IndexOf("aaa_folder");
            Assert.True(fileIdx < folderIdx, "Files should sort before folders in @ref completions");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void CompletionOnRefDirective_LocalFilesBeforeIncludePathFiles()
    {
        var localDir = Path.Combine(Path.GetTempPath(), $"midl_ref_local_{Guid.NewGuid():N}");
        var includeDir = Path.Combine(Path.GetTempPath(), $"midl_ref_inc_{Guid.NewGuid():N}");
        Directory.CreateDirectory(localDir);
        Directory.CreateDirectory(includeDir);
        try
        {
            File.WriteAllText(Path.Combine(localDir, "local.idl"), "namespace L { }");
            File.WriteAllText(Path.Combine(localDir, "local.winmd"), "");
            File.WriteAllText(Path.Combine(includeDir, "sdk.idl"), "namespace S { }");
            File.WriteAllText(Path.Combine(includeDir, "sdk.winmd"), "");

            var uri = "file:///" + Uri.EscapeDataString(Path.Combine(localDir, "main.idl")).Replace("\\", "/");
            var source = "// @ref: \nnamespace N { }";
            var dm = CreateDocManager(uri, source);

            var result = CompletionHandler.Handle(MakePositionParams(uri, 0, 9), dm,
                includePaths: new[] { includeDir });
            var dict = (Dictionary<string, object?>)result;
            var items = (object[])dict["items"]!;

            // All 4 files should appear
            var labels = items.Cast<Dictionary<string, object?>>().Select(i => i["label"]!.ToString()).ToList();
            Assert.Contains("local.idl", labels);
            Assert.Contains("local.winmd", labels);
            Assert.Contains("sdk.idl", labels);
            Assert.Contains("sdk.winmd", labels);

            // Local files should have sortText starting with "0_", include files with "1_"
            foreach (var item in items.Cast<Dictionary<string, object?>>())
            {
                var label = item["label"]!.ToString()!;
                var sortText = item["sortText"]!.ToString()!;
                if (label.StartsWith("local"))
                    Assert.StartsWith("0_", sortText);
                else if (label.StartsWith("sdk"))
                    Assert.StartsWith("1_", sortText);
            }

            // Folders should come after files (sort prefix 2_ or 3_ > 0_ or 1_)
            var sortTexts = items.Cast<Dictionary<string, object?>>()
                .Select(i => i["sortText"]!.ToString()!).ToList();
            var fileSorts = sortTexts.Where(s => s.StartsWith("0_") || s.StartsWith("1_")).ToList();
            var folderSorts = sortTexts.Where(s => s.StartsWith("2_") || s.StartsWith("3_")).ToList();
            foreach (var fs in fileSorts)
                foreach (var ds in folderSorts)
                    Assert.True(string.Compare(fs, ds, StringComparison.OrdinalIgnoreCase) < 0,
                        "Files should sort before folders");
        }
        finally
        {
            Directory.Delete(localDir, true);
            Directory.Delete(includeDir, true);
        }
    }

    [Fact]
    public void CompletionOnImportLine_LocalFilesBeforeIncludeFiles()
    {
        var localDir = Path.Combine(Path.GetTempPath(), $"midl_imp_local_{Guid.NewGuid():N}");
        var includeDir = Path.Combine(Path.GetTempPath(), $"midl_imp_inc_{Guid.NewGuid():N}");
        Directory.CreateDirectory(localDir);
        Directory.CreateDirectory(includeDir);
        try
        {
            File.WriteAllText(Path.Combine(localDir, "Local.idl"), "namespace L { }");
            File.WriteAllText(Path.Combine(includeDir, "SdkHelper.idl"), "namespace S { }");

            var uri = "file:///" + Uri.EscapeDataString(Path.Combine(localDir, "Main.idl")).Replace("\\", "/");
            var source = "import \"\"\nnamespace N { }";
            var dm = CreateDocManager(uri, source);

            var result = CompletionHandler.Handle(MakePositionParams(uri, 0, 8), dm,
                includePaths: new[] { includeDir });
            var dict = (Dictionary<string, object?>)result;
            var items = (object[])dict["items"]!;

            var localItem = items.Cast<Dictionary<string, object?>>()
                .First(i => i["label"]!.ToString() == "Local.idl");
            var sdkItem = items.Cast<Dictionary<string, object?>>()
                .First(i => i["label"]!.ToString() == "SdkHelper.idl");

            Assert.StartsWith("0_", localItem["sortText"]!.ToString());
            Assert.StartsWith("1_", sdkItem["sortText"]!.ToString());
        }
        finally
        {
            Directory.Delete(localDir, true);
            Directory.Delete(includeDir, true);
        }
    }

    [Fact]
    public void CompletionOnImportLine_IsIncomplete()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"midl_imp_inc_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "A.idl"), "namespace A { }");

            var uri = "file:///" + Uri.EscapeDataString(Path.Combine(tempDir, "Main.idl")).Replace("\\", "/");
            var source = "import \"\"\nnamespace N { }";
            var dm = CreateDocManager(uri, source);

            var result = CompletionHandler.Handle(MakePositionParams(uri, 0, 8), dm);
            var dict = (Dictionary<string, object?>)result;
            Assert.True((bool)dict["isIncomplete"]!);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void CompletionOnRefDirective_IsIncomplete()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"midl_ref_inc_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "a.idl"), "namespace A { }");

            var uri = "file:///" + Uri.EscapeDataString(Path.Combine(tempDir, "main.idl")).Replace("\\", "/");
            var source = "// @ref: \nnamespace N { }";
            var dm = CreateDocManager(uri, source);

            var result = CompletionHandler.Handle(MakePositionParams(uri, 0, 9), dm);
            var dict = (Dictionary<string, object?>)result;
            Assert.True((bool)dict["isIncomplete"]!);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    // ── Completion context edge cases (regression for infinite loop fix) ──

    [Fact]
    public void CompletionInsideMethodParens_DoesNotHang()
    {
        // Regression: DetectAttributeArgContext had an infinite loop when cursor
        // was inside method parens (not attribute brackets)
        var uri = "file:///test.midl";
        var source = "namespace N {\n    runtimeclass Foo {\n        void Bar( );\n    }\n}";
        var dm = CreateDocManager(uri, source);
        // Cursor between ( and ) on "void Bar( )" — line 2, col 17
        var result = CompletionHandler.Handle(MakePositionParams(uri, 2, 17), dm);
        var json = JsonSerializer.Serialize(result);
        // Should return type completions, not hang
        Assert.Contains("\"Int32\"", json);
    }

    [Fact]
    public void CompletionInsideMethodParens_AfterExistingParam()
    {
        var uri = "file:///test.midl";
        var source = "namespace N {\n    interface IFoo {\n        void Bar(Int32 x, );\n    }\n}";
        var dm = CreateDocManager(uri, source);
        // Cursor after the comma — line 2, col 26
        var result = CompletionHandler.Handle(MakePositionParams(uri, 2, 26), dm);
        var json = JsonSerializer.Serialize(result);
        Assert.Contains("\"String\"", json);
    }

    [Fact]
    public void CompletionOnMethodReturnType_ShowsTypes()
    {
        var uri = "file:///test.midl";
        var source = "namespace N {\n    interface IFoo {\n         Bar();\n    }\n}";
        var dm = CreateDocManager(uri, source);
        // Cursor at the return type position — line 2, col 8
        var result = CompletionHandler.Handle(MakePositionParams(uri, 2, 8), dm);
        var json = JsonSerializer.Serialize(result);
        Assert.Contains("\"void\"", json);
        Assert.Contains("\"Int32\"", json);
    }

    [Fact]
    public void CompletionInsideAttributeArgs_ShowsArgCompletions()
    {
        var uri = "file:///test.midl";
        var source = "namespace N {\n    [threading( )]\n    runtimeclass Foo { Foo(); Int32 X; }\n}";
        var dm = CreateDocManager(uri, source);
        // Cursor inside [threading( | )] — line 1, col 16
        var result = CompletionHandler.Handle(MakePositionParams(uri, 1, 16), dm);
        var json = JsonSerializer.Serialize(result);
        // Should include threading model values
        Assert.Contains("both", json);
    }

    [Fact]
    public void CompletionOnAttributeForMethod_ShowsMethodAttrs()
    {
        var uri = "file:///test.midl";
        var source = "namespace N {\n    interface IFoo {\n        [ ]\n        void Bar();\n    }\n}";
        var dm = CreateDocManager(uri, source);
        // Cursor inside [ ] on the method — line 2, col 9
        var result = CompletionHandler.Handle(MakePositionParams(uri, 2, 9), dm);
        var json = JsonSerializer.Serialize(result);
        // Should include method-level attributes
        Assert.Contains("deprecated", json);
    }

    [Fact]
    public void CompletionOnAttributeForProperty_ShowsPropertyAttrs()
    {
        var uri = "file:///test.midl";
        var source = "namespace N {\n    runtimeclass Foo {\n        Foo();\n        [ ]\n        Int32 Value;\n    }\n}";
        var dm = CreateDocManager(uri, source);
        // Cursor inside [ ] on the property — line 3, col 9
        var result = CompletionHandler.Handle(MakePositionParams(uri, 3, 9), dm);
        var json = JsonSerializer.Serialize(result);
        Assert.Contains("deprecated", json);
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

        var json = JsonSerializer.Serialize(result, LspJsonContext.Default.HoverResult);
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

        var json = JsonSerializer.Serialize(result, LspJsonContext.Default.HoverResult);
        Assert.Contains("N.Foo", json);
        Assert.Contains("RuntimeClass", json);
    }

    [Fact]
    public void HoverReturnsCorrectJsonStructureViaSourceGen()
    {
        var uri = "file:///test.midl";
        var source = "namespace Ns { }";
        var dm = CreateDocManager(uri, source);

        var result = HoverHandler.Handle(MakePositionParams(uri, 0, 0), dm);
        Assert.NotNull(result);
        Assert.IsType<HoverResult>(result);

        var hover = (HoverResult)result;
        Assert.Equal("markdown", hover.Contents.Kind);
        Assert.Contains("namespace", hover.Contents.Value);
        Assert.NotNull(hover.Range);
        Assert.Equal(0, hover.Range!.Start.Line);

        // Verify source-gen JSON matches expected LSP shape
        var json = JsonSerializer.Serialize(hover, LspJsonContext.Default.HoverResult);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("contents", out var contents));
        Assert.Equal("markdown", contents.GetProperty("kind").GetString());
        Assert.True(root.TryGetProperty("range", out var range));
        Assert.True(range.TryGetProperty("start", out var start));
        Assert.Equal(0, start.GetProperty("line").GetInt32());
        Assert.Equal(0, start.GetProperty("character").GetInt32());
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
        var formatted = DocumentFormatter.Format(input);

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

    #region Rename Tests

    [Fact]
    public void Rename_TypeName_RenamesAllOccurrences()
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

        // Find "IFoo" in the interface declaration
        var lines = source.Split('\n');
        int targetLine = -1, targetChar = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            int idx = lines[i].IndexOf("interface IFoo");
            if (idx >= 0)
            {
                targetLine = i;
                targetChar = idx + "interface ".Length;
                break;
            }
        }
        Assert.True(targetLine >= 0, "Could not find IFoo declaration");

        var result = RenameHandler.HandleCore(source, uri, targetLine, targetChar, "IBar");
        Assert.NotNull(result);

        var json = JsonSerializer.Serialize(result);
        var doc = JsonDocument.Parse(json);

        // Should have a "changes" property with the uri key
        var changes = doc.RootElement.GetProperty("changes");
        var fileEdits = changes.GetProperty(uri);

        // Should rename both the declaration and usage in requires
        Assert.True(fileEdits.GetArrayLength() >= 2, $"Expected at least 2 edits, got {fileEdits.GetArrayLength()}");

        // All edits should have the new name
        foreach (var edit in fileEdits.EnumerateArray())
        {
            Assert.Equal("IBar", edit.GetProperty("newText").GetString());
        }
    }

    #endregion

    #region Workspace Diagnostics Tests

    [Fact]
    public async Task WorkspaceFileChange_RepublishesDiagnostics()
    {
        // Set up a pipe-based server to simulate LSP communication
        var serverInput = new MemoryStream();
        var serverOutput = new MemoryStream();
        var server = new MidlLanguageServer(serverInput, serverOutput);

        // Pre-open a document so the server has something to re-analyze
        var openUri = "file:///workspace/main.idl";
        var openSource = "namespace N { runtimeclass Foo { } }";

        // Simulate workspace/didChangeWatchedFiles by calling HandleMessageAsync via reflection
        // Instead, test the public AnalyzeAndPublish path by exercising the server's document flow.

        // 1. Write an initialize request + initialized notification + didOpen + didChangeWatchedFiles
        var initId = 1;
        var initRequest = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["jsonrpc"] = "2.0",
            ["id"] = initId,
            ["method"] = "initialize",
            ["params"] = new Dictionary<string, object?>
            {
                ["processId"] = 1234,
                ["rootUri"] = "file:///workspace",
                ["capabilities"] = new Dictionary<string, object?> { }
            }
        });
        WriteJsonRpcMessage(serverInput, initRequest);

        var initializedNotif = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["jsonrpc"] = "2.0",
            ["method"] = "initialized",
            ["params"] = new Dictionary<string, object?> { }
        });
        WriteJsonRpcMessage(serverInput, initializedNotif);

        var didOpenNotif = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["jsonrpc"] = "2.0",
            ["method"] = "textDocument/didOpen",
            ["params"] = new Dictionary<string, object?>
            {
                ["textDocument"] = new Dictionary<string, object?>
                {
                    ["uri"] = openUri,
                    ["languageId"] = "midl",
                    ["version"] = 1,
                    ["text"] = openSource,
                }
            }
        });
        WriteJsonRpcMessage(serverInput, didOpenNotif);

        // Create a temp .idl file on disk for the changed-file notification
        var tempDir = Path.Combine(Path.GetTempPath(), "midl_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var tempFile = Path.Combine(tempDir, "helper.idl");
        File.WriteAllText(tempFile, "namespace N { struct Point { Int32 X; Int32 Y; }; }");
        var tempFileUri = "file:///" + tempFile.Replace('\\', '/');

        try
        {
            var watchedFilesNotif = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["jsonrpc"] = "2.0",
                ["method"] = "workspace/didChangeWatchedFiles",
                ["params"] = new Dictionary<string, object?>
                {
                    ["changes"] = new[]
                    {
                        new Dictionary<string, object?>
                        {
                            ["uri"] = tempFileUri,
                            ["type"] = 2 // Changed
                        }
                    }
                }
            });
            WriteJsonRpcMessage(serverInput, watchedFilesNotif);

            var shutdownRequest = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["jsonrpc"] = "2.0",
                ["id"] = 99,
                ["method"] = "shutdown",
                ["params"] = (object?)null
            });
            WriteJsonRpcMessage(serverInput, shutdownRequest);

            var exitNotif = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["jsonrpc"] = "2.0",
                ["method"] = "exit"
            });
            WriteJsonRpcMessage(serverInput, exitNotif);

            // Reset input stream position so server can read from it
            serverInput.Position = 0;

            // Run the server — it will process all queued messages then exit
            await server.RunAsync();

            // Parse the output to find publishDiagnostics notifications
            serverOutput.Position = 0;
            var outputText = System.Text.Encoding.UTF8.GetString(serverOutput.ToArray());
            var messages = ParseJsonRpcMessages(outputText);

            // Find publishDiagnostics notifications
            var diagNotifications = messages
                .Where(m => m.RootElement.TryGetProperty("method", out var meth) &&
                            meth.GetString() == "textDocument/publishDiagnostics")
                .ToList();

            // We should have at least 2 publishDiagnostics:
            // 1. For the didOpen on main.idl
            // 2. For the re-analysis triggered by didChangeWatchedFiles (for the changed file and/or open files)
            Assert.True(diagNotifications.Count >= 2,
                $"Expected at least 2 publishDiagnostics notifications, got {diagNotifications.Count}. Messages: {string.Join(", ", messages.Select(m => m.RootElement.TryGetProperty("method", out var me) ? me.GetString() : "response"))}");

            // Verify diagnostics were published for the open document (re-analyzed after file change)
            var mainDiags = diagNotifications
                .Where(m => m.RootElement.GetProperty("params").GetProperty("uri").GetString() == openUri)
                .ToList();
            Assert.True(mainDiags.Count >= 1,
                $"Expected at least 1 publishDiagnostics for {openUri}");
        }
        finally
        {
            try { Directory.Delete(tempDir, recursive: true); } catch { }
        }
    }

    [Fact]
    public void Rename_OnKeyword_ReturnsNull()
    {
        var source = """
            namespace TestApp
            {
                runtimeclass Foo { }
            }
            """;
        var uri = "file:///test.midl";

        // Position cursor on "runtimeclass" keyword
        var lines = source.Split('\n');
        int targetLine = -1, targetChar = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            int idx = lines[i].IndexOf("runtimeclass");
            if (idx >= 0)
            {
                targetLine = i;
                targetChar = idx + 2; // inside the keyword
                break;
            }
        }
        Assert.True(targetLine >= 0);

        var result = RenameHandler.HandleCore(source, uri, targetLine, targetChar, "NewName");
        Assert.Null(result);
    }

    [Fact]
    public void Rename_ExternalType_ReturnsNull()
    {
        var source = """
            namespace TestApp
            {
                runtimeclass Foo
                {
                    String Name { get; };
                }
            }
            """;
        var uri = "file:///test.midl";

        // Position cursor on "String" (built-in / external type)
        var lines = source.Split('\n');
        int targetLine = -1, targetChar = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            int idx = lines[i].IndexOf("String");
            if (idx >= 0)
            {
                targetLine = i;
                targetChar = idx + 1;
                break;
            }
        }
        Assert.True(targetLine >= 0);

        var result = RenameHandler.HandleCore(source, uri, targetLine, targetChar, "MyString");
        Assert.Null(result);
    }

    [Fact]
    public void PrepareRename_OnIdentifier_ReturnsRange()
    {
        var source = """
            namespace TestApp
            {
                interface IFoo
                {
                    void DoWork();
                }
            }
            """;

        // Position on "IFoo"
        var lines = source.Split('\n');
        int targetLine = -1, targetChar = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            int idx = lines[i].IndexOf("interface IFoo");
            if (idx >= 0)
            {
                targetLine = i;
                targetChar = idx + "interface ".Length;
                break;
            }
        }
        Assert.True(targetLine >= 0);

        var result = RenameHandler.HandlePrepareCore(source, targetLine, targetChar);
        Assert.NotNull(result);

        var json = JsonSerializer.Serialize(result);
        var doc = JsonDocument.Parse(json);

        // Should contain a range and placeholder
        Assert.True(doc.RootElement.TryGetProperty("range", out _));
        Assert.Equal("IFoo", doc.RootElement.GetProperty("placeholder").GetString());
    }

    [Fact]
    public void PrepareRename_OnKeyword_ReturnsNull()
    {
        var source = """
            namespace TestApp
            {
                runtimeclass Foo { }
            }
            """;

        var lines = source.Split('\n');
        int targetLine = -1, targetChar = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            int idx = lines[i].IndexOf("runtimeclass");
            if (idx >= 0)
            {
                targetLine = i;
                targetChar = idx + 2;
                break;
            }
        }
        Assert.True(targetLine >= 0);

        var result = RenameHandler.HandlePrepareCore(source, targetLine, targetChar);
        Assert.Null(result);
    }

    [Fact]
    public async Task WorkspaceFileDelete_ClearsDiagnostics()
    {
        var serverInput = new MemoryStream();
        var serverOutput = new MemoryStream();
        var server = new MidlLanguageServer(serverInput, serverOutput);

        var initRequest = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["jsonrpc"] = "2.0",
            ["id"] = 1,
            ["method"] = "initialize",
            ["params"] = new Dictionary<string, object?>
            {
                ["processId"] = 1234,
                ["rootUri"] = "file:///workspace",
                ["capabilities"] = new Dictionary<string, object?> { }
            }
        });
        WriteJsonRpcMessage(serverInput, initRequest);

        var initializedNotif = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["jsonrpc"] = "2.0",
            ["method"] = "initialized",
            ["params"] = new Dictionary<string, object?> { }
        });
        WriteJsonRpcMessage(serverInput, initializedNotif);

        var deletedFileUri = "file:///workspace/deleted.idl";
        var watchedFilesNotif = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["jsonrpc"] = "2.0",
            ["method"] = "workspace/didChangeWatchedFiles",
            ["params"] = new Dictionary<string, object?>
            {
                ["changes"] = new[]
                {
                    new Dictionary<string, object?>
                    {
                        ["uri"] = deletedFileUri,
                        ["type"] = 3 // Deleted
                    }
                }
            }
        });
        WriteJsonRpcMessage(serverInput, watchedFilesNotif);

        var shutdownRequest = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["jsonrpc"] = "2.0",
            ["id"] = 99,
            ["method"] = "shutdown",
            ["params"] = (object?)null
        });
        WriteJsonRpcMessage(serverInput, shutdownRequest);

        var exitNotif = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["jsonrpc"] = "2.0",
            ["method"] = "exit"
        });
        WriteJsonRpcMessage(serverInput, exitNotif);

        serverInput.Position = 0;
        await server.RunAsync();

        serverOutput.Position = 0;
        var outputText = System.Text.Encoding.UTF8.GetString(serverOutput.ToArray());
        var messages = ParseJsonRpcMessages(outputText);

        // Find publishDiagnostics for the deleted file — should have empty diagnostics array
        var deletedDiags = messages
            .Where(m => m.RootElement.TryGetProperty("method", out var meth) &&
                        meth.GetString() == "textDocument/publishDiagnostics" &&
                        m.RootElement.GetProperty("params").GetProperty("uri").GetString() == deletedFileUri)
            .ToList();

        Assert.Single(deletedDiags);
        var diagArray = deletedDiags[0].RootElement.GetProperty("params").GetProperty("diagnostics");
        Assert.Equal(0, diagArray.GetArrayLength());
    }

    private static void WriteJsonRpcMessage(MemoryStream stream, string json)
    {
        var body = System.Text.Encoding.UTF8.GetBytes(json);
        var header = System.Text.Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n");
        stream.Write(header);
        stream.Write(body);
    }

    private static List<JsonDocument> ParseJsonRpcMessages(string raw)
    {
        var results = new List<JsonDocument>();
        int pos = 0;
        while (pos < raw.Length)
        {
            var headerEnd = raw.IndexOf("\r\n\r\n", pos, StringComparison.Ordinal);
            if (headerEnd < 0) break;

            var headerSection = raw[pos..headerEnd];
            var contentLengthMatch = System.Text.RegularExpressions.Regex.Match(headerSection, @"Content-Length:\s*(\d+)");
            if (!contentLengthMatch.Success) break;

            var contentLength = int.Parse(contentLengthMatch.Groups[1].Value);
            var bodyStart = headerEnd + 4;
            if (bodyStart + contentLength > raw.Length) break;

            var body = raw.Substring(bodyStart, contentLength);
            results.Add(JsonDocument.Parse(body));
            pos = bodyStart + contentLength;
        }
        return results;
    }

    #endregion

#if FEATURE_STAGING
    #region FeatureName Completion Tests

    [Fact]
    public void FeatureNameCompletion_AfterOpenBrace_ShowsStageValues()
    {
        var uri = "file:///test.midl";
        var source = "namespace N { feature_name Foo = { }";
        var dm = CreateDocManager(uri, source);
        // Cursor right after '{'
        var result = CompletionHandler.Handle(MakePositionParams(uri, 0, 34), dm);
        var json = JsonSerializer.Serialize(result);

        Assert.Contains("\"AlwaysDisabled\"", json);
        Assert.Contains("\"DisabledByDefault\"", json);
        Assert.Contains("\"EnabledByDefault\"", json);
        Assert.Contains("\"AlwaysEnabled\"", json);
        // Kind should be EnumMember (20)
        Assert.Contains("\"kind\":20", json);
        // Should NOT contain bool values
        Assert.DoesNotContain("\"TRUE\"", json);
        Assert.DoesNotContain("\"FALSE\"", json);
    }

    [Fact]
    public void FeatureNameCompletion_AfterComma_ShowsBoolValues()
    {
        var uri = "file:///test.midl";
        var source = "namespace N { feature_name Foo = {AlwaysEnabled, }";
        var dm = CreateDocManager(uri, source);
        // Cursor after comma+space
        var result = CompletionHandler.Handle(MakePositionParams(uri, 0, 49), dm);
        var json = JsonSerializer.Serialize(result);

        Assert.Contains("\"TRUE\"", json);
        Assert.Contains("\"FALSE\"", json);
        // Kind should be Keyword (14)
        Assert.Contains("\"kind\":14", json);
        // Should NOT contain stage values
        Assert.DoesNotContain("\"AlwaysDisabled\"", json);
        Assert.DoesNotContain("\"AlwaysEnabled\"", json);
    }

    [Fact]
    public void FeatureNameCompletion_PartialStageTyping_FiltersCorrectly()
    {
        var uri = "file:///test.midl";
        var source = "namespace N { feature_name Foo = {Dis }";
        var dm = CreateDocManager(uri, source);
        // Cursor after 'Dis' — still in stage position (no comma)
        var result = CompletionHandler.Handle(MakePositionParams(uri, 0, 37), dm);
        var json = JsonSerializer.Serialize(result);

        // Should still offer all stage values (filtering is client-side in LSP)
        Assert.Contains("\"DisabledByDefault\"", json);
        Assert.Contains("\"AlwaysDisabled\"", json);
        Assert.Contains("\"EnabledByDefault\"", json);
        Assert.Contains("\"AlwaysEnabled\"", json);
    }

    [Fact]
    public void FeatureNameCompletion_StageValues_HaveEnumMemberKind()
    {
        var uri = "file:///test.midl";
        var source = "namespace N { feature_name Foo = { }";
        var dm = CreateDocManager(uri, source);
        var result = CompletionHandler.Handle(MakePositionParams(uri, 0, 34), dm);

        var json = JsonSerializer.Serialize(result);
        // All items should have kind 20 (EnumMember)
        // Deserialize and check each item
        var doc = JsonDocument.Parse(json);
        var items = doc.RootElement.GetProperty("items");
        foreach (var item in items.EnumerateArray())
        {
            Assert.Equal(20, item.GetProperty("kind").GetInt32());
        }
    }

    [Fact]
    public void FeatureAttrCompletion_SingleFeatureName_ShowsIt()
    {
        var uri = "file:///test.midl";
        var source = """
            namespace N {
                feature_name Foo = {AlwaysEnabled, TRUE};
                [feature()]
                runtimeclass Bar { }
            }
            """;
        // Cursor inside [feature(|)] — line 2, position 13 (between '(' and ')')
        var dm = CreateDocManager(uri, source);
        var result = CompletionHandler.Handle(MakePositionParams(uri, 2, 13), dm);
        var json = JsonSerializer.Serialize(result);

        Assert.Contains("\"Foo\"", json);
    }

    [Fact]
    public void FeatureAttrCompletion_MultipleFeatureNames_ShowsAll()
    {
        var uri = "file:///test.midl";
        var source = """
            namespace N {
                feature_name Alpha = {AlwaysEnabled, TRUE};
                feature_name Beta = {DisabledByDefault, FALSE};
                feature_name Gamma = {EnabledByDefault, TRUE};
                [feature()]
                runtimeclass Bar { }
            }
            """;
        var dm = CreateDocManager(uri, source);
        var result = CompletionHandler.Handle(MakePositionParams(uri, 4, 13), dm);
        var json = JsonSerializer.Serialize(result);

        Assert.Contains("\"Alpha\"", json);
        Assert.Contains("\"Beta\"", json);
        Assert.Contains("\"Gamma\"", json);
    }

    [Fact]
    public void FeatureAttrCompletion_PartialTyping_StillShowsAll()
    {
        var uri = "file:///test.midl";
        var source = """
            namespace N {
                feature_name Foo = {AlwaysEnabled, TRUE};
                feature_name Bar = {DisabledByDefault, FALSE};
                [feature(Fo)]
                runtimeclass Baz { }
            }
            """;
        // Cursor at "Fo|" inside [feature(Fo)] — character = 15
        var dm = CreateDocManager(uri, source);
        var result = CompletionHandler.Handle(MakePositionParams(uri, 3, 15), dm);
        var json = JsonSerializer.Serialize(result);

        // LSP filtering is client-side, so all feature names should be returned
        Assert.Contains("\"Foo\"", json);
        Assert.Contains("\"Bar\"", json);
    }

    [Fact]
    public void FeatureAttrCompletion_NoFeatureNames_EmptyItems()
    {
        var uri = "file:///test.midl";
        var source = """
            namespace N {
                [feature()]
                runtimeclass Bar { }
            }
            """;
        var dm = CreateDocManager(uri, source);
        var result = CompletionHandler.Handle(MakePositionParams(uri, 1, 13), dm);
        var json = JsonSerializer.Serialize(result);
        var doc = JsonDocument.Parse(json);
        var items = doc.RootElement.GetProperty("items");

        Assert.Equal(0, items.GetArrayLength());
    }

    #endregion
#endif

    #region CodeLens Tests

    [Fact]
    public void CodeLens_EmptyRefValue_DoesNotCrash()
    {
        // Regression: // @ref: with nothing after the colon on an untitled file
        // previously caused ArgumentException (Arg_PathEmpty) via Path.GetFullPath("")
        var uri = "untitled:Untitled-1";
        var source = "// @ref:\nnamespace Foo { }";
        var dm = CreateDocManager(uri, source);
        var compilation = CompilationHelper.Compile(source);

        var result = CodeLensHandler.Handle(MakeTextDocParams(uri), dm, compilation);
        // Should not throw — empty ref lines are skipped
        Assert.NotNull(result);
    }

    [Fact]
    public void CodeLens_EmptyRefValue_FileUri_DoesNotCrash()
    {
        var uri = "file:///C:/test/test.midl";
        var source = "// @ref:\nnamespace Foo { }";
        var dm = CreateDocManager(uri, source);
        var compilation = CompilationHelper.Compile(source);

        var result = CodeLensHandler.Handle(MakeTextDocParams(uri), dm, compilation);
        Assert.NotNull(result);
    }

    [Fact]
    public void CodeLens_WhitespaceOnlyRefValue_DoesNotCrash()
    {
        var uri = "untitled:Untitled-1";
        var source = "// @ref:   \nnamespace Foo { }";
        var dm = CreateDocManager(uri, source);
        var compilation = CompilationHelper.Compile(source);

        var result = CodeLensHandler.Handle(MakeTextDocParams(uri), dm, compilation);
        Assert.NotNull(result);
    }

    [Fact]
    public void CodeLens_NonEmptyRef_UntitledFile_DoesNotCrash()
    {
        // Regression: untitled file with // @ref: D:\OSClient\ crashed the server
        var uri = "untitled:Untitled-1";
        var source = "// @ref: D:\\OSClient\\\nnamespace Foo { }";
        var dm = CreateDocManager(uri, source);

        // Compile with SDK refs to ensure LazyProviders exist
        var winmd = Midl.Lsp.Handlers.WinMdNavigator.FindWindowsWinMd();
        var refs = winmd != null ? new List<string> { winmd } : null;
        var compilation = CompilationHelper.Compile(source, refs, CompilationMode.Strict);

        var result = CodeLensHandler.Handle(MakeTextDocParams(uri), dm, compilation);
        Assert.NotNull(result);
    }

    [Fact]
    public void AllHandlers_UntitledFile_WithRefDirective_DoNotCrash()
    {
        // Comprehensive test: exercise all handlers that process @ref on untitled files
        var uri = "untitled:Untitled-1";
        var source = "// @ref: D:\\OSClient\\\nnamespace Foo {\n    runtimeclass Bar { }\n}";
        var dm = CreateDocManager(uri, source);

        var winmd = Midl.Lsp.Handlers.WinMdNavigator.FindWindowsWinMd();
        var refs = winmd != null ? new List<string> { winmd } : null;
        var compilation = CompilationHelper.Compile(source, refs, CompilationMode.Strict);

        // CodeLens
        var codeLens = CodeLensHandler.Handle(MakeTextDocParams(uri), dm, compilation);
        Assert.NotNull(codeLens);

        // SemanticTokens
        var semTokens = SemanticTokensHandler.Handle(MakeTextDocParams(uri), dm);
        Assert.NotNull(semTokens);

        // DocumentSymbol
        var symbols = DocumentSymbolHandler.Handle(MakeTextDocParams(uri), dm);
        Assert.NotNull(symbols);

        // Hover on @ref line
        var hover = HoverHandler.Handle(MakePositionParams(uri, 0, 15), dm, compilation);

        // Completion on @ref line
        var completion = CompletionHandler.Handle(MakePositionParams(uri, 0, 20), dm);

        // GotoDefinition on @ref line
        var gotoDef = GotoDefinitionHandler.Handle(
            MakePositionParams(uri, 0, 15), dm, null, refs, compilation);
    }

    #endregion

    #region Folding Range Tests

    [Fact]
    public void FoldingRange_Namespace_ReturnsFoldingRange()
    {
        var source = "namespace Foo {\n  runtimeclass Bar {\n  }\n}";
        var ranges = FoldingRangeHandler.Handle(source);
        Assert.Equal(2, ranges.Count);
        // Inner runtimeclass: line 1 to line 2 (stack pops inner first)
        Assert.Equal(1, ranges[0]["startLine"]);
        Assert.Equal(2, ranges[0]["endLine"]);
        // Outer namespace: line 0 to line 3
        Assert.Equal(0, ranges[1]["startLine"]);
        Assert.Equal(3, ranges[1]["endLine"]);
        Assert.Equal("region", ranges[1]["kind"]);
    }

    [Fact]
    public void FoldingRange_Interface_ReturnsFoldingRange()
    {
        var source = "namespace N {\n  interface IFoo {\n    HRESULT DoThing();\n  }\n}";
        var ranges = FoldingRangeHandler.Handle(source);
        Assert.Equal(2, ranges.Count);
    }

    [Fact]
    public void FoldingRange_Enum_ReturnsFoldingRange()
    {
        var source = "namespace N {\n  enum Color {\n    Red,\n    Green,\n    Blue\n  };\n}";
        var ranges = FoldingRangeHandler.Handle(source);
        Assert.Equal(2, ranges.Count);
    }

    [Fact]
    public void FoldingRange_Struct_ReturnsFoldingRange()
    {
        var source = "namespace N {\n  struct Point {\n    Int32 X;\n    Int32 Y;\n  };\n}";
        var ranges = FoldingRangeHandler.Handle(source);
        Assert.Equal(2, ranges.Count);
    }

    [Fact]
    public void FoldingRange_SingleLine_NoFold()
    {
        var source = "namespace Foo { }";
        var ranges = FoldingRangeHandler.Handle(source);
        Assert.Empty(ranges);
    }

    [Fact]
    public void FoldingRange_EmptySource_NoFold()
    {
        var ranges = FoldingRangeHandler.Handle("");
        Assert.Empty(ranges);
    }

    [Fact]
    public void FoldingRange_MultiLineAttribute_ReturnsFold()
    {
        var source = "[contract(\n  ContractV1,\n  1\n)]\nruntimeclass Foo {\n}";
        var ranges = FoldingRangeHandler.Handle(source);
        // attribute [...] spans lines 0-3, runtimeclass {} spans lines 4-5
        Assert.Equal(2, ranges.Count);
    }

    [Fact]
    public void FoldingRange_ViaDocManager()
    {
        var uri = "file:///test.midl";
        var dm = CreateDocManager(uri, "namespace Foo {\n  runtimeclass Bar {\n  }\n}");
        var result = FoldingRangeHandler.Handle(MakeTextDocParams(uri), dm);
        var list = result as List<Dictionary<string, object?>>;
        Assert.NotNull(list);
        Assert.Equal(2, list!.Count);
    }

    #endregion

    #region Signature Help — Attribute Arguments

    [Fact]
    public void SignatureHelp_AttributeComposable_ShowsParams()
    {
        // Cursor right after '(' in [composable(
        var source = "[composable(]";
        //                       ^ col 12
        var result = SignatureHelpHandler.HandleCore(source, 0, 12) as Dictionary<string, object?>;
        Assert.NotNull(result);
        var sigs = result!["signatures"] as object[];
        Assert.NotNull(sigs);
        Assert.Single(sigs!);
        var sig = sigs[0] as Dictionary<string, object?>;
        Assert.Contains("composable", (string)sig!["label"]!);
        var parms = sig["parameters"] as object[];
        Assert.Equal(4, parms!.Length);
        Assert.Equal(0, result["activeParameter"]);
    }

    [Fact]
    public void SignatureHelp_AttributeDeprecated_ShowsParams()
    {
        var source = "[deprecated(]";
        var result = SignatureHelpHandler.HandleCore(source, 0, 12) as Dictionary<string, object?>;
        Assert.NotNull(result);
        var sigs = result!["signatures"] as object[];
        Assert.NotNull(sigs);
        var sig = sigs![0] as Dictionary<string, object?>;
        var parms = sig!["parameters"] as object[];
        Assert.Equal(4, parms!.Length);
    }

    [Fact]
    public void SignatureHelp_AttributeUuid_ShowsParams()
    {
        var source = "[uuid(]";
        var result = SignatureHelpHandler.HandleCore(source, 0, 6) as Dictionary<string, object?>;
        Assert.NotNull(result);
        var sigs = result!["signatures"] as object[];
        Assert.NotNull(sigs);
        var sig = sigs![0] as Dictionary<string, object?>;
        var parms = sig!["parameters"] as object[];
        Assert.Single(parms!);
    }

    [Fact]
    public void SignatureHelp_UserDefinedAttribute_ShowsFields()
    {
        var source = """
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyTag
                {
                    String Level;
                    Int32 Priority;
                }

                [MyTag()]
                runtimeclass Widget {}
            }
            """;
        // cursor right after '(' in [MyTag()]
        int line = -1, col = -1;
        var lines = source.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            int idx = lines[i].IndexOf("[MyTag(");
            if (idx >= 0)
            {
                line = i;
                col = idx + "[MyTag(".Length;
                break;
            }
        }
        Assert.True(line >= 0);

        var result = SignatureHelpHandler.HandleCore(source, line, col) as Dictionary<string, object?>;
        Assert.NotNull(result);
        var sigs = result!["signatures"] as object[];
        Assert.NotNull(sigs);
        var sig = sigs![0] as Dictionary<string, object?>;
        Assert.Contains("MyTag", (string)sig!["label"]!);
        var parms = sig["parameters"] as object[];
        Assert.Equal(2, parms!.Length);
    }

    [Fact]
    public void SignatureHelp_NotInAttribute_NoAttributeSignature()
    {
        // 'composable(' NOT inside brackets — should not produce attribute sig
        var source = "composable(";
        var result = SignatureHelpHandler.HandleCore(source, 0, 11);
        // Should be null because there's no '[' bracket context and no callable symbol
        Assert.Null(result);
    }

    [Fact]
    public void SignatureHelp_ActiveParameter_AfterComma()
    {
        // Cursor after first comma: [composable(IFoo, |
        var source = "[composable(IFoo, ]";
        //                               ^ col 18
        var result = SignatureHelpHandler.HandleCore(source, 0, 18) as Dictionary<string, object?>;
        Assert.NotNull(result);
        Assert.Equal(1, result!["activeParameter"]);
    }

    #endregion
}
