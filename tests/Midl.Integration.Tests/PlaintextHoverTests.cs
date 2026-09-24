// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Midl.Lsp;

namespace Midl.Integration.Tests;

public class PlaintextHoverTests
{
    [Fact]
    public void LearnLink_RemainsUsableInPlaintext()
    {
        const string url = "https://learn.microsoft.com/en-us/uwp/api/windows.foundation";
        string args = Uri.EscapeDataString($"[\"{url}\"]");
        string markdown = $"**Windows.Foundation**\n\n[📖 View on Microsoft Learn](command:midl.openDocs?{args})";

        string plain = StripMarkdown(markdown);

        Assert.Contains("Windows.Foundation", plain);
        Assert.Contains(url, plain);
        Assert.DoesNotContain("command:midl.openDocs", plain);
    }

    private static string StripMarkdown(string markdown)
    {
        MethodInfo method = typeof(MidlLanguageServer).GetMethod(
            "StripMarkdown", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (string)method.Invoke(null, [markdown])!;
    }
}
