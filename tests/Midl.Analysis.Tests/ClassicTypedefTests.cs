// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;

namespace Midl.Analysis.Tests;

/// <summary>
/// Classic MIDL <c>typedef</c> in all the shapes real IDL uses.
///
/// <c>typedef</c> is not MIDL3, so as with multi-word C types the rule is: parse it in both
/// modes so one classic declaration produces one message rather than a cascade, and keep
/// rejecting it in strict mode. Parsing a construct is not endorsing it.
///
/// Several shapes already parsed — tagged struct and enum, simple and attributed aliases,
/// pointer aliases, forward declarations. The ones here did not, and each failure cascaded:
/// <c>typedef struct {</c> at line 32 of NVDA's nvdaInProcUtils.idl accounted for around a
/// hundred follow-on parse errors, because the parser never recovered.
/// </summary>
public class ClassicTypedefTests
{
    /// <summary>Every shape, with the alias each is expected to introduce.</summary>
    public static TheoryData<string, string, string> Forms => new()
    {
        // label, declaration, alias name
        { "anonymous struct",     "typedef struct { long x; long y; } Point;", "Point" },
        { "anonymous enum",       "typedef enum { Red, Green } Colour;", "Colour" },
        { "anonymous union",      "typedef union { long a; short b; } U;", "U" },
        { "tagged struct",        "typedef struct tagPoint { long x; } Point;", "Point" },
        { "tagged enum",          "typedef enum tagColour { Red } Colour;", "Colour" },
        { "simple alias",         "typedef long MyLong;", "MyLong" },
        { "pointer alias",        "typedef long* PLong;", "PLong" },
        { "double pointer",       "typedef long** PPLong;", "PPLong" },
        { "attributed handle",    "typedef [context_handle] void* Handle;", "Handle" },
        { "attributed string",    "typedef [string] char* MyString;", "MyString" },
        { "multi-word alias",     "typedef unsigned long DWORD;", "DWORD" },
        { "array alias",          "typedef long Row[10];", "Row" },
        { "attributed anon enum", "typedef [v1_enum] enum { A, B } E;", "E" },
        { "forward struct",       "typedef struct tagOpaque Opaque;", "Opaque" },
    };

    private static Compilation Compile(string decl, CompilationMode mode) =>
        CompilationHelper.Compile(decl, referencePaths: null, mode: mode);

    private static int ParseErrors(Compilation c) =>
        c.Diagnostics.Count(d => d.Code is "MIDL1100" or "MIDL1102");

    private static string Errors(Compilation c) =>
        string.Join("; ", c.Diagnostics.Where(d => d.Code is "MIDL1100" or "MIDL1102").Select(d => d.Message));

    [Theory]
    [MemberData(nameof(Forms))]
    public void ParsesInCompatMode(string label, string decl, string _)
    {
        var c = Compile(decl, CompilationMode.Compat);

        Assert.True(ParseErrors(c) == 0, $"{label}: {Errors(c)}");
    }

    /// <summary>
    /// Strict mode rejects the construct, but must still parse it — otherwise one classic
    /// declaration buries the rest of the file in recovery noise.
    /// </summary>
    [Theory]
    [MemberData(nameof(Forms))]
    public void ParsesInStrictModeToo(string label, string decl, string _)
    {
        var c = Compile(decl, CompilationMode.Strict);

        Assert.True(ParseErrors(c) == 0, $"{label}: {Errors(c)}");
    }

    /// <summary>
    /// The alias has to be usable, or the parse "succeeded" without producing anything.
    /// Asserted by referring to it, since an alias is registered rather than added as a type —
    /// what matters is that a later reference resolves, not where it is stored.
    /// </summary>
    [Theory]
    [MemberData(nameof(Forms))]
    public void TheAliasIsUsable(string label, string decl, string alias)
    {
        var c = Compile(decl + $"\ninterface IUse {{ void M({alias} p); }}", CompilationMode.Compat);

        Assert.True(ParseErrors(c) == 0, $"{label}: {Errors(c)}");
        Assert.DoesNotContain(c.Diagnostics,
            d => d.Code == "MIDL1001" && d.Message.Contains(alias, StringComparison.Ordinal));
    }

    // ---------------------------------------------------------------------
    // Guard: typedef is not MIDL3 and must stay rejected in strict mode.
    // ---------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(Forms))]
    public void StrictModeStillRejectsTypedef(string label, string decl, string _)
    {
        var c = Compile(decl, CompilationMode.Strict);

        Assert.True(c.Diagnostics.Any(d => d.Code == "MIDL2001"),
            $"{label}: expected the classic-construct error in strict mode, got [" +
            string.Join(", ", c.Diagnostics.Select(d => d.Code).Distinct()) + "]");
    }

    [Theory]
    [MemberData(nameof(Forms))]
    public void StrictModeReportsAnError(string label, string decl, string _)
    {
        var c = Compile(decl, CompilationMode.Strict);

        Assert.True(c.Diagnostics.Any(d => d.Severity == Diagnostics.DiagnosticSeverity.Error),
            $"{label}: strict mode should report an error");
    }

    /// <summary>
    /// One classic declaration should cost one message, not a cascade — the whole reason for
    /// parsing constructs strict mode rejects.
    /// </summary>
    [Theory]
    [MemberData(nameof(Forms))]
    public void StrictModeReportsTypedefOnce(string label, string decl, string _)
    {
        var c = Compile(decl, CompilationMode.Strict);

        var typedefErrors = c.Diagnostics
            .Where(d => d.Code == "MIDL2001" && d.Message.Contains("typedef", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(typedefErrors.Count <= 1,
            $"{label}: expected at most one typedef complaint, got {typedefErrors.Count}");
    }

    // ---------------------------------------------------------------------
    // Shapes that carry data worth checking beyond "it parsed".
    // ---------------------------------------------------------------------

    /// <summary>
    /// An anonymous struct introduces a struct under the alias name — the alias is the only
    /// name it has.
    ///
    /// Its fields are not bound: the parser consumes a classic struct body as raw tokens, so
    /// the tagged form does not bind fields either. That is a pre-existing limitation this
    /// change does not alter, and asserting fields here would invent a requirement the
    /// established form does not meet.
    /// </summary>
    [Fact]
    public void AnonymousStructIntroducesAStructNamedAfterTheAlias()
    {
        var c = Compile("typedef struct { long x; long y; } Point;", CompilationMode.Compat);

        Assert.Equal(0, ParseErrors(c));
        var point = Assert.Single(c.GetAllTypes(), t => t.Name == "Point");
        Assert.IsType<Symbols.StructSymbol>(point);
    }

    /// <summary>The tagged form binds enum members, so the anonymous form must too.</summary>
    [Fact]
    public void AnonymousEnumKeepsItsMembers()
    {
        var c = Compile("typedef enum { Red, Green, Blue } Colour;", CompilationMode.Compat);

        var colour = Assert.Single(c.GetAllTypes(), t => t.Name == "Colour");
        var members = Assert.IsType<Symbols.EnumSymbol>(colour).EnumMembers;
        Assert.Equal(new[] { "Red", "Green", "Blue" }, members.Select(m => m.Name).ToArray());
    }

    /// <summary>
    /// A declaration list introduces every name in it: <c>typedef long A, *PA;</c> declares
    /// both. Getting only the first is how a later reference to PA becomes "undefined type".
    /// </summary>
    [Fact]
    public void MultipleDeclaratorsIntroduceEveryName()
    {
        var c = Compile(
            "typedef long MyLong, *PMyLong;\ninterface IUse { void A(MyLong a); void B(PMyLong b); }",
            CompilationMode.Compat);

        Assert.Equal(0, ParseErrors(c));
        Assert.DoesNotContain(c.Diagnostics, d => d.Code == "MIDL1001");
    }

    /// <summary>The shape that motivated this, taken from NVDA's IDL.</summary>
    [Fact]
    public void TheRealWorldDeclarationsParse()
    {
        const string source = """
            typedef struct {
                long left;
                long top;
                long right;
                long bottom;
            } RectLTRB;

            typedef [context_handle] void* nvdaRegistrationHandle_t;

            interface INvda
            {
                error_status_t registerProcess(
                    [in] handle_t bindingHandle,
                    [out] nvdaRegistrationHandle_t* registrationHandle);
            }
            """;

        var c = Compile(source, CompilationMode.Compat);

        Assert.True(ParseErrors(c) == 0, Errors(c));
        // The registration handle is referenced by the interface above; if the typedef did not
        // register it, that reference would be an undefined type.
        Assert.DoesNotContain(c.Diagnostics,
            d => d.Code == "MIDL1001" && d.Message.Contains("nvdaRegistrationHandle_t", StringComparison.Ordinal));
        Assert.Contains(c.GetAllTypes(), t => t.Name == "RectLTRB");
    }

    /// <summary>
    /// Classic MIDL declares types inside the interface body. This is the form the real file
    /// uses, and it is the one my first pass missed: every shape in the theory above is a
    /// top-level declaration, so they all passed while the actual file still failed.
    /// </summary>
    [Fact]
    public void TypedefInsideAnInterfaceBodyIsUsableByItsMethods()
    {
        const string source = """
            interface INvda
            {
                typedef [context_handle] void* Handle;

                error_status_t Register([out] Handle* handle);
            }
            """;

        var c = Compile(source, CompilationMode.Compat);

        Assert.True(ParseErrors(c) == 0, Errors(c));
        Assert.DoesNotContain(c.Diagnostics,
            d => d.Code == "MIDL1001" && d.Message.Contains("Handle", StringComparison.Ordinal));
    }

    /// <summary>
    /// A typedef inside an interface declares a type in the enclosing namespace, so the rest
    /// of the file can use it too — it is a declaration that happens to sit there, not a member.
    /// </summary>
    [Fact]
    public void TypedefInsideAnInterfaceIsVisibleOutsideIt()
    {
        const string source = """
            interface IFirst
            {
                typedef long Handle;
            }

            interface ISecond
            {
                void Use([in] Handle h);
            }
            """;

        var c = Compile(source, CompilationMode.Compat);

        Assert.True(ParseErrors(c) == 0, Errors(c));
        Assert.DoesNotContain(c.Diagnostics, d => d.Code == "MIDL1001");
    }

    /// <summary>
    /// cpp_quote also appears among interface members in classic IDL, and left unparsed it
    /// cost the remainder of the interface.
    /// </summary>
    [Fact]
    public void CppQuoteInsideAnInterfaceBodyDoesNotBreakTheRest()
    {
        const string source = """
            interface INvda
            {
                cpp_quote("/* passthrough */")

                void StillParsed();
            }
            """;

        var c = Compile(source, CompilationMode.Compat);

        Assert.True(ParseErrors(c) == 0, Errors(c));
        var iface = Assert.Single(c.GetAllTypes(), t => t.Name == "INvda");
        Assert.Contains(Assert.IsType<Symbols.InterfaceSymbol>(iface).Members, m => m.Name == "StillParsed");
    }

    /// <summary>Strict mode keeps rejecting both, wherever they appear.</summary>
    [Theory]
    [InlineData("interface I { typedef long H; void M(); }")]
    [InlineData("interface I { cpp_quote(\"x\") void M(); }")]
    public void StrictModeRejectsClassicConstructsInsideInterfaces(string source)
    {
        var c = Compile(source, CompilationMode.Strict);

        Assert.Contains(c.Diagnostics, d => d.Code == "MIDL2001");
    }

    /// <summary>
    /// A classic declaration must not stop the rest of the file being understood — that is
    /// what the cascade did.
    /// </summary>
    [Fact]
    public void AClassicTypedefDoesNotHideLaterDeclarations()
    {
        const string source = """
            typedef struct { long x; } Point;

            interface IAfter
            {
                void StillParsed();
            }
            """;

        var c = Compile(source, CompilationMode.Compat);

        Assert.Equal(0, ParseErrors(c));
        Assert.Contains(c.GetAllTypes(), t => t.Name == "IAfter");
    }
}
