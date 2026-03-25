using Midl.Analysis;
using Midl.CodeGen.Header;

namespace Midl.Integration.Tests;

/// <summary>
/// Integration tests verifying C++ forward declarations with namespace blocks
/// and FWD_DEFINED guards against compiled IDL fixtures.
/// </summary>
public partial class HeaderParityTests
{
    [Fact]
    public void ForwardDecl_BasicClass_HasNamespaceBlocksAndGuards()
    {
        var source = File.ReadAllText(Path.Combine(FixturesDir, "parity_basic_class.idl"));
        var compilation = CompilationHelper.Compile(source);
        var header = _emitter.Emit(compilation);

        // Interface forward declarations have FWD_DEFINED guards
        Assert.Contains("#ifndef ____x_TestApp_CParity_CIBasicWidget_FWD_DEFINED__", header);
        Assert.Contains("#define ____x_TestApp_CParity_CIBasicWidget_FWD_DEFINED__", header);
        Assert.Contains("#endif // ____x_TestApp_CParity_CIBasicWidget_FWD_DEFINED__", header);

        // Namespace blocks for interface
        Assert.Contains("namespace TestApp {", header);
        Assert.Contains("    namespace Parity {", header);
        Assert.Contains("        interface IBasicWidget;", header);

        // Mangled name alias
        Assert.Contains("#define __x_TestApp_CParity_CIBasicWidget TestApp::Parity::IBasicWidget", header);

        // Runtime class forward declaration as class
        Assert.Contains("        class BasicWidget;", header);

        // No old-style typedef in C++ forward declarations section
        var cppFwdSection = ExtractBeforeCppGuard(header);
        Assert.DoesNotContain("typedef interface", cppFwdSection);
    }

    [Fact]
    public void ForwardDecl_InterfaceOnly_AllInterfacesGetGuards()
    {
        var source = File.ReadAllText(Path.Combine(FixturesDir, "parity_interface_only.idl"));
        var compilation = CompilationHelper.Compile(source);
        var header = _emitter.Emit(compilation);

        Assert.Contains("#ifndef ____x_TestApp_CParity_CInterfaceOnly_CIReader_FWD_DEFINED__", header);
        Assert.Contains("#define __x_TestApp_CParity_CInterfaceOnly_CIReader TestApp::Parity::InterfaceOnly::IReader", header);

        Assert.Contains("#ifndef ____x_TestApp_CParity_CInterfaceOnly_CIWriter_FWD_DEFINED__", header);
        Assert.Contains("#define __x_TestApp_CParity_CInterfaceOnly_CIWriter TestApp::Parity::InterfaceOnly::IWriter", header);

        Assert.Contains("#ifndef ____x_TestApp_CParity_CInterfaceOnly_CIReaderWriter_FWD_DEFINED__", header);
        Assert.Contains("#define __x_TestApp_CParity_CInterfaceOnly_CIReaderWriter TestApp::Parity::InterfaceOnly::IReaderWriter", header);

        var cppFwdSection = ExtractBeforeCppGuard(header);
        Assert.DoesNotContain("typedef interface", cppFwdSection);
    }

    [Fact]
    public void ForwardDecl_DeepNamespace_CorrectNesting()
    {
        var source = File.ReadAllText(Path.Combine(FixturesDir, "parity_deep_namespace.idl"));
        var compilation = CompilationHelper.Compile(source);
        var header = _emitter.Emit(compilation);

        // Deep namespace should produce nested namespace blocks
        Assert.Contains("namespace TestApp {", header);
        Assert.Contains("namespace Parity {", header);
        Assert.Contains("namespace Level1 {", header);
        Assert.Contains("namespace Level2 {", header);
        Assert.Contains("namespace Level3 {", header);
        Assert.Contains("namespace Level4 {", header);
        Assert.Contains("namespace Level5 {", header);
        Assert.Contains("interface IDeepWidget;", header);

        var cppFwdSection = ExtractBeforeCppGuard(header);
        Assert.DoesNotContain("typedef interface", cppFwdSection);
    }

    [Fact]
    public void ForwardDecl_ForwardDeclaration_MultipleClassesAndInterfaces()
    {
        var source = File.ReadAllText(Path.Combine(FixturesDir, "parity_forward_declaration.idl"));
        var compilation = CompilationHelper.Compile(source);
        var header = _emitter.Emit(compilation);

        // All interfaces get guards
        Assert.Contains("____x_TestApp_CParity_CIFwdUser_FWD_DEFINED__", header);
        Assert.Contains("____x_TestApp_CParity_CIFwdTarget_FWD_DEFINED__", header);
        Assert.Contains("____x_TestApp_CParity_CIFwdTargetFactory_FWD_DEFINED__", header);

        // Runtime classes get class forward declarations
        Assert.Contains("class FwdUser;", header);
        Assert.Contains("class FwdTarget;", header);

        // Section header
        Assert.Contains("/* Forward Declarations */", header);
    }

    /// <summary>
    /// Extracts the section of header before the C++ guard (#if defined(__cplusplus)).
    /// This contains the C++ forward declarations with namespace blocks.
    /// </summary>
    private static string ExtractBeforeCppGuard(string header)
    {
        var idx = header.IndexOf("#if defined(__cplusplus)", StringComparison.Ordinal);
        return idx >= 0 ? header[..idx] : header;
    }
}
