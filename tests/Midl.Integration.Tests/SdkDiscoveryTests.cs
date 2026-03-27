using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;
using Midl.Cli;
using Midl.Lsp.Handlers;
using System.Runtime.Versioning;

namespace Midl.Integration.Tests;

[SupportedOSPlatform("windows")]
public class SdkDiscoveryTests
{
    private static bool IsSdkInstalled()
    {
        return WindowsSdkLocator.FindSdkRoot() != null;
    }

    [SkippableFact]
    public void FindsSdkRoot()
    {
        var root = WindowsSdkLocator.FindSdkRoot();
        Skip.If(root == null, "Windows SDK not installed");

        Assert.True(Directory.Exists(root), $"SDK root should exist: {root}");
    }

    [SkippableFact]
    public void FindsLatestVersion()
    {
        var root = WindowsSdkLocator.FindSdkRoot();
        Skip.If(root == null, "Windows SDK not installed");

        var version = WindowsSdkLocator.FindLatestVersion(root);
        Assert.NotNull(version);
        Assert.Matches(@"^10\.0\.\d+\.\d+$", version);
    }

    [SkippableFact]
    public void IncludePathsExist()
    {
        var sdk = WindowsSdkLocator.Discover();
        Skip.If(sdk == null, "Windows SDK not installed");

        Assert.NotEmpty(sdk.IncludePaths);
        foreach (var path in sdk.IncludePaths)
        {
            Assert.True(Directory.Exists(path), $"Include path should exist: {path}");
        }
    }

    [SkippableFact]
    public void WindowsWinMdExists()
    {
        var sdk = WindowsSdkLocator.Discover();
        Skip.If(sdk == null, "Windows SDK not installed");
        Skip.If(sdk.WindowsWinMdPath == null, "Windows.winmd not found in SDK");

        Assert.True(File.Exists(sdk.WindowsWinMdPath), $"Windows.winmd should exist: {sdk.WindowsWinMdPath}");
    }

    [SkippableFact]
    public void CompileWithSdkReferences()
    {
        var sdk = WindowsSdkLocator.Discover();
        Skip.If(sdk == null, "Windows SDK not installed");
        Skip.If(sdk.WindowsWinMdPath == null, "Windows.winmd not found in SDK");

        var source = @"
            namespace N
            {
                runtimeclass C : Windows.Foundation.IStringable
                {
                    C();
                };
            }";

        var compilation = CompilationHelper.Compile(
            source,
            new[] { sdk.WindowsWinMdPath },
            CompilationMode.Strict);

        var errors = compilation.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        Assert.Empty(errors);
    }

    [Fact]
    public void DiscoverReturnsNullGracefully()
    {
        // Discover should never throw, even if SDK is not installed
        var sdk = WindowsSdkLocator.Discover();
        // Result is either valid SdkInfo or null — no exception
        if (sdk != null)
        {
            Assert.NotEmpty(sdk.Root);
            Assert.NotEmpty(sdk.Version);
        }
    }

    [Fact]
    public void NoDefIdirSwitchParsed()
    {
        var options = CommandLineParser.Parse(new[] { "/no_def_idir", "test.idl" });
        Assert.True(options.NoDefIdir);
    }

    [Fact]
    public void NoDefIdirNotSetByDefault()
    {
        var options = CommandLineParser.Parse(new[] { "test.idl" });
        Assert.False(options.NoDefIdir);
    }

    [Fact]
    public void FindLatestVersion_NonExistentDir_ReturnsNull()
    {
        var version = WindowsSdkLocator.FindLatestVersion(@"C:\NonExistentSdkPath");
        Assert.Null(version);
    }

    [Fact]
    public void GetIncludePaths_NonExistentDir_ReturnsEmpty()
    {
        var paths = WindowsSdkLocator.GetIncludePaths(@"C:\NonExistentSdkPath", "10.0.00000.0");
        Assert.Empty(paths);
    }

    [Fact]
    public void GetWindowsWinMdPath_NonExistentDir_ReturnsNull()
    {
        var path = WindowsSdkLocator.GetWindowsWinMdPath(@"C:\NonExistentSdkPath", "10.0.00000.0");
        Assert.Null(path);
    }

    [Fact]
    public void GetContractReferences_NonExistentDir_ReturnsEmpty()
    {
        var refs = WindowsSdkLocator.GetContractReferences(@"C:\NonExistentSdkPath", "10.0.00000.0");
        Assert.Empty(refs);
    }

    [SkippableFact]
    public void NoexceptAttributeResolvedFromSdk()
    {
        var sdk = WindowsSdkLocator.Discover();
        Skip.If(sdk == null, "Windows SDK not installed");
        Skip.If(sdk.WindowsWinMdPath == null, "Windows.winmd not found in SDK");

        var source = @"
            namespace N
            {
                runtimeclass C
                {
                    [noexcept] void Fast();
                }
            }";

        var compilation = CompilationHelper.Compile(
            source,
            new[] { sdk.WindowsWinMdPath },
            CompilationMode.Strict);

        // [noexcept] should NOT produce MIDL1065 (unknown attribute)
        var unknownAttrWarnings = compilation.Diagnostics
            .Where(d => d.Code == "MIDL1065")
            .ToList();

        Assert.Empty(unknownAttrWarnings);
    }

    [SkippableFact]
    public void SdkAttributeWithOverrideName_ResolvedByShortName()
    {
        var sdk = WindowsSdkLocator.Discover();
        Skip.If(sdk == null, "Windows SDK not installed");
        Skip.If(sdk.WindowsWinMdPath == null, "Windows.winmd not found in SDK");

        // Verify NoExceptionAttribute is in the lazy provider
        var provider = new LazyWinMdProvider(sdk.WindowsWinMdPath);
        var allNames = provider.GetAllTypeNames().ToList();
        Assert.Contains("Windows.Foundation.Metadata.NoExceptionAttribute", allNames);

        // Verify it resolves as an AttributeTypeSymbol with the right override name
        var compilation = CompilationHelper.Compile("namespace N { }",
            new[] { sdk.WindowsWinMdPath }, CompilationMode.Strict);
        var resolved = compilation.GetTypeByName("Windows.Foundation.Metadata.NoExceptionAttribute");
        Assert.NotNull(resolved);
        var attrType = Assert.IsType<Midl.Analysis.Symbols.AttributeTypeSymbol>(resolved);
        Assert.Equal("noexcept", attrType.AttributeNameOverride);
    }

    [SkippableFact]
    public void SdkAttributeCompletions_OnlyShortNames_NoClassNames()
    {
        var sdk = WindowsSdkLocator.Discover();
        Skip.If(sdk == null, "Windows SDK not installed");
        Skip.If(sdk.WindowsWinMdPath == null, "Windows.winmd not found in SDK");

        var idl = "namespace N { runtimeclass C { } }";
        var compilation = CompilationHelper.Compile(idl,
            new[] { sdk.WindowsWinMdPath }, CompilationMode.Strict);

        var items = AttributeCompletionProvider.BuildAttributeCompletions(
            idl, "target_all", cachedCompilation: compilation);
        var labels = items.Cast<Dictionary<string, object?>>()
            .Select(d => (string)d["label"]!).ToList();

        // Short names should be present
        Assert.Contains("activatable", labels);
        Assert.Contains("deprecated", labels);
        Assert.Contains("noexcept", labels);

        // No label should end with "Attribute" — all should use short names
        var classNames = labels.Where(l => l.EndsWith("Attribute", StringComparison.Ordinal)).ToList();
        Assert.True(classNames.Count == 0,
            $"Completions should use short names, not class names. Found: {string.Join(", ", classNames)}");
    }
}
