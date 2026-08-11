// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Cli;
using Xunit;

namespace Midl.TestSupport;

/// <summary>
/// The Windows SDK metadata, resolved exactly as the product resolves it.
///
/// Tests used to hardcode a single SDK build and silently <c>return</c> when it was absent.
/// A method that returns without asserting is indistinguishable from one that verified
/// everything, so xUnit reports it as <em>passed</em> — 16 such tests could report success
/// on any machine or CI image with a different SDK build while checking nothing at all.
///
/// Discovery is not reimplemented here. <see cref="WindowsSdkLocator"/> is the same code the
/// compiler and language server use, honouring <c>MIDL_SDK_VERSION</c> and otherwise taking
/// the newest install; a test that resolved the SDK differently would be exercising
/// something the product never does.
/// </summary>
public static class SdkWinMd
{
    /// <summary>
    /// Set to 1 to make discovery report no SDK. Exists so the skip path itself can be
    /// tested — otherwise it can only be exercised by uninstalling the SDK.
    /// </summary>
    public const string ForceMissingVariable = "MIDL_TEST_NO_SDK";

    private static readonly Lazy<string?> s_path = new(Resolve);

    /// <summary>The Windows.winmd the product would use, or null if no SDK is installed.</summary>
    public static string? PathOrNull => s_path.Value;

    /// <summary>
    /// The Windows.winmd, throwing if absent. For use inside a <see cref="WindowsSdkFactAttribute"/>
    /// test, where the attribute has already established that one exists.
    /// </summary>
    public static string Path => PathOrNull
        ?? throw new InvalidOperationException(
            "No Windows SDK found. Tests needing one should use [WindowsSdkFact].");

    /// <summary>Why the SDK is unavailable, or null when it is available.</summary>
    public static string? UnavailableReason =>
        PathOrNull is null
            ? "Requires an installed Windows SDK (UnionMetadata\\Windows.winmd)."
            : null;

    private static string? Resolve()
    {
        if (Environment.GetEnvironmentVariable(ForceMissingVariable) == "1") return null;
        if (!OperatingSystem.IsWindows()) return null;
        return WindowsSdkLocator.Discover()?.WindowsWinMdPath;
    }
}

/// <summary>
/// A <see cref="FactAttribute"/> for tests that genuinely need an installed Windows SDK.
///
/// When none is present the test is reported as <em>skipped</em>, which is visible, rather
/// than returning early and being reported as passed, which is not. The difference matters:
/// a green run should mean the assertions ran.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class WindowsSdkFactAttribute : FactAttribute
{
    public WindowsSdkFactAttribute()
    {
        var reason = SdkWinMd.UnavailableReason;
        if (reason != null) Skip = reason;
    }
}
