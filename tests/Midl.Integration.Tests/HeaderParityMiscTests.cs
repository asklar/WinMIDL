using Midl.Analysis;
using Midl.CodeGen.Header;

namespace Midl.Integration.Tests;

/// <summary>
/// Integration tests for RuntimeClass comments, forward decl placement, and boilerplate whitespace.
/// </summary>
public partial class HeaderParityTests
{
    [Fact]
    public void Parity_BasicClass_EmitsRuntimeClassComment()
    {
        var source = File.ReadAllText(Path.Combine(FixturesDir, "parity_basic_class.idl"));
        var compilation = CompilationHelper.Compile(source);
        var errors = compilation.Diagnostics
            .Where(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(errors);

        var header = _emitter.Emit(compilation);

        Assert.Contains("Class TestApp.Parity.BasicWidget", header);
        Assert.Contains("Class implements the following interfaces:", header);
        Assert.Contains("** Default Interface **", header);
        Assert.Contains("Class Threading Model:  Both Single and Multi Threaded Apartment", header);
        Assert.Contains("Class Marshaling Behavior:  Agile - Class is agile", header);
    }

    [Fact]
    public void Parity_BasicClass_HasRuntimeClassDefinedGuard()
    {
        var source = File.ReadAllText(Path.Combine(FixturesDir, "parity_basic_class.idl"));
        var compilation = CompilationHelper.Compile(source);
        var header = _emitter.Emit(compilation);

        Assert.Contains("#ifndef RUNTIMECLASS_TestApp_Parity_BasicWidget_DEFINED", header);
        Assert.Contains("#define RUNTIMECLASS_TestApp_Parity_BasicWidget_DEFINED", header);
    }

    [Fact]
    public void Parity_BasicClass_ForwardDeclsInsideCppGuard()
    {
        var source = File.ReadAllText(Path.Combine(FixturesDir, "parity_basic_class.idl"));
        var compilation = CompilationHelper.Compile(source);
        var header = _emitter.Emit(compilation);

        var fwdIdx = header.IndexOf("/* Forward Declarations */");
        var cppGuard = header.IndexOf("#if defined(__cplusplus) && !defined(CINTERFACE)");
        Assert.True(fwdIdx >= 0, "Forward declarations should be present");
        Assert.True(cppGuard >= 0, "C++ guard should be present");
        Assert.True(fwdIdx > cppGuard, "Forward declarations should appear INSIDE the C++ guard");
    }

    [Fact]
    public void Parity_BasicClass_BlankLineAfterPragmaWarning_MatchesMidl()
    {
        var source = File.ReadAllText(Path.Combine(FixturesDir, "parity_basic_class.idl"));
        var compilation = CompilationHelper.Compile(source);
        var header = _emitter.Emit(compilation);

        var pragmaIdx = header.IndexOf("#pragma warning( disable: 4049 )");
        Assert.True(pragmaIdx >= 0, "Pragma warning should be present");

        var nextLineStart = header.IndexOf('\n', pragmaIdx) + 1;
        var nextLine = header.Substring(nextLineStart, header.IndexOf('\n', nextLineStart) - nextLineStart).Trim();

        // Real midl.exe has a blank line after #pragma warning
        Assert.Equal("", nextLine);
    }

    [Fact]
    public void Parity_Composable_EmitsRuntimeClassComment()
    {
        var source = File.ReadAllText(Path.Combine(FixturesDir, "parity_composable.idl"));
        var compilation = CompilationHelper.Compile(source);
        var errors = compilation.Diagnostics
            .Where(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(errors);

        var header = _emitter.Emit(compilation);

        Assert.Contains("Class TestApp.Parity.ComposableWidget", header);
        Assert.Contains("#ifndef RUNTIMECLASS_TestApp_Parity_ComposableWidget_DEFINED", header);
    }

    [Fact]
    public void Parity_ForwardDeclaration_ForwardDeclsInsideCppGuard()
    {
        var source = File.ReadAllText(Path.Combine(FixturesDir, "parity_forward_declaration.idl"));
        var compilation = CompilationHelper.Compile(source);
        var header = _emitter.Emit(compilation);

        var fwdIdx = header.IndexOf("/* Forward Declarations */");
        var cppGuard = header.IndexOf("#if defined(__cplusplus) && !defined(CINTERFACE)");
        Assert.True(fwdIdx >= 0, "Forward declarations should be present");
        Assert.True(cppGuard >= 0, "C++ guard should be present");
        Assert.True(fwdIdx > cppGuard, "Forward declarations should appear INSIDE the C++ guard");
    }
}
