using Midl.Analysis;
using Midl.CodeGen.Header;

namespace Midl.Integration.Tests;

/// <summary>
/// Integration tests verifying that compiled IDL fixtures emit C types
/// matching real midl.exe output.
/// </summary>
public partial class HeaderParityTypeMapTests
{
    private static readonly string FixturesDir = Path.GetFullPath(
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "fixtures", "valid"));

    private readonly HeaderEmitter _emitter = new();

    [Fact]
    public void AllPrimitives_EmitsLowercaseCTypes()
    {
        var idlPath = Path.Combine(FixturesDir, "parity_all_primitives.idl");
        Assert.True(File.Exists(idlPath), $"IDL fixture not found: {idlPath}");

        var source = File.ReadAllText(idlPath);
        var compilation = CompilationHelper.Compile(source);

        var errors = compilation.Diagnostics
            .Where(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(errors);

        var header = _emitter.Emit(compilation);

        // Return types (out params) — must use real midl.exe types
        Assert.Contains("::byte * result", header);
        Assert.Contains("short * result", header);
        Assert.Contains("unsigned short * result", header);
        Assert.Contains("int * result", header);
        Assert.Contains("unsigned int * result", header);
        Assert.Contains("__int64 * result", header);
        Assert.Contains("unsigned __int64 * result", header);
        Assert.Contains("float * result", header);
        Assert.Contains("double * result", header);
        Assert.Contains("::boolean * result", header);
        Assert.Contains("wchar_t * result", header);

        // Input params — must use real midl.exe types
        Assert.Contains("::byte v", header);
        Assert.Contains("short v", header);
        Assert.Contains("unsigned short v", header);
        Assert.Contains("int v", header);
        Assert.Contains("unsigned int v", header);
        Assert.Contains("__int64 v", header);
        Assert.Contains("unsigned __int64 v", header);
        Assert.Contains("float v", header);
        Assert.Contains("double v", header);
        Assert.Contains("::boolean v", header);
        Assert.Contains("wchar_t v", header);

        // Types that stay the same
        Assert.Contains("HSTRING * result", header);
        Assert.Contains("GUID * result", header);
        Assert.Contains("IInspectable* * result", header);
        Assert.Contains("HSTRING v", header);
        Assert.Contains("GUID v", header);

        // Must NOT contain old uppercase types for primitives in parameter/field positions
        // Note: "WCHAR" still appears in RuntimeClass activation string declarations
        Assert.DoesNotContain("INT16 ", header);
        Assert.DoesNotContain("INT32 ", header);
        Assert.DoesNotContain("UINT16 ", header);
        Assert.DoesNotContain("UINT32 ", header);
        Assert.DoesNotContain("FLOAT ", header);
        Assert.DoesNotContain("DOUBLE ", header);
    }

    [Fact]
    public void StructsFixture_UsesCorrectTypes()
    {
        var idlPath = Path.Combine(FixturesDir, "parity_structs.idl");
        if (!File.Exists(idlPath)) return;

        var source = File.ReadAllText(idlPath);
        var compilation = CompilationHelper.Compile(source);
        var header = _emitter.Emit(compilation);

        // Structs should use lowercase C types, not Windows typedefs
        // Check that no uppercase primitive types leak through
        Assert.DoesNotContain("INT32 ", header);
        Assert.DoesNotContain("FLOAT ", header);
        Assert.DoesNotContain("DOUBLE ", header);
    }
}
