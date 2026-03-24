using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Header;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for the patched IidEmitter that computes GUIDs for all interfaces/delegates,
/// not just those with explicit [uuid] attributes.
/// </summary>
public partial class IidEmitterPatchTests
{
    private readonly Midl.CodeGen.Iid.IidEmitter _emitter = new();

    [Fact]
    public void Emit_InterfaceWithoutUuid_ProducesGuidConstant()
    {
        var source = """
            namespace TestNs
            {
                interface IFoo
                {
                    void Bar();
                }
            }
            """;

        var compilation = CompilationHelper.Compile(source);
        var output = _emitter.Emit(compilation);

        Assert.Contains("IID___x_TestNs_CIFoo", output);
        Assert.Contains("const IID", output);
    }

    [Fact]
    public void Emit_InterfaceWithoutUuid_MatchesHeaderEmitterUuid()
    {
        const string fullName = "TestNs.IFoo";
        var source = """
            namespace TestNs
            {
                interface IFoo
                {
                    void Bar();
                }
            }
            """;

        var compilation = CompilationHelper.Compile(source);
        var output = _emitter.Emit(compilation);

        // The computed UUID should match HeaderEmitter.GenerateUuid
        var expectedUuid = HeaderEmitter.GenerateUuid(fullName);
        var expectedGuid = Guid.Parse(expectedUuid);

        // Verify the GUID data1 appears in the output
        var bytes = expectedGuid.ToByteArray();
        uint data1 = BitConverter.ToUInt32(bytes, 0);
        Assert.Contains($"0x{data1:X8}", output);
    }

    [Fact]
    public void Emit_InterfaceWithExplicitUuid_UsesExplicitValue()
    {
        var source = """
            [uuid("AABBCCDD-1122-3344-5566-778899AABBCC")]
            interface IExplicit
            {
                void DoIt();
            }
            """;

        var compilation = CompilationHelper.Compile(source);
        var output = _emitter.Emit(compilation);

        Assert.Contains("IID_", output);
        Assert.Contains("0xAABBCCDD", output);
    }

    [Fact]
    public void Emit_DelegateWithoutUuid_ProducesGuidConstant()
    {
        var source = """
            namespace TestNs
            {
                delegate void MyHandler(Int32 value);
            }
            """;

        var compilation = CompilationHelper.Compile(source);
        var output = _emitter.Emit(compilation);

        Assert.Contains("IID___x_TestNs_CMyHandler", output);
        Assert.Contains("const IID", output);
    }

    [Fact]
    public void Emit_HasExternCGuard()
    {
        var source = """
            namespace TestNs
            {
                interface IFoo
                {
                    void Bar();
                }
            }
            """;

        var compilation = CompilationHelper.Compile(source);
        var output = _emitter.Emit(compilation);

        Assert.Contains("#ifdef __cplusplus", output);
        Assert.Contains("extern \"C\" {", output);
    }

    [Fact]
    public void Emit_MangledName_UsesDoubleUnderscoreXFormat()
    {
        var source = """
            namespace Deep.Nested.Ns
            {
                interface IDeep
                {
                    void Go();
                }
            }
            """;

        var compilation = CompilationHelper.Compile(source);
        var output = _emitter.Emit(compilation);

        // Mangled name replaces dots with underscores and prefixes __x_
        Assert.Contains("IID___x_Deep_CNested_CNs_CIDeep", output);
    }

    [Fact]
    public void Emit_MixedExplicitAndComputed_EmitsBothCorrectly()
    {
        var source = """
            namespace Mix
            {
                [uuid("11111111-2222-3333-4444-555555555555")]
                interface IExplicit
                {
                    void A();
                }

                interface IComputed
                {
                    void B();
                }
            }
            """;

        var compilation = CompilationHelper.Compile(source);
        var output = _emitter.Emit(compilation);

        // Explicit one uses its UUID
        Assert.Contains("0x11111111", output);
        // Both interfaces should have IID entries
        Assert.Contains("IID_", output);

        // The computed one should be present
        var expectedUuid = HeaderEmitter.GenerateUuid("Mix.IComputed");
        var expectedGuid = Guid.Parse(expectedUuid);
        var bytes = expectedGuid.ToByteArray();
        uint data1 = BitConverter.ToUInt32(bytes, 0);
        Assert.Contains($"0x{data1:X8}", output);
    }
}
