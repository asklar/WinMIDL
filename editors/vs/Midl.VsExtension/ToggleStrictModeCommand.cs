namespace Midl.VsExtension;

using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;

#pragma warning disable VSEXTPREVIEW_SETTINGS

/// <summary>
/// Toggles the midl.strictMode setting between strict MIDL3 and compatibility mode.
/// </summary>
[VisualStudioContribution]
public class ToggleStrictModeCommand : Command
{
    public override CommandConfiguration CommandConfiguration => new("%ToggleStrictModeCommand.DisplayName%")
    {
        Placements = [CommandPlacement.KnownPlacements.ToolsMenu],
    };

    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        var settings = this.Extensibility.Settings();
        var current = await settings.ReadEffectiveValueAsync(MidlSettingDefinitions.StrictMode, cancellationToken);
        bool newValue = !(current.Succeeded && current.Value);

        await settings.WriteAsync(
            batch => batch.WriteSetting(MidlSettingDefinitions.StrictMode, newValue),
            "Toggle MIDL strict mode",
            cancellationToken);

        string mode = newValue ? "Strict MIDL3" : "Compatibility";
        await this.Extensibility.Shell().ShowPromptAsync(
            $"MIDL: Switched to {mode} mode",
            PromptOptions.OK,
            cancellationToken);
    }
}
