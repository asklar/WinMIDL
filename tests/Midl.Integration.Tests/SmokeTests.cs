using Midl.Cli;

namespace Midl.Integration.Tests;

public class CommandLineParserTests
{
    [Fact]
    public void ParseBasicOptions()
    {
        var args = new[] { "/winrt", "/h", "output.h", "/out", "dir", "input.idl" };
        var options = CommandLineParser.Parse(args);

        Assert.True(options.WinRtMode);
        Assert.Equal("output.h", options.HeaderFile);
        Assert.Equal("dir", options.OutputDirectory);
        Assert.Equal("input.idl", options.InputFile);
    }

    [Fact]
    public void ParseDefines()
    {
        var args = new[] { "/D", "FOO", "/D", "BAR=1" };
        var options = CommandLineParser.Parse(args);

        Assert.Equal(2, options.Defines.Count);
        Assert.Equal("FOO", options.Defines[0]);
        Assert.Equal("BAR=1", options.Defines[1]);
    }

    [Fact]
    public void ParseWarningLevel()
    {
        var args = new[] { "/W3", "/WX" };
        var options = CommandLineParser.Parse(args);

        Assert.Equal(3, options.WarningLevel);
        Assert.True(options.WarningsAsErrors);
    }

    [Fact]
    public void ParseResponseFile()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "/winrt /h output.h input.idl");
            var args = new[] { $"@{tempFile}" };
            var options = CommandLineParser.Parse(args);

            Assert.True(options.WinRtMode);
            Assert.Equal("output.h", options.HeaderFile);
            Assert.Equal("input.idl", options.InputFile);
            Assert.Equal(tempFile, options.ResponseFile);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseReferences()
    {
        var args = new[] { "/reference", "a.winmd", "/reference", "b.winmd" };
        var options = CommandLineParser.Parse(args);

        Assert.Equal(2, options.References.Count);
        Assert.Equal("a.winmd", options.References[0]);
        Assert.Equal("b.winmd", options.References[1]);
    }

    [Fact]
    public void HelpFlag()
    {
        var args = new[] { "/?" };
        var options = CommandLineParser.Parse(args);

        Assert.True(options.ShowHelp);
    }

    [Fact]
    public void SyntaxCheckMode()
    {
        var args = new[] { "/Zs", "input.idl" };
        var options = CommandLineParser.Parse(args);

        Assert.True(options.SyntaxCheckOnly);
        Assert.Equal("input.idl", options.InputFile);
    }

    [Fact]
    public void EndToEndCompile()
    {
        var source = @"
namespace TestApp
{
    runtimeclass Widget
    {
        Widget();
        String Name { get; };
    }
}";
        var header = CompilationDriver.CompileToHeader(source);

        Assert.NotNull(header);
        Assert.NotEmpty(header);
        Assert.Contains("Widget", header);
    }

    [Fact]
    public void UnknownSwitchWarning()
    {
        var args = new[] { "/bogus", "input.idl" };
        var options = CommandLineParser.Parse(args);

        Assert.Contains("/bogus", options.UnknownSwitches);
        Assert.Equal("input.idl", options.InputFile);
        Assert.Empty(options.Errors);
    }
}

