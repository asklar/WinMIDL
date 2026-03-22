using Midl.Syntax;

namespace Midl.Parser;

/// <summary>
/// A green node that holds N children. Used to build composite syntax tree nodes.
/// </summary>
public sealed class GreenNodeList : GreenNode
{
    private readonly GreenNode?[] _children;

    public GreenNodeList(SyntaxKind kind, GreenNode?[] children) : base(kind)
    {
        _children = children;
        int width = 0;
        for (int i = 0; i < children.Length; i++)
        {
            var c = children[i];
            if (c != null) width += c.FullWidth;
        }
        FullWidth = width;
    }

    public override int SlotCount => _children.Length;
    public override GreenNode? GetSlot(int index) => _children[index];
}
