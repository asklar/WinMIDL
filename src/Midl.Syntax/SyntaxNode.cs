namespace Midl.Syntax;

/// <summary>
/// Red tree node wrapper providing parent pointers and absolute positions.
/// Lazily wraps the underlying immutable green node.
/// </summary>
public class SyntaxNode
{
    private readonly GreenNode _green;
    public SyntaxNode? Parent { get; }
    public int Position { get; }

    public SyntaxKind Kind => _green.Kind;
    public int FullWidth => _green.FullWidth;

    internal SyntaxNode(GreenNode green, SyntaxNode? parent, int position)
    {
        _green = green;
        Parent = parent;
        Position = position;
    }

    internal GreenNode Green => _green;

    public int ChildCount => _green.SlotCount;

    public SyntaxNode? GetChild(int index)
    {
        var greenChild = _green.GetSlot(index);
        if (greenChild is null) return null;

        int offset = Position;
        for (int i = 0; i < index; i++)
        {
            var prev = _green.GetSlot(i);
            if (prev is not null) offset += prev.FullWidth;
        }
        return new SyntaxNode(greenChild, this, offset);
    }
}
