using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.Lsp.Handlers;

var source = "namespace N { runtimeclass Foo { event Windows.Foundation.TypedEventHandler<Foo, Object> Changed; } }";

// Step 1: Compile and find the symbol
var compilation = CompilationHelper.Compile(source);
Console.WriteLine($"Diagnostics: {compilation.Diagnostics.Count}");
foreach (var d in compilation.Diagnostics) Console.WriteLine($"  {d.Code}: {d.Message}");

// Step 2: Search for TypedEventHandler
var allTypes = compilation.GetAllTypes().ToList();
Console.WriteLine($"\nAll types ({allTypes.Count}):");
foreach (var t in allTypes) Console.WriteLine($"  {t.FullName} (External={t.IsExternal}, WinMd={t.SourceWinMdPath})");

// Step 3: Try FindSymbol logic
Console.WriteLine($"\nLazy type names:");
foreach (var n in compilation.GetAllLazyTypeNames().Take(20)) Console.WriteLine($"  {n}");

// Step 4: Check what HandleCore returns
int col = source.IndexOf("TypedEventHandler");
Console.WriteLine($"\nF12 at col {col} (TypedEventHandler)");
var result = GotoDefinitionHandler.HandleCore(source, "file:///test.idl", 0, col);
Console.WriteLine($"Result: {(result == null ? "NULL" : System.Text.Json.JsonSerializer.Serialize(result))}");
