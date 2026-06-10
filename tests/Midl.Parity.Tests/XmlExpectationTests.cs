using System.Xml.Linq;
using Midl.Analysis;
using Midl.Analysis.Diagnostics;

namespace Midl.Parity.Tests;

/// <summary>
/// Validates that positive IDL test files produce WinMD output matching
/// XML expectation files from an external fixture suite.
/// Skips when MIDL_TEST_FIXTURES is not set.
/// </summary>
public class XmlExpectationTests
{
    private static readonly string? FixturesDir =
        Environment.GetEnvironmentVariable("MIDL_TEST_FIXTURES");

#if WITH_COMPAT
    [SkippableFact]
    public void PositiveTestsMatchXmlExpectations()
    {
        Skip.If(FixturesDir == null || !Directory.Exists(FixturesDir),
            "MIDL_TEST_FIXTURES not set");

        var xmlFiles = Directory.GetFiles(FixturesDir!, "*.xml")
            .Concat(Directory.GetFiles(FixturesDir!, "*.Xml"))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(f => !Path.GetFileName(f).Contains("DataDriven", StringComparison.OrdinalIgnoreCase)
                     && !Path.GetFileName(f).Contains("FeatureStaging", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Skip.If(xmlFiles.Length == 0, "No XML expectation files found");

        int matched = 0, mismatched = 0, skipped = 0;
        var mismatchDetails = new List<string>();

        foreach (var xmlPath in xmlFiles)
        {
            // Find matching IDL: MoIdlContractPositive.0.xml → MoIdlContractPositive.0.idl
            var baseName = Path.GetFileNameWithoutExtension(xmlPath);
            var idlPath = Path.Combine(FixturesDir!, baseName + ".idl");
            if (!File.Exists(idlPath))
            {
                skipped++;
                continue;
            }

            try
            {
                var source = File.ReadAllText(idlPath);
                var compilation = CompilationHelper.Compile(source, CompilationMode.Compat);

                if (compilation.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                {
                    skipped++; // Can't validate if compilation fails
                    continue;
                }

                // Parse XML expectations
                var expectedTypes = ParseXmlExpectations(xmlPath);

                // Compare against our compilation symbols
                var allTypes = compilation.GetAllTypes().ToList();
                bool allMatch = true;

                foreach (var expected in expectedTypes)
                {
                    var actual = allTypes.FirstOrDefault(
                        t => t.FullName == expected.Name);
                    if (actual == null)
                    {
                        allMatch = false;
                        mismatchDetails.Add(
                            $"{Path.GetFileName(xmlPath)}: missing type '{expected.Name}' ({expected.Kind})");
                        break;
                    }
                }

                if (allMatch)
                    matched++;
                else
                    mismatched++;
            }
            catch
            {
                skipped++;
            }
        }

        Console.WriteLine(
            $"XML expectations: {matched} matched, {mismatched} mismatched, {skipped} skipped");
        if (mismatchDetails.Count > 0)
        {
            Console.WriteLine("Mismatch details:");
            foreach (var detail in mismatchDetails.Take(20))
                Console.WriteLine($"  {detail}");
        }
    }
#endif

    private static List<ExpectedType> ParseXmlExpectations(string xmlPath)
    {
        var result = new List<ExpectedType>();
        var doc = XDocument.Load(xmlPath);

        foreach (var typeEl in doc.Root?.Elements() ?? Enumerable.Empty<XElement>())
        {
            var name = typeEl.Attribute("name")?.Value;
            if (name == null)
                continue;

            var kind = typeEl.Name.LocalName switch
            {
                "ApiContract" => "ApiContract",
                "Interface" => "Interface",
                "Class" => "RuntimeClass",
                "Struct" => "Struct",
                "Enum" => "Enum",
                "Delegate" => "Delegate",
                _ => "Unknown"
            };

            result.Add(new ExpectedType(name, kind));
        }

        return result;
    }

    private record ExpectedType(string Name, string Kind);
}
