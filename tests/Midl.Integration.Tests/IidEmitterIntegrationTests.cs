using Midl.Analysis;
using Midl.Analysis.Symbols;

namespace Midl.Integration.Tests;

/// <summary>
/// Integration tests verifying IidEmitter produces correct _i.c content
/// for full IDL compilations.
/// </summary>
public partial class IidEmitterIntegrationTests
{
    private readonly Midl.CodeGen.Iid.IidEmitter _emitter = new();

    [Fact]
    public void EndToEnd_SimpleInterface_EmitsIidInOutput()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    void Activate();
                }

                runtimeclass Widget
                {
                    Widget();
                }
            }
            """;

        var compilation = CompilationHelper.Compile(source);
        var output = _emitter.Emit(compilation);

        // Should have the extern "C" wrapper
        Assert.Contains("#ifdef __cplusplus", output);
        Assert.Contains("extern \"C\" {", output);

        // Should contain IID for the interface with mangled name
        Assert.Contains("IID___x_TestApp_CIWidget", output);
        Assert.Contains("MIDL_DEFINE_GUID(IID", output);

        // The GUID value should match the ABI computation
        var iface = compilation.GetAllTypes().OfType<InterfaceSymbol>().First(i => i.Name == "IWidget");
        var expectedGuid = AbiGuidComputer.ComputeGuid(iface);
        var bytes = expectedGuid.ToByteArray();
        uint data1 = BitConverter.ToUInt32(bytes, 0);
        Assert.Contains($"0x{data1:X8}", output);
    }

    [Fact]
    public void EndToEnd_DelegateAndInterface_EmitsBothIids()
    {
        var source = """
            namespace TestApp
            {
                delegate void StatusChanged(Int32 code);

                interface INotifier
                {
                    void Notify();
                }
            }
            """;

        var compilation = CompilationHelper.Compile(source);
        var output = _emitter.Emit(compilation);

        Assert.Contains("IID___x_TestApp_CStatusChanged", output);
        Assert.Contains("IID___x_TestApp_CINotifier", output);
    }

    [Fact]
    public void EndToEnd_ExplicitUuid_PreservedInOutput()
    {
        var source = """
            [uuid("DEADBEEF-CAFE-BABE-FACE-0123456789AB")]
            interface ILegacy
            {
                void OldMethod();
            }
            """;

        var compilation = CompilationHelper.Compile(source);
        var output = _emitter.Emit(compilation);

        Assert.Contains("0xDEADBEEF", output);
        Assert.Contains("0xCAFE", output);
        Assert.Contains("0xBABE", output);
    }
}
