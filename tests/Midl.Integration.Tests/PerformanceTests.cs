// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.Analysis.Diagnostics;

namespace Midl.Integration.Tests;

public class PerformanceTests
{
    [Fact]
    public void CompilationCompletesInReasonableTime()
    {
        // A moderately complex MIDL3 file should compile quickly
        var source = @"
namespace PerfTest
{
    delegate void Handler(Int32 code, String msg);

    struct Point { Int32 X; Int32 Y; };
    enum Color { Red, Green, Blue };

    [flags]
    enum Permissions { None = 0, Read = 1, Write = 2 };

    interface IWidget
    {
        void Activate();
        String Name { get; };
        Boolean IsEnabled { get; set; };
        event Handler Changed;
    }

    runtimeclass Widget
    {
        Widget();
        Widget(String name);
        static Int32 Count { get; };
        void DoWork(Int32 iterations);
        String Transform(String input);
    }
}";

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var compilation = CompilationHelper.Compile(source);
        sw.Stop();

        Assert.True(sw.ElapsedMilliseconds < 500,
            $"Compilation took {sw.ElapsedMilliseconds}ms (should be < 500ms)");

        // Also verify it's correct
        var errors = compilation.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void LexerPerformance()
    {
        // Generate a large-ish source
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("namespace PerfTest {");
        for (int i = 0; i < 100; i++)
        {
            sb.AppendLine($"    runtimeclass Class{i} {{");
            sb.AppendLine($"        Class{i}();");
            for (int j = 0; j < 10; j++)
                sb.AppendLine($"        void Method{j}(Int32 p{j});");
            sb.AppendLine("    }");
        }
        sb.AppendLine("}");
        var source = sb.ToString();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var lexer = new Midl.Lexer.Lexer(source);
        var tokens = lexer.Lex();
        sw.Stop();

        Assert.True(sw.ElapsedMilliseconds < 200,
            $"Lexing {source.Length} chars ({tokens.Count} tokens) took {sw.ElapsedMilliseconds}ms");
    }
}
