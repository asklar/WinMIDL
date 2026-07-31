// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.Analysis.Diagnostics;

namespace Midl.Integration.Tests;

/// <summary>
/// Integration tests for array parameter support end-to-end.
/// </summary>
public class ArrayIntegrationTests
{
    private static Compilation CompileWithArrays(string source) =>
        CompilationHelper.Compile(source, referencePaths: null,
            mode: CompilationMode.Strict, includePaths: null, currentFilePath: null,
            arraysEnabled: true);

    [Fact]
    public void ArraysEnabled_NoDiagnostics()
    {
        var compilation = CompileWithArrays("""
            namespace TestApp
            {
                interface IDataService
                {
                    void UploadBytes(UInt8[] data);
                    UInt8[] DownloadBytes();
                    void GetResults(out Int32[] results);
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ArraysAlwaysEnabled_NoDiagnostic()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                interface IWidget
                {
                    void SetItems(Int32[] items);
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1108");
    }

    [Fact]
    public void ArraysEnabled_FullPipeline()
    {
        var compilation = CompileWithArrays("""
            namespace TestApp
            {
                struct Point { Int32 X; Int32 Y; };
                enum Color { Red, Green, Blue };
                interface ICanvas
                {
                    void DrawPoints(Point[] points);
                    void SetColors(Color[] colors);
                    String[] GetLabels();
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Severity == DiagnosticSeverity.Error);

        // Verify we can generate a header
        var emitter = new Midl.CodeGen.Header.HeaderEmitter();
        var header = emitter.Emit(compilation);
        Assert.Contains("UINT32 __pointsSize", header);
        Assert.Contains("Point* points", header);
        Assert.Contains("UINT32 __colorsSize", header);
        Assert.Contains("HSTRING** __returnItems", header);
    }

    [Fact]
    public void ProjectFile_ArraysSetting()
    {
        var json = """{"inputs": ["test.idl"], "arrays": true}""";
        var tempDir = Path.Combine(Path.GetTempPath(), "midl_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var projectPath = Path.Combine(tempDir, "winmidl.json");
            File.WriteAllText(projectPath, json);
            var project = ProjectFile.Load(projectPath);
            Assert.True(project.Arrays);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ProjectFile_ArraysNotSet_DefaultsFalse()
    {
        var json = """{"inputs": ["test.idl"]}""";
        var tempDir = Path.Combine(Path.GetTempPath(), "midl_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var projectPath = Path.Combine(tempDir, "winmidl.json");
            File.WriteAllText(projectPath, json);
            var project = ProjectFile.Load(projectPath);
            Assert.False(project.Arrays);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
