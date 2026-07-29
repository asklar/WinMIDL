// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Quality sweep tests for WireFormat:
/// Issue 1: sizeof() expressions in constexpr sizes (no hardcoded numbers)
/// Issue 2: HSTRING wire format uses char_count only (no null terminator)
/// Issue 6: Async HSTRING safety detection
/// </summary>
public partial class WireFormatTests
{
    // ─── Issue 1: sizeof expressions for constexpr sizes ───

    [Fact]
    public void ComputeFixedBufferSizeExpression_SinglePrimitive_ReturnsSizeof()
    {
        var parms = new List<ProxyParam>
        {
            new("x", new TypeRef("Single"), ParamDirection.In),
        };
        var expr = WireFormat.ComputeFixedBufferSizeExpression(parms);
        Assert.Equal("sizeof(FLOAT)", expr);
    }

    [Fact]
    public void ComputeFixedBufferSizeExpression_TwoPrimitives_ReturnsSizeofSum()
    {
        var parms = new List<ProxyParam>
        {
            new("x", new TypeRef("Single"), ParamDirection.In),
            new("y", new TypeRef("Single"), ParamDirection.In),
        };
        var expr = WireFormat.ComputeFixedBufferSizeExpression(parms);
        Assert.Equal("sizeof(FLOAT) + sizeof(FLOAT)", expr);
    }

    [Fact]
    public void ComputeFixedBufferSizeExpression_Empty_ReturnsZero()
    {
        Assert.Equal("0", WireFormat.ComputeFixedBufferSizeExpression([]));
    }

    [Fact]
    public void ComputeFixedBufferSizeExpression_Enum_ReturnsSizeofINT32()
    {
        var enumSym = new EnumSymbol("Color", null);
        var parms = new List<ProxyParam>
        {
            new("c", new TypeRef("Color") { ResolvedSymbol = enumSym }, ParamDirection.In),
        };
        var expr = WireFormat.ComputeFixedBufferSizeExpression(parms);
        Assert.Equal("sizeof(INT32)", expr);
    }

    [Fact]
    public void ComputeFixedBufferSizeExpression_Struct_ReturnsSizeofStruct()
    {
        var structSym = new StructSymbol("Point", null);
        structSym.AddField(new FieldSymbol("X", structSym) { Type = new TypeRef("Single") });
        structSym.AddField(new FieldSymbol("Y", structSym) { Type = new TypeRef("Single") });
        var parms = new List<ProxyParam>
        {
            new("pos", new TypeRef("Point") { ResolvedSymbol = structSym }, ParamDirection.In),
        };
        var expr = WireFormat.ComputeFixedBufferSizeExpression(parms);
        // For a struct, sizeof(Point) is correct for the constexpr
        Assert.Contains("sizeof", expr);
        Assert.DoesNotMatch(@"^\d+$", expr); // Not a bare number
    }

    [Fact]
    public void ComputeFixedBufferSizeExpression_NoBareNumbers()
    {
        var parms = new List<ProxyParam>
        {
            new("a", new TypeRef("Int32"), ParamDirection.In),
            new("b", new TypeRef("Double"), ParamDirection.In),
            new("c", new TypeRef("Boolean"), ParamDirection.In),
        };
        var expr = WireFormat.ComputeFixedBufferSizeExpression(parms);
        // Should not contain any bare numbers (each part should be sizeof(...))
        Assert.DoesNotMatch(@"(?<!\w)\d+(?!\w)", expr); // No standalone numbers
        Assert.Contains("sizeof(INT32)", expr);
        Assert.Contains("sizeof(DOUBLE)", expr);
        Assert.Contains("sizeof(boolean)", expr);
    }

    // ─── Issue 2: HSTRING no null terminator ───

    [Fact]
    public void EmitSerializeHString_NoNullTerminator()
    {
        var sb = new StringBuilder();
        WireFormat.EmitSerializeHString(sb, "name", "p");
        var code = sb.ToString();

        // Wire format: [uint32_t char_count][wchar_t chars * char_count]
        Assert.Contains("name_len * sizeof(WCHAR)", code);
        Assert.DoesNotContain("+ 1)", code);
        Assert.DoesNotContain("+1)", code);
    }

    [Fact]
    public void EmitDeserializeHString_NoNullTerminator()
    {
        var sb = new StringBuilder();
        WireFormat.EmitDeserializeHString(sb, "name", "p");
        var code = sb.ToString();

        Assert.Contains("name_len * sizeof(WCHAR)", code);
        Assert.DoesNotContain("+ 1)", code);
    }

    [Fact]
    public void EmitSerializeHStringReply_NoNullTerminator()
    {
        var sb = new StringBuilder();
        WireFormat.EmitSerializeHStringReply(sb, "val", "p");
        var code = sb.ToString();

        Assert.Contains("val_len * sizeof(WCHAR)", code);
        Assert.DoesNotContain("+ 1)", code);
    }

    [Fact]
    public void EmitDeserializeReply_HString_NoNullTerminator()
    {
        var typeRef = new TypeRef("String");
        var sb = new StringBuilder();
        WireFormat.EmitDeserializeReply(sb, typeRef, "result", "pReply", isPointer: true);
        var code = sb.ToString();

        Assert.Contains("winmidl::unmarshal_hstring(pReply, result)", code);
        Assert.DoesNotContain("+ 1)", code);
    }

    [Fact]
    public void EmitDeserializeHStringZeroCopy_NoNullTerminator()
    {
        var sb = new StringBuilder();
        WireFormat.EmitDeserializeHStringZeroCopy(sb, "str", "p");
        var code = sb.ToString();

        Assert.Contains("str_len * sizeof(WCHAR)", code);
        Assert.DoesNotContain("+ 1)", code);
    }

    [Fact]
    public void GetWireSizeExpression_String_NoNullTerminator()
    {
        var typeRef = new TypeRef("String");
        var result = WireFormat.GetWireSizeExpression(typeRef);

        Assert.Contains("len * sizeof(WCHAR)", result);
        Assert.DoesNotContain("+ 1)", result);
    }

    [Fact]
    public void GetTypeSizeExpression_String_NoNullTerminator()
    {
        var typeRef = new TypeRef("String");
        var result = WireFormat.GetTypeSizeExpression(typeRef);

        Assert.Contains("sizeof(WCHAR)", result);
        Assert.DoesNotContain("+ 1)", result);
    }

    // ─── Issue 6: Async HSTRING safety ───

    [Fact]
    public void IsAsyncReturnType_IAsyncAction_ReturnsTrue()
    {
        var typeRef = new TypeRef("IAsyncAction");
        Assert.True(WireFormat.IsAsyncReturnType(typeRef));
    }

    [Fact]
    public void IsAsyncReturnType_IAsyncOperation_ReturnsTrue()
    {
        var typeRef = new TypeRef("IAsyncOperation", [new TypeRef("String")]);
        Assert.True(WireFormat.IsAsyncReturnType(typeRef));
    }

    [Fact]
    public void IsAsyncReturnType_IAsyncOperationWithProgress_ReturnsTrue()
    {
        var typeRef = new TypeRef("IAsyncOperationWithProgress", [new TypeRef("Int32"), new TypeRef("Double")]);
        Assert.True(WireFormat.IsAsyncReturnType(typeRef));
    }

    [Fact]
    public void IsAsyncReturnType_IAsyncActionWithProgress_ReturnsTrue()
    {
        var typeRef = new TypeRef("IAsyncActionWithProgress", [new TypeRef("Double")]);
        Assert.True(WireFormat.IsAsyncReturnType(typeRef));
    }

    [Fact]
    public void IsAsyncReturnType_RegularType_ReturnsFalse()
    {
        Assert.False(WireFormat.IsAsyncReturnType(new TypeRef("Int32")));
        Assert.False(WireFormat.IsAsyncReturnType(new TypeRef("String")));
        Assert.False(WireFormat.IsAsyncReturnType(TypeRef.Void));
    }

    [Fact]
    public void EmitDeserializeHStringAllocating_UsesWindowsCreateString()
    {
        var sb = new StringBuilder();
        WireFormat.EmitDeserializeHStringAllocating(sb, "name", "p");
        var code = sb.ToString();

        Assert.Contains("WindowsCreateString(", code);
        Assert.DoesNotContain("WindowsCreateStringReference", code);
        Assert.DoesNotContain("HSTRING_HEADER", code);
    }

    [Fact]
    public void EmitDeserializeHStringAllocating_ChecksHResult()
    {
        var sb = new StringBuilder();
        WireFormat.EmitDeserializeHStringAllocating(sb, "name", "p");
        var code = sb.ToString();

        Assert.Contains("FAILED(", code);
    }
}
