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
