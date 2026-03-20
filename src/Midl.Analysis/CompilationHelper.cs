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

    public static Compilation Compile(string source, IReadOnlyList<string>? referencePaths,
                                       CompilationMode mode,
                                       IReadOnlyList<string>? includePaths,
                                       string? currentFilePath,
                                       IReadOnlyList<string>? defines = null,
                                        bool emitQualityHints = false,
                                        string? cppCmd = null)
    {
        MidlTrace.Write("COMPILE", $"Compiling {source.Length} chars, mode={mode}, {referencePaths?.Count ?? 0} references");

        // Run preprocessor before lexing
        var preprocessor = new Midl.Lexer.Preprocessor(defines, includePaths, currentFilePath);
        source = preprocessor.Process(source);

        // Collect preprocessor-level diagnostics
        var syntaxDiagnostics = new List<Midl.Analysis.Diagnostics.Diagnostic>();
        if (mode == CompilationMode.Strict && preprocessor.HasIncludeDirectives)
        {
            syntaxDiagnostics.Add(DiagnosticDescriptors.IncludeDirectiveInStrictMode(0));
        }

        // Report preprocessor errors as diagnostics
        foreach (var err in preprocessor.Errors)
        {
            syntaxDiagnostics.Add(new Diagnostics.Diagnostic(
                Diagnostics.DiagnosticSeverity.Error, "MIDL1074", err, 0, 1));
        }

        // Lazily index external types from referenced .winmd files
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

        var lexer = new Midl.Lexer.Lexer(source);
        var tokens = lexer.Lex();
        var parser = new Midl.Parser.Parser(tokens);
        var tree = parser.Parse();

        // Collect syntax errors from the error-recovering parser's tree
        SyntaxErrorCollector.CollectErrors(tree, tokens, syntaxDiagnostics);

        // Process import directives before binding
        var importResolver = new ImportResolver(includePaths, currentFilePath);
        if (currentFilePath != null)
            importResolver.MarkImported(Path.GetFullPath(currentFilePath));
        var importedTrees = new List<(GreenNode Tree, IReadOnlyList<SyntaxToken> Tokens)>();
        CollectImportedTrees(tree, importResolver, importedTrees, syntaxDiagnostics,
            includePaths: includePaths, cppCmd: cppCmd);

        var binder = new Binder(mode, emitQualityHints);
        var compilation = binder.Bind(tree, tokens, externalTypes: null, importedTrees, lazyProviders);

        // Synthesize exclusive interfaces for runtime classes
        var synthesizer = new InterfaceSynthesizer();
        synthesizer.Synthesize(compilation);

        // Propagate version/contract info and validate constraints
        var propagator = new VersionPropagator(syntaxDiagnostics);
        propagator.Propagate(compilation);

        // Validate attribute semantics
        var validator = new AttributeValidator(compilation, syntaxDiagnostics);
        validator.Validate();

        // Merge syntax diagnostics with semantic diagnostics
        if (syntaxDiagnostics.Count > 0)
        {
            compilation = compilation.WithAdditionalDiagnostics(syntaxDiagnostics);
        }

        return compilation;
    }

    /// <summary>
    /// Walks the syntax tree to find ImportDirective nodes, resolves them,
    /// parses the imported files, and collects the resulting trees.
    /// </summary>
    private static void CollectImportedTrees(
        GreenNode tree,
        ImportResolver importResolver,
        List<(GreenNode Tree, IReadOnlyList<SyntaxToken> Tokens)> importedTrees,
        List<Midl.Analysis.Diagnostics.Diagnostic> diagnostics,
        IReadOnlyList<string>? includePaths = null,
        string? cppCmd = null)
    {
        for (int i = 0; i < tree.SlotCount; i++)
        {
            var child = tree.GetSlot(i);
            if (child == null || child.Kind != SyntaxKind.ImportDirective) continue;

            var pathToken = ExtractImportPath(child);
            if (pathToken == null) continue;

            var resolved = importResolver.ResolveImportPath(pathToken);
            if (resolved == null)
            {
                diagnostics.Add(DiagnosticDescriptors.ImportFileNotFound(pathToken, 0));
                continue;
            }

            if (importResolver.IsAlreadyImported(resolved)) continue;
            importResolver.MarkImported(resolved);

            // Preprocess imported file — try external cl.exe first, fall back to built-in
            string importedSource;
            if (cppCmd != null)
            {
                var externalResult = Midl.Lexer.Preprocessor.ProcessWithExternalCpp(
                    resolved, includePaths, null, cppCmd);
                importedSource = externalResult ?? File.ReadAllText(resolved);
                if (externalResult == null)
                {
                    var pp = new Midl.Lexer.Preprocessor(null, includePaths, resolved);
                    importedSource = pp.Process(importedSource);
                }
            }
            else
            {
                importedSource = File.ReadAllText(resolved);
                var pp = new Midl.Lexer.Preprocessor(null, includePaths, resolved);
                importedSource = pp.Process(importedSource);
            }

            var importedLexer = new Midl.Lexer.Lexer(importedSource);
            var importedTokens = importedLexer.Lex();
            var importedParser = new Midl.Parser.Parser(importedTokens);
            var importedTree = importedParser.Parse();

            importedTrees.Add((importedTree, importedTokens));

            // Recursively collect transitive imports
            var childResolver = importResolver.CreateChildResolver(resolved);
            CollectImportedTrees(importedTree, childResolver, importedTrees, diagnostics,
                includePaths: includePaths, cppCmd: cppCmd);
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
}
