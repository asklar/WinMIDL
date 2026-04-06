using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Unit tests for ProxyGenerator helper methods — buffer size computation,
/// vtable slot calculation, and type size helpers.
/// </summary>
public partial class ProxyGeneratorTests
{
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
    public void GetPrimitiveSize_ReturnsCorrectSize(string typeName, int expectedSize)
    {
        Assert.Equal(expectedSize, ProxyGenerator.GetPrimitiveSize(typeName));
    }

    [Fact]
    public void GetPrimitiveSize_UnknownType_ReturnsZero()
    {
        Assert.Equal(0, ProxyGenerator.GetPrimitiveSize("CustomType"));
    }

    [Fact]
    public void GetTypeSize_Guid_Returns16()
    {
        var typeRef = new TypeRef("Guid");
        Assert.Equal("sizeof(GUID)", ProxyGenerator.GetTypeSize(typeRef));
    }

    [Fact]
    public void GetTypeSize_Enum_Returns4()
    {
        var enumSym = new EnumSymbol("Status", null);
        var typeRef = new TypeRef("Status") { ResolvedSymbol = enumSym };
        Assert.Equal("sizeof(INT32)", ProxyGenerator.GetTypeSize(typeRef));
    }

    [Fact]
    public void GetTypeSize_Struct_ReturnsSumOfFields()
    {
        var structSym = new StructSymbol("Point", null);
        var fx = new FieldSymbol("X", structSym) { Type = new TypeRef("Single") };
        var fy = new FieldSymbol("Y", structSym) { Type = new TypeRef("Single") };
        structSym.AddField(fx);
        structSym.AddField(fy);

        var typeRef = new TypeRef("Point") { ResolvedSymbol = structSym };
        Assert.Equal("sizeof(FLOAT) + sizeof(FLOAT)", ProxyGenerator.GetTypeSize(typeRef));
    }

    [Fact]
    public void GetTypeSize_String_ReturnsDynamicExpression()
    {
        var typeRef = new TypeRef("String");
        var result = ProxyGenerator.GetTypeSize(typeRef);
        Assert.Contains("winmidl::hstring_len", result);
        Assert.DoesNotContain("WindowsGetStringLen", result);
    }

    [Fact]
    public void ComputeBufferSize_NoParams_ReturnsZero()
    {
        var result = ProxyGenerator.ComputeBufferSize([]);
        Assert.Equal("0", result);
    }

    [Fact]
    public void ComputeBufferSize_SinglePrimitive_ReturnsSize()
    {
        var parms = new List<ProxyParam>
        {
            new("count", new TypeRef("Int32"), ParamDirection.In)
        };
        Assert.Equal("sizeof(INT32)", ProxyGenerator.ComputeBufferSize(parms));
    }

    [Fact]
    public void ComputeBufferSize_MultiplePrimitives_ReturnsSumExpression()
    {
        var parms = new List<ProxyParam>
        {
            new("x", new TypeRef("Single"), ParamDirection.In),
            new("y", new TypeRef("Single"), ParamDirection.In)
        };
        Assert.Equal("sizeof(FLOAT) + sizeof(FLOAT)", ProxyGenerator.ComputeBufferSize(parms));
    }

    [Fact]
    public void ComputeVtableSlot_FirstMethod_Returns6()
    {
        var iface = new InterfaceSymbol("IWidget", null);
        var method = new MethodSymbol("DoWork", iface);
        iface.AddMember(method);

        Assert.Equal(6, ProxyGenerator.ComputeVtableSlot(iface, "DoWork"));
    }

    [Fact]
    public void ComputeVtableSlot_AfterProperty_Correct()
    {
        var iface = new InterfaceSymbol("IWidget", null);
        var prop = new PropertySymbol("Name", iface) { HasGetter = true, HasSetter = true, Type = new TypeRef("String") };
        var method = new MethodSymbol("DoWork", iface);
        iface.AddMember(prop);
        iface.AddMember(method);

        // get_Name = 6, put_Name = 7, DoWork = 8
        Assert.Equal(6, ProxyGenerator.ComputeVtableSlot(iface, "get_Name"));
        Assert.Equal(7, ProxyGenerator.ComputeVtableSlot(iface, "put_Name"));
        Assert.Equal(8, ProxyGenerator.ComputeVtableSlot(iface, "DoWork"));
    }

    [Fact]
    public void ComputeVtableSlot_AfterEvent_Correct()
    {
        var iface = new InterfaceSymbol("IWidget", null);
        var evt = new EventSymbol("Click", iface) { DelegateType = new TypeRef("ClickHandler") };
        var method = new MethodSymbol("DoWork", iface);
        iface.AddMember(evt);
        iface.AddMember(method);

        // add_Click = 6, remove_Click = 7, DoWork = 8
        Assert.Equal(6, ProxyGenerator.ComputeVtableSlot(iface, "add_Click"));
        Assert.Equal(7, ProxyGenerator.ComputeVtableSlot(iface, "remove_Click"));
        Assert.Equal(8, ProxyGenerator.ComputeVtableSlot(iface, "DoWork"));
    }

    [Fact]
    public void ComputeVtableSlot_NotFound_ReturnsNegative()
    {
        var iface = new InterfaceSymbol("IWidget", null);
        var method = new MethodSymbol("DoWork", iface);
        iface.AddMember(method);

        Assert.Equal(-1, ProxyGenerator.ComputeVtableSlot(iface, "NonExistent"));
    }

    [Fact]
    public void CollectInterfaceMethods_PropertyGetterOnly()
    {
        var iface = new InterfaceSymbol("IWidget", null);
        var prop = new PropertySymbol("Name", iface) { HasGetter = true, HasSetter = false, Type = new TypeRef("String") };
        iface.AddMember(prop);

        var methods = ProxyGenerator.CollectInterfaceMethods(iface);

        Assert.Single(methods);
        Assert.Equal("get_Name", methods[0].Name);
        Assert.Equal(MemberKind.PropertyGetter, methods[0].Kind);
    }

    [Fact]
    public void CollectInterfaceMethods_PropertyGetterAndSetter()
    {
        var iface = new InterfaceSymbol("IWidget", null);
        var prop = new PropertySymbol("Name", iface) { HasGetter = true, HasSetter = true, Type = new TypeRef("String") };
        iface.AddMember(prop);

        var methods = ProxyGenerator.CollectInterfaceMethods(iface);

        Assert.Equal(2, methods.Count);
        Assert.Equal("get_Name", methods[0].Name);
        Assert.Equal("put_Name", methods[1].Name);
    }

    [Fact]
    public void CollectInterfaceMethods_Event_AddsAddAndRemove()
    {
        var iface = new InterfaceSymbol("IWidget", null);
        var evt = new EventSymbol("Click", iface) { DelegateType = new TypeRef("ClickHandler") };
        iface.AddMember(evt);

        var methods = ProxyGenerator.CollectInterfaceMethods(iface);

        Assert.Equal(2, methods.Count);
        Assert.Equal("add_Click", methods[0].Name);
        Assert.Equal(MemberKind.EventAdd, methods[0].Kind);
        Assert.Equal("remove_Click", methods[1].Name);
        Assert.Equal(MemberKind.EventRemove, methods[1].Kind);
    }

    [Fact]
    public void CollectInterfaceMethods_PreservesDeclarationOrder()
    {
        var iface = new InterfaceSymbol("IWidget", null);
        var m1 = new MethodSymbol("Alpha", iface);
        var m2 = new MethodSymbol("Beta", iface);
        var m3 = new MethodSymbol("Gamma", iface);
        iface.AddMember(m1);
        iface.AddMember(m2);
        iface.AddMember(m3);

        var methods = ProxyGenerator.CollectInterfaceMethods(iface);

        Assert.Equal(3, methods.Count);
        Assert.Equal("Alpha", methods[0].Name);
        Assert.Equal("Beta", methods[1].Name);
        Assert.Equal("Gamma", methods[2].Name);
    }

    [Fact]
    public void ComputeStructSize_EmptyStruct_ReturnsZero()
    {
        var structSym = new StructSymbol("Empty", null);
        Assert.Equal("0", ProxyGenerator.ComputeStructSize(structSym));
    }

    [Fact]
    public void ComputeStructSize_MixedFields()
    {
        var structSym = new StructSymbol("Rect", null);
        structSym.AddField(new FieldSymbol("X", structSym) { Type = new TypeRef("Single") });
        structSym.AddField(new FieldSymbol("Y", structSym) { Type = new TypeRef("Single") });
        structSym.AddField(new FieldSymbol("Width", structSym) { Type = new TypeRef("Double") });
        structSym.AddField(new FieldSymbol("Height", structSym) { Type = new TypeRef("Double") });

        // 4 + 4 + 8 + 8 = 24
        Assert.Equal("24", ProxyGenerator.ComputeStructSize(structSym));
    }
}
