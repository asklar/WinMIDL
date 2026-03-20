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
                                       string? currentFilePath)
    {
        MidlTrace.Write("COMPILE", $"Compiling {source.Length} chars, mode={mode}, {referencePaths?.Count ?? 0} references");

        // Read external types from referenced .winmd files
        IReadOnlyList<ISymbol>? externalTypes = null;
        if (referencePaths is { Count: > 0 })
        {
            var reader = new WinMdReader();
            var allExternal = new List<ISymbol>();
            foreach (var path in referencePaths)
            {
                if (File.Exists(path))
                {
                    var types = reader.ReadTypes(path);
                    allExternal.AddRange(types);
                }
            }
            externalTypes = allExternal;
        }

        var lexer = new Midl.Lexer.Lexer(source);
        var tokens = lexer.Lex();
        var parser = new Midl.Parser.Parser(tokens);
        var tree = parser.Parse();

        // Collect syntax errors from the error-recovering parser's tree
        var syntaxDiagnostics = new List<Midl.Analysis.Diagnostics.Diagnostic>();
        SyntaxErrorCollector.CollectErrors(tree, tokens, syntaxDiagnostics);

        // Process import directives before binding
        var importResolver = new ImportResolver(includePaths, currentFilePath);
        if (currentFilePath != null)
            importResolver.MarkImported(Path.GetFullPath(currentFilePath));
        var importedTrees = new List<(GreenNode Tree, IReadOnlyList<SyntaxToken> Tokens)>();
        CollectImportedTrees(tree, importResolver, importedTrees, syntaxDiagnostics);

        var binder = new Binder(mode);
        var compilation = binder.Bind(tree, tokens, externalTypes, importedTrees);

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
        List<Midl.Analysis.Diagnostics.Diagnostic> diagnostics)
    {
        for (int i = 0; i < tree.SlotCount; i++)
        {
            var child = tree.GetSlot(i);
            if (child == null || child.Kind != SyntaxKind.ImportDirective) continue;

            // Extract the string literal from the import directive
            var pathToken = ExtractImportPath(child);
            if (pathToken == null) continue;

            var resolved = importResolver.ResolveImportPath(pathToken);
            if (resolved == null) continue; // File not found — silently skip for now

            if (importResolver.IsAlreadyImported(resolved)) continue; // Skip redundant
            importResolver.MarkImported(resolved);

            // Parse the imported file
            var importedSource = File.ReadAllText(resolved);
            var importedLexer = new Midl.Lexer.Lexer(importedSource);
            var importedTokens = importedLexer.Lex();
            var importedParser = new Midl.Parser.Parser(importedTokens);
            var importedTree = importedParser.Parse();

            importedTrees.Add((importedTree, importedTokens));
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
