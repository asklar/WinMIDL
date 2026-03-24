using Midl.Analysis;
using Midl.CodeGen.Header;

namespace Midl.Integration.Tests;

/// <summary>
/// Integration tests verifying that header emission uses correct ABI GUIDs.
/// Tests compile IDL source and check the emitted header contains real midl.exe GUIDs.
/// </summary>
public partial class HeaderParityTests
{
    [Fact]
    public void Parity_BasicClass_GuidMatchesRealMidl()
    {
        var source = File.ReadAllText(Path.Combine(FixturesDir, "parity_basic_class.idl"));
        var compilation = CompilationHelper.Compile(source);
        var errors = compilation.Diagnostics
            .Where(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(errors);

        var header = _emitter.Emit(compilation);

        // IBasicWidget real GUID from midl.exe
        Assert.Contains("55dbe203-ced8-5f0c-b3c1-bc70c944db28", header);
        // IBasicWidgetFactory real GUID from midl.exe
        Assert.Contains("4b447d0d-331b-548f-85b8-bb2cd7a05654", header);
        // Must NOT contain the old name-hashed GUIDs
        Assert.DoesNotContain("47afeb94-9b85-3c52-b7a5-ded74af0bb55", header);
    }

    [Fact]
    public void Parity_Interfaces_GuidMatchesRealMidl()
    {
        var source = File.ReadAllText(Path.Combine(FixturesDir, "parity_interfaces.idl"));
        var compilation = CompilationHelper.Compile(source);
        var errors = compilation.Diagnostics
            .Where(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(errors);

        var header = _emitter.Emit(compilation);

        // Real GUIDs from midl.exe (DataChangedHandler=a767a7e7, IDataSource=6185c161, ICacheable=c3cfecd8)
        Assert.Contains("6185c161-0082-53b2-98ed-9b11e71e123a", header); // IDataSource
        Assert.Contains("c3cfecd8-26e8-5eee-af72-002d2fa0a535", header); // ICacheable
    }

    [Fact]
    public void Parity_EmptyInterface_ExplicitUuidPreserved()
    {
        var source = File.ReadAllText(Path.Combine(FixturesDir, "parity_empty_interface.idl"));
        var compilation = CompilationHelper.Compile(source);
        var errors = compilation.Diagnostics
            .Where(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(errors);

        var header = _emitter.Emit(compilation);

        // These have explicit [uuid(...)] attributes — must be preserved exactly
        Assert.Contains("a1b2c3d4-e5f6-7890-abcd-ef0123456789", header.ToLowerInvariant()); // IMarker
        Assert.Contains("b2c3d4e5-f6a7-8901-bcde-f01234567890", header.ToLowerInvariant()); // ITagged
        Assert.Contains("c3d4e5f6-a7b8-9012-cdef-012345678901", header.ToLowerInvariant()); // IExtendedMarker
    }

    [Fact]
    public void Parity_Delegates_GuidMatchesRealMidl()
    {
        var source = File.ReadAllText(Path.Combine(FixturesDir, "parity_delegates.idl"));
        var compilation = CompilationHelper.Compile(source);
        var errors = compilation.Diagnostics
            .Where(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(errors);

        var header = _emitter.Emit(compilation);

        // Real GUIDs from midl.exe for delegates
        Assert.Contains("65d97cec-4707-5cd8-a342-ad8c102a34bc", header); // SimpleCallback
        Assert.Contains("a196f933-fc8a-5ff8-8917-e5083038b5ff", header); // MessageHandler
    }
}
