namespace Midl.VsExtension.Tests
{
    public class ClassDiagramHtmlTests
    {
        [Fact]
        public void Build_EscapesDotTextBeforeEmbeddingInScript()
        {
            string html = ClassDiagramHtml.Build("label=\"</script><script>bad()</script>\"", false);

            Assert.DoesNotContain("</script><script>bad()", html);
            Assert.Contains("\\u003c/script\\u003e", html);
        }
    }
}
