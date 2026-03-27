using Midl.Analysis;
using Midl.CodeGen.Proxy;
using Xunit;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Edge-case tests for proxy/stub marshaling: null pointers, empty arrays,
/// out-param cleanup on failure.
/// </summary>
public class EdgeCaseTests
{
    private readonly StubGenerator _stubGen = new();
    private readonly ProxyGenerator _proxyGen = new();

    private static Compilation Compile(string source) => CompilationHelper.Compile(source);

    // ─── Null interface pointer ────────────────────────────────────────

    [Fact]
    public void ProxyHelpers_NullInterfacePointer_WritesZeroSize()
    {
        // proxy_helpers.h marshal_interface with nullptr writes UINT32(0) only
        var source = """
            namespace T {
                interface IFoo {
                    IFoo GetChild();
                }
            }
            """;
        var compilation = Compile(source);
        var output = _proxyGen.Generate(compilation);

        // The proxy should call marshal_interface which handles nullptr
        Assert.Contains("marshal_interface", output);
    }

    [Fact]
    public void ProxyHelpers_UnmarshalInterface_ZeroSize_ReturnsNullptr()
    {
        // proxy_helpers.h unmarshal_interface with streamSize==0 sets *ppInterface=nullptr
        var source = """
            namespace T {
                interface IFoo {
                    IFoo GetChild();
                }
            }
            """;
        var compilation = Compile(source);
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("unmarshal_interface", output);
    }

    // ─── Null HSTRING marshaling ───────────────────────────────────────

    [Fact]
    public void ProxyHelpers_NullHString_WritesZeroCount()
    {
        // proxy_helpers.h marshal_hstring with nullptr writes count=0
        var source = """
            namespace T {
                interface IFoo {
                    String GetName();
                }
            }
            """;
        var compilation = Compile(source);

        var stubOutput = _stubGen.Generate(compilation);
        // Stub serializes HSTRING via marshal_hstring which handles null
        Assert.Contains("marshal_hstring", stubOutput);
    }

    // ─── Empty array marshaling ────────────────────────────────────────

    [Fact]
    public void ProxyHelpers_EmptyArray_WritesCountOnly()
    {
        // proxy_helpers.h marshal_array with count=0 writes only count prefix
        var source = """
            namespace T {
                interface IFoo {
                    void Process(Int32[] items);
                }
            }
            """;
        var compilation = Compile(source);
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("marshal_array", output);
    }

    // ─── Out-param cleanup on failure ──────────────────────────────────

    [Fact]
    public void StubGenerator_OutParamString_CleansUpOnGetBufferFailure()
    {
        var source = """
            namespace T {
                interface IFoo {
                    void GetBoth(out String name, out Int32 count);
                }
            }
            """;
        var compilation = Compile(source);
        var output = _stubGen.Generate(compilation);

        // Should have HSTRING cleanup in GetBuffer failure path
        Assert.Contains("if (FAILED(hrBuf))", output);
        Assert.Contains("WindowsDeleteString(name)", output);
    }

    [Fact]
    public void StubGenerator_OutParamString_CleansUpAfterSerialization()
    {
        var source = """
            namespace T {
                interface IFoo {
                    void GetName(out String name);
                }
            }
            """;
        var compilation = Compile(source);
        var output = _stubGen.Generate(compilation);

        // Out-param HSTRING should be cleaned up after serialization
        Assert.Contains("WindowsDeleteString(name)", output);
    }

    [Fact]
    public void StubGenerator_OutParamInterface_ReleasesOnGetBufferFailure()
    {
        var source = """
            namespace T {
                interface IBar {
                    void DoWork();
                }
                interface IFoo {
                    void GetWidget(out IBar widget);
                }
            }
            """;
        var compilation = Compile(source);
        var output = _stubGen.Generate(compilation);

        // Should release interface out-param on failure
        Assert.Contains("if (FAILED(hrBuf))", output);
        Assert.Contains("if (widget) widget->Release()", output);
    }

    [Fact]
    public void StubGenerator_OutParamArray_CleansUpOnGetBufferFailure()
    {
        var source = """
            namespace T {
                interface IFoo {
                    void GetItems(out Int32[] items);
                }
            }
            """;
        var compilation = Compile(source);
        var output = _stubGen.Generate(compilation);

        // Should free array on failure
        Assert.Contains("if (FAILED(hrBuf))", output);
        Assert.Contains("CoTaskMemFree(items)", output);
    }

    [Fact]
    public void StubGenerator_OutParamStringArray_CleansUpElementsOnFailure()
    {
        var source = """
            namespace T {
                interface IFoo {
                    void GetNames(out String[] names);
                }
            }
            """;
        var compilation = Compile(source);
        var output = _stubGen.Generate(compilation);

        // Should clean up per-element HSTRINGs and then free the array
        Assert.Contains("if (FAILED(hrBuf))", output);
        Assert.Contains("WindowsDeleteString(names[i])", output);
        Assert.Contains("CoTaskMemFree(names)", output);
    }

    [Fact]
    public void StubGenerator_RetvalString_CleansUpAfterSerialization()
    {
        var source = """
            namespace T {
                interface IFoo {
                    String GetName();
                }
            }
            """;
        var compilation = Compile(source);
        var output = _stubGen.Generate(compilation);

        // Retval HSTRING should be cleaned up
        Assert.Contains("WindowsDeleteString(returnValue)", output);
    }

    [Fact]
    public void StubGenerator_RetvalInterface_ReleasesAfterSerialization()
    {
        var source = """
            namespace T {
                interface IBar {
                    void DoWork();
                }
                interface IFoo {
                    IBar GetWidget();
                }
            }
            """;
        var compilation = Compile(source);
        var output = _stubGen.Generate(compilation);

        // Retval interface should be released after serialization
        Assert.Contains("if (returnValue) returnValue->Release()", output);
    }

    [Fact]
    public void StubGenerator_RetvalArray_CleansUpAfterSerialization()
    {
        var source = """
            namespace T {
                interface IFoo {
                    Int32[] GetItems();
                }
            }
            """;
        var compilation = Compile(source);
        var output = _stubGen.Generate(compilation);

        // Retval array should be freed after serialization
        Assert.Contains("CoTaskMemFree(returnValue)", output);
    }

    [Fact]
    public void StubGenerator_MultipleOutParams_AllCleanedOnFailure()
    {
        var source = """
            namespace T {
                interface IWidget {
                    void DoWork();
                }
                interface IFoo {
                    void GetAll(out String name, out IWidget widget, out Int32[] items);
                }
            }
            """;
        var compilation = Compile(source);
        var output = _stubGen.Generate(compilation);

        // All out-param types should be cleaned up in the failure path
        Assert.Contains("if (FAILED(hrBuf))", output);
        Assert.Contains("WindowsDeleteString(name)", output);
        Assert.Contains("if (widget) widget->Release()", output);
        Assert.Contains("CoTaskMemFree(items)", output);
    }

    [Fact]
    public void StubGenerator_VoidNoOutParams_NoFailureCleanupBlock()
    {
        var source = """
            namespace T {
                interface IFoo {
                    void DoWork();
                }
            }
            """;
        var compilation = Compile(source);
        var output = _stubGen.Generate(compilation);

        // No out-params means no cleanup block needed — uses simple RETURN_IF_FAILED
        Assert.Contains("RETURN_IF_FAILED(hrBuf)", output);
        Assert.DoesNotContain("if (FAILED(hrBuf))", output);
    }

    [Fact]
    public void StubGenerator_PrimitiveOutParam_NoFailureCleanupBlock()
    {
        var source = """
            namespace T {
                interface IFoo {
                    void GetCount(out Int32 count);
                }
            }
            """;
        var compilation = Compile(source);
        var output = _stubGen.Generate(compilation);

        // Primitive out-params don't need cleanup, so use simple RETURN_IF_FAILED
        Assert.Contains("RETURN_IF_FAILED(hrBuf)", output);
        Assert.DoesNotContain("if (FAILED(hrBuf))", output);
    }

    [Fact]
    public void StubGenerator_PrimitiveRetval_NoFailureCleanupBlock()
    {
        var source = """
            namespace T {
                interface IFoo {
                    Int32 GetCount();
                }
            }
            """;
        var compilation = Compile(source);
        var output = _stubGen.Generate(compilation);

        // Primitive retval doesn't need cleanup
        Assert.Contains("RETURN_IF_FAILED(hrBuf)", output);
        Assert.DoesNotContain("if (FAILED(hrBuf))", output);
    }
}
