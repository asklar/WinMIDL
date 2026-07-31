// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Midl.Syntax;

/// <summary>
/// Represents trivia (whitespace, comments, skipped tokens) attached to tokens.
/// </summary>
public sealed class SyntaxTrivia : GreenNode
{
    public string Text { get; }

    public SyntaxTrivia(SyntaxKind kind, string text) : base(kind)
    {
        Text = text;
        FullWidth = text.Length;
    }

    public override int SlotCount => 0;
    public override GreenNode? GetSlot(int index) => null;
}
