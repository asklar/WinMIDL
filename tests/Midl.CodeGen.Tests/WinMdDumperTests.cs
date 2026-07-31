// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.CodeGen.Decompiler;
using Midl.CodeGen.WinMd;

namespace Midl.CodeGen.Tests;

public class WinMdDumperTests
{
    private static byte[] CompileAndEmit(string source)
    {
        var compilation = CompilationHelper.Compile(source);
        Assert.Empty(compilation.Diagnostics);
        var emitter = new WinMdEmitter();
        return emitter.EmitToBytes(compilation);
    }

    [Fact]
    public void DumpContainsExpectedTypeNamesAndMembers()
    {
        var idl = """
            namespace Test
            {
                enum Color { Red, Green, Blue }
                struct Point { Single X; Single Y; }
                delegate void Callback();
                interface IWidget
                {
                    void Activate();
                    String Name { get; };
                }
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                }
            }
            """;

        var bytes = CompileAndEmit(idl);
        var dump = WinMdDumper.Dump(bytes);

        // Type names
        Assert.Contains("[Enum] Test.Color", dump);
        Assert.Contains("[Struct] Test.Point", dump);
        Assert.Contains("[Delegate] Test.Callback", dump);
        Assert.Contains("[Interface] Test.IWidget", dump);
        Assert.Contains("[RuntimeClass] Test.Widget", dump);

        // Enum members (enum fields show up as methods/fields)
        Assert.Contains("Red", dump);
        Assert.Contains("Green", dump);
        Assert.Contains("Blue", dump);

        // Struct fields
        Assert.Contains("Single X", dump);
        Assert.Contains("Single Y", dump);

        // Interface methods
        Assert.Contains("void Activate(", dump);

        // Interface properties
        Assert.Contains("Name", dump);

        // RuntimeClass methods
        Assert.Contains("void DoWork(", dump);

        // Flags always include WindowsRuntime for WinRT types
        Assert.Contains("WindowsRuntime", dump);
    }

    [Fact]
    public void DumpIsSortedByTypeName()
    {
        var idl = """
            namespace Zebra
            {
                enum Z { A, }
            }
            namespace Alpha
            {
                enum A { X, }
            }
            """;

        var bytes = CompileAndEmit(idl);
        var dump = WinMdDumper.Dump(bytes);

        var alphaPos = dump.IndexOf("[Enum] Alpha.A");
        var zebraPos = dump.IndexOf("[Enum] Zebra.Z");

        Assert.True(alphaPos >= 0, "Alpha.A should appear in dump");
        Assert.True(zebraPos >= 0, "Zebra.Z should appear in dump");
        Assert.True(alphaPos < zebraPos, "Alpha.A should appear before Zebra.Z (sorted)");
    }

    [Fact]
    public void DumpSkipsModuleAndNonPublicTypes()
    {
        var idl = """
            namespace Test
            {
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                }
            }
            """;

        var bytes = CompileAndEmit(idl);
        var dump = WinMdDumper.Dump(bytes);

        Assert.DoesNotContain("<Module>", dump);
        // Synthesized non-public interfaces (IWidget, IWidgetFactory) should not appear
        Assert.DoesNotContain("[Interface] Test.IWidget", dump);
    }

    [Fact]
    public void RoundTripCompileDumpVerify()
    {
        var idl = """
            namespace MyApp
            {
                enum Status { Active, Inactive }
                struct Dimensions { Single Width; Single Height; }
                delegate void Handler(Int32 code);
                interface IService
                {
                    void Start();
                    Boolean IsRunning { get; };
                }
                runtimeclass Service
                {
                    Service();
                    void Stop();
                }
            }
            """;

        // Compile IDL → emit WinMD
        var bytes = CompileAndEmit(idl);

        // Dump the WinMD
        var dump = WinMdDumper.Dump(bytes);

        // Verify all expected types present
        Assert.Contains("[Enum] MyApp.Status", dump);
        Assert.Contains("[Struct] MyApp.Dimensions", dump);
        Assert.Contains("[Delegate] MyApp.Handler", dump);
        Assert.Contains("[Interface] MyApp.IService", dump);
        Assert.Contains("[RuntimeClass] MyApp.Service", dump);

        // Verify members
        Assert.Contains("Active", dump);
        Assert.Contains("Inactive", dump);
        Assert.Contains("Single Width", dump);
        Assert.Contains("Single Height", dump);
        Assert.Contains("void Start(", dump);
        Assert.Contains("IsRunning", dump);
        Assert.Contains("void Stop(", dump);

        // Verify sections
        Assert.Contains("Base:", dump);
        Assert.Contains("Flags:", dump);
        Assert.Contains("Methods:", dump);
    }
}
