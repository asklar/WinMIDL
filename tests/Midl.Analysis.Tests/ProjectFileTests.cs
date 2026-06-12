// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;

namespace Midl.Analysis.Tests;

public class ProjectFileTests
{
    [Fact]
    public void ParseProjectFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"winmidl_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var json = """
                {
                    "inputs": ["Widget.idl", "Helper.idl"],
                    "references": ["lib/MyLib.winmd"],
                    "defines": ["FEATURE_X"],
                    "includePaths": ["include"],
                    "noSdk": true
                }
                """;
            var projectPath = Path.Combine(tempDir, "winmidl.json");
            File.WriteAllText(projectPath, json);

            var project = ProjectFile.Load(projectPath);

            Assert.Equal(2, project.Inputs.Count);
            Assert.Contains("Widget.idl", project.Inputs);
            Assert.Contains("Helper.idl", project.Inputs);
            Assert.Single(project.References);
            Assert.Single(project.Defines);
            Assert.Equal("FEATURE_X", project.Defines[0]);
            Assert.True(project.NoSdk);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void DiscoverProjectFile_WalksUp()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"winmidl_test_{Guid.NewGuid():N}");
        var subDir = Path.Combine(tempDir, "src");
        Directory.CreateDirectory(subDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "winmidl.json"),
                """{ "inputs": ["src/Widget.idl"] }""");
            File.WriteAllText(Path.Combine(subDir, "Widget.idl"), "namespace N { }");

            var project = ProjectFile.Discover(Path.Combine(subDir, "Widget.idl"));

            Assert.NotNull(project);
            Assert.Single(project!.Inputs);
            Assert.Equal("src/Widget.idl", project.Inputs[0]);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void GetSiblingInputs_ExcludesSelf()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"winmidl_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "Widget.idl"), "namespace W { }");
            File.WriteAllText(Path.Combine(tempDir, "Helper.idl"), "namespace H { }");
            File.WriteAllText(Path.Combine(tempDir, "winmidl.json"),
                """{ "inputs": ["Widget.idl", "Helper.idl"] }""");

            var project = ProjectFile.Load(Path.Combine(tempDir, "winmidl.json"));
            var siblings = project.GetSiblingInputs(Path.Combine(tempDir, "Widget.idl"));

            Assert.Single(siblings);
            Assert.Contains("Helper.idl", siblings[0]);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ParseRefDirectives()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"winmidl_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "Helper.idl"), "namespace H { }");

            var source = """
                // @ref: Helper.idl
                // Some other comment
                namespace N { }
                """;

            var refs = RefDirectiveParser.ParseRefs(source, tempDir);
            Assert.Single(refs);
            Assert.Contains("Helper.idl", refs[0]);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ParseRefDirectives_RelativePaths()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"winmidl_test_{Guid.NewGuid():N}");
        var srcDir = Path.Combine(tempDir, "src");
        var sharedDir = Path.Combine(tempDir, "shared");
        Directory.CreateDirectory(srcDir);
        Directory.CreateDirectory(sharedDir);
        try
        {
            File.WriteAllBytes(Path.Combine(sharedDir, "Lib.winmd"), new byte[] { 0 });
            File.WriteAllText(Path.Combine(sharedDir, "Types.idl"), "namespace T { }");

            var source = "// @ref: ../shared/Lib.winmd\n// @ref: ../shared/Types.idl\nnamespace N { }\n";

            // baseDir is srcDir (where the IDL file lives)
            var refs = RefDirectiveParser.ParseRefs(source, srcDir);
            Assert.Equal(2, refs.Count);
            Assert.Contains(refs, r => r.EndsWith("Lib.winmd"));
            Assert.Contains(refs, r => r.EndsWith("Types.idl"));
            // Paths should be fully resolved (no ../)
            Assert.DoesNotContain("..", refs[0]);
            Assert.DoesNotContain("..", refs[1]);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ParseRefDirectives_WinmdFiles()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"winmidl_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            // Create a dummy .winmd file
            File.WriteAllBytes(Path.Combine(tempDir, "ExtLib.winmd"), new byte[] { 0 });
            File.WriteAllText(Path.Combine(tempDir, "Helper.idl"), "namespace H { }");

            var source = """
                // @ref: ExtLib.winmd
                // @ref: Helper.idl
                namespace N { }
                """;

            var refs = RefDirectiveParser.ParseRefs(source, tempDir);
            Assert.Equal(2, refs.Count);
            Assert.Contains(refs, r => r.EndsWith("ExtLib.winmd"));
            Assert.Contains(refs, r => r.EndsWith("Helper.idl"));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ParseRefDirectives_QuotedPaths()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"winmidl_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllBytes(Path.Combine(tempDir, "My Lib.winmd"), new byte[] { 0 });

            var source = """
                // @ref: "My Lib.winmd"
                namespace N { }
                """;

            var refs = RefDirectiveParser.ParseRefs(source, tempDir);
            Assert.Single(refs);
            Assert.Contains("My Lib.winmd", refs[0]);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ParseRefDirectives_DirectoryPath()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"winmidl_test_{Guid.NewGuid():N}");
        var refDir = Path.Combine(tempDir, "refs");
        Directory.CreateDirectory(refDir);
        try
        {
            var source = "// @ref: refs\nnamespace N { }\n";

            var refs = RefDirectiveParser.ParseRefs(source, tempDir);
            Assert.Single(refs);
            Assert.True(Directory.Exists(refs[0]));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ResolveWinMdsFromDirectory_FindsAll()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"winmidl_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllBytes(Path.Combine(tempDir, "Lib.winmd"), new byte[] { 0 });
            File.WriteAllBytes(Path.Combine(tempDir, "Other.winmd"), new byte[] { 0 });

            var results = RefDirectiveParser.ResolveWinMdsFromDirectory(tempDir);
            Assert.Equal(2, results.Count);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ProjectFile_CommentsInJson()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"winmidl_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var json = """
                {
                    // This is a comment
                    "inputs": ["Widget.idl"],
                    "noSdk": false
                }
                """;
            File.WriteAllText(Path.Combine(tempDir, "winmidl.json"), json);

            var project = ProjectFile.Load(Path.Combine(tempDir, "winmidl.json"));
            Assert.Single(project.Inputs);
            Assert.False(project.NoSdk);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
