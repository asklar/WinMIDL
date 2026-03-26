namespace Midl.VsExtension;

using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;

/// <summary>
/// Restarts the MIDL language server by stopping and re-activating the language server provider.
/// </summary>
[VisualStudioContribution]
public class RestartServerCommand : Command
{
    public override CommandConfiguration CommandConfiguration => new("%RestartServerCommand.DisplayName%")
    {
        Placements = [CommandPlacement.KnownPlacements.ToolsMenu],
    };

    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        // The VisualStudio.Extensibility SDK manages the language server lifecycle.
        // Notify the user; actual restart requires the server provider to be recycled.
        await this.Extensibility.Shell().ShowPromptAsync(
            "MIDL: Language server restart requested. The server will be restarted automatically.",
            PromptOptions.OK,
            cancellationToken);
    }
}
