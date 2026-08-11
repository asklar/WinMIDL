// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;

namespace Midl.Analysis.Tests;

/// <summary>
/// Three more classic constructs found by compiling real RPC IDL, each of which cascaded:
///
///   int getTextLength([in] Handle b) const;                    trailing const on a method
///   typedef error_status_t (__stdcall *FnType)([in] long a);   function-pointer typedef
///   [callback] error_status_t onMark([in] long a);             the callback attribute
///
/// None is MIDL3, so the rule is unchanged: parse in both modes so one classic declaration
/// costs one message rather than burying the rest of the file, and keep rejecting in strict.
/// </summary>
public class ClassicCallbackAndFunctionPointerTests
{
    private static Compilation Compile(string source, CompilationMode mode) =>
        CompilationHelper.Compile(source, referencePaths: null, mode: mode);

    private static int ParseErrors(Compilation c) =>
        c.Diagnostics.Count(d => d.Code is "MIDL1100" or "MIDL1102");

    private static string Errors(Compilation c) =>
        string.Join("; ", c.Diagnostics.Where(d => d.Code is "MIDL1100" or "MIDL1102").Select(d => d.Message));

    // ---------------------------------------------------------------------
    // Trailing const on a method — a C++ member-function qualifier.
    // ---------------------------------------------------------------------

    private const string TrailingConst = """
        interface IVBuf
        {
            int getTextLength([in] long buffer) const;
            int getText([in] long buffer);
        }
        """;

    [Theory]
    [InlineData(CompilationMode.Compat)]
    [InlineData(CompilationMode.Strict)]
    public void TrailingConstOnAMethodParses(CompilationMode mode)
    {
        var c = Compile(TrailingConst, mode);

        Assert.True(ParseErrors(c) == 0, Errors(c));
    }

    /// <summary>The method after it must survive — that is what the cascade destroyed.</summary>
    [Fact]
    public void TrailingConstDoesNotHideTheFollowingMethod()
    {
        var c = Compile(TrailingConst, CompilationMode.Compat);

        var iface = Assert.Single(c.GetAllTypes(), t => t.Name == "IVBuf");
        var members = Assert.IsType<Symbols.InterfaceSymbol>(iface).Members;
        Assert.Contains(members, m => m.Name == "getTextLength");
        Assert.Contains(members, m => m.Name == "getText");
    }

    [Fact]
    public void TrailingConstIsRejectedInStrictMode()
    {
        var c = Compile(TrailingConst, CompilationMode.Strict);

        Assert.Contains(c.Diagnostics,
            d => d.Code == "MIDL2001" && d.Message.Contains("const", StringComparison.OrdinalIgnoreCase));
    }

    // ---------------------------------------------------------------------
    // Function-pointer typedef.
    // ---------------------------------------------------------------------

    private const string FunctionPointerTypedef = """
        typedef error_status_t (__stdcall *onMarkReachedFuncType)([in, string] const wchar_t* mark);

        interface INvdaController
        {
            error_status_t setCallback([in] onMarkReachedFuncType fn);
        }
        """;

    [Theory]
    [InlineData(CompilationMode.Compat)]
    [InlineData(CompilationMode.Strict)]
    public void FunctionPointerTypedefParses(CompilationMode mode)
    {
        var c = Compile(FunctionPointerTypedef, mode);

        Assert.True(ParseErrors(c) == 0, Errors(c));
    }

    /// <summary>The name it introduces has to be usable, or every reference is undefined.</summary>
    [Fact]
    public void FunctionPointerTypedefIntroducesAUsableName()
    {
        var c = Compile(FunctionPointerTypedef, CompilationMode.Compat);

        Assert.DoesNotContain(c.Diagnostics,
            d => d.Code == "MIDL1001" && d.Message.Contains("onMarkReachedFuncType", StringComparison.Ordinal));
    }

    [Fact]
    public void FunctionPointerTypedefIsRejectedInStrictMode()
    {
        var c = Compile(FunctionPointerTypedef, CompilationMode.Strict);

        Assert.Contains(c.Diagnostics, d => d.Code == "MIDL2001");
    }

    /// <summary>A function-pointer typedef must not swallow what follows it.</summary>
    [Fact]
    public void FunctionPointerTypedefDoesNotHideLaterDeclarations()
    {
        var c = Compile(FunctionPointerTypedef, CompilationMode.Compat);

        Assert.Contains(c.GetAllTypes(), t => t.Name == "INvdaController");
    }

    // ---------------------------------------------------------------------
    // The [callback] attribute.
    // ---------------------------------------------------------------------

    private const string CallbackAttribute = """
        interface INvdaController
        {
            [callback] error_status_t onMarkReached([in, string] const wchar_t* mark);
        }
        """;

    [Theory]
    [InlineData(CompilationMode.Compat)]
    [InlineData(CompilationMode.Strict)]
    public void CallbackAttributeParses(CompilationMode mode)
    {
        var c = Compile(CallbackAttribute, mode);

        Assert.True(ParseErrors(c) == 0, Errors(c));
    }

    /// <summary>
    /// It is a standard classic MIDL attribute, so compat mode must recognise it rather than
    /// telling the author to declare it themselves.
    /// </summary>
    [Fact]
    public void CallbackAttributeIsRecognisedInCompatMode()
    {
        var c = Compile(CallbackAttribute, CompilationMode.Compat);

        Assert.DoesNotContain(c.Diagnostics,
            d => d.Code == "MIDL1065" && d.Message.Contains("callback", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CallbackAttributeIsRejectedInStrictMode()
    {
        var c = Compile(CallbackAttribute, CompilationMode.Strict);

        // Specifically that `[callback]` itself is named as a classic attribute — not
        // merely that some error exists, which would also be satisfied by the
        // error_status_t / wchar_t / [in] in the same fixture.
        Assert.Contains(c.Diagnostics,
            d => d.Code == "MIDL2003" && d.Message.Contains("callback", StringComparison.OrdinalIgnoreCase));
    }

    // ---------------------------------------------------------------------
    // Guard: none of this changes MIDL3.
    // ---------------------------------------------------------------------

    [Fact]
    public void Midl3IsUnaffected()
    {
        const string source = """
            namespace N
            {
                runtimeclass Widget
                {
                    Widget();
                    Int32 Value { get; };
                    void Go(Int32 p);
                }
            }
            """;

        var c = Compile(source, CompilationMode.Strict);

        Assert.Equal(0, ParseErrors(c));
        Assert.DoesNotContain(c.Diagnostics, d => d.Code == "MIDL2001");
    }

    /// <summary>
    /// A trailing const is a method qualifier; 'ref const' remains the MIDL3 parameter
    /// direction modifier and must keep working untouched.
    /// </summary>
    [Fact]
    public void RefConstParametersAreStillValidMidl3()
    {
        const string source = """
            namespace N
            {
                struct S { Int32 A; };
                runtimeclass Widget
                {
                    Widget();
                    void Go(ref const S value);
                }
            }
            """;

        var c = Compile(source, CompilationMode.Strict);

        Assert.Equal(0, ParseErrors(c));
        Assert.DoesNotContain(c.Diagnostics,
            d => d.Code == "MIDL2001" && d.Message.Contains("const", StringComparison.OrdinalIgnoreCase));
    }

    // ---------------------------------------------------------------------
    // Top-level free function — classic RPC declares operations at file scope,
    // outside any interface. nvdaController.idl:76 is exactly this.
    // ---------------------------------------------------------------------

    private const string TopLevelFunction = """
        typedef long error_status_t;

        error_status_t __stdcall setCallback(long callback);

        interface INvdaController
        {
            long testIfRunning();
        }
        """;

    [Theory]
    [InlineData(CompilationMode.Compat)]
    [InlineData(CompilationMode.Strict)]
    public void TopLevelFunctionParses(CompilationMode mode)
    {
        var c = Compile(TopLevelFunction, mode);
        Assert.True(ParseErrors(c) == 0, $"expected no parse errors in {mode}, got: {Errors(c)}");
    }

    [Fact]
    public void TopLevelFunctionDoesNotSwallowWhatFollows()
    {
        // The point of parsing it: the interface after it must still be found.
        var c = Compile(TopLevelFunction, CompilationMode.Compat);
        Assert.Contains(c.GetAllTypes(), t => t.Name == "INvdaController");
    }

    [Fact]
    public void TopLevelFunctionIsRejectedInStrictMode()
    {
        var c = Compile(TopLevelFunction, CompilationMode.Strict);
        Assert.Contains(c.Diagnostics, d => d.Code == "MIDL2001" && d.Message.Contains("function"));
    }

    [Fact]
    public void TopLevelFunctionIsAcceptedInCompatMode()
    {
        var c = Compile(TopLevelFunction, CompilationMode.Compat);
        Assert.DoesNotContain(c.Diagnostics, d => d.Code == "MIDL2001");
    }

    // ---------------------------------------------------------------------
    // Position and guard tightness. The whole point of parsing these constructs
    // is to report them accurately; a diagnostic on the wrong line is the bug
    // this work exists to prevent.
    // ---------------------------------------------------------------------

    [Fact]
    public void TopLevelFunctionDiagnosticPointsAtTheFunctionName()
    {
        var c = Compile(TopLevelFunction, CompilationMode.Strict);

        var d = Assert.Single(c.Diagnostics, x => x.Code == "MIDL2001" && x.Message.Contains("function"));
        int expected = TopLevelFunction.IndexOf("setCallback", StringComparison.Ordinal);
        Assert.True(d.Position == expected,
            $"expected position {expected} (the name 'setCallback'), got {d.Position} " +
            $"which is at '{TopLevelFunction.Substring(Math.Max(0, d.Position), 20)}'");
    }

    [Fact]
    public void ASingleIdentifierCallIsNotMistakenForAFunctionDeclaration()
    {
        // Guard on the two-word requirement in the lookahead. `SOME_MACRO(1);` has one
        // word before the paren, so it is not a declaration. Accepting it produces two
        // "Expected identifier" messages pointing into the middle of the line instead of
        // one that names the construct — the cascade this work exists to prevent.
        var c = Compile("SOME_MACRO(1);\n\ninterface IFoo { long f(); }", CompilationMode.Compat);

        Assert.Equal(1, ParseErrors(c));
        Assert.Contains(c.Diagnostics, d => d.Code == "MIDL1100");
        Assert.Contains(c.GetAllTypes(), t => t.Name == "IFoo");
    }

    [Fact]
    public void TrailingConstDiagnosticPointsAtTheConstKeyword()
    {
        var c = Compile(TrailingConst, CompilationMode.Strict);

        var d = Assert.Single(c.Diagnostics, x => x.Code == "MIDL2001" && x.Message.Contains("const"));
        int expected = TrailingConst.IndexOf(") const", StringComparison.Ordinal) + 2;
        Assert.True(d.Position == expected,
            $"expected position {expected} (the trailing 'const'), got {d.Position}");
    }
}
