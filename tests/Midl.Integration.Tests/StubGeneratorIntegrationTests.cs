using Midl.Analysis;
using Midl.CodeGen.Proxy;
using Xunit;

namespace Midl.Integration.Tests;

/// <summary>
/// Integration tests for StubGenerator — compiles realistic IDL and verifies
/// the generated stub has correct overall structure.
/// </summary>
public class StubGeneratorIntegrationTests
{
    private readonly StubGenerator _generator = new();

    private static Compilation Compile(string source) => CompilationHelper.Compile(source);

    [Fact]
    public void FullInterface_GeneratesCompleteStub()
    {
        var source = """
            namespace Widgets
            {
                enum Priority
                {
                    Low,
                    Medium,
                    High
                };

                struct Point
                {
                    Single X;
                    Single Y;
                };

                delegate void ClickHandler();

                interface IWidget
                {
                    String GetName();
                    void SetPosition(Point pos);
                    Int32 GetPriority();
                    Boolean TryReset(out Int32 errorCode);
                    event ClickHandler Click;
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        // Class definition
        Assert.Contains("class CStub___x_Widgets_IWidget", output);
        Assert.Contains("winmidl::stub_base_t<CStub___x_Widgets_IWidget>", output);

        // IRpcStubBuffer methods
        Assert.Contains("Connect(IUnknown* pUnkServer)", output);
        Assert.Contains("Disconnect()", output);
        Assert.Contains("Invoke(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        Assert.Contains("IsIIDSupported(REFIID riid)", output);
        Assert.Contains("CountRefs()", output);
        Assert.Contains("DebugServerQueryInterface(void** ppv)", output);
        Assert.Contains("DebugServerRelease(void* pv)", output);
        // IUnknown methods now provided by stub_base_t — verify entry table instead
        Assert.Contains("kStubQIEntries", output);
        Assert.Contains("{ IID_IUnknown, 0, false }", output);
        Assert.Contains("{ IID_IRpcStubBuffer, 0, false }", output);

        // Method dispatch — slots 6+
        Assert.Contains("case 6: return Invoke_GetName(pMsg, pChannel);", output);
        Assert.Contains("case 7: return Invoke_SetPosition(pMsg, pChannel);", output);
        Assert.Contains("case 8: return Invoke_GetPriority(pMsg, pChannel);", output);
        Assert.Contains("case 9: return Invoke_TryReset(pMsg, pChannel);", output);
        Assert.Contains("case 10: return Invoke_add_Click(pMsg, pChannel);", output);
        Assert.Contains("case 11: return Invoke_remove_Click(pMsg, pChannel);", output);

        // Per-method handlers exist
        Assert.Contains("Invoke_GetName(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        Assert.Contains("Invoke_SetPosition(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        Assert.Contains("Invoke_GetPriority(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        Assert.Contains("Invoke_TryReset(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        Assert.Contains("Invoke_add_Click(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        Assert.Contains("Invoke_remove_Click(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
    }

    [Fact]
    public void FullInterface_GetName_CorrectHStringSerialization()
    {
        var source = """
            namespace Widgets
            {
                interface IWidget
                {
                    String GetName();
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        // GetName returns HSTRING — no input params
        Assert.Contains("HSTRING returnValue", output);
        Assert.Contains("m_pServer->GetName(&returnValue)", output);
        Assert.Contains("winmidl::marshal_hstring(pOut, returnValue);", output);
        // Reply: [sizeof(UINT32) len][N*sizeof(WCHAR) chars][sizeof(WCHAR) null][sizeof(HRESULT)]
        Assert.Contains("sizeof(UINT32) + returnValue_len * sizeof(WCHAR) + sizeof(WCHAR) + sizeof(HRESULT)", output);
        Assert.Contains("pChannel->GetBuffer(pMsg,", output);
        Assert.Contains("WindowsDeleteString(returnValue)", output);
    }

    [Fact]
    public void FullInterface_SetPosition_StructDeserialization()
    {
        var source = """
            namespace Widgets
            {
                struct Point
                {
                    Single X;
                    Single Y;
                };

                interface IWidget
                {
                    void SetPosition(Point pos);
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        // SetPosition takes struct Point — blittable, uses unmarshal helper
        Assert.Contains("Point pos;", output);
        Assert.Contains("winmidl::unmarshal_struct(p, pos);", output);
        Assert.Contains("m_pServer->SetPosition(pos)", output);
        // Reply: just HRESULT (constexpr)
        Assert.Contains("constexpr", output);
    }

    [Fact]
    public void FullInterface_TryReset_OutParamsAndRetval()
    {
        var source = """
            namespace Widgets
            {
                interface IWidget
                {
                    Boolean TryReset(out Int32 errorCode);
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        // TryReset: no input, out errorCode + boolean retval
        Assert.Contains("errorCode", output);
        Assert.Contains("returnValue", output);
        Assert.Contains("TryReset(", output);
        // Reply: [errorCode][returnValue][HRESULT]
        Assert.Contains("winmidl::marshal_int32(pOut, errorCode);", output);
        Assert.Contains("winmidl::marshal_bool(pOut, returnValue);", output);
        Assert.Contains("winmidl::marshal_int32(pOut, hr);", output);
    }

    [Fact]
    public void MultipleInterfaces_EachGetsOwnStub()
    {
        var source = """
            namespace Widgets
            {
                interface IWidgetA
                {
                    void MethodA();
                }

                interface IWidgetB
                {
                    void MethodB();
                    String GetLabel();
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        // Both interfaces get stubs
        Assert.Contains("class CStub___x_Widgets_IWidgetA", output);
        Assert.Contains("class CStub___x_Widgets_IWidgetB", output);

        // Each has its own dispatch
        Assert.Contains("Invoke_MethodA", output);
        Assert.Contains("Invoke_MethodB", output);
        Assert.Contains("Invoke_GetLabel", output);
    }

    [Fact]
    public void ComplexMethod_MixedParamTypes()
    {
        var source = """
            namespace Widgets
            {
                struct Rect
                {
                    Single X;
                    Single Y;
                    Single Width;
                    Single Height;
                };

                enum Alignment
                {
                    Left,
                    Center,
                    Right
                };

                interface ILayout
                {
                    void Arrange(Rect bounds, Alignment align, String label);
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("Invoke_Arrange(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        // Blittable struct uses unmarshal helper
        Assert.Contains("Rect bounds;", output);
        Assert.Contains("winmidl::unmarshal_struct(p, bounds);", output);
        // Enum deserialization
        Assert.Contains("Alignment align;", output);
        Assert.Contains("winmidl::unmarshal_enum(p, align);", output);
        // HSTRING deserialization
        Assert.Contains("winmidl::unmarshal_hstring_ref(p, label_header);", output);
    }

    [Fact]
    public void GeneratedOutput_ContainsFileHeader()
    {
        var source = """
            namespace TestApp
            {
                interface IFoo
                {
                    void Bar();
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("auto-generated", output);
        Assert.Contains("#include", output);
    }

    [Fact]
    public void NoInterfaces_EmitsEmptyStub()
    {
        var source = """
            namespace TestApp
            {
                enum Color
                {
                    Red,
                    Green,
                    Blue
                };
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        // Should still produce valid output, just no stub classes
        Assert.DoesNotContain("class CStub_", output);
        Assert.Contains("auto-generated", output);
    }
}
