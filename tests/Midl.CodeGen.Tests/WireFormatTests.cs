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
        Assert.Equal("4", WireFormat.GetWireSizeExpression(typeRef));
    }

    [Fact]
    public void GetWireSizeExpression_String_ContainsDynamicExpression()
    {
        var typeRef = new TypeRef("String");
        var result = WireFormat.GetWireSizeExpression(typeRef);
        Assert.Contains("len", result);
        Assert.Contains("2", result);
    }

    [Fact]
    public void GetWireSizeExpression_Struct_ReturnsSumAsString()
    {
        var structSym = new StructSymbol("Point", null);
        structSym.AddField(new FieldSymbol("X", structSym) { Type = new TypeRef("Single") });
        structSym.AddField(new FieldSymbol("Y", structSym) { Type = new TypeRef("Single") });
        var typeRef = new TypeRef("Point") { ResolvedSymbol = structSym };

        Assert.Equal("8", WireFormat.GetWireSizeExpression(typeRef));
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
        Assert.Equal("4", WireFormat.ComputeBufferSize(parms));
    }

    [Fact]
    public void ComputeBufferSize_MultiplePrimitives_ReturnsSumExpression()
    {
        var parms = new List<ProxyParam>
        {
            new("x", new TypeRef("Single"), ParamDirection.In),
            new("y", new TypeRef("Single"), ParamDirection.In)
        };
        Assert.Equal("4 + 4", WireFormat.ComputeBufferSize(parms));
    }

    // ─── EmitSerialize / EmitDeserialize symmetry ───

    [Theory]
    [InlineData("Boolean", 1)]
    [InlineData("UInt8", 1)]
    [InlineData("Int16", 2)]
    [InlineData("UInt16", 2)]
    [InlineData("Char", 2)]
    [InlineData("Int32", 4)]
    [InlineData("UInt32", 4)]
    [InlineData("Single", 4)]
    [InlineData("Int64", 8)]
    [InlineData("UInt64", 8)]
    [InlineData("Double", 8)]
    public void EmitSerializeDeserialize_Primitive_Symmetric(string typeName, int size)
    {
        var typeRef = new TypeRef(typeName);

        var serSb = new StringBuilder();
        WireFormat.EmitSerialize(serSb, typeRef, "val", "p");
        var serCode = serSb.ToString();

        var deserSb = new StringBuilder();
        WireFormat.EmitDeserialize(deserSb, typeRef, "val", "pReply");
        var deserCode = deserSb.ToString();

        // Serialize writes `size` bytes from val
        Assert.Contains($"memcpy(p, &val, {size})", serCode);
        Assert.Contains($"p += {size}", serCode);

        // Deserialize reads `size` bytes into val
        Assert.Contains($"memcpy(&val, pReply, {size})", deserCode);
        Assert.Contains($"pReply += {size}", deserCode);
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

        Assert.Contains("memcpy(p, &id, 16)", serCode);
        Assert.Contains("memcpy(&id, pReply, 16)", deserCode);
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

        // Both use 4 bytes
        Assert.Contains("4", serCode);
        Assert.Contains("4", deserCode);
    }

    [Fact]
    public void EmitSerialize_HString_EmitsLengthPrefixedUtf16()
    {
        var typeRef = new TypeRef("String");

        var sb = new StringBuilder();
        WireFormat.EmitSerialize(sb, typeRef, "name", "p");
        var code = sb.ToString();

        Assert.Contains("WindowsGetStringLen(name)", code);
        Assert.Contains("memcpy(p, &name_len, 4)", code);
        Assert.Contains("WindowsGetStringRawBuffer(name", code);
        Assert.Contains("name_len * 2", code);
    }

    [Fact]
    public void EmitDeserialize_HString_EmitsLengthPrefixedUtf16()
    {
        var typeRef = new TypeRef("String");

        var sb = new StringBuilder();
        WireFormat.EmitDeserialize(sb, typeRef, "name", "pReply");
        var code = sb.ToString();

        Assert.Contains("memcpy(&name_len, pReply, 4)", code);
        Assert.Contains("WindowsCreateString", code);
        Assert.Contains("name_len * 2", code);
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

        Assert.Contains("memcpy(p, &token, 8)", code);
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
        Assert.Equal("4", result); // just HRESULT
    }

    [Fact]
    public void BuildReplySizeExpression_WithOutAndRetval_Includes_All()
    {
        var outParams = new List<(TypeRef Type, string Name)>
        {
            (new TypeRef("Int32"), "errorCode"),
        };
        var result = WireFormat.BuildReplySizeExpression(outParams, hasRetval: true, new TypeRef("Boolean"));
        // [errorCode=4] + [retval=1] + [HRESULT=4]
        Assert.Equal("4 + 1 + 4", result);
    }
}
