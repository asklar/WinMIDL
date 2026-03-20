namespace Midl.Lsp;

/// <summary>
/// Converts zero-based character offsets to LSP line/character positions.
/// </summary>
public static class PositionHelper
{
    /// <summary>
    /// Builds a line-start table from source text. lineStarts[i] is the char offset of line i.
    /// </summary>
    public static int[] BuildLineStarts(string text)
    {
        var starts = new List<int> { 0 };
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
                starts.Add(i + 1);
            else if (text[i] == '\r')
            {
                if (i + 1 < text.Length && text[i + 1] == '\n')
                    i++; // skip \r\n as a single newline
                starts.Add(i + 1);
            }
        }
        return starts.ToArray();
    }

    /// <summary>
    /// Converts a zero-based character offset to (line, character) using the line-start table.
    /// Both line and character are zero-based per LSP spec.
    /// </summary>
    public static (int Line, int Character) OffsetToLineChar(int[] lineStarts, int offset)
    {
        int line = Array.BinarySearch(lineStarts, offset);
        if (line < 0)
            line = ~line - 1;
        int character = offset - lineStarts[line];
        return (line, character);
    }

    /// <summary>
    /// Converts (line, character) to a zero-based character offset.
    /// </summary>
    public static int LineCharToOffset(int[] lineStarts, int line, int character)
    {
        if (line < 0 || line >= lineStarts.Length) return 0;
        return lineStarts[line] + character;
    }
}
