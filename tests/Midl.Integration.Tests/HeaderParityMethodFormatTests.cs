using Midl.Analysis;
using Midl.CodeGen.Header;

namespace Midl.Integration.Tests;

/// <summary>
/// Integration tests verifying method parameter formatting matches real midl.exe:
/// multi-line params, no space after annotation close, separated pointer star,
/// 'result' for retval, propget/propput annotations.
/// </summary>
public partial class HeaderParityTests
{
    [Fact]
    public void MethodFormat_MultiLineParams()
    {
        var idlPath = Path.Combine(FixturesDir, "parity_interfaces.idl");
        Assert.True(File.Exists(idlPath), $"IDL fixture not found: {idlPath}");

        var source = File.ReadAllText(idlPath);
        var compilation = CompilationHelper.Compile(source);
        var header = _emitter.Emit(compilation);
        var norm = header.Replace("\r\n", "\n");

        // Methods with params should be multi-line (open paren followed by newline)
        Assert.Matches(@"STDMETHODCALLTYPE \w+\(\n", norm);
        // Closing paren on own line
        Assert.Matches(@"\n\s+\) = 0;", norm);
    }

    [Fact]
    public void MethodFormat_NoSpaceAfterAnnotation()
    {
        var idlPath = Path.Combine(FixturesDir, "parity_interfaces.idl");
        Assert.True(File.Exists(idlPath));

        var source = File.ReadAllText(idlPath);
        var compilation = CompilationHelper.Compile(source);
        var header = _emitter.Emit(compilation);

        // No space after */ in param annotations
        Assert.DoesNotContain("*/ HSTRING", header);
        Assert.DoesNotContain("*/ int", header);
        Assert.DoesNotContain("*/ ::boolean", header);
    }

    [Fact]
    public void MethodFormat_RetvalUsesResult()
    {
        var idlPath = Path.Combine(FixturesDir, "parity_return_types.idl");
        Assert.True(File.Exists(idlPath));

        var winmd = Midl.Lsp.Handlers.WinMdNavigator.FindWindowsWinMd();
        Skip.If(winmd == null, "Windows SDK not available");

        var source = File.ReadAllText(idlPath);
        var compilation = CompilationHelper.Compile(source, new List<string> { winmd! }, CompilationMode.Strict);
        var header = _emitter.Emit(compilation);

        // Retval params should use 'result' not 'returnValue'
        Assert.Contains("* result", header);
        Assert.DoesNotContain("returnValue", header);
    }

    [Fact]
    public void MethodFormat_PointerStarSeparated()
    {
        var idlPath = Path.Combine(FixturesDir, "parity_all_primitives.idl");
        Assert.True(File.Exists(idlPath));

        var source = File.ReadAllText(idlPath);
        var compilation = CompilationHelper.Compile(source);
        var header = _emitter.Emit(compilation);

        // Pointer star should be separated: "Type * name" not "Type* name"
        Assert.Contains("int * result", header);
        Assert.Contains("HSTRING * result", header);
        Assert.DoesNotMatch(@"\w\*\s+result", header);
    }

    [Fact]
    public void MethodFormat_PropgetPropputAnnotations()
    {
        var idlPath = Path.Combine(FixturesDir, "parity_property_patterns.idl");
        Assert.True(File.Exists(idlPath));

        var source = File.ReadAllText(idlPath);
        var compilation = CompilationHelper.Compile(source);
        var header = _emitter.Emit(compilation);

        Assert.Contains("/* [propget] */", header);
        Assert.Contains("/* [propput] */", header);
    }

    [Fact]
    public void MethodFormat_EventMultiLine()
    {
        var idlPath = Path.Combine(FixturesDir, "parity_event_patterns.idl");
        Assert.True(File.Exists(idlPath));

        var winmd = Midl.Lsp.Handlers.WinMdNavigator.FindWindowsWinMd();
        Skip.If(winmd == null, "Windows SDK not available");

        var source = File.ReadAllText(idlPath);
        var compilation = CompilationHelper.Compile(source, new List<string> { winmd! }, CompilationMode.Strict);
        var header = _emitter.Emit(compilation);
        var norm = header.Replace("\r\n", "\n");

        // Event add/remove should be multi-line
        Assert.Matches(@"add_\w+\(\n", norm);
        Assert.Matches(@"remove_\w+\(\n", norm);
        Assert.Contains("EventRegistrationToken * token", header);
    }
}
