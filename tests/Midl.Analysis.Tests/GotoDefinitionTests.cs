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
}
