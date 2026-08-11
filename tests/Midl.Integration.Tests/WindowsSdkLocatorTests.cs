// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Cli;
using System.Runtime.Versioning;

namespace Midl.Integration.Tests;

/// <summary>
/// Picking a Windows SDK build from an install layout.
///
/// Two traps, both of which the product hit:
///
/// <c>UnionMetadata</c> contains a <c>Facade</c> directory whose <c>Windows.winmd</c> is a
/// type-forwarding stub rather than real union metadata. "Facade" sorts <em>above</em> the
/// version numbers, so an ordinary newest-first pick chooses it and every type lookup then
/// fails to find anything.
///
/// Versions compared as strings order 10.0.9 above 10.0.10, so the newest build is not
/// necessarily the one chosen.
///
/// These run against a synthetic layout so they do not depend on what is installed.
/// </summary>
[SupportedOSPlatform("windows")]
public class WindowsSdkLocatorTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "midl-sdk-" + Guid.NewGuid().ToString("N")[..8]);

    public WindowsSdkLocatorTests() => Directory.CreateDirectory(_root);

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); } catch { }
    }

    private void AddUnionMetadata(string versionDir)
    {
        var dir = Path.Combine(_root, "UnionMetadata", versionDir);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "Windows.winmd"), "not a real winmd");
    }

    [Fact]
    public void TheFacadeStubIsNeverChosen()
    {
        AddUnionMetadata("Facade");
        AddUnionMetadata("10.0.26100.0");

        var found = WindowsSdkLocator.FindNewestUnionMetadata(_root);

        Assert.NotNull(found);
        Assert.DoesNotContain("Facade", found!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("10.0.26100.0", found!);
    }

    [Fact]
    public void FacadeAloneYieldsNothingRatherThanAStub()
    {
        AddUnionMetadata("Facade");

        Assert.Null(WindowsSdkLocator.FindNewestUnionMetadata(_root));
    }

    [Fact]
    public void VersionsAreComparedAsVersionsNotStrings()
    {
        AddUnionMetadata("10.0.9.0");
        AddUnionMetadata("10.0.10.0");

        var found = WindowsSdkLocator.FindNewestUnionMetadata(_root);

        Assert.NotNull(found);
        Assert.Contains("10.0.10.0", found!);
    }

    [Fact]
    public void AnExactVersionIsPreferredWhenItExists()
    {
        AddUnionMetadata("10.0.22621.0");
        AddUnionMetadata("10.0.26100.0");

        var found = WindowsSdkLocator.GetWindowsWinMdPath(_root, "10.0.22621.0");

        Assert.NotNull(found);
        Assert.Contains("10.0.22621.0", found!);
    }

    [Fact]
    public void AMissingExactVersionFallsBackToTheNewest()
    {
        AddUnionMetadata("Facade");
        AddUnionMetadata("10.0.22621.0");
        AddUnionMetadata("10.0.26100.0");

        var found = WindowsSdkLocator.GetWindowsWinMdPath(_root, "10.0.99999.0");

        Assert.NotNull(found);
        Assert.Contains("10.0.26100.0", found!);
        Assert.DoesNotContain("Facade", found!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NoUnionMetadataAtAllYieldsNull()
    {
        Assert.Null(WindowsSdkLocator.FindNewestUnionMetadata(_root));
    }
}
