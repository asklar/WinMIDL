using Midl.Analysis;
using Midl.CodeGen.Header;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Unit tests for C++ namespace wrapping in header emission.
/// </summary>
public partial class HeaderEmitterNamespaceTests
{
    private readonly HeaderEmitter _emitter = new();

    private static Compilation Compile(string source) => CompilationHelper.Compile(source);

    [Fact]
    public void Interface_WrappedInCppGuard()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("#if defined(__cplusplus) && !defined(CINTERFACE)", header);
        Assert.Contains("#endif // defined(__cplusplus) && !defined(CINTERFACE)", header);
    }

    [Fact]
    public void Interface_UsesShortNameInsideNamespace()
    {
        var compilation = Compile(@"
            namespace TestApp.Parity
            {
                interface IReader
                {
                    String Read();
                };
            }");

        var header = _emitter.Emit(compilation);

        // Inside namespace block, short name should be used
        Assert.Contains("IReader : public IInspectable", header);
        // The mangled name should NOT appear as the class declaration name inside namespace
        Assert.DoesNotContain("__x_TestApp_CParity_CIReader : public IInspectable", header);
    }

    [Fact]
    public void Interface_WrappedInNamespaceBlocks()
    {
        var compilation = Compile(@"
            namespace TestApp.Parity
            {
                interface IReader
                {
                    String Read();
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("namespace TestApp {", header);
        Assert.Contains("namespace Parity {", header);
        Assert.Contains("} /* Parity */", header);
        Assert.Contains("} /* TestApp */", header);
    }

    [Fact]
    public void Interface_EmitsDefineAlias()
    {
        var compilation = Compile(@"
            namespace TestApp.Parity
            {
                interface IReader
                {
                    String Read();
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("#define __x_TestApp_CParity_CIReader TestApp::Parity::IReader", header);
    }

    [Fact]
    public void Interface_EmitsInterfaceNameConstant()
    {
        var compilation = Compile(@"
            namespace TestApp.Parity
            {
                interface IReader
                {
                    String Read();
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains(
            "extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IReader[] = L\"TestApp.Parity.IReader\";",
            header);
    }

    [Fact]
    public void Interface_IidDeclarationOutsideNamespace()
    {
        var compilation = Compile(@"
            namespace TestApp.Parity
            {
                interface IReader
                {
                    String Read();
                };
            }");

        var header = _emitter.Emit(compilation);

        // EXTERN_C IID should be outside the C++ guard
        var cppGuardStart = header.IndexOf("#if defined(__cplusplus) && !defined(CINTERFACE)");
        var iidDecl = header.IndexOf("EXTERN_C const IID IID___x_TestApp_CParity_CIReader;");

        Assert.True(iidDecl >= 0, "IID declaration should be present");
        Assert.True(cppGuardStart >= 0, "C++ guard should be present");
        Assert.True(iidDecl < cppGuardStart, "IID declaration should be before C++ guard");
    }

    [Fact]
    public void Delegate_WrappedInNamespaceBlocks()
    {
        var compilation = Compile(@"
            namespace TestApp.Parity
            {
                delegate void SimpleCallback();
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("namespace TestApp {", header);
        Assert.Contains("namespace Parity {", header);
        // Short name
        Assert.Contains("SimpleCallback : public IUnknown", header);
        Assert.DoesNotContain("__x_TestApp_CParity_CSimpleCallback : public IUnknown", header);
    }

    [Fact]
    public void Delegate_EmitsDefineAlias()
    {
        var compilation = Compile(@"
            namespace TestApp.Parity
            {
                delegate void SimpleCallback();
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("#define __x_TestApp_CParity_CSimpleCallback TestApp::Parity::SimpleCallback", header);
    }

    [Fact]
    public void DeepNamespace_NestedBlocks()
    {
        var compilation = Compile(@"
            namespace A.B.C.D
            {
                interface IDeep
                {
                    void Go();
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("namespace A {", header);
        Assert.Contains("namespace B {", header);
        Assert.Contains("namespace C {", header);
        Assert.Contains("namespace D {", header);
        Assert.Contains("} /* D */", header);
        Assert.Contains("} /* C */", header);
        Assert.Contains("} /* B */", header);
        Assert.Contains("} /* A */", header);
        Assert.Contains("#define __x_A_CB_CC_CD_CIDeep A::B::C::D::IDeep", header);
    }

    [Fact]
    public void SingleNamespace_OneBlock()
    {
        var compilation = Compile(@"
            namespace MyApp
            {
                interface ISimple
                {
                    void Run();
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("namespace MyApp {", header);
        Assert.Contains("} /* MyApp */", header);
        Assert.Contains("#define __x_MyApp_CISimple MyApp::ISimple", header);
    }

    [Fact]
    public void Enum_NotWrappedInCppBlock()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                enum Color { Red, Green, Blue };
            }");

        var header = _emitter.Emit(compilation);

        // Enums should NOT be inside #if defined(__cplusplus)
        Assert.Contains("enum Color", header);
        // If there are no interfaces, there should be no C++ guard
        Assert.DoesNotContain("#if defined(__cplusplus)", header);
    }

    [Fact]
    public void Struct_NotWrappedInCppBlock()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                struct Point { Int32 X; Int32 Y; };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("struct Point", header);
        Assert.DoesNotContain("#if defined(__cplusplus)", header);
    }

    [Fact]
    public void MixedTypes_InterfaceInCppBlock_EnumOutside()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                enum Status { Active, Inactive };
                interface IWidget
                {
                    void DoWork();
                };
            }");

        var header = _emitter.Emit(compilation);

        // Enum should be before the C++ block
        var enumPos = header.IndexOf("enum Status");
        var cppGuardStart = header.IndexOf("#if defined(__cplusplus) && !defined(CINTERFACE)");

        Assert.True(enumPos >= 0 && cppGuardStart >= 0);
        Assert.True(enumPos < cppGuardStart, "Enum should be before C++ guard");
    }

    [Fact]
    public void NoParamMethod_EmitsVoid()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void Reset();
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("Reset(void) = 0;", header);
    }

    [Fact]
    public void ForwardDeclarations_StillEmitted()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("#ifndef ____x_TestApp_CIWidget_FWD_DEFINED__", header);
        Assert.Contains("#define ____x_TestApp_CIWidget_FWD_DEFINED__", header);
        Assert.Contains("#endif // ____x_TestApp_CIWidget_FWD_DEFINED__", header);
    }
}
