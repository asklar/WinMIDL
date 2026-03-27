using Midl.Cli;

namespace Midl.Integration.Tests;

/// <summary>
/// Tests for /exceptions CLI flag parsing.
/// </summary>
public class ExceptionsFlagTests
{
    [Fact]
    public void ParseExceptionsFlag_SetsUseExceptions()
    {
        var args = new[] { "/exceptions", "input.idl" };
        var options = CommandLineParser.Parse(args);

        Assert.True(options.UseExceptions);
        Assert.Equal("input.idl", options.InputFile);
    }

    [Fact]
    public void DefaultOptions_UseExceptionsIsFalse()
    {
        var args = new[] { "input.idl" };
        var options = CommandLineParser.Parse(args);

        Assert.False(options.UseExceptions);
    }

    [Fact]
    public void ExceptionsFlag_NotRecordedAsUnknown()
    {
        var args = new[] { "/exceptions", "input.idl" };
        var options = CommandLineParser.Parse(args);

        Assert.DoesNotContain("/exceptions", options.UnknownSwitches);
    }
}
