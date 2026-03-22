using System.Diagnostics;
using Midl.Lexer;

// Generate a representative MIDL source
var sb = new System.Text.StringBuilder(50000);
sb.AppendLine("// Test MIDL file for benchmarking");
for (int ns = 0; ns < 5; ns++)
{
    sb.AppendLine($"#define MACRO_{ns} {ns}");
    sb.AppendLine($"namespace Test.Ns{ns}");
    sb.AppendLine("{");
    for (int cls = 0; cls < 10; cls++)
    {
        sb.AppendLine($"    [contract(TestContract, MACRO_{ns})]");
        sb.AppendLine($"    runtimeclass Widget{cls}");
        sb.AppendLine("    {");
        sb.AppendLine($"        Widget{cls}();");
        sb.AppendLine($"        static Int32 Count {{ get; }};");
        for (int m = 0; m < 5; m++)
        {
            sb.AppendLine($"        void Method{m}(String name, out Int32 result);");
        }
        sb.AppendLine("    }");
    }
    sb.AppendLine("}");
}
string source = sb.ToString();
Console.WriteLine($"Source: {source.Length} chars");

// Warmup
var pp = new Preprocessor();
string processed = pp.Process(source);
var lexer = new Lexer(processed);
lexer.Lex();

// Benchmark preprocessor
const int iterations = 200;
var sw = Stopwatch.StartNew();
for (int i = 0; i < iterations; i++)
{
    pp = new Preprocessor();
    pp.Process(source);
}
sw.Stop();
double ppMs = sw.Elapsed.TotalMilliseconds / iterations;
Console.WriteLine($"Preprocess: {ppMs:F3}ms avg ({iterations} iterations)");

// Benchmark lexer
sw.Restart();
for (int i = 0; i < iterations; i++)
{
    lexer = new Lexer(processed);
    lexer.Lex();
}
sw.Stop();
double lexMs = sw.Elapsed.TotalMilliseconds / iterations;
Console.WriteLine($"Lex: {lexMs:F3}ms avg ({iterations} iterations)");

// Check allocation savings
GC.Collect();
long before = GC.GetAllocatedBytesForCurrentThread();
lexer = new Lexer(processed);
var tokens = lexer.Lex();
long after = GC.GetAllocatedBytesForCurrentThread();
Console.WriteLine($"Lex allocations: {(after - before) / 1024}KB for {tokens.Count} tokens");

// Verify keyword interning
var kw1 = tokens.First(t => t.Kind == Midl.Syntax.SyntaxKind.NamespaceKeyword);
var kw2 = tokens.Where(t => t.Kind == Midl.Syntax.SyntaxKind.NamespaceKeyword).Skip(1).First();
Console.WriteLine($"Keyword interning works: {ReferenceEquals(kw1.Text, kw2.Text)}");
