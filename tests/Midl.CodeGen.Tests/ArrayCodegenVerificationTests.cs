using Midl.Analysis;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Proxy/stub codegen verification for array params parsed from IDL with /arrays flag.
/// Validates end-to-end: IDL → compile → proxy/stub generation.
/// </summary>
public class ArrayCodegenVerificationTests
{
    private readonly ProxyGenerator _proxyGenerator = new();
    private readonly StubGenerator _stubGenerator = new();

    private static Compilation CompileWithArrays(string source) =>
        CompilationHelper.Compile(source, referencePaths: null,
            mode: CompilationMode.Strict, includePaths: null, currentFilePath: null,
            arraysEnabled: true);

    [Fact]
    public void Proxy_PassArray_FromIdl_EmitsCorrectCode()
    {
        var compilation = CompileWithArrays("""
            namespace TestApp
            {
                interface IWidget
                {
                    void SetItems(Int32[] items);
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error);

        var output = _proxyGenerator.Generate(compilation);
        Assert.Contains("UINT32 __itemsSize", output);
        Assert.Contains("INT32* items", output);
        Assert.Contains("marshal_array", output);
    }

    [Fact]
    public void Stub_PassArray_FromIdl_EmitsCorrectCode()
    {
        var compilation = CompileWithArrays("""
            namespace TestApp
            {
                interface IWidget
                {
                    void SetItems(Int32[] items);
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error);

        var output = _stubGenerator.Generate(compilation);
        Assert.Contains("unmarshal_array", output);
    }

    [Fact]
    public void Proxy_OutArray_FromIdl_EmitsCorrectCode()
    {
        var compilation = CompileWithArrays("""
            namespace TestApp
            {
                interface IWidget
                {
                    void GetItems(out Int32[] items);
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error);

        var output = _proxyGenerator.Generate(compilation);
        Assert.Contains("UINT32*", output);
        Assert.Contains("INT32**", output);
    }

    [Fact]
    public void Proxy_ArrayReturn_FromIdl_EmitsCorrectCode()
    {
        var compilation = CompileWithArrays("""
            namespace TestApp
            {
                interface IWidget
                {
                    Int32[] GetItems();
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error);

        var output = _proxyGenerator.Generate(compilation);
        Assert.Contains("marshal_array", output);
    }

    [Fact]
    public void Proxy_StringArray_FromIdl_EmitsHStringMarshal()
    {
        var compilation = CompileWithArrays("""
            namespace TestApp
            {
                interface IWidget
                {
                    void SetNames(String[] names);
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error);

        var output = _proxyGenerator.Generate(compilation);
        Assert.Contains("marshal_hstring_array", output);
    }

    [Fact]
    public void Stub_StringArray_FromIdl_EmitsHStringUnmarshal()
    {
        var compilation = CompileWithArrays("""
            namespace TestApp
            {
                interface IWidget
                {
                    void SetNames(String[] names);
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error);

        var output = _stubGenerator.Generate(compilation);
        Assert.Contains("unmarshal_hstring_array", output);
    }

    [Fact]
    public void Proxy_MixedParams_ArrayAndScalar()
    {
        var compilation = CompileWithArrays("""
            namespace TestApp
            {
                interface IWidget
                {
                    void Process(Int32 count, UInt8[] data);
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error);

        var output = _proxyGenerator.Generate(compilation);
        Assert.Contains("INT32 count", output);
        Assert.Contains("UINT32 __dataSize", output);
        Assert.Contains("BYTE* data", output);
    }

    [Fact]
    public void Proxy_FillArray_RefParam_EmitsCorrectSignature()
    {
        var compilation = CompileWithArrays("""
            namespace Test
            {
                interface IBufferReader
                {
                    void ReadInto(ref Int32[] buffer);
                }
            }
            """);
        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error);

        var output = _proxyGenerator.Generate(compilation);
        Assert.Contains("UINT32 __bufferCapacity", output);
        Assert.Contains("INT32* buffer", output);
        Assert.Contains("UINT32* __bufferActual", output);
    }

    [Fact]
    public void Proxy_FillArray_RefParam_SerializesCapacityInRequest()
    {
        var compilation = CompileWithArrays("""
            namespace Test
            {
                interface IBufferReader
                {
                    void ReadInto(ref Int32[] buffer);
                }
            }
            """);
        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error);

        var output = _proxyGenerator.Generate(compilation);
        Assert.Contains("marshal_uint32", output);
        Assert.Contains("__bufferCapacity", output);
    }

    [Fact]
    public void Proxy_FillArray_RefParam_DeserializesActualFromReply()
    {
        var compilation = CompileWithArrays("""
            namespace Test
            {
                interface IBufferReader
                {
                    void ReadInto(ref Int32[] buffer);
                }
            }
            """);
        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error);

        var output = _proxyGenerator.Generate(compilation);
        Assert.Contains("__bufferActual", output);
        Assert.Contains("_actual_temp", output);
    }

    [Fact]
    public void Stub_FillArray_RefParam_EmitsCorrectCode()
    {
        var compilation = CompileWithArrays("""
            namespace Test
            {
                interface IBufferReader
                {
                    void ReadInto(ref Int32[] buffer);
                }
            }
            """);
        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error);

        var output = _stubGenerator.Generate(compilation);
        Assert.Contains("__bufferCapacity", output);
        Assert.Contains("__bufferActual", output);
        Assert.Contains("CoTaskMemAlloc", output);
        Assert.Contains("CoTaskMemFree", output);
    }

    [Fact]
    public void Stub_FillArray_RefParam_ReadsCapacityFromRequest()
    {
        var compilation = CompileWithArrays("""
            namespace Test
            {
                interface IBufferReader
                {
                    void ReadInto(ref Int32[] buffer);
                }
            }
            """);
        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error);

        var output = _stubGenerator.Generate(compilation);
        Assert.Contains("__bufferCapacity", output);
        Assert.Contains("memcpy(&__bufferCapacity", output);
    }

    [Fact]
    public void Stub_FillArray_RefParam_PassesThreeArgsToServer()
    {
        var compilation = CompileWithArrays("""
            namespace Test
            {
                interface IBufferReader
                {
                    void ReadInto(ref Int32[] buffer);
                }
            }
            """);
        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error);

        var output = _stubGenerator.Generate(compilation);
        // Server call should pass capacity, buffer, &actual
        Assert.Contains("__bufferCapacity, buffer, &__bufferActual", output);
    }

    [Fact]
    public void Stub_FillArray_RefParam_SerializesActualAndElementsInReply()
    {
        var compilation = CompileWithArrays("""
            namespace Test
            {
                interface IBufferReader
                {
                    void ReadInto(ref Int32[] buffer);
                }
            }
            """);
        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error);

        var output = _stubGenerator.Generate(compilation);
        Assert.Contains("marshal_array(pOut, buffer, __bufferActual)", output);
    }

    [Fact]
    public void Proxy_FillArray_StringRef_EmitsCorrectSignature()
    {
        var compilation = CompileWithArrays("""
            namespace Test
            {
                interface INameReader
                {
                    void ReadNames(ref String[] names);
                }
            }
            """);
        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error);

        var output = _proxyGenerator.Generate(compilation);
        Assert.Contains("UINT32 __namesCapacity", output);
        Assert.Contains("HSTRING* names", output);
        Assert.Contains("UINT32* __namesActual", output);
    }

    [Fact]
    public void Stub_FillArray_StringRef_EmitsHStringArrayHelpers()
    {
        var compilation = CompileWithArrays("""
            namespace Test
            {
                interface INameReader
                {
                    void ReadNames(ref String[] names);
                }
            }
            """);
        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error);

        var output = _stubGenerator.Generate(compilation);
        Assert.Contains("__namesCapacity", output);
        Assert.Contains("__namesActual", output);
        Assert.Contains("CoTaskMemAlloc", output);
        Assert.Contains("CoTaskMemFree", output);
        Assert.Contains("marshal_hstring_array(pOut, names, __namesActual)", output);
    }

    [Fact]
    public void Stub_FillArray_StringRef_CleansUpStrings()
    {
        var compilation = CompileWithArrays("""
            namespace Test
            {
                interface INameReader
                {
                    void ReadNames(ref String[] names);
                }
            }
            """);
        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error);

        var output = _stubGenerator.Generate(compilation);
        Assert.Contains("WindowsDeleteString(names[i])", output);
    }

    [Fact]
    public void Proxy_FillArray_StringRef_DeserializesHStringsFromReply()
    {
        var compilation = CompileWithArrays("""
            namespace Test
            {
                interface INameReader
                {
                    void ReadNames(ref String[] names);
                }
            }
            """);
        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error);

        var output = _proxyGenerator.Generate(compilation);
        Assert.Contains("unmarshal_hstring_alloc", output);
        Assert.Contains("__namesActual", output);
    }
}
