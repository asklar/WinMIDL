using Midl.Analysis;
using Midl.CodeGen.Header;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for header section ordering and blank lines to match real midl.exe output.
/// Uses partial class pattern to avoid merge conflicts.
/// </summary>
public partial class HeaderEmitterSectionOrderingTests
{
    private readonly HeaderEmitter _emitter = new();

    private static Compilation Compile(string source) => CompilationHelper.Compile(source);

    private static string InterfaceWithClassIdl => @"
        namespace TestApp
        {
            interface IWidget
            {
                void DoWork();
            };

            runtimeclass Widget
            {
                Widget();
            };
        }";

    private static string SimpleIdl => @"
        namespace TestApp
        {
            enum Color { Red, Green, Blue };
        }";

    // ───────────────────────────────────────────────
    // Boilerplate whitespace tests
    // ───────────────────────────────────────────────

    [Fact]
    public void Boilerplate_BlankLineAfterCompilerComment_BeforePragmaWarning()
    {
        var header = _emitter.Emit(Compile(SimpleIdl));
        // There should be a blank line between the compiler comment close and #pragma warning
        Assert.Contains(" */\r\n\r\n#pragma warning", header);
    }

    [Fact]
    public void Boilerplate_BlankLineAfterPragmaWarning()
    {
        var header = _emitter.Emit(Compile(SimpleIdl));
        // There should be a blank line after #pragma warning before the first version check
        Assert.Contains("#pragma warning( disable: 4049 )  /* more than 64k source lines */\r\n\r\n/* verify", header);
    }

    [Fact]
    public void Boilerplate_NoBlankLineBetweenComNoWindowsH_And_IncludeGuards()
    {
        var header = _emitter.Emit(Compile(SimpleIdl));
        // Real midl: no blank line between COM_NO_WINDOWS_H close and include guards
        Assert.Contains("#endif /*COM_NO_WINDOWS_H*/\r\n#ifndef __TestApp_h__", header);
    }

    [Fact]
    public void Boilerplate_TwoBlankLinesBeforePragmaOnce()
    {
        var header = _emitter.Emit(Compile(SimpleIdl));
        // Real midl: two blank lines between #define p_h__ and #pragma once
        Assert.Contains("#define __TestApp_p_h__\r\n\r\n\r\n#pragma once", header);
    }

    [Fact]
    public void Boilerplate_TwoBlankLinesBeforePushMacro()
    {
        var header = _emitter.Emit(Compile(SimpleIdl));
        // Real midl: two blank lines between NS prefix close and MIDL_CONST_ID push
        Assert.Contains("#endif // !defined(DISABLE_NS_PREFIX_CHECKS)\r\n\r\n\r\n#pragma push_macro", header);
    }

    [Fact]
    public void Boilerplate_TwoBlankLinesBeforeHeaderFilesComment()
    {
        var header = _emitter.Emit(Compile(SimpleIdl));
        // Real midl: two blank lines between MIDL_CONST_ID define and header files comment
        Assert.Contains("#define MIDL_CONST_ID const __declspec(selectany)\r\n\r\n\r\n// Header files", header);
    }

    // ───────────────────────────────────────────────
    // C++ section ordering tests
    // ───────────────────────────────────────────────

    [Fact]
    public void CppBlock_ForwardDecls_BeforeParameterizedSpecializations()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    event Windows.Foundation.TypedEventHandler<Widget, Object> Changed;
                };

                runtimeclass Widget
                {
                    Widget();
                };
            }");

        var header = _emitter.Emit(compilation);

        var fwdDeclPos = header.IndexOf("FWD_DEFINED__");
        var paramDefPos = header.IndexOf("DEF___FI");

        Assert.True(fwdDeclPos >= 0, "Forward declarations should be present");
        Assert.True(paramDefPos >= 0, "Parameterized interface definitions should be present");
        Assert.True(fwdDeclPos < paramDefPos,
            "Forward declarations should appear BEFORE parameterized interface template specializations");
    }

    [Fact]
    public void CppBlock_SectionComments_AfterForwardDecls()
    {
        var compilation = Compile(InterfaceWithClassIdl);
        var header = _emitter.Emit(compilation);

        // These section comment headers should appear in the C++ block
        Assert.Contains("// Parameterized interface forward declarations (C++)", header);
        Assert.Contains("// Collection interface definitions", header);
    }

    [Fact]
    public void CppBlock_ClassForwardDecl_AfterSectionComments()
    {
        var compilation = Compile(InterfaceWithClassIdl);
        var header = _emitter.Emit(compilation);

        var cppGuard = header.IndexOf("#if defined(__cplusplus)");
        var collectionComment = header.IndexOf("// Collection interface definitions", cppGuard);
        var classForward = header.IndexOf("class Widget;", collectionComment > 0 ? collectionComment : 0);

        Assert.True(collectionComment >= 0, "Collection interface definitions comment should be present");
        Assert.True(classForward >= 0, "Class forward declaration should be present");
        Assert.True(classForward > collectionComment,
            "Class forward declaration should appear after collection interface definitions comment");
    }

    [Fact]
    public void CppBlock_RuntimeClass_InsideCppGuard()
    {
        var compilation = Compile(InterfaceWithClassIdl);
        var header = _emitter.Emit(compilation);

        var cppGuardStart = header.IndexOf("#if defined(__cplusplus)");
        var cppGuardEnd = header.IndexOf("#else", cppGuardStart);
        var runtimeClassDef = header.IndexOf("RUNTIMECLASS_TestApp_Widget_DEFINED");

        Assert.True(runtimeClassDef >= 0, "RuntimeClass definition should be present");
        Assert.True(runtimeClassDef > cppGuardStart && runtimeClassDef < cppGuardEnd,
            "RuntimeClass should be inside C++ guard, not before it");
    }

    [Fact]
    public void CppBlock_RuntimeClass_AfterInterfaceDefinitions()
    {
        var compilation = Compile(InterfaceWithClassIdl);
        var header = _emitter.Emit(compilation);

        var cppGuardStart = header.IndexOf("#if defined(__cplusplus)");
        var ifaceDef = header.IndexOf("INTERFACE_DEFINED__", cppGuardStart);
        var cppElse = header.IndexOf("#else", cppGuardStart);

        // Find RuntimeClass between the interface def and #else
        var runtimeClassInCpp = header.IndexOf("RUNTIMECLASS_TestApp_Widget_DEFINED", cppGuardStart);

        Assert.True(runtimeClassInCpp >= 0, "RuntimeClass should be in C++ block");
        Assert.True(runtimeClassInCpp > ifaceDef, "RuntimeClass should appear after interface definitions");
        Assert.True(runtimeClassInCpp < cppElse, "RuntimeClass should appear before #else");
    }

    [Fact]
    public void CppBlock_NoExternCIID_BeforeCppGuard()
    {
        var compilation = Compile(InterfaceWithClassIdl);
        var header = _emitter.Emit(compilation);

        var cppGuardStart = header.IndexOf("#if defined(__cplusplus)");
        var beforeCppGuard = header.Substring(0, cppGuardStart);

        // EXTERN_C IID declarations should NOT appear before the C++ guard
        Assert.DoesNotContain("EXTERN_C const IID IID_", beforeCppGuard);
    }

    // ───────────────────────────────────────────────
    // C block ordering tests
    // ───────────────────────────────────────────────

    [Fact]
    public void CBlock_RuntimeClass_InsideCBlock()
    {
        var compilation = Compile(InterfaceWithClassIdl);
        var header = _emitter.Emit(compilation);

        var elsePos = header.IndexOf("#else");
        var endifPos = header.IndexOf("#endif // defined(__cplusplus)");

        // RuntimeClass should also appear in C block
        var runtimeClassInC = header.IndexOf("RUNTIMECLASS_TestApp_Widget_DEFINED", elsePos);

        Assert.True(runtimeClassInC >= 0, "RuntimeClass should be in C block");
        Assert.True(runtimeClassInC > elsePos && runtimeClassInC < endifPos,
            "RuntimeClass should appear between #else and #endif");
    }

    [Fact]
    public void CBlock_SectionComments_Present()
    {
        var compilation = Compile(InterfaceWithClassIdl);
        var header = _emitter.Emit(compilation);

        var elsePos = header.IndexOf("#else");

        // Section comments should appear in C block too
        var paramComment = header.IndexOf("// Parameterized interface forward declarations (C)", elsePos);
        var collComment = header.IndexOf("// Collection interface definitions", elsePos);

        Assert.True(paramComment >= 0, "Parameterized interface comment should be in C block");
        Assert.True(collComment >= 0, "Collection interface definitions comment should be in C block");
    }

    [Fact]
    public void CBlock_RuntimeClass_AfterInterfaceVtables()
    {
        var compilation = Compile(InterfaceWithClassIdl);
        var header = _emitter.Emit(compilation);

        var elsePos = header.IndexOf("#else");
        var endifPos = header.IndexOf("#endif // defined(__cplusplus)");

        var cIfaceDef = header.IndexOf("INTERFACE_DEFINED__", elsePos);
        var runtimeClassInC = header.IndexOf("RUNTIMECLASS_TestApp_Widget_DEFINED", elsePos);

        Assert.True(cIfaceDef >= 0, "C interface definition should be present");
        Assert.True(runtimeClassInC >= 0, "RuntimeClass should be in C block");
        Assert.True(runtimeClassInC > cIfaceDef,
            "RuntimeClass should appear after C interface vtable blocks");
    }

    // ───────────────────────────────────────────────
    // Footer ordering
    // ───────────────────────────────────────────────

    [Fact]
    public void Footer_CppEndif_BeforePopMacro()
    {
        var compilation = Compile(InterfaceWithClassIdl);
        var header = _emitter.Emit(compilation);

        var cppEndif = header.IndexOf("#endif // defined(__cplusplus)");
        var popMacro = header.IndexOf("#pragma pop_macro");

        Assert.True(cppEndif >= 0, "#endif for C++ should be present");
        Assert.True(popMacro >= 0, "#pragma pop_macro should be present");
        Assert.True(cppEndif < popMacro,
            "#endif for C++ guard should come before #pragma pop_macro");
    }
}
