namespace Midl.Lexer;

/// <summary>
/// Efficient character-level scanner over source text.
/// </summary>
public sealed class SlidingTextWindow
{
    private readonly string _text;
    private int _position;

    public SlidingTextWindow(string text)
    {
        _text = text;
        _position = 0;
    }

    public int Position
    {
        get => _position;
        set => _position = value;
    }

    public bool IsAtEnd => _position >= _text.Length;

    public char PeekChar()
    {
        if (_position >= _text.Length)
            return '\0';
        return _text[_position];
    }

    public char PeekChar(int offset)
    {
        int index = _position + offset;
        if (index < 0 || index >= _text.Length)
            return '\0';
        return _text[index];
    }

    public void AdvanceChar()
    {
        if (_position < _text.Length)
            _position++;
    }

    public void AdvanceChar(int count)
    {
        _position = Math.Min(_position + count, _text.Length);
    }

    public string GetText(int start, int length)
    {
        return _text.Substring(start, length);
    }
}
