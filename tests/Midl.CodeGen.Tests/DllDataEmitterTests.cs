using Midl.Analysis;
using Midl.CodeGen.DllData;

namespace Midl.CodeGen.Tests;

public class DllDataEmitterTests
{
    private readonly DllDataEmitter _emitter = new();

    [Fact]
    public void EmitDllDataSkeleton()
    {
        var source = """
            interface IFoo
            {
                void Bar();
            }
            """;

        var compilation = CompilationHelper.Compile(source);
        var output = _emitter.Emit(compilation);

        Assert.Contains("dlldata.c", output);
        Assert.Contains("DLLDATA_ROUTINES", output);
        Assert.Contains("REGISTER_PROXY_DLL", output);
        Assert.Contains("PROXY_CLSID_IS", output);
    }

    [Fact]
    public void EmitDllDataIncludes()
    {
        var source = """
            interface IFoo
            {
                void Bar();
            }
            """;

        var compilation = CompilationHelper.Compile(source);
        var output = _emitter.Emit(compilation);

        Assert.Contains("#include <rpcproxy.h>", output);
    }
}
