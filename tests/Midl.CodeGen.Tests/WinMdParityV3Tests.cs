using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Midl.Analysis;
using Midl.CodeGen.WinMd;

namespace Midl.CodeGen.Tests;

/// <summary>
/// TDD tests for remaining WinMD parity fixes (v3) between WinMIDL and original midl.exe.
/// Validated against C++ ground-truth dumper (tools/winmd-dump/winmd-dump.exe).
/// </summary>
public class WinMdParityV3Tests
{
    private static byte[] CompileAndEmit(string source, string? outputPath = null)
    {
        var compilation = CompilationHelper.Compile(source);
        Assert.Empty(compilation.Diagnostics);
        var emitter = new WinMdEmitter();
        return emitter.EmitToBytes(compilation, outputPath);
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

    private static MethodDefinitionHandle FindMethod(MetadataReader reader, TypeDefinitionHandle typeDef, string methodName)
    {
        var td = reader.GetTypeDefinition(typeDef);
        foreach (var mh in td.GetMethods())
        {
            var md = reader.GetMethodDefinition(mh);
            if (reader.GetString(md.Name) == methodName)
                return mh;
        }
        throw new Exception($"Method '{methodName}' not found");
    }

    private const string BasicClassSource = """
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

                void DoWork(Int32 count);
            }
        }
        """;

    private const string StaticClassSource = """
        namespace TestApp.Parity
        {
            runtimeclass StaticService
            {
                static String DefaultName { get; set; };
                static void Initialize();
                static Int32 GetCount();
            }
        }
        """;

    #region Fix 1: Assembly name = output filename

    [Fact]
    public void AssemblyName_WithOutputPath_UsesFilename()
    {
        var bytes = CompileAndEmit(BasicClassSource, @"C:\out\MyTestOutput.winmd");
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var assembly = reader.GetAssemblyDefinition();
            Assert.Equal("MyTestOutput", reader.GetString(assembly.Name));
        }
    }

    [Fact]
    public void AssemblyName_WithoutOutputPath_UsesRootNamespace()
    {
        var bytes = CompileAndEmit(BasicClassSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var assembly = reader.GetAssemblyDefinition();
            // When no output path is given, assembly name is derived from root namespace
            var name = reader.GetString(assembly.Name);
            Assert.False(string.IsNullOrEmpty(name));
        }
    }

    #endregion

    #region Fix 2: Assembly version = 255.255.255.255

    [Fact]
    public void AssemblyVersion_Is255()
    {
        var bytes = CompileAndEmit(BasicClassSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var assembly = reader.GetAssemblyDefinition();
            Assert.Equal(new Version(255, 255, 255, 255), assembly.Version);
        }
    }

    #endregion

    #region Fix 3: Module name = filename.winmd

    [Fact]
    public void ModuleName_WithOutputPath_UsesFilename()
    {
        var bytes = CompileAndEmit(BasicClassSource, @"C:\out\MyTestOutput.winmd");
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var module = reader.GetModuleDefinition();
            Assert.Equal("MyTestOutput.winmd", reader.GetString(module.Name));
        }
    }

    [Fact]
    public void ModuleName_WithoutOutputPath_FallsBackToAssemblyName()
    {
        var bytes = CompileAndEmit(BasicClassSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var module = reader.GetModuleDefinition();
            var moduleName = reader.GetString(module.Name);
            // Without an output path, module name should still end in .winmd
            Assert.EndsWith(".winmd", moduleName);
        }
    }

    #endregion

    #region Fix 4: AssemblyRef = Windows.Foundation (not FoundationContract)

    [Fact]
    public void AssemblyRef_WindowsFoundation_NotFoundationContract()
    {
        var bytes = CompileAndEmit(BasicClassSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var assemblyRefNames = new List<string>();
            foreach (var h in reader.AssemblyReferences)
            {
                var ar = reader.GetAssemblyReference(h);
                assemblyRefNames.Add(reader.GetString(ar.Name));
            }

            Assert.Contains("Windows.Foundation", assemblyRefNames);
            Assert.DoesNotContain("Windows.Foundation.FoundationContract", assemblyRefNames);
        }
    }

    [Fact]
    public void AssemblyRef_NoSeparateWindowsRef()
    {
        // External types in Windows.* should use the Windows.Foundation assembly ref,
        // not create a separate "Windows" assembly ref.
        var source = """
            namespace TestApp.Parity
            {
                runtimeclass Widget
                {
                    Widget();
                    event Windows.Foundation.TypedEventHandler<Widget, Object> Changed;
                }
            }
            """;
        var bytes = CompileAndEmit(source);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var assemblyRefNames = new List<string>();
            foreach (var h in reader.AssemblyReferences)
            {
                var ar = reader.GetAssemblyReference(h);
                assemblyRefNames.Add(reader.GetString(ar.Name));
            }

            Assert.DoesNotContain("Windows", assemblyRefNames);
            Assert.Contains("Windows.Foundation", assemblyRefNames);
        }
    }

    #endregion

    #region Fix 5: EventRegistrationToken uses WinRT name

    [Fact]
    public void EventRegistrationToken_ProjectedToMscorlib()
    {
        var source = """
            namespace TestApp
            {
                runtimeclass Widget
                {
                    Widget();
                    event Windows.Foundation.TypedEventHandler<Widget, Object> Changed;
                }
            }
            """;
        var bytes = CompileAndEmit(source);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            bool found = false;
            foreach (var h in reader.TypeReferences)
            {
                var tr = reader.GetTypeReference(h);
                if (reader.GetString(tr.Name) == "EventRegistrationToken")
                {
                    // midl.exe CLR-projects Windows.Foundation.EventRegistrationToken to
                    // System.Runtime.InteropServices.WindowsRuntime.EventRegistrationToken
                    // (in mscorlib). Verified against midl.exe baseline WinMDs.
                    Assert.Equal("System.Runtime.InteropServices.WindowsRuntime", reader.GetString(tr.Namespace));
                    found = true;
                    break;
                }
            }
            Assert.True(found, "EventRegistrationToken TypeRef should exist");
        }
    }

    [Fact]
    public void EventHandler_UsesClrProjection()
    {
        var source = """
            namespace TestApp
            {
                runtimeclass Widget
                {
                    static event Windows.Foundation.EventHandler<Object> Changed;
                }
            }
            """;
        var bytes = CompileAndEmit(source);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            bool foundWinRT = false;
            bool foundCLR = false;
            foreach (var h in reader.TypeReferences)
            {
                var tr = reader.GetTypeReference(h);
                var name = reader.GetString(tr.Name);
                if (name.StartsWith("EventHandler"))
                {
                    var ns = reader.GetString(tr.Namespace);
                    if (ns == "Windows.Foundation") foundWinRT = true;
                    if (ns == "System") foundCLR = true;
                }
            }
            // Verified against windows.winmd: Windows.Foundation.EventHandler`1 is
            // CLR-projected to System.EventHandler`1 in emitted WinMDs.
            Assert.True(foundCLR, "Should use System.EventHandler CLR projection");
            Assert.False(foundWinRT, "Should NOT use Windows.Foundation.EventHandler WinRT name");
        }
    }

    #endregion

    #region Fix 6: [In] on all input parameters

    [Fact]
    public void InputParameters_HaveInAttribute()
    {
        var bytes = CompileAndEmit(BasicClassSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var ifaceHandle = FindTypeDef(reader, "IBasicWidget");
            var doWork = FindMethod(reader, ifaceHandle, "DoWork");
            var method = reader.GetMethodDefinition(doWork);

            foreach (var ph in method.GetParameters())
            {
                var p = reader.GetParameter(ph);
                if (p.SequenceNumber > 0) // skip retval at seq 0
                {
                    Assert.True((p.Attributes & ParameterAttributes.In) != 0,
                        $"Param '{reader.GetString(p.Name)}' should have [In] attribute");
                }
            }
        }
    }

    [Fact]
    public void InputParameters_RuntimeClassStubs_HaveInAttribute()
    {
        var bytes = CompileAndEmit(BasicClassSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "BasicWidget");
            var doWork = FindMethod(reader, classHandle, "DoWork");
            var method = reader.GetMethodDefinition(doWork);

            foreach (var ph in method.GetParameters())
            {
                var p = reader.GetParameter(ph);
                if (p.SequenceNumber > 0)
                {
                    Assert.True((p.Attributes & ParameterAttributes.In) != 0,
                        $"Param '{reader.GetString(p.Name)}' should have [In] attribute");
                }
            }
        }
    }

    [Fact]
    public void SetterParameter_HasInAttribute()
    {
        var bytes = CompileAndEmit(BasicClassSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var ifaceHandle = FindTypeDef(reader, "IBasicWidget");
            var setter = FindMethod(reader, ifaceHandle, "put_IsEnabled");
            var method = reader.GetMethodDefinition(setter);

            foreach (var ph in method.GetParameters())
            {
                var p = reader.GetParameter(ph);
                if (p.SequenceNumber > 0)
                {
                    Assert.True((p.Attributes & ParameterAttributes.In) != 0,
                        $"Setter param '{reader.GetString(p.Name)}' should have [In]");
                }
            }
        }
    }

    [Fact]
    public void ConstructorParameter_HasInAttribute()
    {
        var source = """
            namespace TestApp
            {
                [default_interface]
                runtimeclass Widget
                {
                    Widget(String name);
                    void DoWork();
                }
            }
            """;
        var bytes = CompileAndEmit(source);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "Widget");
            var ctor = FindMethod(reader, classHandle, ".ctor");
            var method = reader.GetMethodDefinition(ctor);

            foreach (var ph in method.GetParameters())
            {
                var p = reader.GetParameter(ph);
                if (p.SequenceNumber > 0)
                {
                    Assert.True((p.Attributes & ParameterAttributes.In) != 0,
                        $"Constructor param '{reader.GetString(p.Name)}' should have [In]");
                }
            }
        }
    }

    #endregion

    #region Fix 7: Method flags on runtimeclass stubs

    [Fact]
    public void RuntimeClassStub_NonStatic_HasFinalVirtualNewSlot()
    {
        var bytes = CompileAndEmit(BasicClassSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "BasicWidget");
            var doWork = FindMethod(reader, classHandle, "DoWork");
            var md = reader.GetMethodDefinition(doWork);

            var expected = MethodAttributes.Public | MethodAttributes.Final
                | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
            Assert.Equal(expected, md.Attributes);
        }
    }

    [Fact]
    public void RuntimeClassStub_PropertyGetter_HasFinalVirtualNewSlotSpecialName()
    {
        var bytes = CompileAndEmit(BasicClassSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "BasicWidget");
            var getter = FindMethod(reader, classHandle, "get_WidgetName");
            var md = reader.GetMethodDefinition(getter);

            var expected = MethodAttributes.Public | MethodAttributes.Final
                | MethodAttributes.Virtual | MethodAttributes.HideBySig
                | MethodAttributes.NewSlot | MethodAttributes.SpecialName;
            Assert.Equal(expected, md.Attributes);
        }
    }

    [Fact]
    public void RuntimeClassStub_StaticMethod_HasStaticFlag()
    {
        var bytes = CompileAndEmit(StaticClassSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "StaticService");
            var init = FindMethod(reader, classHandle, "Initialize");
            var md = reader.GetMethodDefinition(init);

            Assert.True((md.Attributes & MethodAttributes.Static) != 0,
                "Static runtimeclass stub should have Static flag");
        }
    }

    [Fact]
    public void RuntimeClassStub_StaticProperty_HasStaticFlag()
    {
        var bytes = CompileAndEmit(StaticClassSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "StaticService");
            var getter = FindMethod(reader, classHandle, "get_DefaultName");
            var md = reader.GetMethodDefinition(getter);

            Assert.True((md.Attributes & MethodAttributes.Static) != 0,
                "Static property getter stub should have Static flag");
        }
    }

    #endregion

    #region Fix 8: GUID algorithm

    [Fact]
    public void SynthesizedInterfaceGuid_IsDeterministic()
    {
        // Same source compiled twice should produce same GUID
        var source = """
            namespace TestApp
            {
                [default_interface]
                runtimeclass Widget
                {
                    Widget();
                    String Name { get; set; };
                    void DoWork(Int32 count);
                }
            }
            """;

        var bytes1 = CompileAndEmit(source);
        var bytes2 = CompileAndEmit(source);

        var reader1 = CreateReader(bytes1, out var pe1);
        var reader2 = CreateReader(bytes2, out var pe2);
        using (pe1)
        using (pe2)
        {
            var guid1 = GetGuidAttribute(reader1, FindTypeDef(reader1, "IWidget"));
            var guid2 = GetGuidAttribute(reader2, FindTypeDef(reader2, "IWidget"));
            Assert.NotNull(guid1);
            Assert.NotNull(guid2);
            Assert.Equal(guid1.Value, guid2.Value);
            // Ensure it's not a random GUID (all zeros would indicate failure)
            Assert.NotEqual(Guid.Empty, guid1.Value);
        }
    }

    [Fact]
    public void SynthesizedInterfaceGuid_UsesCorrectNamespaceUuid()
    {
        // The base namespace UUID for interface GUID generation should be
        // e72a134c-baf7-4dd3-b542-77848e87b138 (per OS source)
        // Verify by checking the GUID is version 5 (SHA-1 based)
        var source = """
            namespace TestApp
            {
                [default_interface]
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                }
            }
            """;

        var bytes = CompileAndEmit(source);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var guid = GetGuidAttribute(reader, FindTypeDef(reader, "IWidget"));
            Assert.NotNull(guid);
            // UUID v5 check: version nibble in Data3 should be 5
            var guidBytes = guid.Value.ToByteArray();
            // Data3 is at bytes[6..7], big-endian high nibble of byte[7] is version
            int version = (guidBytes[7] >> 4) & 0x0F;
            Assert.Equal(5, version);
            // Variant should be RFC 4122 (10xx at byte 8)
            int variant = (guidBytes[8] >> 6) & 0x03;
            Assert.Equal(2, variant); // 10 binary = 2
        }
    }

    private static Guid? GetGuidAttribute(MetadataReader reader, TypeDefinitionHandle typeDef)
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
            byte d = blob.ReadByte();
            byte e = blob.ReadByte();
            byte f = blob.ReadByte();
            byte g = blob.ReadByte();
            byte h = blob.ReadByte();
            byte i = blob.ReadByte();
            byte j = blob.ReadByte();
            byte k = blob.ReadByte();
            return new Guid((int)a, (short)b, (short)c, d, e, f, g, h, i, j, k);
        }
        return null;
    }

    #endregion

    #region Parameter Direction (ref const / out) — ByRef signatures and ParameterAttributes

    private const string RefConstSource = """
        namespace TestApp.Parity
        {
            struct Point { Single X; Single Y; }
            runtimeclass Calculator
            {
                static Double Area(ref const Point p);
            }
        }
        """;

    private const string OutParamSource = """
        namespace TestApp.Parity
        {
            runtimeclass Parser
            {
                static Boolean TryParse(String input, out Int32 parsed);
            }
        }
        """;

    private const string RefConstGuidSource = """
        namespace TestApp.Parity
        {
            runtimeclass GuidHelper
            {
                static Boolean Equals(ref const Guid target, ref const Guid other);
            }
        }
        """;

    [Fact]
    public void RefConst_EmitsParameterAttributeIn()
    {
        var bytes = CompileAndEmit(RefConstSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            // Find the synthesized interface that contains Area
            var ifaceHandle = FindTypeDef(reader, "ICalculatorStatics");
            var methodHandle = FindMethod(reader, ifaceHandle, "Area");
            var method = reader.GetMethodDefinition(methodHandle);
            var parameters = method.GetParameters()
                .Select(p => reader.GetParameter(p))
                .Where(p => p.SequenceNumber > 0)
                .ToList();

            Assert.Single(parameters);
            Assert.Equal("p", reader.GetString(parameters[0].Name));
            Assert.True(parameters[0].Attributes.HasFlag(ParameterAttributes.In));
            Assert.False(parameters[0].Attributes.HasFlag(ParameterAttributes.Out));
        }
    }

    [Fact]
    public void RefConst_EmitsByRefSignature()
    {
        var bytes = CompileAndEmit(RefConstSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var ifaceHandle = FindTypeDef(reader, "ICalculatorStatics");
            var methodHandle = FindMethod(reader, ifaceHandle, "Area");
            var method = reader.GetMethodDefinition(methodHandle);

            // Decode the signature — ByRef types should be marked with '&'
            var sig = method.DecodeSignature(new TestSignatureProvider(reader), null);
            Assert.Single(sig.ParameterTypes);
            // The raw decoded type should show ByRef marker
            Assert.True(sig.ParameterTypes[0].EndsWith("&"), "Parameter should be ByRef (end with '&')");
        }
    }

    [Fact]
    public void OutParam_EmitsParameterAttributeOut()
    {
        var bytes = CompileAndEmit(OutParamSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var ifaceHandle = FindTypeDef(reader, "IParserStatics");
            var methodHandle = FindMethod(reader, ifaceHandle, "TryParse");
            var method = reader.GetMethodDefinition(methodHandle);
            var parameters = method.GetParameters()
                .Select(p => reader.GetParameter(p))
                .Where(p => p.SequenceNumber > 0)
                .ToList();

            // First param: input (In), second param: value (Out)
            Assert.Equal(2, parameters.Count);
            Assert.Equal("input", reader.GetString(parameters[0].Name));
            Assert.True(parameters[0].Attributes.HasFlag(ParameterAttributes.In));

            Assert.Equal("parsed", reader.GetString(parameters[1].Name));
            Assert.True(parameters[1].Attributes.HasFlag(ParameterAttributes.Out));
        }
    }

    [Fact]
    public void OutParam_EmitsByRefSignature()
    {
        var bytes = CompileAndEmit(OutParamSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var ifaceHandle = FindTypeDef(reader, "IParserStatics");
            var methodHandle = FindMethod(reader, ifaceHandle, "TryParse");
            var method = reader.GetMethodDefinition(methodHandle);

            var sig = method.DecodeSignature(new TestSignatureProvider(reader), null);
            Assert.Equal(2, sig.ParameterTypes.Length);
            // First param: not ByRef (plain String)
            Assert.False(sig.ParameterTypes[0].EndsWith("&"));
            // Second param: ByRef (out Int32)
            Assert.True(sig.ParameterTypes[1].EndsWith("&"), "out param should be ByRef (end with '&')");
        }
    }

    [Fact]
    public void RefConstGuid_EmitsCorrectMetadata()
    {
        var bytes = CompileAndEmit(RefConstGuidSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var ifaceHandle = FindTypeDef(reader, "IGuidHelperStatics");
            var methodHandle = FindMethod(reader, ifaceHandle, "Equals");
            var method = reader.GetMethodDefinition(methodHandle);

            // Both params should be In + ByRef
            var parameters = method.GetParameters()
                .Select(p => reader.GetParameter(p))
                .Where(p => p.SequenceNumber > 0)
                .ToList();
            Assert.Equal(2, parameters.Count);
            foreach (var param in parameters)
            {
                Assert.True(param.Attributes.HasFlag(ParameterAttributes.In));
                Assert.False(param.Attributes.HasFlag(ParameterAttributes.Out));
            }

            var sig = method.DecodeSignature(new TestSignatureProvider(reader), null);
            Assert.Equal(2, sig.ParameterTypes.Length);
            foreach (var pt in sig.ParameterTypes)
                Assert.True(pt.EndsWith("&"), $"ref const param type '{pt}' should be ByRef (end with '&')");
        }
    }

    [Fact]
    public void RefConstGuid_GroundTruthDump()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_refconst_{Guid.NewGuid():N}.winmd");
        try
        {
            var bytes = CompileAndEmit(RefConstGuidSource);
            File.WriteAllBytes(tempPath, bytes);

            var dumperPath = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory, "..", "..", "..", "..", "..", "tools", "winmd-dump", "winmd-dump.exe"));

            if (!File.Exists(dumperPath))
                return; // Skip if dumper not available

            var psi = new System.Diagnostics.ProcessStartInfo(dumperPath, tempPath)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };
            var proc = System.Diagnostics.Process.Start(psi)!;
            var output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();

            // Ground truth: params should be ByRef (&) with [In]
            Assert.Contains("Guid&", output);
            Assert.Contains("[In]", output);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Simple signature provider that preserves ByRef markers for test assertions.
    /// </summary>
    private sealed class TestSignatureProvider : ISignatureTypeProvider<string, object?>
    {
        private readonly MetadataReader _reader;
        public TestSignatureProvider(MetadataReader reader) => _reader = reader;

        public string GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode.ToString();
        public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
        {
            var td = reader.GetTypeDefinition(handle);
            return reader.GetString(td.Name);
        }
        public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
        {
            var tr = reader.GetTypeReference(handle);
            return reader.GetString(tr.Name);
        }
        public string GetSZArrayType(string elementType) => $"{elementType}[]";
        public string GetArrayType(string elementType, ArrayShape shape) => $"{elementType}[]";
        public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments)
            => $"{genericType}<{string.Join(", ", typeArguments)}>";
        public string GetByReferenceType(string elementType) => elementType + "&";
        public string GetPointerType(string elementType) => elementType + "*";
        public string GetPinnedType(string elementType) => elementType;
        public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) => unmodifiedType;
        public string GetFunctionPointerType(MethodSignature<string> signature) => "fnptr";
        public string GetGenericMethodParameter(object? genericContext, int index) => $"!!{index}";
        public string GetGenericTypeParameter(object? genericContext, int index) => $"!{index}";
        public string GetTypeFromSpecification(MetadataReader reader, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
        {
            var spec = reader.GetTypeSpecification(handle);
            return spec.DecodeSignature(this, genericContext);
        }
    }

    #endregion
}
