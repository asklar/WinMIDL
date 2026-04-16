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

    [Fact]
    public void LineContinuation_InComment_NotJoined()
    {
        // Backslash at end of a // comment line should NOT join with next line
        var pp = new Preprocessor();
        var source = "// @ref: D:\\path\\to\\file\nnamespace N { }";
        var result = pp.Process(source);
        // Both lines should be preserved separately
        var lines = result.Split('\n');
        Assert.True(lines.Length >= 2, $"Expected 2+ lines, got {lines.Length}: '{result}'");
        Assert.Contains("namespace N", result);
    }

    [Fact]
    public void LineContinuation_InCode_StillJoined()
    {
        // Backslash at end of a code line SHOULD join with next line (with space)
        var pp = new Preprocessor();
        var source = "#define FOO ba\\\nr\nFOO";
        var result = pp.Process(source);
        // Continuation joins "ba" + " " + "r" = "ba r"
        Assert.Contains("ba r", result);
    }

    [Fact]
    public void TokenPaste_FunctionMacro_Basic()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define PASTE(a,b) a##b\nPASTE(foo,bar)");
        Assert.Contains("foobar", result);
    }

    [Fact]
    public void TokenPaste_FunctionMacro_WithNumbers()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define PASTE(a,b) a##b\nPASTE(x,123)");
        Assert.Contains("x123", result);
    }

    [Fact]
    public void TokenPaste_FunctionMacro_Prefix()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define MAKE(n) item_##n\nMAKE(1)");
        Assert.Contains("item_1", result);
    }

    [Fact]
    public void TokenPaste_ObjectLikeMacro()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define AB c##d\nAB");
        Assert.Contains("cd", result);
    }

    [Fact]
    public void TokenPaste_Multiple()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define TRIPLE(a,b,c) a##b##c\nTRIPLE(x,y,z)");
        Assert.Contains("xyz", result);
    }

    [Fact]
    public void TokenPaste_WithWhitespace()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define PASTE(a,b) a ## b\nPASTE(foo,bar)");
        Assert.Contains("foobar", result);
    }

    [Fact]
    public void Stringize_Basic()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define STR(x) #x\nSTR(hello)");
        Assert.Contains("\"hello\"", result);
    }

    [Fact]
    public void Stringize_WithSpaces()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define STR(x) #x\nSTR(foo bar)");
        Assert.Contains("\"foo bar\"", result);
    }

    [Fact]
    public void Stringize_WithNumbers()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define STR(x) #x\nSTR(123)");
        Assert.Contains("\"123\"", result);
    }

    [Fact]
    public void Stringize_CombinedWithText()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define MSG(x) \"Error: \" #x\nMSG(fail)");
        Assert.Contains("\"Error: \" \"fail\"", result);
    }

    [Fact]
    public void Stringize_MultipleParams()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define PAIR(a,b) #a \" and \" #b\nPAIR(hello,world)");
        Assert.Contains("\"hello\" \" and \" \"world\"", result);
    }

    [Fact]
    public void Stringize_DoesNotMatchTokenPaste()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define PASTE(a,b) a##b\nPASTE(foo,bar)");
        Assert.Contains("foobar", result);
        Assert.DoesNotContain("\"foo\"", result);
        Assert.DoesNotContain("\"bar\"", result);
    }

    [Fact]
    public void VariadicMacro_Basic()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define LOG(fmt, ...) fmt __VA_ARGS__\nLOG(\"hi\", a, b)");
        Assert.Contains("\"hi\" a, b", result);
    }

    [Fact]
    public void VariadicMacro_OnlyVariadic()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define ALL(...) __VA_ARGS__\nALL(a, b, c)");
        Assert.Contains("a, b, c", result);
    }

    [Fact]
    public void VariadicMacro_ZeroVariadicArgs()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define OPT(x, ...) x __VA_ARGS__\nOPT(hello)");
        Assert.Contains("hello", result);
    }

    [Fact]
    public void VariadicMacro_WithRegularParams()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define F(a, b, ...) a + b + __VA_ARGS__\nF(1, 2, 3, 4)");
        Assert.Contains("1 + 2 + 3, 4", result);
    }

    [Fact]
    public void VariadicMacro_SingleVariadicArg()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define ALL(...) __VA_ARGS__\nALL(solo)");
        Assert.Contains("solo", result);
    }

    [Fact]
    public void CommentStripping_LineComment_MacroNotExpanded()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define FOO replaced\n// FOO should not expand\nInt32 x;");
        Assert.DoesNotContain("replaced", result);
        Assert.Contains("// FOO should not expand", result); // comment preserved
        Assert.Contains("Int32 x;", result);
    }

    [Fact]
    public void CommentStripping_InlineBlockComment_MacroNotExpanded()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define FOO replaced\nInt32 /* FOO */ x;");
        Assert.DoesNotContain("replaced", result);
        Assert.Contains("/* FOO */", result); // comment preserved verbatim
        Assert.Contains("Int32", result);
        Assert.Contains("x;", result);
    }

    [Fact]
    public void CommentStripping_MultiLineBlockComment_MacroNotExpanded()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define FOO replaced\n/* FOO\nFOO\nFOO */\nInt32 x;");
        Assert.DoesNotContain("replaced", result);
        Assert.Contains("/* FOO", result);  // comment text preserved
        Assert.Contains("FOO */", result);
        Assert.Contains("Int32 x;", result);
    }

    [Fact]
    public void CommentStripping_PreservesLineCount()
    {
        var pp = new Preprocessor();
        // 6 source lines (including #define) → 6 output lines
        var source = "#define M val\n/* start\nM inside\nend */\nM outside\nInt32 x;";
        var result = pp.Process(source);
        var lines = result.Split('\n');
        Assert.Equal(6, lines.Length);
        Assert.Contains("val", result);       // M expanded on code line
        Assert.Contains("Int32 x;", result);
    }

    [Fact]
    public void CommentStripping_CodeAfterBlockCommentEnd_StillProcessed()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define FOO replaced\n/* comment */ FOO;");
        Assert.Contains("replaced;", result);
        Assert.Contains("/* comment */", result); // comment preserved
    }

    [Fact]
    public void CommentStripping_CodeBeforeBlockCommentStart_StillProcessed()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define FOO replaced\nFOO /* comment */;");
        Assert.Contains("replaced", result);
        Assert.Contains("/* comment */", result); // comment preserved
    }

    [Fact]
    public void CommentStripping_SALLikeMacrosInComments_NoExplosion()
    {
        // Simulates the specstrings.h scenario: macros that expand to other macros,
        // appearing inside documentation comments. Without comment stripping this would
        // cause exponential string growth.
        var pp = new Preprocessor();
        var source = """
            #define _Inner_ INNER_EXPANDED
            #define _Outer_(x) _Inner_ x
            #define _SAL_(a,b) _Outer_(a) _Outer_(b)
            // Example: _SAL_(_Inner_, _Outer_(z)) should NOT expand
            Int32 x;
            """;
        var result = pp.Process(source);
        Assert.DoesNotContain("INNER_EXPANDED", result);
        Assert.Contains("Int32 x;", result);
    }

    [Fact]
    public void CommentStripping_DocComment_Preserved()
    {
        // /// doc comments must survive preprocessing for the lexer
        var pp = new Preprocessor();
        var result = pp.Process("#define FOO bar\n/// This is a doc comment about FOO\nInt32 x;");
        Assert.Contains("/// This is a doc comment about FOO", result);
        Assert.DoesNotContain("bar", result); // macro NOT expanded inside comment
    }

    [Fact]
    public void CommentStripping_ColumnOffset_Preserved()
    {
        // Code after an inline comment should keep its original column offset
        var pp = new Preprocessor();
        var source = "Int32 /* comment */ x;";
        var result = pp.Process(source);
        Assert.Equal(source, result.TrimEnd()); // nothing changed — no macros to expand
    }

    [Fact]
    public void MacroExpansion_SpecstringsH_DoesNotExplode()
    {
        // Regression test for the 25 GB specstrings.h bug.
        // specstrings.h contains documentation comments with SAL annotation macros
        // (_Inout_, _SAL1_Source_, etc.) that are real macros defined by sal.h.
        // Without comment-awareness, SubstituteMacros recursively expands them
        // inside comments, allocating ~28 GB for 307 KB of output.
        var sdkShared = @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.26100.0\shared";
        if (!Directory.Exists(sdkShared))
        {
            // Skip on machines without the Windows SDK installed
            return;
        }

        var pp = new Preprocessor(includePaths: [sdkShared]);
        var source = "#include <specstrings.h>\nInt32 x;";

        var before = GC.GetTotalAllocatedBytes(precise: true);
        var result = pp.Process(source);
        var after = GC.GetTotalAllocatedBytes(precise: true);
        var allocMB = (after - before) / (1024.0 * 1024.0);

        Assert.Contains("Int32 x;", result);
        // With the fix this allocates < 100 MB. Without it, it blows up to 25+ GB.
        Assert.True(allocMB < 512,
            $"Processing specstrings.h allocated {allocMB:F0} MB — " +
            $"possible macro expansion inside comments (pre-fix was ~28,000 MB)");
    }
}
