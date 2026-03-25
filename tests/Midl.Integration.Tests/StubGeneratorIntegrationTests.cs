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
        Assert.Contains("public IRpcStubBuffer", output);

        // IRpcStubBuffer methods
        Assert.Contains("Connect(IUnknown* pUnkServer)", output);
        Assert.Contains("Disconnect()", output);
        Assert.Contains("Invoke(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        Assert.Contains("IsIIDSupported(REFIID riid)", output);
        Assert.Contains("CountRefs()", output);
        Assert.Contains("DebugServerQueryInterface(void** ppv)", output);
        Assert.Contains("DebugServerRelease(void* pv)", output);
        Assert.Contains("QueryInterface(REFIID riid, void** ppv)", output);
        Assert.Contains("AddRef()", output);
        Assert.Contains("Release()", output);

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
        Assert.Contains("HSTRING returnValue;", output);
        Assert.Contains("m_pServer->GetName(&returnValue)", output);
        Assert.Contains("WindowsGetStringRawBuffer(returnValue, &returnValue_len)", output);
        // Reply: [sizeof(UINT32) len][N*sizeof(WCHAR) chars][sizeof(HRESULT)]
        Assert.Contains("sizeof(UINT32) + returnValue_len * sizeof(WCHAR) + sizeof(HRESULT)", output);
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

        // SetPosition takes struct Point — blittable, uses single memcpy
        Assert.Contains("Point pos;", output);
        Assert.Contains("memcpy(&pos, p, sizeof(Point))", output);
        Assert.Contains("static_assert(std::is_trivially_copyable_v<Point>)", output);
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
        Assert.Contains("INT32 errorCode;", output);
        Assert.Contains("boolean returnValue;", output);
        Assert.Contains("m_pServer->TryReset(&errorCode, &returnValue)", output);
        // Reply: [errorCode][returnValue][HRESULT]
        Assert.Contains("memcpy(pOut, &errorCode, sizeof(INT32))", output);
        Assert.Contains("memcpy(pOut, &returnValue, sizeof(boolean))", output);
        Assert.Contains("memcpy(pOut, &hr, sizeof(HRESULT))", output);
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
        // Blittable struct uses single memcpy
        Assert.Contains("Rect bounds;", output);
        Assert.Contains("memcpy(&bounds, p, sizeof(Rect))", output);
        Assert.Contains("static_assert(std::is_trivially_copyable_v<Rect>)", output);
        // Enum deserialization
        Assert.Contains("Alignment align;", output);
        Assert.Contains("memcpy(&align, p, sizeof(INT32))", output);
        // HSTRING deserialization
        Assert.Contains("HSTRING label = nullptr;", output);
        Assert.Contains("WindowsCreateStringReference(", output);
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
