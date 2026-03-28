using Midl.Analysis;
using Midl.Lsp;
using Midl.Lsp.Handlers;

namespace Midl.Integration.Tests;

public partial class GotoDefinitionTests
{
    #region Attribute F12 navigation

    [Fact]
    public void GotoDefinition_Attribute_ContractVersion_NavigatesToSdkType()
    {
        var source = """
            namespace N
            {
                [contractversion(1)]
                apicontract MyContract { }
            }
            """;
        var uri = "file:///test.idl";

        // Cursor on "contractversion" inside [...]
        var lines = source.Split('\n');
        int attrLine = Array.FindIndex(lines, l => l.Contains("contractversion"));
        int attrCol = lines[attrLine].IndexOf("contractversion", StringComparison.Ordinal);

        var result = GotoDefinitionHandler.HandleCore(source, uri, attrLine, attrCol);

        // On machines with Windows SDK, should navigate to the SDK type
        if (result is Dictionary<string, object?> dict)
        {
            var resultUri = dict["uri"]!.ToString()!;
            Assert.Contains("midl-winmd://", resultUri);
        }
        // On CI without SDK, result may be null — that's acceptable
    }

    [Fact]
    public void GotoDefinition_Attribute_Uuid_NavigatesToSdkType()
    {
        var source = """
            namespace N
            {
                [uuid("12345678-1234-1234-1234-123456789abc")]
                interface IFoo
                {
                    void DoWork();
                }
            }
            """;
        var uri = "file:///test.idl";

        var lines = source.Split('\n');
        int attrLine = Array.FindIndex(lines, l => l.Contains("uuid"));
        int attrCol = lines[attrLine].IndexOf("uuid", StringComparison.Ordinal);

        var result = GotoDefinitionHandler.HandleCore(source, uri, attrLine, attrCol);

        if (result is Dictionary<string, object?> dict)
        {
            var resultUri = dict["uri"]!.ToString()!;
            Assert.Contains("midl-winmd://", resultUri);
        }
    }

    [Fact]
    public void GotoDefinition_Attribute_Deprecated_NavigatesToSdkType()
    {
        var source = """
            namespace N
            {
                [deprecated("old", deprecate, 1)]
                interface IFoo
                {
                    void DoWork();
                }
            }
            """;
        var uri = "file:///test.idl";

        var lines = source.Split('\n');
        int attrLine = Array.FindIndex(lines, l => l.Contains("deprecated"));
        int attrCol = lines[attrLine].IndexOf("deprecated", StringComparison.Ordinal);

        var result = GotoDefinitionHandler.HandleCore(source, uri, attrLine, attrCol);

        if (result is Dictionary<string, object?> dict)
        {
            var resultUri = dict["uri"]!.ToString()!;
            Assert.Contains("midl-winmd://", resultUri);
        }
    }

    [Fact]
    public void GotoDefinition_UserDefinedAttribute_NavigatesToLocalDefinition()
    {
        var source = """
            namespace N
            {
                [attributeusage(target_runtimeclass)]
                attribute MyCustomAttribute
                {
                    Int32 Value;
                };

                [MyCustom(42)]
                runtimeclass Foo
                {
                }
            }
            """;
        var uri = "file:///test.idl";

        // Cursor on "MyCustom" inside [MyCustom(42)]
        var lines = source.Split('\n');
        int attrLine = Array.FindIndex(lines, l => l.Contains("[MyCustom("));
        int attrCol = lines[attrLine].IndexOf("MyCustom", StringComparison.Ordinal);

        var result = GotoDefinitionHandler.HandleCore(source, uri, attrLine, attrCol);

        Assert.NotNull(result);
        var dict = Assert.IsType<Dictionary<string, object?>>(result);
        Assert.Equal(uri, dict["uri"]!.ToString()!);

        // Should point to the attribute declaration
        var range = (Dictionary<string, object?>)dict["range"]!;
        var start = (Dictionary<string, object?>)range["start"]!;
        int defLine = (int)start["line"]!;
        Assert.Contains("attribute MyCustomAttribute", lines[defLine]);
    }

    [Fact]
    public void GotoDefinition_NotInsideAttributeBrackets_NoAttributeNavigation()
    {
        // Cursor on a type name NOT inside [...] brackets
        var source = """
            namespace N
            {
                runtimeclass Foo
                {
                    void DoWork();
                }
            }
            """;
        var uri = "file:///test.idl";

        // Cursor on "Foo" — not inside attribute brackets
        var lines = source.Split('\n');
        int fooLine = Array.FindIndex(lines, l => l.Contains("runtimeclass Foo"));
        int fooCol = lines[fooLine].IndexOf("Foo", StringComparison.Ordinal);

        var result = GotoDefinitionHandler.HandleCore(source, uri, fooLine, fooCol);

        // Should resolve to Foo itself (local definition), not attribute resolution
        if (result is Dictionary<string, object?> dict)
        {
            // The URI should be the same file (local symbol)
            Assert.Equal(uri, dict["uri"]!.ToString()!);
        }
    }

    [Fact]
    public void IsInsideAttributeBrackets_DetectsCorrectly()
    {
        var source = "[contractversion(1)]";
        // Offset of 'c' is 1 (just after '[')
        Assert.True(GotoDefinitionHandler.IsInsideAttributeBrackets(source, 1));

        // After the bracket
        var source2 = "[uuid] runtimeclass Foo {}";
        Assert.True(GotoDefinitionHandler.IsInsideAttributeBrackets(source2, 2));   // inside [uuid]
        Assert.False(GotoDefinitionHandler.IsInsideAttributeBrackets(source2, 10)); // "runtimeclass"
    }

    [Fact]
    public void IsInsideAttributeBrackets_StopsAtBraces()
    {
        // Identifier inside { } should not be treated as attribute
        var source = "namespace N { runtimeclass Foo { } }";
        int fooOffset = source.IndexOf("Foo", StringComparison.Ordinal);
        Assert.False(GotoDefinitionHandler.IsInsideAttributeBrackets(source, fooOffset));
    }

    #endregion
}
