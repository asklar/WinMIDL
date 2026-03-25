using Midl.Analysis;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for stub codegen optimizations: zero-copy HSTRING deserialization,
/// blittable struct single-memcpy, and struct layout assertions.
/// </summary>
public partial class StubGeneratorTests
{
    // _generator and Compile() are declared in StubGeneratorTests.cs

    // ─── Zero-copy HSTRING ───

    [Fact]
    public void Stub_HStringInput_UsesWindowsCreateString()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    HRESULT SetName(String name);
                };
            }");
        var output = _generator.Generate(compilation);

        // Stub input deserialization uses WindowsCreateStringReference (zero-copy)
        Assert.Contains("WindowsCreateStringReference(", output);
        Assert.Contains("HSTRING name = nullptr;", output);
    }

    [Fact]
    public void Stub_HStringInput_NeedsWindowsDeleteString()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    HRESULT SetName(String name);
                };
            }");
        var output = _generator.Generate(compilation);

        // WindowsCreateString creates an owning HSTRING,
        // so WindowsDeleteString is needed for input params
        Assert.Contains("WindowsDeleteString(name)", output);
    }

    [Fact]
    public void Stub_HStringReturn_StillUsesWindowsDeleteString()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    String GetName();
                };
            }");
        var output = _generator.Generate(compilation);

        // Return values still need cleanup
        Assert.Contains("WindowsDeleteString(returnValue)", output);
    }

    // ─── Blittable struct ───

    [Fact]
    public void Stub_BlittableStructInput_UsesSingleMemcpy()
    {
        var compilation = Compile(@"
            namespace TestApp {
                struct Point { Single X; Single Y; };
                interface IWidget {
                    HRESULT SetPosition(Point pos);
                };
            }");
        var output = _generator.Generate(compilation);

        // Should use single memcpy for blittable struct deserialization
        Assert.Contains("memcpy(&pos, p, sizeof(Point))", output);
        Assert.Contains("static_assert(std::is_trivially_copyable_v<Point>)", output);
        // Should NOT have field-by-field
        Assert.DoesNotContain("pos.X", output);
        Assert.DoesNotContain("pos.Y", output);
    }

    [Fact]
    public void Stub_BlittableStructReply_UsesSingleMemcpy()
    {
        var compilation = Compile(@"
            namespace TestApp {
                struct Point { Single X; Single Y; };
                interface IWidget {
                    Point GetPosition();
                };
            }");
        var output = _generator.Generate(compilation);

        // Reply serialization should also use single memcpy for blittable
        Assert.Contains("memcpy(pOut, &returnValue, sizeof(Point))", output);
    }

    // ─── Struct layout assertions ───

    [Fact]
    public void Stub_BlittableStruct_EmitsStaticAsserts()
    {
        var compilation = Compile(@"
            namespace TestApp {
                struct Point { Single X; Single Y; };
                interface IWidget {
                    HRESULT SetPosition(Point pos);
                };
            }");
        var output = _generator.Generate(compilation);

        Assert.Contains("static_assert(offsetof(Point, X) == 0)", output);
        Assert.Contains("static_assert(offsetof(Point, Y) == 4)", output);
        Assert.Contains("static_assert(sizeof(Point) == 8)", output);
    }

    // ─── Constexpr in stubs ───

    [Fact]
    public void Stub_FixedSizeReply_EmitsConstexprReplySize()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    HRESULT DoWork();
                };
            }");
        var output = _generator.Generate(compilation);

        // Fixed-size reply should use constexpr
        Assert.Contains("constexpr", output);
    }
}
