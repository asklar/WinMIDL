using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;

namespace Midl.VsExtension
{
    internal sealed class HoverDocsMiddleLayer : ILanguageClientMiddleLayer2<JsonDocument>
    {
        private const string HoverMethod = "textDocument/hover";
        private const int MaxDescriptionLength = 200;

        private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(3) };
        private static readonly ConcurrentDictionary<string, string> DescriptionCache = new(StringComparer.Ordinal);
        private static readonly Regex LearnLinkRegex = new(
            @"\[📖 View on Microsoft Learn\]\(command:midl\.openDocs\?([^)]+)\)", RegexOptions.Compiled);
        private static readonly Regex DescriptionMetaRegex = new(
            "<meta\\s+name=[\"']description[\"']\\s+content=[\"']([^\"']+)[\"']",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex DescriptionMetaReversedRegex = new(
            "content=[\"']([^\"']+)[\"']\\s+name=[\"']description[\"']",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly Func<string, Task<string>> getHtml;

        internal HoverDocsMiddleLayer(Func<string, Task<string>>? getHtml = null)
        {
            this.getHtml = getHtml ?? HttpClient.GetStringAsync;
        }

        public bool CanHandle(string methodName) => methodName == HoverMethod;

        public async Task<JsonDocument?> HandleRequestAsync(
            string methodName, JsonDocument methodParam, Func<JsonDocument, Task<JsonDocument?>> sendRequest)
        {
            JsonDocument? response = await sendRequest(methodParam);
            if (response is null ||
                response.RootElement.ValueKind != JsonValueKind.Object ||
                !response.RootElement.TryGetProperty("contents", out JsonElement contents) ||
                contents.ValueKind != JsonValueKind.Object ||
                !contents.TryGetProperty("kind", out JsonElement kind) ||
                kind.ValueKind != JsonValueKind.String ||
                kind.GetString() != "markdown" ||
                !contents.TryGetProperty("value", out JsonElement value) ||
                value.ValueKind != JsonValueKind.String)
                return response;

            string text = value.GetString()!;
            string? learnUrl = ExtractLearnUrl(text);
            if (learnUrl is null)
                return response;

            string updated = LearnLinkRegex.Replace(text, _ => $"[📖 View on Microsoft Learn]({learnUrl})", 1);

            string? description = await GetDescriptionAsync(learnUrl);
            if (!string.IsNullOrEmpty(description))
            {
                int anchor = updated.IndexOf("[📖 View on Microsoft Learn]", StringComparison.Ordinal);
                updated = updated.Insert(anchor, description + "\n\n");
            }

            if (updated == text)
                return response;

            JsonObject hover = (JsonObject)JsonNode.Parse(response.RootElement.GetRawText())!;
            ((JsonObject)hover["contents"]!)["value"] = updated;
            return JsonDocument.Parse(hover.ToJsonString());
        }

        public Task HandleNotificationAsync(
            string methodName, JsonDocument methodParam, Func<JsonDocument, Task> sendNotification) =>
            sendNotification(methodParam);

        internal static string? ExtractLearnUrl(string text)
        {
            Match link = LearnLinkRegex.Match(text);
            if (!link.Success)
                return null;

            try
            {
                using JsonDocument args = JsonDocument.Parse(Uri.UnescapeDataString(link.Groups[1].Value));
                return args.RootElement.ValueKind == JsonValueKind.Array &&
                    args.RootElement.GetArrayLength() > 0 &&
                    args.RootElement[0].ValueKind == JsonValueKind.String
                    ? ValidateLearnUrl(args.RootElement[0].GetString())
                    : null;
            }
            catch (JsonException ex)
            {
                MidlLanguageClient.LogWarning($"Invalid Microsoft Learn link in hover: {ex.Message}");
                return null;
            }
        }

        private static string? ValidateLearnUrl(string? url) =>
            Uri.TryCreate(url, UriKind.Absolute, out Uri uri) &&
            uri.Scheme == Uri.UriSchemeHttps &&
            uri.Host.Equals("learn.microsoft.com", StringComparison.OrdinalIgnoreCase) &&
            uri.Port == 443 &&
            uri.UserInfo.Length == 0
                ? uri.AbsoluteUri
                : null;

        private async Task<string?> GetDescriptionAsync(string learnUrl)
        {
            if (DescriptionCache.TryGetValue(learnUrl, out string cached))
                return cached;

            string html;
            try
            {
                html = await getHtml(learnUrl);
            }
            catch (HttpRequestException ex)
            {
                MidlLanguageClient.LogWarning($"Could not fetch Microsoft Learn hover description: {ex.Message}");
                return null;
            }
            catch (TaskCanceledException ex)
            {
                MidlLanguageClient.LogWarning($"Microsoft Learn hover description timed out: {ex.Message}");
                return null;
            }

            Match meta = DescriptionMetaRegex.Match(html);
            if (!meta.Success)
                meta = DescriptionMetaReversedRegex.Match(html);

            string description = meta.Success ? WebUtility.HtmlDecode(meta.Groups[1].Value) : string.Empty;
            if (description.Length > MaxDescriptionLength)
                description = description.Substring(0, MaxDescriptionLength - 3) + "...";

            DescriptionCache[learnUrl] = description;
            return description;
        }
    }
}
