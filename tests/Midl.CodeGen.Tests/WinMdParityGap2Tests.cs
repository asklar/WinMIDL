using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Midl.Analysis;
using Midl.CodeGen.WinMd;

namespace Midl.CodeGen.Tests;

/// <summary>
/// TDD tests for WinMD parity gaps round 2:
///   Gap 1: Interface GUID must use ABI signature seed with correct base UUID
///   Gap 2: EventHandler TypeRef must use System.EventHandler (CLR projection)
///   Gap 3: Static-only class must get IXxxStatics, NOT IXxx
/// </summary>
public class WinMdParityGap2Tests
{
    private static byte[] CompileAndEmit(string source)
    {
        var compilation = CompilationHelper.Compile(source);
        Assert.Empty(compilation.Diagnostics);
        var emitter = new WinMdEmitter();
        return emitter.EmitToBytes(compilation);
    }

    private static MetadataReader CreateReader(byte[] bytes, out PEReader peReader)
    {
        peReader = new PEReader(new MemoryStream(bytes));
        return peReader.GetMetadataReader();
    }

    private static TypeDefinitionHandle FindTypeDef(MetadataReader reader, string name)
    {
        foreach (var handle in reader.TypeDefinitions)
        {
            var td = reader.GetTypeDefinition(handle);
            if (reader.GetString(td.Name) == name)
                return handle;
        }
        throw new Exception($"TypeDef '{name}' not found");
    }

    private static TypeDefinitionHandle? TryFindTypeDef(MetadataReader reader, string name)
    {
        foreach (var handle in reader.TypeDefinitions)
        {
            var td = reader.GetTypeDefinition(handle);
            if (reader.GetString(td.Name) == name)
                return handle;
        }
        return null;
    }

    private static Guid? GetGuidAttributeValue(MetadataReader reader, TypeDefinitionHandle typeDef)
    {
        foreach (var attrHandle in reader.GetCustomAttributes(typeDef))
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            if (attr.Constructor.Kind != HandleKind.MemberReference) continue;
            var memberRef = reader.GetMemberReference((MemberReferenceHandle)attr.Constructor);
            if (memberRef.Parent.Kind != HandleKind.TypeReference) continue;
            var typeRef = reader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
            if (reader.GetString(typeRef.Name) != "GuidAttribute") continue;

            var blob = reader.GetBlobReader(attr.Value);
            blob.ReadUInt16(); // prolog
            uint a = blob.ReadUInt32();
            ushort b = blob.ReadUInt16();
            ushort c = blob.ReadUInt16();
            byte[] d = new byte[8];
            for (int i = 0; i < 8; i++) d[i] = blob.ReadByte();
            return new Guid((int)a, (short)b, (short)c, d);
        }
        return null;
    }

    private static bool HasCustomAttribute(MetadataReader reader, EntityHandle parent, string attrTypeName)
    {
        foreach (var attrHandle in reader.GetCustomAttributes(parent))
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            if (attr.Constructor.Kind == HandleKind.MemberReference)
            {
                var memberRef = reader.GetMemberReference((MemberReferenceHandle)attr.Constructor);
                if (memberRef.Parent.Kind == HandleKind.TypeReference)
                {
                    var typeRef = reader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
                    if (reader.GetString(typeRef.Name) == attrTypeName)
                        return true;
                }
            }
        }
        return false;
    }

    #region Gap 1: Interface GUID must use ABI signature seed

    [Fact]
    public void SynthesizedInterface_Guid_MatchesOriginalMidl_SimpleMethod()
    {
        // namespace N { runtimeclass Foo { Foo(); void M(); }; }
        // Seed: "N.IFoo:HRESULT M();"
        // Base UUID: e72a134c-baf7-4dd3-b542-77848e87b138
        // Expected: 0ad721ed-b6cd-5695-9112-ba995bc583cd (verified against midl.exe)
        var bytes = CompileAndEmit("""
            namespace N
            {
                runtimeclass Foo
                {
                    Foo();
                    void M();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "IFoo");
            var guid = GetGuidAttributeValue(reader, handle);
            Assert.NotNull(guid);
            Assert.Equal(new Guid("0ad721ed-b6cd-5695-9112-ba995bc583cd"), guid.Value);
        }
    }

    [Fact]
    public void SynthesizedInterface_Guid_MethodWithReturnValue()
    {
        // void M() → ABI seed: "HRESULT M();"
        // String GetName() → ABI seed: "HRESULT GetName(String*);"
        // Seed: "Test.IWidget:HRESULT GetName(String*);"
        var bytes = CompileAndEmit("""
            namespace Test
            {
                runtimeclass Widget
                {
                    Widget();
                    String GetName();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "IWidget");
            var guid = GetGuidAttributeValue(reader, handle);
            Assert.NotNull(guid);

            // Compute expected using correct algorithm
            var expected = ComputeInterfaceGuid("Test.IWidget:HRESULT GetName(String*);");
            Assert.Equal(expected, guid.Value);
        }
    }

    [Fact]
    public void SynthesizedInterface_Guid_MethodWithParams()
    {
        // Int32 Add(Int32 a, Int32 b)
        // ABI: "HRESULT Add(Int32,Int32,Int32*);"
        var bytes = CompileAndEmit("""
            namespace Test
            {
                runtimeclass Calc
                {
                    Calc();
                    Int32 Add(Int32 a, Int32 b);
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "ICalc");
            var guid = GetGuidAttributeValue(reader, handle);
            Assert.NotNull(guid);

            var expected = ComputeInterfaceGuid("Test.ICalc:HRESULT Add(Int32,Int32,Int32*);");
            Assert.Equal(expected, guid.Value);
        }
    }

    [Fact]
    public void SynthesizedInterface_Guid_WithProperty()
    {
        // String Name { get; set; }
        // ABI: "HRESULT get_Name(String*);HRESULT put_Name(String);"
        var bytes = CompileAndEmit("""
            namespace Test
            {
                runtimeclass Widget
                {
                    Widget();
                    String Name { get; set; };
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "IWidget");
            var guid = GetGuidAttributeValue(reader, handle);
            Assert.NotNull(guid);

            var expected = ComputeInterfaceGuid("Test.IWidget:HRESULT get_Name(String*);HRESULT put_Name(String);");
            Assert.Equal(expected, guid.Value);
        }
    }

    [Fact]
    public void UserDefinedInterface_Guid_MatchesSeedAlgorithm()
    {
        // User-defined interfaces (without explicit [uuid]) also use this algorithm
        var bytes = CompileAndEmit("""
            namespace Test
            {
                interface IFoo
                {
                    void Bar();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "IFoo");
            var guid = GetGuidAttributeValue(reader, handle);
            Assert.NotNull(guid);

            var expected = ComputeInterfaceGuid("Test.IFoo:HRESULT Bar();");
            Assert.Equal(expected, guid.Value);
        }
    }

    #endregion

    #region Gap 2: EventHandler TypeRef projection

    [Fact]
    public void EventHandler_TypeRef_UsesWindowsFoundationNamespace()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                runtimeclass Widget
                {
                    Widget();
                    event Windows.Foundation.EventHandler<Object> Changed;
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            bool found = false;
            foreach (var handle in reader.TypeReferences)
            {
                var typeRef = reader.GetTypeReference(handle);
                var name = reader.GetString(typeRef.Name);
                if (name == "EventHandler`1")
                {
                    var ns = reader.GetString(typeRef.Namespace);
                    // C++ dumper confirms original midl uses WinRT names, not CLR projections
                    Assert.Equal("Windows.Foundation", ns);

                    // Should be scoped to Windows.Foundation assembly ref
                    Assert.Equal(HandleKind.AssemblyReference, typeRef.ResolutionScope.Kind);
                    var assemblyRef = reader.GetAssemblyReference((AssemblyReferenceHandle)typeRef.ResolutionScope);
                    Assert.Equal("Windows.Foundation", reader.GetString(assemblyRef.Name));
                    found = true;
                    break;
                }
            }
            Assert.True(found, "EventHandler`1 TypeRef should exist with Windows.Foundation namespace");
        }
    }

    [Fact]
    public void EventHandler_UsesWinRTNames_NotClrProjection()
    {
        // C++ dumper confirms: original midl uses Windows.Foundation.EventHandler, not System.EventHandler
        var bytes = CompileAndEmit("""
            namespace Test
            {
                runtimeclass Widget
                {
                    Widget();
                    event Windows.Foundation.EventHandler<Object> Changed;
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            // Verify EventHandler IS in Windows.Foundation namespace
            bool foundWinRT = false;
            foreach (var handle in reader.TypeReferences)
            {
                var typeRef = reader.GetTypeReference(handle);
                var name = reader.GetString(typeRef.Name);
                var ns = reader.GetString(typeRef.Namespace);
                if (name.StartsWith("EventHandler") && ns == "Windows.Foundation")
                    foundWinRT = true;
            }
            Assert.True(foundWinRT, "EventHandler should be in Windows.Foundation namespace");
        }
    }

    #endregion

    #region Gap 3: Static class statics interface

    [Fact]
    public void StaticClass_HasStaticsInterface_NotDefaultInterface()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                static runtimeclass StaticService
                {
                    static void DoWork();
                    static String GetName();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            // IStaticServiceStatics SHOULD exist
            var staticsHandle = TryFindTypeDef(reader, "IStaticServiceStatics");
            Assert.NotNull(staticsHandle);

            // IStaticService should NOT exist (no default instance interface for static-only class)
            var defaultHandle = TryFindTypeDef(reader, "IStaticService");
            Assert.Null(defaultHandle);
        }
    }

    [Fact]
    public void StaticClass_HasStaticAttribute()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                static runtimeclass StaticService
                {
                    static void DoWork();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "StaticService");
            Assert.True(HasCustomAttribute(reader, classHandle, "StaticAttribute"),
                "Static class should have [Static] attribute referencing the statics interface");
        }
    }

    [Fact]
    public void StaticClass_StaticsInterface_HasCorrectMembers()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                static runtimeclass StaticService
                {
                    static void DoWork();
                    static String GetName();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "IStaticServiceStatics");
            var td = reader.GetTypeDefinition(handle);
            var methods = td.GetMethods()
                .Select(m => reader.GetString(reader.GetMethodDefinition(m).Name))
                .ToList();

            Assert.Contains("DoWork", methods);
            Assert.Contains("GetName", methods);
        }
    }

    [Fact]
    public void MixedClass_HasBothDefaultAndStaticsInterfaces()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                    static String GetVersion();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            // Both should exist
            Assert.NotNull(TryFindTypeDef(reader, "IWidget"));
            Assert.NotNull(TryFindTypeDef(reader, "IWidgetStatics"));

            // IWidget should only have instance members
            var iwidgetHandle = FindTypeDef(reader, "IWidget");
            var td = reader.GetTypeDefinition(iwidgetHandle);
            var methods = td.GetMethods()
                .Select(m => reader.GetString(reader.GetMethodDefinition(m).Name))
                .ToList();
            Assert.Contains("DoWork", methods);
            Assert.DoesNotContain("GetVersion", methods);

            // IWidgetStatics should only have static members
            var staticsHandle = FindTypeDef(reader, "IWidgetStatics");
            var stdTd = reader.GetTypeDefinition(staticsHandle);
            var staticMethods = stdTd.GetMethods()
                .Select(m => reader.GetString(reader.GetMethodDefinition(m).Name))
                .ToList();
            Assert.Contains("GetVersion", staticMethods);
            Assert.DoesNotContain("DoWork", staticMethods);
        }
    }

    #endregion

    #region Seed debugging

    [Fact]
    public void AbiSignatureSeed_BasicWidget_MatchesOriginalMidl()
    {
        var source = """
            namespace TestApp.Parity
            {
                runtimeclass BasicWidget
                {
                    BasicWidget();
                    String WidgetName { get; };
                    Int32 WidgetId { get; };
                    Boolean IsEnabled { get; set; };
                    Double Factor { get; set; };
                    Int32 Priority;
                    event Windows.Foundation.TypedEventHandler<BasicWidget, Object> Changed;
                    event Windows.Foundation.TypedEventHandler<BasicWidget, String> NameChanged;
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        Assert.Empty(compilation.Diagnostics);
        var rc = compilation.GetAllTypes().OfType<Midl.Analysis.Symbols.RuntimeClassSymbol>().First();
        var instanceMembers = rc.Members
            .Where(m => m is Midl.Analysis.Symbols.MethodSymbol { IsStatic: false }
                or Midl.Analysis.Symbols.PropertySymbol { IsStatic: false }
                or Midl.Analysis.Symbols.EventSymbol { IsStatic: false })
            .ToList();
        var seed = Midl.CodeGen.WinMd.TypeDefEmitter.BuildAbiSignatureSeed($"TestApp.Parity.I{rc.Name}", instanceMembers);
        var expected = "TestApp.Parity.IBasicWidget:HRESULT get_WidgetName(String*);HRESULT get_WidgetId(Int32*);HRESULT get_IsEnabled(Boolean*);HRESULT put_IsEnabled(Boolean);HRESULT get_Factor(Double*);HRESULT put_Factor(Double);HRESULT get_Priority(Int32*);HRESULT put_Priority(Int32);HRESULT add_Changed(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.BasicWidget, Object>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_Changed(EventRegistrationToken);HRESULT add_NameChanged(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.BasicWidget, String>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_NameChanged(EventRegistrationToken);";
        Assert.Equal(expected, seed);
    }

    #endregion

    /// <summary>
    /// Computes an interface GUID using original midl's algorithm:
    /// UUID v5 with base {e72a134c-baf7-4dd3-b542-77848e87b138} and ABI signature seed.
    /// </summary>
    private static Guid ComputeInterfaceGuid(string seed)
    {
        var baseGuid = new Guid("e72a134c-baf7-4dd3-b542-77848e87b138");
        var nsBytes = baseGuid.ToByteArray();
        // Convert to network byte order
        (nsBytes[0], nsBytes[3]) = (nsBytes[3], nsBytes[0]);
        (nsBytes[1], nsBytes[2]) = (nsBytes[2], nsBytes[1]);
        (nsBytes[4], nsBytes[5]) = (nsBytes[5], nsBytes[4]);
        (nsBytes[6], nsBytes[7]) = (nsBytes[7], nsBytes[6]);

        var nameBytes = System.Text.Encoding.UTF8.GetBytes(seed);
        var input = new byte[nsBytes.Length + nameBytes.Length];
        Array.Copy(nsBytes, input, nsBytes.Length);
        Array.Copy(nameBytes, 0, input, nsBytes.Length, nameBytes.Length);

        var hash = System.Security.Cryptography.SHA1.HashData(input);

        // Set version 5 and variant RFC 4122
        hash[6] = (byte)((hash[6] & 0x0F) | 0x50);
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80);

        int a = (hash[0] << 24) | (hash[1] << 16) | (hash[2] << 8) | hash[3];
        short b = (short)((hash[4] << 8) | hash[5]);
        short c = (short)((hash[6] << 8) | hash[7]);
        byte[] d = { hash[8], hash[9], hash[10], hash[11], hash[12], hash[13], hash[14], hash[15] };

        return new Guid(a, b, c, d);
    }
}
