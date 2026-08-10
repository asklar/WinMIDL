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
    /// <paramref name="startIndex"/> onward to the file their position really refers to.
    ///
    /// <paramref name="unitPath"/> names the preprocessed unit the positions were produced
    /// against. When <paramref name="sourceMaps"/> knows that unit, the position is
    /// translated to the original file and offset — which may be a file the unit merely
    /// <c>#include</c>d. Otherwise the unit is its own original.
    ///
    /// Setting <c>OriginalPosition</c> records the resolved offset and marks the position
    /// as final, so the main document's source-map pass leaves it alone: that pass assumes
    /// offsets belong to the main document and would reinterpret these.
    ///
    /// A null <paramref name="unitPath"/> means the main document, which the main
    /// source-map pass already handles. Diagnostics that already carry a SourceFile — for
    /// instance from a nested import — keep it.
    /// </summary>
    public static void Tag(List<Diagnostic> diagnostics, int startIndex, string? unitPath,
        CompilationSourceMap? sourceMaps = null)
    {
        if (unitPath == null) return;

        for (int i = startIndex; i < diagnostics.Count; i++)
        {
            var d = diagnostics[i];
            if (d.SourceFile != null) continue;

            var (file, offset) = sourceMaps?.Resolve(unitPath, d.Position) ?? (unitPath, d.Position);
            diagnostics[i] = d with
            {
                SourceFile = file,
                OriginalPosition = offset,
            };
        }
    }
}
