using Midl.Analysis;
using Midl.CodeGen.Header;

namespace Midl.Integration.Tests;

/// <summary>
/// Integration tests verifying that compiled IDL produces headers with
/// correct C-separator mangled names (matching real midl.exe output).
/// </summary>
public partial class HeaderManglingIntegrationTests
{
    private readonly HeaderEmitter _emitter = new();

    [Fact]
    public void BasicClass_Header_Uses_C_Separator_In_MangledNames()
    {
        var fixturesDir = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "fixtures", "valid"));
        var idlPath = Path.Combine(fixturesDir, "parity_basic_class.idl");

        Assert.True(File.Exists(idlPath), $"IDL fixture not found: {idlPath}");

        var source = File.ReadAllText(idlPath);
        var compilation = CompilationHelper.Compile(source);

        var errors = compilation.Diagnostics
            .Where(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(errors);

        var header = _emitter.Emit(compilation);

        // Forward declarations should use _C separator
        Assert.Contains("__x_TestApp_CParity_CIBasicWidget", header);
        Assert.Contains("__x_TestApp_CParity_CIBasicWidgetFactory", header);

        // Old underscore-only mangling must NOT appear
        Assert.DoesNotContain("__x_TestApp_Parity_IBasicWidget", header);
    }

    [Fact]
    public void DeepNamespace_Header_Uses_C_Separator()
    {
        var source = @"
            namespace A.B.C
            {
                interface IFoo
                {
                    void Bar();
                };
            }";

        var compilation = CompilationHelper.Compile(source);
        var header = _emitter.Emit(compilation);

        Assert.Contains("__x_A_CB_CC_CIFoo", header);
        Assert.DoesNotContain("__x_A_B_C_IFoo", header);
    }
}
