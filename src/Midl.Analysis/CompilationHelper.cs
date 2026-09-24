// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;
using Midl.Syntax;

namespace Midl.Analysis;

/// <summary>
/// Wires lexer → parser → binder for convenient compilation.
/// </summary>
public static class CompilationHelper
{
    public static Compilation Compile(string source)
    {
        return Compile(source, referencePaths: null, mode: CompilationMode.Strict);
    }

    public static Compilation Compile(string source, CompilationMode mode)
    {
        return Compile(source, referencePaths: null, mode: mode);
    }

    public static Compilation Compile(string source, IReadOnlyList<string>? referencePaths,
                                       CompilationMode mode = CompilationMode.Strict)
    {
        return Compile(source, referencePaths, mode, includePaths: null, currentFilePath: null);
    }

    /// <summary>
    /// Compile with pre-built LazyWinMdProviders (avoids re-reading .winmd files from disk).
    /// </summary>
    public static Compilation Compile(string source, IReadOnlyList<LazyWinMdProvider> lazyProviders,
                                       CompilationMode mode = CompilationMode.Strict,
                                       IReadOnlyList<string>? includePaths = null,
                                       string? currentFilePath = null,
                                       IReadOnlyList<string>? defines = null,
                                       bool emitQualityHints = false,
                                       IReadOnlyList<string>? refDirectories = null,
                                       bool arraysEnabled = false
#if WITH_COMPAT
                                       , string? cppCmd = null
#endif
                                       )
    {
        return CompileCore(source, lazyProviders, mode, includePaths, currentFilePath,
            null, defines, emitQualityHints, refDirectories,
            profile: null, arraysEnabled: arraysEnabled
#if WITH_COMPAT
            , cppCmd
#endif
            );
    }

    /// <summary>
    /// Compiles source while resolving imported files from synchronized content before disk.
    /// </summary>
    /// <param name="source">Source text of the owning document.</param>
    /// <param name="lazyProviders">Pre-built WinMD providers, or <see langword="null"/> when no references are configured.</param>
    /// <param name="importContentProvider">Provider returning synchronized content for a resolved full path, or <see langword="null"/> to use disk content.</param>
    /// <param name="mode">Compilation mode controlling language validation.</param>
    /// <param name="includePaths">Directories searched for imported files, or <see langword="null"/> for none.</param>
    /// <param name="currentFilePath">Path of the owning document, or <see langword="null"/> when it has no file-system path.</param>
    /// <param name="defines">Preprocessor symbols, or <see langword="null"/> for none.</param>
    /// <param name="emitQualityHints">Whether to emit code-quality diagnostics.</param>
    /// <param name="refDirectories">Directories used for lazy reference resolution, or <see langword="null"/> for none.</param>
    /// <param name="arraysEnabled">Whether array syntax is enabled.</param>
#if WITH_COMPAT
    /// <param name="cppCmd">External preprocessor command, or <see langword="null"/> to use the built-in preprocessor.</param>
#endif
    /// <returns>The completed semantic compilation.</returns>
    internal static Compilation CompileWithImportContent(
        string source,
        IReadOnlyList<LazyWinMdProvider>? lazyProviders,
        Func<string, string?> importContentProvider,
        CompilationMode mode = CompilationMode.Strict,
        IReadOnlyList<string>? includePaths = null,
        string? currentFilePath = null,
        IReadOnlyList<string>? defines = null,
        bool emitQualityHints = false,
        IReadOnlyList<string>? refDirectories = null,
        bool arraysEnabled = false
#if WITH_COMPAT
        , string? cppCmd = null
#endif
        )
    {
        return CompileCore(
            source,
            lazyProviders,
            mode,
            includePaths,
            currentFilePath,
            importContentProvider,
            defines,
            emitQualityHints,
            refDirectories,
            profile: null,
            arraysEnabled: arraysEnabled
#if WITH_COMPAT
            , cppCmd
#endif
            );
    }

    public static Compilation Compile(string source, IReadOnlyList<string>? referencePaths,
                                       CompilationMode mode,
                                       IReadOnlyList<string>? includePaths,
                                       string? currentFilePath,
                                       IReadOnlyList<string>? defines = null,
                                        bool emitQualityHints = false,
                                        IReadOnlyList<string>? refDirectories = null,
                                        CompilationProfile? profile = null,
                                        bool arraysEnabled = false
#if WITH_COMPAT
                                        , string? cppCmd = null
#endif
                                        )
    {
        // Build providers from paths (non-cached path)
        List<LazyWinMdProvider>? lazyProviders = null;
        if (referencePaths is { Count: > 0 })
        {
            lazyProviders = new List<LazyWinMdProvider>();
            foreach (var path in referencePaths)
            {
                if (File.Exists(path))
                {
                    var provider = new LazyWinMdProvider(path);
                    lazyProviders.Add(provider);
                    MidlTrace.Write("COMPILE", $"Lazy-indexed {Path.GetFileName(path)}: {provider.TypeCount} types");
                }
            }
        }

        return CompileCore(source, lazyProviders, mode, includePaths, currentFilePath,
            null, defines, emitQualityHints, refDirectories,
            profile, arraysEnabled: arraysEnabled
#if WITH_COMPAT
            , cppCmd
#endif
            );
    }

    private static Compilation CompileCore(string source, IReadOnlyList<LazyWinMdProvider>? lazyProviders,
                                       CompilationMode mode,
                                       IReadOnlyList<string>? includePaths,
                                       string? currentFilePath,
                                       Func<string, string?>? importContentProvider,
                                       IReadOnlyList<string>? defines = null,
                                        bool emitQualityHints = false,
                                        IReadOnlyList<string>? refDirectories = null,
                                        CompilationProfile? profile = null,
                                        bool arraysEnabled = false
#if WITH_COMPAT
                                        , string? cppCmd = null
#endif
                                        )
    {
        var fileName = currentFilePath != null ? Path.GetFileName(currentFilePath) : "<unknown>";
        MidlTrace.Write("COMPILE", $"Compiling {fileName} ({source.Length} chars, mode={mode}, {lazyProviders?.Count ?? 0} providers)");
        var sw = profile != null ? Stopwatch.StartNew() : null;

        // Validate @ref directives before preprocessing (uses original source for accurate positions)
        string? baseDir = currentFilePath != null ? Path.GetDirectoryName(Path.GetFullPath(currentFilePath)) : null;
        var refDiagnostics = RefDirectiveParser.ValidateRefDirectives(source, baseDir);

        // Save original source for position remapping after preprocessing
        var originalSource = source;

        // Run preprocessor before lexing
        var preprocessor = new Midl.Lexer.Preprocessor(defines, includePaths, currentFilePath);
        source = preprocessor.Process(source);

        // Build source map from preprocessor segment data
        SourceMap? sourceMap = BuildSourceMap(preprocessor);

        // Collect preprocessor-level diagnostics
        var syntaxDiagnostics = new List<Midl.Analysis.Diagnostics.Diagnostic>(refDiagnostics);
        if (mode == CompilationMode.Strict && preprocessor.HasIncludeDirectives)
        {
            foreach (var includePos in preprocessor.IncludeDirectivePositions)
            {
                syntaxDiagnostics.Add(DiagnosticDescriptors.IncludeDirectiveInStrictMode(includePos)
                    with { OriginalPosition = includePos, SourceFile = currentFilePath });
            }
        }

        // Report preprocessor errors as diagnostics (positions are in original source coordinates)
        foreach (var err in preprocessor.Errors)
        {
            syntaxDiagnostics.Add(new Diagnostics.Diagnostic(
                Diagnostics.DiagnosticSeverity.Error, "MIDL1074", err.Message, err.Position, 1)
                with { OriginalPosition = err.Position, SourceFile = currentFilePath });
        }

        // Report preprocessor warnings as diagnostics
        foreach (var warn in preprocessor.Warnings)
        {
            syntaxDiagnostics.Add(new Diagnostics.Diagnostic(
                Diagnostics.DiagnosticSeverity.Warning, "MIDL1074", warn.Message, warn.Position, 1)
                with { OriginalPosition = warn.Position, SourceFile = currentFilePath });
        }

        if (profile != null) { profile.Preprocess = sw!.Elapsed; sw.Restart(); }

        var lexer = new Midl.Lexer.Lexer(source);
        var tokens = lexer.Lex();

        if (profile != null) { profile.Lex = sw!.Elapsed; sw.Restart(); }

        var parser = new Midl.Parser.Parser(tokens);
        var tree = parser.Parse();

        if (profile != null) { profile.Parse = sw!.Elapsed; sw.Restart(); }

        // Collect syntax errors from the error-recovering parser's tree
        SyntaxErrorCollector.CollectErrors(tree, tokens, syntaxDiagnostics);

        if (profile != null) { profile.SyntaxErrors = sw!.Elapsed; sw.Restart(); }

        // Process import directives before binding
        ImportResolver importResolver = importContentProvider is null
            ? new ImportResolver(includePaths, currentFilePath)
            : new ImportResolver(includePaths, currentFilePath, importContentProvider);
        if (currentFilePath != null)
            importResolver.MarkImported(Path.GetFullPath(currentFilePath));
        var importedTrees = new List<(GreenNode Tree, IReadOnlyList<SyntaxToken> Tokens, string? FilePath, SourceMap? Map)>();
        CollectImportedTrees(tree, importResolver, importedTrees, syntaxDiagnostics,
            includePaths: includePaths,
            containingFilePath: currentFilePath
#if WITH_COMPAT
            , cppCmd: cppCmd
#endif
            );

        if (profile != null) { profile.ImportResolve = sw!.Elapsed; sw.Restart(); }

        // One map spanning every file this compilation preprocessed. Each imported file has
        // its own coordinate space, so a position is only resolvable if the unit it came
        // from is known alongside that unit's map.
        var unitSourceMaps = new CompilationSourceMap();
        unitSourceMaps.Add(currentFilePath, sourceMap);
        foreach (var imported in importedTrees)
            unitSourceMaps.Add(imported.FilePath, imported.Map);

        var binder = new Binder(mode, emitQualityHints, arraysEnabled);
        var compilation = binder.Bind(tree, tokens, externalTypes: null, importedTrees, lazyProviders,
            refDirectories, unitSourceMaps);
        compilation.UnitSourceMaps = unitSourceMaps;
        compilation.Macros = preprocessor.Macros;

        if (profile != null) { profile.Bind = sw!.Elapsed; sw.Restart(); }

        // Synthesize exclusive interfaces for runtime classes
        var synthesizer = new InterfaceSynthesizer();
        synthesizer.Synthesize(compilation);

        if (profile != null) { profile.Synthesize = sw!.Elapsed; sw.Restart(); }

        // Propagate version/contract info and validate constraints
        var propagator = new VersionPropagator(syntaxDiagnostics);
        propagator.Propagate(compilation);

        if (profile != null) { profile.VersionPropagate = sw!.Elapsed; sw.Restart(); }

        // Validate attribute semantics
        var validator = new AttributeValidator(compilation, syntaxDiagnostics);
        validator.Validate();
        validator.ValidateParityRules();
        validator.ValidateVersionArguments();

        if (profile != null) { profile.Validate = sw!.Elapsed; sw.Stop(); profile.Total = profile.Preprocess + profile.Lex + profile.Parse + profile.SyntaxErrors + profile.ImportResolve + profile.Bind + profile.Synthesize + profile.VersionPropagate + profile.Validate; }

        // Merge syntax diagnostics with semantic diagnostics
        if (syntaxDiagnostics.Count > 0)
        {
            compilation = compilation.WithAdditionalDiagnostics(syntaxDiagnostics);
        }

        // Attach source map if available
        if (sourceMap != null)
        {
            compilation.SourceMap = sourceMap;

            // Resolve diagnostic positions through the source map
            compilation = ResolveDiagnosticSourceFiles(compilation, sourceMap);
        }

        // For single-file preprocessing (no #include), the preprocessor preserves line count.
        // Remap positions from expanded text to original text using line+column correspondence.
        if (originalSource != source && !preprocessor.HasIncludeDirectives)
        {
            MidlTrace.Write("COMPILE", $"Remapping {compilation.Diagnostics.Count} diagnostics (expanded={source.Length} chars, original={originalSource.Length} chars)");
            compilation = RemapDiagnosticPositionsByLine(compilation, source, originalSource, currentFilePath);
        }
        else
        {
            MidlTrace.Write("COMPILE", $"No remap needed (same={originalSource == source}, hasIncludes={preprocessor.HasIncludeDirectives})");
        }

        // Attach inactive regions for editor graying
        if (preprocessor.InactiveRegions.Count > 0)
            compilation.InactiveRegions = preprocessor.InactiveRegions;

        List<string> importedFilePaths = [];
        foreach (var importedTree in importedTrees)
        {
            if (importedTree.FilePath is not null)
                importedFilePaths.Add(Path.GetFullPath(importedTree.FilePath));
        }

        compilation.ImportedFilePaths = importedFilePaths.ToArray();

        return compilation;
    }

    /// <summary>
    /// Resolves diagnostic positions through the SourceMap to set SourceFile and OriginalPosition.
    /// </summary>
    private static Compilation ResolveDiagnosticSourceFiles(Compilation compilation, SourceMap sourceMap)
    {
        if (!sourceMap.HasMappings || compilation.Diagnostics.Count == 0)
            return compilation;

        var resolved = new List<Diagnostics.Diagnostic>(compilation.Diagnostics.Count);
        bool anyChanged = false;

        foreach (var diag in compilation.Diagnostics)
        {
            // Skip diagnostics that already have positions resolved (preprocessor diagnostics)
            if (diag.OriginalPosition.HasValue)
            {
                resolved.Add(diag);
                continue;
            }

            var mapping = sourceMap.Resolve(diag.Position);
            if (mapping.HasValue)
            {
                resolved.Add(diag with
                {
                    SourceFile = mapping.Value.filePath,
                    OriginalPosition = mapping.Value.originalOffset
                });
                anyChanged = true;
            }
            else
            {
                resolved.Add(diag);
            }
        }

        if (!anyChanged)
            return compilation;

        var result = new Compilation(compilation.GlobalNamespace, resolved, compilation.LazyProviders);
        compilation.CopyCarriedStateTo(result);
        result.SourceMap = sourceMap;
        return result;
    }

    /// <summary>
    /// Remaps diagnostic positions from expanded text to original text using line+column correspondence.
    /// The preprocessor preserves line count (directive/inactive lines → empty lines), so
    /// line numbers are identical between expanded and original. When a macro substitution
    /// changes a line's length, columns are adjusted by searching for the diagnostic's token
    /// text in the original line near the expected position.
    /// </summary>
    private static Compilation RemapDiagnosticPositionsByLine(
        Compilation compilation, string expandedText, string originalText, string? filePath)
    {
        if (compilation.Diagnostics.Count == 0)
            return compilation;

        var expandedLineStarts = BuildLineStartsArray(expandedText);
        var originalLineStarts = BuildLineStartsArray(originalText);

        var resolved = new List<Diagnostics.Diagnostic>(compilation.Diagnostics.Count);
        bool anyChanged = false;

        foreach (var diag in compilation.Diagnostics)
        {
            // Skip diagnostics that already have OriginalPosition set
            // (e.g., preprocessor diagnostics with positions in original coordinates)
            if (diag.OriginalPosition.HasValue)
            {
                resolved.Add(diag);
                continue;
            }

            int pos = diag.Position;
            // Find line in expanded text
            int line = FindLine(expandedLineStarts, pos);
            int col = pos - expandedLineStarts[line];

            // Map to same line+col in original text
            if (line < originalLineStarts.Length)
            {
                int originalPos = originalLineStarts[line] + col;

                // When macro substitution changed the line length, the naïve same-column
                // mapping lands on the wrong character. Find the diagnostic's token text
                // in the original line to correct the column.
                int expandedLineEnd = (line + 1 < expandedLineStarts.Length)
                    ? expandedLineStarts[line + 1] : expandedText.Length;
                int originalLineEnd = (line + 1 < originalLineStarts.Length)
                    ? originalLineStarts[line + 1] : originalText.Length;

                var expandedLine = expandedText.AsSpan(expandedLineStarts[line],
                    expandedLineEnd - expandedLineStarts[line]);
                var originalLine = originalText.AsSpan(originalLineStarts[line],
                    originalLineEnd - originalLineStarts[line]);

                if (!expandedLine.SequenceEqual(originalLine) && diag.Length > 0 && col + diag.Length <= expandedLine.Length)
                {
                    // Structural alignment: find common prefix and suffix between
                    // expanded and original lines. Code in the prefix maps 1:1,
                    // code in the suffix maps 1:1 offset by the length difference,
                    // and the middle (macro expansion zone) maps to the start of
                    // the macro invocation in the original.
                    int maxPrefix = Math.Min(expandedLine.Length, originalLine.Length);
                    int prefixLen = 0;
                    while (prefixLen < maxPrefix && expandedLine[prefixLen] == originalLine[prefixLen])
                        prefixLen++;

                    int maxSuffix = Math.Min(expandedLine.Length - prefixLen, originalLine.Length - prefixLen);
                    int suffixLen = 0;
                    while (suffixLen < maxSuffix &&
                           expandedLine[expandedLine.Length - 1 - suffixLen] == originalLine[originalLine.Length - 1 - suffixLen])
                        suffixLen++;

                    if (col < prefixLen)
                    {
                        // In the common prefix — column is exact
                        originalPos = originalLineStarts[line] + col;
                    }
                    else if (col >= expandedLine.Length - suffixLen)
                    {
                        // In the common suffix — adjust by the length difference
                        int offsetFromEnd = expandedLine.Length - col;
                        originalPos = originalLineStarts[line] + (originalLine.Length - offsetFromEnd);
                    }
                    else
                    {
                        // In the middle (macro expansion) — point to start of
                        // the divergent region in the original
                        originalPos = originalLineStarts[line] + prefixLen;
                    }
                }

                // Clamp to original text length
                if (originalPos > originalText.Length)
                    originalPos = originalText.Length;
                resolved.Add(diag with
                {
                    SourceFile = filePath ?? diag.SourceFile,
                    OriginalPosition = originalPos
                });
                anyChanged = true;
            }
            else
            {
                resolved.Add(diag);
            }
        }

        if (!anyChanged)
            return compilation;

        var result = new Compilation(compilation.GlobalNamespace, resolved, compilation.LazyProviders);
        compilation.CopyCarriedStateTo(result);
        return result;
    }

    private static int[] BuildLineStartsArray(string text)
    {
        var starts = new List<int> { 0 };
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
                starts.Add(i + 1);
        }
        return starts.ToArray();
    }

    private static int FindLine(int[] lineStarts, int offset)
    {
        int lo = 0, hi = lineStarts.Length - 1;
        while (lo <= hi)
        {
            int mid = lo + (hi - lo) / 2;
            if (mid + 1 < lineStarts.Length && offset >= lineStarts[mid + 1])
                lo = mid + 1;
            else if (offset < lineStarts[mid])
                hi = mid - 1;
            else
                return mid;
        }
        return lo;
    }

    /// <summary>
    /// Builds a <see cref="SourceMap"/> from a preprocessor's segments, or null when the
    /// file needed no translation (no directives, no macro expansion, no conditionals).
    /// </summary>
    private static SourceMap? BuildSourceMap(Midl.Lexer.Preprocessor preprocessor)
    {
        if (preprocessor.SourceMapSegments.Count == 0) return null;

        var map = new SourceMap();
        foreach (var seg in preprocessor.SourceMapSegments)
            map.AddSegment(seg.ExpandedStart, seg.ExpandedEnd, seg.FilePath, seg.OriginalStart, seg.IsConstant);
        map.SortSegments();
        return map;
    }

    /// <summary>
    /// Walks the syntax tree to find ImportDirective nodes, resolves them,
    /// parses the imported files, and collects the resulting trees.
    /// </summary>
    private static void CollectImportedTrees(
        GreenNode tree,
        ImportResolver importResolver,
        List<(GreenNode Tree, IReadOnlyList<SyntaxToken> Tokens, string? FilePath, SourceMap? Map)> importedTrees,
        List<Midl.Analysis.Diagnostics.Diagnostic> diagnostics,
        IReadOnlyList<string>? includePaths = null,
        string? containingFilePath = null
#if WITH_COMPAT
        , string? cppCmd = null
#endif
        )
    {
        // Green nodes carry widths, not absolute positions; accumulate as we walk so a
        // failed import can point at its own path text rather than the start of the file.
        int offset = 0;
        for (int i = 0; i < tree.SlotCount; i++)
        {
            var child = tree.GetSlot(i);
            if (child == null) continue;
            if (child.Kind != SyntaxKind.ImportDirective) { offset += child.FullWidth; continue; }

            var childOffset = offset;
            offset += child.FullWidth;

            var pathToken = ExtractImportPath(child);
            if (pathToken == null) continue;

            var resolved = importResolver.ResolveImportPath(pathToken);
            if (resolved == null)
            {
                var pathPos = FindImportPathPosition(child, childOffset);
                diagnostics.Add(DiagnosticDescriptors.ImportFileNotFound(pathToken, pathPos)
                    with { SourceFile = containingFilePath, OriginalPosition = pathPos });
                continue;
            }

            if (importResolver.IsAlreadyImported(resolved)) continue;
            importResolver.MarkImported(resolved);

            // Preprocess imported file — try external cl.exe first, fall back to built-in.
            // Keep the segments the preprocessor produces: they are what makes a position
            // in this file's expanded text resolvable back to an original file and offset.
            string importedSource;
            SourceMap? importedMap = null;
#if WITH_COMPAT
            if (cppCmd != null)
            {
                var externalResult = Midl.Lexer.Preprocessor.ProcessWithExternalCpp(
                    resolved, includePaths, null, cppCmd);
                importedSource = externalResult ?? importResolver.ReadImportContent(resolved);
                if (externalResult == null)
                {
                    var pp = new Midl.Lexer.Preprocessor(null, includePaths, resolved);
                    importedSource = pp.Process(importedSource);
                    importedMap = BuildSourceMap(pp);
                }
            }
            else
#endif
            {
                importedSource = importResolver.ReadImportContent(resolved);
                var pp = new Midl.Lexer.Preprocessor(null, includePaths, resolved);
                importedSource = pp.Process(importedSource);
                importedMap = BuildSourceMap(pp);
            }

            var importedLexer = new Midl.Lexer.Lexer(importedSource);
            var importedTokens = importedLexer.Lex();
            var importedParser = new Midl.Parser.Parser(importedTokens);
            var importedTree = importedParser.Parse();

            importedTrees.Add((importedTree, importedTokens, resolved, importedMap));

            // Recursively collect transitive imports
            var childResolver = importResolver.CreateChildResolver(resolved);
            CollectImportedTrees(importedTree, childResolver, importedTrees, diagnostics,
                includePaths: includePaths,
                containingFilePath: resolved
#if WITH_COMPAT
                , cppCmd: cppCmd
#endif
                );
        }
    }

    /// <summary>Extracts the file path string from an ImportDirective node.</summary>
    private static string? ExtractImportPath(GreenNode importDirective)
    {
        for (int i = 0; i < importDirective.SlotCount; i++)
        {
            var slot = importDirective.GetSlot(i);
            if (slot is SyntaxToken { Kind: SyntaxKind.StringLiteralToken } token)
            {
                // Strip surrounding quotes
                var text = token.Text;
                if (text.Length >= 2 && text[0] == '"' && text[^1] == '"')
                    return text[1..^1];
                return text;
            }
        }
        return null;
    }

    /// <summary>
    /// Absolute position of the path text inside an <c>import "..."</c> directive,
    /// so a "file not found" squiggle covers the path rather than the start of the file.
    /// Skips the token's leading trivia and the opening quote.
    /// </summary>
    private static int FindImportPathPosition(GreenNode importDirective, int directiveOffset)
    {
        int offset = directiveOffset;
        for (int i = 0; i < importDirective.SlotCount; i++)
        {
            var slot = importDirective.GetSlot(i);
            if (slot == null) continue;

            if (slot is SyntaxToken { Kind: SyntaxKind.StringLiteralToken } token)
            {
                int pos = offset;
                foreach (var trivia in token.LeadingTrivia)
                    pos += trivia.FullWidth;
                if (token.Text.Length >= 2 && token.Text[0] == '"')
                    pos++;   // step past the opening quote onto the path text
                return pos;
            }

            offset += slot.FullWidth;
        }
        return directiveOffset;
    }
}
