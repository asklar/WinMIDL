using System.Security.Cryptography;
using System.Text;
using Midl.Analysis;

namespace Midl.Integration.Tests;

/// <summary>
/// Integration tests verifying IidEmitter produces correct _i.c content
/// for full IDL compilations.
/// </summary>
public partial class IidEmitterIntegrationTests
{
    private readonly Midl.CodeGen.Iid.IidEmitter _emitter = new();

    /// <summary>
    /// Replicates the UUID v5 computation for test verification.
    /// </summary>
    private static Guid ComputeUuid(string fullName)
    {
        var namespaceId = new byte[]
        {
            0x6b, 0xa7, 0xb8, 0x10, 0x9d, 0xad, 0x11, 0xd1,
            0x80, 0xb4, 0x00, 0xc0, 0x4f, 0xd4, 0x30, 0xc8
        };
        var nameBytes = Encoding.UTF8.GetBytes(fullName);
        var input = new byte[namespaceId.Length + nameBytes.Length];
        Array.Copy(namespaceId, 0, input, 0, namespaceId.Length);
        Array.Copy(nameBytes, 0, input, namespaceId.Length, nameBytes.Length);
        var hash = SHA1.HashData(input);
        hash[6] = (byte)((hash[6] & 0x0F) | 0x50);
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80);
        return new Guid(new ReadOnlySpan<byte>(hash, 0, 16));
    }

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
        Assert.Contains("const IID", output);

        // The GUID value should match the deterministic computation
        var expectedGuid = ComputeUuid("TestApp.IWidget");
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
