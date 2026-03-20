using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;
using Midl.CodeGen.WinMd;

namespace Midl.Integration.Tests;

public class ReferenceTests
{
    [Fact]
    public void CompileWithReference()
    {
        // Create a .winmd with a type
        var refSource = @"
            namespace ExtLib
            {
                [uuid(11111111-2222-3333-4444-555555555555)]
                interface IWidget
                {
                    void Activate();
                };

                enum WidgetState
                {
                    Idle,
                    Active,
                    Disposed
                };
            }";

        var refCompilation = CompilationHelper.Compile(refSource);
        Assert.False(refCompilation.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
            "Reference compilation should succeed");

        var emitter = new WinMdEmitter();
        var winmdBytes = emitter.EmitToBytes(refCompilation);

        var tempPath = Path.Combine(Path.GetTempPath(), $"extlib_{Guid.NewGuid()}.winmd");
        try
        {
            File.WriteAllBytes(tempPath, winmdBytes);

            // Compile IDL that references types from the .winmd
            var appSource = @"
                namespace MyApp
                {
                    runtimeclass WidgetController
                    {
                        WidgetController();
                        ExtLib.WidgetState GetState();
                    };
                }";

            var compilation = CompilationHelper.Compile(appSource, new[] { tempPath });

            // Should compile with no errors
            Assert.False(compilation.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
                "Compilation with reference should succeed");

            // The referenced type should be resolvable
            var widgetState = compilation.GetTypeByName("ExtLib.WidgetState");
            Assert.NotNull(widgetState);
            Assert.True(widgetState.IsExternal);
            Assert.Equal(SymbolKind.Enum, widgetState.Kind);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }
}
