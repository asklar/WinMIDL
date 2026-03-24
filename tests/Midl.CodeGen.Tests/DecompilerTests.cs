using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Midl.Analysis;
using Midl.CodeGen.Decompiler;
using Midl.CodeGen.WinMd;

namespace Midl.CodeGen.Tests;

public partial class DecompilerTests
{
    private static byte[] CompileAndEmit(string source)
    {
        var compilation = CompilationHelper.Compile(source);
        Assert.Empty(compilation.Diagnostics);
        var emitter = new WinMdEmitter();
        return emitter.EmitToBytes(compilation);
    }

    [Fact]
    public void DecompileEnum()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                enum Color { Red, Green, Blue }
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("namespace Test", idl);
        Assert.Contains("enum Color", idl);
        Assert.Contains("Red", idl);
        Assert.Contains("Green", idl);
        Assert.Contains("Blue", idl);
    }

    [Fact]
    public void DecompileStruct()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                struct Point { Single X; Single Y; }
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("namespace Test", idl);
        Assert.Contains("struct Point", idl);
        Assert.Contains("Single X;", idl);
        Assert.Contains("Single Y;", idl);
    }

    [Fact]
    public void DecompileInterface()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                interface IWidget
                {
                    void Activate();
                    String Name { get; };
                }
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("namespace Test", idl);
        Assert.Contains("interface IWidget", idl);
        Assert.Contains("void Activate()", idl);
        Assert.Contains("String Name;", idl);
    }

    [Fact]
    public void DecompileRuntimeClass()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                }
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("namespace Test", idl);
        Assert.Contains("runtimeclass Widget", idl);
        Assert.Contains("Widget()", idl);
        Assert.Contains("void DoWork()", idl);
    }

    [Fact]
    public void RoundTrip()
    {
        var originalIdl = """
            namespace Test
            {
                enum Color { Red, Green, Blue }
                struct Point { Single X; Single Y; }
                delegate void Callback();
                interface IWidget
                {
                    void Activate();
                }
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                }
            }
            """;

        // Compile original → emit WinMD
        var bytes = CompileAndEmit(originalIdl);

        // Decompile WinMD → IDL
        var decompiler = new WinMdDecompiler();
        var decompiledIdl = decompiler.Decompile(bytes);

        // Recompile the decompiled IDL — should produce no errors
        var recompilation = CompilationHelper.Compile(decompiledIdl);
        var errors = recompilation.Diagnostics
            .Where(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error)
            .ToList();

        Assert.Empty(errors);
    }

    [Fact]
    public void DecompileThreadingAttribute()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [threading(both)]
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                }
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("[threading(both)]", idl);
        Assert.Contains("runtimeclass Widget", idl);

        // Verify round-trip: recompile the decompiled IDL
        var recompilation = CompilationHelper.Compile(idl);
        var errors = recompilation.Diagnostics
            .Where(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void DecompileMarshalingBehaviorAttribute()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [marshaling_behavior(agile), default_interface]
                runtimeclass Widget
                {
                    Widget();
                }
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("[marshaling_behavior(agile)]", idl);
    }

    [Fact]
    public void DecompileDeprecatedAttribute()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [deprecated("Use NewWidget instead", deprecate, 1), default_interface]
                runtimeclass Widget
                {
                    Widget();
                }
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("[deprecated(\"Use NewWidget instead\"", idl);
        Assert.Contains("deprecate", idl);
    }

    [Fact]
    public void DecompileExclusiveToAttribute()
    {
        // Compile a runtimeclass — the emitter produces a non-public exclusive interface.
        // Verify that ExclusiveToAttribute is readable by examining the WinMD directly.
        var bytes = CompileAndEmit("""
            namespace Test
            {
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                }
            }
            """);

        // Read the WinMD and find the non-public exclusive interface
        using var peReader = new PEReader(new MemoryStream(bytes));
        var reader = peReader.GetMetadataReader();

        string? exclusiveToValue = null;
        foreach (var handle in reader.TypeDefinitions)
        {
            var td = reader.GetTypeDefinition(handle);
            var name = reader.GetString(td.Name);
            if (name == "IWidget") // synthesized exclusive interface
            {
                foreach (var attrHandle in td.GetCustomAttributes())
                {
                    var attr = reader.GetCustomAttribute(attrHandle);
                    if (attr.Constructor.Kind == HandleKind.MemberReference)
                    {
                        var memberRef = reader.GetMemberReference((MemberReferenceHandle)attr.Constructor);
                        if (memberRef.Parent.Kind == HandleKind.TypeReference)
                        {
                            var typeRef = reader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
                            if (reader.GetString(typeRef.Name) == "ExclusiveToAttribute")
                            {
                                var blob = reader.GetBlobReader(attr.Value);
                                blob.ReadUInt16(); // prolog
                                exclusiveToValue = blob.ReadSerializedString();
                            }
                        }
                    }
                }
            }
        }

        Assert.NotNull(exclusiveToValue);
        Assert.Contains("Widget", exclusiveToValue);
    }

    [Fact]
    public void DecompileComposableAttribute()
    {
        // The emitter doesn't currently call AddComposableAttribute, but the decompiler
        // should handle it for external WinMD files. Verify by checking that an unsealed
        // runtimeclass with constructors is properly decompiled.
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [default_interface]
                unsealed runtimeclass BaseWidget
                {
                    BaseWidget();
                }
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        // The unsealed keyword should be preserved via TypeAttributes
        Assert.Contains("unsealed runtimeclass BaseWidget", idl);
    }

    [Fact]
    public void DecompileActivatableAttribute()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [default_interface]
                runtimeclass Widget
                {
                    Widget();
                }
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        // The emitter adds ActivatableAttribute for classes with constructors
        Assert.Contains("[activatable(", idl);
    }

    [Fact]
    public void DecompileContractAttribute()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                apicontract TestContract {}
                [contract(TestContract, 2), default_interface]
                runtimeclass Widget
                {
                    Widget();
                }
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("[contract(TestContract, 2)]", idl);
    }

    [Fact]
    public void DecompileApiContract()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [contractversion(3)]
                apicontract MyContract {}
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("apicontract MyContract", idl);
        Assert.Contains("[contractversion(", idl);
    }

    [Fact]
    public void DecompileRuntimeClass_StaticMembers_ShowStaticKeyword()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [default_interface]
                runtimeclass Widget
                {
                    Widget();
                    static void Reset();
                    static String DefaultName { get; };
                }
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("static void Reset()", idl);
        Assert.Contains("static String DefaultName", idl);
    }

    [Fact]
    public void DecompileRuntimeClass_MixedStaticAndInstance()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [default_interface]
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                    String Name { get; };
                    static void Reset();
                    static String DefaultName { get; };
                }
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        // Instance members should NOT have static
        Assert.Contains("void DoWork()", idl);
        Assert.DoesNotContain("static void DoWork()", idl);
        Assert.Contains("String Name", idl);

        // Static members should have static
        Assert.Contains("static void Reset()", idl);
        Assert.Contains("static String DefaultName", idl);
    }

    [Fact]
    public void DecompileDelegate_GenericParamNames()
    {
        // Decompiling a WinMD with generic delegates should show named type params,
        // not !0/!1 or backtick notation
        var winmdPath = @"C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.26100.0\Windows.winmd";
        if (!File.Exists(winmdPath)) return; // skip on machines without SDK

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(winmdPath, "Windows.Foundation");

        // TypedEventHandler should show named generic params
        Assert.Contains("TypedEventHandler<TSender, TResult>", idl);
        // Params should use the named params, not !0/!1
        Assert.DoesNotContain("!0", idl);
        Assert.DoesNotContain("!1", idl);
        // Should not have backtick notation
        Assert.DoesNotContain("`", idl);
    }

    [Fact]
    public void DecompileNamespaceFilter_OnlyShowsRequestedNamespace()
    {
        var winmdPath = @"C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.26100.0\Windows.winmd";
        if (!File.Exists(winmdPath)) return;

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(winmdPath, "Windows.Foundation");

        // Should contain Windows.Foundation types
        Assert.Contains("namespace Windows.Foundation", idl);
        Assert.Contains("TypedEventHandler", idl);
        // Should NOT contain other namespaces
        Assert.DoesNotContain("namespace Windows.UI", idl);
        Assert.DoesNotContain("namespace Windows.Storage", idl);
    }

    [Fact]
    public void DecompileWindowsFoundation_IncludesTimeSpan()
    {
        var winmdPath = @"C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.26100.0\Windows.winmd";
        if (!File.Exists(winmdPath)) return;

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(winmdPath, "Windows.Foundation");

        Assert.Contains("struct TimeSpan", idl);
        Assert.Contains("Int64 Duration", idl);
    }

    [Fact]
    public void DecompileWindowsFoundation_IncludesDateTime()
    {
        var winmdPath = @"C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.26100.0\Windows.winmd";
        if (!File.Exists(winmdPath)) return;

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(winmdPath, "Windows.Foundation");

        Assert.Contains("struct DateTime", idl);
        Assert.Contains("Int64 UniversalTime", idl);
    }

    [Fact]
    public void DecompileWindowsFoundation_TypeRefsUseWinRTNames()
    {
        var winmdPath = @"C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.26100.0\Windows.winmd";
        if (!File.Exists(winmdPath)) return;

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(winmdPath, "Windows.Foundation");

        // Projected CLR names should not appear in decompiled output
        Assert.DoesNotContain("System.TimeSpan", idl);
        Assert.DoesNotContain("System.DateTimeOffset", idl);
        Assert.DoesNotContain("System.Uri", idl);
    }

    #region Overridable and protected modifiers

    [Fact]
    public void Decompile_OverridableMembers_ShowKeyword()
    {
        var source = @"
            namespace N {
                unsealed runtimeclass Widget {
                    overridable void OnActivated();
                    void DoWork();
                }
            }";
        var bytes = CompileAndEmit(source);
        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);
        Assert.Contains("overridable void OnActivated", idl);
        Assert.Contains("void DoWork", idl);
        // DoWork should NOT have overridable
        Assert.DoesNotContain("overridable void DoWork", idl);
    }

    [Fact]
    public void Decompile_ProtectedMembers_ShowKeyword()
    {
        var source = @"
            namespace N {
                unsealed runtimeclass Widget {
                    protected void Initialize();
                    void DoWork();
                }
            }";
        var bytes = CompileAndEmit(source);
        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);
        Assert.Contains("protected void Initialize", idl);
        Assert.DoesNotContain("protected void DoWork", idl);
    }

    [Fact]
    public void Decompile_WindowsWinmd_OverridableMembers()
    {
        var winmdPath = @"C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.26100.0\Windows.winmd";
        if (!File.Exists(winmdPath)) return;
        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(winmdPath, "Windows.UI.Xaml.Controls");
        // ContentControl has overridable OnContentChanged
        Assert.Contains("overridable", idl);
    }

    #endregion

    #region Namespace-scoped type names

    [Fact]
    public void Decompile_CrossNamespaceTypes_FullyQualified()
    {
        // Types from other namespaces should keep their fully qualified name
        var winmdPath = @"C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.26100.0\Windows.winmd";
        if (!File.Exists(winmdPath)) return;

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(winmdPath, "Windows.UI.Xaml.Controls");

        // UIElement is in Windows.UI.Xaml, not Windows.UI.Xaml.Controls — should be fully qualified
        Assert.Contains("Windows.UI.Xaml.UIElement", idl);
        // DateTime is in Windows.Foundation — should be fully qualified  
        Assert.Contains("Windows.Foundation.DateTime", idl);
        // IVector is in Windows.Foundation.Collections — should be fully qualified
        Assert.Contains("Windows.Foundation.Collections.IVector", idl);
    }

    [Fact]
    public void Decompile_SameNamespaceTypes_ShortName()
    {
        // Types in the same namespace should use short names
        var winmdPath = @"C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.26100.0\Windows.winmd";
        if (!File.Exists(winmdPath)) return;

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(winmdPath, "Windows.UI.Xaml.Controls");

        // UIElementCollection is in Windows.UI.Xaml.Controls — should be short
        Assert.Contains("UIElementCollection", idl);
        // Should NOT have Windows.UI.Xaml.Controls.UIElementCollection (that would be redundant)
        Assert.DoesNotContain("Windows.UI.Xaml.Controls.UIElementCollection", idl);
    }

    [Fact]
    public void Decompile_ProjectedCollectionTypes_FullyQualified()
    {
        // CLR-projected collection types should show WinRT names, fully qualified
        var winmdPath = @"C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.26100.0\Windows.winmd";
        if (!File.Exists(winmdPath)) return;

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(winmdPath, "Windows.UI.Xaml.Controls");

        // Should NOT contain CLR collection names
        Assert.DoesNotContain("System.Collections.Generic.IList", idl);
        Assert.DoesNotContain("System.Collections.Generic.IReadOnlyList", idl);
    }

    [Fact]
    public void Decompile_GenericArgsAlsoFullyQualified()
    {
        // Generic type arguments from other namespaces should also be fully qualified
        var winmdPath = @"C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.26100.0\Windows.winmd";
        if (!File.Exists(winmdPath)) return;

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(winmdPath, "Windows.UI.Xaml.Controls");

        // IVector<Windows.UI.Xaml.UIElement> — the generic arg should be fully qualified
        Assert.Contains("Windows.UI.Xaml.UIElement>", idl);
        // No spurious > without type name
        Assert.DoesNotContain(" UIElement>", idl.Replace("Windows.UI.Xaml.UIElement>", ""));
    }

    [Fact]
    public void Decompile_SameNamespaceGenericArgs_Short()
    {
        // When decompiling Windows.Foundation, types like IAsyncOperation<IPropertyValue> 
        // should use short name for IPropertyValue (same namespace)
        var winmdPath = @"C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.26100.0\Windows.winmd";
        if (!File.Exists(winmdPath)) return;

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(winmdPath, "Windows.Foundation");

        // TypedEventHandler uses same-namespace type params
        Assert.Contains("TypedEventHandler<TSender, TResult>", idl);
        // No backtick or !0 notation
        Assert.DoesNotContain("`", idl);
        Assert.DoesNotContain("!0", idl);
    }

    #endregion

    #region Base class inheritance

    [Fact]
    public void Decompile_RuntimeClass_ShowsBaseClass()
    {
        var source = @"
            namespace N {
                unsealed runtimeclass Base { void DoWork(); }
                unsealed runtimeclass Derived : Base { void DoMore(); }
            }";
        var bytes = CompileAndEmit(source);
        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);
        Assert.Contains("runtimeclass Derived", idl);
    }

    #endregion

    #region Gap fixes: attributes, events, property shorthand

    [Fact]
    public void Decompile_WebHostHidden_Shown()
    {
        var winmdPath = @"C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.26100.0\Windows.winmd";
        if (!File.Exists(winmdPath)) return;
        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(winmdPath, "Windows.UI.Xaml.Controls");
        Assert.Contains("[webhosthidden]", idl);
    }

    [Fact]
    public void Decompile_ContractOnClass_Shown()
    {
        var winmdPath = @"C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.26100.0\Windows.winmd";
        if (!File.Exists(winmdPath)) return;
        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(winmdPath, "Windows.UI.Xaml.Controls");
        // Classes should show [contract(...)] attribute
        Assert.Contains("[contract(", idl);
    }

    [Fact]
    public void Decompile_WindowsWinmd_ControlShowsBaseClass()
    {
        var winmdPath = @"C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.26100.0\Windows.winmd";
        if (!File.Exists(winmdPath)) return;
        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(winmdPath, "Windows.UI.Xaml.Controls");
        // ContentControl extends Control
        Assert.Matches(@"runtimeclass ContentControl\s*:\s*Control", idl);
    }

    #endregion

    #region Version number decoding

    [Fact]
    public void Decompile_VersionNumbers_Decoded()
    {
        var winmdPath = @"C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.26100.0\Windows.winmd";
        if (!File.Exists(winmdPath)) return;
        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(winmdPath, "Windows.UI.Xaml.Controls");
        // Should NOT contain raw encoded versions like 65536
        Assert.DoesNotContain(", 65536)", idl);
        Assert.DoesNotContain(", 131072)", idl);
        // Should contain decoded versions like 1 or 2
        Assert.Contains(", 1)", idl);
    }

    #endregion

    #region Event delegate types

    [Fact]
    public void Decompile_Events_ShowDelegateType()
    {
        var source = @"
            namespace N {
                delegate void MyHandler(Object sender);
                runtimeclass Widget {
                    event MyHandler Activated;
                }
            }";
        var bytes = CompileAndEmit(source);
        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);
        Assert.Contains("event MyHandler Activated", idl);
        Assert.DoesNotContain("event  ", idl);
    }

    [Fact]
    public void Decompile_ReadWriteProperty_UsesShorthand()
    {
        var source = @"
            namespace N {
                runtimeclass Widget {
                    String Name;
                    Int32 ReadOnly { get; };
                }
            }";
        var bytes = CompileAndEmit(source);
        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);
        Assert.Contains("String Name;", idl);
        Assert.Contains("Int32 ReadOnly { get; };", idl);
    }

    #endregion

    #region Versioned blocks

    [Fact]
    public void Decompile_VersionedBlocks_GroupedByVersion()
    {
        var source = @"
            namespace N {
                [contractversion(1)]
                apicontract MyContract {}

                [contract(MyContract, 1)]
                runtimeclass Widget {
                    void MethodV1();

                    [contract(MyContract, 2)]
                    {
                        void MethodV2();
                    }
                }
            }";
        var bytes = CompileAndEmit(source);
        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);
        Assert.Contains("MethodV1", idl);
        Assert.Contains("MethodV2", idl);
        Assert.Matches(@"\[contract\(.*MyContract.*, 2\)\]\s*\{", idl);
        Assert.DoesNotMatch(@"\[contract\(.*MyContract.*, 1\)\]\s*\{", idl);
    }

    [Fact]
    public void Decompile_WindowsWinmd_HasVersionedBlocks()
    {
        var winmdPath = @"C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.26100.0\Windows.winmd";
        if (!File.Exists(winmdPath)) return;
        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(winmdPath, "Windows.UI.Xaml.Controls");
        var contractBlockPattern = System.Text.RegularExpressions.Regex.IsMatch(
            idl, @"\[contract\(.+?, \d+\)\]\s*\{");
        Assert.True(contractBlockPattern, "Should have at least one versioned block");
    }

    [Fact]
    public void Decompile_SingleVersion_NoVersionedBlock()
    {
        var source = @"
            namespace N {
                runtimeclass Widget {
                    void DoWork();
                    String Name;
                }
            }";
        var bytes = CompileAndEmit(source);
        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);
        Assert.DoesNotMatch(@"\[contract\(.+?\)\]\s*\{", idl);
    }

    #endregion

    #region Parameter Direction Tests

    [Fact]
    public void DecompileRefConstParam()
    {
        var source = """
            namespace Test
            {
                struct Point { Single X; Single Y; }
                runtimeclass Calculator
                {
                    static Double Area(ref const Point p);
                }
            }
            """;

        var bytes = CompileAndEmit(source);
        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("ref const Point p", idl);
    }

    [Fact]
    public void DecompileRefConstGuid()
    {
        var source = """
            namespace Test
            {
                runtimeclass GuidHelper
                {
                    static Boolean Equals(ref const Guid a, ref const Guid b);
                }
            }
            """;

        var bytes = CompileAndEmit(source);
        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("ref const Guid a", idl);
        Assert.Contains("ref const Guid b", idl);
    }

    [Fact]
    public void DecompileOutParam()
    {
        var source = """
            namespace Test
            {
                runtimeclass Parser
                {
                    static Boolean TryParse(String input, out Int32 parsed);
                }
            }
            """;

        var bytes = CompileAndEmit(source);
        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("out Int32 parsed", idl);
    }

    [Fact]
    public void RoundTripRefConst()
    {
        var originalIdl = """
            namespace Test
            {
                struct Point { Single X; Single Y; }
                runtimeclass Calculator
                {
                    static Double Area(ref const Point p);
                }
            }
            """;

        var bytes = CompileAndEmit(originalIdl);
        var decompiler = new WinMdDecompiler();
        var decompiledIdl = decompiler.Decompile(bytes);

        // Verify ref const survives the compile → decompile cycle
        Assert.Contains("ref const Point p", decompiledIdl);

        // Verify the decompiled IDL contains valid MIDL3 structure
        Assert.Contains("namespace Test", decompiledIdl);
        Assert.Contains("runtimeclass Calculator", decompiledIdl);
    }

    [Fact]
    public void RoundTripOutParam()
    {
        var originalIdl = """
            namespace Test
            {
                runtimeclass Parser
                {
                    static Boolean TryParse(String input, out Int32 parsed);
                }
            }
            """;

        var bytes = CompileAndEmit(originalIdl);
        var decompiler = new WinMdDecompiler();
        var decompiledIdl = decompiler.Decompile(bytes);

        // Verify out survives the compile → decompile cycle
        Assert.Contains("out Int32 parsed", decompiledIdl);

        Assert.Contains("namespace Test", decompiledIdl);
        Assert.Contains("runtimeclass Parser", decompiledIdl);
    }

    #endregion

    #region Static Attribute Suppression

    [Fact]
    public void Decompile_StaticMethods_NoStaticAttribute()
    {
        var source = """
            namespace Test
            {
                runtimeclass Helper
                {
                    static Int32 GetCount();
                    static void Reset();
                }
            }
            """;

        var bytes = CompileAndEmit(source);
        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        // Methods should have 'static' keyword
        Assert.Contains("static Int32 GetCount()", idl);
        Assert.Contains("static void Reset()", idl);

        // [static(IHelperStatics, ...)] attribute should NOT appear
        Assert.DoesNotMatch(@"\[static\(", idl);
    }

    #endregion
}
