using System.Text;
using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for array parameter marshaling in proxy/stub codegen.
/// Covers PassArray (in), ReceiveArray (out retval), and string array patterns.
/// </summary>
public class ArrayMarshalingTests
{
    private readonly ProxyGenerator _proxyGenerator = new();
    private readonly StubGenerator _stubGenerator = new();

    private static Compilation Compile(string source) => CompilationHelper.Compile(source);

    // ════════════════════════════════════════════════════════════════
    // WireFormat tests
    // ════════════════════════════════════════════════════════════════

    [Fact]
    public void WireFormat_ArrayParam_IsNotFixedSize()
    {
        var arrayType = new TypeRef("Int32").WithArray();
        Assert.False(WireFormat.IsFixedSize(arrayType));
    }

    [Fact]
    public void WireFormat_StringArrayParam_IsNotFixedSize()
    {
        var arrayType = new TypeRef("String").WithArray();
        Assert.False(WireFormat.IsFixedSize(arrayType));
    }

    [Fact]
    public void WireFormat_ElementType_ReturnsNonArrayTypeRef()
    {
        var arrayType = new TypeRef("Int32").WithArray();
        var elemType = WireFormat.ElementType(arrayType);
        Assert.Equal("Int32", elemType.Name);
        Assert.False(elemType.IsArray);
    }

    [Fact]
    public void WireFormat_ElementType_PreservesResolvedSymbol()
    {
        var enumSym = new EnumSymbol("MyEnum", null);
        var arrayType = new TypeRef("MyEnum").WithArray();
        arrayType.ResolvedSymbol = enumSym;
        var elemType = WireFormat.ElementType(arrayType);
        Assert.Same(enumSym, elemType.ResolvedSymbol);
        Assert.False(elemType.IsArray);
    }

    [Fact]
    public void WireFormat_AllParamsFixedSize_FalseForArrayParam()
    {
        var arrayType = new TypeRef("Int32").WithArray();
        var parms = new List<ProxyParam>
        {
            new("values", arrayType, ParamDirection.In)
        };
        Assert.False(WireFormat.AllParamsFixedSize(parms));
    }

    [Fact]
    public void WireFormat_ComputeBufferSize_ArrayParam()
    {
        var arrayType = new TypeRef("Int32").WithArray();
        var parms = new List<ProxyParam>
        {
            new("values", arrayType, ParamDirection.In)
        };
        var result = WireFormat.ComputeBufferSize(parms);
        Assert.Contains("sizeof(UINT32)", result);
        Assert.Contains("__valuesSize", result);
        Assert.Contains("sizeof(INT32)", result);
    }

    [Fact]
    public void WireFormat_ComputeBufferSize_StringArrayParam()
    {
        var arrayType = new TypeRef("String").WithArray();
        var parms = new List<ProxyParam>
        {
            new("names", arrayType, ParamDirection.In)
        };
        var result = WireFormat.ComputeBufferSize(parms);
        Assert.Contains("hstring_array_size", result);
        Assert.Contains("names", result);
        Assert.Contains("__namesSize", result);
    }

    [Fact]
    public void WireFormat_EmitSerialize_ArrayParam_EmitsMarshalArray()
    {
        var sb = new StringBuilder();
        var arrayType = new TypeRef("Int32").WithArray();
        WireFormat.EmitSerialize(sb, arrayType, "values", "p");
        var output = sb.ToString();
        Assert.Contains("winmidl::marshal_array(p, values, __valuesSize)", output);
    }

    [Fact]
    public void WireFormat_EmitSerialize_StringArrayParam_EmitsMarshalHStringArray()
    {
        var sb = new StringBuilder();
        var arrayType = new TypeRef("String").WithArray();
        WireFormat.EmitSerialize(sb, arrayType, "names", "p");
        var output = sb.ToString();
        Assert.Contains("winmidl::marshal_hstring_array(p, names, __namesSize)", output);
    }

    [Fact]
    public void WireFormat_EmitDeserialize_ArrayParam_EmitsUnmarshalArray()
    {
        var sb = new StringBuilder();
        var arrayType = new TypeRef("Int32").WithArray();
        WireFormat.EmitDeserialize(sb, arrayType, "values", "p");
        var output = sb.ToString();
        Assert.Contains("UINT32 __valuesSize;", output);
        Assert.Contains("INT32* values;", output);
        Assert.Contains("winmidl::unmarshal_array(p, &values, &__valuesSize)", output);
    }

    [Fact]
    public void WireFormat_EmitDeserialize_StringArrayParam_EmitsUnmarshalHStringArray()
    {
        var sb = new StringBuilder();
        var arrayType = new TypeRef("String").WithArray();
        WireFormat.EmitDeserialize(sb, arrayType, "names", "p");
        var output = sb.ToString();
        Assert.Contains("UINT32 __namesSize;", output);
        Assert.Contains("HSTRING* names;", output);
        Assert.Contains("winmidl::unmarshal_hstring_array(p, &names, &__namesSize)", output);
    }

    [Fact]
    public void WireFormat_EmitDeserializeReply_ArrayParam_EmitsUnmarshalArray()
    {
        var sb = new StringBuilder();
        var arrayType = new TypeRef("Int32").WithArray();
        WireFormat.EmitDeserializeReply(sb, arrayType, "returnValue", "pReply", isPointer: true);
        var output = sb.ToString();
        Assert.Contains("winmidl::unmarshal_array(pReply, returnValue, __returnValueSize)", output);
    }

    [Fact]
    public void WireFormat_EmitSerializeReplyValue_ArrayParam_EmitsMarshalArray()
    {
        var sb = new StringBuilder();
        var arrayType = new TypeRef("Int32").WithArray();
        WireFormat.EmitSerializeReplyValue(sb, arrayType, "returnValue", "pOut");
        var output = sb.ToString();
        Assert.Contains("winmidl::marshal_array(pOut, returnValue, __returnValueSize)", output);
    }

    [Fact]
    public void WireFormat_GetCppTypeName_PrimitiveTypes()
    {
        Assert.Equal("INT32", WireFormat.GetCppTypeName(new TypeRef("Int32")));
        Assert.Equal("FLOAT", WireFormat.GetCppTypeName(new TypeRef("Single")));
        Assert.Equal("DOUBLE", WireFormat.GetCppTypeName(new TypeRef("Double")));
        Assert.Equal("HSTRING", WireFormat.GetCppTypeName(new TypeRef("String")));
        Assert.Equal("BYTE", WireFormat.GetCppTypeName(new TypeRef("UInt8")));
        Assert.Equal("GUID", WireFormat.GetCppTypeName(new TypeRef("Guid")));
    }

    // ════════════════════════════════════════════════════════════════
    // ProxyGenerator tests
    // ════════════════════════════════════════════════════════════════

    [Fact]
    public void Proxy_PassArray_EmitsArraySignature()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void SetValues(Int32[] values);
                }
            }");

        var output = _proxyGenerator.Generate(compilation);

        // Should emit ABI-expanded signature: UINT32 size + element pointer
        Assert.Contains("UINT32 __valuesSize", output);
        Assert.Contains("INT32* values", output);
    }

    [Fact]
    public void Proxy_PassArray_EmitsMarshalArray()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void SetValues(Int32[] values);
                }
            }");

        var output = _proxyGenerator.Generate(compilation);

        Assert.Contains("winmidl::marshal_array(p, values, __valuesSize)", output);
    }

    [Fact]
    public void Proxy_PassArray_BufferSizeIncludesArrayExpression()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void SetValues(Int32[] values);
                }
            }");

        var output = _proxyGenerator.Generate(compilation);

        // Array buffer size should be dynamic, not constexpr
        Assert.Contains("__valuesSize", output);
        Assert.Contains("sizeof(INT32)", output);
    }

    [Fact]
    public void Proxy_StringArray_EmitsHStringArrayMarshal()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void SetNames(String[] names);
                }
            }");

        var output = _proxyGenerator.Generate(compilation);

        Assert.Contains("UINT32 __namesSize", output);
        Assert.Contains("HSTRING* names", output);
        Assert.Contains("winmidl::marshal_hstring_array(p, names, __namesSize)", output);
    }

    [Fact]
    public void Proxy_StringArray_BufferSizeUsesHStringArraySize()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void SetNames(String[] names);
                }
            }");

        var output = _proxyGenerator.Generate(compilation);

        Assert.Contains("hstring_array_size(names, __namesSize)", output);
    }

    [Fact]
    public void Proxy_ReceiveArrayRetval_EmitsOutArraySignature()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    Int32[] GetValues();
                }
            }");

        var output = _proxyGenerator.Generate(compilation);

        // Return type array should emit out-pointer pair
        Assert.Contains("UINT32* __returnValueSize", output);
        Assert.Contains("INT32** returnValue", output);
    }

    [Fact]
    public void Proxy_ReceiveArrayRetval_EmitsUnmarshalArray()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    Int32[] GetValues();
                }
            }");

        var output = _proxyGenerator.Generate(compilation);

        Assert.Contains("unmarshal_array(pReply, returnValue, __returnValueSize)", output);
    }

    [Fact]
    public void Proxy_OutArray_EmitsOutArraySignature()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void GetValues(out Int32[] values);
                }
            }");

        var output = _proxyGenerator.Generate(compilation);

        Assert.Contains("UINT32* __valuesSize", output);
        Assert.Contains("INT32** values", output);
    }

    [Fact]
    public void Proxy_MixedParams_ArrayAndScalar()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void Process(Int32 count, UInt8[] data);
                }
            }");

        var output = _proxyGenerator.Generate(compilation);

        // Should have scalar param followed by array pair
        Assert.Contains("INT32 count", output);
        Assert.Contains("UINT32 __dataSize", output);
        Assert.Contains("BYTE* data", output);
    }

    // ════════════════════════════════════════════════════════════════
    // StubGenerator tests
    // ════════════════════════════════════════════════════════════════

    [Fact]
    public void Stub_PassArray_EmitsUnmarshalArray()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void SetValues(Int32[] values);
                }
            }");

        var output = _stubGenerator.Generate(compilation);

        Assert.Contains("UINT32 __valuesSize;", output);
        Assert.Contains("INT32* values;", output);
        Assert.Contains("winmidl::unmarshal_array(p, &values, &__valuesSize)", output);
    }

    [Fact]
    public void Stub_PassArray_PassesSizeAndPointerToServer()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void SetValues(Int32[] values);
                }
            }");

        var output = _stubGenerator.Generate(compilation);

        // Server call should pass __valuesSize and values as separate args
        Assert.Contains("__valuesSize, values", output);
    }

    [Fact]
    public void Stub_PassArray_CleansUpAllocatedMemory()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void SetValues(Int32[] values);
                }
            }");

        var output = _stubGenerator.Generate(compilation);

        Assert.Contains("CoTaskMemFree(values)", output);
    }

    [Fact]
    public void Stub_StringArray_EmitsUnmarshalHStringArray()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void SetNames(String[] names);
                }
            }");

        var output = _stubGenerator.Generate(compilation);

        Assert.Contains("HSTRING* names;", output);
        Assert.Contains("winmidl::unmarshal_hstring_array(p, &names, &__namesSize)", output);
    }

    [Fact]
    public void Stub_StringArray_CleansUpStringsAndMemory()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void SetNames(String[] names);
                }
            }");

        var output = _stubGenerator.Generate(compilation);

        // Should clean up individual HSTRINGs before freeing array
        Assert.Contains("WindowsDeleteString(names[i])", output);
        Assert.Contains("CoTaskMemFree(names)", output);
    }

    [Fact]
    public void Stub_ReceiveArrayRetval_EmitsReturnValueArrayDecl()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    Int32[] GetValues();
                }
            }");

        var output = _stubGenerator.Generate(compilation);

        Assert.Contains("UINT32 __returnValueSize = 0;", output);
        Assert.Contains("INT32* returnValue = nullptr;", output);
    }

    [Fact]
    public void Stub_ReceiveArrayRetval_PassesSizeAndPointerToServer()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    Int32[] GetValues();
                }
            }");

        var output = _stubGenerator.Generate(compilation);

        Assert.Contains("&__returnValueSize, &returnValue", output);
    }

    [Fact]
    public void Stub_ReceiveArrayRetval_SerializesArrayInReply()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    Int32[] GetValues();
                }
            }");

        var output = _stubGenerator.Generate(compilation);

        Assert.Contains("winmidl::marshal_array(pOut, returnValue, __returnValueSize)", output);
    }

    [Fact]
    public void Stub_OutArray_DeclaresArrayVars()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void GetValues(out Int32[] values);
                }
            }");

        var output = _stubGenerator.Generate(compilation);

        Assert.Contains("UINT32 __valuesSize = 0;", output);
        Assert.Contains("INT32* values = nullptr;", output);
    }

    [Fact]
    public void Stub_OutArray_SerializesArrayInReply()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void GetValues(out Int32[] values);
                }
            }");

        var output = _stubGenerator.Generate(compilation);

        Assert.Contains("winmidl::marshal_array(pOut, values, __valuesSize)", output);
    }

    [Fact]
    public void Stub_OutArray_ReplySizeIncludesArraySize()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void GetValues(out Int32[] values);
                }
            }");

        var output = _stubGenerator.Generate(compilation);

        // Reply size should include array size expression
        Assert.Contains("__valuesSize", output);
    }

    [Fact]
    public void Stub_DoubleArray_EmitsMarshalArray()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void SetPositions(Double[] positions);
                }
            }");

        var output = _stubGenerator.Generate(compilation);

        Assert.Contains("DOUBLE* positions;", output);
        Assert.Contains("winmidl::unmarshal_array(p, &positions, &__positionsSize)", output);
    }
}
