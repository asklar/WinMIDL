using Midl.Analysis;
var c = CompilationHelper.Compile(""namespace N { interface IFoo { IAsyncOperation GetResult(); } }"");
Console.WriteLine(___BEGIN___COMMAND_DONE_MARKER___$LASTEXITCODE""Diagnostics: {{c.Diagnostics.Count}}"");
foreach (var d in c.Diagnostics) Console.WriteLine(___BEGIN___COMMAND_DONE_MARKER___$LASTEXITCODE""  {{d.Code}}: {{d.Message}}"");
