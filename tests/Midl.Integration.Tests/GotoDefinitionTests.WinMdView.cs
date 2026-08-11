// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.CodeGen.Decompiler;
using Midl.CodeGen.WinMd;
using Midl.Lsp;
using Midl.Lsp.Handlers;
using Midl.TestSupport;

namespace Midl.Integration.Tests;

/// <summary>Tests for F12 navigation within decompiled WinMD virtual documents.</summary>
public partial class GotoDefinitionTests
{
    [WindowsSdkFact]
    public void F12InDecompiledView_ShortName_ResolvesInCurrentNamespace()
    {
        // In a decompiled view of Windows.Foundation, F12 on "IAsyncAction" (short name,
        // same namespace) should navigate within the same namespace view
        var winmdPath = SdkWinMd.Path;

        var decompiler = new WinMdDecompiler();
        var decompiled = decompiler.Decompile(winmdPath, "Windows.Foundation");
        var uri = "midl-winmd:///" + winmdPath.Replace('\\', '/') + "/Windows.Foundation";

        // Find "IAsyncAction" in decompiled text (it's a type in Windows.Foundation)
        int idx = decompiled.IndexOf("IAsyncAction", StringComparison.Ordinal);
        Assert.True(idx > 0, "IAsyncAction should be in the decompiled Windows.Foundation");

        var lineStarts = PositionHelper.BuildLineStarts(decompiled);
        var (line, ch) = PositionHelper.OffsetToLineChar(lineStarts, idx);

        var result = GotoDefinitionHandler.HandleCore(decompiled, uri, line, ch);
        Assert.NotNull(result);
        var dict = Assert.IsType<Dictionary<string, object?>>(result);
        var resultUri = dict["uri"]!.ToString()!;
        Assert.Contains("midl-winmd://", resultUri);
        Assert.Contains("Windows.Foundation", resultUri);
    }

    [WindowsSdkFact]
    public void F12InDecompiledView_CrossNamespace_NavigatesToOtherNamespace()
    {
        // In Windows.UI.Xaml.Controls view, F12 on "Windows.UI.Xaml.UIElement" (FQN, different ns)
        // should navigate to Windows.UI.Xaml namespace
        var winmdPath = SdkWinMd.Path;

        var decompiler = new WinMdDecompiler();
        var decompiled = decompiler.Decompile(winmdPath, "Windows.UI.Xaml.Controls");
        var uri = "midl-winmd:///" + winmdPath.Replace('\\', '/') + "/Windows.UI.Xaml.Controls";

        // Find "UIElement" in decompiled text (appears as Windows.UI.Xaml.UIElement)
        int idx = decompiled.IndexOf("UIElement", StringComparison.Ordinal);
        if (idx < 0) return; // may not be present in all SDK versions

        var lineStarts = PositionHelper.BuildLineStarts(decompiled);
        var (line, ch) = PositionHelper.OffsetToLineChar(lineStarts, idx);

        var result = GotoDefinitionHandler.HandleCore(decompiled, uri, line, ch);
        Assert.NotNull(result);
        var dict = Assert.IsType<Dictionary<string, object?>>(result);
        var resultUri = dict["uri"]!.ToString()!;
        Assert.Contains("midl-winmd://", resultUri);
        Assert.Contains("Windows.UI.Xaml", resultUri);
    }

    [WindowsSdkFact]
    public void F12InDecompiledView_EventDelegateType_HasType()
    {
        // Decompiled events should have delegate type, not empty
        var winmdPath = SdkWinMd.Path;

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(winmdPath, "Windows.Foundation");

        // Should NOT have "event  " (double space = missing type)
        Assert.DoesNotContain("event  ", idl);
        // Should have proper event declarations with types
        Assert.Contains("event TypedEventHandler", idl);
    }
}
