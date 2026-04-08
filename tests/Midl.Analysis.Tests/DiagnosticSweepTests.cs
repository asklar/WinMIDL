using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis.Tests;

public class DiagnosticSweepTests
{
    [Fact]
    public void ConstructorInInterfaceErrors()
    {
        // The parser rejects constructor syntax in interface bodies.
        // The binder also has a defensive check (MIDL1039) in case the parser ever
        // produces a ConstructorDeclaration node in interface context.
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                interface I
                {
                    I();
                }
            }
            """);

        Assert.NotEmpty(compilation.Diagnostics);
        Assert.All(compilation.Diagnostics, d => Assert.Equal(DiagnosticSeverity.Error, d.Severity));
    }

    [Fact]
    public void StaticInInterfaceErrors()
    {
        // The parser rejects static modifiers in interface member context.
        // The binder also has a defensive check (MIDL1040) for static interface members.
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                interface I
                {
                    static void M();
                }
            }
            """);

        Assert.NotEmpty(compilation.Diagnostics);
        Assert.Contains(compilation.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void PropertyInStructErrors()
    {
        // The parser rejects property syntax in struct bodies (only fields are valid).
        // The binder also has a defensive check (MIDL1041) for properties in structs.
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                struct S
                {
                    Int32 X;
                    String Name { get; };
                }
            }
            """);

        Assert.NotEmpty(compilation.Diagnostics);
        Assert.Contains(compilation.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void EventInStructErrors()
    {
        // The parser rejects event syntax in struct bodies.
        // The binder also has a defensive check (MIDL1041) for events in structs.
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                delegate void Handler();
                struct S
                {
                    Int32 X;
                    event Handler E;
                }
            }
            """);

        Assert.NotEmpty(compilation.Diagnostics);
        Assert.Contains(compilation.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void EventWithNonDelegateTypeErrors()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                runtimeclass C
                {
                    event Int32 Changed;
                }
            }
            """);

        Assert.Contains(compilation.Diagnostics, d =>
            d.Code == "MIDL1042" && d.Severity == DiagnosticSeverity.Error
            && d.Message.Contains("Int32") && d.Message.Contains("not a delegate type"));
    }

    [Fact]
    public void EventWithNonDelegateResolvedTypeErrors()
    {
        // Event type resolves to a struct (not a delegate)
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                struct Point { Int32 X; Int32 Y; }
                runtimeclass C
                {
                    event Point Changed;
                }
            }
            """);

        Assert.Contains(compilation.Diagnostics, d =>
            d.Code == "MIDL1042" && d.Severity == DiagnosticSeverity.Error
            && d.Message.Contains("Point") && d.Message.Contains("not a delegate type"));
    }

    [Fact]
    public void VoidParameterErrors()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                runtimeclass C
                {
                    void M(void x);
                }
            }
            """);

        Assert.Contains(compilation.Diagnostics, d =>
            d.Code == "MIDL1043" && d.Severity == DiagnosticSeverity.Error
            && d.Message.Contains("'void' is not valid as a parameter type"));
    }

    [Fact]
    public void DuplicateMethodNameWarns()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                runtimeclass C
                {
                    void Do(Int32 x);
                    void Do(String x);
                }
            }
            """);

        // Should have MIDL1044 (duplicate method name) and possibly MIDL1011 (same arity)
        Assert.Contains(compilation.Diagnostics, d =>
            d.Code == "MIDL1044" && d.Severity == DiagnosticSeverity.Warning
            && d.Message.Contains("Do") && d.Message.Contains("[method_name("));
    }

    [Fact]
    public void DuplicateMethodName_SuggestsMethodNameNotOverload()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                runtimeclass C
                {
                    void Do(Int32 x);
                    void Do(String x);
                }
            }
            """);

        var diag = compilation.Diagnostics.First(d => d.Code == "MIDL1044");
        // Must NOT recommend [overload]
        Assert.DoesNotContain("[overload(", diag.Message);
        // Must recommend [method_name("UniqueName")]
        Assert.Contains("[method_name(", diag.Message);
        // Must mention [default_overload]
        Assert.Contains("[default_overload]", diag.Message);
    }

    [Fact]
    public void DuplicateMethodWithMethodNameOk()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                runtimeclass C
                {
                    void Do(Int32 x);
                    [method_name("DoWithString")]
                    void Do(String x);
                }
            }
            """);

        // [method_name] is present on at least one, so no MIDL1044
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1044");
    }

    [Fact]
    public void DuplicateMethodWithOverloadOk()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                runtimeclass C
                {
                    [overload("DoInt")]
                    void Do(Int32 x);
                    void Do(String x);
                }
            }
            """);

        // [overload] is present on at least one, so no MIDL1044
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1044");
    }

    [Fact]
    public void CircularStructErrors()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                struct A
                {
                    B b;
                }
                struct B
                {
                    A a;
                }
            }
            """);

        Assert.Contains(compilation.Diagnostics, d =>
            d.Code == "MIDL1045" && d.Severity == DiagnosticSeverity.Error
            && d.Message.Contains("Circular reference"));
    }

    [Fact]
    public void DuplicateEnumMemberErrors()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                enum E
                {
                    A,
                    B,
                    A,
                }
            }
            """);

        Assert.Contains(compilation.Diagnostics, d =>
            d.Code == "MIDL1046" && d.Severity == DiagnosticSeverity.Error
            && d.Message.Contains("Duplicate enum member") && d.Message.Contains("A"));
    }

    [Fact]
    public void ImportNotFoundErrors()
    {
        var compilation = CompilationHelper.Compile(
            """
            import "nonexistent.idl";
            namespace N
            {
                runtimeclass C { }
            }
            """,
            referencePaths: null,
            mode: CompilationMode.Strict,
            includePaths: null,
            currentFilePath: @"C:\fake\test.idl");

        Assert.Contains(compilation.Diagnostics, d =>
            d.Code == "MIDL1047" && d.Severity == DiagnosticSeverity.Error
            && d.Message.Contains("nonexistent.idl"));
    }

    [Fact]
    public void ValidCodeNoDiagnostics()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                delegate void MyHandler(Int32 code);

                enum Color
                {
                    Red,
                    Green,
                    Blue,
                }

                struct Point
                {
                    Int32 X;
                    Int32 Y;
                }

                interface IGreeter
                {
                    String Greet(String name);
                    String Greeting { get; };
                    event MyHandler OnGreet;
                }

                runtimeclass Widget
                {
                    Widget();
                    Widget(String name);
                    void DoWork(Int32 count);
                    String Name { get; set; };
                    event MyHandler OnChanged;
                    static void Reset();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
    }

    [Fact]
    public void SameNameMethodsDifferentContractVersions_NoMIDL1044()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [contractversion(2)]
                apicontract PaletteContract {};

                [contract(PaletteContract, 1)]
                runtimeclass PaletteExtension
                {
                    static String CreateExtension(Int32 reporter);

                    [contract(PaletteContract, 2)]
                    {
                        static String CreateExtension(String host);
                    }
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1044");
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1011");
    }

    [Fact]
    public void SameNameMethodsNoContractBlock_StillWarnsMIDL1044()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass Widget
                {
                    void DoWork(Int32 x);
                    void DoWork(String s);
                }
            }
            """);

        Assert.Contains(compilation.Diagnostics, d =>
            d.Code == "MIDL1044" && d.Message.Contains("DoWork"));
    }

    [Fact]
    public void SameNameMethodsSameContractVersion_StillWarnsMIDL1044()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [contractversion(1)]
                apicontract TestContract {};

                [contract(TestContract, 1)]
                runtimeclass Widget
                {
                    void DoWork(Int32 x);
                    void DoWork(String s);
                }
            }
            """);

        Assert.Contains(compilation.Diagnostics, d =>
            d.Code == "MIDL1044" && d.Message.Contains("DoWork"));
    }

    [Fact]
    public void SameNameMethodsMixedContractAndNoContract_NoMIDL1044()
    {
        // One method has no contract version (default interface), the other has a contract
        // version (versioned interface) — they're on different synthesized interfaces.
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [contractversion(1)]
                apicontract TestContract {};

                runtimeclass Widget
                {
                    void DoWork(Int32 x);

                    [contract(TestContract, 1)]
                    {
                        void DoWork(String s);
                    }
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1044");
    }

    [Fact]
    public void SameNameMethodsDifferentContractVersions_NoDefaultOverloadRequired()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [contractversion(2)]
                apicontract PaletteContract {};

                [contract(PaletteContract, 1)]
                runtimeclass PaletteExtension
                {
                    static String CreateExtension(Int32 reporter);

                    [contract(PaletteContract, 2)]
                    {
                        static String CreateExtension(String host);
                    }
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1044");
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1011");
        Assert.DoesNotContain(compilation.Diagnostics, d =>
            d.Code is "MIDL1055" or "MIDL1059");
    }

    [Fact]
    public void SameNameMethodsDifferentContractNames_SameVersion_NoMIDL1044()
    {
        // Different contract names with the same version number are on different
        // synthesized interfaces — no warning needed.
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [contractversion(1)]
                apicontract FooContract {};

                [contractversion(1)]
                apicontract BarContract {};

                runtimeclass Widget
                {
                    [contract(FooContract, 1)]
                    {
                        static String Create(Int32 reporter);
                    }

                    [contract(BarContract, 1)]
                    {
                        static String Create(String host);
                    }
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1044");
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1011");
    }

    [Fact]
    public void SameNameMethodsSameContractNameAndVersion_StillWarnsMIDL1044()
    {
        // Same contract name + same version → same synthesized interface → must warn.
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [contractversion(2)]
                apicontract TestContract {};

                [contract(TestContract, 1)]
                runtimeclass Widget
                {
                    void DoWork(Int32 x);
                    void DoWork(String s);
                }
            }
            """);

        Assert.Contains(compilation.Diagnostics, d =>
            d.Code == "MIDL1044" && d.Message.Contains("DoWork"));
    }

    [Fact]
    public void SameNameMethodsBothNullContractVersion_StillWarnsMIDL1044()
    {
        // Neither method has a contract block → both null → same interface → must warn.
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass Widget
                {
                    void Process(Int32 x);
                    void Process(String s);
                }
            }
            """);

        Assert.Contains(compilation.Diagnostics, d =>
            d.Code == "MIDL1044" && d.Message.Contains("Process"));
    }
}
