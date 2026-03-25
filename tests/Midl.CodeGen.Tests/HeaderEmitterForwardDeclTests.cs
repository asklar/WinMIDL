using Midl.Analysis;
using Midl.CodeGen.Header;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Unit tests for C++ forward declarations with namespace blocks and FWD_DEFINED guards.
/// </summary>
public partial class HeaderEmitterTests
{
    [Fact]
    public void ForwardDecl_InterfaceHasFwdDefinedGuard()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IGreeter
                {
                    void SayHello();
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("#ifndef ____x_TestApp_CIGreeter_FWD_DEFINED__", header);
        Assert.Contains("#define ____x_TestApp_CIGreeter_FWD_DEFINED__", header);
        Assert.Contains("#endif // ____x_TestApp_CIGreeter_FWD_DEFINED__", header);
    }

    [Fact]
    public void ForwardDecl_InterfaceUsesNamespaceBlock()
    {
        var compilation = Compile(@"
            namespace TestApp.Parity
            {
                interface IWidget
                {
                    void DoWork();
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("namespace TestApp {", header);
        Assert.Contains("    namespace Parity {", header);
        Assert.Contains("        interface IWidget;", header);
        Assert.Contains("    } /* Parity */", header);
        Assert.Contains("} /* TestApp */", header);
    }

    [Fact]
    public void ForwardDecl_InterfaceHasMangledNameAlias()
    {
        var compilation = Compile(@"
            namespace TestApp.Parity
            {
                interface IWidget
                {
                    void DoWork();
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("#define __x_TestApp_CParity_CIWidget TestApp::Parity::IWidget", header);
    }

    [Fact]
    public void ForwardDecl_NoTypedefInterface()
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

        // The C++ forward declarations section should not use typedef interface.
        // (The C block does legitimately use "typedef interface".)
        var cppSection = ExtractCppSection(header);
        Assert.DoesNotContain("typedef interface", cppSection);
    }

    [Fact]
    public void ForwardDecl_SectionHeaderComment()
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

        Assert.Contains("/* Forward Declarations */", header);
        Assert.DoesNotContain("// Forward declarations", header);
    }

    [Fact]
    public void ForwardDecl_RuntimeClassForwardDeclaredAsClass()
    {
        var compilation = Compile(@"
            namespace TestApp.Parity
            {
                runtimeclass Widget
                {
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("namespace TestApp {", header);
        Assert.Contains("class Widget;", header);
        Assert.Contains("} /* Parity */", header);
        Assert.Contains("} /* TestApp */", header);
    }

    [Fact]
    public void ForwardDecl_DeepNamespace()
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
        Assert.Contains("    namespace B {", header);
        Assert.Contains("        namespace C {", header);
        Assert.Contains("            namespace D {", header);
        Assert.Contains("                interface IDeep;", header);
        Assert.Contains("            } /* D */", header);
        Assert.Contains("        } /* C */", header);
        Assert.Contains("    } /* B */", header);
        Assert.Contains("} /* A */", header);
        Assert.Contains("#define __x_A_CB_CC_CD_CIDeep A::B::C::D::IDeep", header);
    }

    [Fact]
    public void ForwardDecl_SingleNamespace()
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
        Assert.Contains("    interface ISimple;", header);
        Assert.Contains("} /* MyApp */", header);
        Assert.Contains("#define __x_MyApp_CISimple MyApp::ISimple", header);
    }

    [Fact]
    public void ForwardDecl_MultipleInterfacesEachGetGuard()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IFirst
                {
                    void A();
                };
                interface ISecond
                {
                    void B();
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("#ifndef ____x_TestApp_CIFirst_FWD_DEFINED__", header);
        Assert.Contains("#endif // ____x_TestApp_CIFirst_FWD_DEFINED__", header);
        Assert.Contains("#ifndef ____x_TestApp_CISecond_FWD_DEFINED__", header);
        Assert.Contains("#endif // ____x_TestApp_CISecond_FWD_DEFINED__", header);
    }

    [Fact]
    public void ForwardDecl_InterfaceAndRuntimeClassBothPresent()
    {
        var compilation = Compile(@"
            namespace MyCompany.MyApp
            {
                interface IWidget
                {
                    String Name { get; };
                };

                runtimeclass Widget
                {
                };
            }");

        var header = _emitter.Emit(compilation);

        // Interface forward declaration with guard
        Assert.Contains("#ifndef ____x_MyCompany_CMyApp_CIWidget_FWD_DEFINED__", header);
        Assert.Contains("interface IWidget;", header);
        Assert.Contains("#define __x_MyCompany_CMyApp_CIWidget MyCompany::MyApp::IWidget", header);

        // Runtime class forward declaration as class
        Assert.Contains("class Widget;", header);

        // No old-style typedef in C++ section
        var cppSection = ExtractCppSection(header);
        Assert.DoesNotContain("typedef interface", cppSection);
    }

    [Fact]
    public void ForwardDecl_NoForwardDeclsWhenOnlyEnumsStructs()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                enum Color { Red, Green, Blue };
                struct Point { Int32 X; Int32 Y; };
            }");

        var header = _emitter.Emit(compilation);

        Assert.DoesNotContain("/* Forward Declarations */", header);
        Assert.DoesNotContain("typedef interface", header);
        Assert.DoesNotContain("FWD_DEFINED", header);
    }

    /// <summary>
    /// Extracts just the C++ section (between #if defined(__cplusplus) and #else).
    /// </summary>
    private static string ExtractCppSection(string header)
    {
        var start = header.IndexOf("#if defined(__cplusplus)", StringComparison.Ordinal);
        if (start < 0) return header;

        var elseMarker = "#else // !defined(__cplusplus)";
        var end = header.IndexOf(elseMarker, start, StringComparison.Ordinal);
        if (end < 0)
        {
            var endifMarker = "#endif // defined(__cplusplus)";
            end = header.IndexOf(endifMarker, start, StringComparison.Ordinal);
        }
        if (end < 0) return header[start..];
        return header[start..end];
    }
}
