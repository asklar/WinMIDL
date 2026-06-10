using Midl.Analysis;
using Midl.Syntax;

namespace Midl.Integration.Tests;

/// <summary>
/// Tests that parse real-world IDL files from an external fixture directory.
/// These tests reference external IDL files via the MIDL_TEST_FIXTURES env var
/// and skip gracefully when the env var is not set.
///
/// To run: set MIDL_TEST_FIXTURES=path\to\idl\fixtures
/// </summary>
public class ExternalFixtureTests
{
    private static readonly string? FixturesDir = Environment.GetEnvironmentVariable("MIDL_TEST_FIXTURES");

    public static bool HasFixtures => FixturesDir != null && Directory.Exists(FixturesDir);

    public static IEnumerable<object[]> GetExternalIdlFiles()
    {
        if (!HasFixtures)
        {
            // Yield a sentinel row so the Theory doesn't fail with "0 parameter values"
            yield return new object[] { "(skipped)", "" };
            yield break;
        }

        foreach (var file in Directory.GetFiles(FixturesDir!, "*.idl", SearchOption.TopDirectoryOnly))
        {
            yield return new object[] { Path.GetFileName(file), file };
        }
    }

    [SkippableTheory]
    [MemberData(nameof(GetExternalIdlFiles))]
    public void ExternalIdlFile_ParsesWithoutCrash(string name, string path)
    {
        Skip.If(!HasFixtures, "MIDL_TEST_FIXTURES not set");
        _ = name; // used for xUnit test display name

        var source = File.ReadAllText(path);

        // The parser must NEVER crash — it should always produce a tree
        var lexer = new Midl.Lexer.Lexer(source);
        var tokens = lexer.Lex();
        Assert.NotEmpty(tokens);

        var parser = new Midl.Parser.Parser(tokens);
        var tree = parser.Parse();
        Assert.NotNull(tree);
        Assert.Equal(SyntaxKind.CompilationUnit, tree.Kind);
    }

#if WITH_COMPAT
    [SkippableTheory]
    [MemberData(nameof(GetExternalIdlFiles))]
    public void ExternalIdlFile_CompilesWithoutCrash(string name, string path)
    {
        Skip.If(!HasFixtures, "MIDL_TEST_FIXTURES not set");
        _ = name; // used for xUnit test display name

        var source = File.ReadAllText(path);

        // Compilation in compat mode should not crash
        // (it may produce diagnostics, that's fine)
        var compilation = CompilationHelper.Compile(source, CompilationMode.Compat);
        Assert.NotNull(compilation);
    }
#endif
}
