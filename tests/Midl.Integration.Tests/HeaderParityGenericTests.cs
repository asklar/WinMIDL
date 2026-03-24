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

        // IVector<String> → __x_Windows_CFoundation_CCollections_CIVector*
        Assert.Contains("__x_Windows_CFoundation_CCollections_CIVector*", header);
        // IVectorView<Int32> → __x_Windows_CFoundation_CCollections_CIVectorView*
        Assert.Contains("__x_Windows_CFoundation_CCollections_CIVectorView*", header);
        // IMap<String, Object> → __x_Windows_CFoundation_CCollections_CIMap*
        Assert.Contains("__x_Windows_CFoundation_CCollections_CIMap*", header);
        // IAsyncOperation<Boolean> → __x_Windows_CFoundation_CIAsyncOperation*
        Assert.Contains("__x_Windows_CFoundation_CIAsyncOperation*", header);
        // IIterable<String> → __x_Windows_CFoundation_CCollections_CIIterable*
        Assert.Contains("__x_Windows_CFoundation_CCollections_CIIterable*", header);
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
        Assert.Contains("__x_Windows_CFoundation_CCollections_CIVector** returnValue", header);
        Assert.Contains("__x_Windows_CFoundation_CCollections_CIVectorView** returnValue", header);
        Assert.Contains("__x_Windows_CFoundation_CCollections_CIMap** returnValue", header);
        Assert.Contains("__x_Windows_CFoundation_CIAsyncOperation** returnValue", header);
        Assert.Contains("__x_Windows_CFoundation_CCollections_CIIterable** returnValue", header);
    }
}
