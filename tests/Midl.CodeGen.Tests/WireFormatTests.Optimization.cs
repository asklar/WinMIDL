using System.Text;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for wire-format optimizations: constexpr sizes, blittable structs,
/// zero-copy HSTRING deserialization, and struct layout assertions.
/// </summary>
public partial class WireFormatTests
{
    // ─── IsFixedSize ───

    [Theory]
    [InlineData("Boolean")]
    [InlineData("Int32")]
    [InlineData("UInt64")]
    [InlineData("Single")]
    [InlineData("Double")]
    [InlineData("Guid")]
    [InlineData("EventRegistrationToken")]
    public void IsFixedSize_Primitive_ReturnsTrue(string typeName)
    {
        var typeRef = new TypeRef(typeName);
        Assert.True(WireFormat.IsFixedSize(typeRef));
    }

    [Fact]
    public void IsFixedSize_String_ReturnsFalse()
    {
        var typeRef = new TypeRef("String");
        Assert.False(WireFormat.IsFixedSize(typeRef));
    }

    [Fact]
    public void IsFixedSize_Enum_ReturnsTrue()
    {
        var enumSym = new EnumSymbol("Color", null);
        var typeRef = new TypeRef("Color") { ResolvedSymbol = enumSym };
        Assert.True(WireFormat.IsFixedSize(typeRef));
    }

    [Fact]
    public void IsFixedSize_BlittableStruct_ReturnsTrue()
    {
        var structSym = new StructSymbol("Point", null);
        structSym.AddField(new FieldSymbol("X", structSym) { Type = new TypeRef("Single") });
        structSym.AddField(new FieldSymbol("Y", structSym) { Type = new TypeRef("Single") });
        var typeRef = new TypeRef("Point") { ResolvedSymbol = structSym };

        Assert.True(WireFormat.IsFixedSize(typeRef));
    }

    [Fact]
    public void IsFixedSize_StructWithString_ReturnsFalse()
    {
        var structSym = new StructSymbol("NamedItem", null);
        structSym.AddField(new FieldSymbol("Id", structSym) { Type = new TypeRef("Int32") });
        structSym.AddField(new FieldSymbol("Name", structSym) { Type = new TypeRef("String") });
        var typeRef = new TypeRef("NamedItem") { ResolvedSymbol = structSym };

        Assert.False(WireFormat.IsFixedSize(typeRef));
    }

    // ─── IsBlittable ───

    [Fact]
    public void IsBlittable_AllPrimitiveFields_ReturnsTrue()
    {
        var structSym = new StructSymbol("Point", null);
        structSym.AddField(new FieldSymbol("X", structSym) { Type = new TypeRef("Single") });
        structSym.AddField(new FieldSymbol("Y", structSym) { Type = new TypeRef("Single") });

        Assert.True(WireFormat.IsBlittable(structSym));
    }

    [Fact]
    public void IsBlittable_WithStringField_ReturnsFalse()
    {
        var structSym = new StructSymbol("NamedItem", null);
        structSym.AddField(new FieldSymbol("Id", structSym) { Type = new TypeRef("Int32") });
        structSym.AddField(new FieldSymbol("Name", structSym) { Type = new TypeRef("String") });

        Assert.False(WireFormat.IsBlittable(structSym));
    }

    [Fact]
    public void IsBlittable_WithEnumField_ReturnsTrue()
    {
        // WinRT enums are always 4 bytes (Int32/UInt32) — blittable via explicit underlying type
        var enumSym = new EnumSymbol("Status", null);
        var structSym = new StructSymbol("Item", null);
        structSym.AddField(new FieldSymbol("Value", structSym) { Type = new TypeRef("Int32") });
        structSym.AddField(new FieldSymbol("State", structSym) { Type = new TypeRef("Status") { ResolvedSymbol = enumSym } });

        Assert.True(WireFormat.IsBlittable(structSym));
    }

    [Fact]
    public void IsBlittable_WithGuidField_ReturnsTrue()
    {
        var structSym = new StructSymbol("IdPair", null);
        structSym.AddField(new FieldSymbol("Id1", structSym) { Type = new TypeRef("Guid") });
        structSym.AddField(new FieldSymbol("Id2", structSym) { Type = new TypeRef("Guid") });

        Assert.True(WireFormat.IsBlittable(structSym));
    }

    [Fact]
    public void IsBlittable_NestedBlittableStruct_ReturnsTrue()
    {
        var inner = new StructSymbol("Point", null);
        inner.AddField(new FieldSymbol("X", inner) { Type = new TypeRef("Single") });
        inner.AddField(new FieldSymbol("Y", inner) { Type = new TypeRef("Single") });

        var outer = new StructSymbol("Rect", null);
        outer.AddField(new FieldSymbol("TopLeft", outer) { Type = new TypeRef("Point") { ResolvedSymbol = inner } });
        outer.AddField(new FieldSymbol("BottomRight", outer) { Type = new TypeRef("Point") { ResolvedSymbol = inner } });

        Assert.True(WireFormat.IsBlittable(outer));
    }

    [Fact]
    public void IsBlittable_EmptyStruct_ReturnsFalse()
    {
        var structSym = new StructSymbol("Empty", null);
        Assert.False(WireFormat.IsBlittable(structSym));
    }

    // ─── AllParamsFixedSize ───

    [Fact]
    public void AllParamsFixedSize_AllPrimitives_ReturnsTrue()
    {
        var parms = new List<ProxyParam>
        {
            new("x", new TypeRef("Single"), ParamDirection.In),
            new("y", new TypeRef("Single"), ParamDirection.In),
        };
        Assert.True(WireFormat.AllParamsFixedSize(parms));
    }

    [Fact]
    public void AllParamsFixedSize_WithString_ReturnsFalse()
    {
        var parms = new List<ProxyParam>
        {
            new("name", new TypeRef("String"), ParamDirection.In),
        };
        Assert.False(WireFormat.AllParamsFixedSize(parms));
    }

    [Fact]
    public void AllParamsFixedSize_Empty_ReturnsTrue()
    {
        Assert.True(WireFormat.AllParamsFixedSize([]));
    }

    // ─── EmitStructLayoutAsserts ───

    [Fact]
    public void EmitStructLayoutAsserts_EmitsStandardLayoutAssert()
    {
        var structSym = new StructSymbol("Point", null);
        structSym.AddField(new FieldSymbol("X", structSym) { Type = new TypeRef("Single") });
        structSym.AddField(new FieldSymbol("Y", structSym) { Type = new TypeRef("Single") });

        var sb = new StringBuilder();
        WireFormat.EmitStructLayoutAsserts(sb, structSym, "Point");
        var code = sb.ToString();

        Assert.Contains("static_assert(std::is_standard_layout_v<Point>)", code);
    }

    [Fact]
    public void EmitStructLayoutAsserts_EmitsOffsetAndSizeof()
    {
        var structSym = new StructSymbol("Point", null);
        structSym.AddField(new FieldSymbol("X", structSym) { Type = new TypeRef("Single") });
        structSym.AddField(new FieldSymbol("Y", structSym) { Type = new TypeRef("Single") });

        var sb = new StringBuilder();
        WireFormat.EmitStructLayoutAsserts(sb, structSym, "Point");
        var code = sb.ToString();

        Assert.Contains("static_assert(offsetof(Point, X) == 0)", code);
        Assert.Contains("static_assert(offsetof(Point, Y) == 4)", code);
        Assert.Contains("static_assert(sizeof(Point) == 8)", code);
    }

    [Fact]
    public void EmitStructLayoutAsserts_MultipleFieldTypes()
    {
        var structSym = new StructSymbol("Record", null);
        structSym.AddField(new FieldSymbol("Id", structSym) { Type = new TypeRef("Int32") });
        structSym.AddField(new FieldSymbol("Value", structSym) { Type = new TypeRef("Double") });

        var sb = new StringBuilder();
        WireFormat.EmitStructLayoutAsserts(sb, structSym, "Record");
        var code = sb.ToString();

        Assert.Contains("static_assert(std::is_standard_layout_v<Record>)", code);
        Assert.Contains("static_assert(offsetof(Record, Id) == 0)", code);
        Assert.Contains("static_assert(offsetof(Record, Value) == 4)", code);
        Assert.Contains("static_assert(sizeof(Record) == 12)", code);
    }

    // ─── Blittable struct serialization ───

    [Fact]
    public void EmitSerialize_BlittableStruct_UsesSingleMemcpy()
    {
        var structSym = new StructSymbol("Point", null);
        structSym.AddField(new FieldSymbol("X", structSym) { Type = new TypeRef("Single") });
        structSym.AddField(new FieldSymbol("Y", structSym) { Type = new TypeRef("Single") });
        var typeRef = new TypeRef("Point") { ResolvedSymbol = structSym };

        var sb = new StringBuilder();
        WireFormat.EmitSerialize(sb, typeRef, "pos", "p");
        var code = sb.ToString();

        // Should use marshal_struct helper, not field-by-field
        Assert.Contains("winmidl::marshal_struct(p, pos)", code);
        // Should NOT contain field-by-field copies
        Assert.DoesNotContain("pos.X", code);
        Assert.DoesNotContain("pos.Y", code);
    }

    [Fact]
    public void EmitSerialize_NonBlittableStruct_StillFieldByField()
    {
        // Struct with String field is non-blittable — must serialize field-by-field
        var structSym = new StructSymbol("NamedItem", null);
        structSym.AddField(new FieldSymbol("Id", structSym) { Type = new TypeRef("Int32") });
        structSym.AddField(new FieldSymbol("Name", structSym) { Type = new TypeRef("String") });
        var typeRef = new TypeRef("NamedItem") { ResolvedSymbol = structSym };

        var sb = new StringBuilder();
        WireFormat.EmitSerialize(sb, typeRef, "item", "p");
        var code = sb.ToString();

        // Non-blittable should still do field-by-field
        Assert.Contains("item.Id", code);
        Assert.Contains("item.Name", code);
        Assert.DoesNotContain("sizeof(NamedItem)", code);
    }

    [Fact]
    public void EmitDeserialize_BlittableStruct_UsesSingleMemcpy()
    {
        var structSym = new StructSymbol("Point", null);
        structSym.AddField(new FieldSymbol("X", structSym) { Type = new TypeRef("Single") });
        structSym.AddField(new FieldSymbol("Y", structSym) { Type = new TypeRef("Single") });
        var typeRef = new TypeRef("Point") { ResolvedSymbol = structSym };

        var sb = new StringBuilder();
        WireFormat.EmitDeserialize(sb, typeRef, "pos", "pReply");
        var code = sb.ToString();

        Assert.Contains("winmidl::unmarshal_struct(pReply, pos)", code);
    }

    // ─── Zero-copy HSTRING ───

    [Fact]
    public void EmitDeserializeHStringZeroCopy_UsesWindowsCreateStringReference()
    {
        var sb = new StringBuilder();
        WireFormat.EmitDeserializeHStringZeroCopy(sb, "name", "p");
        var code = sb.ToString();

        Assert.Contains("HSTRING_HEADER", code);
        Assert.Contains("WindowsCreateStringReference", code);
        Assert.DoesNotContain("WindowsCreateString(", code);
        Assert.Contains("name_len", code);
        Assert.Contains("reinterpret_cast<PCWSTR>(p)", code);
    }

    [Fact]
    public void EmitDeserializeHStringZeroCopy_AdvancesPointer()
    {
        var sb = new StringBuilder();
        WireFormat.EmitDeserializeHStringZeroCopy(sb, "str", "p");
        var code = sb.ToString();

        Assert.Contains("str_len * sizeof(WCHAR)", code);
    }

    // ─── ComputeFixedBufferSize ───

    [Fact]
    public void ComputeFixedBufferSize_AllFixed_ReturnsNumericValue()
    {
        var parms = new List<ProxyParam>
        {
            new("x", new TypeRef("Single"), ParamDirection.In),
            new("y", new TypeRef("Single"), ParamDirection.In),
        };
        var result = WireFormat.ComputeFixedBufferSize(parms);
        Assert.Equal(8, result);
    }

    [Fact]
    public void ComputeFixedBufferSize_Empty_ReturnsZero()
    {
        Assert.Equal(0, WireFormat.ComputeFixedBufferSize([]));
    }

    [Fact]
    public void ComputeFixedBufferSize_MixedTypes_ReturnsTotalSize()
    {
        var parms = new List<ProxyParam>
        {
            new("id", new TypeRef("Int32"), ParamDirection.In),
            new("value", new TypeRef("Double"), ParamDirection.In),
            new("flag", new TypeRef("Boolean"), ParamDirection.In),
        };
        // 4 + 8 + 1 = 13
        Assert.Equal(13, WireFormat.ComputeFixedBufferSize(parms));
    }
}
