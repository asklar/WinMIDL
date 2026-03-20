using Microsoft.Win32;
using System.Runtime.Versioning;

namespace Midl.Cli;

/// <summary>
/// Auto-discovers the Windows SDK installation for include paths and WinMD references.
/// </summary>
[SupportedOSPlatform("windows")]
public static class WindowsSdkLocator
{
    /// <summary>
    /// Find the Windows SDK root from the registry.
    /// </summary>
    public static string? FindSdkRoot()
    {
        try
        {
            using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using var key = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Windows Kits\Installed Roots");
            return key?.GetValue("KitsRoot10") as string;
        }
        catch { return null; }
    }

    /// <summary>
    /// Find the latest SDK version installed.
    /// </summary>
    public static string? FindLatestVersion(string sdkRoot)
    {
        var includeDir = Path.Combine(sdkRoot, "Include");
        if (!Directory.Exists(includeDir)) return null;

        return Directory.GetDirectories(includeDir)
            .Select(Path.GetFileName)
            .Where(d => d != null && Version.TryParse(d, out _))
            .OrderByDescending(d => d)
            .FirstOrDefault();
    }

    /// <summary>
    /// Get default include paths for the SDK version.
    /// </summary>
    public static List<string> GetIncludePaths(string sdkRoot, string version)
    {
        var paths = new List<string>();
        var baseDir = Path.Combine(sdkRoot, "Include", version);

        // Order matters — shared first, then um, winrt, ucrt
        foreach (var subdir in new[] { "shared", "um", "winrt", "ucrt" })
        {
            var path = Path.Combine(baseDir, subdir);
            if (Directory.Exists(path))
                paths.Add(path);
        }

        return paths;
    }

    /// <summary>
    /// Get the Windows.winmd union metadata path.
    /// </summary>
    public static string? GetWindowsWinMdPath(string sdkRoot, string version)
    {
        // Try UnionMetadata first (contains all Windows types in one file)
        var unionPath = Path.Combine(sdkRoot, "UnionMetadata", version, "Windows.winmd");
        if (File.Exists(unionPath)) return unionPath;

        // Fall back to older version
        var unionDir = Path.Combine(sdkRoot, "UnionMetadata");
        if (Directory.Exists(unionDir))
        {
            var latest = Directory.GetDirectories(unionDir)
                .OrderByDescending(d => Path.GetFileName(d))
                .FirstOrDefault();
            if (latest != null)
            {
                var path = Path.Combine(latest, "Windows.winmd");
                if (File.Exists(path)) return path;
            }
        }

        return null;
    }

    /// <summary>
    /// Get individual contract WinMD references.
    /// </summary>
    public static List<string> GetContractReferences(string sdkRoot, string version)
    {
        var refsDir = Path.Combine(sdkRoot, "References", version);
        if (!Directory.Exists(refsDir)) return new();

        return Directory.GetFiles(refsDir, "*.winmd", SearchOption.AllDirectories).ToList();
    }

    /// <summary>
    /// Auto-discover and return all default settings.
    /// </summary>
    public static SdkInfo? Discover()
    {
        var root = FindSdkRoot();
        if (root == null) return null;

        var version = FindLatestVersion(root);
        if (version == null) return null;

        return new SdkInfo
        {
            Root = root,
            Version = version,
            IncludePaths = GetIncludePaths(root, version),
            WindowsWinMdPath = GetWindowsWinMdPath(root, version),
        };
    }
}

public class SdkInfo
{
    public string Root { get; init; } = "";
    public string Version { get; init; } = "";
    public List<string> IncludePaths { get; init; } = new();
    public string? WindowsWinMdPath { get; init; }
}
