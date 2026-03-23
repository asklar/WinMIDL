using Midl.Analysis;
using Midl.Lsp;
using Midl.Lsp.Handlers;

namespace Midl.Analysis.Tests;

public class GotoDefinitionTests
{
    [Fact]
    public void GotoDefinitionOfType()
    {
        var source = """
            namespace TestApp
            {
                runtimeclass Foo
                {
                    Foo GetSelf();
                }
            }
            """;

        // Find position of the "Foo" in "Foo GetSelf()" — the reference, not the definition
        var lines = source.Split('\n');
        int targetLine = -1;
        int targetChar = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            int idx = lines[i].IndexOf("Foo GetSelf");
            if (idx >= 0)
            {
                targetLine = i;
                targetChar = idx;
                break;
            }
        }
        Assert.True(targetLine >= 0, "Could not find reference position");

        var result = GotoDefinitionHandler.HandleCore(source, "file:///test.midl", targetLine, targetChar);
        Assert.NotNull(result);

        // Result should point to the definition of "runtimeclass Foo"
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        Assert.Contains("\"uri\":\"file:///test.midl\"", json);
        // The definition is on the "runtimeclass Foo" line
        var defLine = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("runtimeclass Foo"))
            {
                defLine = i;
                break;
            }
        }
        Assert.Contains($"\"line\":{defLine}", json);
    }

    [Fact]
    public void GotoDefinitionOfInterface()
    {
        var source = """
            namespace TestApp
            {
                interface IFoo
                {
                    void DoWork();
                }
                runtimeclass Bar
                {
                    IFoo GetFoo();
                }
            }
            """;

        var lines = source.Split('\n');
        int targetLine = -1;
        int targetChar = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            int idx = lines[i].IndexOf("IFoo GetFoo");
            if (idx >= 0)
            {
                targetLine = i;
                targetChar = idx;
                break;
            }
        }
        Assert.True(targetLine >= 0, "Could not find reference position");

        var result = GotoDefinitionHandler.HandleCore(source, "file:///test.midl", targetLine, targetChar);
        Assert.NotNull(result);

        var json = System.Text.Json.JsonSerializer.Serialize(result);
        // Should point to the "interface IFoo" declaration
        var defLine = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("interface IFoo"))
            {
                defLine = i;
                break;
            }
        }
        Assert.Contains($"\"line\":{defLine}", json);
    }

    [Fact]
    public void GotoDefinitionUnresolved()
    {
        var source = """
            namespace TestApp
            {
                runtimeclass Widget
                {
                    UnknownType GetThing();
                }
            }
            """;

        // Position on "UnknownType"
        var lines = source.Split('\n');
        int targetLine = -1;
        int targetChar = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            int idx = lines[i].IndexOf("UnknownType");
            if (idx >= 0)
            {
                targetLine = i;
                targetChar = idx;
                break;
            }
        }
        Assert.True(targetLine >= 0, "Could not find reference position");

        // UnknownType is not defined, so we should get null (or the name as found text which is fine too)
        // Since the symbol won't resolve in the compilation, HandleCore should return null
        var result = GotoDefinitionHandler.HandleCore(source, "file:///test.midl", targetLine, targetChar);
        // UnknownType is not in the symbol table as a proper type, so navigation should fail
        Assert.Null(result);
    }

    [Fact]
    public void F12OnRefLine_ReturnsFileLocation()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"winmidl_f12_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "Helper.idl"), "namespace H { }");
            var source = "// @ref: Helper.idl\nnamespace N { }\n";
            var uri = "file:///" + tempDir.Replace('\\', '/') + "/test.idl";

            var result = GotoDefinitionHandler.HandleCore(source, uri, 0, 10);
            Assert.NotNull(result);

            var json = System.Text.Json.JsonSerializer.Serialize(result);
            Assert.Contains("Helper.idl", json);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void F12OnRefLine_QuotedPath()
    {
        var source = "// @ref: \"Some Path.winmd\"\nnamespace N { }\n";
        var lineStarts = Midl.Lsp.PositionHelper.BuildLineStarts(source);

        var target = RefDirectiveNavigator.TryGetRefDirectiveTarget(source, lineStarts, 0);
        Assert.NotNull(target);
        Assert.Equal("Some Path.winmd", target);
    }

    [Fact]
    public void F12OnRefLine_DirectoryPath()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"winmidl_f12d_{Guid.NewGuid():N}");
        var refDir = Path.Combine(tempDir, "refs");
        Directory.CreateDirectory(refDir);
        try
        {
            var source = "// @ref: refs\nnamespace N { }\n";
            var uri = "file:///" + tempDir.Replace('\\', '/') + "/test.idl";

            var result = GotoDefinitionHandler.HandleCore(source, uri, 0, 10);
            Assert.NotNull(result);

            var json = System.Text.Json.JsonSerializer.Serialize(result);
            Assert.Contains("refs", json);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void FindRefLineForExternalSymbol_MatchesByFilename()
    {
        var source = "// @ref: ExtLib.winmd\n// @ref: Other.idl\nnamespace N { }\n";
        var symbol = new Midl.Analysis.Symbols.RuntimeClassSymbol("Widget",
            new Midl.Analysis.Symbols.NamespaceSymbol("ExtLib"))
        {
            IsExternal = true,
            SourceWinMdPath = @"C:\some\path\ExtLib.winmd"
        };

        var lineNum = RefDirectiveNavigator.FindRefLineForExternalSymbol(source, symbol);
        Assert.Equal(0, lineNum);
    }

    [Fact]
    public void FindRefLineForExternalSymbol_MatchesByNamespace()
    {
        var source = "// @ref: refs\n// @ref: Microsoft.UI.Xaml.winmd\nnamespace N { }\n";
        var ns = new Midl.Analysis.Symbols.NamespaceSymbol("Xaml");
        var parent = new Midl.Analysis.Symbols.NamespaceSymbol("UI");
        // Simulate full namespace Microsoft.UI.Xaml
        var symbol = new Midl.Analysis.Symbols.RuntimeClassSymbol("Control", ns)
        {
            IsExternal = true,
            SourceWinMdPath = @"D:\Build\Microsoft.UI.Xaml.winmd"
        };

        var lineNum = RefDirectiveNavigator.FindRefLineForExternalSymbol(source, symbol);
        Assert.Equal(1, lineNum);
    }

    [Fact]
    public void F12OnNonRefComment_ReturnsNull()
    {
        var source = "// This is a regular comment\nnamespace N { }\n";
        var lineStarts = Midl.Lsp.PositionHelper.BuildLineStarts(source);

        var target = RefDirectiveNavigator.TryGetRefDirectiveTarget(source, lineStarts, 0);
        Assert.Null(target);
    }
}
