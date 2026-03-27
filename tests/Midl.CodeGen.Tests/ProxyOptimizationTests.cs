using Midl.Analysis;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// TDD tests for proxy/stub codegen optimization:
/// - Shared marshal/unmarshal helper calls replace inline memcpy
/// - Method descriptor tables for data-driven dispatch
/// - Signature folding for shared proxy/stub implementations
/// </summary>
public class ProxyOptimizationTests
{
    private readonly ProxyGenerator _proxyGen = new();
    private readonly StubGenerator _stubGen = new();

    private static Compilation Compile(string source) => CompilationHelper.Compile(source);

    // ════════════════════════════════════════════════════════════════
    // Phase 4: Marshal/unmarshal helper calls
    // ════════════════════════════════════════════════════════════════

    [Fact]
    public void Proxy_HStringParam_UsesMarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetName(String name);
                };
            }");
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::marshal_hstring(p,", output);
        // Inline serialization pattern should be replaced by helpers
        // (WindowsGetStringLen may still appear in buffer size computation — that's OK)
        Assert.DoesNotContain("PCWSTR name_raw = WindowsGetStringRawBuffer(name, nullptr)", output);
    }

    [Fact]
    public void Proxy_Int32Param_UsesMarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetValue(Int32 value);
                };
            }");
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::marshal_int32(p, value)", output);
    }

    [Fact]
    public void Proxy_UInt32Param_UsesMarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetCount(UInt32 count);
                };
            }");
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::marshal_uint32(p, count)", output);
    }

    [Fact]
    public void Proxy_Int64Param_UsesMarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetBigValue(Int64 bigValue);
                };
            }");
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::marshal_int64(p, bigValue)", output);
    }

    [Fact]
    public void Proxy_UInt64Param_UsesMarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetBigCount(UInt64 bigCount);
                };
            }");
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::marshal_uint64(p, bigCount)", output);
    }

    [Fact]
    public void Proxy_BooleanParam_UsesMarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetEnabled(Boolean enabled);
                };
            }");
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::marshal_bool(p, enabled)", output);
    }

    [Fact]
    public void Proxy_GuidParam_UsesMarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetId(Guid id);
                };
            }");
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::marshal_guid(p, id)", output);
    }

    [Fact]
    public void Proxy_SingleParam_UsesMarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetRatio(Single ratio);
                };
            }");
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::marshal_float(p, ratio)", output);
    }

    [Fact]
    public void Proxy_DoubleParam_UsesMarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetPrecision(Double precision);
                };
            }");
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::marshal_double(p, precision)", output);
    }

    [Fact]
    public void Proxy_EnumParam_UsesMarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                enum Status { Active, Inactive };
                interface IWidget {
                    void SetStatus(Status status);
                };
            }");
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::marshal_enum(p, status)", output);
    }

    [Fact]
    public void Proxy_Int16Param_UsesMarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetSmall(Int16 small);
                };
            }");
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::marshal_int16(p,", output);
    }

    [Fact]
    public void Proxy_UInt16Param_UsesMarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetSmallCount(UInt16 smallCount);
                };
            }");
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::marshal_uint16(p, smallCount)", output);
    }

    [Fact]
    public void Proxy_UInt8Param_UsesMarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetByte(UInt8 b);
                };
            }");
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::marshal_uint8(p, b)", output);
    }

    [Fact]
    public void Proxy_CharParam_UsesMarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetChar(Char c);
                };
            }");
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::marshal_char16(p, c)", output);
    }

    // ─── Proxy-side unmarshal (reply deserialization) ───

    [Fact]
    public void Proxy_HStringRetval_UsesUnmarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    String GetName();
                };
            }");
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::unmarshal_hstring(pReply,", output);
    }

    [Fact]
    public void Proxy_Int32Retval_UsesUnmarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    Int32 GetValue();
                };
            }");
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::unmarshal_int32(pReply,", output);
    }

    [Fact]
    public void Proxy_GuidRetval_UsesUnmarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    Guid GetId();
                };
            }");
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::unmarshal_guid(pReply,", output);
    }

    [Fact]
    public void Proxy_EnumRetval_UsesUnmarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                enum Status { Active, Inactive };
                interface IWidget {
                    Status GetStatus();
                };
            }");
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::unmarshal_enum(pReply,", output);
    }

    // ─── Stub-side unmarshal (request deserialization) ───

    [Fact]
    public void Stub_HStringParam_UsesUnmarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetName(String name);
                };
            }");
        var output = _stubGen.Generate(compilation);

        Assert.Contains("winmidl::unmarshal_hstring_ref(p,", output);
    }

    [Fact]
    public void Stub_Int32Param_UsesUnmarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetValue(Int32 value);
                };
            }");
        var output = _stubGen.Generate(compilation);

        Assert.Contains("winmidl::unmarshal_int32(p,", output);
    }

    [Fact]
    public void Stub_GuidParam_UsesUnmarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetId(Guid id);
                };
            }");
        var output = _stubGen.Generate(compilation);

        Assert.Contains("winmidl::unmarshal_guid(p,", output);
    }

    [Fact]
    public void Stub_EnumParam_UsesUnmarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                enum Status { Active, Inactive };
                interface IWidget {
                    void SetStatus(Status status);
                };
            }");
        var output = _stubGen.Generate(compilation);

        Assert.Contains("winmidl::unmarshal_enum(p,", output);
    }

    // ─── Stub-side marshal (reply serialization) ───

    [Fact]
    public void Stub_HStringRetval_UsesMarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    String GetName();
                };
            }");
        var output = _stubGen.Generate(compilation);

        Assert.Contains("winmidl::marshal_hstring(pOut, returnValue)", output);
    }

    [Fact]
    public void Stub_Int32Retval_UsesMarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    Int32 GetValue();
                };
            }");
        var output = _stubGen.Generate(compilation);

        Assert.Contains("winmidl::marshal_int32(pOut, returnValue)", output);
    }

    // ════════════════════════════════════════════════════════════════
    // Phase 5: Method descriptor tables
    // ════════════════════════════════════════════════════════════════

    [Fact]
    public void Proxy_EmitsMethodDescriptorTable()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    String GetName();
                    void SetName(String name);
                };
            }");
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::param_descriptor", output);
        Assert.Contains("winmidl::method_descriptor", output);
    }

    [Fact]
    public void Proxy_MethodDescriptor_HasCorrectSlots()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    String GetName();
                    void SetName(String name);
                };
            }");
        var output = _proxyGen.Generate(compilation);

        // First user method is slot 6 (after IUnknown 0-2, IInspectable 3-5)
        Assert.Contains("6,", output);
        Assert.Contains("7,", output);
    }

    [Fact]
    public void Stub_EmitsInterfaceDescriptor()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    String GetName();
                    void SetName(String name);
                };
            }");
        var output = _stubGen.Generate(compilation);

        Assert.Contains("winmidl::interface_descriptor", output);
        Assert.Contains("IID___x_TestApp_IWidget", output);
    }

    [Fact]
    public void Proxy_MethodDescriptor_IncludesParamTypes()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetName(String name);
                };
            }");
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::param_type::t_hstring", output);
        Assert.Contains("winmidl::param_dir::dir_in", output);
    }

    [Fact]
    public void Proxy_MethodDescriptor_RetvalHasCorrectDir()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    String GetName();
                };
            }");
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::param_dir::dir_retval", output);
    }

    [Fact]
    public void Proxy_MethodDescriptor_Int32Type()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    Int32 GetValue();
                };
            }");
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::param_type::t_int32", output);
    }

    [Fact]
    public void Proxy_MethodDescriptor_MultipleParams()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void Configure(Int32 width, Int32 height, String label);
                };
            }");
        var output = _proxyGen.Generate(compilation);

        // Should have 3 param descriptors
        Assert.Contains("t_int32", output);
        Assert.Contains("t_hstring", output);
    }

    // ════════════════════════════════════════════════════════════════
    // Phase 6: Signature folding
    // ════════════════════════════════════════════════════════════════

    [Fact]
    public void Proxy_SignatureFolding_SameShapeSharesImpl()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    String Name { get; };
                    String Title { get; };
                    String Description { get; };
                };
            }");
        var output = _proxyGen.Generate(compilation);

        // All three getters have same shape: void → HSTRING
        // Should reference a shared proxy implementation
        Assert.Contains("proxy_shape_", output);
    }

    [Fact]
    public void Proxy_SignatureFolding_DifferentShapesGetDifferentImpls()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    String GetName();
                    Int32 GetValue();
                };
            }");
        var output = _proxyGen.Generate(compilation);

        // void→HSTRING and void→Int32 should have different shape names
        var shapeMatches = output.Split("proxy_shape_").Length - 1;
        Assert.True(shapeMatches >= 2, $"Expected at least 2 shape references, got {shapeMatches}");
    }

    [Fact]
    public void Stub_SignatureFolding_SameShapeSharesImpl()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    String Name { get; };
                    String Title { get; };
                };
            }");
        var output = _stubGen.Generate(compilation);

        Assert.Contains("stub_shape_", output);
    }

    // ─── Multiple params signature folding ───

    [Fact]
    public void Proxy_SignatureFolding_SetterShapeShared()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    String Name { get; set; };
                    String Title { get; set; };
                };
            }");
        var output = _proxyGen.Generate(compilation);

        // Both setters have shape: HSTRING → void
        // Count occurrences of the setter shape function
        Assert.Contains("proxy_shape_", output);
    }

    // ─── Event registration token shapes ───

    [Fact]
    public void Proxy_EventToken_UsesMarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                delegate void ClickHandler();
                interface IWidget {
                    event ClickHandler Click;
                };
            }");
        var output = _proxyGen.Generate(compilation);

        // remove_Click takes EventRegistrationToken — should use marshal helper
        Assert.Contains("winmidl::marshal_event_token(p, token)", output);
    }

    // ─── Struct handling with helpers ───

    [Fact]
    public void Proxy_BlittableStruct_UsesMarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                struct Point { Int32 X; Int32 Y; };
                interface IWidget {
                    void SetPosition(Point pos);
                };
            }");
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::marshal_struct(p, pos)", output);
    }

    [Fact]
    public void Stub_BlittableStruct_UsesUnmarshalHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                struct Point { Int32 X; Int32 Y; };
                interface IWidget {
                    void SetPosition(Point pos);
                };
            }");
        var output = _stubGen.Generate(compilation);

        Assert.Contains("winmidl::unmarshal_struct(p,", output);
    }

    // ─── Combined: verify no inline memcpy for primitives ───

    [Fact]
    public void Proxy_NoInlineMemcpyForPrimitives()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetValue(Int32 value);
                    void SetName(String name);
                };
            }");
        var output = _proxyGen.Generate(compilation);

        // Serialization should use helpers, not raw memcpy for known types
        Assert.DoesNotContain("memcpy(p, &value, sizeof(INT32))", output);
        // Inline HSTRING serialization pattern should be replaced
        Assert.DoesNotContain("PCWSTR name_raw = WindowsGetStringRawBuffer(name, nullptr)", output);
    }

    [Fact]
    public void Stub_NoInlineMemcpyForPrimitives()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetValue(Int32 value);
                };
            }");
        var output = _stubGen.Generate(compilation);

        Assert.DoesNotContain("memcpy(&value, p, sizeof(INT32)); p += sizeof(INT32)", output);
    }

    // ─── Proxy helpers.h include ───

    [Fact]
    public void Stub_IncludesProxyHelpersHeader()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _stubGen.Generate(compilation);

        Assert.Contains("#include \"winmidl/proxy_helpers.h\"", output);
    }
}
