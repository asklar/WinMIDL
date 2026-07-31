// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.Cli;
using Midl.CodeGen.WinMd;

namespace Midl.Integration.Tests;

/// <summary>
/// Integration tests for the --manifest CLI mode.
/// </summary>
public class ManifestCliTests : IDisposable
{
    private readonly string _tempDir;

    public ManifestCliTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "midl_manifest_tests_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { }
    }

    /// <summary>
    /// Compile IDL source to a .winmd file on disk, return the path.
    /// </summary>
    private string CreateWinMd(string source, string fileName = "test.winmd")
    {
        var compilation = CompilationHelper.Compile(source);
        Assert.Empty(compilation.Diagnostics);
        var emitter = new WinMdEmitter();
        var bytes = emitter.EmitToBytes(compilation);
        var path = Path.Combine(_tempDir, fileName);
        File.WriteAllBytes(path, bytes);
        return path;
    }

    private static (int ExitCode, string StdOut, string StdErr) Run(params string[] args)
    {
        var options = CommandLineParser.Parse(args);
        var stdOut = new StringWriter();
        var stdErr = new StringWriter();
        var driver = new CompilationDriver(stdOut, stdErr);
        var exitCode = driver.Run(options);
        return (exitCode, stdOut.ToString(), stdErr.ToString());
    }

    // -----------------------------------------------------------------------
    // 1. Missing --server_dll → error, non-zero exit
    // -----------------------------------------------------------------------
    [Fact]
    public void MissingServerDll_Error()
    {
        var winmd = CreateWinMd("""
            namespace TestNs
            {
                runtimeclass Widget { Widget(); void DoWork(); }
            }
            """);

        var outputPath = Path.Combine(_tempDir, "out.manifest");
        var (exit, _, stderr) = Run("--manifest", outputPath, "/nologo", "/no_def_idir", winmd);

        Assert.NotEqual(0, exit);
        Assert.Contains("--server_dll", stderr);
    }

    // -----------------------------------------------------------------------
    // 2. Missing input file → error
    // -----------------------------------------------------------------------
    [Fact]
    public void MissingInput_Error()
    {
        var outputPath = Path.Combine(_tempDir, "out.manifest");
        var (exit, _, stderr) = Run("--manifest", outputPath, "--server_dll", "foo.dll", "/nologo", "/no_def_idir");

        Assert.NotEqual(0, exit);
        Assert.Contains("no input", stderr, StringComparison.OrdinalIgnoreCase);
    }

    // -----------------------------------------------------------------------
    // 3. Non-winmd input → error
    // -----------------------------------------------------------------------
    [Fact]
    public void NonWinmdInput_Error()
    {
        var idlPath = Path.Combine(_tempDir, "input.idl");
        File.WriteAllText(idlPath, "namespace Foo { }");
        var outputPath = Path.Combine(_tempDir, "out.manifest");

        var (exit, _, stderr) = Run("--manifest", outputPath, "--server_dll", "foo.dll", "/nologo", "/no_def_idir", idlPath);

        Assert.NotEqual(0, exit);
        Assert.Contains(".winmd", stderr);
    }

    // -----------------------------------------------------------------------
    // 4. SxS output — full pipeline
    // -----------------------------------------------------------------------
    [Fact]
    public void SxsOutput_FullPipeline()
    {
        var winmd = CreateWinMd("""
            namespace TestNs
            {
                runtimeclass Widget
                {
                    Widget();
                    String Name { get; };
                }
            }
            """);

        var outputPath = Path.Combine(_tempDir, "app.manifest");
        var (exit, stdout, stderr) = Run("--manifest", outputPath, "--server_dll", "TestComponent.dll", "/nologo", "/no_def_idir", winmd);

        Assert.Equal(0, exit);
        Assert.True(File.Exists(outputPath), "Expected manifest file to be written");

        var content = File.ReadAllText(outputPath);
        Assert.Contains("""<file name="TestComponent.dll">""", content);
        Assert.Contains("TestNs.Widget", content);
        Assert.Contains("xmlns=\"urn:schemas-microsoft-com:winrt.v1\"", content);
        Assert.Contains("threadingModel=\"both\"", content);
    }

    // -----------------------------------------------------------------------
    // 5. AppX output — full pipeline
    // -----------------------------------------------------------------------
    [Fact]
    public void AppxOutput_FullPipeline()
    {
        var winmd = CreateWinMd("""
            namespace TestNs
            {
                runtimeclass Widget
                {
                    Widget();
                    String Name { get; };
                }
            }
            """);

        var outputPath = Path.Combine(_tempDir, "app.appxmanifest");
        var (exit, stdout, stderr) = Run("--manifest", outputPath, "--server_dll", "TestComponent.dll", "/nologo", "/no_def_idir", winmd);

        Assert.Equal(0, exit);
        Assert.True(File.Exists(outputPath), "Expected AppX manifest file to be written");

        var content = File.ReadAllText(outputPath);
        Assert.Contains("<Extensions>", content);
        Assert.Contains("""<Extension Category="windows.activatableClass.inProcessServer">""", content);
        Assert.Contains("<Path>TestComponent.dll</Path>", content);
        Assert.Contains("ActivatableClassId=\"TestNs.Widget\"", content);
    }

    // -----------------------------------------------------------------------
    // 6. Help text includes --manifest and --server_dll
    // -----------------------------------------------------------------------
    [Fact]
    public void HelpText_IncludesManifestAndServerDll()
    {
        var (exit, stdout, _) = Run("/help");

        Assert.Equal(0, exit);
        Assert.Contains("--manifest", stdout);
        Assert.Contains("--server_dll", stdout);
    }

    // -----------------------------------------------------------------------
    // 7. Static-only classes excluded in CLI pipeline
    // -----------------------------------------------------------------------
    [Fact]
    public void CliPipeline_StaticClassesExcluded()
    {
        var winmd = CreateWinMd("""
            namespace TestNs
            {
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                }

                static runtimeclass Utils
                {
                    static Int32 Add(Int32 a, Int32 b);
                }
            }
            """);

        var outputPath = Path.Combine(_tempDir, "app.manifest");
        var (exit, _, _) = Run("--manifest", outputPath, "--server_dll", "Test.dll", "/nologo", "/no_def_idir", winmd);

        Assert.Equal(0, exit);
        var content = File.ReadAllText(outputPath);
        Assert.Contains("TestNs.Widget", content);
        Assert.DoesNotContain("TestNs.Utils", content);
    }

    // -----------------------------------------------------------------------
    // 8. Output message includes file path
    // -----------------------------------------------------------------------
    [Fact]
    public void OutputMessage_IncludesFilePath()
    {
        var winmd = CreateWinMd("""
            namespace TestNs
            {
                runtimeclass Widget { Widget(); void DoWork(); }
            }
            """);

        var outputPath = Path.Combine(_tempDir, "out.manifest");
        var (exit, stdout, _) = Run("--manifest", outputPath, "--server_dll", "Test.dll", "/nologo", "/no_def_idir", winmd);

        Assert.Equal(0, exit);
        Assert.Contains(outputPath, stdout);
    }

    // -----------------------------------------------------------------------
    // 9. Nonexistent winmd file → error
    // -----------------------------------------------------------------------
    [Fact]
    public void NonexistentWinmd_Error()
    {
        var outputPath = Path.Combine(_tempDir, "out.manifest");
        var fakePath = Path.Combine(_tempDir, "nonexistent.winmd");

        var (exit, _, stderr) = Run("--manifest", outputPath, "--server_dll", "foo.dll", "/nologo", "/no_def_idir", fakePath);

        Assert.NotEqual(0, exit);
        Assert.Contains("cannot open", stderr, StringComparison.OrdinalIgnoreCase);
    }
}
