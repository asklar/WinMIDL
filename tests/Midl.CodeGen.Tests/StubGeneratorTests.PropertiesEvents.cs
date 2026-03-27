using Midl.Analysis;
using Midl.CodeGen.Proxy;
using Xunit;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for property and event stub generation.
/// Partial class to reduce merge conflicts.
/// </summary>
public partial class StubGeneratorTests
{
    [Fact]
    public void Property_Getter_GeneratesGetAccessorStub()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    String Name { get; };
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("case 6:", output); // get_Name at slot 6
        Assert.Contains("Invoke_get_Name(pMsg, pChannel)", output);
        Assert.Contains("Invoke_get_Name(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        Assert.Contains("m_pServer->get_Name(&returnValue)", output);
    }

    [Fact]
    public void Property_GetterSetter_GeneratesBothAccessors()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    String Name { get; set; };
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        // Getter at slot 6, setter at slot 7
        Assert.Contains("case 6:", output);
        Assert.Contains("Invoke_get_Name(pMsg, pChannel)", output);
        Assert.Contains("case 7:", output);
        Assert.Contains("Invoke_put_Name(pMsg, pChannel)", output);

        // put_ handler deserializes HSTRING input
        Assert.Contains("Invoke_put_Name(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
    }

    [Fact]
    public void Event_GeneratesAddRemoveHandlers()
    {
        var source = """
            namespace TestApp
            {
                delegate void ClickHandler();

                interface IWidget
                {
                    event ClickHandler Click;
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        // add_ at slot 6, remove_ at slot 7
        Assert.Contains("case 6:", output);
        Assert.Contains("Invoke_add_Click(pMsg, pChannel)", output);
        Assert.Contains("case 7:", output);
        Assert.Contains("Invoke_remove_Click(pMsg, pChannel)", output);

        // add_ handler: takes delegate (interface pointer — not marshaled as raw pointer in stub)
        Assert.Contains("Invoke_add_Click(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        Assert.Contains("EventRegistrationToken", output);

        // remove_ handler: takes EventRegistrationToken
        Assert.Contains("Invoke_remove_Click(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
    }

    [Fact]
    public void VtableSlots_PropertiesAndMethods_NumberedCorrectly()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    String Name { get; set; };
                    void DoWork();
                    Int32 Priority { get; };
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        // Slots: 0-2 IUnknown, 3-5 IInspectable, then:
        // 6: get_Name, 7: put_Name, 8: DoWork, 9: get_Priority
        Assert.Contains("case 6:", output);
        Assert.Contains("Invoke_get_Name", output);
        Assert.Contains("case 7:", output);
        Assert.Contains("Invoke_put_Name", output);
        Assert.Contains("case 8:", output);
        Assert.Contains("Invoke_DoWork", output);
        Assert.Contains("case 9:", output);
        Assert.Contains("Invoke_get_Priority", output);
    }

    [Fact]
    public void MultipleInterfaces_GeneratesMultipleStubs()
    {
        var source = """
            namespace TestApp
            {
                interface IFirst
                {
                    void MethodA();
                }

                interface ISecond
                {
                    void MethodB();
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("class CStub___x_TestApp_IFirst", output);
        Assert.Contains("class CStub___x_TestApp_ISecond", output);
    }

    [Fact]
    public void ExternalInterface_NotIncludedInStub()
    {
        // Only non-external interfaces should get stubs
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

        // IInspectable and IUnknown are external, should not get stubs
        Assert.DoesNotContain("CStub_IInspectable", output);
        Assert.DoesNotContain("CStub_IUnknown", output);
        Assert.Contains("CStub___x_TestApp_IWidget", output);
    }

    [Fact]
    public void Property_PrimitiveType_GetterSerializesCorrectly()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    Int32 Priority { get; };
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("Invoke_get_Priority(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        Assert.Contains("INT32 returnValue{};", output);
        Assert.Contains("m_pServer->get_Priority(&returnValue)", output);
        Assert.Contains("winmidl::marshal_int32(pOut, returnValue)", output);
    }

    [Fact]
    public void Property_Setter_DeserializesInput()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    Int32 Priority { get; set; };
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("Invoke_put_Priority(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        Assert.Contains("INT32 value;", output);
        Assert.Contains("winmidl::unmarshal_int32(p, value)", output);
        Assert.Contains("m_pServer->put_Priority(value)", output);
    }
}
