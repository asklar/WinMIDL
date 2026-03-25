using System.Runtime.Versioning;
using Midl.Analysis;
using Midl.Cli;
using Midl.CodeGen.Header;

namespace Midl.Integration.Tests;

/// <summary>
/// Integration tests for parameterized interface instantiation emission.
/// Uses real IDL fixtures compiled with Windows SDK.
/// Partial class to reduce merge conflicts.
/// </summary>
[SupportedOSPlatform("windows")]
public partial class HeaderParityParameterizedTests
{
    private static readonly string FixturesDir = Path.GetFullPath(
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "fixtures", "valid"));

    private static (Compilation compilation, string header)? CompileFixture(string fixtureName)
    {
        var sdk = WindowsSdkLocator.Discover();
        if (sdk?.WindowsWinMdPath == null) return null;

        var idlPath = Path.Combine(FixturesDir, $"{fixtureName}.idl");
        if (!File.Exists(idlPath)) return null;

        var source = File.ReadAllText(idlPath);
        var compilation = CompilationHelper.Compile(source,
            new[] { sdk.WindowsWinMdPath! }, CompilationMode.Strict);

        var errors = compilation.Diagnostics
            .Where(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error)
            .ToList();
        if (errors.Count > 0) return null;

        var emitter = new HeaderEmitter();
        var header = emitter.Emit(compilation);
        return (compilation, header);
    }

    [SkippableFact]
    public void BasicClass_EmitsTypedEventHandlerFIBlock()
    {
        var result = CompileFixture("parity_basic_class");
        Skip.If(result == null, "Windows SDK or fixture not available");

        var header = result.Value.header;

        // Should emit __FI blocks for TypedEventHandler<BasicWidget, Object>
        Assert.Contains("__FITypedEventHandler_2_TestApp__CParity__CBasicWidget_IInspectable", header);
        // Should have DEF guard
        Assert.Contains("DEF___FITypedEventHandler_2_TestApp__CParity__CBasicWidget_IInspectable_USE", header);
    }

    [SkippableFact]
    public void Generics_EmitsIVectorFIBlock()
    {
        var result = CompileFixture("parity_generics");
        Skip.If(result == null, "Windows SDK or fixture not available");

        var header = result.Value.header;

        // Should emit __FI block for IVector<String>
        Assert.Contains("__FIVector_1_HSTRING", header);
        // Should emit __FI block for IVectorView<Int32>
        Assert.Contains("__FIVectorView_1_int", header);
        // Should emit __FI block for IMap<String, Object>
        Assert.Contains("__FIMap_2_HSTRING_IInspectable", header);
        // Should emit __FI block for IAsyncOperation<Boolean>
        Assert.Contains("__FIAsyncOperation_1_boolean", header);
    }

    [SkippableFact]
    public void Generics_FIBlockAppearsInCppSection()
    {
        var result = CompileFixture("parity_generics");
        Skip.If(result == null, "Windows SDK or fixture not available");

        var header = result.Value.header;

        // __FI blocks should appear in the C++ section (before #else for CINTERFACE)
        var cppStart = header.IndexOf("#if defined(__cplusplus)");
        var cppEnd = header.IndexOf("#else // !defined(__cplusplus)");
        Skip.If(cppStart < 0 || cppEnd < 0, "No C++ section found");

        var cppSection = header[cppStart..cppEnd];
        Assert.Contains("DEF___FIVector_1_HSTRING_USE", cppSection);
    }

    [SkippableFact]
    public void Generics_FIBlockAppearsInCSection()
    {
        var result = CompileFixture("parity_generics");
        Skip.If(result == null, "Windows SDK or fixture not available");

        var header = result.Value.header;

        // __FI C blocks should appear in the C section (after #else for CINTERFACE)
        var cStart = header.IndexOf("#else // !defined(__cplusplus)");
        var cEnd = header.IndexOf("#endif // defined(__cplusplus)");
        Skip.If(cStart < 0 || cEnd < 0, "No C section found");

        var cSection = header[cStart..cEnd];
        Assert.Contains("____FIVector_1_HSTRING_INTERFACE_DEFINED__", cSection);
    }

    [SkippableFact]
    public void Delegates_EmitsEventHandlerFIBlock()
    {
        var result = CompileFixture("parity_delegates");
        Skip.If(result == null, "Windows SDK or fixture not available");

        var header = result.Value.header;

        // TypedEventHandler<EventSource, String>
        Assert.Contains("__FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING", header);
    }

    [SkippableFact]
    public void ObservableCollections_EmitsFIBlocks()
    {
        var result = CompileFixture("parity_observable_collections");
        Skip.If(result == null, "Windows SDK or fixture not available");

        var header = result.Value.header;

        // IObservableVector<String>
        Assert.Contains("__FIObservableVector_1_HSTRING", header);
        // IObservableMap<String, Int32>
        Assert.Contains("__FIObservableMap_2_HSTRING_int", header);
    }

    [SkippableFact]
    public void NestedGenerics_EmitsFIBlocksForInnerAndOuter()
    {
        var result = CompileFixture("parity_nested_generics");
        Skip.If(result == null, "Windows SDK or fixture not available");

        var header = result.Value.header;

        // Inner generic: IMap<String, Int32>
        Assert.Contains("__FIMap_2_HSTRING_int", header);
        // Outer generic: IVector<IMap<String, Int32>>
        Assert.Contains("__FIVector_1___FIMap_2_HSTRING_int", header);
    }
}
