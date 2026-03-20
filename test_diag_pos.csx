using Midl.Analysis;
var source = "// test\r\n\r\nnamespace{\r\n    interface foo\r\n    {\r\n        float x();\r\n    }\r\n}";
var compilation = CompilationHelper.Compile(source);
foreach (var d in compilation.Diagnostics)
{
    var line = source[..d.Position].Count(c => c == '\n');
    var text = d.Position + d.Length <= source.Length ? source.Substring(d.Position, d.Length) : "???";
    Console.WriteLine($"{d.Code} pos={d.Position} len={d.Length} line={line} text='{text}' msg={d.Message}");
}
