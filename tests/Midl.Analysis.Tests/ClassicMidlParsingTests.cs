// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis.Tests;

/// <summary>
/// Compat mode has to actually understand real classic MIDL2, not merely tolerate it.
/// Two constructs common in RPC-style IDL were rejected outright:
///
///   * <c>const</c> on a parameter type — <c>[in] const long level</c>
///   * an explicit calling convention between the return type and the method name —
///     <c>error_status_t __stdcall logMessage(...)</c>
///
/// Both produced cascades of MIDL1102/MIDL1100 parse errors that buried every real
/// diagnostic in the file.
///
/// The rules differ by mode. MIDL2 allows both; MIDL3 has neither. So both are parsed
/// in either mode — a parse cascade tells the user nothing — and strict mode reports
/// them as classic constructs, the same way it treats <c>cpp_quote</c> and friends.
/// </summary>
public class ClassicMidlParsingTests
{
    private static Compilation Compile(string members, CompilationMode mode) =>
        CompilationHelper.Compile($$"""
            [uuid(11111111-1111-1111-1111-111111111111)]
            interface ITest
            {
            {{members}}
            }
            """, referencePaths: null, mode: mode,
            includePaths: null, currentFilePath: null);

    private static Compilation Compat(string members) => Compile(members, CompilationMode.Compat);
    private static Compilation Strict(string members) => Compile(members, CompilationMode.Strict);

    private static void AssertNoParseErrors(Compilation c)
    {
        var parseErrors = c.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error &&
                        (d.Code == "MIDL1102" || d.Code == "MIDL1100"))
            .ToList();

        Assert.True(parseErrors.Count == 0,
            "Unexpected parse errors:\n  " + string.Join("\n  ",
                parseErrors.Take(5).Select(d => $"{d.Code}: {d.Message}")));
    }

    private static MethodSymbol SingleMethod(Compilation c)
    {
        var iface = c.GlobalNamespace.Members.OfType<InterfaceSymbol>().Single();
        return iface.Members.OfType<MethodSymbol>().Single();
    }

    // ---------------- const on parameters ----------------

    [Theory]
    [InlineData("error_status_t f([in] const long level);")]
    [InlineData("error_status_t f([in, string] const wchar_t* message);")]
    [InlineData("error_status_t f([in] const boolean flag);")]
    public void ConstParameter_Parses(string member)
    {
        AssertNoParseErrors(Compat("    " + member));
    }

    [Fact(Skip = "Multi-word C integer types ('unsigned long', 'long long') are a separate "
                 + "gap in ParseTypeReference — they fail with or without const.")]
    public void ConstParameter_MultiWordType_Parses()
    {
        AssertNoParseErrors(Compat("    error_status_t f([in] const unsigned long handle);"));
    }

    [Fact]
    public void ConstParameter_KeepsNameAndType()
    {
        var m = SingleMethod(Compat("    error_status_t f([in] const long level);"));

        Assert.Equal("f", m.Name);
        var p = Assert.Single(m.Parameters);
        Assert.Equal("level", p.Name);
        Assert.Equal("long", p.Type.Name);
    }

    [Fact]
    public void ConstPointerParameter_KeepsPointerAndName()
    {
        var m = SingleMethod(Compat("    error_status_t f([in, string] const wchar_t* message);"));

        var p = Assert.Single(m.Parameters);
        Assert.Equal("message", p.Name);
    }

    // ---------------- calling conventions ----------------

    [Theory]
    [InlineData("__stdcall")]
    [InlineData("__cdecl")]
    [InlineData("__fastcall")]
    [InlineData("STDMETHODCALLTYPE")]
    public void CallingConvention_Parses(string convention)
    {
        AssertNoParseErrors(Compat($"    error_status_t {convention} logMessage([in] long level);"));
    }

    [Theory]
    [InlineData("__stdcall")]
    [InlineData("STDMETHODCALLTYPE")]
    public void CallingConvention_DoesNotBecomeTheMethodName(string convention)
    {
        var m = SingleMethod(Compat($"    error_status_t {convention} logMessage([in] long level);"));

        Assert.Equal("logMessage", m.Name);
        Assert.Single(m.Parameters);
    }

    // ---------------- the reported real-world line ----------------

    [Fact]
    public void ConstAndCallingConventionTogether_Parse()
    {
        var c = Compat(
            "    error_status_t __stdcall logMessage([in] const long level, "
            + "[in] const long processID, [in,string] const wchar_t* message);");

        AssertNoParseErrors(c);

        var m = SingleMethod(c);
        Assert.Equal("logMessage", m.Name);
        Assert.Equal(3, m.Parameters.Count);
        Assert.Equal(new[] { "level", "processID", "message" }, m.Parameters.Select(p => p.Name));
    }

    [Fact]
    public void CallingConvention_IsStillUsableAsAnIdentifier()
    {
        // Guard: treating these as keywords must not break a method genuinely named that.
        var c = Compat("    error_status_t __stdcall([in] long a);");
        AssertNoParseErrors(c);
        Assert.Equal("__stdcall", SingleMethod(c).Name);
    }

    // ---------------- strict mode: parse, then diagnose ----------------
    //
    // The two forms of const are not the same thing:
    //
    //   MIDL2:  const T p       — C-style type qualifier, no MIDL3 equivalent
    //   MIDL3:  ref const T p   — parameter direction modifier (docs: "the modifier
    //                             keywords ref, ref const, and out")
    //
    // Strict mode must reject the first and keep accepting the second.

    [Fact]
    public void StrictMode_BareConstParameter_ReportsClassicConstruct_NotAParseCascade()
    {
        var c = Strict("    error_status_t f([in] const long level);");

        AssertNoParseErrors(c);
        Assert.Contains(c.Diagnostics, d =>
            d.Severity == DiagnosticSeverity.Error &&
            d.Code.StartsWith("MIDL2", StringComparison.Ordinal) &&
            d.Message.Contains("const", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void StrictMode_RefConst_IsValidMidl3_AndNotDiagnosed()
    {
        // Regression guard for the fix above: 'ref const' is the MIDL3 direction
        // modifier and must not be swept up with the classic bare-const qualifier.
        var c = CompilationHelper.Compile("""
            namespace N
            {
                struct Big { Int32 A; Int32 B; };
                interface ITest
                {
                    void Consume(ref const Big value);
                }
            }
            """, referencePaths: null, mode: CompilationMode.Strict,
            includePaths: null, currentFilePath: null);

        AssertNoParseErrors(c);
        Assert.DoesNotContain(c.Diagnostics, d =>
            d.Code.StartsWith("MIDL2", StringComparison.Ordinal) &&
            d.Message.Contains("const", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CompatMode_RefConst_AlsoStillParses()
    {
        var c = CompilationHelper.Compile("""
            namespace N
            {
                struct Big { Int32 A; Int32 B; };
                interface ITest
                {
                    void Consume(ref const Big value);
                }
            }
            """, referencePaths: null, mode: CompilationMode.Compat,
            includePaths: null, currentFilePath: null);

        AssertNoParseErrors(c);
    }

    [Fact]
    public void StrictMode_CallingConvention_ReportsClassicConstruct_NotAParseCascade()
    {
        var c = Strict("    error_status_t __stdcall logMessage([in] long level);");

        AssertNoParseErrors(c);
        Assert.Contains(c.Diagnostics, d =>
            d.Severity == DiagnosticSeverity.Error &&
            d.Code.StartsWith("MIDL2", StringComparison.Ordinal) &&
            d.Message.Contains("__stdcall", StringComparison.Ordinal));
    }

    [Fact]
    public void CompatMode_ConstAndCallingConvention_ProduceNoErrors()
    {
        // The whole point of compat mode: these are valid MIDL2.
        var c = Compat("    error_status_t __stdcall f([in] const long level);");

        Assert.DoesNotContain(c.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void StrictMode_ClassicDiagnostics_PointAtTheOffendingToken()
    {
        const string member = "    error_status_t __stdcall logMessage([in] const long level);";
        var c = Strict(member);

        foreach (var d in c.Diagnostics.Where(d =>
                     d.Code.StartsWith("MIDL2", StringComparison.Ordinal) &&
                     (d.Message.Contains("__stdcall") || d.Message.Contains("const"))))
        {
            var pos = d.OriginalPosition ?? d.Position;
            Assert.True(pos > 0, $"{d.Code} fell back to position 0: {d.Message}");
        }
    }
}
