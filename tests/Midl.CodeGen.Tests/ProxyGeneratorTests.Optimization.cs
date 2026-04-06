using Midl.Analysis;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for proxy codegen optimizations: constexpr buffer sizes, blittable struct
/// single-memcpy, and variable-size inline computation.
/// </summary>
public partial class ProxyGeneratorTests
{
    // _generator and Compile() are declared in ProxyGeneratorTests.cs

    // ─── Constexpr buffer sizes ───

    [Fact]
    public void Proxy_FixedSizeMethod_EmitsConstexprBufferSize()
    {
        var compilation = Compile(@"
            namespace TestApp {
                struct Point { Single X; Single Y; };
                interface IWidget {
                    HRESULT SetPosition(Point pos);
                };
            }");
        var output = _generator.Generate(compilation);

        // Should emit a static constexpr for the request size
        Assert.Contains("static constexpr UINT32", output);
        Assert.Contains("RequestSize", output);
    }

    [Fact]
    public void Proxy_FixedSizeMethod_UsesConstexprInCbBuffer()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    HRESULT SetValue(Int32 x, Int32 y);
                };
            }");
        var output = _generator.Generate(compilation);

        // msg.cbBuffer should reference the constexpr constant, not inline expression
        Assert.Contains("constexpr", output);
        Assert.Contains("msg.cbBuffer = k", output);
    }

    [Fact]
    public void Proxy_VariableSizeMethod_DoesNotEmitConstexprForRequest()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    HRESULT SetName(String name);
                };
            }");
        var output = _generator.Generate(compilation);

        // Variable-size request should NOT use constexpr for request
        Assert.DoesNotContain("RequestSize", output);
        // But should still compute buffer size with inline expression
        Assert.Contains("winmidl::hstring_len(name)", output);
    }

    [Fact]
    public void Proxy_FixedSizeReply_EmitsConstexprReplySize()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    Int32 GetValue();
                };
            }");
        var output = _generator.Generate(compilation);

        // Reply with fixed-size retval should also have constexpr
        Assert.Contains("static constexpr UINT32", output);
        Assert.Contains("ReplySize", output);
    }

    [Fact]
    public void Proxy_NoParamsMethod_EmitsConstexprZeroRequestSize()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    HRESULT DoWork();
                };
            }");
        var output = _generator.Generate(compilation);

        // Zero-param method should emit constexpr with value 0
        Assert.Contains("static constexpr UINT32", output);
        Assert.Contains("= 0;", output);
    }

    // ─── Blittable struct single-memcpy ───

    [Fact]
    public void Proxy_BlittableStructParam_UsesSingleMemcpy()
    {
        var compilation = Compile(@"
            namespace TestApp {
                struct Point { Single X; Single Y; };
                interface IWidget {
                    HRESULT SetPosition(Point pos);
                };
            }");
        var output = _generator.Generate(compilation);

        // Should use marshal_struct helper for blittable struct
        Assert.Contains("winmidl::marshal_struct(p, pos)", output);
        // Should NOT have field-by-field
        Assert.DoesNotContain("pos.X", output);
        Assert.DoesNotContain("pos.Y", output);
    }

    [Fact]
    public void Proxy_BlittableStructReply_UsesSingleMemcpy()
    {
        var compilation = Compile(@"
            namespace TestApp {
                struct Point { Single X; Single Y; };
                interface IWidget {
                    Point GetPosition();
                };
            }");
        var output = _generator.Generate(compilation);

        // Reply deserialization should also use unmarshal_struct for blittable
        Assert.Contains("winmidl::unmarshal_struct(pReply, *returnValue)", output);
    }
}
