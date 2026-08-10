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
}
