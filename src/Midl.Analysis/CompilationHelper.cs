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

        var binder = new Binder(mode);
        var compilation = binder.Bind(tree, tokens, externalTypes);

        // Synthesize exclusive interfaces for runtime classes
        var synthesizer = new InterfaceSynthesizer();
        synthesizer.Synthesize(compilation);

        // Merge syntax diagnostics with semantic diagnostics
        if (syntaxDiagnostics.Count > 0)
        {
            compilation = compilation.WithAdditionalDiagnostics(syntaxDiagnostics);
        }

        return compilation;
    }
}
