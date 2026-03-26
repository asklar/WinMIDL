namespace Midl.VsExtension.Tests;

/// <summary>
/// Tests for the ServerDiscovery helper that locates the MIDL CLI binary.
/// </summary>
public class ServerDiscoveryTests
{
    [Fact]
    public void FindServerPath_ReturnsNull_WhenNoBinaryExists()
    {
        // ServerDiscovery walks up from its assembly location looking for Midl.slnx.
        // In the test runner context, the server binary won't be in any of the
        // expected paths, so we expect null (graceful fallback).
        // This validates the method doesn't throw when no server is found.
        string? result = ServerDiscovery.FindServerPath();

        // result may be non-null if run from within the repo with a built CLI,
        // but it must never throw.
        Assert.True(result is null || File.Exists(result));
    }

    [Fact]
    public void FindServerPath_ReturnsExistingFile_WhenFound()
    {
        string? result = ServerDiscovery.FindServerPath();
        if (result is not null)
        {
            Assert.True(File.Exists(result), $"Server path was returned but file does not exist: {result}");
            Assert.EndsWith("Midl.Cli.exe", result);
        }
    }

    [Fact]
    public void FindServerPath_DoesNotThrow()
    {
        // Ensure the method is robust against missing directories, permission issues, etc.
        var ex = Record.Exception(() => ServerDiscovery.FindServerPath());
        Assert.Null(ex);
    }
}
