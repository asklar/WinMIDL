using Midl.Analysis;
using Midl.Analysis.Symbols;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for ABI signature-based GUID computation.
/// Real midl.exe GUIDs are the ground truth, obtained by running:
///   midl.exe /winrt /nomidl /h temp.h /reference Windows.winmd &lt;fixture&gt;.idl
/// </summary>
public partial class AbiGuidTests
{
    private static Compilation Compile(string source) => CompilationHelper.Compile(source);

    private static InterfaceSymbol FindInterface(Compilation c, string name)
        => c.GetAllTypes().OfType<InterfaceSymbol>().First(i => i.Name == name);

    private static DelegateSymbol FindDelegate(Compilation c, string name)
        => c.GetAllTypes().OfType<DelegateSymbol>().First(d => d.Name == name);

    // ── Unit tests: signature seed format ────────────────────────────

    [Fact]
    public void SignatureSeed_EmptyInterface_HasOnlyNameAndColon()
    {
        var c = Compile(@"
            namespace TestApp.Parity
            {
                interface IEmpty { }
            }");
        var iface = FindInterface(c, "IEmpty");
        var seed = AbiGuidComputer.BuildAbiSignatureSeed(iface.FullName, iface.Members.ToList());
        Assert.Equal("TestApp.Parity.IEmpty:", seed);
    }

    [Fact]
    public void SignatureSeed_SingleMethod_ReturningVoid()
    {
        var c = Compile(@"
            namespace TestApp.Parity
            {
                interface ISimple
                {
                    void Reset();
                }
            }");
        var iface = FindInterface(c, "ISimple");
        var seed = AbiGuidComputer.BuildAbiSignatureSeed(iface.FullName, iface.Members.ToList());
        Assert.Equal("TestApp.Parity.ISimple:HRESULT Reset();", seed);
    }

    [Fact]
    public void SignatureSeed_MethodWithReturnType()
    {
        var c = Compile(@"
            namespace TestApp.Parity
            {
                interface ISimple
                {
                    String GetName();
                }
            }");
        var iface = FindInterface(c, "ISimple");
        var seed = AbiGuidComputer.BuildAbiSignatureSeed(iface.FullName, iface.Members.ToList());
        // String is value-like (HSTRING handle) → single * for return
        Assert.Equal("TestApp.Parity.ISimple:HRESULT GetName(String*);", seed);
    }

    [Fact]
    public void SignatureSeed_MethodWithValueReturn()
    {
        var c = Compile(@"
            namespace TestApp.Parity
            {
                interface ISimple
                {
                    Int32 GetCount();
                }
            }");
        var iface = FindInterface(c, "ISimple");
        var seed = AbiGuidComputer.BuildAbiSignatureSeed(iface.FullName, iface.Members.ToList());
        // Int32 is a value type → * for out
        Assert.Equal("TestApp.Parity.ISimple:HRESULT GetCount(Int32*);", seed);
    }

    [Fact]
    public void SignatureSeed_MethodWithInputParams()
    {
        var c = Compile(@"
            namespace TestApp.Parity
            {
                interface ISimple
                {
                    void SetName(String name);
                }
            }");
        var iface = FindInterface(c, "ISimple");
        var seed = AbiGuidComputer.BuildAbiSignatureSeed(iface.FullName, iface.Members.ToList());
        // String input is value-like (HSTRING) → no extra *
        Assert.Equal("TestApp.Parity.ISimple:HRESULT SetName(String);", seed);
    }

    [Fact]
    public void SignatureSeed_Property_ReadOnly()
    {
        var c = Compile(@"
            namespace TestApp.Parity
            {
                interface ISimple
                {
                    String Name { get; };
                }
            }");
        var iface = FindInterface(c, "ISimple");
        var seed = AbiGuidComputer.BuildAbiSignatureSeed(iface.FullName, iface.Members.ToList());
        // String return → single * (value-like handle)
        Assert.Equal("TestApp.Parity.ISimple:HRESULT get_Name(String*);", seed);
    }

    [Fact]
    public void SignatureSeed_Property_ReadWrite()
    {
        var c = Compile(@"
            namespace TestApp.Parity
            {
                interface ISimple
                {
                    Boolean IsEnabled { get; set; };
                }
            }");
        var iface = FindInterface(c, "ISimple");
        var seed = AbiGuidComputer.BuildAbiSignatureSeed(iface.FullName, iface.Members.ToList());
        // Boolean is value type: get returns Boolean*, put takes Boolean (no *)
        Assert.Equal("TestApp.Parity.ISimple:HRESULT get_IsEnabled(Boolean*);HRESULT put_IsEnabled(Boolean);", seed);
    }

    // ── GUID computation: parity with real midl.exe ─────────────────

    [Fact]
    public void Guid_IBasicWidget_MatchesRealMidl()
    {
        // Real midl.exe produces: 55dbe203-ced8-5f0c-b3c1-bc70c944db28
        var c = Compile(@"
            namespace TestApp.Parity
            {
                runtimeclass BasicWidget
                {
                    BasicWidget();
                    BasicWidget(String name);
                    String GetName();
                    Int32 GetCount();
                    Boolean IsActive();
                    Double GetValue();
                    Single GetRatio();
                    void Reset();
                    void SetName(String name);
                    void Configure(String name, Int32 count, Boolean enabled);
                    String WidgetName { get; };
                    Int32 WidgetId { get; };
                    Boolean IsEnabled { get; set; };
                    Double Factor { get; set; };
                    Int32 Priority;
                    event Windows.Foundation.TypedEventHandler<BasicWidget, Object> Changed;
                    event Windows.Foundation.TypedEventHandler<BasicWidget, String> NameChanged;
                }
            }");

        var iface = FindInterface(c, "IBasicWidget");
        var guid = AbiGuidComputer.ComputeGuid(iface);
        Assert.Equal(Guid.Parse("55dbe203-ced8-5f0c-b3c1-bc70c944db28"), guid);
    }

    [Fact]
    public void Guid_IBasicWidgetFactory_MatchesRealMidl()
    {
        // Real midl.exe produces: 4b447d0d-331b-548f-85b8-bb2cd7a05654
        var c = Compile(@"
            namespace TestApp.Parity
            {
                runtimeclass BasicWidget
                {
                    BasicWidget();
                    BasicWidget(String name);
                    String GetName();
                    Int32 GetCount();
                    Boolean IsActive();
                    Double GetValue();
                    Single GetRatio();
                    void Reset();
                    void SetName(String name);
                    void Configure(String name, Int32 count, Boolean enabled);
                    String WidgetName { get; };
                    Int32 WidgetId { get; };
                    Boolean IsEnabled { get; set; };
                    Double Factor { get; set; };
                    Int32 Priority;
                    event Windows.Foundation.TypedEventHandler<BasicWidget, Object> Changed;
                    event Windows.Foundation.TypedEventHandler<BasicWidget, String> NameChanged;
                }
            }");

        var iface = FindInterface(c, "IBasicWidgetFactory");
        var guid = AbiGuidComputer.ComputeGuid(iface);
        Assert.Equal(Guid.Parse("4b447d0d-331b-548f-85b8-bb2cd7a05654"), guid);
    }

    [Fact]
    public void Guid_ExplicitUuid_IsPreserved()
    {
        var c = Compile(@"
            namespace TestApp.Parity
            {
                [uuid(""A1B2C3D4-E5F6-7890-ABCD-EF0123456789"")]
                interface IMarker { }
            }");

        var iface = FindInterface(c, "IMarker");
        var guid = AbiGuidComputer.ComputeGuid(iface);
        Assert.Equal(Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF0123456789"), guid);
    }

    [Fact]
    public void Guid_IDataSource_MatchesRealMidl()
    {
        // Real midl.exe produces: 6185c161-0082-53b2-98ed-9b11e71e123a for IDataSource
        var c = Compile(@"
            namespace TestApp.Parity
            {
                delegate void DataChangedHandler(String key, Object data);

                interface IDataSource
                {
                    String Name { get; };
                    Int32 ItemCount { get; };
                    void Refresh();
                    Object GetItem(Int32 index);
                    event DataChangedHandler DataChanged;
                }
            }");

        var iface = FindInterface(c, "IDataSource");
        var guid = AbiGuidComputer.ComputeGuid(iface);
        Assert.Equal(Guid.Parse("6185c161-0082-53b2-98ed-9b11e71e123a"), guid);
    }

    [Fact]
    public void Guid_ICacheable_MatchesRealMidl()
    {
        // Real midl.exe produces: c3cfecd8-26e8-5eee-af72-002d2fa0a535 for ICacheable
        var c = Compile(@"
            namespace TestApp.Parity
            {
                interface ICacheable
                {
                    Boolean IsCached { get; };
                    void Invalidate();
                }
            }");

        var iface = FindInterface(c, "ICacheable");
        var guid = AbiGuidComputer.ComputeGuid(iface);
        Assert.Equal(Guid.Parse("c3cfecd8-26e8-5eee-af72-002d2fa0a535"), guid);
    }

    [Fact]
    public void Guid_Delegate_SimpleCallback_MatchesRealMidl()
    {
        // Real midl.exe produces: 65d97cec-4707-5cd8-a342-ad8c102a34bc
        var c = Compile(@"
            namespace TestApp.Parity
            {
                delegate void SimpleCallback();
            }");

        var del = FindDelegate(c, "SimpleCallback");
        var guid = AbiGuidComputer.ComputeGuid(del);
        Assert.Equal(Guid.Parse("65d97cec-4707-5cd8-a342-ad8c102a34bc"), guid);
    }

    [Fact]
    public void Guid_Delegate_MessageHandler_MatchesRealMidl()
    {
        // Real midl.exe produces: a196f933-fc8a-5ff8-8917-e5083038b5ff
        var c = Compile(@"
            namespace TestApp.Parity
            {
                delegate void MessageHandler(String message);
            }");

        var del = FindDelegate(c, "MessageHandler");
        var guid = AbiGuidComputer.ComputeGuid(del);
        Assert.Equal(Guid.Parse("a196f933-fc8a-5ff8-8917-e5083038b5ff"), guid);
    }
}
