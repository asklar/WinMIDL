// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Midl.Syntax;

/// <summary>
/// Represents a single token in the green tree, with optional leading and trailing trivia.
/// </summary>
public sealed class SyntaxToken : GreenNode
{
    public string Text { get; }
    public object? Value { get; }
    public GreenNode[] LeadingTrivia { get; }
    public GreenNode[] TrailingTrivia { get; }

    public SyntaxToken(SyntaxKind kind, string text, object? value = null,
        GreenNode[]? leadingTrivia = null, GreenNode[]? trailingTrivia = null)
        : base(kind)
    {
        Text = text;
        Value = value;
        LeadingTrivia = leadingTrivia ?? [];
        TrailingTrivia = trailingTrivia ?? [];
        FullWidth = Text.Length
            + LeadingTrivia.Sum(t => t.FullWidth)
            + TrailingTrivia.Sum(t => t.FullWidth);
    }

    public override int SlotCount => 0;
    public override GreenNode? GetSlot(int index) => null;
}
