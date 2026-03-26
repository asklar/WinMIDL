using Midl.Analysis;
using Midl.CodeGen.Proxy;
using Xunit;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for per-method Invoke handler generation — deserialization/serialization.
/// Partial class to reduce merge conflicts.
/// </summary>
public partial class StubGeneratorTests
{
    [Fact]
    public void InvokeHandler_VoidMethod_NoInputDeserialization()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("Invoke_DoWork(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        // Void no-param method: reply is just HRESULT, constexpr
        Assert.Contains("m_pServer->DoWork()", output);
        Assert.Contains("constexpr", output);
        Assert.Contains("memcpy(pMsg->Buffer, &hr, sizeof(HRESULT))", output);
    }

    [Fact]
    public void InvokeHandler_PrimitiveInputParam_Deserializes()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    void SetPriority(Int32 priority);
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("Invoke_SetPriority(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        // Should deserialize Int32 input
        Assert.Contains("INT32 priority;", output);
        Assert.Contains("winmidl::unmarshal_int32(p, priority)", output);
        Assert.Contains("m_pServer->SetPriority(priority)", output);
    }

    [Fact]
    public void InvokeHandler_HStringReturn_SerializesLengthPrefixed()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    String GetName();
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("Invoke_GetName(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        Assert.Contains("HSTRING returnValue;", output);
        Assert.Contains("m_pServer->GetName(&returnValue)", output);
        Assert.Contains("winmidl::marshal_hstring(pOut, returnValue)", output);
        Assert.Contains("WindowsDeleteString(returnValue)", output);
    }

    [Fact]
    public void InvokeHandler_StructInput_DeserializesFields()
    {
        var source = """
            namespace TestApp
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

        Assert.Contains("Invoke_SetPosition(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        Assert.Contains("Point pos;", output);
        // Blittable struct uses unmarshal_struct helper
        Assert.Contains("winmidl::unmarshal_struct(p, pos)", output);
        Assert.Contains("m_pServer->SetPosition(pos)", output);
    }

    [Fact]
    public void InvokeHandler_OutParam_AllocatedLocallyAndSerialized()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    void TryReset(out Int32 errorCode);
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("Invoke_TryReset(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        Assert.Contains("INT32 errorCode;", output);
        Assert.Contains("m_pServer->TryReset(&errorCode)", output);
        // Out param serialized into reply
        Assert.Contains("winmidl::marshal_int32(pOut, errorCode)", output);
    }

    [Fact]
    public void InvokeHandler_GuidParam_16BytesSerialization()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    void SetId(Guid id);
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("Invoke_SetId(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        Assert.Contains("GUID id;", output);
        Assert.Contains("winmidl::unmarshal_guid(p, id)", output);
    }

    [Fact]
    public void InvokeHandler_EnumParam_4Bytes()
    {
        var source = """
            namespace TestApp
            {
                enum Priority
                {
                    Low,
                    Medium,
                    High
                };

                interface IWidget
                {
                    void SetPriority(Priority level);
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("Invoke_SetPriority(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        Assert.Contains("Priority level;", output);
        Assert.Contains("winmidl::unmarshal_enum(p, level)", output);
    }

    [Fact]
    public void InvokeHandler_BooleanReturn_Serialized()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    Boolean IsActive();
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("Invoke_IsActive(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        Assert.Contains("boolean returnValue;", output);
        Assert.Contains("m_pServer->IsActive(&returnValue)", output);
    }

    [Fact]
    public void InvokeHandler_MultipleInputParams_DeserializedInOrder()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    void Configure(Int32 width, Int32 height, Boolean visible);
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("Invoke_Configure(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        Assert.Contains("INT32 width;", output);
        Assert.Contains("INT32 height;", output);
        Assert.Contains("boolean visible;", output);
        Assert.Contains("m_pServer->Configure(width, height, visible)", output);
    }

    [Fact]
    public void InvokeHandler_MixedInOutParams()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    Boolean TryParse(String input, out Int32 result);
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("Invoke_TryParse(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        // in param: HSTRING deserialized from buffer using unmarshal_hstring_ref
        Assert.Contains("HSTRING_HEADER input_header;", output);
        Assert.Contains("winmidl::unmarshal_hstring_ref(p, input_header)", output);
        // out param: allocated locally
        Assert.Contains("INT32 result;", output);
        // Server call with mixed params
        Assert.Contains("m_pServer->TryParse(input, &result, &returnValue)", output);
    }

    [Fact]
    public void InvokeHandler_HStringInput_Deserialized()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    void SetName(String name);
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("Invoke_SetName(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        Assert.Contains("HSTRING_HEADER name_header;", output);
        // Uses unmarshal_hstring_ref for zero-copy deserialization
        Assert.Contains("winmidl::unmarshal_hstring_ref(p, name_header)", output);
        Assert.Contains("WindowsDeleteString(name)", output);
    }

    [Fact]
    public void InvokeHandler_ReplyFormat_OutParamsThenRetvalThenHresult()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    Boolean TryReset(out Int32 errorCode);
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        // Reply should contain: [errorCode] [returnValue] [HRESULT]
        Assert.Contains("winmidl::marshal_int32(pOut, errorCode)", output);
        Assert.Contains("winmidl::marshal_bool(pOut, returnValue)", output);
        Assert.Contains("winmidl::marshal_int32(pOut, hr)", output);
    }
}
