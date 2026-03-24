using System.Runtime.Versioning;
using Midl.Analysis;
using Midl.Cli;
using Midl.CodeGen.Header;

namespace Midl.Integration.Tests;

/// <summary>
/// Integration tests verifying header emission for generic WinRT types.
/// Requires Windows SDK with Windows.winmd to resolve generic interfaces.
/// </summary>
[SupportedOSPlatform("windows")]
public partial class HeaderParityGenericTests
{
    private static readonly string FixturesDir = Path.GetFullPath(
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "fixtures", "valid"));

    [SkippableFact]
    public void GenericReturnTypes_EmitMangledPointerNames()
    {
        var sdk = WindowsSdkLocator.Discover();
        Skip.If(sdk?.WindowsWinMdPath == null, "Windows SDK not installed");

        var idlPath = Path.Combine(FixturesDir, "parity_generics.idl");
        Skip.If(!File.Exists(idlPath), "parity_generics.idl fixture not found");

        var source = File.ReadAllText(idlPath);
        var compilation = CompilationHelper.Compile(source,
            new[] { sdk.WindowsWinMdPath! }, CompilationMode.Strict);

        var errors = compilation.Diagnostics
            .Where(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(errors);

        var emitter = new HeaderEmitter();
        var header = emitter.Emit(compilation);

        // IVector<String> → __x_Windows_Foundation_Collections_IVector*
        Assert.Contains("__x_Windows_Foundation_Collections_IVector*", header);
        // IVectorView<Int32> → __x_Windows_Foundation_Collections_IVectorView*
        Assert.Contains("__x_Windows_Foundation_Collections_IVectorView*", header);
        // IMap<String, Object> → __x_Windows_Foundation_Collections_IMap*
        Assert.Contains("__x_Windows_Foundation_Collections_IMap*", header);
        // IAsyncOperation<Boolean> → __x_Windows_Foundation_IAsyncOperation*
        Assert.Contains("__x_Windows_Foundation_IAsyncOperation*", header);
        // IIterable<String> → __x_Windows_Foundation_Collections_IIterable*
        Assert.Contains("__x_Windows_Foundation_Collections_IIterable*", header);
    }

    [SkippableFact]
    public void GenericReturnTypes_MethodSignaturesHaveDoublePointerForOutParams()
    {
        var sdk = WindowsSdkLocator.Discover();
        Skip.If(sdk?.WindowsWinMdPath == null, "Windows SDK not installed");

        var idlPath = Path.Combine(FixturesDir, "parity_generics.idl");
        Skip.If(!File.Exists(idlPath), "parity_generics.idl fixture not found");

        var source = File.ReadAllText(idlPath);
        var compilation = CompilationHelper.Compile(source,
            new[] { sdk.WindowsWinMdPath! }, CompilationMode.Strict);

        var emitter = new HeaderEmitter();
        var header = emitter.Emit(compilation);

        // Out/retval params for interface pointers should be double-pointer
        Assert.Contains("__x_Windows_Foundation_Collections_IVector** returnValue", header);
        Assert.Contains("__x_Windows_Foundation_Collections_IVectorView** returnValue", header);
        Assert.Contains("__x_Windows_Foundation_Collections_IMap** returnValue", header);
        Assert.Contains("__x_Windows_Foundation_IAsyncOperation** returnValue", header);
        Assert.Contains("__x_Windows_Foundation_Collections_IIterable** returnValue", header);
    }
}
