// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Midl.Analysis;

/// <summary>
/// Maps offsets in preprocessor-expanded text back to original file paths and positions.
/// Used to attribute diagnostics to the correct source file after #include expansion.
/// </summary>
public class SourceMap
{
    /// <summary>
    /// A run of expanded text and where it came from.
    ///
    /// <paramref name="Constant"/> marks text produced by macro expansion, where the
    /// expanded length bears no relation to the original: every offset in the run resolves
    /// to <paramref name="OriginalStart"/>, the macro invocation site. Mapping such a run
    /// linearly would walk the position off the end of the macro name and into whatever
    /// follows it.
    /// </summary>
    public record Segment(int ExpandedStart, int ExpandedEnd, string FilePath, int OriginalStart, bool Constant = false);

    private readonly List<Segment> _segments = new();

    public void AddSegment(int expandedStart, int expandedEnd, string filePath, int originalStart, bool constant = false)
    {
        _segments.Add(new(expandedStart, expandedEnd, filePath, originalStart, constant));
    }

    /// <summary>
    /// Sort segments by ExpandedStart. Call after all segments are added, before calling Resolve.
    /// </summary>
    public void SortSegments()
    {
        _segments.Sort((a, b) => a.ExpandedStart.CompareTo(b.ExpandedStart));
    }

    /// <summary>
    /// Resolve an offset in the expanded text back to the original file and offset.
    /// </summary>
    public (string filePath, int originalOffset)? Resolve(int expandedOffset)
    {
        // Binary search for the segment containing the expanded offset
        int lo = 0, hi = _segments.Count - 1;
        while (lo <= hi)
        {
            int mid = lo + (hi - lo) / 2;
            var seg = _segments[mid];
            if (expandedOffset < seg.ExpandedStart)
                hi = mid - 1;
            else if (expandedOffset >= seg.ExpandedEnd)
                lo = mid + 1;
            else
            {
                if (seg.Constant) return (seg.FilePath, seg.OriginalStart);
                int delta = expandedOffset - seg.ExpandedStart;
                return (seg.FilePath, seg.OriginalStart + delta);
            }
        }
        return null;
    }

    public bool HasMappings => _segments.Count > 0;

    /// <summary>Segments for testing/inspection.</summary>
    internal IReadOnlyList<Segment> Segments => _segments;
}
