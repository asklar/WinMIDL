using Newtonsoft.Json.Linq;

namespace Midl.VsExtension.Tests
{
    public class DiagnosticReportFormatterTests
    {
        private static JObject Report() => JObject.Parse("""
            {
              "platform": "Windows",
              "fileStats": [
                {
                  "relativePath": "example.idl",
                  "sizeBytes": 18,
                  "lines": 1,
                  "content": "private source text"
                }
              ]
            }
            """);

        [Fact]
        public void BuildIssueBody_ExcludesSourceWithoutConsent()
        {
            string body = DiagnosticReportFormatter.BuildIssueBody(Report(), false, "17.0", "0.1");

            Assert.Contains("example.idl", body);
            Assert.DoesNotContain("private source text", body);
        }

        [Fact]
        public void BuildIssueBody_IncludesSourceWithConsent()
        {
            string body = DiagnosticReportFormatter.BuildIssueBody(Report(), true, "17.0", "0.1");

            Assert.Contains("private source text", body);
        }

        [Fact]
        public void BuildIssueUrl_TargetsWinMidl()
        {
            string url = DiagnosticReportFormatter.BuildIssueUrl("file #1");

            Assert.StartsWith("https://github.com/asklar/WinMIDL/issues/new?", url);
            Assert.Contains("body=file%20%231", url);
        }
    }
}
