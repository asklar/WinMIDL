// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Lsp;

namespace Midl.Analysis.Tests;

/// <summary>
/// Coverage for the predicate behind <c>midl.reportSystemImportDiagnostics</c>.
///
/// Importing a classic system IDL (wtypes.idl, oaidl.idl, objidl.idl, ...) yields
/// thousands of strict-mode errors about the SDK's own headers, so those are hidden
/// by default. The failure mode of a bug here is that the user's <em>own</em>
/// diagnostics get silently suppressed — no error, no squiggle, no clue. That is
/// worse than showing noise, so every case below pins the conservative direction:
/// when in doubt, report.
/// </summary>
public class SystemImportFilterTests
{
    private static readonly string[] SdkPaths =
    [
        @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.26100.0\um",
        @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.26100.0\shared",
    ];

    private static bool IsSystem(string path) =>
        MidlLanguageServer.IsUnderSystemIncludePath(path, SdkPaths);

    [Theory]
    [InlineData(@"C:\Program Files (x86)\Windows Kits\10\Include\10.0.26100.0\um\oaidl.idl")]
    [InlineData(@"C:\Program Files (x86)\Windows Kits\10\Include\10.0.26100.0\shared\wtypes.idl")]
    public void FilesDirectlyUnderAnSdkPath_AreSystem(string path)
        => Assert.True(IsSystem(path));

    [Fact]
    public void FilesInASubdirectoryOfAnSdkPath_AreSystem()
        => Assert.True(IsSystem(
            @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.26100.0\um\nested\deep.idl"));

    [Fact]
    public void CasingAndSeparatorStyle_DoNotMatter()
    {
        Assert.True(IsSystem(
            @"c:\program files (x86)\windows kits\10\include\10.0.26100.0\UM\OAIDL.IDL"));
        Assert.True(IsSystem(
            "C:/Program Files (x86)/Windows Kits/10/Include/10.0.26100.0/um/oaidl.idl"));
    }

    [Fact]
    public void UnnormalizedPaths_AreResolvedBeforeComparing()
        => Assert.True(IsSystem(
            @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.26100.0\shared\..\um\oaidl.idl"));

    [Fact]
    public void TheUsersOwnFile_IsNotSystem()
        => Assert.False(IsSystem(@"E:\project\interfaces\myThing.idl"));

    /// <summary>
    /// A prefix match without a directory boundary is a different directory.
    /// <c>...\10.0.26100.0\umbrella</c> merely starts with <c>...\10.0.26100.0\um</c>.
    /// A naive StartsWith would suppress the whole tree.
    /// </summary>
    [Fact]
    public void ASiblingDirectoryWithAnSdkPathAsItsNamePrefix_IsNotSystem()
        => Assert.False(IsSystem(
            @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.26100.0\umbrella\mine.idl"));

    /// <summary>
    /// Same trap one level up: a user directory named <c>...\Include-backup</c> sitting
    /// next to the SDK's <c>Include</c>.
    /// </summary>
    [Fact]
    public void ADirectoryWhoseNameExtendsAnSdkParent_IsNotSystem()
        => Assert.False(IsSystem(
            @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.26100.0\um-backup\mine.idl"));

    [Fact]
    public void NoSdkPathsConfigured_MeansNothingIsSystem()
        => Assert.False(MidlLanguageServer.IsUnderSystemIncludePath(
            @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.26100.0\um\oaidl.idl", []));

    /// <summary>
    /// An unparseable path must not throw and must not be suppressed — reporting a
    /// diagnostic against a strange path is recoverable, silently dropping it is not.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\0invalid")]
    public void MalformedPaths_AreReportedNotSuppressed(string path)
        => Assert.False(IsSystem(path));

    /// <summary>
    /// A configured SDK path that cannot be resolved is skipped rather than aborting the
    /// scan, so the remaining (valid) SDK paths still filter.
    /// </summary>
    [Fact]
    public void AnInvalidSdkPath_DoesNotPreventMatchingTheValidOnes()
    {
        string[] paths = ["\0bogus", @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.26100.0\um"];
        Assert.True(MidlLanguageServer.IsUnderSystemIncludePath(
            @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.26100.0\um\oaidl.idl", paths));
    }

    /// <summary>
    /// A trailing separator on the configured path must not change the answer.
    /// </summary>
    [Fact]
    public void TrailingSeparatorOnTheSdkPath_IsEquivalent()
    {
        string[] withSlash = [@"C:\Program Files (x86)\Windows Kits\10\Include\10.0.26100.0\um\"];
        Assert.True(MidlLanguageServer.IsUnderSystemIncludePath(
            @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.26100.0\um\oaidl.idl", withSlash));
        Assert.False(MidlLanguageServer.IsUnderSystemIncludePath(
            @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.26100.0\umbrella\mine.idl", withSlash));
    }
}
