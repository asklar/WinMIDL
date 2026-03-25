using Midl.Analysis;
using Midl.CodeGen.Header;

namespace Midl.Integration.Tests;

/// <summary>
/// Integration tests verifying C compatibility block emission (vtable structs, COBJMACROS)
/// against compiled IDL fixtures.
/// </summary>
public partial class HeaderParityTests
{
    [Fact]
    public void CBlock_BasicClass_HasVtableAndMacros()
    {
        var source = File.ReadAllText(Path.Combine(FixturesDir, "parity_basic_class.idl"));
        var compilation = CompilationHelper.Compile(source);
        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        // C forward declarations
        Assert.Contains("typedef interface __x_TestApp_CParity_CIBasicWidget __x_TestApp_CParity_CIBasicWidget;", cBlock);
        Assert.Contains("typedef interface __x_TestApp_CParity_CIBasicWidgetFactory __x_TestApp_CParity_CIBasicWidgetFactory;", cBlock);

        // Vtable struct
        Assert.Contains("typedef struct __x_TestApp_CParity_CIBasicWidgetVtbl", cBlock);
        Assert.Contains("BEGIN_INTERFACE", cBlock);
        Assert.Contains("END_INTERFACE", cBlock);

        // Base methods
        Assert.Contains("*QueryInterface", cBlock);
        Assert.Contains("*GetTrustLevel", cBlock);

        // User methods
        Assert.Contains("*GetName", cBlock);
        Assert.Contains("*Reset", cBlock);
        Assert.Contains("*Configure", cBlock);

        // Properties
        Assert.Contains("/* [propget] */", cBlock);
        Assert.Contains("*get_WidgetName", cBlock);
        Assert.Contains("*get_IsEnabled", cBlock);
        Assert.Contains("/* [propput] */", cBlock);
        Assert.Contains("*put_IsEnabled", cBlock);

        // Events
        Assert.Contains("/* [eventadd] */", cBlock);
        Assert.Contains("*add_Changed", cBlock);
        Assert.Contains("/* [eventremove] */", cBlock);
        Assert.Contains("*remove_Changed", cBlock);

        // Interface struct
        Assert.Contains("CONST_VTBL struct __x_TestApp_CParity_CIBasicWidgetVtbl *lpVtbl;", cBlock);

        // COBJMACROS
        Assert.Contains("#ifdef COBJMACROS", cBlock);
        Assert.Contains("#define __x_TestApp_CParity_CIBasicWidget_QueryInterface(This,riid,ppvObject)", cBlock);
        Assert.Contains("#define __x_TestApp_CParity_CIBasicWidget_GetName(This,result)", cBlock);
        Assert.Contains("#define __x_TestApp_CParity_CIBasicWidget_get_WidgetName(This,value)", cBlock);
        Assert.Contains("#define __x_TestApp_CParity_CIBasicWidget_add_Changed(This,handler,token)", cBlock);
        Assert.Contains("#endif /* COBJMACROS */", cBlock);
    }

    [Fact]
    public void CBlock_InterfaceOnly_MultipleInterfaces()
    {
        var source = File.ReadAllText(Path.Combine(FixturesDir, "parity_interface_only.idl"));
        var compilation = CompilationHelper.Compile(source);
        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        // Each interface gets its own vtable
        Assert.Contains("typedef struct __x_TestApp_CParity_CInterfaceOnly_CIReaderVtbl", cBlock);
        Assert.Contains("typedef struct __x_TestApp_CParity_CInterfaceOnly_CIWriterVtbl", cBlock);
        Assert.Contains("typedef struct __x_TestApp_CParity_CInterfaceOnly_CIReaderWriterVtbl", cBlock);

        // Each gets COBJMACROS
        Assert.Contains("#define __x_TestApp_CParity_CInterfaceOnly_CIReader_Read(This,result)", cBlock);
        Assert.Contains("#define __x_TestApp_CParity_CInterfaceOnly_CIWriter_Write(This,data)", cBlock);
        Assert.Contains("#define __x_TestApp_CParity_CInterfaceOnly_CIReaderWriter_Flush(This)", cBlock);
    }

    [Fact]
    public void CBlock_Delegates_HaveIUnknownNotIInspectable()
    {
        var source = File.ReadAllText(Path.Combine(FixturesDir, "parity_delegates.idl"));
        var compilation = CompilationHelper.Compile(source);
        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        // Delegates should have vtable structs
        Assert.Contains("typedef struct __x_TestApp_CParity_CSimpleCallbackVtbl", cBlock);
        Assert.Contains("typedef struct __x_TestApp_CParity_CMessageHandlerVtbl", cBlock);

        // Delegate COBJMACROS
        Assert.Contains("#define __x_TestApp_CParity_CSimpleCallback_Invoke(This)", cBlock);
        Assert.Contains("#define __x_TestApp_CParity_CMessageHandler_Invoke(This,message)", cBlock);

        // Delegate vtable should have Invoke
        Assert.Contains("*Invoke", cBlock);
    }

    [Fact]
    public void CBlock_EmptyInterface_OnlyBaseMethods()
    {
        var source = File.ReadAllText(Path.Combine(FixturesDir, "parity_empty_interface.idl"));
        var compilation = CompilationHelper.Compile(source);
        var header = _emitter.Emit(compilation);
        var cBlock = ExtractCBlock(header);

        // Empty interface IMarker still gets a vtable with base methods
        Assert.Contains("typedef struct __x_TestApp_CParity_CIMarkerVtbl", cBlock);
        Assert.Contains("*QueryInterface", cBlock);
        Assert.Contains("*GetTrustLevel", cBlock);
        Assert.Contains("BEGIN_INTERFACE", cBlock);
        Assert.Contains("END_INTERFACE", cBlock);
    }

    [Fact]
    public void CBlock_MatchesGolden_BasicClass()
    {
        AssertHeaderMatchesGolden("parity_basic_class");
    }

    [Fact]
    public void CBlock_MatchesGolden_InterfaceOnly()
    {
        AssertHeaderMatchesGolden("parity_interface_only");
    }

    [Fact]
    public void CBlock_MatchesGolden_Delegates()
    {
        AssertHeaderMatchesGolden("parity_delegates");
    }

    [Fact]
    public void CBlock_MatchesGolden_EmptyInterface()
    {
        AssertHeaderMatchesGolden("parity_empty_interface");
    }

    /// <summary>
    /// Extracts the C block from a header (between #else and the matching #endif).
    /// </summary>
    private static string ExtractCBlock(string header)
    {
        var elseMarker = "#else // !defined(__cplusplus)";
        var elseIdx = header.IndexOf(elseMarker, StringComparison.Ordinal);
        if (elseIdx < 0) return "";

        var endifMarker = "#endif // defined(__cplusplus) && !defined(CINTERFACE)";
        var endifIdx = header.IndexOf(endifMarker, elseIdx, StringComparison.Ordinal);
        if (endifIdx < 0) return header[elseIdx..];

        return header[elseIdx..endifIdx];
    }
}
