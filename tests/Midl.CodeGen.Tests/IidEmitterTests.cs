// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Iid;

namespace Midl.CodeGen.Tests;

public class IidEmitterTests
{
    private readonly IidEmitter _emitter = new();

    [Fact]
    public void EmitInterfaceIid()
    {
        var source = """
            [uuid("12345678-1234-1234-1234-123456789ABC")]
            interface IMyInterface
            {
                void DoSomething();
            }
            """;

        var compilation = CompilationHelper.Compile(source);
        var output = _emitter.Emit(compilation);

        Assert.Contains("IID___x_IMyInterface", output);
        Assert.Contains("0x12345678", output);
        Assert.Contains("0x1234", output);
    }

    [Fact]
    public void EmitMultipleIids()
    {
        var source = """
            [uuid("11111111-1111-1111-1111-111111111111")]
            interface IFirst
            {
                void A();
            }

            [uuid("22222222-2222-2222-2222-222222222222")]
            interface ISecond
            {
                void B();
            }
            """;

        var compilation = CompilationHelper.Compile(source);
        var output = _emitter.Emit(compilation);

        Assert.Contains("IID___x_IFirst", output);
        Assert.Contains("IID___x_ISecond", output);
        Assert.Contains("0x11111111", output);
        Assert.Contains("0x22222222", output);
    }

    [Fact]
    public void EmitCoclassClsid()
    {
        // Use RuntimeClass with uuid as the closest analog to coclass
        var source = """
            [uuid("AABBCCDD-AABB-CCDD-EEFF-001122334455")]
            runtimeclass MyCoclass
            {
            }
            """;

        var compilation = CompilationHelper.Compile(source);
        var output = _emitter.Emit(compilation);

        Assert.Contains("CLSID_MyCoclass", output);
        Assert.Contains("0xAABBCCDD", output);
    }

    [Fact]
    public void EmitExternCGuard()
    {
        var source = """
            [uuid("12345678-1234-1234-1234-123456789ABC")]
            interface IFoo
            {
                void Bar();
            }
            """;

        var compilation = CompilationHelper.Compile(source);
        var output = _emitter.Emit(compilation);

        Assert.Contains("#ifdef __cplusplus", output);
        Assert.Contains("extern \"C\" {", output);
        Assert.Contains("}", output);
    }

    [Fact]
    public void EmitComputedIidForMissingUuid()
    {
        var source = """
            interface INoUuid
            {
                void Something();
            }
            """;

        var compilation = CompilationHelper.Compile(source);
        var output = _emitter.Emit(compilation);

        // Now emits computed GUIDs for all interfaces
        Assert.Contains("IID___x_INoUuid", output);
        Assert.Contains("MIDL_DEFINE_GUID", output);
    }
}
