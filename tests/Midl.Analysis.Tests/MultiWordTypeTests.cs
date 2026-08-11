// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;

namespace Midl.Analysis.Tests;

/// <summary>
/// Multi-word C integer types — <c>unsigned long</c>, <c>long long</c>, <c>unsigned __int64</c>.
///
/// These are classic MIDL/C spellings and are <em>not</em> MIDL3. Two things follow, and both
/// matter:
///
/// They must <b>parse</b> in both modes. <c>ParseBaseTypeReference</c> consumed a single token,
/// so <c>unsigned long handle</c> left <c>long</c> unexpected and produced a cascade of ten
/// parse errors that buried every real diagnostic in the file.
///
/// They must still be <b>rejected in strict mode</b>. Parsing a construct is not endorsing it:
/// the point of folding them is that the user sees one accurate message instead of a cascade,
/// not that MIDL3 grows a C type system.
/// </summary>
public class MultiWordTypeTests
{
    /// <summary>Every multi-word spelling, with the WinRT type it corresponds to.</summary>
    public static TheoryData<string, string> Spellings => new()
    {
        { "unsigned char", "UInt8" },
        { "unsigned short", "UInt16" },
        { "unsigned int", "UInt32" },
        { "unsigned long", "UInt32" },
        { "unsigned hyper", "UInt64" },
        { "unsigned __int64", "UInt64" },
        { "signed char", "Int16" },
        { "signed short", "Int16" },
        { "signed int", "Int32" },
        { "signed long", "Int32" },
        { "long long", "Int64" },
        { "unsigned long long", "UInt64" },
    };

    private static Compilation Compile(string parameterType, CompilationMode mode) =>
        CompilationHelper.Compile(
            $"interface IFoo {{ void M([in] {parameterType} p); }}",
            referencePaths: null, mode: mode);

    private static int ParseErrors(Compilation c) =>
        c.Diagnostics.Count(d => d.Code is "MIDL1100" or "MIDL1102");

    [Theory]
    [MemberData(nameof(Spellings))]
    public void ParsesWithoutACascade_InCompatMode(string spelling, string _)
    {
        var c = Compile(spelling, CompilationMode.Compat);

        Assert.True(ParseErrors(c) == 0,
            $"'{spelling}' produced {ParseErrors(c)} parse errors: " +
            string.Join("; ", c.Diagnostics.Where(d => d.Code is "MIDL1100" or "MIDL1102").Select(d => d.Message)));
    }

    [Theory]
    [MemberData(nameof(Spellings))]
    public void ParsesWithoutACascade_InStrictMode(string spelling, string _)
    {
        var c = Compile(spelling, CompilationMode.Strict);

        Assert.True(ParseErrors(c) == 0,
            $"'{spelling}' produced {ParseErrors(c)} parse errors in strict mode");
    }

    /// <summary>
    /// In compat mode the type resolves, so nothing downstream reports it as undefined.
    /// </summary>
    [Theory]
    [MemberData(nameof(Spellings))]
    public void ResolvesInCompatMode(string spelling, string _)
    {
        var c = Compile(spelling, CompilationMode.Compat);

        Assert.DoesNotContain(c.Diagnostics, d => d.Code == "MIDL1001");
    }

    /// <summary>
    /// Strict mode must still reject it — and say so once, naming the whole type and the
    /// WinRT type to use, rather than emitting one complaint per word.
    /// </summary>
    [Theory]
    [MemberData(nameof(Spellings))]
    public void StrictModeRejectsIt_OnceForTheWholeType(string spelling, string winrtType)
    {
        var c = Compile(spelling, CompilationMode.Strict);

        var classic = c.Diagnostics.Where(d => d.Code == "MIDL2005").ToList();

        Assert.True(classic.Count == 1,
            $"expected exactly one MIDL2005 for '{spelling}', got {classic.Count}: " +
            string.Join(" | ", classic.Select(d => d.Message)));
        Assert.Contains(spelling, classic[0].Message);
        Assert.Contains(winrtType, classic[0].Message);
    }

    /// <summary>
    /// The diagnostic must point at the start of the type, not at the second word.
    /// </summary>
    [Theory]
    [MemberData(nameof(Spellings))]
    public void TheDiagnosticPointsAtTheStartOfTheType(string spelling, string _)
    {
        var source = $"interface IFoo {{ void M([in] {spelling} p); }}";
        var c = CompilationHelper.Compile(source, referencePaths: null, mode: CompilationMode.Strict);

        var d = Assert.Single(c.Diagnostics, x => x.Code == "MIDL2005");

        Assert.Equal(source.IndexOf(spelling, StringComparison.Ordinal), d.OriginalPosition ?? d.Position);
    }

    /// <summary>
    /// The whole spelling is covered, so the squiggle underlines the type rather than one word.
    /// </summary>
    [Theory]
    [MemberData(nameof(Spellings))]
    public void TheDiagnosticCoversTheWholeSpelling(string spelling, string _)
    {
        var c = Compile(spelling, CompilationMode.Strict);

        var d = Assert.Single(c.Diagnostics, x => x.Code == "MIDL2005");

        Assert.Equal(spelling.Length, d.Length);
    }

    // ---------------------------------------------------------------------
    // Guards: folding these must not smuggle C types into MIDL3.
    // ---------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(Spellings))]
    public void StrictModeStillFails(string spelling, string _)
    {
        var c = Compile(spelling, CompilationMode.Strict);

        Assert.Contains(c.Diagnostics, d => d.Severity == Diagnostics.DiagnosticSeverity.Error);
    }

    /// <summary>
    /// Proper MIDL3 types keep parsing cleanly and attract no classic-construct complaint.
    /// </summary>
    [Theory]
    [InlineData("UInt32")]
    [InlineData("Int64")]
    [InlineData("UInt8")]
    [InlineData("String")]
    public void Midl3TypesAreUnaffected(string type)
    {
        var c = Compile(type, CompilationMode.Strict);

        Assert.Equal(0, ParseErrors(c));
        Assert.DoesNotContain(c.Diagnostics, d => d.Code == "MIDL2005");
        Assert.DoesNotContain(c.Diagnostics, d => d.Code == "MIDL1001");
    }

    /// <summary>
    /// Single-word classic types keep behaving as before: still reported in strict mode.
    /// </summary>
    [Theory]
    [InlineData("long", "Int32")]
    [InlineData("short", "Int16")]
    [InlineData("hyper", "Int64")]
    public void SingleWordClassicTypesAreUnchanged(string spelling, string winrtType)
    {
        var strict = Compile(spelling, CompilationMode.Strict);
        var d = Assert.Single(strict.Diagnostics, x => x.Code == "MIDL2005");
        Assert.Contains(winrtType, d.Message);

        var compat = Compile(spelling, CompilationMode.Compat);
        Assert.Equal(0, ParseErrors(compat));
        Assert.DoesNotContain(compat.Diagnostics, x => x.Code == "MIDL1001");
    }

    /// <summary>
    /// A parameter named after a type keyword must not be swallowed by the fold: in
    /// <c>const unsigned long windowHandle</c> the name is a name, not a third type word.
    /// </summary>
    [Fact]
    public void TheParameterNameIsNotFoldedIntoTheType()
    {
        var c = CompilationHelper.Compile(
            "interface IFoo { void M([in] const unsigned long windowHandle); }",
            referencePaths: null, mode: CompilationMode.Compat);

        Assert.Equal(0, ParseErrors(c));
        Assert.DoesNotContain(c.Diagnostics, d => d.Code == "MIDL1001");
    }

    /// <summary>The real-world shape from NVDA's IDL, which is what motivated this.</summary>
    [Fact]
    public void ARealClassicSignatureParses()
    {
        const string source = """
            interface INvda
            {
                error_status_t winword_expandToLine(
                    [in] const unsigned long windowHandle,
                    [in] const int offset,
                    [out] int* lineStart,
                    [out] int* lineEnd);
            }
            """;

        var c = CompilationHelper.Compile(source, referencePaths: null, mode: CompilationMode.Compat);

        Assert.True(ParseErrors(c) == 0,
            "expected the signature to parse: " +
            string.Join("; ", c.Diagnostics.Where(d => d.Code is "MIDL1100" or "MIDL1102").Select(d => d.Message)));
    }
}
