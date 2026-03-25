using Midl.Analysis;
using Midl.CodeGen.Header;

namespace Midl.Integration.Tests;

/// <summary>
/// Integration tests verifying C++ namespace wrapping in generated headers.
/// Uses inline IDL to verify end-to-end namespace emission behavior.
/// </summary>
public partial class HeaderParityNamespaceTests
{
    private readonly HeaderEmitter _emitter = new();

    private static Compilation Compile(string source) => CompilationHelper.Compile(source);

    [Fact]
    public void FullFile_InterfaceInNamespaceBlock()
    {
        var compilation = Compile(@"
            namespace TestApp.Parity
            {
                interface IBasicWidget
                {
                    String GetName();
                    void Reset();
                };

                runtimeclass BasicWidget
                {
                };
            }");

        var header = _emitter.Emit(compilation);

        // Forward declarations with FWD_DEFINED guards
        Assert.Contains("#ifndef ____x_TestApp_CParity_CIBasicWidget_FWD_DEFINED__", header);

        // C++ guard
        Assert.Contains("#if defined(__cplusplus) && !defined(CINTERFACE)", header);

        // InterfaceName constant
        Assert.Contains(
            "extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IBasicWidget[] = L\"TestApp.Parity.IBasicWidget\";",
            header);

        // Namespace blocks with short name
        Assert.Contains("namespace TestApp {", header);
        Assert.Contains("namespace Parity {", header);
        Assert.Contains("IBasicWidget : public IInspectable", header);
        Assert.Contains("} /* Parity */", header);
        Assert.Contains("} /* TestApp */", header);

        // Define alias
        Assert.Contains("#define __x_TestApp_CParity_CIBasicWidget TestApp::Parity::IBasicWidget", header);

        // C++ guard closing
        Assert.Contains("#endif // defined(__cplusplus)", header);

        // Runtime class string (inside C++ and C blocks)
        Assert.Contains("RuntimeClass_TestApp_Parity_BasicWidget", header);

        // void param for no-arg methods
        Assert.Contains("Reset(void) = 0;", header);
    }

    [Fact]
    public void MultipleInterfaces_EachInNamespaceBlock()
    {
        var compilation = Compile(@"
            namespace TestApp.Parity
            {
                interface IReader
                {
                    String Read();
                };
                interface IWriter
                {
                    void Write(String data);
                };
            }");

        var header = _emitter.Emit(compilation);

        // Both interfaces should have their own namespace blocks
        Assert.Contains("#define __x_TestApp_CParity_CIReader TestApp::Parity::IReader", header);
        Assert.Contains("#define __x_TestApp_CParity_CIWriter TestApp::Parity::IWriter", header);

        // Both should have InterfaceName constants
        Assert.Contains("InterfaceName_TestApp_Parity_IReader", header);
        Assert.Contains("InterfaceName_TestApp_Parity_IWriter", header);
    }

    [Fact]
    public void DelegateAndInterface_BothWrapped()
    {
        var compilation = Compile(@"
            namespace TestApp.Parity
            {
                delegate void SimpleCallback();

                interface IEventSource
                {
                    event TestApp.Parity.SimpleCallback Completed;
                };

                runtimeclass EventSource
                {
                };
            }");

        var header = _emitter.Emit(compilation);

        // Delegate alias
        Assert.Contains("#define __x_TestApp_CParity_CSimpleCallback TestApp::Parity::SimpleCallback", header);

        // Interface alias
        Assert.Contains("#define __x_TestApp_CParity_CIEventSource TestApp::Parity::IEventSource", header);
    }

    [Fact]
    public void DeepNamespace_AllLevelsEmitted()
    {
        var compilation = Compile(@"
            namespace TestApp.Parity.Level1.Level2.Level3
            {
                interface IDeepWidget
                {
                    String GetFullName();
                };
            }");

        var header = _emitter.Emit(compilation);

        // 5 namespace levels
        Assert.Contains("namespace TestApp {", header);
        Assert.Contains("namespace Parity {", header);
        Assert.Contains("namespace Level1 {", header);
        Assert.Contains("namespace Level2 {", header);
        Assert.Contains("namespace Level3 {", header);

        // Closing comments in reverse order
        Assert.Contains("} /* Level3 */", header);
        Assert.Contains("} /* Level2 */", header);
        Assert.Contains("} /* Level1 */", header);
        Assert.Contains("} /* Parity */", header);
        Assert.Contains("} /* TestApp */", header);

        // Define alias with all namespace parts
        Assert.Contains(
            "#define __x_TestApp_CParity_CLevel1_CLevel2_CLevel3_CIDeepWidget TestApp::Parity::Level1::Level2::Level3::IDeepWidget",
            header);
    }

    [Fact]
    public void OrderVerification_IidBeforeCppGuard_EnumBeforeCppGuard()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                enum Status { Active, Inactive };

                struct Point { Int32 X; Int32 Y; };

                interface IWidget
                {
                    void DoWork();
                };
            }");

        var header = _emitter.Emit(compilation);

        var enumPos = header.IndexOf("enum Status");
        var structPos = header.IndexOf("struct Point");
        var iidPos = header.IndexOf("EXTERN_C const IID");
        var cppGuard = header.IndexOf("#if defined(__cplusplus) && !defined(CINTERFACE)");

        Assert.True(enumPos < cppGuard, "Enum should be before C++ guard");
        Assert.True(structPos < cppGuard, "Struct should be before C++ guard");
        Assert.True(iidPos < cppGuard, "IID decl should be before C++ guard");
    }
}
