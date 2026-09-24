using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Midl.VsExtension.Tests
{
    public class HoverDocsMiddleLayerTests
    {
        [Fact]
        public void PlaintextHover_ExtractsMicrosoftLearnUrl()
        {
            const string url = "https://learn.microsoft.com/en-us/uwp/api/windows.foundation";

            Assert.Equal(url, HoverDocsMiddleLayer.ExtractLearnUrl(
                $"Windows.Foundation\n\nView on Microsoft Learn: {url}"));
        }

        [Fact]
        public async Task WinMdDefinition_UsesLocalDecompiledDocument()
        {
            JObject location = JObject.Parse("""
                {
                  "uri": "midl-winmd:///C:/sdk/Windows.winmd/Windows.Foundation",
                  "range": {
                    "start": { "line": 2, "character": 0 },
                    "end": { "line": 2, "character": 10 }
                  }
                }
                """);
            const string content = "namespace Windows.Foundation { }\n";

            await HoverDocsMiddleLayer.RewriteDefinitionAsync(
                location, _ => Task.FromResult(content));

            string filePath = new Uri(location.Value<string>("uri")!).LocalPath;
            try
            {
                Assert.Equal(content, File.ReadAllText(filePath));
                Assert.Equal(2, location["range"]!["start"]!["line"]!.Value<int>());
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        public async Task MissingLearnDescription_DoesNotBreakHoverEnrichment()
        {
            string url = "https://learn.microsoft.com/en-us/midl/no-description-" + Guid.NewGuid().ToString("N");

            string? description = await HoverDocsMiddleLayer.GetDescriptionAsync(
                url, _ => Task.FromResult("<html><head></head></html>"));

            Assert.Null(description);
        }
    }
}
