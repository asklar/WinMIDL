// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Lsp.Handlers;

namespace Midl.Integration.Tests;

public class SemanticTokensTests
{
    [Fact]
    public void ClassifyKeywords()
    {
        var source = "namespace TestApp\n{\n}\n";
        var (data, tokenTypes, _) = SemanticTokensHandler.HandleCore(source);

        // data is delta-encoded: [deltaLine, deltaChar, length, tokenType, tokenModifiers, ...]
        // First token should be "namespace" keyword
        Assert.True(data.Length >= 5, "Should have at least one token (5 ints)");

        // tokenType index for keyword
        int keywordIdx = Array.IndexOf(tokenTypes, "keyword");
        Assert.True(keywordIdx >= 0, "keyword should be in token types legend");

        // First token: "namespace" at line 0, char 0, length 9, type = keyword
        Assert.Equal(0, data[0]); // deltaLine
        Assert.Equal(0, data[1]); // deltaChar
        Assert.Equal(9, data[2]); // length of "namespace"
        Assert.Equal(keywordIdx, data[3]); // tokenType = keyword
    }

    [Fact]
    public void ClassifyTypes()
    {
        var source = """
            namespace TestApp
            {
                runtimeclass Widget
                {
                    String GetName();
                }
            }
            """;
        var (data, tokenTypes, _) = SemanticTokensHandler.HandleCore(source);
        Assert.True(data.Length > 0);

        // "String" should be classified as keyword (it's a built-in type keyword)
        int keywordIdx = Array.IndexOf(tokenTypes, "keyword");
        Assert.True(keywordIdx >= 0);

        // Walk the delta-encoded data to find the "String" token
        bool foundString = false;
        int line = 0, col = 0;
        for (int i = 0; i + 4 < data.Length; i += 5)
        {
            int deltaLine = data[i];
            int deltaChar = data[i + 1];
            int length = data[i + 2];
            int tokenType = data[i + 3];

            if (deltaLine > 0) { line += deltaLine; col = deltaChar; }
            else { col += deltaChar; }

            // "String" keyword is length 6 and classified as keyword
            if (length == 6 && tokenType == keywordIdx)
            {
                // Check the source at this position to verify it's "String"
                var lineStarts = Midl.Lsp.PositionHelper.BuildLineStarts(source);
                int offset = Midl.Lsp.PositionHelper.LineCharToOffset(lineStarts, line, col);
                if (offset + 6 <= source.Length && source.Substring(offset, 6) == "String")
                {
                    foundString = true;
                    break;
                }
            }
        }
        Assert.True(foundString, "String should be classified as keyword token type");
    }

    [Fact]
    public void ClassifyIdentifiers()
    {
        var source = """
            namespace TestApp
            {
                runtimeclass Widget
                {
                    void DoWork();
                }
            }
            """;
        var (data, tokenTypes, _) = SemanticTokensHandler.HandleCore(source);
        Assert.True(data.Length > 0);

        int methodIdx = Array.IndexOf(tokenTypes, "method");
        Assert.True(methodIdx >= 0, "method should be in token types legend");

        // Walk the delta-encoded data to find "DoWork" token
        bool foundMethod = false;
        int line = 0, col = 0;
        for (int i = 0; i + 4 < data.Length; i += 5)
        {
            int deltaLine = data[i];
            int deltaChar = data[i + 1];
            int length = data[i + 2];
            int tokenType = data[i + 3];

            if (deltaLine > 0) { line += deltaLine; col = deltaChar; }
            else { col += deltaChar; }

            if (length == 6 && tokenType == methodIdx)
            {
                var lineStarts = Midl.Lsp.PositionHelper.BuildLineStarts(source);
                int offset = Midl.Lsp.PositionHelper.LineCharToOffset(lineStarts, line, col);
                if (offset + 6 <= source.Length && source.Substring(offset, 6) == "DoWork")
                {
                    foundMethod = true;
                    break;
                }
            }
        }
        Assert.True(foundMethod, "DoWork should be classified as method token type");
    }

    [Fact]
    public void ClassifyRefDirective()
    {
        var source = "// @ref: SomeFile.winmd\nnamespace N { }\n";
        var (data, tokenTypes, _) = SemanticTokensHandler.HandleCore(source);
        Assert.True(data.Length >= 15, "Should have at least 3 tokens for @ref line (comment + decorator + string)");

        int commentIdx = Array.IndexOf(tokenTypes, "comment");
        int decoratorIdx = Array.IndexOf(tokenTypes, "decorator");
        int stringIdx = Array.IndexOf(tokenTypes, "string");

        // First token: "// " (3 chars) as comment
        Assert.Equal(0, data[0]); // deltaLine
        Assert.Equal(0, data[1]); // deltaChar
        Assert.Equal(3, data[2]); // length of "// "
        Assert.Equal(commentIdx, data[3]);

        // Second token: "@ref:" (5 chars) as decorator
        Assert.Equal(0, data[5]); // same line
        Assert.Equal(3, data[6]); // deltaChar = 3
        Assert.Equal(5, data[7]); // length of "@ref:"
        Assert.Equal(decoratorIdx, data[8]);

        // Third token: "SomeFile.winmd" (14 chars) as string
        Assert.Equal(0, data[10]); // same line
        Assert.Equal(14, data[12]); // length
        Assert.Equal(stringIdx, data[13]);
    }

    /// <summary>
    /// A macro name should not be coloured as if it were an ordinary identifier. The
    /// preprocessor removed it, so the reader needs to see that it is not part of the
    /// declaration it appears to sit inside.
    /// </summary>
    [Fact]
    public void MacroNamesAreClassifiedAsMacros()
    {
        var source = "#define FOO\n\nnamespace N\n{\n    runtimeclass C\n    {\n        Int32 FOO x;\n    }\n}\n";
        var (data, tokenTypes, _) = SemanticTokensHandler.HandleCore(source);

        int macroIdx = Array.IndexOf(tokenTypes, "macro");
        Assert.True(macroIdx >= 0, "macro should be in the token types legend");

        // Walk the delta encoding looking for a macro-typed token on the FOO use line.
        bool found = false;
        int line = 0, col = 0;
        for (int i = 0; i + 4 < data.Length; i += 5)
        {
            if (data[i] > 0) { line += data[i]; col = data[i + 1]; }
            else { col += data[i + 1]; }

            if (data[i + 3] == macroIdx && line == 6 && data[i + 2] == 3)
                found = true;
        }

        Assert.True(found, "FOO on line 6 should be classified as a macro");
    }

    [Fact]
    public void OrdinaryIdentifiersAreNotClassifiedAsMacros()
    {
        var source = "namespace N\n{\n    runtimeclass C\n    {\n        Int32 x;\n    }\n}\n";
        var (data, tokenTypes, _) = SemanticTokensHandler.HandleCore(source);

        int macroIdx = Array.IndexOf(tokenTypes, "macro");
        for (int i = 0; i + 4 < data.Length; i += 5)
            Assert.NotEqual(macroIdx, data[i + 3]);
    }
}
