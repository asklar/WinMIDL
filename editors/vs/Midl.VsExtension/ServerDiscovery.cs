namespace Midl.VsExtension;

/// <summary>
/// Locates the Midl.Cli language server binary.
/// Mirrors the discovery logic in the VS Code extension (editors/vscode/src/server.ts).
/// </summary>
internal static class ServerDiscovery
{
    private const string ServerExeName = "Midl.Cli.exe";

    /// <summary>
    /// Searches for the Midl.Cli server binary in the following order:
    /// 1. Bundled inside the extension's server/ directory
    /// 2. Dev inner-loop paths relative to the repository root
    /// 3. Published output in the repo's publish/ directory
    /// </summary>
    public static string? FindServerPath()
    {
        string extensionDir = Path.GetDirectoryName(typeof(ServerDiscovery).Assembly.Location)!;

        // 1. Bundled server binary (shipped inside the extension package)
        string[] bundledCandidates =
        [
            Path.Combine(extensionDir, "server", "win-x64", ServerExeName),
            Path.Combine(extensionDir, "server", ServerExeName),
        ];

        foreach (string path in bundledCandidates)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        // 2. Dev inner-loop: walk up to find the repository root (contains Midl.slnx)
        string? repoRoot = FindRepoRoot(extensionDir);
        if (repoRoot is not null)
        {
            string[] devCandidates =
            [
                Path.Combine(repoRoot, "src", "Midl.Cli", "bin", "Debug", "net10.0", ServerExeName),
                Path.Combine(repoRoot, "src", "Midl.Cli", "bin", "Debug", "net10.0", "win-x64", ServerExeName),
                Path.Combine(repoRoot, "src", "Midl.Cli", "bin", "Release", "net10.0", ServerExeName),
                Path.Combine(repoRoot, "src", "Midl.Cli", "bin", "Release", "net10.0", "win-x64", ServerExeName),
                Path.Combine(repoRoot, "publish", ServerExeName),
            ];

            foreach (string path in devCandidates)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }
        }

        return null;
    }

    private static string? FindRepoRoot(string startDir)
    {
        string? dir = startDir;
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir, "Midl.slnx")))
            {
                return dir;
            }

            dir = Path.GetDirectoryName(dir);
        }

        return null;
    }
}
