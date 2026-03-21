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

    [Fact]
    public void ComposableAttribute_SetsIsUnsealed()
    {
        // Create an unsealed runtimeclass, emit to WinMD, read it back
        var refSource = @"
            namespace ExtLib
            {
                [default_interface]
                unsealed runtimeclass BaseWidget
                {
                    BaseWidget();
                };
            }";

        var refCompilation = CompilationHelper.Compile(refSource);
        Assert.False(refCompilation.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
            "Reference compilation should succeed");

        var emitter = new WinMdEmitter();
        var winmdBytes = emitter.EmitToBytes(refCompilation);

        var tempPath = Path.Combine(Path.GetTempPath(), $"extlib_unsealed_{Guid.NewGuid()}.winmd");
        try
        {
            File.WriteAllBytes(tempPath, winmdBytes);

            // Read it back via LazyWinMdProvider
            using var provider = new LazyWinMdProvider(tempPath);
            var symbol = provider.TryResolve("ExtLib.BaseWidget");
            Assert.NotNull(symbol);
            Assert.IsType<RuntimeClassSymbol>(symbol);
            var rc = (RuntimeClassSymbol)symbol!;
            Assert.True(rc.IsUnsealed, "Composable runtimeclass should be IsUnsealed");
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void DeriveFromUnsealedExternalClass_NoError()
    {
        // Create an unsealed base class WinMD
        var refSource = @"
            namespace ExtLib
            {
                unsealed runtimeclass BaseWidget
                {
                    BaseWidget();
                };
            }";

        var refCompilation = CompilationHelper.Compile(refSource);
        var emitter = new WinMdEmitter();
        var winmdBytes = emitter.EmitToBytes(refCompilation);

        var tempPath = Path.Combine(Path.GetTempPath(), $"extlib_derive_{Guid.NewGuid()}.winmd");
        try
        {
            File.WriteAllBytes(tempPath, winmdBytes);

            // Derive from the unsealed class
            var appSource = @"
                namespace MyApp
                {
                    runtimeclass MyWidget : ExtLib.BaseWidget
                    {
                        MyWidget();
                    };
                }";

            var compilation = CompilationHelper.Compile(appSource, new[] { tempPath });
            var errors = compilation.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            Assert.Empty(errors);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void DeriveFromSealedExternalClass_ProducesError()
    {
        // Create a sealed (default) class WinMD
        var refSource = @"
            namespace ExtLib
            {
                runtimeclass SealedWidget
                {
                    SealedWidget();
                };
            }";

        var refCompilation = CompilationHelper.Compile(refSource);
        var emitter = new WinMdEmitter();
        var winmdBytes = emitter.EmitToBytes(refCompilation);

        var tempPath = Path.Combine(Path.GetTempPath(), $"extlib_sealed_{Guid.NewGuid()}.winmd");
        try
        {
            File.WriteAllBytes(tempPath, winmdBytes);

            var appSource = @"
                namespace MyApp
                {
                    runtimeclass MyWidget : ExtLib.SealedWidget
                    {
                        MyWidget();
                    };
                }";

            var compilation = CompilationHelper.Compile(appSource, new[] { tempPath });
            var errors = compilation.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            Assert.Contains(errors, e => e.Code == "MIDL1036");
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void RefDirectory_ResolvesWinMdByNamespace()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"winmidl_refdir_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            // Create a WinMD named by namespace convention
            var refSource = @"
                namespace ExtLib
                {
                    enum WidgetMode { Normal, Compact };
                }";

            var refCompilation = CompilationHelper.Compile(refSource);
            var emitter = new WinMdEmitter();
            var winmdBytes = emitter.EmitToBytes(refCompilation);
            File.WriteAllBytes(Path.Combine(tempDir, "ExtLib.winmd"), winmdBytes);

            // Compile IDL that references the type, passing the directory
            var appSource = @"
                namespace MyApp
                {
                    runtimeclass Controller
                    {
                        Controller();
                        ExtLib.WidgetMode GetMode();
                    };
                }";

            var compilation = CompilationHelper.Compile(appSource,
                referencePaths: null,
                mode: CompilationMode.Strict,
                includePaths: null,
                currentFilePath: null,
                refDirectories: new[] { tempDir });

            var errors = compilation.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            Assert.Empty(errors);

            var mode = compilation.GetTypeByName("ExtLib.WidgetMode");
            Assert.NotNull(mode);
            Assert.True(mode!.IsExternal);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void RefDirectory_WalksUpNamespace()
    {
        // For type A.B.C.Foo, should find A.B.winmd if A.B.C.winmd doesn't exist
        var tempDir = Path.Combine(Path.GetTempPath(), $"winmidl_refdir2_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var refSource = @"
                namespace Company.Lib.Controls
                {
                    enum ControlSize { Small, Medium, Large };
                }";

            var refCompilation = CompilationHelper.Compile(refSource);
            var emitter = new WinMdEmitter();
            var winmdBytes = emitter.EmitToBytes(refCompilation);
            // Name it Company.Lib.winmd (not Company.Lib.Controls.winmd)
            File.WriteAllBytes(Path.Combine(tempDir, "Company.Lib.winmd"), winmdBytes);

            var appSource = @"
                namespace MyApp
                {
                    runtimeclass Widget
                    {
                        Widget();
                        Company.Lib.Controls.ControlSize Size{ get; };
                    };
                }";

            var compilation = CompilationHelper.Compile(appSource,
                referencePaths: null,
                mode: CompilationMode.Strict,
                includePaths: null,
                currentFilePath: null,
                refDirectories: new[] { tempDir });

            var errors = compilation.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            Assert.Empty(errors);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
