// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Lexer;

namespace Midl.Lexer.Tests;

/// <summary>
/// The preprocessor must not lose the last character of a file.
///
/// <c>ProcessInternal</c> ends by removing the newline its final <c>AppendLine</c> added,
/// and did so by subtracting <c>Environment.NewLine.Length</c> — two characters on Windows.
/// But an <c>#include</c>'s content is normalised to end with a lone <c>"\n"</c>, so when a
/// file ended with an include, only one of those two characters was a newline and the other
/// was real source. The closing brace of the included file was silently eaten, leaving an
/// unterminated namespace: the types inside it parsed into an empty declaration and vanished.
/// </summary>
public class PreprocessorTrailingNewlineTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "midl-ppnl-" + Guid.NewGuid().ToString("N"));

    public PreprocessorTrailingNewlineTests() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        try { Directory.Delete(_dir, recursive: true); } catch { }
    }

    private string Write(string name, string content)
    {
        var p = Path.Combine(_dir, name);
        File.WriteAllText(p, content);
        return p;
    }

    private const string InnerBody = "namespace Inner\r\n{\r\n    struct FromInclude { Int32 Z; };\r\n}";

    public static TheoryData<bool, bool> TrailingNewlineCombinations => new()
    {
        { false, false },   // the combination that lost the brace
        { false, true },
        { true, false },
        { true, true },
    };

    /// <summary>
    /// Whatever combination of trailing newlines the two files have, the included content
    /// must arrive complete — closing brace and all.
    /// </summary>
    [Theory]
    [MemberData(nameof(TrailingNewlineCombinations))]
    public void IncludedContentIsNotTruncated(bool innerEndsWithNewline, bool outerEndsWithNewline)
    {
        var inner = innerEndsWithNewline ? InnerBody + "\r\n" : InnerBody;
        Write("inner.h", inner);

        var outer = "namespace First\r\n{\r\n    struct A { Int32 X; };\r\n}\r\n#include \"inner.h\""
                  + (outerEndsWithNewline ? "\r\n" : "");
        var outerPath = Write("outer.idl", outer);

        var pp = new Preprocessor(null, new[] { _dir }, outerPath);
        var expanded = pp.Process(outer);

        Assert.Contains("struct FromInclude { Int32 Z; };", expanded);
        Assert.EndsWith("}", expanded.TrimEnd());
    }

    /// <summary>
    /// The same guarantee without any include: a file whose last line has no newline keeps
    /// that line.
    /// </summary>
    [Theory]
    [InlineData("namespace N\r\n{\r\n    struct S { Int32 X; };\r\n}")]
    [InlineData("namespace N\r\n{\r\n    struct S { Int32 X; };\r\n}\r\n")]
    public void TheLastLineSurvivesWithOrWithoutATrailingNewline(string source)
    {
        var path = Write("solo.idl", source);

        var pp = new Preprocessor(null, new[] { _dir }, path);
        var expanded = pp.Process(source);

        Assert.EndsWith("}", expanded.TrimEnd());
        Assert.Contains("struct S { Int32 X; };", expanded);
    }

    /// <summary>
    /// The construction-level invariant behind the fix: expanded output uses one newline
    /// convention throughout.
    ///
    /// Included content used to be terminated with a lone <c>"\n"</c> while everything
    /// around it used <c>Environment.NewLine</c>. Any consumer reasoning about "the
    /// newline at the end" then had two different answers to choose from, and the one it
    /// chose removed a real character. Keeping the buffer uniform is what makes that class
    /// of mistake impossible rather than merely survivable.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ExpandedOutputUsesOneNewlineConvention(bool innerEndsWithNewline)
    {
        Write("inner.h", innerEndsWithNewline ? InnerBody + "\r\n" : InnerBody);

        // Content after the include, so a stray newline it introduces sits in the middle of
        // the buffer rather than at the end where the trailing-newline strip would hide it.
        var outer = "namespace First\r\n{\r\n    struct A { Int32 X; };\r\n}\r\n"
                  + "#include \"inner.h\"\r\n"
                  + "namespace Last\r\n{\r\n    struct B { Int32 Y; };\r\n}\r\n";
        var outerPath = Write("outer.idl", outer);

        var pp = new Preprocessor(null, new[] { _dir }, outerPath);
        var expanded = pp.Process(outer);

        for (int i = 0; i < expanded.Length; i++)
        {
            if (expanded[i] != '\n') continue;
            Assert.True(i > 0 && expanded[i - 1] == '\r',
                $"bare LF at offset {i}; expanded output should use CRLF throughout");
        }
    }

    /// <summary>
    /// An include that cannot be resolved must not corrupt the surrounding text either:
    /// the placeholder it leaves behind follows the same convention.
    /// </summary>
    [Fact]
    public void AnUnresolvedIncludeLeavesTheSurroundingTextIntact()
    {
        var outer = "namespace First\r\n{\r\n    struct A { Int32 X; };\r\n}\r\n"
                  + "#include \"does_not_exist.h\"\r\n"
                  + "namespace Last\r\n{\r\n    struct B { Int32 Y; };\r\n}\r\n";
        var outerPath = Write("outer.idl", outer);

        var pp = new Preprocessor(null, new[] { _dir }, outerPath);
        var expanded = pp.Process(outer);

        Assert.Contains("struct A { Int32 X; };", expanded);
        Assert.Contains("struct B { Int32 Y; };", expanded);

        for (int i = 0; i < expanded.Length; i++)
        {
            if (expanded[i] != '\n') continue;
            Assert.True(i > 0 && expanded[i - 1] == '\r',
                $"bare LF at offset {i}; an unresolved include should not change the convention");
        }
    }
}
