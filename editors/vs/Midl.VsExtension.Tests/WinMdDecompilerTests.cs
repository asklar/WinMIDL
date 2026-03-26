namespace Midl.VsExtension.Tests;

/// <summary>
/// Tests for the WinMD decompiler URI parsing logic.
/// </summary>
public class WinMdDecompilerTests
{
    [Theory]
    [InlineData("midl-winmd:///C:/Program Files/Windows.winmd/Windows.Foundation.IAsyncAction")]
    [InlineData("midl-winmd:///C:/sdk/Windows.winmd/Windows.UI.Xaml.Controls")]
    [InlineData("midl-winmd:///C:/sdk/Windows.winmd")]
    public void Decompile_ParsesUriCorrectly(string uri)
    {
        // We can't fully test decompilation without the CLI binary,
        // but we verify URI parsing doesn't throw and returns content.
        string? result = WinMdDecompiler.Decompile(uri);

        // Should always return something (either content or error message)
        Assert.NotNull(result);

        if (ServerDiscovery.FindServerPath() is null)
        {
            Assert.Contains("MIDL server binary not found", result);
        }
    }

    [Fact]
    public void Decompile_HandlesEmptyUri()
    {
        string? result = WinMdDecompiler.Decompile("");
        Assert.NotNull(result);
    }

    [Fact]
    public void SelectSdkVersionCommand_DiscoverSdkVersions_DoesNotThrow()
    {
        var ex = Record.Exception(SelectSdkVersionCommand.DiscoverSdkVersions);
        Assert.Null(ex);
    }

    [Fact]
    public void SelectSdkVersionCommand_DiscoverSdkVersions_ReturnsListOfStrings()
    {
        List<string> versions = SelectSdkVersionCommand.DiscoverSdkVersions();
        Assert.NotNull(versions);
        // On machines with Windows SDK installed, we'd expect results.
        // On CI without SDK, we expect an empty list.
        foreach (string v in versions)
        {
            Assert.False(string.IsNullOrEmpty(v));
        }
    }
}
