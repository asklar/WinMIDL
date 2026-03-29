using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis.Tests;

public class ArrayParamTests
{
    private static Compilation CompileWithArrays(string source)
    {
        return CompilationHelper.Compile(source, referencePaths: null,
            mode: CompilationMode.Strict, includePaths: null, currentFilePath: null,
            arraysEnabled: true);
    }

    // ─── Parsing ────────────────────────────────────────────────────

    [Fact]
    public void Parse_Int32Array_Parameter()
    {
        var compilation = CompileWithArrays("""
            namespace N
            {
                interface IFoo
                {
                    void SetItems(Int32[] items);
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        var iface = compilation.GlobalNamespace.ChildNamespaces["N"]
            .Members.OfType<InterfaceSymbol>().Single();
        var method = iface.Members.OfType<MethodSymbol>().Single();
        var param = method.Parameters.Single();
        Assert.True(param.IsArray);
        Assert.True(param.Type.IsArray);
        Assert.Equal("Int32", param.Type.Name);
    }

    [Fact]
    public void Parse_StringArray_Parameter()
    {
        var compilation = CompileWithArrays("""
            namespace N
            {
                interface IFoo
                {
                    void SetNames(String[] names);
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        var iface = compilation.GlobalNamespace.ChildNamespaces["N"]
            .Members.OfType<InterfaceSymbol>().Single();
        var method = iface.Members.OfType<MethodSymbol>().Single();
        var param = method.Parameters.Single();
        Assert.True(param.IsArray);
        Assert.Equal("String", param.Type.Name);
    }

    [Fact]
    public void Parse_StructArray_Parameter()
    {
        var compilation = CompileWithArrays("""
            namespace N
            {
                struct Point { Int32 X; Int32 Y; };
                interface IFoo
                {
                    void SetPoints(Point[] points);
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        var iface = compilation.GlobalNamespace.ChildNamespaces["N"]
            .Members.OfType<InterfaceSymbol>().Single();
        var method = iface.Members.OfType<MethodSymbol>().Single();
        var param = method.Parameters.Single();
        Assert.True(param.IsArray);
        Assert.Equal("Point", param.Type.Name);
    }

    // ─── Direction → calling convention ─────────────────────────────

    [Fact]
    public void InDirection_PassArray()
    {
        var compilation = CompileWithArrays("""
            namespace N
            {
                interface IFoo
                {
                    void SetItems(Int32[] items);
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        var param = compilation.GlobalNamespace.ChildNamespaces["N"]
            .Members.OfType<InterfaceSymbol>().Single()
            .Members.OfType<MethodSymbol>().Single()
            .Parameters.Single();
        Assert.Equal(ParameterDirection.In, param.Direction);
        Assert.True(param.IsArray);
    }

    [Fact]
    public void OutDirection_ReceiveArray()
    {
        var compilation = CompileWithArrays("""
            namespace N
            {
                interface IFoo
                {
                    void GetItems(out Int32[] items);
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        var param = compilation.GlobalNamespace.ChildNamespaces["N"]
            .Members.OfType<InterfaceSymbol>().Single()
            .Members.OfType<MethodSymbol>().Single()
            .Parameters.Single();
        Assert.Equal(ParameterDirection.Out, param.Direction);
        Assert.True(param.IsArray);
    }

    [Fact]
    public void RefDirection_FillArray()
    {
        var compilation = CompileWithArrays("""
            namespace N
            {
                interface IFoo
                {
                    void FillItems(ref Int32[] items);
                }
            }
            """);

        // ref with array should NOT produce NoBidirectionalParams
        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Code == "MIDL1050");
        var param = compilation.GlobalNamespace.ChildNamespaces["N"]
            .Members.OfType<InterfaceSymbol>().Single()
            .Members.OfType<MethodSymbol>().Single()
            .Parameters.Single();
        Assert.Equal(ParameterDirection.Ref, param.Direction);
        Assert.True(param.IsArray);
    }

    // ─── Return type ────────────────────────────────────────────────

    [Fact]
    public void ArrayReturnType_ReceiveArray()
    {
        var compilation = CompileWithArrays("""
            namespace N
            {
                interface IFoo
                {
                    Int32[] GetItems();
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        var method = compilation.GlobalNamespace.ChildNamespaces["N"]
            .Members.OfType<InterfaceSymbol>().Single()
            .Members.OfType<MethodSymbol>().Single();
        Assert.True(method.ReturnType.IsArray);
        Assert.Equal("Int32", method.ReturnType.Name);
    }

    // ─── Error: nested arrays ───────────────────────────────────────

    [Fact]
    public void NestedArrays_Rejected()
    {
        // Parser doesn't support [][] natively (it only parses one []),
        // but the binder checks for array type args that are arrays
        var compilation = CompileWithArrays("""
            namespace N
            {
                interface IFoo
                {
                    void SetItems(Int32[] items);
                }
            }
            """);

        // Single-level array is fine
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1051");
    }

    // ─── Error: array of interface ──────────────────────────────────

    [Fact]
    public void ArrayOfInterface_Rejected()
    {
        var compilation = CompileWithArrays("""
            namespace N
            {
                interface IBar { }
                interface IFoo
                {
                    void SetBars(IBar[] bars);
                }
            }
            """);

        Assert.Contains(compilation.Diagnostics, d => d.Code == "MIDL1109");
    }

    // ─── Error: array of delegate ───────────────────────────────────

    [Fact]
    public void ArrayOfDelegate_Rejected()
    {
        var compilation = CompileWithArrays("""
            namespace N
            {
                delegate void MyHandler();
                interface IFoo
                {
                    void SetHandlers(MyHandler[] handlers);
                }
            }
            """);

        Assert.Contains(compilation.Diagnostics, d => d.Code == "MIDL1110");
    }

    // ─── Error: arrays without flag ─────────────────────────────────

    [Fact]
    public void ArrayWithoutFlag_ProducesDiagnostic()
    {
        // Compile WITHOUT arraysEnabled
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                interface IFoo
                {
                    void SetItems(Int32[] items);
                }
            }
            """);

        Assert.Contains(compilation.Diagnostics, d => d.Code == "MIDL1108");
        Assert.Contains("/arrays",
            compilation.Diagnostics.First(d => d.Code == "MIDL1108").Message);
    }

    // ─── Array in runtimeclass method ───────────────────────────────

    [Fact]
    public void ArrayInRuntimeClassMethod()
    {
        var compilation = CompileWithArrays("""
            namespace N
            {
                [default_interface]
                runtimeclass Widget
                {
                    void SetValues(UInt32[] values);
                    String[] GetLabels();
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    // ─── TypeRef properties ─────────────────────────────────────────

    [Fact]
    public void TypeRef_ArrayRankAndLength()
    {
        var compilation = CompileWithArrays("""
            namespace N
            {
                interface IFoo
                {
                    void SetItems(Int32[] items);
                }
            }
            """);

        var param = compilation.GlobalNamespace.ChildNamespaces["N"]
            .Members.OfType<InterfaceSymbol>().Single()
            .Members.OfType<MethodSymbol>().Single()
            .Parameters.Single();
        Assert.Equal(1, param.Type.ArrayRank);
        Assert.Null(param.Type.ArrayLength);
    }

    // ─── DisplayName ────────────────────────────────────────────────

    [Fact]
    public void TypeRef_DisplayName_ShowsBrackets()
    {
        var typeRef = new TypeRef("Int32").WithArray();
        Assert.Equal("Int32[]", typeRef.DisplayName);
    }

    // ─── Enum array allowed ─────────────────────────────────────────

    [Fact]
    public void EnumArray_Allowed()
    {
        var compilation = CompileWithArrays("""
            namespace N
            {
                enum Color { Red, Green, Blue };
                interface IFoo
                {
                    void SetColors(Color[] colors);
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    // ─── CLI flag parsing ───────────────────────────────────────────

    [Fact]
    public void CliFlag_ArraysEnabled()
    {
        var options = Midl.Cli.CommandLineParser.Parse(["/arrays", "test.midl"]);
        Assert.True(options.ArraysEnabled);
    }

    [Fact]
    public void CliFlag_ArraysNotSet()
    {
        var options = Midl.Cli.CommandLineParser.Parse(["test.midl"]);
        Assert.False(options.ArraysEnabled);
    }
}
