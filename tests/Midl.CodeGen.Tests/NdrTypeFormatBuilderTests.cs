using Midl.Analysis.Symbols;
using Midl.CodeGen.Stub;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Unit tests for NdrTypeFormatBuilder — NDR type format string generation.
/// Uses partial class to allow splitting tests across files.
/// </summary>
public partial class NdrTypeFormatBuilderTests
{
    private readonly NdrTypeFormatBuilder _builder = new();

    #region Primitive Format Character Tests

    [Theory]
    [InlineData("UInt8", FC.FC_BYTE)]
    [InlineData("byte", FC.FC_BYTE)]
    [InlineData("Boolean", FC.FC_SMALL)]
    [InlineData("boolean", FC.FC_SMALL)]
    [InlineData("Char16", FC.FC_WCHAR)]
    [InlineData("wchar_t", FC.FC_WCHAR)]
    [InlineData("char", FC.FC_WCHAR)]
    [InlineData("Int16", FC.FC_SHORT)]
    [InlineData("short", FC.FC_SHORT)]
    [InlineData("UInt16", FC.FC_USHORT)]
    [InlineData("Int32", FC.FC_LONG)]
    [InlineData("int", FC.FC_LONG)]
    [InlineData("long", FC.FC_LONG)]
    [InlineData("HRESULT", FC.FC_LONG)]
    [InlineData("UInt32", FC.FC_ULONG)]
    [InlineData("Single", FC.FC_FLOAT)]
    [InlineData("float", FC.FC_FLOAT)]
    [InlineData("Int64", FC.FC_HYPER)]
    [InlineData("UInt64", FC.FC_HYPER)]
    [InlineData("hyper", FC.FC_HYPER)]
    [InlineData("Double", FC.FC_DOUBLE)]
    [InlineData("double", FC.FC_DOUBLE)]
    [InlineData("error_status_t", FC.FC_ERROR_STATUS_T)]
    public void GetPrimitiveFormatChar_ReturnsCorrectCode(string typeName, byte expected)
    {
        var result = NdrTypeFormatBuilder.GetPrimitiveFormatChar(typeName);
        Assert.NotNull(result);
        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("String")]
    [InlineData("Guid")]
    [InlineData("Object")]
    [InlineData("IFoo")]
    [InlineData("MyStruct")]
    public void GetPrimitiveFormatChar_ReturnsNull_ForNonPrimitives(string typeName)
    {
        Assert.Null(NdrTypeFormatBuilder.GetPrimitiveFormatChar(typeName));
    }

    #endregion

    #region GetOrAddType — Primitives return -1

    [Theory]
    [InlineData("Int32")]
    [InlineData("Boolean")]
    [InlineData("Double")]
    [InlineData("UInt8")]
    public void GetOrAddType_ReturnsMinus1_ForPrimitiveTypes(string typeName)
    {
        var typeRef = new TypeRef(typeName);
        int offset = _builder.GetOrAddType(typeRef);
        Assert.Equal(-1, offset);
    }

    #endregion

    #region Build — Initial State

    [Fact]
    public void Build_EmptyBuilder_ReturnsInitialPaddingAndTerminator()
    {
        var bytes = _builder.Build();

        // Should have: 0x00, 0x00 (NdrFcShort padding) + 0x00 (terminator)
        Assert.Equal(3, bytes.Length);
        Assert.Equal(0x00, bytes[0]);
        Assert.Equal(0x00, bytes[1]);
        Assert.Equal(0x00, bytes[2]);
    }

    #endregion

    #region HSTRING Type Encoding

    [Fact]
    public void GetOrAddType_String_ReturnsNonNegativeOffset()
    {
        var typeRef = new TypeRef("String");
        int offset = _builder.GetOrAddType(typeRef);

        Assert.True(offset >= 2, "HSTRING offset should be >= 2 (after initial padding)");
    }

    [Fact]
    public void GetOrAddType_String_EmitsUserMarshalEntry()
    {
        var typeRef = new TypeRef("String");
        int offset = _builder.GetOrAddType(typeRef);
        var bytes = _builder.Build();

        // The FC_USER_MARSHAL entry should start at the returned offset
        Assert.Equal(FC.FC_USER_MARSHAL, bytes[offset]);
        Assert.Equal(0x83, bytes[offset + 1]); // flags: unique | alignment 3
    }

    [Fact]
    public void GetOrAddType_String_EmitsCStructWireRepresentation()
    {
        var typeRef = new TypeRef("String");
        _builder.GetOrAddType(typeRef);
        var bytes = _builder.Build();

        // Wire representation should contain FC_OP, FC_CARRAY, FC_CSTRUCT
        Assert.Contains(FC.FC_OP, bytes);
        Assert.Contains(FC.FC_CARRAY, bytes);
        Assert.Contains(FC.FC_CSTRUCT, bytes);
    }

    [Fact]
    public void GetOrAddType_String_Deduplicates()
    {
        var typeRef1 = new TypeRef("String");
        var typeRef2 = new TypeRef("String");
        int offset1 = _builder.GetOrAddType(typeRef1);
        int offset2 = _builder.GetOrAddType(typeRef2);

        Assert.Equal(offset1, offset2);
    }

    #endregion

    #region Interface Pointer Encoding

    [Fact]
    public void GetOrAddType_Object_EmitsInterfacePointer()
    {
        var typeRef = new TypeRef("Object");
        int offset = _builder.GetOrAddType(typeRef);
        var bytes = _builder.Build();

        Assert.True(offset >= 2);
        Assert.Equal(FC.FC_IP, bytes[offset]);
        Assert.Equal(FC.FC_CONSTANT_IID, bytes[offset + 1]);
    }

    [Fact]
    public void GetOrAddType_Object_EmitsIInspectableIid()
    {
        var typeRef = new TypeRef("Object");
        int offset = _builder.GetOrAddType(typeRef);
        var bytes = _builder.Build();

        // IInspectable IID: {AF86E2E0-B12D-4C6A-9C5A-D7AA65101E90}
        // After FC_IP(1) + FC_CONSTANT_IID(1), the next 16 bytes are the IID
        var iidBytes = bytes.AsSpan(offset + 2, 16);
        var iid = new Guid(iidBytes);
        Assert.Equal(new Guid("AF86E2E0-B12D-4C6A-9C5A-D7AA65101E90"), iid);
    }

    [Fact]
    public void GetOrAddType_ResolvedInterface_EmitsCorrectIid()
    {
        var expectedGuid = new Guid("12345678-ABCD-EF01-2345-678901234567");
        var iface = new InterfaceSymbol("IMyInterface", null) { Uuid = expectedGuid };
        var typeRef = new TypeRef("IMyInterface") { ResolvedSymbol = iface };

        int offset = _builder.GetOrAddType(typeRef);
        var bytes = _builder.Build();

        Assert.Equal(FC.FC_IP, bytes[offset]);
        Assert.Equal(FC.FC_CONSTANT_IID, bytes[offset + 1]);

        var iidBytes = bytes.AsSpan(offset + 2, 16);
        var iid = new Guid(iidBytes);
        Assert.Equal(expectedGuid, iid);
    }

    [Fact]
    public void GetOrAddType_Interface_Deduplicates()
    {
        var iface = new InterfaceSymbol("IFoo", null)
        {
            Uuid = new Guid("11111111-2222-3333-4444-555555555555")
        };
        var typeRef1 = new TypeRef("IFoo") { ResolvedSymbol = iface };
        var typeRef2 = new TypeRef("IFoo") { ResolvedSymbol = iface };

        int offset1 = _builder.GetOrAddType(typeRef1);
        int offset2 = _builder.GetOrAddType(typeRef2);

        Assert.Equal(offset1, offset2);
    }

    #endregion

    #region GUID Struct Encoding

    [Fact]
    public void GetOrAddType_Guid_EmitsStructAndSmfarray()
    {
        var typeRef = new TypeRef("Guid");
        int offset = _builder.GetOrAddType(typeRef);
        var bytes = _builder.Build();

        Assert.True(offset >= 2);

        // GUID struct should be FC_STRUCT
        Assert.Equal(FC.FC_STRUCT, bytes[offset]);
        Assert.Equal(0x03, bytes[offset + 1]); // alignment 3 (4-byte)
        // struct size = 16 (0x10) as little-endian short
        Assert.Equal(0x10, bytes[offset + 2]);
        Assert.Equal(0x00, bytes[offset + 3]);
    }

    [Fact]
    public void GetOrAddType_Guid_ContainsSmfarray()
    {
        var typeRef = new TypeRef("Guid");
        _builder.GetOrAddType(typeRef);
        var bytes = _builder.Build();

        // Should contain FC_SMFARRAY for Data4[8]
        Assert.Contains(FC.FC_SMFARRAY, bytes);
    }

    [Fact]
    public void GetOrAddType_Guid_ContainsFieldLayout()
    {
        var typeRef = new TypeRef("Guid");
        int offset = _builder.GetOrAddType(typeRef);
        var bytes = _builder.Build();

        // After FC_STRUCT(1) + alignment(1) + size(2), the fields are:
        // FC_LONG (Data1), FC_SHORT (Data2), FC_SHORT (Data3), FC_EMBEDDED_COMPLEX (Data4)
        Assert.Equal(FC.FC_LONG, bytes[offset + 4]);    // Data1
        Assert.Equal(FC.FC_SHORT, bytes[offset + 5]);   // Data2
        Assert.Equal(FC.FC_SHORT, bytes[offset + 6]);   // Data3
        Assert.Equal(FC.FC_EMBEDDED_COMPLEX, bytes[offset + 7]); // Data4
    }

    [Fact]
    public void GetOrAddType_Guid_Deduplicates()
    {
        var t1 = new TypeRef("Guid");
        var t2 = new TypeRef("Guid");

        int o1 = _builder.GetOrAddType(t1);
        int o2 = _builder.GetOrAddType(t2);

        Assert.Equal(o1, o2);
    }

    #endregion

    #region Enum Encoding

    [Fact]
    public void GetOrAddType_Enum_ReturnsMinus1()
    {
        var enumSym = new EnumSymbol("MyEnum", null);
        var typeRef = new TypeRef("MyEnum") { ResolvedSymbol = enumSym };

        int offset = _builder.GetOrAddType(typeRef);
        Assert.Equal(-1, offset);
    }

    #endregion

    #region Simple Struct Encoding

    [Fact]
    public void GetOrAddType_SimpleStruct_EmitsFcStruct()
    {
        var structSym = new StructSymbol("MyStruct", null);
        var f1 = new FieldSymbol("x", structSym) { Type = new TypeRef("Int32") };
        var f2 = new FieldSymbol("y", structSym) { Type = new TypeRef("Int32") };
        structSym.AddField(f1);
        structSym.AddField(f2);

        var typeRef = new TypeRef("MyStruct") { ResolvedSymbol = structSym };
        int offset = _builder.GetOrAddType(typeRef);
        var bytes = _builder.Build();

        Assert.True(offset >= 2);
        Assert.Equal(FC.FC_STRUCT, bytes[offset]);
        Assert.Equal(0x03, bytes[offset + 1]); // 4-byte alignment for Int32 fields
    }

    [Fact]
    public void GetOrAddType_SimpleStruct_CorrectSize()
    {
        var structSym = new StructSymbol("TwoInts", null);
        structSym.AddField(new FieldSymbol("a", structSym) { Type = new TypeRef("Int32") });
        structSym.AddField(new FieldSymbol("b", structSym) { Type = new TypeRef("Int32") });

        var typeRef = new TypeRef("TwoInts") { ResolvedSymbol = structSym };
        int offset = _builder.GetOrAddType(typeRef);
        var bytes = _builder.Build();

        // Struct size = 8 (two Int32s)
        short size = (short)(bytes[offset + 2] | (bytes[offset + 3] << 8));
        Assert.Equal(8, size);
    }

    [Fact]
    public void GetOrAddType_SimpleStruct_ContainsFieldTypes()
    {
        var structSym = new StructSymbol("IntShort", null);
        structSym.AddField(new FieldSymbol("a", structSym) { Type = new TypeRef("Int32") });
        structSym.AddField(new FieldSymbol("b", structSym) { Type = new TypeRef("Int16") });

        var typeRef = new TypeRef("IntShort") { ResolvedSymbol = structSym };
        int offset = _builder.GetOrAddType(typeRef);
        var bytes = _builder.Build();

        // After header (4 bytes): FC_LONG, FC_SHORT
        Assert.Equal(FC.FC_LONG, bytes[offset + 4]);
        Assert.Equal(FC.FC_SHORT, bytes[offset + 5]);
    }

    [Fact]
    public void GetOrAddType_SimpleStruct_EndsWithFcEnd()
    {
        var structSym = new StructSymbol("OneField", null);
        structSym.AddField(new FieldSymbol("x", structSym) { Type = new TypeRef("Int32") });

        var typeRef = new TypeRef("OneField") { ResolvedSymbol = structSym };
        int offset = _builder.GetOrAddType(typeRef);
        var bytes = _builder.Build();

        // Find FC_END after the field layout
        bool foundEnd = false;
        for (int i = offset + 4; i < bytes.Length; i++)
        {
            if (bytes[i] == FC.FC_END)
            {
                foundEnd = true;
                break;
            }
        }
        Assert.True(foundEnd, "Struct encoding should end with FC_END");
    }

    [Fact]
    public void GetOrAddType_SimpleStruct_Deduplicates()
    {
        var structSym = new StructSymbol("S", null);
        structSym.AddField(new FieldSymbol("x", structSym) { Type = new TypeRef("Int32") });

        var t1 = new TypeRef("S") { ResolvedSymbol = structSym };
        var t2 = new TypeRef("S") { ResolvedSymbol = structSym };

        int o1 = _builder.GetOrAddType(t1);
        int o2 = _builder.GetOrAddType(t2);

        Assert.Equal(o1, o2);
    }

    #endregion

    #region Reference Pointer Encoding

    [Fact]
    public void EmitRefPointerToBaseType_EmitsFcRpSimplePointer()
    {
        int offset = _builder.EmitRefPointerToBaseType(FC.FC_LONG);
        var bytes = _builder.Build();

        Assert.True(offset >= 2);
        Assert.Equal(FC.FC_RP, bytes[offset]);
        Assert.Equal(0x0C, bytes[offset + 1]); // alloced_on_stack | simple_pointer
        Assert.Equal(FC.FC_LONG, bytes[offset + 2]);
        Assert.Equal(FC.FC_PAD, bytes[offset + 3]);
    }

    [Theory]
    [InlineData(FC.FC_LONG)]
    [InlineData(FC.FC_SMALL)]
    [InlineData(FC.FC_DOUBLE)]
    [InlineData(FC.FC_HYPER)]
    public void EmitRefPointerToBaseType_Deduplicates(byte fc)
    {
        int o1 = _builder.EmitRefPointerToBaseType(fc);
        int o2 = _builder.EmitRefPointerToBaseType(fc);
        Assert.Equal(o1, o2);
    }

    [Fact]
    public void EmitRefPointerDeref_EmitsFcRpPointerDeref()
    {
        var iface = new InterfaceSymbol("IWidget", null)
        {
            Uuid = new Guid("AABBCCDD-1122-3344-5566-778899AABBCC")
        };
        var typeRef = new TypeRef("IWidget") { ResolvedSymbol = iface };

        int offset = _builder.EmitRefPointerDeref(typeRef);
        var bytes = _builder.Build();

        Assert.True(offset >= 2);
        Assert.Equal(FC.FC_RP, bytes[offset]);
        Assert.Equal(FC.POINTER_DEREF, bytes[offset + 1]); // 0x10
    }

    #endregion

    #region EmitTypeFormatString C source output

    [Fact]
    public void EmitTypeFormatString_ContainsStructDecl()
    {
        string output = _builder.EmitTypeFormatString("test_file");
        Assert.Contains("test_file_MIDL_TYPE_FORMAT_STRING", output);
        Assert.Contains("test_file__MIDL_TypeFormatString", output);
    }

    [Fact]
    public void EmitTypeFormatString_ContainsNdrFcShortPadding()
    {
        string output = _builder.EmitTypeFormatString("test_file");
        Assert.Contains("NdrFcShort( 0x0 )", output);
    }

    [Fact]
    public void EmitTypeFormatString_ContainsTerminator()
    {
        string output = _builder.EmitTypeFormatString("test_file");
        Assert.Contains("0x0", output);
    }

    [Fact]
    public void EmitTypeFormatString_IncludesTypeEntries()
    {
        // Add an HSTRING type to ensure there are entries
        _builder.GetOrAddType(new TypeRef("String"));
        string output = _builder.EmitTypeFormatString("myfile");

        Assert.Contains("FC_USER_MARSHAL", output);
        Assert.Contains("FC_CSTRUCT", output);
    }

    #endregion

    #region CurrentOffset

    [Fact]
    public void CurrentOffset_StartsAt2_AfterInitialPadding()
    {
        Assert.Equal(2, _builder.CurrentOffset);
    }

    [Fact]
    public void CurrentOffset_IncreasesAfterAddingTypes()
    {
        int before = _builder.CurrentOffset;
        _builder.GetOrAddType(new TypeRef("String"));
        int after = _builder.CurrentOffset;

        Assert.True(after > before, "Offset should increase after adding a complex type");
    }

    #endregion
}
