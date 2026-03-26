namespace Midl.VsExtension;

using System.Diagnostics;

/// <summary>
/// Provides decompiled IDL content for WinMD files by invoking the MIDL CLI.
/// When go-to-definition targets a WinMD type, the LSP server returns a midl-winmd:// URI.
/// This helper decompiles the WinMD to IDL and writes it to a temporary file that can be
/// opened in Visual Studio as a read-only document.
/// </summary>
internal static class WinMdDecompiler
{
    /// <summary>
    /// Decompiles a WinMD URI into IDL source text.
    /// URI format: midl-winmd:///path/to/file.winmd/Namespace.TypeName
    /// </summary>
    public static string? Decompile(string uri)
    {
        string? serverPath = ServerDiscovery.FindServerPath();
        if (serverPath is null)
        {
            return "// MIDL server binary not found — cannot decompile WinMD";
        }

        // Parse the URI to extract the winmd path and optional namespace filter
        string fullPath = uri;
        if (fullPath.StartsWith("midl-winmd://", StringComparison.OrdinalIgnoreCase))
        {
            fullPath = fullPath["midl-winmd://".Length..];
        }

        // Remove leading slash on Windows paths (e.g., /C:/path)
        if (fullPath.Length > 2 && fullPath[0] == '/' && fullPath[2] == ':')
        {
            fullPath = fullPath[1..];
        }

        // Split at .winmd boundary
        string winmdPath;
        string? nsFilter = null;
        int winmdIdx = fullPath.IndexOf(".winmd", StringComparison.OrdinalIgnoreCase);
        if (winmdIdx >= 0)
        {
            winmdPath = fullPath[..(winmdIdx + 6)];
            string remainder = fullPath[(winmdIdx + 6)..];
            if (remainder.StartsWith('/') && remainder.Length > 1)
            {
                nsFilter = remainder[1..];
            }
        }
        else
        {
            winmdPath = fullPath;
        }

        string nsArgs = nsFilter is not null ? $" --namespace \"{nsFilter}\"" : "";

        try
        {
            ProcessStartInfo psi = new()
            {
                FileName = serverPath,
                Arguments = $"--decompile \"{winmdPath}\"{nsArgs}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using Process? process = Process.Start(psi);
            if (process is null)
            {
                return $"// Failed to start decompiler process: {serverPath}";
            }

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(30_000);
            return output;
        }
        catch (Exception ex)
        {
            return $"// Failed to decompile {winmdPath}\n// {ex.Message}";
        }
    }

    /// <summary>
    /// Decompiles a WinMD URI and writes the result to a temporary .idl file,
    /// returning the path. Returns null if decompilation fails.
    /// </summary>
    public static string? DecompileToFile(string uri)
    {
        string? content = Decompile(uri);
        if (content is null)
        {
            return null;
        }

        // Extract a meaningful filename from the namespace filter
        string fileName = "decompiled";
        int winmdIdx = uri.IndexOf(".winmd", StringComparison.OrdinalIgnoreCase);
        if (winmdIdx >= 0)
        {
            string remainder = uri[(winmdIdx + 6)..];
            if (remainder.StartsWith('/') && remainder.Length > 1)
            {
                fileName = remainder[1..].Replace('.', '_');
            }
        }

        string tempDir = Path.Combine(Path.GetTempPath(), "midl-decompiled");
        Directory.CreateDirectory(tempDir);
        string filePath = Path.Combine(tempDir, $"{fileName}.idl");
        File.WriteAllText(filePath, content);
        return filePath;
    }
}
