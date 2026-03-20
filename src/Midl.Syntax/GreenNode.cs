namespace Midl.Syntax;

/// <summary>
/// Base class for immutable green tree nodes (Roslyn red-green tree pattern).
/// Green nodes are structurally shared across edits and have no parent pointers.
/// </summary>
public abstract class GreenNode
{
    public SyntaxKind Kind { get; }
    public int FullWidth { get; protected set; }

    protected GreenNode(SyntaxKind kind)
    {
        Kind = kind;
    }

    public abstract int SlotCount { get; }
    public abstract GreenNode? GetSlot(int index);

    public bool IsMissing => FullWidth == 0 && Kind != SyntaxKind.EndOfFileToken;
}
