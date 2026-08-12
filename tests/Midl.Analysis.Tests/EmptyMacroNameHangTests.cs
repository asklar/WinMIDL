// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Lexer;

namespace Midl.Analysis.Tests;

/// <summary>
/// <c>#define  NAME value</c> — with two spaces — used to hang the compiler forever,
/// consuming several GB and pinning a core.
///
/// ParseDirective skipped exactly one whitespace character after the directive name, so the
/// body kept a leading space. HandleDefine then read the macro name as everything before the
/// first space, which was the empty string, and registered <c>_defines[""]</c>. From then on
/// <c>line.Contains("")</c> was true for every line, and ReplaceWholeWord with an empty word
/// never advanced its cursor — it appended the replacement into a StringBuilder forever.
///
/// Aligned <c>#define</c> columns are ordinary formatting, so this was reachable from stock
/// Windows SDK headers: xmldsodid.h reaches it, and through it msxml.idl, urlmon.idl,
/// ocidl.idl, activscp.idl, dbgprop.idl and activdbg.idl.
/// </summary>
public class EmptyMacroNameHangTests
{
    private static Preprocessor Run(string source, out string expanded)
    {
        var pp = new Preprocessor();
        expanded = pp.Process(source);
        return pp;
    }

    [Theory]
    [InlineData("#define FOO 1")]            // one space
    [InlineData("#define  FOO 1")]           // two — the reported case
    [InlineData("#define   FOO 1")]          // three
    [InlineData("#define\tFOO 1")]           // tab
    [InlineData("#define \t FOO 1")]         // mixed
    public void TheMacroNameIsFoundWhateverTheSpacing(string define)
    {
        var pp = Run(define + "\nInt32 FOO;\n", out _);

        Assert.True(pp.Macros.ContainsKey("FOO"), "FOO should be defined regardless of spacing");
        Assert.Equal("1", pp.Macros["FOO"].Body);
    }

    [Theory]
    [InlineData("#define  FOO 1")]
    [InlineData("#define   FOO 1")]
    [InlineData("#define    FOO")]
    [InlineData("#define  ")]
    [InlineData("#define ")]
    [InlineData("#define")]
    public void NoMacroIsEverRegisteredUnderAnEmptyName(string define)
    {
        var pp = Run(define + "\nInt32 X;\n", out _);

        // An empty name matches everywhere, which is what turned substitution into an
        // unbounded loop. Nothing should ever be registered under one.
        Assert.DoesNotContain("", pp.Macros.Keys);
    }

    [Theory]
    [InlineData("#define  FOO 1")]
    [InlineData("#define   FOO 1")]
    public void SubstitutionUsesTheValueAndLeavesTheRestAlone(string define)
    {
        Run(define + "\nInt32 FOO;\n", out var expanded);

        Assert.Contains("Int32 1;", expanded);
    }

    /// <summary>
    /// The failure was non-termination rather than a wrong answer, so this is bounded by a
    /// generous timeout: it either finishes in milliseconds or never.
    /// </summary>
    [Fact]
    public async Task PreprocessingTerminates()
    {
        const string source = """
            #define  DISPID_XMLDSO                       0x00010000
            #define  DISPID_XMLDSO_DOCUMENT              DISPID_XMLDSO  +  1
            #define  DISPID_XMLDSO_JAVADSOCOMPATIBLE     DISPID_XMLDSO_DOCUMENT  +  1

            namespace N { runtimeclass C { Int32 X; } }
            """;

        string? expanded = null;
        var work = Task.Run(() => { expanded = new Preprocessor().Process(source); });

        var finished = await Task.WhenAny(work, Task.Delay(TimeSpan.FromSeconds(10)));

        Assert.True(finished == work,
            "preprocessing the aligned #define block did not terminate");
        Assert.NotNull(expanded);
    }

    [Fact]
    public void ADefineWithNoValueStillDefinesTheName()
    {
        var pp = Run("#define  EMPTY_ONE\nInt32 EMPTY_ONE x;\n", out var expanded);

        Assert.True(pp.Macros.ContainsKey("EMPTY_ONE"));
        Assert.Equal("", pp.Macros["EMPTY_ONE"].Body);
        Assert.Contains("Int32  x;", expanded);
    }

    [Fact]
    public void AFunctionLikeMacroSurvivesExtraSpacing()
    {
        var pp = Run("#define  SQUARE(x) ((x)*(x))\nInt32 SQUARE(3);\n", out var expanded);

        Assert.True(pp.Macros.ContainsKey("SQUARE"));
        Assert.True(pp.Macros["SQUARE"].IsFunctionLike);
        Assert.Contains("((3)*(3))", expanded);
    }

    [Fact]
    public async Task CompilationTerminatesEndToEnd()
    {
        const string source = """
            #define  WIDTH   1920

            namespace N { runtimeclass C { Int32 X; } }
            """;

        Compilation? c = null;
        var work = Task.Run(() => { c = CompilationHelper.Compile(source, CompilationMode.Compat); });

        var finished = await Task.WhenAny(work, Task.Delay(TimeSpan.FromSeconds(20)));

        Assert.True(finished == work, "compilation did not terminate");
        Assert.NotNull(c);
        Assert.Contains(c!.GetAllTypes(), t => t.Name == "C");
    }
}
