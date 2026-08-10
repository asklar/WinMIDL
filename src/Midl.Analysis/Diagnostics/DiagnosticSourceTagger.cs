// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Midl.Analysis.Diagnostics;

/// <summary>
/// Attributes diagnostics to the file their position actually refers to.
///
/// A diagnostic's Position is an offset into <em>some</em> file. For anything reached
/// through an <c>import</c>, that is not the document being compiled — and painting such
/// an offset onto the main document puts a squiggle on unrelated text, or past the end of
/// the file entirely.
///
/// Binding has an ambient "current file" and can tag as it goes, but every pass that runs
/// afterwards — type resolution, attribute validation, version propagation — walks symbols
/// and type references that outlive that context. Those passes must state the file
/// explicitly, which they can, because <see cref="Symbols.ISymbol.SourceFile"/> and the
/// tracked type references both carry it.
/// </summary>
internal static class DiagnosticSourceTagger
{
    /// <summary>
    /// Attributes diagnostics added to <paramref name="diagnostics"/> from
    /// <paramref name="startIndex"/> onward to <paramref name="sourceFile"/>.
    ///
    /// Setting <c>OriginalPosition</c> marks the position as already resolved so the
    /// preprocessor SourceMap pass leaves it alone: that map only models <c>#include</c>,
    /// and would otherwise reinterpret the offset as one into the main document.
    ///
    /// A null <paramref name="sourceFile"/> means the main document, which the source-map
    /// pass already handles. Diagnostics that already carry a SourceFile — for instance
    /// from a nested import — keep it.
    /// </summary>
    public static void Tag(List<Diagnostic> diagnostics, int startIndex, string? sourceFile)
    {
        if (sourceFile == null) return;

        for (int i = startIndex; i < diagnostics.Count; i++)
        {
            var d = diagnostics[i];
            if (d.SourceFile != null) continue;
            diagnostics[i] = d with
            {
                SourceFile = sourceFile,
                OriginalPosition = d.OriginalPosition ?? d.Position,
            };
        }
    }
}
