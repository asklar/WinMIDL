// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Midl.Integration.Tests;

/// <summary>
/// Locates a Windows SDK metadata file for tests that genuinely need one.
///
/// Tests used to hardcode a single SDK build (<c>10.0.26100.0</c>) and silently
/// <c>return</c> when it was absent — which xUnit records as a pass, so on any machine or
/// CI image with a different build they asserted nothing at all. Discovering whichever
/// build is installed makes those tests real on far more machines.
///
/// <see cref="PathOrNull"/> still returns null when no SDK is present, because a test that
/// needs one genuinely cannot run without it. Prefer writing tests that do not need the SDK
/// — emitting a WinMD from IDL in the test is deterministic and runs everywhere.
/// </summary>
internal static class SdkWinMd
{
    private static readonly Lazy<string?> s_path = new(Discover);

    /// <summary>The newest installed Windows SDK union metadata, or null if none is installed.</summary>
    public static string? PathOrNull => s_path.Value;

    private static string? Discover()
    {
        foreach (var root in new[]
                 {
                     @"C:\Program Files (x86)\Windows Kits\10\UnionMetadata",
                     @"C:\Program Files\Windows Kits\10\UnionMetadata",
                 })
        {
            if (!Directory.Exists(root)) continue;

            // Only version-numbered directories. UnionMetadata also contains "Facade", whose
            // Windows.winmd is a type-forwarding facade rather than the real union metadata —
            // and "Facade" sorts above the version numbers, so it would otherwise win.
            var found = Directory.EnumerateDirectories(root)
                .Select(d => (Dir: d, Parsed: Version.TryParse(Path.GetFileName(d), out var v) ? v : null))
                .Where(x => x.Parsed != null)
                .OrderByDescending(x => x.Parsed)
                .Select(x => Path.Combine(x.Dir, "Windows.winmd"))
                .FirstOrDefault(File.Exists);

            if (found != null) return found;
        }

        return null;
    }
}
