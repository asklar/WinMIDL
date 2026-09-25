// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Midl.Analysis;

/// <summary>
/// Resolves import "file.idl" paths relative to the current file or include paths (/I).
/// Tracks already-imported files to prevent circular/redundant imports.
/// </summary>
public class ImportResolver
{
    private readonly IReadOnlyList<string>? _includePaths;
    private readonly string? _currentFileDirectory;
    private readonly HashSet<string> _importedFiles;

    public ImportResolver(IReadOnlyList<string>? includePaths, string? currentFilePath)
        : this(includePaths, currentFilePath, new HashSet<string>(StringComparer.OrdinalIgnoreCase))
    {
    }

    private ImportResolver(IReadOnlyList<string>? includePaths, string? currentFilePath,
                           HashSet<string> sharedImportedFiles)
    {
        _includePaths = includePaths;
        _currentFileDirectory = currentFilePath != null
            ? Path.GetDirectoryName(Path.GetFullPath(currentFilePath))
            : null;
        _importedFiles = sharedImportedFiles;
    }

    /// <summary>
    /// Creates a child resolver that resolves paths relative to the given file
    /// but shares the same already-imported tracking set to prevent circular imports.
    /// </summary>
    internal ImportResolver CreateChildResolver(string childFilePath)
    {
        return new ImportResolver(_includePaths, childFilePath, _importedFiles);
    }

    /// <summary>
    /// Resolves an imported file path. Tries relative to current file first, then include paths.
    /// Returns the full resolved path, or null if not found.
    /// </summary>
    public string? ResolveImportPath(string importedFile)
    {
        // 1. Try relative to current file
        if (_currentFileDirectory != null)
        {
            var candidate = Path.GetFullPath(Path.Combine(_currentFileDirectory, importedFile));
            if (File.Exists(candidate))
                return candidate;
        }

        // 2. Try each include path
        if (_includePaths != null)
        {
            foreach (var includePath in _includePaths)
            {
                var candidate = Path.GetFullPath(Path.Combine(includePath, importedFile));
                if (File.Exists(candidate))
                    return candidate;
            }
        }

        return null;
    }

    /// <summary>Returns true if the file has already been imported (prevents redundant/circular imports).</summary>
    public bool IsAlreadyImported(string resolvedPath)
    {
        return _importedFiles.Contains(Path.GetFullPath(resolvedPath));
    }

    /// <summary>Marks a file as imported.</summary>
    public void MarkImported(string resolvedPath)
    {
        _importedFiles.Add(Path.GetFullPath(resolvedPath));
    }
}
