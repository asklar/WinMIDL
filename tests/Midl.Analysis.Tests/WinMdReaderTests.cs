using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;
using Midl.CodeGen.WinMd;

namespace Midl.Analysis.Tests;

public class WinMdReaderTests
{
    /// <summary>Helper: compile IDL source to WinMD bytes using the existing emitter.</summary>
    private static byte[] EmitWinMd(string idlSource)
    {
        var compilation = CompilationHelper.Compile(idlSource);
        Assert.False(compilation.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
            "IDL compilation should not have errors");
        var emitter = new WinMdEmitter();
        return emitter.EmitToBytes(compilation);
    }

    [Fact]
    public void ReadEnumFromWinMd()
    {
        var winmd = EmitWinMd(@"
            namespace TestNs
            {
                enum MyColor
                {
                    Red,
                    Green,
                    Blue
                };
            }");

        var reader = new WinMdReader();
        var types = reader.ReadTypes(winmd);

        var enumSymbol = types.OfType<EnumSymbol>().FirstOrDefault(e => e.Name == "MyColor");
        Assert.NotNull(enumSymbol);
        Assert.Equal("TestNs.MyColor", enumSymbol.FullName);
        Assert.True(enumSymbol.IsExternal);
        Assert.Equal(3, enumSymbol.EnumMembers.Count);
        Assert.Equal("Red", enumSymbol.EnumMembers[0].Name);
        Assert.Equal("Green", enumSymbol.EnumMembers[1].Name);
        Assert.Equal("Blue", enumSymbol.EnumMembers[2].Name);
    }

    [Fact]
    public void ReadStructFromWinMd()
    {
        var winmd = EmitWinMd(@"
            namespace TestNs
            {
                struct MyPoint
                {
                    Single X;
                    Single Y;
                };
            }");

        var reader = new WinMdReader();
        var types = reader.ReadTypes(winmd);

        var structSymbol = types.OfType<StructSymbol>().FirstOrDefault(s => s.Name == "MyPoint");
        Assert.NotNull(structSymbol);
        Assert.Equal("TestNs.MyPoint", structSymbol.FullName);
        Assert.True(structSymbol.IsExternal);
        Assert.Equal(2, structSymbol.Fields.Count);
        Assert.Equal("X", structSymbol.Fields[0].Name);
        Assert.Equal("Y", structSymbol.Fields[1].Name);
    }

    [Fact]
    public void ReadInterfaceFromWinMd()
    {
        var winmd = EmitWinMd(@"
            namespace TestNs
            {
                [uuid(12345678-1234-1234-1234-123456789abc)]
                interface IMyInterface
                {
                    void DoSomething();
                    String GetName();
                };
            }");

        var reader = new WinMdReader();
        var types = reader.ReadTypes(winmd);

        var iface = types.OfType<InterfaceSymbol>().FirstOrDefault(i => i.Name == "IMyInterface");
        Assert.NotNull(iface);
        Assert.Equal("TestNs.IMyInterface", iface.FullName);
        Assert.True(iface.IsExternal);

        // Should have methods
        var methods = iface.Members.OfType<MethodSymbol>().ToList();
        Assert.True(methods.Count >= 2, $"Expected at least 2 methods, got {methods.Count}");
        Assert.Contains(methods, m => m.Name == "DoSomething");
        Assert.Contains(methods, m => m.Name == "GetName");
    }

    [Fact]
    public void ReadRuntimeClassFromWinMd()
    {
        var winmd = EmitWinMd(@"
            namespace TestNs
            {
                [uuid(AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE)]
                interface IMyClass
                {
                    void Activate();
                };

                [default_interface]
                runtimeclass MyClass
                {
                    MyClass();
                };
            }");

        var reader = new WinMdReader();
        var types = reader.ReadTypes(winmd);

        var rc = types.OfType<RuntimeClassSymbol>().FirstOrDefault(r => r.Name == "MyClass");
        Assert.NotNull(rc);
        Assert.Equal("TestNs.MyClass", rc.FullName);
        Assert.True(rc.IsExternal);
    }

    [Fact]
    public void ResolveTypeFromReference()
    {
        // Create a WinMD with a type we'll reference
        var winmd = EmitWinMd(@"
            namespace External
            {
                enum StatusCode
                {
                    Ok,
                    Error
                };
            }");

        // Write to temp file
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.winmd");
        try
        {
            File.WriteAllBytes(tempPath, winmd);

            // Compile IDL that references the external type
            var source = @"
                namespace MyApp
                {
                    runtimeclass Widget
                    {
                        External.StatusCode GetStatus();
                    };
                }";

            var compilation = CompilationHelper.Compile(source, new[] { tempPath });

            // Should have no errors — the type resolves from the reference
            Assert.False(compilation.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
                "Compilation with reference should succeed");
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void UnresolvedWithoutReference()
    {
        // Same IDL but without the reference — should fail
        var source = @"
            namespace MyApp
            {
                runtimeclass Widget
                {
                    External.StatusCode GetStatus();
                };
            }";

        var compilation = CompilationHelper.Compile(source);
        var errors = compilation.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Code == "MIDL1001");
    }

    [Fact]
    public void ExternalSymbolMarkedCorrectly()
    {
        var winmd = EmitWinMd(@"
            namespace External
            {
                struct MyData
                {
                    Int32 Value;
                };
            }");

        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.winmd");
        try
        {
            File.WriteAllBytes(tempPath, winmd);

            var source = @"
                namespace MyApp
                {
                    struct LocalData
                    {
                        Int32 Count;
                    };
                }";

            var compilation = CompilationHelper.Compile(source, new[] { tempPath });

            // External type should have IsExternal=true
            var externalType = compilation.GetTypeByName("External.MyData");
            Assert.NotNull(externalType);
            Assert.True(externalType.IsExternal);

            // User type should have IsExternal=false
            var localType = compilation.GetTypeByName("MyApp.LocalData");
            Assert.NotNull(localType);
            Assert.False(localType.IsExternal);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void ReadTypes_IndexesNonPublicWinRTTypes()
    {
        // Synthesized exclusive interfaces are emitted as non-public WinRT types.
        // The reader must index ALL WinRT-flagged types regardless of CLR visibility.
        var winmd = EmitWinMd(@"
            namespace TestNs
            {
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                };
            }");

        var reader = new WinMdReader();
        var types = reader.ReadTypes(winmd);

        // The synthesized IWidget interface is non-public but WinRT-flagged
        var iface = types.OfType<InterfaceSymbol>().FirstOrDefault(i => i.Name == "IWidget");
        Assert.NotNull(iface);
        Assert.Equal("TestNs.IWidget", iface!.FullName);
    }

    [Fact]
    public void ReadSingleType_ReadsNonPublicWinRTType()
    {
        // ReadSingleType should not filter out non-public WinRT types
        var winmd = EmitWinMd(@"
            namespace TestNs
            {
                runtimeclass Gadget
                {
                    Gadget();
                    void Activate();
                };
            }");

        using var provider = new LazyWinMdProvider(winmd);

        // The non-public synthesized IGadget should be indexed and resolvable
        Assert.True(provider.HasType("TestNs.IGadget"),
            "Non-public WinRT-flagged synthesized interface should be indexed");
        var resolved = provider.TryResolve("TestNs.IGadget");
        Assert.NotNull(resolved);
        Assert.IsType<InterfaceSymbol>(resolved);
    }
}
