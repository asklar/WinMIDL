namespace Midl.VsExtension.Tests;

using System.Reflection;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Settings;

#pragma warning disable VSEXTPREVIEW_SETTINGS

/// <summary>
/// Tests that all MIDL settings are properly defined with correct defaults.
/// </summary>
public class SettingsDefaultsTests
{
    [Fact]
    public void ServerPath_DefaultIsMidl()
    {
        Assert.Equal("midl", MidlSettingDefinitions.ServerPath.DefaultValue);
    }

    [Fact]
    public void StrictMode_DefaultIsTrue()
    {
        Assert.True(MidlSettingDefinitions.StrictMode.DefaultValue);
    }

    [Fact]
    public void CodeQualityHints_DefaultIsTrue()
    {
        Assert.True(MidlSettingDefinitions.CodeQualityHints.DefaultValue);
    }

    [Fact]
    public void UseWindowsSdk_DefaultIsTrue()
    {
        Assert.True(MidlSettingDefinitions.UseWindowsSdk.DefaultValue);
    }

    [Fact]
    public void WindowsSdkVersion_DefaultIsLatest()
    {
        Assert.Equal("latest", MidlSettingDefinitions.WindowsSdkVersion.DefaultValue);
    }

    [Fact]
    public void FlatSdkTypeCompletions_DefaultIsFalse()
    {
        Assert.False(MidlSettingDefinitions.FlatSdkTypeCompletions.DefaultValue);
    }

    [Fact]
    public void FetchSdkDocs_DefaultIsTrue()
    {
        Assert.True(MidlSettingDefinitions.FetchSdkDocs.DefaultValue);
    }

    [Fact]
    public void InlineDocsBrowser_DefaultIsTrue()
    {
        Assert.True(MidlSettingDefinitions.InlineDocsBrowser.DefaultValue);
    }

    [Fact]
    public void AllSettings_HaveVisualStudioContributionAttribute()
    {
        var settingsType = typeof(MidlSettingDefinitions);
        var settingProps = settingsType.GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
            .Where(p => p.PropertyType.IsAssignableTo(typeof(Setting)))
            .ToList();

        Assert.Equal(8, settingProps.Count);

        foreach (var prop in settingProps)
        {
            var attr = prop.GetCustomAttribute<VisualStudioContributionAttribute>();
            Assert.NotNull(attr);
        }
    }

    [Fact]
    public void SettingCategory_HasVisualStudioContributionAttribute()
    {
        var categoryProp = typeof(MidlSettingDefinitions).GetProperty(
            "MidlCategory",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

        Assert.NotNull(categoryProp);
        var attr = categoryProp!.GetCustomAttribute<VisualStudioContributionAttribute>();
        Assert.NotNull(attr);
    }
}
