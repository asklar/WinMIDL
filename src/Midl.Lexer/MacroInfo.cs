// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Midl.Lexer;

/// <summary>
/// A macro as the preprocessor saw it, kept so that editors can explain a name that no
/// longer exists by the time the binder runs.
///
/// A macro is substituted out during preprocessing, so nothing downstream — no symbol, no
/// syntax node — records that the name was ever there. The editor, however, still shows
/// the original text, so an unexplained identifier is exactly what the user is looking at.
/// </summary>
/// <param name="Name">The macro name as written in the <c>#define</c>.</param>
/// <param name="Parameters">Parameter names for a function-like macro; <c>null</c> for an object-like one.</param>
/// <param name="Body">The replacement text. Empty for <c>#define FOO</c> with no value.</param>
/// <param name="IsVariadic">True when the macro was declared with a trailing <c>...</c>.</param>
/// <param name="FilePath">
/// File containing the <c>#define</c>. Null when the source was supplied as text rather than
/// read from disk — which is the normal case in an editor — so this cannot be used to tell a
/// built-in from a source macro. Use <paramref name="IsBuiltIn"/> for that.
/// </param>
/// <param name="Position">Offset of the <c>#define</c> line within <paramref name="FilePath"/>.</param>
/// <param name="IsBuiltIn">
/// True for a macro predefined by the compiler or supplied with <c>/D</c>, rather than one
/// written in source. Recorded explicitly because an in-memory document has no file path.
/// </param>
public sealed record MacroInfo(
    string Name,
    IReadOnlyList<string>? Parameters,
    string Body,
    bool IsVariadic,
    string? FilePath,
    int Position,
    bool IsBuiltIn = false)
{
    /// <summary>True for a function-like macro — one declared with a parameter list.</summary>
    public bool IsFunctionLike => Parameters is not null;
}
