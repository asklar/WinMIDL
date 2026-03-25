using Midl.Analysis;
using Midl.CodeGen.Header;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests that method parameter formatting matches real midl.exe output:
/// multi-line params, no space after annotation close, separated pointer star,
/// 'result' for retval, propget/propput annotations, etc.
/// </summary>
public partial class HeaderEmitterTests
{
    // ── Multi-line parameter formatting ──────────────────────────────

    [Fact]
    public void Method_SingleParam_MultiLine()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    String GetName();
                };
            }");

        var header = _emitter.Emit(compilation);

        // Single-param retval method should be multi-line
        Assert.Contains("GetName(\n", header.Replace("\r\n", "\n"));
        Assert.Contains("result\n", header.Replace("\r\n", "\n"));
    }

    [Fact]
    public void Method_MultiParam_EachOnOwnLine()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void Configure(String name, Int32 count, Boolean enabled);
                };
            }");

        var header = _emitter.Emit(compilation);
        var norm = header.Replace("\r\n", "\n");

        // Each param should be on its own line
        Assert.Contains("Configure(\n", norm);
        Assert.Contains("/* [in] */HSTRING name,\n", norm);
        Assert.Contains("/* [in] */int count,\n", norm);
        Assert.Contains("/* [in] */::boolean enabled\n", norm);
    }

    [Fact]
    public void Method_NoParams_VoidOnSameLine()
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

        // No-param methods stay on one line with (void)
        Assert.Contains("Reset(void) = 0;", header);
    }

    [Fact]
    public void Method_ClosingParenOnOwnLine()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork(Int32 count);
                };
            }");

        var header = _emitter.Emit(compilation);
        var norm = header.Replace("\r\n", "\n");

        // Closing ) should be on its own line followed by = 0;
        Assert.Contains(") = 0;\n", norm);
        // And it should NOT be on the same line as the last param
        Assert.DoesNotContain("count) = 0;", header);
    }

    // ── No space after */ in annotations ────────────────────────────

    [Fact]
    public void Method_NoSpaceAfterAnnotationClose()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork(Int32 count);
                    String GetName();
                };
            }");

        var header = _emitter.Emit(compilation);

        // No space between */ and type name
        Assert.Contains("/* [in] */int count", header);
        Assert.Contains("/* [out, retval] */HSTRING", header);
        // Should NOT have space after */
        Assert.DoesNotContain("/* [in] */ int", header);
        Assert.DoesNotContain("/* [out, retval] */ HSTRING", header);
    }

    // ── Pointer star separated ──────────────────────────────────────

    [Fact]
    public void Method_PointerStarSeparatedFromType()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    String GetName();
                    Int32 GetCount();
                };
            }");

        var header = _emitter.Emit(compilation);

        // Star should be separated: "HSTRING * result" not "HSTRING* result"
        Assert.Contains("HSTRING * result", header);
        Assert.Contains("int * result", header);
        Assert.DoesNotContain("HSTRING* result", header);
        Assert.DoesNotContain("int* result", header);
    }

    // ── Retval uses 'result' ────────────────────────────────────────

    [Fact]
    public void Method_RetvalUsesResult()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    String GetName();
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("result", header);
        Assert.DoesNotContain("returnValue", header);
    }

    // ── Property annotations ────────────────────────────────────────

    [Fact]
    public void Property_GetterHasPropgetAnnotation()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    String Name { get; };
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("virtual /* [propget] */ HRESULT STDMETHODCALLTYPE get_Name(", header);
    }

    [Fact]
    public void Property_SetterHasPropputAnnotation()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    String Name { get; set; };
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("virtual /* [propput] */ HRESULT STDMETHODCALLTYPE put_Name(", header);
    }

    [Fact]
    public void Property_GetterMultiLineFormat()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    String Name { get; };
                };
            }");

        var header = _emitter.Emit(compilation);
        var norm = header.Replace("\r\n", "\n");

        // Getter should have multi-line param format
        Assert.Contains("get_Name(\n", norm);
        Assert.Contains("/* [out, retval] */HSTRING * value", header);
    }

    [Fact]
    public void Property_SetterMultiLineFormat()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    String Name { get; set; };
                };
            }");

        var header = _emitter.Emit(compilation);
        var norm = header.Replace("\r\n", "\n");

        // Setter should have multi-line param format
        Assert.Contains("put_Name(\n", norm);
        Assert.Contains("/* [in] */HSTRING value", header);
    }

    // ── Event formatting ────────────────────────────────────────────

    [Fact]
    public void Event_AddRemoveMultiLineFormat()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                delegate void SimpleHandler();
                interface IWidget
                {
                    event SimpleHandler Changed;
                };
            }");

        var header = _emitter.Emit(compilation);
        var norm = header.Replace("\r\n", "\n");

        // add_ method should be multi-line with 2 params
        Assert.Contains("add_Changed(\n", norm);
        Assert.Contains("/* [in] */", header);
        Assert.Contains("/* [out, retval] */EventRegistrationToken * token", header);

        // remove_ method should be multi-line with 1 param
        Assert.Contains("remove_Changed(\n", norm);
        Assert.Contains("/* [in] */EventRegistrationToken token", header);
    }

    // ── Delegate Invoke formatting ──────────────────────────────────

    [Fact]
    public void Delegate_InvokeMultiLineFormat()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                delegate void Callback(String message, Int32 code);
            }");

        var header = _emitter.Emit(compilation);
        var norm = header.Replace("\r\n", "\n");

        // Invoke method should be multi-line
        Assert.Contains("Invoke(\n", norm);
        Assert.Contains("/* [in] */HSTRING message,", header);
        Assert.Contains("/* [in] */int code", header);
    }

    [Fact]
    public void Delegate_NoParamInvoke_VoidOnSameLine()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                delegate void SimpleNotify();
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("Invoke(void) = 0;", header);
    }

    // ── Out param formatting ────────────────────────────────────────

    [Fact]
    public void Method_OutParam_PointerStarSeparated()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void GetInfo(out String name, out Int32 count);
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("/* [out] */HSTRING * name", header);
        Assert.Contains("/* [out] */int * count", header);
    }
}
