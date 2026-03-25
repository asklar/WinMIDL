using Midl.Analysis;
using Midl.CodeGen.Header;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Unit tests for C compatibility block emission (vtable structs, COBJMACROS).
/// </summary>
public partial class HeaderEmitterCBlockTests
{
    private readonly HeaderEmitter _emitter = new();

    private static Compilation Compile(string source) => CompilationHelper.Compile(source);

    // ── C block structure ──────────────────────────────────────────────

    [Fact]
    public void EmitsCElseBlock_WhenInterfacesExist()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IFoo
                {
                    void DoIt();
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("#else // !defined(__cplusplus)", header);
    }

    [Fact]
    public void NoCBlock_WhenNoInterfacesOrDelegates()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                enum Color { Red, Green };
            }");

        var header = _emitter.Emit(compilation);

        Assert.DoesNotContain("#else // !defined(__cplusplus)", header);
    }

    // ── C forward declarations ─────────────────────────────────────────

    [Fact]
    public void EmitsCForwardDeclarations()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IFoo
                {
                    void DoIt();
                };
            }");

        var header = _emitter.Emit(compilation);

        // In the C block, forward decls use typedef interface
        var cBlock = ExtractCBlock(header);
        Assert.Contains("typedef interface __x_TestApp_CIFoo __x_TestApp_CIFoo;", cBlock);
    }

    [Fact]
    public void CForwardDecl_HasFwdDefinedGuard()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IFoo
                {
                    void DoIt();
                };
            }");

        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        Assert.Contains("#ifndef ____x_TestApp_CIFoo_FWD_DEFINED__", cBlock);
        Assert.Contains("#define ____x_TestApp_CIFoo_FWD_DEFINED__", cBlock);
        Assert.Contains("#endif // ____x_TestApp_CIFoo_FWD_DEFINED__", cBlock);
    }

    // ── Vtable struct ──────────────────────────────────────────────────

    [Fact]
    public void EmitsVtableStruct_WithBeginEndInterface()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IFoo
                {
                    void DoIt();
                };
            }");

        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        Assert.Contains("typedef struct __x_TestApp_CIFooVtbl", cBlock);
        Assert.Contains("BEGIN_INTERFACE", cBlock);
        Assert.Contains("END_INTERFACE", cBlock);
        Assert.Contains("} __x_TestApp_CIFooVtbl;", cBlock);
    }

    [Fact]
    public void VtableHasIInspectableBaseMethods()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IFoo
                {
                    void DoIt();
                };
            }");

        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        Assert.Contains("QueryInterface", cBlock);
        Assert.Contains("AddRef", cBlock);
        Assert.Contains("Release", cBlock);
        Assert.Contains("GetIids", cBlock);
        Assert.Contains("GetRuntimeClassName", cBlock);
        Assert.Contains("GetTrustLevel", cBlock);
    }

    [Fact]
    public void VtableHasUserMethod()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IFoo
                {
                    void DoIt();
                };
            }");

        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        Assert.Contains("STDMETHODCALLTYPE *DoIt", cBlock);
        Assert.Contains("__x_TestApp_CIFoo * This", cBlock);
    }

    [Fact]
    public void VtableMethod_WithParams()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IFoo
                {
                    void SetName(String name, Int32 count);
                };
            }");

        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        Assert.Contains("*SetName", cBlock);
        Assert.Contains("HSTRING name", cBlock);
        Assert.Contains("int count", cBlock);
    }

    [Fact]
    public void VtableMethod_WithReturnValue()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IFoo
                {
                    String GetName();
                };
            }");

        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        Assert.Contains("*GetName", cBlock);
        Assert.Contains("HSTRING* returnValue", cBlock);
    }

    // ── Property annotations ───────────────────────────────────────────

    [Fact]
    public void VtableProperty_HasPropgetAnnotation()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IFoo
                {
                    String Name { get; };
                };
            }");

        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        Assert.Contains("/* [propget] */", cBlock);
        Assert.Contains("*get_Name", cBlock);
    }

    [Fact]
    public void VtableProperty_HasPropputAnnotation()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IFoo
                {
                    String Name { get; set; };
                };
            }");

        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        Assert.Contains("/* [propput] */", cBlock);
        Assert.Contains("*put_Name", cBlock);
    }

    // ── Event annotations ──────────────────────────────────────────────

    [Fact]
    public void VtableEvent_HasEventAddAnnotation()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                delegate void MyHandler();
                runtimeclass Widget
                {
                    event MyHandler Changed;
                };
            }");

        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        Assert.Contains("/* [eventadd] */", cBlock);
        Assert.Contains("*add_Changed", cBlock);
    }

    [Fact]
    public void VtableEvent_HasEventRemoveAnnotation()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                delegate void MyHandler();
                runtimeclass Widget
                {
                    event MyHandler Changed;
                };
            }");

        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        Assert.Contains("/* [eventremove] */", cBlock);
        Assert.Contains("*remove_Changed", cBlock);
    }

    // ── Interface struct ───────────────────────────────────────────────

    [Fact]
    public void EmitsInterfaceStruct_WithLpVtbl()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IFoo
                {
                    void DoIt();
                };
            }");

        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        Assert.Contains("interface __x_TestApp_CIFoo", cBlock);
        Assert.Contains("CONST_VTBL struct __x_TestApp_CIFooVtbl *lpVtbl;", cBlock);
    }

    // ── COBJMACROS ─────────────────────────────────────────────────────

    [Fact]
    public void EmitsCObjMacros_ForBaseMethods()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IFoo
                {
                    void DoIt();
                };
            }");

        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        Assert.Contains("#ifdef COBJMACROS", cBlock);
        Assert.Contains("#endif /* COBJMACROS */", cBlock);
        Assert.Contains("#define __x_TestApp_CIFoo_QueryInterface(This,riid,ppvObject)", cBlock);
        Assert.Contains("#define __x_TestApp_CIFoo_AddRef(This)", cBlock);
        Assert.Contains("#define __x_TestApp_CIFoo_Release(This)", cBlock);
        Assert.Contains("#define __x_TestApp_CIFoo_GetIids(This,iidCount,iids)", cBlock);
        Assert.Contains("#define __x_TestApp_CIFoo_GetRuntimeClassName(This,className)", cBlock);
        Assert.Contains("#define __x_TestApp_CIFoo_GetTrustLevel(This,trustLevel)", cBlock);
    }

    [Fact]
    public void EmitsCObjMacros_ForUserMethods()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IFoo
                {
                    void DoIt();
                    String GetName();
                };
            }");

        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        Assert.Contains("#define __x_TestApp_CIFoo_DoIt(This)", cBlock);
        Assert.Contains("(This)->lpVtbl->DoIt(This)", cBlock);
        Assert.Contains("#define __x_TestApp_CIFoo_GetName(This,returnValue)", cBlock);
        Assert.Contains("(This)->lpVtbl->GetName(This,returnValue)", cBlock);
    }

    [Fact]
    public void CObjMacros_ForProperties()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IFoo
                {
                    String Name { get; set; };
                };
            }");

        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        Assert.Contains("#define __x_TestApp_CIFoo_get_Name(This,value)", cBlock);
        Assert.Contains("#define __x_TestApp_CIFoo_put_Name(This,value)", cBlock);
    }

    [Fact]
    public void CObjMacros_ForEvents()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                delegate void MyHandler();
                runtimeclass Widget
                {
                    event MyHandler Changed;
                };
            }");

        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        Assert.Contains("#define __x_TestApp_CIWidget_add_Changed(This,handler,token)", cBlock);
        Assert.Contains("#define __x_TestApp_CIWidget_remove_Changed(This,token)", cBlock);
    }

    // ── Delegate handling ──────────────────────────────────────────────

    [Fact]
    public void Delegate_HasIUnknownBaseMethods_NotIInspectable()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                delegate void MyHandler(String message);
            }");

        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        // Delegates derive from IUnknown, not IInspectable
        Assert.Contains("*QueryInterface", cBlock);
        Assert.Contains("*AddRef", cBlock);
        Assert.Contains("*Release", cBlock);
        Assert.Contains("*Invoke", cBlock);

        // Should NOT have IInspectable methods
        Assert.DoesNotContain("GetIids", cBlock);
        Assert.DoesNotContain("GetRuntimeClassName", cBlock);
        Assert.DoesNotContain("GetTrustLevel", cBlock);
    }

    [Fact]
    public void Delegate_HasInvokeInVtable()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                delegate void MyHandler(String message);
            }");

        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        Assert.Contains("*Invoke", cBlock);
        Assert.Contains("HSTRING message", cBlock);
    }

    [Fact]
    public void Delegate_HasCObjMacros()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                delegate void MyHandler(String message);
            }");

        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        Assert.Contains("#define __x_TestApp_CMyHandler_QueryInterface(This,riid,ppvObject)", cBlock);
        Assert.Contains("#define __x_TestApp_CMyHandler_AddRef(This)", cBlock);
        Assert.Contains("#define __x_TestApp_CMyHandler_Release(This)", cBlock);
        Assert.Contains("#define __x_TestApp_CMyHandler_Invoke(This,message)", cBlock);
    }

    // ── INTERFACE_DEFINED guard ────────────────────────────────────────

    [Fact]
    public void CInterface_HasInterfaceDefinedGuard()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IFoo
                {
                    void DoIt();
                };
            }");

        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        Assert.Contains("#if !defined(____x_TestApp_CIFoo_INTERFACE_DEFINED__)", cBlock);
        Assert.Contains("#define ____x_TestApp_CIFoo_INTERFACE_DEFINED__", cBlock);
        Assert.Contains("#endif // ____x_TestApp_CIFoo_INTERFACE_DEFINED__", cBlock);
    }

    [Fact]
    public void CInterface_HasInterfaceNameConstant()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IFoo
                {
                    void DoIt();
                };
            }");

        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        Assert.Contains("InterfaceName_TestApp_IFoo", cBlock);
        Assert.Contains("L\"TestApp.IFoo\"", cBlock);
    }

    // ── Empty interface ────────────────────────────────────────────────

    [Fact]
    public void EmptyInterface_HasOnlyBaseMethods()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IEmpty
                {
                };
            }");

        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        Assert.Contains("typedef struct __x_TestApp_CIEmptyVtbl", cBlock);
        Assert.Contains("QueryInterface", cBlock);
        Assert.Contains("GetTrustLevel", cBlock);
        Assert.DoesNotContain("*Invoke", cBlock);
    }

    // ── Method with multiple params ────────────────────────────────────

    [Fact]
    public void CObjMacro_MethodWithMultipleParams()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IFoo
                {
                    void Configure(String name, Int32 count, Boolean enabled);
                };
            }");

        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        Assert.Contains("#define __x_TestApp_CIFoo_Configure(This,name,count,enabled)", cBlock);
        Assert.Contains("(This)->lpVtbl->Configure(This,name,count,enabled)", cBlock);
    }

    // ── Helper ─────────────────────────────────────────────────────────

    /// <summary>
    /// Extracts the C block from a header (between #else and the matching #endif).
    /// </summary>
    private static string ExtractCBlock(string header)
    {
        var elseMarker = "#else // !defined(__cplusplus)";
        var elseIdx = header.IndexOf(elseMarker, StringComparison.Ordinal);
        if (elseIdx < 0) return "";

        var endifMarker = "#endif // defined(__cplusplus) && !defined(CINTERFACE)";
        var endifIdx = header.IndexOf(endifMarker, elseIdx, StringComparison.Ordinal);
        if (endifIdx < 0) return header[elseIdx..];

        return header[elseIdx..endifIdx];
    }
}
