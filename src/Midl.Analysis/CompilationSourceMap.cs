// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Midl.Analysis;

/// <summary>
/// The source maps for every file a compilation preprocessed — the main document and each
/// file reached through <c>import</c>, transitively.
///
/// Each file is preprocessed independently, so each has its own coordinate space: a
/// position produced while binding an imported tree is an offset into <em>that file's</em>
/// preprocessed text. Resolving it needs two things — which unit the offset belongs to,
/// and that unit's map. A per-file <see cref="SourceMap"/> supplies the second; this type
/// keeps them all so any offset in the compilation can be resolved.
///
/// Without it, only the main document could be resolved, and every imported file's
/// positions were reported raw: correct only when that file contained no macro expansion,
/// no <c>#include</c> and no conditional block.
///
/// A unit's map also resolves <em>through</em> its includes, so text pulled in by an
/// imported file is attributed to the file it actually lives in rather than to the
/// importer.
/// </summary>
public sealed class CompilationSourceMap
{
    private readonly Dictionary<string, SourceMap> _byUnit = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Registers the map produced by preprocessing <paramref name="unitPath"/>.</summary>
    public void Add(string? unitPath, SourceMap? map)
    {
        if (unitPath == null || map == null || !map.HasMappings) return;
        _byUnit[unitPath] = map;
    }

    public bool IsEmpty => _byUnit.Count == 0;

    /// <summary>
    /// Translates an offset in <paramref name="unitPath"/>'s preprocessed text to the file
    /// and offset it originally came from.
    ///
    /// Falls back to the unit itself when there is nothing to translate — a file with no
    /// directives is its own original — so callers get a usable answer either way.
    /// </summary>
    public (string File, int Offset) Resolve(string unitPath, int offsetInUnit)
    {
        if (_byUnit.TryGetValue(unitPath, out var map))
        {
            var resolved = map.Resolve(offsetInUnit);
            if (resolved != null)
                return (resolved.Value.filePath, resolved.Value.originalOffset);
        }
        return (unitPath, offsetInUnit);
    }
}
