namespace Midl.VsExtension;

using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;

/// <summary>
/// Shows the MIDL language server output/trace in the Output window.
/// </summary>
[VisualStudioContribution]
public class ShowServerLogCommand : Command
{
    public override CommandConfiguration CommandConfiguration => new("%ShowServerLogCommand.DisplayName%")
    {
        Placements = [CommandPlacement.KnownPlacements.ToolsMenu],
    };

    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        await this.Extensibility.Shell().ShowPromptAsync(
            "MIDL: Check the Output window (View → Output) and select 'MIDL Language Server' from the dropdown.",
            PromptOptions.OK,
            cancellationToken);
    }
}
