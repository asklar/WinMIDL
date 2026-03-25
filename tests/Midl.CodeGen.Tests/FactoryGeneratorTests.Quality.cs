using Midl.Analysis;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Quality sweep tests for FactoryGenerator:
/// Issue 3: Consolidated includes
/// </summary>
public partial class FactoryGeneratorTests
{
    // ─── Issue 3: Consolidated includes ───

    [Fact]
    public void Factory_IncludesAppearOnlyOnce()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        var lines = output.Split('\n');
        var includeLines = lines.Where(l => l.TrimStart().StartsWith("#include")).ToList();
        var uniqueIncludes = includeLines.Select(l => l.Trim()).Distinct().ToList();
        Assert.Equal(uniqueIncludes.Count, includeLines.Count);
    }

    [Fact]
    public void Factory_UsesConsolidatedIncludes()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        Assert.Contains("#include <windows.h>", output);
        Assert.Contains("#include <objidl.h>", output);
    }
}
