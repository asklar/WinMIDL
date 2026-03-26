namespace Midl.VsExtension;

using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Settings;

#pragma warning disable VSEXTPREVIEW_SETTINGS

internal static class MidlSettingDefinitions
{
    [VisualStudioContribution]
    internal static SettingCategory MidlCategory { get; } = new("midl", "%MidlSettings.Category.DisplayName%")
    {
        Description = "Settings for the MIDL Language Support extension.",
    };

    [VisualStudioContribution]
    internal static Setting.String ServerPath { get; } = new(
        "serverPath", "%MidlSettings.ServerPath.DisplayName%", MidlCategory, defaultValue: "midl")
    {
        Description = "Path to the MIDL CLI executable for the language server.",
    };

    [VisualStudioContribution]
    internal static Setting.Boolean StrictMode { get; } = new(
        "strictMode", "%MidlSettings.StrictMode.DisplayName%", MidlCategory, defaultValue: true)
    {
        Description = "Enable strict MIDL3 mode. When enabled, classic MIDL constructs produce warnings.",
    };

    [VisualStudioContribution]
    internal static Setting.Boolean CodeQualityHints { get; } = new(
        "codeQualityHints", "%MidlSettings.CodeQualityHints.DisplayName%", MidlCategory, defaultValue: true)
    {
        Description = "Show code quality hints (naming conventions, empty types, etc.)",
    };

    [VisualStudioContribution]
    internal static Setting.Boolean UseWindowsSdk { get; } = new(
        "useWindowsSdk", "%MidlSettings.UseWindowsSdk.DisplayName%", MidlCategory, defaultValue: true)
    {
        Description = "Automatically reference Windows.winmd from the installed Windows SDK for type completion and F12 navigation.",
    };

    [VisualStudioContribution]
    internal static Setting.String WindowsSdkVersion { get; } = new(
        "windowsSdkVersion", "%MidlSettings.WindowsSdkVersion.DisplayName%", MidlCategory, defaultValue: "latest")
    {
        Description = "Windows SDK version to use. Use the MIDL: Select Windows SDK Version command to pick from installed versions.",
    };

    [VisualStudioContribution]
    internal static Setting.Boolean FlatSdkTypeCompletions { get; } = new(
        "flatSdkTypeCompletions", "%MidlSettings.FlatSdkTypeCompletions.DisplayName%", MidlCategory, defaultValue: false)
    {
        Description = "Show all SDK types by short name in completions. When off, only SDK namespace names appear and you drill in with dot notation.",
    };

    [VisualStudioContribution]
    internal static Setting.Boolean FetchSdkDocs { get; } = new(
        "fetchSdkDocs", "%MidlSettings.FetchSdkDocs.DisplayName%", MidlCategory, defaultValue: true)
    {
        Description = "Fetch type descriptions from Microsoft Learn on hover. Requires internet access.",
    };

    [VisualStudioContribution]
    internal static Setting.Boolean InlineDocsBrowser { get; } = new(
        "inlineDocsBrowser", "%MidlSettings.InlineDocsBrowser.DisplayName%", MidlCategory, defaultValue: true)
    {
        Description = "Open Microsoft Learn documentation inside Visual Studio instead of the external browser.",
    };
}
