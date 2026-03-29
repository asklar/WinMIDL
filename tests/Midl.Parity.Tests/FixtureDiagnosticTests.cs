using System.Text.Json;
using Midl.Analysis;
using Midl.Analysis.Diagnostics;

namespace Midl.Parity.Tests;

public class FixtureDiagnosticTests
{
    private static string FixturesDir
    {
        get
        {
            var assemblyDir = Path.GetDirectoryName(typeof(FixtureDiagnosticTests).Assembly.Location)!;
            return Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "..", "..", "fixtures"));
        }
    }

    public static IEnumerable<object[]> GetValidFixtures() =>
        Directory.GetFiles(Path.Combine(FixturesDir, "valid"), "*.idl")
            .Select(f => new object[] { Path.GetFileName(f), f });

    public static IEnumerable<object[]> GetErrorFixtures() =>
        Directory.GetFiles(Path.Combine(FixturesDir, "errors"), "*.idl")
            .Select(f => new object[] { Path.GetFileName(f), f });

    [Theory]
    [MemberData(nameof(GetValidFixtures))]
    public void ValidFixture_NoDiagnostics(string name, string path)
    {
        _ = name; // used for test display name
        var source = File.ReadAllText(path);
        var compilation = CompilationHelper.Compile(source);
        var errors = compilation.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.Empty(errors);
    }

    [Theory]
    [MemberData(nameof(GetErrorFixtures))]
    public void ErrorFixture_MatchesExpected(string name, string path)
    {
        var source = File.ReadAllText(path);
        var expectedPath = Path.ChangeExtension(path, ".expected.json");
        Assert.True(File.Exists(expectedPath), $"Missing {expectedPath}");

        var expectedJson = JsonDocument.Parse(File.ReadAllText(expectedPath));
        var compilation = CompilationHelper.Compile(source);

        var expectedDiags = expectedJson.RootElement.GetProperty("diagnostics");
        foreach (var expected in expectedDiags.EnumerateArray())
        {
            var code = expected.GetProperty("code").GetString()!;
            var severity = expected.GetProperty("severity").GetString()!;
            var messageContains = expected.TryGetProperty("messageContains", out var mc)
                ? mc.GetString() : null;
            var tokenText = expected.TryGetProperty("tokenText", out var tt)
                ? tt.GetString() : null;

            var match = compilation.Diagnostics.FirstOrDefault(d =>
                d.Code == code &&
                d.Severity.ToString() == severity &&
                (messageContains == null || d.Message.Contains(messageContains)) &&
                (tokenText == null || (d.Position + d.Length <= source.Length &&
                    source.Substring(d.Position, d.Length) == tokenText)));

            Assert.True(match != null,
                $"Expected diagnostic {code} ({severity}) with message containing '{messageContains}' " +
                $"and token '{tokenText}' not found in [{name}]. " +
                $"Actual diagnostics: [{string.Join(", ", compilation.Diagnostics.Select(d => $"{d.Code}({d.Severity}): {d.Message}"))}]");
        }
    }
}
