// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Lexer;

namespace Midl.Analysis.Tests;

/// <summary>
/// A comment may follow a preprocessor directive with no whitespace between them:
/// <c>#endif//_WINERROR_</c>. The directive name was taken as everything up to the first
/// space, so the name became <c>endif//_WINERROR_</c> — an unknown directive, and worse, an
/// <c>#endif</c> that never closed its conditional.
///
/// This is not a corner case. winerror.h and ws2def.h both do it, and between them they are
/// included by most of the Windows SDK.
/// </summary>
public class DirectiveCommentWithoutSpaceTests
{
    private static (string Expanded, Preprocessor Pp) Run(string source)
    {
        var pp = new Preprocessor();
        return (pp.Process(source), pp);
    }

    private static IEnumerable<string> ErrorMessages(Preprocessor pp) => pp.Errors.Select(e => e.Message);

    [Theory]
    [InlineData("#endif//comment")]
    [InlineData("#endif//_WINERROR_")]
    [InlineData("#endif/*comment*/")]
    [InlineData("#endif //comment")]
    public void AnEndifIsRecognisedWhateverFollowsIt(string endif)
    {
        var (expanded, pp) = Run($"#define GATE 1\n#ifdef GATE\nInt32 Inside;\n{endif}\nInt32 After;\n");

        Assert.DoesNotContain(ErrorMessages(pp), m => m.Contains("Unknown preprocessor directive"));
        Assert.Contains("Int32 Inside;", expanded);
        Assert.Contains("Int32 After;", expanded);
    }

    [Fact]
    public void AnUnclosedEndifDoesNotSwallowTheRestOfTheFile()
    {
        // The real damage: an unrecognised #endif leaves the conditional open, so everything
        // after it is treated as still inside the #ifdef.
        var (expanded, _) = Run("#ifndef NOT_DEFINED\nInt32 Inside;\n#endif//guard\nInt32 After;\n");

        Assert.Contains("Int32 After;", expanded);
    }

    [Theory]
    [InlineData("#ifdef GATE//comment")]
    [InlineData("#ifdef GATE/*comment*/")]
    public void AnIfdefIgnoresATrailingComment(string ifdef)
    {
        var (expanded, pp) = Run($"#define GATE 1\n{ifdef}\nInt32 Inside;\n#endif\n");

        Assert.DoesNotContain(ErrorMessages(pp), m => m.Contains("Unknown preprocessor directive"));
        Assert.Contains("Int32 Inside;", expanded);
    }

    [Fact]
    public void AnIfndefIgnoresATrailingComment()
    {
        var (expanded, _) = Run("#ifndef ABSENT//comment\nInt32 Inside;\n#endif\n");

        Assert.Contains("Int32 Inside;", expanded);
    }

    [Fact]
    public void AnElseIgnoresATrailingComment()
    {
        var (expanded, pp) = Run("#ifdef ABSENT\nInt32 Taken;\n#else//comment\nInt32 NotTaken;\n#endif\n");

        Assert.DoesNotContain(ErrorMessages(pp), m => m.Contains("Unknown preprocessor directive"));
        Assert.Contains("Int32 NotTaken;", expanded);
        Assert.DoesNotContain("Int32 Taken;", expanded);
    }

    [Fact]
    public void AnUndefIgnoresATrailingComment()
    {
        var (_, pp) = Run("#define GATE 1\n#undef GATE//comment\nInt32 X;\n");

        Assert.False(pp.Macros.ContainsKey("GATE"), "GATE should have been undefined");
    }

    [Fact]
    public void ADefineIgnoresATrailingComment()
    {
        var (_, pp) = Run("#define WIDTH 1920//comment\nInt32 X;\n");

        Assert.True(pp.Macros.ContainsKey("WIDTH"));
        Assert.Equal("1920", pp.Macros["WIDTH"].Body);
    }

    [Fact]
    public void ADirectiveNameIsStillReadNormallyWithoutAComment()
    {
        // Guard: the name must not be truncated by an unrelated slash later in the line.
        var (expanded, pp) = Run("#define PATH_SEP 1\n#ifdef PATH_SEP\nInt32 Inside;\n#endif\n");

        Assert.DoesNotContain(ErrorMessages(pp), m => m.Contains("Unknown preprocessor directive"));
        Assert.Contains("Int32 Inside;", expanded);
    }

    [Theory]
    [InlineData("#if(1)")]
    [InlineData("#if (1)")]
    [InlineData("#if(GATE >= 1)")]
    public void AnIfMayAbutItsParenthesis(string ifLine)
    {
        // `#if(NTDDI_VERSION >= NTDDI_WIN8)` appears throughout the Windows SDK.
        var (expanded, pp) = Run($"#define GATE 1\n{ifLine}\nInt32 Inside;\n#endif\n");

        Assert.DoesNotContain(ErrorMessages(pp), m => m.Contains("Unknown preprocessor directive"));
        Assert.Contains("Int32 Inside;", expanded);
    }

    [Fact]
    public void AnIfdefMayAbutAParenthesisFreeName()
    {
        var (expanded, pp) = Run("#define GATE 1\n#if defined(GATE)\nInt32 Inside;\n#endif\n");

        Assert.DoesNotContain(ErrorMessages(pp), m => m.Contains("Unknown preprocessor directive"));
        Assert.Contains("Int32 Inside;", expanded);
    }
}
