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
}
