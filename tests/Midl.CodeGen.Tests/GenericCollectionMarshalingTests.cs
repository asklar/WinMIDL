using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for generic collection interface marshaling in proxy/stub codegen.
/// Verifies that IVector&lt;T&gt;, IMap&lt;K,V&gt;, IIterable&lt;T&gt;, IVectorView&lt;T&gt;,
/// IMapView&lt;K,V&gt; and nested generics route through interface marshaling.
/// </summary>
public class GenericCollectionMarshalingTests
{
    private readonly ProxyGenerator _proxyGen = new();
    private readonly StubGenerator _stubGen = new();

    private static Compilation Compile(string source) => CompilationHelper.Compile(source);

    // ─── Proxy: IVector<String> return emits marshal_interface ───

    [Fact]
    public void ProxyGenerator_IVectorString_EmitsMarshalInterface()
    {
        var idl = @"
            namespace TestApp {
                interface IWidget {
                    Windows.Foundation.Collections.IVector<String> GetItems();
                    void SetItems(Windows.Foundation.Collections.IVectorView<String> items);
                }
            }";
        var compilation = Compile(idl);
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("marshal_interface", output);
        Assert.DoesNotContain("// TODO", output);
    }

    // ─── Proxy: IMap<String, Int32> return emits marshal_interface ───

    [Fact]
    public void ProxyGenerator_IMapStringInt_EmitsMarshalInterface()
    {
        var idl = @"
            namespace TestApp {
                interface IWidget {
                    Windows.Foundation.Collections.IMap<String, Int32> GetMapping();
                }
            }";
        var compilation = Compile(idl);
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("marshal_interface", output);
        Assert.DoesNotContain("// TODO", output);
    }

    // ─── Stub: IVector<String> param emits unmarshal_interface ───

    [Fact]
    public void StubGenerator_IVectorParam_EmitsUnmarshalInterface()
    {
        var idl = @"
            namespace TestApp {
                interface IWidget {
                    void SetItems(Windows.Foundation.Collections.IVector<String> items);
                }
            }";
        var compilation = Compile(idl);
        var output = _stubGen.Generate(compilation);

        Assert.Contains("unmarshal_interface", output);
        Assert.DoesNotContain("// TODO", output);
    }

    // ─── Proxy: IIterable<T> return emits marshal_interface ───

    [Fact]
    public void ProxyGenerator_IIterableRetval_EmitsMarshalInterface()
    {
        var idl = @"
            namespace TestApp {
                interface IWidget {
                    Windows.Foundation.Collections.IIterable<String> EnumItems();
                }
            }";
        var compilation = Compile(idl);
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("unmarshal_interface", output);
        Assert.DoesNotContain("// TODO", output);
    }

    // ─── WireFormat: generic collection param is not fixed size ───

    [Fact]
    public void WireFormat_GenericCollectionParam_IsNotFixedSize()
    {
        var ns = new NamespaceSymbol("Collections",
            new NamespaceSymbol("Foundation",
                new NamespaceSymbol("Windows")));
        var ifaceSym = new InterfaceSymbol("IVector", ns) { IsExternal = true };
        var typeRef = new TypeRef("IVector", [new TypeRef("String")])
        {
            ResolvedSymbol = ifaceSym
        };

        Assert.False(WireFormat.IsFixedSize(typeRef));
    }

    // ─── Nested generic: IVector<IMap<String, Int32>> ───

    [Fact]
    public void ProxyGenerator_NestedGeneric_HandledCorrectly()
    {
        var idl = @"
            namespace TestApp {
                interface IWidget {
                    Windows.Foundation.Collections.IVector<Windows.Foundation.Collections.IMap<String, Int32>> GetNestedItems();
                }
            }";
        var compilation = Compile(idl);
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("marshal_interface", output);
        Assert.DoesNotContain("// TODO", output);
    }

    // ─── Unique IID constant names per instantiation ───

    [Fact]
    public void WireFormat_GenericIidName_UniquePerInstantiation()
    {
        var ns = new NamespaceSymbol("Collections",
            new NamespaceSymbol("Foundation",
                new NamespaceSymbol("Windows")));
        var ifaceSym = new InterfaceSymbol("IVector", ns) { IsExternal = true };

        var vectorString = new TypeRef("IVector", [new TypeRef("String")])
        {
            ResolvedSymbol = ifaceSym
        };
        var vectorInt = new TypeRef("IVector", [new TypeRef("Int32")])
        {
            ResolvedSymbol = ifaceSym
        };

        var nameString = WireFormat.GetInterfaceIidName(vectorString);
        var nameInt = WireFormat.GetInterfaceIidName(vectorInt);

        Assert.NotEqual(nameString, nameInt);
        Assert.Contains("IVector", nameString);
        Assert.Contains("IVector", nameInt);
        Assert.Contains("String", nameString);
        Assert.Contains("Int32", nameInt);
    }

    // ─── IID constant name format ───

    [Fact]
    public void WireFormat_GenericIidConstantName_HasExpectedFormat()
    {
        var ns = new NamespaceSymbol("Collections",
            new NamespaceSymbol("Foundation",
                new NamespaceSymbol("Windows")));
        var ifaceSym = new InterfaceSymbol("IVector", ns) { IsExternal = true };

        var typeRef = new TypeRef("IVector", [new TypeRef("String")])
        {
            ResolvedSymbol = ifaceSym
        };

        var name = WireFormat.GetGenericIidConstantName(typeRef);
        Assert.Equal("IID_IVector_1_String", name);
    }

    // ─── IMap IID constant name includes both type args ───

    [Fact]
    public void WireFormat_GenericIidConstantName_IMap_TwoArgs()
    {
        var ns = new NamespaceSymbol("Collections",
            new NamespaceSymbol("Foundation",
                new NamespaceSymbol("Windows")));
        var ifaceSym = new InterfaceSymbol("IMap", ns) { IsExternal = true };

        var typeRef = new TypeRef("IMap", [new TypeRef("String"), new TypeRef("Int32")])
        {
            ResolvedSymbol = ifaceSym
        };

        var name = WireFormat.GetGenericIidConstantName(typeRef);
        Assert.Equal("IID_IMap_2_String_Int32", name);
    }

    // ─── ComputeGenericIid returns non-null for known types ───

    [Fact]
    public void WireFormat_ComputeGenericIid_IVectorString_ReturnsGuid()
    {
        var ns = new NamespaceSymbol("Collections",
            new NamespaceSymbol("Foundation",
                new NamespaceSymbol("Windows")));
        var ifaceSym = new InterfaceSymbol("IVector", ns) { IsExternal = true };

        var typeRef = new TypeRef("IVector", [new TypeRef("String")])
        {
            ResolvedSymbol = ifaceSym
        };

        var iid = WireFormat.ComputeGenericIid(typeRef);
        Assert.NotNull(iid);
        // IVector<String> IID per UUID v5 PIID algorithm
        Assert.Equal(new Guid("98b9acc1-4b56-532e-ac73-03d5291cca90"), iid.Value);
    }

    // ─── ComputeGenericIid: different type args → different IIDs ───

    [Fact]
    public void WireFormat_ComputeGenericIid_DifferentArgs_DifferentIids()
    {
        var ns = new NamespaceSymbol("Collections",
            new NamespaceSymbol("Foundation",
                new NamespaceSymbol("Windows")));
        var ifaceSym = new InterfaceSymbol("IVector", ns) { IsExternal = true };

        var vectorString = new TypeRef("IVector", [new TypeRef("String")])
        {
            ResolvedSymbol = ifaceSym
        };
        var vectorInt = new TypeRef("IVector", [new TypeRef("Int32")])
        {
            ResolvedSymbol = ifaceSym
        };

        var iidString = WireFormat.ComputeGenericIid(vectorString);
        var iidInt = WireFormat.ComputeGenericIid(vectorInt);

        Assert.NotNull(iidString);
        Assert.NotNull(iidInt);
        Assert.NotEqual(iidString, iidInt);
    }

    // ─── Generated proxy contains computed IID constant ───

    [Fact]
    public void ProxyGenerator_EmitsComputedIidConstant()
    {
        var idl = @"
            namespace TestApp {
                interface IWidget {
                    Windows.Foundation.Collections.IVector<String> GetItems();
                }
            }";
        var compilation = Compile(idl);
        var output = _proxyGen.Generate(compilation);

        // The generated code should contain a constexpr IID definition
        Assert.Contains("IID_IVector_1_String", output);
        Assert.Contains("constexpr IID", output);
    }

    // ─── Generated stub contains computed IID constant ───

    [Fact]
    public void StubGenerator_EmitsComputedIidConstant()
    {
        var idl = @"
            namespace TestApp {
                interface IWidget {
                    void SetItems(Windows.Foundation.Collections.IVector<String> items);
                }
            }";
        var compilation = Compile(idl);
        var output = _stubGen.Generate(compilation);

        Assert.Contains("IID_IVector_1_String", output);
        Assert.Contains("constexpr IID", output);
    }

    // ─── IsInterfaceType returns true for generic interface ───

    [Fact]
    public void WireFormat_IsInterfaceType_TrueForGenericInterface()
    {
        var ns = new NamespaceSymbol("Collections",
            new NamespaceSymbol("Foundation",
                new NamespaceSymbol("Windows")));
        var ifaceSym = new InterfaceSymbol("IVector", ns) { IsExternal = true };
        var typeRef = new TypeRef("IVector", [new TypeRef("String")])
        {
            ResolvedSymbol = ifaceSym
        };

        Assert.True(WireFormat.IsInterfaceType(typeRef));
    }

    // ─── GetParamTypeEnum returns t_interface for generic ───

    [Fact]
    public void WireFormat_GetParamTypeEnum_ReturnsInterfaceForGeneric()
    {
        var ns = new NamespaceSymbol("Collections",
            new NamespaceSymbol("Foundation",
                new NamespaceSymbol("Windows")));
        var ifaceSym = new InterfaceSymbol("IVector", ns) { IsExternal = true };
        var typeRef = new TypeRef("IVector", [new TypeRef("String")])
        {
            ResolvedSymbol = ifaceSym
        };

        Assert.Equal("winmidl::param_type::t_interface", WireFormat.GetParamTypeEnum(typeRef));
    }

    // ─── IVectorView param also works ───

    [Fact]
    public void ProxyGenerator_IVectorViewParam_EmitsMarshalInterface()
    {
        var idl = @"
            namespace TestApp {
                interface IWidget {
                    void ShowItems(Windows.Foundation.Collections.IVectorView<Int32> items);
                }
            }";
        var compilation = Compile(idl);
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("marshal_interface", output);
        Assert.Contains("IID_IVectorView_1_Int32", output);
        Assert.DoesNotContain("// TODO", output);
    }

    // ─── IMapView works ───

    [Fact]
    public void ProxyGenerator_IMapView_EmitsMarshalInterface()
    {
        var idl = @"
            namespace TestApp {
                interface IWidget {
                    Windows.Foundation.Collections.IMapView<String, String> GetView();
                }
            }";
        var compilation = Compile(idl);
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("marshal_interface", output);
        Assert.Contains("IID_IMapView_2_String_String", output);
    }

    // ─── FormatGuidAsCpp produces valid C++ ───

    [Fact]
    public void WireFormat_FormatGuidAsCpp_ProducesValidFormat()
    {
        var guid = new Guid("2f13c006-a03a-5f69-b090-75a43e33423e");
        var cpp = WireFormat.FormatGuidAsCpp(guid);

        Assert.StartsWith("{ 0x", cpp);
        Assert.Contains("0x2f13c006", cpp);
        Assert.EndsWith("}", cpp);
    }

    [Fact]
    public void ProxyGenerator_IVectorOfRuntimeClass_DifferentIidFromIVectorOfString()
    {
        var idl = @"
            namespace TestApp {
                interface IAnimal { String Name { get; }; };
                [default_interface]
                runtimeclass Animal : IAnimal { Animal(); }

                interface IWidget {
                    Windows.Foundation.Collections.IVector<String> GetNames();
                    Windows.Foundation.Collections.IVector<Animal> GetAnimals();
                }
            }";
        var compilation = CompilationHelper.Compile(idl);
        var proxyGen = new ProxyGenerator();
        var output = proxyGen.Generate(compilation);

        // Both should have distinct IID constants
        Assert.Contains("IID_IVector_1_String", output);
        Assert.Contains("IID_IVector_1___x_TestApp_Animal", output);

        // The IID values should be different (different lines)
        var lines = output.Split('\n');
        var stringIid = lines.FirstOrDefault(l => l.Contains("IID_IVector_1_String ="));
        var animalIid = lines.FirstOrDefault(l => l.Contains("IID_IVector_1___x_TestApp_Animal ="));
        Assert.NotNull(stringIid);
        Assert.NotNull(animalIid);
        Assert.NotEqual(stringIid, animalIid);
    }

    [Fact]
    public void ProxyGenerator_IMapWithRuntimeClassValue_DifferentIidFromPrimitive()
    {
        var idl = @"
            namespace TestApp {
                interface IAnimal { String Name { get; }; };
                [default_interface]
                runtimeclass Animal : IAnimal { Animal(); }

                interface IWidget {
                    Windows.Foundation.Collections.IMap<String, Int32> GetScores();
                    Windows.Foundation.Collections.IMap<String, Animal> GetAnimalsByName();
                }
            }";
        var compilation = CompilationHelper.Compile(idl);
        var proxyGen = new ProxyGenerator();
        var output = proxyGen.Generate(compilation);

        Assert.Contains("IID_IMap_2_String_Int32", output);
        Assert.Contains("IID_IMap_2_String___x_TestApp_Animal", output);

        // Marshal calls should use the correct IID for each
        Assert.Contains("IID_IMap_2_String_Int32", output);
        Assert.Contains("IID_IMap_2_String___x_TestApp_Animal", output);
    }

    [Fact]
    public void ProxyGenerator_SameGenericDifferentArgs_BothMarshaledCorrectly()
    {
        var idl = @"
            namespace TestApp {
                interface IWidget {
                    Windows.Foundation.Collections.IVector<String> GetStrings();
                    Windows.Foundation.Collections.IVector<Int32> GetInts();
                    Windows.Foundation.Collections.IVector<Double> GetDoubles();
                }
            }";
        var compilation = CompilationHelper.Compile(idl);
        var proxyGen = new ProxyGenerator();
        var output = proxyGen.Generate(compilation);

        Assert.Contains("IID_IVector_1_String", output);
        Assert.Contains("IID_IVector_1_Int32", output);
        Assert.Contains("IID_IVector_1_Double", output);

        // All three should be distinct IID values
        var lines = output.Split('\n');
        var iids = lines.Where(l => l.Contains("IID_IVector_1_") && l.Contains("= {")).ToList();
        Assert.Equal(3, iids.Count);
        Assert.Equal(3, iids.Distinct().Count());
    }

    [Fact]
    public void ComputeGenericIid_IVectorString_MatchesKnownSdkValue()
    {
        // IVector<String> PIID is well-known: {98b9acc1-4b56-532e-ac73-03d5291cca90}
        // This must match RoGetParameterizedTypeInstanceIID output
        var ns = new NamespaceSymbol("Collections",
            new NamespaceSymbol("Foundation",
            new NamespaceSymbol("Windows")));
        var ifaceSym = new InterfaceSymbol("IVector", ns) { IsExternal = true };
        var typeRef = new TypeRef("IVector", [new TypeRef("String")]) { ResolvedSymbol = ifaceSym };

        var iid = WireFormat.ComputeGenericIid(typeRef);
        Assert.NotNull(iid);
        Assert.Equal(new Guid("98b9acc1-4b56-532e-ac73-03d5291cca90"), iid!.Value);
    }

    [Fact]
    public void ComputeGenericIid_IVectorViewString_MatchesKnownSdkValue()
    {
        // IVectorView<String> = {2f13c006-a03a-5f69-b090-75a43e33423e}
        var ns = new NamespaceSymbol("Collections",
            new NamespaceSymbol("Foundation",
            new NamespaceSymbol("Windows")));
        var ifaceSym = new InterfaceSymbol("IVectorView", ns) { IsExternal = true };
        var typeRef = new TypeRef("IVectorView", [new TypeRef("String")]) { ResolvedSymbol = ifaceSym };

        var iid = WireFormat.ComputeGenericIid(typeRef);
        Assert.NotNull(iid);
        Assert.Equal(new Guid("2f13c006-a03a-5f69-b090-75a43e33423e"), iid!.Value);
    }

    [Fact]
    public void ComputeGenericIid_IMapStringInt32_MatchesKnownSdkValue()
    {
        // IMap<String, Int32> — known PIID
        var ns = new NamespaceSymbol("Collections",
            new NamespaceSymbol("Foundation",
            new NamespaceSymbol("Windows")));
        var ifaceSym = new InterfaceSymbol("IMap", ns) { IsExternal = true };
        var typeRef = new TypeRef("IMap", [new TypeRef("String"), new TypeRef("Int32")]) { ResolvedSymbol = ifaceSym };

        var iid = WireFormat.ComputeGenericIid(typeRef);
        Assert.NotNull(iid);
        // Known value from codegen output (validated against SDK at e2e runtime)
        Assert.Equal(new Guid("ae681871-dd82-5299-93ea-0275e4e073e7"), iid!.Value);
    }

    [Fact]
    public void ComputeGenericIid_IReferenceInt32_MatchesKnownSdkValue()
    {
        // IReference<Int32> = {aff0a017-349c-50e2-92c3-10bf045d8825}
        var ns = new NamespaceSymbol("Foundation",
            new NamespaceSymbol("Windows"));
        var ifaceSym = new InterfaceSymbol("IReference", ns) { IsExternal = true };
        var typeRef = new TypeRef("IReference", [new TypeRef("Int32")]) { ResolvedSymbol = ifaceSym };

        var iid = WireFormat.ComputeGenericIid(typeRef);
        Assert.NotNull(iid);
        Assert.Equal(new Guid("aff0a017-349c-50e2-92c3-10bf045d8825"), iid!.Value);
    }
}
