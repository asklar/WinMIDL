// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.Analysis.Diagnostics;

namespace Midl.Analysis.Tests;

public class RefDirectiveValidationTests
{
    [Fact]
    public void MissingFileRef_ProducesDiagnostic()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"winmidl_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var source = "// @ref: nonexistent.winmd\nnamespace N { }\n";
            var currentFile = Path.Combine(tempDir, "Test.idl");
            File.WriteAllText(currentFile, source);

            var compilation = CompilationHelper.Compile(source,
                referencePaths: null, CompilationMode.Strict,
                includePaths: null, currentFilePath: currentFile);

            var diag = Assert.Single(compilation.Diagnostics,
                d => d.Code == "MIDL1102");
            Assert.Equal(DiagnosticSeverity.Error, diag.Severity);
            Assert.Contains("nonexistent.winmd", diag.Message);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void MissingDirectoryRef_ProducesDiagnostic()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"winmidl_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var source = "// @ref: missing_folder/\nnamespace N { }\n";
            var currentFile = Path.Combine(tempDir, "Test.idl");
            File.WriteAllText(currentFile, source);

            var compilation = CompilationHelper.Compile(source,
                referencePaths: null, CompilationMode.Strict,
                includePaths: null, currentFilePath: currentFile);

            var diag = Assert.Single(compilation.Diagnostics,
                d => d.Code == "MIDL1102");
            Assert.Equal(DiagnosticSeverity.Error, diag.Severity);
            Assert.Contains("missing_folder/", diag.Message);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void MissingQuotedRef_ProducesDiagnostic()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"winmidl_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var source = "// @ref: \"doesnt exist.idl\"\nnamespace N { }\n";
            var currentFile = Path.Combine(tempDir, "Test.idl");
            File.WriteAllText(currentFile, source);

            var compilation = CompilationHelper.Compile(source,
                referencePaths: null, CompilationMode.Strict,
                includePaths: null, currentFilePath: currentFile);

            var diag = Assert.Single(compilation.Diagnostics,
                d => d.Code == "MIDL1102");
            Assert.Equal(DiagnosticSeverity.Error, diag.Severity);
            Assert.Contains("doesnt exist.idl", diag.Message);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ExistingFileRef_NoDiagnostic()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"winmidl_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "Helper.idl"), "namespace H { }");

            var source = "// @ref: Helper.idl\nnamespace N { }\n";
            var currentFile = Path.Combine(tempDir, "Test.idl");
            File.WriteAllText(currentFile, source);

            var compilation = CompilationHelper.Compile(source,
                referencePaths: null, CompilationMode.Strict,
                includePaths: null, currentFilePath: currentFile);

            Assert.DoesNotContain(compilation.Diagnostics,
                d => d.Code == "MIDL1102");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ExistingDirectoryRef_NoDiagnostic()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"winmidl_test_{Guid.NewGuid():N}");
        var subDir = Path.Combine(tempDir, "existing");
        Directory.CreateDirectory(subDir);
        try
        {
            var source = "// @ref: existing/\nnamespace N { }\n";
            var currentFile = Path.Combine(tempDir, "Test.idl");
            File.WriteAllText(currentFile, source);

            var compilation = CompilationHelper.Compile(source,
                referencePaths: null, CompilationMode.Strict,
                includePaths: null, currentFilePath: currentFile);

            Assert.DoesNotContain(compilation.Diagnostics,
                d => d.Code == "MIDL1102");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void DiagnosticPosition_CoversPathOnly()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"winmidl_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            // "// @ref: nonexistent.winmd"
            //  0123456789...
            // The path "nonexistent.winmd" starts at position 10
            var source = "// @ref: nonexistent.winmd\nnamespace N { }\n";
            var currentFile = Path.Combine(tempDir, "Test.idl");
            File.WriteAllText(currentFile, source);

            var compilation = CompilationHelper.Compile(source,
                referencePaths: null, CompilationMode.Strict,
                includePaths: null, currentFilePath: currentFile);

            var diag = Assert.Single(compilation.Diagnostics,
                d => d.Code == "MIDL1102");

            // Position should be at "nonexistent.winmd", not at "//"
            Assert.Equal(9, diag.Position); // "// @ref: " is 9 chars
            Assert.Equal("nonexistent.winmd".Length, diag.Length);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
