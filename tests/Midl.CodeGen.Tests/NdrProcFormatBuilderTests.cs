using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Stub;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Unit tests for NdrProcFormatBuilder — NDR procedure format string generation.
/// Uses partial class to allow splitting tests across files.
/// </summary>
public partial class NdrProcFormatBuilderTests
{
    private readonly NdrProcFormatBuilder _proc = new();
    private readonly NdrTypeFormatBuilder _types = new();

    /// <summary>Helper to create an IInspectable-based interface with a UUID.</summary>
    private static InterfaceSymbol MakeInterface(string name, string uuid = "12345678-1234-1234-1234-123456789012")
    {
        var iface = new InterfaceSymbol(name, null) { Uuid = new Guid(uuid) };
        iface.RequiredInterfaces.Add(new TypeRef("IInspectable"));
        return iface;
    }

    /// <summary>Helper to create a method with given params (all on an interface).</summary>
    private static MethodSymbol MakeMethod(string name, InterfaceSymbol iface, params ParameterSymbol[] parms)
    {
        var method = new MethodSymbol(name, iface)
        {
            ReturnType = new TypeRef("HRESULT"),
        };
        foreach (var p in parms)
            method.Parameters.Add(p);
        return method;
    }

    private static ParameterSymbol InParam(string name, string typeName, MethodSymbol? parent = null)
    {
        return new ParameterSymbol(name, parent)
        {
            Type = new TypeRef(typeName),
            Direction = ParameterDirection.In,
        };
    }

    private static ParameterSymbol OutParam(string name, string typeName, MethodSymbol? parent = null)
    {
        var p = new ParameterSymbol(name, parent)
        {
            Type = new TypeRef(typeName),
            Direction = ParameterDirection.Out,
        };
        p.Attributes.Add(new AttributeData { Name = "retval" });
        return p;
    }

    #region Basic Method Encoding

    [Fact]
    public void AddMethod_ReturnsNonNegativeOffset()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("DoWork", iface);

        int offset = _proc.AddMethod(method, 6, _types);
        Assert.True(offset >= 0);
    }

    [Fact]
    public void AddMethod_VoidMethodWithNoParams_HeaderHasCorrectHandleAndFlags()
    {
        var iface = MakeInterface("IFoo");
        // Void method with no user params — only HRESULT return
        var method = MakeMethod("DoWork", iface);

        _proc.AddMethod(method, 6, _types);
        var bytes = _proc.Build();

        // Byte 0: FC_AUTO_HANDLE (0x33)
        Assert.Equal(0x33, bytes[0]);
        // Byte 1: Old Oi flags (0x6c = object | Oi2)
        Assert.Equal(0x6c, bytes[1]);
    }

    [Fact]
    public void AddMethod_VoidMethod_RpcFlagsAreZero()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("DoWork", iface);

        _proc.AddMethod(method, 6, _types);
        var bytes = _proc.Build();

        // Bytes 2-5: RPC flags = 0x00000000
        Assert.Equal(0x00, bytes[2]);
        Assert.Equal(0x00, bytes[3]);
        Assert.Equal(0x00, bytes[4]);
        Assert.Equal(0x00, bytes[5]);
    }

    [Fact]
    public void AddMethod_MethodNumber_EncodedCorrectly()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("DoWork", iface);

        _proc.AddMethod(method, 9, _types);
        var bytes = _proc.Build();

        // Bytes 6-7: method number as little-endian short
        short methodNum = (short)(bytes[6] | (bytes[7] << 8));
        Assert.Equal(9, methodNum);
    }

    [Fact]
    public void AddMethod_VoidMethod_StackSize_IncludesThisAndReturn()
    {
        var iface = MakeInterface("IFoo");
        // No user params => this(8) + return HRESULT(8) = 16 = 0x10
        var method = MakeMethod("DoWork", iface);

        _proc.AddMethod(method, 6, _types);
        var bytes = _proc.Build();

        // Bytes 8-9: stack size
        short stackSize = (short)(bytes[8] | (bytes[9] << 8));
        Assert.Equal(0x10, stackSize);
    }

    [Fact]
    public void AddMethod_VoidMethod_ParamCount_IsOneForReturn()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("DoWork", iface);

        _proc.AddMethod(method, 6, _types);
        var bytes = _proc.Build();

        // Byte 15: param_count = 1 (just HRESULT return, 'this' is not counted)
        Assert.Equal(1, bytes[15]);
    }

    [Fact]
    public void AddMethod_VoidMethod_Oi2Flags_HasReturnAndExtensions()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("DoWork", iface);

        _proc.AddMethod(method, 6, _types);
        var bytes = _proc.Build();

        // Byte 14: Oi2 flags = 0x44 (HasReturn | HasExtensions)
        Assert.Equal(0x44, bytes[14]);
    }

    [Fact]
    public void AddMethod_VoidMethod_ExtensionBlock()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("DoWork", iface);

        _proc.AddMethod(method, 6, _types);
        var bytes = _proc.Build();

        // Byte 16: extension size = 0x0a (10)
        Assert.Equal(0x0a, bytes[16]);
        // Byte 17: extension flags = 0x01 (HasNewCorrDesc)
        Assert.Equal(0x01, bytes[17]);
    }

    #endregion

    #region HRESULT Return Encoding

    [Fact]
    public void AddMethod_VoidMethod_HresultReturn_EncodedAfterHeader()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("DoWork", iface);

        _proc.AddMethod(method, 6, _types);
        var bytes = _proc.Build();

        // After 26-byte header, the first (and only) param descriptor is HRESULT return
        // Param flags at offset 26-27: 0x0070 (IsOut | IsReturn | IsBasetype)
        short flags = (short)(bytes[26] | (bytes[27] << 8));
        Assert.Equal(0x0070, flags);

        // Stack offset at 28-29: last slot = 0x08 (this=0, return=8)
        short stackOffset = (short)(bytes[28] | (bytes[29] << 8));
        Assert.Equal(0x08, stackOffset);

        // FC code at 30: FC_LONG (0x08)
        Assert.Equal(FC.FC_LONG, bytes[30]);
        // Padding at 31: 0x00
        Assert.Equal(0x00, bytes[31]);
    }

    #endregion

    #region In Base Type Parameters

    [Fact]
    public void AddMethod_WithInInt32Param_ParamCountIs2()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("SetValue", iface, InParam("value", "Int32"));

        _proc.AddMethod(method, 6, _types);
        var bytes = _proc.Build();

        // Param count = 2 (Int32 param + HRESULT return)
        Assert.Equal(2, bytes[15]);
    }

    [Fact]
    public void AddMethod_WithInInt32Param_StackSizeCorrect()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("SetValue", iface, InParam("value", "Int32"));

        _proc.AddMethod(method, 6, _types);
        var bytes = _proc.Build();

        // Stack size: this(8) + value(8) + return(8) = 24 = 0x18
        short stackSize = (short)(bytes[8] | (bytes[9] << 8));
        Assert.Equal(0x18, stackSize);
    }

    [Fact]
    public void AddMethod_WithInInt32Param_ParamFlags_InBasetype()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("SetValue", iface, InParam("value", "Int32"));

        _proc.AddMethod(method, 6, _types);
        var bytes = _proc.Build();

        // First param at offset 26: flags = 0x0048 (IsIn | IsBasetype)
        short flags = (short)(bytes[26] | (bytes[27] << 8));
        Assert.Equal(0x0048, flags);

        // Stack offset = 0x08 (after this)
        short stackOff = (short)(bytes[28] | (bytes[29] << 8));
        Assert.Equal(0x08, stackOff);

        // FC_LONG
        Assert.Equal(FC.FC_LONG, bytes[30]);
    }

    [Fact]
    public void AddMethod_WithInBooleanParam_UsesSmall()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("SetEnabled", iface, InParam("enabled", "Boolean"));

        _proc.AddMethod(method, 6, _types);
        var bytes = _proc.Build();

        // Param at offset 26: FC_SMALL for Boolean
        Assert.Equal(FC.FC_SMALL, bytes[30]);
    }

    [Fact]
    public void AddMethod_WithInDoubleParam_UsesDouble()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("SetValue", iface, InParam("val", "Double"));

        _proc.AddMethod(method, 6, _types);
        var bytes = _proc.Build();

        Assert.Equal(FC.FC_DOUBLE, bytes[30]);
    }

    #endregion

    #region Out Base Type Parameters

    [Fact]
    public void AddMethod_WithOutInt32Retval_ParamFlagsCorrect()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("GetCount", iface, OutParam("count", "Int32"));

        _proc.AddMethod(method, 6, _types);
        var bytes = _proc.Build();

        // First param at offset 26: [out] Int32* = 0x2150
        short flags = (short)(bytes[26] | (bytes[27] << 8));
        Assert.Equal(0x2150, flags);

        // FC_LONG for the base type
        Assert.Equal(FC.FC_LONG, bytes[30]);
    }

    [Fact]
    public void AddMethod_WithOutRetval_ReturnDescHasIsReturn()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("GetCount", iface, OutParam("count", "Int32"));

        _proc.AddMethod(method, 6, _types);
        var bytes = _proc.Build();

        // HRESULT return at offset 32
        short flags = (short)(bytes[32] | (bytes[33] << 8));
        Assert.Equal(0x0070, flags);
    }

    #endregion

    #region Multiple Methods

    [Fact]
    public void AddMethod_MultipleMethodsGetSequentialOffsets()
    {
        var iface = MakeInterface("IFoo");
        var m1 = MakeMethod("GetA", iface, OutParam("a", "Int32"));
        var m2 = MakeMethod("GetB", iface, OutParam("b", "Int32"));

        int offset1 = _proc.AddMethod(m1, 6, _types);
        int offset2 = _proc.AddMethod(m2, 7, _types);

        Assert.Equal(0, offset1);
        Assert.True(offset2 > offset1);
    }

    #endregion

    #region Property Encoding

    [Fact]
    public void AddProperty_GetterOnly_GeneratesOneMethod()
    {
        var iface = MakeInterface("IFoo");
        var prop = new PropertySymbol("Count", iface)
        {
            Type = new TypeRef("Int32"),
            HasGetter = true,
            HasSetter = false,
        };

        int methodNumber = 6;
        int offset = _proc.AddProperty(prop, ref methodNumber, _types);

        Assert.True(offset >= 0);
        Assert.Equal(7, methodNumber); // consumed one method slot
    }

    [Fact]
    public void AddProperty_GetterAndSetter_GeneratesTwoMethods()
    {
        var iface = MakeInterface("IFoo");
        var prop = new PropertySymbol("Count", iface)
        {
            Type = new TypeRef("Int32"),
            HasGetter = true,
            HasSetter = true,
        };

        int methodNumber = 6;
        _proc.AddProperty(prop, ref methodNumber, _types);

        Assert.Equal(8, methodNumber); // consumed two method slots
    }

    [Fact]
    public void AddProperty_Getter_HasOutRetvalParam()
    {
        var iface = MakeInterface("IFoo");
        var prop = new PropertySymbol("Count", iface)
        {
            Type = new TypeRef("Int32"),
            HasGetter = true,
            HasSetter = false,
        };

        int methodNumber = 6;
        _proc.AddProperty(prop, ref methodNumber, _types);
        var bytes = _proc.Build();

        // Param count = 2 (out retval + HRESULT return)
        Assert.Equal(2, bytes[15]);
    }

    [Fact]
    public void AddProperty_Setter_HasInParam()
    {
        var iface = MakeInterface("IFoo");
        var prop = new PropertySymbol("Enabled", iface)
        {
            Type = new TypeRef("Boolean"),
            HasGetter = false,
            HasSetter = true,
        };

        int methodNumber = 6;
        _proc.AddProperty(prop, ref methodNumber, _types);
        var bytes = _proc.Build();

        // Param count = 2 (in param + HRESULT return)
        Assert.Equal(2, bytes[15]);

        // First param should be [in] basetype
        short flags = (short)(bytes[26] | (bytes[27] << 8));
        Assert.Equal(0x0048, flags);
    }

    #endregion

    #region Event Encoding

    [Fact]
    public void AddEvent_GeneratesTwoMethods_AddAndRemove()
    {
        var iface = MakeInterface("IFoo");
        var delegateType = new TypeRef("SomeHandler")
        {
            ResolvedSymbol = new InterfaceSymbol("SomeHandler", null)
            {
                Uuid = new Guid("AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE"),
            }
        };
        var evt = new EventSymbol("Clicked", iface) { DelegateType = delegateType };

        int methodNumber = 6;
        _proc.AddEvent(evt, ref methodNumber, _types);

        Assert.Equal(8, methodNumber); // add_ + remove_ = 2 method slots
    }

    [Fact]
    public void AddEvent_AddMethod_HasInInterfacePointerParam()
    {
        var iface = MakeInterface("IFoo");
        var delegateType = new TypeRef("SomeHandler")
        {
            ResolvedSymbol = new InterfaceSymbol("SomeHandler", null)
            {
                Uuid = new Guid("AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE"),
            }
        };
        var evt = new EventSymbol("Clicked", iface) { DelegateType = delegateType };

        int methodNumber = 6;
        _proc.AddEvent(evt, ref methodNumber, _types);
        var bytes = _proc.Build();

        // First method (add_): params = handler [in] + token [out,retval] + HRESULT = 3
        Assert.Equal(3, bytes[15]);
    }

    #endregion

    #region EmitProcFormatString Output

    [Fact]
    public void EmitProcFormatString_ContainsStructDecl()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("DoWork", iface);
        _proc.AddMethod(method, 6, _types);

        string output = _proc.EmitProcFormatString("test_file");

        Assert.Contains("test_file_MIDL_PROC_FORMAT_STRING", output);
        Assert.Contains("test_file__MIDL_ProcFormatString", output);
    }

    [Fact]
    public void EmitProcFormatString_ContainsPadField()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("DoWork", iface);
        _proc.AddMethod(method, 6, _types);

        string output = _proc.EmitProcFormatString("test_file");
        // Proc format string struct has a Pad field initialized to 0
        Assert.Contains("0,", output);
    }

    [Fact]
    public void EmitProcFormatString_ContainsFcAutoHandle()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("DoWork", iface);
        _proc.AddMethod(method, 6, _types);

        string output = _proc.EmitProcFormatString("test_file");
        Assert.Contains("0x33", output); // FC_AUTO_HANDLE
    }

    [Fact]
    public void EmitProcFormatString_ContainsTerminator()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("DoWork", iface);
        _proc.AddMethod(method, 6, _types);

        string output = _proc.EmitProcFormatString("test_file");
        // Last line before closing brace should be terminator 0x0
        Assert.Contains("0x0", output);
    }

    #endregion

    #region Complex Parameter Types

    [Fact]
    public void AddMethod_WithInHString_Oi2Flags_HasClientMustSize()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("SetName", iface, InParam("name", "String"));

        _proc.AddMethod(method, 6, _types);
        var bytes = _proc.Build();

        // Byte 14: Oi2 flags should include ClientMustSize (0x02)
        byte oi2Flags = bytes[14];
        Assert.True((oi2Flags & 0x02) != 0, "ClientMustSize should be set for [in] HSTRING");
        Assert.True((oi2Flags & 0x04) != 0, "HasReturn should be set");
        Assert.True((oi2Flags & 0x40) != 0, "HasExtensions should be set");
    }

    [Fact]
    public void AddMethod_WithOutHString_Oi2Flags_HasServerMustSize()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("GetName", iface, OutParam("name", "String"));

        _proc.AddMethod(method, 6, _types);
        var bytes = _proc.Build();

        // Byte 14: Oi2 flags should include ServerMustSize (0x01)
        byte oi2Flags = bytes[14];
        Assert.True((oi2Flags & 0x01) != 0, "ServerMustSize should be set for [out] HSTRING");
    }

    [Fact]
    public void AddMethod_WithInHString_ParamFlags()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("SetName", iface, InParam("name", "String"));

        _proc.AddMethod(method, 6, _types);
        var bytes = _proc.Build();

        // [in] HSTRING by value: 0x008b (MustSize | MustFree | IsIn | IsByValue)
        short flags = (short)(bytes[26] | (bytes[27] << 8));
        Assert.Equal(0x008b, flags);
    }

    [Fact]
    public void AddMethod_WithOutHString_ParamFlags()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("GetName", iface, OutParam("name", "String"));

        _proc.AddMethod(method, 6, _types);
        var bytes = _proc.Build();

        // [out] HSTRING*: 0x2113 (MustSize | MustFree | IsOut | IsSimpleRef | ServerAllocSize=8)
        short flags = (short)(bytes[26] | (bytes[27] << 8));
        Assert.Equal(0x2113, flags);
    }

    [Fact]
    public void AddMethod_WithOutInterfacePointer_ParamFlags()
    {
        var iface = MakeInterface("IFoo");
        var widgetIface = new InterfaceSymbol("IWidget", null)
        {
            Uuid = new Guid("11111111-2222-3333-4444-555555555555"),
        };
        var outParam = new ParameterSymbol("widget", null)
        {
            Type = new TypeRef("IWidget") { ResolvedSymbol = widgetIface },
            Direction = ParameterDirection.Out,
        };
        outParam.Attributes.Add(new AttributeData { Name = "retval" });
        var method = MakeMethod("GetWidget", iface, outParam);

        _proc.AddMethod(method, 6, _types);
        var bytes = _proc.Build();

        // [out] IWidget**: 0x0013 (MustSize | MustFree | IsOut)
        short flags = (short)(bytes[26] | (bytes[27] << 8));
        Assert.Equal(0x0013, flags);
    }

    [Fact]
    public void AddMethod_WithInGuid_ParamFlags()
    {
        var iface = MakeInterface("IFoo");
        var guidParam = new ParameterSymbol("id", null)
        {
            Type = new TypeRef("Guid"),
            Direction = ParameterDirection.In,
        };
        var method = MakeMethod("SetGuid", iface, guidParam);

        _proc.AddMethod(method, 6, _types);
        var bytes = _proc.Build();

        // [in] GUID: 0x010a (MustFree | IsIn | IsSimpleRef)
        short flags = (short)(bytes[26] | (bytes[27] << 8));
        Assert.Equal(0x010a, flags);
    }

    #endregion

    #region Multiple Parameters

    [Fact]
    public void AddMethod_MultipleInParams_StackOffsetsIncrement()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("Add", iface,
            InParam("a", "Int32"),
            InParam("b", "Int32"),
            OutParam("result", "Int32"));

        _proc.AddMethod(method, 3, _types);
        var bytes = _proc.Build();

        // Param count = 4 (a, b, result, HRESULT)
        Assert.Equal(4, bytes[15]);

        // Stack size: this(8) + a(8) + b(8) + result(8) + return(8) = 40 = 0x28
        short stackSize = (short)(bytes[8] | (bytes[9] << 8));
        Assert.Equal(0x28, stackSize);

        // Param 'a' at offset 26: stack_offset = 0x08
        short aStackOff = (short)(bytes[28] | (bytes[29] << 8));
        Assert.Equal(0x08, aStackOff);

        // Param 'b' at offset 32: stack_offset = 0x10
        short bStackOff = (short)(bytes[34] | (bytes[35] << 8));
        Assert.Equal(0x10, bStackOff);

        // Param 'result' at offset 38: stack_offset = 0x18
        short resultStackOff = (short)(bytes[40] | (bytes[41] << 8));
        Assert.Equal(0x18, resultStackOff);

        // HRESULT return at offset 44: stack_offset = 0x20
        short retStackOff = (short)(bytes[46] | (bytes[47] << 8));
        Assert.Equal(0x20, retStackOff);
    }

    #endregion

    #region Buffer Size Hints

    [Fact]
    public void AddMethod_WithInInt32_ClientBufferSizeNonZero()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("SetValue", iface, InParam("val", "Int32"));

        _proc.AddMethod(method, 6, _types);
        var bytes = _proc.Build();

        // Client buffer size at bytes 10-11 should account for [in] Int32
        short clientBuf = (short)(bytes[10] | (bytes[11] << 8));
        Assert.True(clientBuf > 0, "Client buffer size should be > 0 for [in] base type param");
    }

    [Fact]
    public void AddMethod_WithOutInt32_ServerBufferSizeNonZero()
    {
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("GetCount", iface, OutParam("count", "Int32"));

        _proc.AddMethod(method, 6, _types);
        var bytes = _proc.Build();

        // Server buffer size at bytes 12-13 should account for [out] Int32 + HRESULT return
        short serverBuf = (short)(bytes[12] | (bytes[13] << 8));
        Assert.True(serverBuf > 0, "Server buffer size should be > 0 for [out] base type param");
    }

    #endregion
}
