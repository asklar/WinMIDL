using Midl.Analysis;
using Midl.CodeGen.Manifest;
using Midl.CodeGen.WinMd;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for <see cref="ManifestEmitter"/> — SxS and AppX manifest generation from WinMD.
/// </summary>
public class ManifestEmitterTests
{
    private static byte[] CompileAndEmit(string source)
    {
        var compilation = CompilationHelper.Compile(source);
        Assert.Empty(compilation.Diagnostics);
        var emitter = new WinMdEmitter();
        return emitter.EmitToBytes(compilation);
    }

    private static List<ManifestEmitter.ActivatableClassInfo> ExtractClasses(string source)
    {
        var bytes = CompileAndEmit(source);
        return ManifestEmitter.ExtractFromBytes(bytes);
    }

    // -----------------------------------------------------------------------
    // 1. SxS format — basic runtimeclass with constructor
    // -----------------------------------------------------------------------
    [Fact]
    public void SxsFormat_BasicRuntimeClass()
    {
        var classes = new List<ManifestEmitter.ActivatableClassInfo>
        {
            new("TestNs.Widget", "both")
        };
        var emitter = new ManifestEmitter();
        var xml = emitter.EmitSxs("MyComponent.dll", classes);

        Assert.Contains("""<?xml version="1.0" encoding="utf-8"?>""", xml);
        Assert.Contains("""<assembly xmlns="urn:schemas-microsoft-com:asm.v1" manifestVersion="1.0">""", xml);
        Assert.Contains("""<file name="MyComponent.dll">""", xml);
        Assert.Contains("""<activatableClass name="TestNs.Widget" threadingModel="both" xmlns="urn:schemas-microsoft-com:winrt.v1" />""", xml);
        Assert.Contains("</file>", xml);
        Assert.Contains("</assembly>", xml);
    }

    // -----------------------------------------------------------------------
    // 2. AppX format — basic runtimeclass with constructor
    // -----------------------------------------------------------------------
    [Fact]
    public void AppxFormat_BasicRuntimeClass()
    {
        var classes = new List<ManifestEmitter.ActivatableClassInfo>
        {
            new("TestNs.Widget", "both")
        };
        var emitter = new ManifestEmitter();
        var xml = emitter.EmitAppx("MyComponent.dll", classes);

        Assert.Contains("""<?xml version="1.0" encoding="utf-8"?>""", xml);
        Assert.Contains("<Extensions>", xml);
        Assert.Contains("""<Extension Category="windows.activatableClass.inProcessServer">""", xml);
        Assert.Contains("<InProcessServer>", xml);
        Assert.Contains("<Path>MyComponent.dll</Path>", xml);
        Assert.Contains("""<ActivatableClass ActivatableClassId="TestNs.Widget" ThreadingModel="both" />""", xml);
        Assert.Contains("</InProcessServer>", xml);
        Assert.Contains("</Extension>", xml);
        Assert.Contains("</Extensions>", xml);
    }

    // -----------------------------------------------------------------------
    // 3. Multiple classes — all appear in output
    // -----------------------------------------------------------------------
    [Fact]
    public void SxsFormat_MultipleClasses()
    {
        var classes = new List<ManifestEmitter.ActivatableClassInfo>
        {
            new("NS.Alpha", "both"),
            new("NS.Beta", "STA"),
            new("NS.Gamma", "MTA"),
        };
        var emitter = new ManifestEmitter();
        var xml = emitter.EmitSxs("Multi.dll", classes);

        Assert.Contains("name=\"NS.Alpha\" threadingModel=\"both\"", xml);
        Assert.Contains("name=\"NS.Beta\" threadingModel=\"STA\"", xml);
        Assert.Contains("name=\"NS.Gamma\" threadingModel=\"MTA\"", xml);
    }

    [Fact]
    public void AppxFormat_MultipleClasses()
    {
        var classes = new List<ManifestEmitter.ActivatableClassInfo>
        {
            new("NS.Alpha", "both"),
            new("NS.Beta", "STA"),
            new("NS.Gamma", "MTA"),
        };
        var emitter = new ManifestEmitter();
        var xml = emitter.EmitAppx("Multi.dll", classes);

        Assert.Contains("ActivatableClassId=\"NS.Alpha\" ThreadingModel=\"both\"", xml);
        Assert.Contains("ActivatableClassId=\"NS.Beta\" ThreadingModel=\"STA\"", xml);
        Assert.Contains("ActivatableClassId=\"NS.Gamma\" ThreadingModel=\"MTA\"", xml);
    }

    // -----------------------------------------------------------------------
    // 4. Threading model STA
    // -----------------------------------------------------------------------
    [Fact]
    public void ThreadingModel_STA()
    {
        var result = ManifestEmitter.NormalizeThreadingModel("STA");
        Assert.Equal("STA", result);
    }

    // -----------------------------------------------------------------------
    // 5. Threading model MTA
    // -----------------------------------------------------------------------
    [Fact]
    public void ThreadingModel_MTA()
    {
        var result = ManifestEmitter.NormalizeThreadingModel("MTA");
        Assert.Equal("MTA", result);
    }

    // -----------------------------------------------------------------------
    // 6. Threading model Both (default)
    // -----------------------------------------------------------------------
    [Fact]
    public void ThreadingModel_Both_Default()
    {
        Assert.Equal("both", ManifestEmitter.NormalizeThreadingModel("Both"));
        Assert.Equal("both", ManifestEmitter.NormalizeThreadingModel(null));
        Assert.Equal("both", ManifestEmitter.NormalizeThreadingModel(""));
    }

    // -----------------------------------------------------------------------
    // 7. Static-only class excluded
    // -----------------------------------------------------------------------
    [Fact]
    public void StaticOnlyClass_IsExcluded()
    {
        var classes = ExtractClasses("""
            namespace TestNs
            {
                runtimeclass Helper
                {
                    Helper();
                    void Work();
                }

                static runtimeclass Utils
                {
                    static Int32 Add(Int32 a, Int32 b);
                }
            }
            """);

        Assert.Single(classes);
        Assert.Equal("TestNs.Helper", classes[0].FullName);
    }

    // -----------------------------------------------------------------------
    // 8. Class with constructor is included
    // -----------------------------------------------------------------------
    [Fact]
    public void ClassWithConstructor_IsIncluded()
    {
        var classes = ExtractClasses("""
            namespace TestNs
            {
                runtimeclass Widget
                {
                    Widget();
                    Widget(String name);
                    void DoWork();
                }
            }
            """);

        Assert.Single(classes);
        Assert.Equal("TestNs.Widget", classes[0].FullName);
    }

    // -----------------------------------------------------------------------
    // 9. Class with [default_interface] is included
    // -----------------------------------------------------------------------
    [Fact]
    public void ClassWithDefaultInterface_IsIncluded()
    {
        var classes = ExtractClasses("""
            namespace TestNs
            {
                runtimeclass Gadget
                {
                    Gadget();
                    String Name { get; };
                }
            }
            """);

        // A class with constructor and members gets a default interface
        Assert.Single(classes);
        Assert.Equal("TestNs.Gadget", classes[0].FullName);
    }

    // -----------------------------------------------------------------------
    // 10. Empty WinMD — no activatable classes
    // -----------------------------------------------------------------------
    [Fact]
    public void EmptyWinMd_ProducesSkeletonManifest()
    {
        var classes = new List<ManifestEmitter.ActivatableClassInfo>();
        var emitter = new ManifestEmitter();

        var sxs = emitter.EmitSxs("Empty.dll", classes);
        Assert.Contains("<assembly", sxs);
        Assert.Contains("""<file name="Empty.dll">""", sxs);
        Assert.Contains("</file>", sxs);
        Assert.DoesNotContain("activatableClass", sxs);

        var appx = emitter.EmitAppx("Empty.dll", classes);
        Assert.Contains("<Extensions>", appx);
        Assert.Contains("<Path>Empty.dll</Path>", appx);
        Assert.DoesNotContain("ActivatableClass", appx);
    }

    // -----------------------------------------------------------------------
    // 11. Server DLL name appears correctly in output
    // -----------------------------------------------------------------------
    [Fact]
    public void ServerDllName_AppearsInOutput()
    {
        var classes = new List<ManifestEmitter.ActivatableClassInfo>
        {
            new("NS.Foo", "both")
        };
        var emitter = new ManifestEmitter();

        var sxs = emitter.EmitSxs("MySpecialComponent.dll", classes);
        Assert.Contains("""<file name="MySpecialComponent.dll">""", sxs);

        var appx = emitter.EmitAppx("MySpecialComponent.dll", classes);
        Assert.Contains("<Path>MySpecialComponent.dll</Path>", appx);
    }

    // -----------------------------------------------------------------------
    // 12. Multiple WinMD inputs — merged output
    // -----------------------------------------------------------------------
    [Fact]
    public void MultipleWinMdInputs_MergedOutput()
    {
        var bytes1 = CompileAndEmit("""
            namespace LibA
            {
                runtimeclass Alpha
                {
                    Alpha();
                    void Work();
                }
            }
            """);

        var bytes2 = CompileAndEmit("""
            namespace LibB
            {
                runtimeclass Beta
                {
                    Beta();
                    String Name { get; };
                }
            }
            """);

        var classes1 = ManifestEmitter.ExtractFromBytes(bytes1);
        var classes2 = ManifestEmitter.ExtractFromBytes(bytes2);
        var merged = classes1.Concat(classes2).ToList();

        var emitter = new ManifestEmitter();
        var sxs = emitter.EmitSxs("Combined.dll", merged);

        Assert.Contains("LibA.Alpha", sxs);
        Assert.Contains("LibB.Beta", sxs);
    }

    // -----------------------------------------------------------------------
    // 13. Threading model from compiled WinMD — default (both)
    // -----------------------------------------------------------------------
    [Fact]
    public void ExtractFromWinMd_DefaultThreading_IsBoth()
    {
        var classes = ExtractClasses("""
            namespace TestNs
            {
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                }
            }
            """);

        Assert.Single(classes);
        Assert.Equal("both", classes[0].ThreadingModel);
    }

    // -----------------------------------------------------------------------
    // 14. Threading model from compiled WinMD — explicit STA
    // -----------------------------------------------------------------------
    [Fact]
    public void ExtractFromWinMd_StaThreading()
    {
        var classes = ExtractClasses("""
            namespace TestNs
            {
                [threading(sta)]
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                }
            }
            """);

        Assert.Single(classes);
        Assert.Equal("STA", classes[0].ThreadingModel);
    }

    // -----------------------------------------------------------------------
    // 15. Threading model from compiled WinMD — explicit MTA
    // -----------------------------------------------------------------------
    [Fact]
    public void ExtractFromWinMd_MtaThreading()
    {
        var classes = ExtractClasses("""
            namespace TestNs
            {
                [threading(mta)]
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                }
            }
            """);

        Assert.Single(classes);
        Assert.Equal("MTA", classes[0].ThreadingModel);
    }

    // -----------------------------------------------------------------------
    // 16. Threading model from compiled WinMD — explicit both
    // -----------------------------------------------------------------------
    [Fact]
    public void ExtractFromWinMd_BothThreading()
    {
        var classes = ExtractClasses("""
            namespace TestNs
            {
                [threading(both)]
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                }
                }
            }
            """);

        Assert.Single(classes);
        Assert.Equal("both", classes[0].ThreadingModel);
    }

    // -----------------------------------------------------------------------
    // 17. Full round-trip: compile IDL → emit WinMD → extract → emit SxS
    // -----------------------------------------------------------------------
    [Fact]
    public void FullRoundTrip_SxsManifest()
    {
        var classes = ExtractClasses("""
            namespace MyApp
            {
                [threading(both)]
                runtimeclass MainPage
                {
                    MainPage();
                    String Title { get; set; };
                }

                static runtimeclass AppHelper
                {
                    static String GetVersion();
                }
            }
            """);

        var emitter = new ManifestEmitter();
        var sxs = emitter.EmitSxs("MyApp.dll", classes);

        Assert.Contains("MyApp.MainPage", sxs);
        Assert.DoesNotContain("MyApp.AppHelper", sxs);
        Assert.Contains("threadingModel=\"both\"", sxs);
        Assert.Contains("""<file name="MyApp.dll">""", sxs);
    }

    // -----------------------------------------------------------------------
    // 18. Full round-trip: compile IDL → emit WinMD → extract → emit AppX
    // -----------------------------------------------------------------------
    [Fact]
    public void FullRoundTrip_AppxManifest()
    {
        var classes = ExtractClasses("""
            namespace MyApp
            {
                runtimeclass MainPage
                {
                    MainPage();
                    String Title { get; set; };
                }
            }
            """);

        var emitter = new ManifestEmitter();
        var appx = emitter.EmitAppx("MyApp.dll", classes);

        Assert.Contains("<Extensions>", appx);
        Assert.Contains("<ActivatableClass ActivatableClassId=\"MyApp.MainPage\"", appx);
        Assert.Contains("<Path>MyApp.dll</Path>", appx);
    }

    // -----------------------------------------------------------------------
    // 19. XML escaping in DLL name and class name
    // -----------------------------------------------------------------------
    [Fact]
    public void XmlEscaping_InOutput()
    {
        var classes = new List<ManifestEmitter.ActivatableClassInfo>
        {
            new("NS.Class<T>", "both")
        };
        var emitter = new ManifestEmitter();

        var sxs = emitter.EmitSxs("My&Lib.dll", classes);
        Assert.Contains("My&amp;Lib.dll", sxs);
        Assert.Contains("NS.Class&lt;T&gt;", sxs);
    }

    // -----------------------------------------------------------------------
    // 20. Class ordering is alphabetical
    // -----------------------------------------------------------------------
    [Fact]
    public void ClassOrdering_IsAlphabetical()
    {
        var classes = ExtractClasses("""
            namespace TestNs
            {
                runtimeclass Zebra
                {
                    Zebra();
                    void Z();
                }

                runtimeclass Alpha
                {
                    Alpha();
                    void A();
                }

                runtimeclass Middle
                {
                    Middle();
                    void M();
                }
            }
            """);

        Assert.Equal(3, classes.Count);
        Assert.Equal("TestNs.Alpha", classes[0].FullName);
        Assert.Equal("TestNs.Middle", classes[1].FullName);
        Assert.Equal("TestNs.Zebra", classes[2].FullName);
    }

    // -----------------------------------------------------------------------
    // 21. Enum and interface types are not included
    // -----------------------------------------------------------------------
    [Fact]
    public void NonRuntimeClasses_AreExcluded()
    {
        var classes = ExtractClasses("""
            namespace TestNs
            {
                enum Color { Red, Green, Blue };

                interface IFoo
                {
                    void Bar();
                };

                struct Point
                {
                    Int32 X;
                    Int32 Y;
                };

                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                }
            }
            """);

        Assert.Single(classes);
        Assert.Equal("TestNs.Widget", classes[0].FullName);
    }

    // -----------------------------------------------------------------------
    // 22. Multiple constructors still yields one entry
    // -----------------------------------------------------------------------
    [Fact]
    public void MultipleConstructors_SingleEntry()
    {
        var classes = ExtractClasses("""
            namespace TestNs
            {
                runtimeclass Widget
                {
                    Widget();
                    Widget(String name);
                    Widget(Int32 id, String name);
                    void DoWork();
                }
                }
            }
            """);

        Assert.Single(classes);
        Assert.Equal("TestNs.Widget", classes[0].FullName);
    }

    // -----------------------------------------------------------------------
    // 23. Deep namespace
    // -----------------------------------------------------------------------
    [Fact]
    public void DeepNamespace_FullNamePreserved()
    {
        var classes = ExtractClasses("""
            namespace Company.Product.Feature
            {
                runtimeclass DeepWidget
                {
                    DeepWidget();
                    void DoWork();
                }
            }
            """);

        Assert.Single(classes);
        Assert.Equal("Company.Product.Feature.DeepWidget", classes[0].FullName);
    }

    // -----------------------------------------------------------------------
    // 24. Mixed static and non-static classes
    // -----------------------------------------------------------------------
    [Fact]
    public void MixedStaticAndNonStatic()
    {
        var classes = ExtractClasses("""
            namespace TestNs
            {
                runtimeclass Activatable1
                {
                    Activatable1();
                    void Work1();
                }

                static runtimeclass StaticOnly1
                {
                    static void Foo();
                }

                runtimeclass Activatable2
                {
                    Activatable2();
                    String Name { get; };
                }

                static runtimeclass StaticOnly2
                {
                    static Int32 Bar();
                }
            }
            """);

        Assert.Equal(2, classes.Count);
        var names = classes.Select(c => c.FullName).ToList();
        Assert.Contains("TestNs.Activatable1", names);
        Assert.Contains("TestNs.Activatable2", names);
        Assert.DoesNotContain("TestNs.StaticOnly1", names);
        Assert.DoesNotContain("TestNs.StaticOnly2", names);
    }
}
