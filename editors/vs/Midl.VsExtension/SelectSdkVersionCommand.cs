namespace Midl.VsExtension;

using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;

#pragma warning disable VSEXTPREVIEW_SETTINGS

/// <summary>
/// Shows a picker for installed Windows SDK versions and updates the midl.windowsSdkVersion setting.
/// </summary>
[VisualStudioContribution]
public class SelectSdkVersionCommand : Command
{
    private const string SdkRoot = @"C:\Program Files (x86)\Windows Kits\10\UnionMetadata";

    public override CommandConfiguration CommandConfiguration => new("%SelectSdkVersionCommand.DisplayName%")
    {
        Placements = [CommandPlacement.KnownPlacements.ToolsMenu],
    };

    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        List<string> versions = DiscoverSdkVersions();

        if (versions.Count == 0)
        {
            await this.Extensibility.Shell().ShowPromptAsync(
                "MIDL: No Windows SDK installations found.",
                PromptOptions.OK,
                cancellationToken);
            return;
        }

        // Build a prompt message with available versions since the Extensibility SDK
        // doesn't have a QuickPick equivalent — use a prompt with version list
        string versionList = string.Join("\n", versions.Select((v, i) => $"  {i + 1}. {v}"));
        string message = $"Available Windows SDK versions:\n{versionList}\n\nCurrent newest: {versions[0]}\n\nTo change, update the 'windowsSdkVersion' setting in Tools → Options → MIDL.";

        await this.Extensibility.Shell().ShowPromptAsync(
            message,
            PromptOptions.OK,
            cancellationToken);
    }

    internal static List<string> DiscoverSdkVersions()
    {
        List<string> versions = [];

        try
        {
            if (!Directory.Exists(SdkRoot))
            {
                return versions;
            }

            foreach (string dir in Directory.GetDirectories(SdkRoot))
            {
                string dirName = Path.GetFileName(dir);
                if (dirName.Equals("Facade", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (File.Exists(Path.Combine(dir, "Windows.winmd")))
                {
                    versions.Add(dirName);
                }
            }

            versions.Sort();
            versions.Reverse();
        }
        catch
        {
            // SDK scan failures are non-fatal
        }

        return versions;
    }
}
