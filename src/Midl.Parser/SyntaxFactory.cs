// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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

/// <summary>
/// Specialized green node for the common case of exactly one child.
/// Avoids the GreenNode?[] allocation that GreenNodeList requires.
/// </summary>
public sealed class SingleChildGreenNode : GreenNode
{
    private readonly GreenNode? _child;

    public SingleChildGreenNode(SyntaxKind kind, GreenNode? child) : base(kind)
    {
        _child = child;
        FullWidth = child?.FullWidth ?? 0;
    }

    public override int SlotCount => 1;
    public override GreenNode? GetSlot(int index) => index == 0 ? _child : null;
}
