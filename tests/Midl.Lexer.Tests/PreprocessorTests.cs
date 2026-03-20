using Midl.Lexer;

namespace Midl.Lexer.Tests;

public class PreprocessorTests
{
    [Fact]
    public void DefineAndSubstitute()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define FOO 42\nInt32 x = FOO;");
        Assert.Contains("Int32 x = 42;", result);
        Assert.DoesNotContain("FOO", result);
    }

    [Fact]
    public void IfdefTrue()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define FOO\n#ifdef FOO\nincluded\n#endif");
        Assert.Contains("included", result);
    }

    [Fact]
    public void IfdefFalse()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#ifdef UNDEFINED\nskipped\n#endif");
        Assert.DoesNotContain("skipped", result);
    }

    [Fact]
    public void IfndefTrue()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#ifndef UNDEFINED\nincluded\n#endif");
        Assert.Contains("included", result);
    }

    [Fact]
    public void IfElse()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#ifdef X\na\n#else\nb\n#endif");
        Assert.Contains("b", result);
        // 'a' should be skipped
        var lines = result.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
        Assert.DoesNotContain("a", lines);
    }

    [Fact]
    public void NestedIfdef()
    {
        var pp = new Preprocessor();
        var input = "#define A\n#ifdef A\nouter\n#ifdef B\nskipped\n#else\ninner\n#endif\n#endif";
        var result = pp.Process(input);
        Assert.Contains("outer", result);
        Assert.Contains("inner", result);
        Assert.DoesNotContain("skipped", result);
    }

    [Fact]
    public void Undef()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define X\n#undef X\n#ifdef X\nskipped\n#endif");
        Assert.DoesNotContain("skipped", result);
    }

    [Fact]
    public void UndefStopsSubstitution()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define FOO 1\n#undef FOO\nFOO");
        // FOO must NOT be substituted after #undef — it should remain as literal "FOO"
        Assert.Contains("FOO", result);
        Assert.DoesNotContain("1", result);
    }

    [Fact]
    public void PragmaOnce()
    {
        var dir = Path.Combine(Path.GetTempPath(), "midl_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var headerPath = Path.Combine(dir, "header.h");
            File.WriteAllText(headerPath, "#pragma once\nHEADER_CONTENT");

            var mainPath = Path.Combine(dir, "main.idl");
            var source = "#include \"header.h\"\n#include \"header.h\"\nDONE";

            var pp = new Preprocessor(currentFilePath: mainPath);
            var result = pp.Process(source);

            // HEADER_CONTENT should appear exactly once
            var count = CountOccurrences(result, "HEADER_CONTENT");
            Assert.Equal(1, count);
            Assert.Contains("DONE", result);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void PreDefinedMacros()
    {
        var pp = new Preprocessor();
        // NTDDI_VERSION should be defined
        var result = pp.Process("#ifdef NTDDI_VERSION\ndefined\n#endif");
        Assert.Contains("defined", result);

        // __midl should be 1
        var result2 = pp.Process("#if __midl\nmidl_active\n#endif");
        Assert.Contains("midl_active", result2);
    }

    [Fact]
    public void CliDefines()
    {
        var pp = new Preprocessor(defines: new[] { "FOO=bar" });
        var result = pp.Process("value is FOO;");
        Assert.Contains("value is bar;", result);
    }

    [Fact]
    public void IncludeFile()
    {
        var dir = Path.Combine(Path.GetTempPath(), "midl_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var includePath = Path.Combine(dir, "types.idl");
            File.WriteAllText(includePath, "typedef int MYINT;");

            var mainPath = Path.Combine(dir, "main.idl");

            var pp = new Preprocessor(currentFilePath: mainPath);
            var result = pp.Process("#include \"types.idl\"\nMYINT x;");
            Assert.Contains("typedef int MYINT;", result);
            Assert.Contains("MYINT x;", result);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void RealWorldNtddi()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#if (NTDDI_VERSION >= NTDDI_WIN8)\nincluded\n#endif");
        Assert.Contains("included", result);
    }

    [Fact]
    public void IncludeFileNotFound()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#include \"nonexistent.h\"");
        // Unresolved #include should produce empty output (directive consumed)
        Assert.DoesNotContain("nonexistent", result);
    }

    [Fact]
    public void IfDefinedSyntax()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#if defined(MIDL_PASS)\nyes\n#endif");
        Assert.Contains("yes", result);
    }

    [Fact]
    public void ElifChain()
    {
        var pp = new Preprocessor();
        var input = "#if 0\nfirst\n#elif 0\nsecond\n#elif 1\nthird\n#else\nfourth\n#endif";
        var result = pp.Process(input);
        Assert.DoesNotContain("first", result);
        Assert.DoesNotContain("second", result);
        Assert.Contains("third", result);
        Assert.DoesNotContain("fourth", result);
    }

    [Fact]
    public void LogicalOperators()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#if defined(_WIN32) && defined(_WIN64)\nboth\n#endif");
        Assert.Contains("both", result);
    }

    [Fact]
    public void NotOperator()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#if !defined(NONEXISTENT)\nyes\n#endif");
        Assert.Contains("yes", result);
    }

    [Fact]
    public void WholeWordSubstitution()
    {
        var pp = new Preprocessor(defines: new[] { "A=1" });
        // Should not substitute 'A' inside 'BAD'
        var result = pp.Process("BAD A OKAY");
        Assert.Contains("BAD 1 OKAY", result);
    }

    [Fact]
    public void EmptyDefine()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define MARKER\n#ifdef MARKER\nfound\n#endif");
        Assert.Contains("found", result);
    }

    [Fact]
    public void PragmaWarningPassthrough()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#pragma warning(disable: 4001)");
        // Unknown pragmas are consumed (directive dropped), not injected as comments
        Assert.DoesNotContain("#pragma", result);
        Assert.DoesNotContain("/*", result);
    }

    [Fact]
    public void IncludeWithSearchPaths()
    {
        var dir = Path.Combine(Path.GetTempPath(), "midl_test_" + Guid.NewGuid().ToString("N"));
        var incDir = Path.Combine(dir, "inc");
        Directory.CreateDirectory(incDir);
        try
        {
            File.WriteAllText(Path.Combine(incDir, "shared.h"), "SHARED_CONTENT");

            var pp = new Preprocessor(includePaths: new[] { incDir });
            var result = pp.Process("#include <shared.h>\nrest");
            Assert.Contains("SHARED_CONTENT", result);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    private static int CountOccurrences(string text, string pattern)
    {
        int count = 0, idx = 0;
        while ((idx = text.IndexOf(pattern, idx, StringComparison.Ordinal)) >= 0)
        {
            count++;
            idx += pattern.Length;
        }
        return count;
    }
}
