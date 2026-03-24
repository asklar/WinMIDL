namespace Midl.VsExtension;

using Microsoft.VisualStudio.Extensibility;

[VisualStudioContribution]
public class MidlExtension : Extension
{
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        Metadata = new(
            id: "WinMIDL.LanguageSupport",
            version: this.ExtensionAssemblyVersion,
            publisherName: "AlexanderSklarMSFT",
            displayName: "MIDL Language Support",
            description: "Language support for WinRT Interface Definition Language (MIDL/IDL) powered by the Midl.Cli language server."),
    };
}
