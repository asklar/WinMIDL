using Midl.Analysis;
using Midl.CodeGen.Proxy;

namespace Midl.Integration.Tests;

/// <summary>
/// Integration tests for ProxyGenerator — compiles IDL source end-to-end
/// and verifies the generated proxy C++ code structure.
/// </summary>
public partial class ProxyGeneratorIntegrationTests
{
    private readonly ProxyGenerator _generator = new();

    private static Compilation Compile(string source) => CompilationHelper.Compile(source);

    [Fact]
    public void EndToEnd_SimpleInterface_GeneratesCompleteProxy()
    {
        var source = @"
            namespace Widgets
            {
                interface IWidget
                {
                    String Name { get; set; };
                    void DoWork(Int32 count);
                    Boolean IsActive();
                };
            }";

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        // Class declaration
        Assert.Contains("class CIWidgetProxy : public IRpcProxyBuffer {", output);

        // IRpcProxyBuffer plumbing
        Assert.Contains("Connect(IRpcChannelBuffer* pChannel)", output);
        Assert.Contains("Disconnect()", output);
        Assert.Contains("QueryInterface(REFIID riid, void** ppv)", output);
        Assert.Contains("AddRef()", output);
        Assert.Contains("Release()", output);

        // IInspectable forwarding
        Assert.Contains("GetIids(", output);
        Assert.Contains("GetRuntimeClassName(", output);
        Assert.Contains("GetTrustLevel(", output);

        // Property getter/setter
        Assert.Contains("get_Name(HSTRING* returnValue)", output);
        Assert.Contains("put_Name(HSTRING name)", output);

        // Method with param
        Assert.Contains("DoWork(INT32 count)", output);
        Assert.Contains("msg.cbBuffer = kDoWork_RequestSize;", output);
        Assert.Contains("IsActive(boolean* returnValue)", output);

        // Vtable slots in order
        Assert.Contains("msg.iMethod = 6;", output); // get_Name
        Assert.Contains("msg.iMethod = 7;", output); // put_Name
        Assert.Contains("msg.iMethod = 8;", output); // DoWork
        Assert.Contains("msg.iMethod = 9;", output); // IsActive
    }

    [Fact]
    public void EndToEnd_InterfaceWithEvents_GeneratesEventProxies()
    {
        var source = @"
            namespace TestApp
            {
                delegate void ClickHandler(Object sender, Int32 args);

                interface IButton
                {
                    event ClickHandler Click;
                    String Label { get; };
                };
            }";

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        // Event add/remove
        Assert.Contains("add_Click(", output);
        Assert.Contains("remove_Click(", output);

        // Property after event
        Assert.Contains("get_Label(HSTRING* returnValue)", output);

        // Vtable ordering: add_Click=6, remove_Click=7, get_Label=8
        Assert.Contains("msg.iMethod = 6;", output);
        Assert.Contains("msg.iMethod = 7;", output);
        Assert.Contains("msg.iMethod = 8;", output);
    }

    [Fact]
    public void EndToEnd_InterfaceWithStruct_SerializesStructFields()
    {
        var source = @"
            namespace TestApp
            {
                struct Point
                {
                    Single X;
                    Single Y;
                };

                interface ICanvas
                {
                    void DrawAt(Point pos);
                };
            }";

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        // Blittable struct uses marshal helper
        Assert.Contains("winmidl::marshal_struct(p, pos);", output);
        Assert.Contains("msg.cbBuffer = kDrawAt_RequestSize;", output);
    }

    [Fact]
    public void EndToEnd_InterfaceWithEnum_SerializesAsInt()
    {
        var source = @"
            namespace TestApp
            {
                enum Color
                {
                    Red,
                    Green,
                    Blue
                };

                interface IPainter
                {
                    void SetColor(Color c);
                };
            }";

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        // Enum serialized via marshal helper
        Assert.Contains("msg.cbBuffer = kSetColor_RequestSize;", output);
        Assert.Contains("winmidl::marshal_enum(p, c);", output);
    }

    [Fact]
    public void EndToEnd_MultipleInterfaces_GeneratesAll()
    {
        var source = @"
            namespace TestApp
            {
                interface IAlpha
                {
                    void DoAlpha();
                };

                interface IBeta
                {
                    void DoBeta();
                };

                interface IGamma
                {
                    void DoGamma(Int32 value);
                };
            }";

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("class CIAlphaProxy", output);
        Assert.Contains("class CIBetaProxy", output);
        Assert.Contains("class CIGammaProxy", output);
    }

    [Fact]
    public void EndToEnd_InterfaceWithOutParams_DeserializesFromReply()
    {
        var source = @"
            namespace TestApp
            {
                interface ICalc
                {
                    void GetDimensions(out Int32 width, out Int32 height);
                };
            }";

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("GetDimensions(INT32* width, INT32* height)", output);
        Assert.Contains("msg.cbBuffer = kGetDimensions_RequestSize;", output); // no in params
        Assert.Contains("winmidl::unmarshal_int32(pReply, *width);", output);
        Assert.Contains("winmidl::unmarshal_int32(pReply, *height);", output);
    }

    [Fact]
    public void EndToEnd_MixedMembers_VtableOrderCorrect()
    {
        var source = @"
            namespace TestApp
            {
                delegate void Handler();

                interface IComplex
                {
                    String Title { get; set; };
                    event Handler Changed;
                    void Process(Int32 value);
                    Double GetResult();
                };
            }";

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        // Expected slot order:
        // get_Title=6, put_Title=7, add_Changed=8, remove_Changed=9, Process=10, GetResult=11
        Assert.Contains("msg.iMethod = 6;", output);
        Assert.Contains("msg.iMethod = 7;", output);
        Assert.Contains("msg.iMethod = 8;", output);
        Assert.Contains("msg.iMethod = 9;", output);
        Assert.Contains("msg.iMethod = 10;", output);
        Assert.Contains("msg.iMethod = 11;", output);
    }

    [Fact]
    public void EndToEnd_DeepNamespace_ManglesCorrectly()
    {
        var source = @"
            namespace Company.Product.Feature
            {
                interface IService
                {
                    void Execute();
                };
            }";

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("__x_Company_Product_Feature_IService", output);
        Assert.Contains("CIServiceProxy", output);
    }

    [Fact]
    public void EndToEnd_GenerateToFile_CreatesFile()
    {
        var source = @"
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                };
            }";

        var compilation = Compile(source);
        var tempDir = Path.Combine(Directory.GetCurrentDirectory(), "test_proxy_output");
        var filePath = Path.Combine(tempDir, "test_proxy.cpp");

        try
        {
            _generator.GenerateToFile(compilation, filePath);
            Assert.True(File.Exists(filePath));
            var content = File.ReadAllText(filePath);
            Assert.Contains("CIWidgetProxy", content);
            Assert.Contains("#include <objidl.h>", content);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void EndToEnd_HstringParameter_SerializesCorrectly()
    {
        var source = @"
            namespace TestApp
            {
                interface IGreeter
                {
                    void Greet(String name);
                };
            }";

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        // HSTRING serialization via marshal helper
        Assert.Contains("winmidl::marshal_hstring(p, name);", output);
    }

    [Fact]
    public void EndToEnd_GuidParameter_Serializes16Bytes()
    {
        var source = @"
            namespace TestApp
            {
                interface IRegistry
                {
                    void Register(Guid id);
                };
            }";

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("msg.cbBuffer = kRegister_RequestSize;", output);
        Assert.Contains("winmidl::marshal_guid(p, id);", output);
    }

    [Fact]
    public void EndToEnd_SkipsExternalInterfaces()
    {
        // External types (from references) should not generate proxy classes
        var source = @"
            namespace TestApp
            {
                interface ILocal
                {
                    void DoLocal();
                };
            }";

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        // Should have proxy for local interface
        Assert.Contains("CILocalProxy", output);
        // Should not have proxy for IInspectable or any system interface
        Assert.DoesNotContain("CIInspectableProxy", output);
    }
}
