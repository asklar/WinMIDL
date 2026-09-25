// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
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
        [UseLspLanguageService(true)] // Uses LSP and bypasses C/C++ language service for .idl files
        internal static FileExtensionToContentTypeDefinition IdlFileExtension;

        [Export]
        [FileExtension(".idlrt")]
        [ContentType("midl")]
        internal static FileExtensionToContentTypeDefinition IdlrtFileExtension;
    }
}

namespace Microsoft.VisualStudio.Utilities
{
    /// <summary>
    /// When applied to a <see cref="FileExtensionToContentTypeDefinition"/> export, prevents the legacy
    /// language service registered in the VS settings store from overriding the content type mapping
    /// for the specified file extension.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    internal sealed class UseLspLanguageServiceAttribute : Attribute
    {
        /// <summary>
        /// Constructs a new instance of the attribute.
        /// </summary>
        /// <param name="useLspLanguageService">Whether to use the LSP language service for the file extension.</param>
        public UseLspLanguageServiceAttribute(bool useLspLanguageService)
        {
            UseLspLanguageService = useLspLanguageService;
        }

        /// <summary>
        /// Gets whether the extension should use the LSP language service.
        /// </summary>
        public bool UseLspLanguageService { get; }
    }
}