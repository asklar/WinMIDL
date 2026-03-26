using System.ComponentModel.Composition;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;

namespace Midl.VsExtension
{
    public static class MidlContentTypeDefinitions
    {
        [Export]
        [Name("midl")]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
        internal static ContentTypeDefinition MidlContentType;

        [Export]
        [FileExtension(".idl")]
        [ContentType("midl")]
        internal static FileExtensionToContentTypeDefinition IdlFileExtension;

        [Export]
        [FileExtension(".idlrt")]
        [ContentType("midl")]
        internal static FileExtensionToContentTypeDefinition IdlrtFileExtension;
    }
}
