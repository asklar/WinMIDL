// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Midl.VsExtension.Tests
{
    /// <summary>
    /// Tests for WinMD decompiler URI parsing and SDK version discovery.
    /// </summary>
    public class WinMdDecompilerTests
    {
        [Theory]
        [InlineData("midl-winmd:///C:/Program Files/Windows.winmd/Windows.Foundation.IAsyncAction")]
        [InlineData("midl-winmd:///C:/sdk/Windows.winmd/Windows.UI.Xaml.Controls")]
        [InlineData("midl-winmd:///C:/sdk/Windows.winmd")]
        public void Decompile_ParsesUriCorrectly(string uri)
        {
            // We can't fully test decompilation without the CLI binary,
            // but we verify URI parsing doesn't throw and returns content.
            string result = WinMdDecompiler.Decompile(uri);

            // Should always return something (either content or error message)
            Assert.NotNull(result);

            if (ServerDiscovery.FindServerPath() == null)
            {
                Assert.Contains("MIDL server binary not found", result);
            }
        }

        [Fact]
        public void Decompile_HandlesEmptyUri()
        {
            string result = WinMdDecompiler.Decompile("");
            Assert.NotNull(result);
        }

        [Fact]
        public void DiscoverSdkVersions_DoesNotThrow()
        {
            var ex = Record.Exception(() => MidlCommands.DiscoverSdkVersions());
            Assert.Null(ex);
        }

        [Fact]
        public void DiscoverSdkVersions_ReturnsListOfStrings()
        {
            List<string> versions = MidlCommands.DiscoverSdkVersions();
            Assert.NotNull(versions);
            foreach (string v in versions)
            {
                Assert.False(string.IsNullOrEmpty(v));
            }
        }
    }
}
