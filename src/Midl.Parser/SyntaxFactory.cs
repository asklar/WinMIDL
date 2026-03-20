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
        FullWidth = children.Where(c => c != null).Sum(c => c!.FullWidth);
    }

    public override int SlotCount => _children.Length;
    public override GreenNode? GetSlot(int index) => _children[index];
}
