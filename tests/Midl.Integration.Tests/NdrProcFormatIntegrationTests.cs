using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Stub;

namespace Midl.Integration.Tests;

/// <summary>
/// Integration tests for NdrProcFormatBuilder — compiles IDL source and verifies
/// procedure format string generation end-to-end.
/// </summary>
public partial class NdrProcFormatIntegrationTests
{
    private static Compilation Compile(string source) =>
        CompilationHelper.Compile(source, CompilationMode.Compat);

    private static void AssertNoErrors(Compilation compilation)
    {
        var errors = compilation.Diagnostics
            .Where(d => d.Severity == Midl.Analysis.Diagnostics.DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void SimpleMethod_WithOutRetval_ProducesProcFormatString()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(12345678-1234-1234-1234-123456789012)]
                interface ISimple : IInspectable
                {
                    HRESULT GetValue([out, retval] Int32* result);
                }
            }
            """);

        AssertNoErrors(compilation);

        var typeBuilder = new NdrTypeFormatBuilder();
        var procBuilder = new NdrProcFormatBuilder();

        var iface = compilation.GetAllTypes().OfType<InterfaceSymbol>()
            .First(i => i.Name == "ISimple");
        var method = iface.Members.OfType<MethodSymbol>().First();

        // IInspectable-derived → first custom method is slot 6
        int offset = procBuilder.AddMethod(method, 6, typeBuilder);

        var bytes = procBuilder.Build();

        // Header checks
        Assert.Equal(0x33, bytes[0]); // FC_AUTO_HANDLE
        Assert.Equal(0x6c, bytes[1]); // Old Oi flags

        // Method number = 6
        short methodNum = (short)(bytes[6] | (bytes[7] << 8));
        Assert.Equal(6, methodNum);

        // Param count = 2 (out retval + HRESULT return)
        Assert.Equal(2, bytes[15]);

        // Stack size = this(8) + result(8) + return(8) = 24
        short stackSize = (short)(bytes[8] | (bytes[9] << 8));
        Assert.Equal(0x18, stackSize);
    }

    [Fact]
    public void MethodWithInHString_ProducesClientMustSizeFlag()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(AABBCCDD-1122-3344-5566-778899AABBCC)]
                interface INameProvider : IInspectable
                {
                    HRESULT SetName([in] String name);
                }
            }
            """);

        AssertNoErrors(compilation);

        var typeBuilder = new NdrTypeFormatBuilder();
        var procBuilder = new NdrProcFormatBuilder();

        var iface = compilation.GetAllTypes().OfType<InterfaceSymbol>()
            .First(i => i.Name == "INameProvider");
        var method = iface.Members.OfType<MethodSymbol>().First();

        procBuilder.AddMethod(method, 6, typeBuilder);
        var bytes = procBuilder.Build();

        // Oi2 flags: should include ClientMustSize (0x02)
        byte oi2 = bytes[14];
        Assert.True((oi2 & 0x02) != 0, "ClientMustSize should be set for [in] HSTRING");
        Assert.True((oi2 & 0x04) != 0, "HasReturn should be set");
    }

    [Fact]
    public void MethodWithOutInterfacePointer_ProducesServerMustSize()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(11111111-2222-3333-4444-555555555555)]
                interface IWidget : IInspectable
                {
                    HRESULT DoSomething();
                }

                [uuid(AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE)]
                interface IWidgetFactory : IInspectable
                {
                    HRESULT CreateWidget([out, retval] IWidget** widget);
                }
            }
            """);

        AssertNoErrors(compilation);

        var typeBuilder = new NdrTypeFormatBuilder();
        var procBuilder = new NdrProcFormatBuilder();

        var factory = compilation.GetAllTypes().OfType<InterfaceSymbol>()
            .First(i => i.Name == "IWidgetFactory");
        var method = factory.Members.OfType<MethodSymbol>().First();

        procBuilder.AddMethod(method, 6, typeBuilder);
        var bytes = procBuilder.Build();

        // Oi2 flags should include ServerMustSize
        byte oi2 = bytes[14];
        Assert.True((oi2 & 0x01) != 0, "ServerMustSize should be set for [out] interface pointer");
    }

    [Fact]
    public void MultipleMethodInterface_AllMethodsEncoded()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(12345678-ABCD-EF01-2345-678901234567)]
                interface ICalc : IInspectable
                {
                    HRESULT Add([in] Int32 a, [in] Int32 b, [out, retval] Int32* result);
                    HRESULT GetName([out, retval] String* name);
                }
            }
            """);

        AssertNoErrors(compilation);

        var typeBuilder = new NdrTypeFormatBuilder();
        var procBuilder = new NdrProcFormatBuilder();

        var iface = compilation.GetAllTypes().OfType<InterfaceSymbol>()
            .First(i => i.Name == "ICalc");
        var methods = iface.Members.OfType<MethodSymbol>().ToList();

        int offset1 = procBuilder.AddMethod(methods[0], 6, typeBuilder);
        int offset2 = procBuilder.AddMethod(methods[1], 7, typeBuilder);

        Assert.Equal(0, offset1);
        Assert.True(offset2 > offset1);

        var bytes = procBuilder.Build();
        Assert.True(bytes.Length > 50, "Should have encoded at least two method descriptors");
    }

    [Fact]
    public void StubEmitter_EmitsProcFormatString()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(12345678-ABCD-EF01-2345-678901234567)]
                interface ISimple : IInspectable
                {
                    HRESULT GetValue([out, retval] Int32* result);
                }
            }
            """);

        AssertNoErrors(compilation);

        var emitter = new StubEmitter();
        string output = emitter.Emit(compilation);

        // The StubEmitter should now include both type and proc format strings
        Assert.Contains("MIDL_TYPE_FORMAT_STRING", output);
        Assert.Contains("MIDL_PROC_FORMAT_STRING", output);
    }

    [Fact]
    public void EmitProcFormatString_ProducesValidCSource()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(12345678-ABCD-EF01-2345-678901234567)]
                interface ISimple : IInspectable
                {
                    HRESULT GetValue([out, retval] Int32* result);
                    HRESULT SetName([in] String name);
                }
            }
            """);

        AssertNoErrors(compilation);

        var typeBuilder = new NdrTypeFormatBuilder();
        var procBuilder = new NdrProcFormatBuilder();

        var iface = compilation.GetAllTypes().OfType<InterfaceSymbol>()
            .First(i => i.Name == "ISimple");

        int methodNum = 6;
        foreach (var method in iface.Members.OfType<MethodSymbol>())
        {
            procBuilder.AddMethod(method, methodNum++, typeBuilder);
        }

        string output = procBuilder.EmitProcFormatString("test");

        Assert.Contains("test_MIDL_PROC_FORMAT_STRING", output);
        Assert.Contains("test__MIDL_ProcFormatString", output);
        Assert.Contains("0x33", output); // FC_AUTO_HANDLE
        Assert.Contains("0x0", output);  // terminator
    }

    [Fact]
    public void MethodWithMultipleMixedParams_EncodedCorrectly()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(A1B2C3D4-E5F6-7890-ABCD-EF0123456789)]
                interface IWidget : IInspectable
                {
                    HRESULT Configure([in] String name, [in] Int32 count, [in] Boolean enabled);
                }
            }
            """);

        AssertNoErrors(compilation);

        var typeBuilder = new NdrTypeFormatBuilder();
        var procBuilder = new NdrProcFormatBuilder();

        var iface = compilation.GetAllTypes().OfType<InterfaceSymbol>()
            .First(i => i.Name == "IWidget");
        var method = iface.Members.OfType<MethodSymbol>().First();

        procBuilder.AddMethod(method, 12, typeBuilder);
        var bytes = procBuilder.Build();

        // Method number = 12
        short methodNum = (short)(bytes[6] | (bytes[7] << 8));
        Assert.Equal(12, methodNum);

        // Param count = 4 (name, count, enabled, HRESULT)
        Assert.Equal(4, bytes[15]);

        // Stack size: this(8) + name(8) + count(8) + enabled(8) + return(8) = 40 = 0x28
        short stackSize = (short)(bytes[8] | (bytes[9] << 8));
        Assert.Equal(0x28, stackSize);
    }
}
