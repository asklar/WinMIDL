using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Stub;

namespace Midl.Integration.Tests;

/// <summary>
/// Integration tests for NdrTypeFormatBuilder — compiles IDL source and verifies
/// type format string generation end-to-end.
/// </summary>
public partial class NdrTypeFormatIntegrationTests
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
    public void SimpleInterface_WithInt32OutParam_GeneratesRefPointer()
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

        var builder = new NdrTypeFormatBuilder();

        // Find the interface and its method
        var iface = compilation.GetAllTypes().OfType<InterfaceSymbol>().First();
        var method = iface.Members.OfType<MethodSymbol>().First();

        // The [out] Int32* parameter needs an FC_RP [simple_pointer] entry
        // In compat mode, [out] is stored as an attribute, not Direction
        var outParam = method.Parameters.First(p =>
            p.Direction == ParameterDirection.Out ||
            p.Attributes.Any(a => a.Name == "out"));
        var fc = NdrTypeFormatBuilder.GetPrimitiveFormatChar(outParam.Type.Name);
        Assert.NotNull(fc);

        int offset = builder.EmitRefPointerToBaseType(fc!.Value);
        var bytes = builder.Build();

        Assert.Equal(FC.FC_RP, bytes[offset]);
        Assert.Equal(0x0C, bytes[offset + 1]); // alloced_on_stack | simple_pointer
        Assert.Equal(FC.FC_LONG, bytes[offset + 2]);
    }

    [Fact]
    public void Interface_WithHStringParam_GeneratesUserMarshal()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(AABBCCDD-1122-3344-5566-778899AABBCC)]
                interface INameProvider : IInspectable
                {
                    HRESULT GetName([out, retval] String* name);
                }
            }
            """);

        AssertNoErrors(compilation);

        var builder = new NdrTypeFormatBuilder();
        var hstringRef = new TypeRef("String");
        int offset = builder.GetOrAddType(hstringRef);

        var bytes = builder.Build();

        // Should have FC_USER_MARSHAL at the offset
        Assert.Equal(FC.FC_USER_MARSHAL, bytes[offset]);
        Assert.Equal(0x83, bytes[offset + 1]);

        // Should also contain wire representation (FC_CSTRUCT, FC_CARRAY)
        Assert.Contains(FC.FC_CSTRUCT, bytes);
        Assert.Contains(FC.FC_CARRAY, bytes);
    }

    [Fact]
    public void Interface_WithInterfaceOutParam_GeneratesInterfacePointer()
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

        var builder = new NdrTypeFormatBuilder();

        // Find IWidgetFactory and its CreateWidget method
        var factory = compilation.GetAllTypes().OfType<InterfaceSymbol>()
            .First(i => i.Name == "IWidgetFactory");
        var method = factory.Members.OfType<MethodSymbol>().First();
        var outParam = method.Parameters.First(p =>
            p.Direction == ParameterDirection.Out ||
            p.Attributes.Any(a => a.Name == "out"));

        // The parameter type should be IWidget
        int offset = builder.GetOrAddType(outParam.Type);
        var bytes = builder.Build();

        // Should have FC_IP + FC_CONSTANT_IID
        Assert.Equal(FC.FC_IP, bytes[offset]);
        Assert.Equal(FC.FC_CONSTANT_IID, bytes[offset + 1]);

        // The IID should be 11111111-2222-3333-4444-555555555555
        var iidBytes = bytes.AsSpan(offset + 2, 16);
        var iid = new Guid(iidBytes);
        Assert.Equal(new Guid("11111111-2222-3333-4444-555555555555"), iid);
    }

    [Fact]
    public void Interface_WithStructParam_GeneratesStructType()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                struct Point
                {
                    Int32 X;
                    Int32 Y;
                };

                [uuid(12345678-ABCD-EF01-2345-678901234567)]
                interface IPointProvider : IInspectable
                {
                    HRESULT GetPoint([out, retval] Point* point);
                }
            }
            """);

        AssertNoErrors(compilation);

        var builder = new NdrTypeFormatBuilder();

        // Find the struct and create a TypeRef for it
        var structSym = compilation.GetAllTypes().OfType<StructSymbol>().First();
        var typeRef = new TypeRef("Point") { ResolvedSymbol = structSym };

        int offset = builder.GetOrAddType(typeRef);
        var bytes = builder.Build();

        Assert.Equal(FC.FC_STRUCT, bytes[offset]);
        Assert.Equal(0x03, bytes[offset + 1]); // 4-byte alignment

        // Size should be 8 (two Int32s)
        short size = (short)(bytes[offset + 2] | (bytes[offset + 3] << 8));
        Assert.Equal(8, size);

        // Fields: FC_LONG, FC_LONG
        Assert.Equal(FC.FC_LONG, bytes[offset + 4]);
        Assert.Equal(FC.FC_LONG, bytes[offset + 5]);
    }

    [Fact]
    public void Interface_WithEnumParam_TreatedAsBaseType()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                enum Color
                {
                    Red,
                    Green,
                    Blue
                };

                [uuid(12345678-ABCD-EF01-2345-678901234567)]
                interface IColorProvider : IInspectable
                {
                    HRESULT SetColor([in] Color c);
                }
            }
            """);

        AssertNoErrors(compilation);

        var builder = new NdrTypeFormatBuilder();

        var enumSym = compilation.GetAllTypes().OfType<EnumSymbol>().First();
        var typeRef = new TypeRef("Color") { ResolvedSymbol = enumSym };

        int offset = builder.GetOrAddType(typeRef);

        // Enums are base types, so offset should be -1
        Assert.Equal(-1, offset);
    }

    [Fact]
    public void EmitTypeFormatString_ProducesValidCSource()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(12345678-ABCD-EF01-2345-678901234567)]
                interface ISimple : IInspectable
                {
                    HRESULT GetValue([out, retval] Int32* result);
                    HRESULT GetName([out, retval] String* name);
                }
            }
            """);

        AssertNoErrors(compilation);

        var builder = new NdrTypeFormatBuilder();

        // Add types used by the interface
        builder.EmitRefPointerToBaseType(FC.FC_LONG);
        builder.GetOrAddType(new TypeRef("String"));

        string output = builder.EmitTypeFormatString("test");

        // Verify structural elements
        Assert.Contains("test_MIDL_TYPE_FORMAT_STRING", output);
        Assert.Contains("test__MIDL_TypeFormatString", output);
        Assert.Contains("NdrFcShort( 0x0 )", output);
        Assert.Contains("FC_RP", output);
        Assert.Contains("FC_USER_MARSHAL", output);
        Assert.Contains("0x0", output); // terminator
    }

    [Fact]
    public void MultipleTypes_AllEncodedInSingleFormatString()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                struct Size
                {
                    Int32 Width;
                    Int32 Height;
                };

                [uuid(11111111-2222-3333-4444-555555555555)]
                interface IWidget : IInspectable
                {
                    HRESULT GetSize([out, retval] Size* size);
                    HRESULT GetName([out, retval] String* name);
                    HRESULT GetCount([out, retval] Int32* count);
                }
            }
            """);

        AssertNoErrors(compilation);

        var builder = new NdrTypeFormatBuilder();

        // Add all types used by the interface methods
        var structSym = compilation.GetAllTypes().OfType<StructSymbol>().First();
        var structRef = new TypeRef("Size") { ResolvedSymbol = structSym };

        int structOffset = builder.GetOrAddType(structRef);
        int hstringOffset = builder.GetOrAddType(new TypeRef("String"));
        int rpOffset = builder.EmitRefPointerToBaseType(FC.FC_LONG);

        // All offsets should be distinct and valid
        Assert.True(structOffset >= 2);
        Assert.True(hstringOffset >= 2);
        Assert.True(rpOffset >= 2);
        Assert.NotEqual(structOffset, hstringOffset);
        Assert.NotEqual(structOffset, rpOffset);
        Assert.NotEqual(hstringOffset, rpOffset);

        var bytes = builder.Build();

        // Verify all type markers are present
        Assert.Contains(FC.FC_STRUCT, bytes);
        Assert.Contains(FC.FC_USER_MARSHAL, bytes);
        Assert.Contains(FC.FC_RP, bytes);
    }

    [Fact]
    public void StubEmitter_UsesNdrTypeFormatBuilder()
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

        // The StubEmitter should now include type format string content
        Assert.Contains("MIDL_TYPE_FORMAT_STRING", output);
    }

    [Fact]
    public void GuidParam_GeneratesGuidStructEncoding()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(12345678-ABCD-EF01-2345-678901234567)]
                interface IGuidUser : IInspectable
                {
                    HRESULT SetId([in] Guid id);
                }
            }
            """);

        AssertNoErrors(compilation);

        var builder = new NdrTypeFormatBuilder();
        var guidRef = new TypeRef("Guid");
        int offset = builder.GetOrAddType(guidRef);
        var bytes = builder.Build();

        // GUID struct should be encoded
        Assert.Equal(FC.FC_STRUCT, bytes[offset]);

        // Struct size should be 16
        short size = (short)(bytes[offset + 2] | (bytes[offset + 3] << 8));
        Assert.Equal(16, size);

        // Should contain FC_SMFARRAY for Data4 bytes
        Assert.Contains(FC.FC_SMFARRAY, bytes);
    }
}
