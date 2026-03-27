using System.Text;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for WireFormat — shared serialization/deserialization helpers
/// used by both ProxyGenerator and StubGenerator.
/// Verifies type sizes, buffer size computation, serialize/deserialize symmetry,
/// vtable slot constants, and reply layout ordering.
/// </summary>
public partial class WireFormatTests
{
    // ─── GetWireSize ───

    [Theory]
    [InlineData("Boolean", 1)]
    [InlineData("UInt8", 1)]
    [InlineData("Int16", 2)]
    [InlineData("UInt16", 2)]
    [InlineData("Char", 2)]
    [InlineData("Int32", 4)]
    [InlineData("UInt32", 4)]
    [InlineData("Single", 4)]
    [InlineData("HRESULT", 4)]
    [InlineData("Int64", 8)]
    [InlineData("UInt64", 8)]
    [InlineData("Double", 8)]
    public void GetWireSize_Primitive_ReturnsCorrectSize(string typeName, int expectedSize)
    {
        var typeRef = new TypeRef(typeName);
        Assert.Equal(expectedSize, WireFormat.GetWireSize(typeRef));
    }

    [Fact]
    public void GetWireSize_Guid_Returns16()
    {
        var typeRef = new TypeRef("Guid");
        Assert.Equal(16, WireFormat.GetWireSize(typeRef));
    }

    [Fact]
    public void GetWireSize_Enum_Returns4()
    {
        var enumSym = new EnumSymbol("Status", null);
        var typeRef = new TypeRef("Status") { ResolvedSymbol = enumSym };
        Assert.Equal(4, WireFormat.GetWireSize(typeRef));
    }

    [Fact]
    public void GetWireSize_EventRegistrationToken_Returns8()
    {
        var typeRef = new TypeRef("EventRegistrationToken");
        Assert.Equal(8, WireFormat.GetWireSize(typeRef));
    }

    [Fact]
    public void GetWireSize_Struct_ReturnsSumOfFields()
    {
        var structSym = new StructSymbol("Point", null);
        structSym.AddField(new FieldSymbol("X", structSym) { Type = new TypeRef("Single") });
        structSym.AddField(new FieldSymbol("Y", structSym) { Type = new TypeRef("Single") });
        var typeRef = new TypeRef("Point") { ResolvedSymbol = structSym };

        Assert.Equal(8, WireFormat.GetWireSize(typeRef));
    }

    [Fact]
    public void GetWireSize_NestedStruct_ReturnsTotalFieldSizes()
    {
        var innerStruct = new StructSymbol("Inner", null);
        innerStruct.AddField(new FieldSymbol("A", innerStruct) { Type = new TypeRef("Int32") });
        innerStruct.AddField(new FieldSymbol("B", innerStruct) { Type = new TypeRef("Int32") });

        var outerStruct = new StructSymbol("Outer", null);
        outerStruct.AddField(new FieldSymbol("X", outerStruct) { Type = new TypeRef("Inner") { ResolvedSymbol = innerStruct } });
        outerStruct.AddField(new FieldSymbol("Y", outerStruct) { Type = new TypeRef("Double") });

        var typeRef = new TypeRef("Outer") { ResolvedSymbol = outerStruct };
        // Inner = 4+4=8, Double = 8, total = 16
        Assert.Equal(16, WireFormat.GetWireSize(typeRef));
    }

    [Fact]
    public void GetWireSize_UnknownType_ReturnsFallback()
    {
        var typeRef = new TypeRef("SomeUnknownType");
        // Should return a default fallback (not throw)
        var size = WireFormat.GetWireSize(typeRef);
        Assert.True(size > 0);
    }

    // ─── GetWireSizeExpression ───

    [Fact]
    public void GetWireSizeExpression_Primitive_ReturnsNumericString()
    {
        var typeRef = new TypeRef("Int32");
        Assert.Equal("sizeof(INT32)", WireFormat.GetWireSizeExpression(typeRef));
    }

    [Fact]
    public void GetWireSizeExpression_String_ContainsDynamicExpression()
    {
        var typeRef = new TypeRef("String");
        var result = WireFormat.GetWireSizeExpression(typeRef);
        Assert.Contains("len", result);
        Assert.Contains("sizeof(WCHAR)", result);
    }

    [Fact]
    public void GetWireSizeExpression_Struct_ReturnsSumAsString()
    {
        var structSym = new StructSymbol("Point", null);
        structSym.AddField(new FieldSymbol("X", structSym) { Type = new TypeRef("Single") });
        structSym.AddField(new FieldSymbol("Y", structSym) { Type = new TypeRef("Single") });
        var typeRef = new TypeRef("Point") { ResolvedSymbol = structSym };

        Assert.Equal("sizeof(FLOAT) + sizeof(FLOAT)", WireFormat.GetWireSizeExpression(typeRef));
    }

    // ─── ComputeBufferSize ───

    [Fact]
    public void ComputeBufferSize_NoParams_ReturnsZero()
    {
        Assert.Equal("0", WireFormat.ComputeBufferSize([]));
    }

    [Fact]
    public void ComputeBufferSize_SinglePrimitive_ReturnsSize()
    {
        var parms = new List<ProxyParam>
        {
            new("count", new TypeRef("Int32"), ParamDirection.In)
        };
        Assert.Equal("sizeof(INT32)", WireFormat.ComputeBufferSize(parms));
    }

    [Fact]
    public void ComputeBufferSize_MultiplePrimitives_ReturnsSumExpression()
    {
        var parms = new List<ProxyParam>
        {
            new("x", new TypeRef("Single"), ParamDirection.In),
            new("y", new TypeRef("Single"), ParamDirection.In)
        };
        Assert.Equal("sizeof(FLOAT) + sizeof(FLOAT)", WireFormat.ComputeBufferSize(parms));
    }

    // ─── EmitSerialize / EmitDeserialize symmetry ───

    [Theory]
    [InlineData("Boolean", "marshal_bool")]
    [InlineData("UInt8", "marshal_uint8")]
    [InlineData("Int16", "marshal_int16")]
    [InlineData("UInt16", "marshal_uint16")]
    [InlineData("Char", "marshal_char16")]
    [InlineData("Int32", "marshal_int32")]
    [InlineData("UInt32", "marshal_uint32")]
    [InlineData("Single", "marshal_float")]
    [InlineData("Int64", "marshal_int64")]
    [InlineData("UInt64", "marshal_uint64")]
    [InlineData("Double", "marshal_double")]
    public void EmitSerializeDeserialize_Primitive_Symmetric(string typeName, string marshalFunc)
    {
        var typeRef = new TypeRef(typeName);

        var serSb = new StringBuilder();
        WireFormat.EmitSerialize(serSb, typeRef, "val", "p");
        var serCode = serSb.ToString();

        var deserSb = new StringBuilder();
        WireFormat.EmitDeserialize(deserSb, typeRef, "val", "pReply");
        var deserCode = deserSb.ToString();

        // Serialize uses marshal helper
        Assert.Contains($"winmidl::{marshalFunc}(p, val)", serCode);

        // Deserialize uses unmarshal helper
        var unmarshalFunc = marshalFunc.Replace("marshal_", "unmarshal_");
        Assert.Contains($"winmidl::{unmarshalFunc}(pReply, val)", deserCode);
    }

    [Fact]
    public void EmitSerializeDeserialize_Guid_Symmetric()
    {
        var typeRef = new TypeRef("Guid");

        var serSb = new StringBuilder();
        WireFormat.EmitSerialize(serSb, typeRef, "id", "p");
        var serCode = serSb.ToString();

        var deserSb = new StringBuilder();
        WireFormat.EmitDeserialize(deserSb, typeRef, "id", "pReply");
        var deserCode = deserSb.ToString();

        Assert.Contains("winmidl::marshal_guid(p, id)", serCode);
        Assert.Contains("winmidl::unmarshal_guid(pReply, id)", deserCode);
    }

    [Fact]
    public void EmitSerializeDeserialize_Enum_Symmetric()
    {
        var enumSym = new EnumSymbol("Priority", null);
        var typeRef = new TypeRef("Priority") { ResolvedSymbol = enumSym };

        var serSb = new StringBuilder();
        WireFormat.EmitSerialize(serSb, typeRef, "level", "p");
        var serCode = serSb.ToString();

        var deserSb = new StringBuilder();
        WireFormat.EmitDeserialize(deserSb, typeRef, "level", "pReply");
        var deserCode = deserSb.ToString();

        // Both use marshal/unmarshal enum helpers
        Assert.Contains("winmidl::marshal_enum(p, level)", serCode);
        Assert.Contains("winmidl::unmarshal_enum(pReply, level)", deserCode);
    }

    [Fact]
    public void EmitSerialize_HString_EmitsLengthPrefixedUtf16()
    {
        var typeRef = new TypeRef("String");

        var sb = new StringBuilder();
        WireFormat.EmitSerialize(sb, typeRef, "name", "p");
        var code = sb.ToString();

        Assert.Contains("winmidl::marshal_hstring(p, name)", code);
    }

    [Fact]
    public void EmitDeserialize_HString_EmitsLengthPrefixedUtf16()
    {
        var typeRef = new TypeRef("String");

        var sb = new StringBuilder();
        WireFormat.EmitDeserialize(sb, typeRef, "name", "pReply");
        var code = sb.ToString();

        Assert.Contains("memcpy(&name_len, pReply, sizeof(UINT32))", code);
        Assert.Contains("WindowsCreateStringReference", code);
        Assert.Contains("name_len * sizeof(WCHAR)", code);
    }

    [Fact]
    public void EmitSerialize_Struct_EmitsFieldByField()
    {
        // Non-blittable struct (has enum field) should still do field-by-field
        var enumSym = new EnumSymbol("Status", null);
        var structSym = new StructSymbol("Item", null);
        structSym.AddField(new FieldSymbol("Id", structSym) { Type = new TypeRef("Int32") });
        structSym.AddField(new FieldSymbol("State", structSym) { Type = new TypeRef("Status") { ResolvedSymbol = enumSym } });
        var typeRef = new TypeRef("Item") { ResolvedSymbol = structSym };

        var sb = new StringBuilder();
        WireFormat.EmitSerialize(sb, typeRef, "item", "p");
        var code = sb.ToString();

        Assert.Contains("item.Id", code);
        Assert.Contains("item.State", code);
    }

    [Fact]
    public void EmitDeserialize_Struct_EmitsFieldByField()
    {
        // Non-blittable struct (has enum field) should still do field-by-field
        var enumSym = new EnumSymbol("Status", null);
        var structSym = new StructSymbol("Item", null);
        structSym.AddField(new FieldSymbol("Id", structSym) { Type = new TypeRef("Int32") });
        structSym.AddField(new FieldSymbol("State", structSym) { Type = new TypeRef("Status") { ResolvedSymbol = enumSym } });
        var typeRef = new TypeRef("Item") { ResolvedSymbol = structSym };

        var sb = new StringBuilder();
        WireFormat.EmitDeserialize(sb, typeRef, "item", "pReply");
        var code = sb.ToString();

        Assert.Contains("item.Id", code);
        Assert.Contains("item.State", code);
    }

    [Fact]
    public void EmitSerialize_EventRegistrationToken_Uses8Bytes()
    {
        var typeRef = new TypeRef("EventRegistrationToken");

        var sb = new StringBuilder();
        WireFormat.EmitSerialize(sb, typeRef, "token", "p");
        var code = sb.ToString();

        Assert.Contains("winmidl::marshal_event_token(p, token)", code);
    }

    // ─── GetFirstUserSlot ───

    [Fact]
    public void GetFirstUserSlot_IInspectable_Returns6()
    {
        Assert.Equal(6, WireFormat.GetFirstUserSlot(isInspectable: true));
    }

    [Fact]
    public void GetFirstUserSlot_IUnknown_Returns3()
    {
        Assert.Equal(3, WireFormat.GetFirstUserSlot(isInspectable: false));
    }

    // ─── Constants ───

    [Fact]
    public void HResultSize_Is4()
    {
        Assert.Equal(4, WireFormat.HResultSize);
    }

    // ─── Reply layout ───

    [Fact]
    public void BuildReplySizeExpression_NoOutNoRetval_JustHresult()
    {
        var result = WireFormat.BuildReplySizeExpression([], hasRetval: false, TypeRef.Void);
        Assert.Equal("sizeof(HRESULT)", result); // just HRESULT
    }

    [Fact]
    public void BuildReplySizeExpression_WithOutAndRetval_Includes_All()
    {
        var outParams = new List<(TypeRef Type, string Name)>
        {
            (new TypeRef("Int32"), "errorCode"),
        };
        var result = WireFormat.BuildReplySizeExpression(outParams, hasRetval: true, new TypeRef("Boolean"));
        // [errorCode=sizeof(INT32)] + [retval=sizeof(boolean)] + [HRESULT=sizeof(HRESULT)]
        Assert.Equal("sizeof(INT32) + sizeof(boolean) + sizeof(HRESULT)", result);
    }
}
