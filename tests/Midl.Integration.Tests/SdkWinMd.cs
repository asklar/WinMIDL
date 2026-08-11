// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Cli;

namespace Midl.Integration.Tests;

/// <summary>
/// The Windows SDK metadata, resolved exactly as the product resolves it.
///
/// Tests used to hardcode a single SDK build (<c>10.0.26100.0</c>) and silently
/// <c>return</c> when it was absent — which xUnit records as a pass, so on any machine or
/// CI image with a different build they asserted nothing at all.
///
/// Rather than reimplement discovery here, defer to <see cref="WindowsSdkLocator"/>: the
/// same code the compiler and language server use, honouring <c>MIDL_SDK_VERSION</c> and
/// otherwise taking the newest install. A test that resolved the SDK differently from the
/// product would be testing something the product never does.
///
/// Returns null when no SDK is installed, because a test that needs one genuinely cannot
/// run without it. Prefer writing tests that do not need the SDK at all — emitting a WinMD
/// from IDL in the test is deterministic and runs everywhere.
/// </summary>
internal static class SdkWinMd
{
    private static readonly Lazy<string?> s_path = new(() =>
        OperatingSystem.IsWindows() ? WindowsSdkLocator.Discover()?.WindowsWinMdPath : null);

    /// <summary>The Windows.winmd the product would use, or null if no SDK is installed.</summary>
    public static string? PathOrNull => s_path.Value;
}
