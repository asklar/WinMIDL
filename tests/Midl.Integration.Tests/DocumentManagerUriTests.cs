// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Lsp;

namespace Midl.Integration.Tests;

public class DocumentManagerUriTests
{
    [Fact]
    public void DistinctVirtualDocumentUrisRetainTheirContent()
    {
        DocumentManager documents = new();
        documents.OpenDocument("midl-winmd:///types/Widget", "Widget");
        documents.OpenDocument("midl-winmd:///types/widget", "widget");

        Assert.Equal("Widget", documents.GetDocumentText("midl-winmd:///types/Widget"));
        Assert.Equal("widget", documents.GetDocumentText("midl-winmd:///types/widget"));
    }

    [Fact]
    public void FileUriCasingFollowsFileSystemRules()
    {
        DocumentManager documents = new();
        string path = Path.Combine(Path.GetTempPath(), "DocManagerCase.idl");
        string uri = LspUri.FromPath(path);
        string alternateCase = uri.ToLowerInvariant();

        documents.OpenDocument(uri, "original");
        documents.OpenDocument(alternateCase, "alternate");

        Assert.Equal(OperatingSystem.IsWindows() ? 1 : 2, documents.GetOpenDocumentUris().Count());
        Assert.Equal("alternate", documents.GetDocumentText(alternateCase));
    }

    [Fact]
    public void FileUriRoundTripsAnAbsolutePath()
    {
        string path = Path.Combine(Path.GetTempPath(), "document.idl");

        Assert.Equal(Path.GetFullPath(path), LspUri.GetFilePath(LspUri.FromPath(path)));
    }
}
