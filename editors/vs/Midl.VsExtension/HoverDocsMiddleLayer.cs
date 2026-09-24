using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using Newtonsoft.Json.Linq;

namespace Midl.VsExtension
{
    /// <summary>
    /// Enriches MIDL hovers and resolves WinMD definitions for VS clients without virtual-document support.
    /// </summary>
    /// <remarks>
    /// The MIDL server appends a <c>[📖 View on Microsoft Learn](command:midl.openDocs?["&lt;url&gt;"])</c> link to
    /// hovers for Windows SDK types. This layer fetches that page's meta description and injects it before the link,
    /// mirroring the VS Code client's hover middleware.
    /// </remarks>
    internal sealed class HoverDocsMiddleLayer : ILanguageClientMiddleLayer
    {
        private const string HoverMethod = "textDocument/hover";
        private const string DefinitionMethod = "textDocument/definition";
        private const int MaxDescriptionLength = 200;

        private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(3) };

        private static readonly ConcurrentDictionary<string, string> DescriptionCache = new(StringComparer.Ordinal);

        private static readonly Regex LearnLinkRegex = new(
            @"\[📖 View on Microsoft Learn\]\(command:midl\.openDocs\?(.+?)\)", RegexOptions.Compiled);

        private static readonly Regex PlaintextLearnLinkRegex = new(
            @"View on Microsoft Learn:\s+(https://learn\.microsoft\.com/[^\s]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex DescriptionMetaRegex = new(
            "<meta\\s+name=[\"']description[\"']\\s+content=[\"']([^\"']+)[\"']", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex DescriptionMetaReversedRegex = new(
            "content=[\"']([^\"']+)[\"']\\s+name=[\"']description[\"']", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex InsertAnchorRegex = new(
            @"(\n\n)(\[📖 View on Microsoft Learn\])", RegexOptions.Compiled);

        /// <inheritdoc/>
        public bool CanHandle(string methodName) =>
            methodName is HoverMethod or DefinitionMethod;

        /// <inheritdoc/>
        public async Task<JToken?> HandleRequestAsync(string methodName, JToken methodParam, Func<JToken, Task<JToken?>> sendRequest)
        {
            JToken? response = await sendRequest(methodParam);

            if (methodName == DefinitionMethod)
            {
                if (response is not null)
                    await RewriteDefinitionAsync(response, GetDecompiledTextAsync);

                return response;
            }

            await AugmentHoverAsync(response);

            return response;
        }

        internal static async Task RewriteDefinitionAsync(
            JToken response, Func<string, Task<string>> getDecompiledText)
        {
            IEnumerable<JObject> locations = response switch
            {
                JObject single => [single],
                JArray array => array.OfType<JObject>(),
                _ => [],
            };
            Dictionary<string, string> localUris = new(StringComparer.OrdinalIgnoreCase);

            foreach (JObject location in locations)
            {
                string propertyName = location["uri"] is not null ? "uri" : "targetUri";
                string? uri = location[propertyName]?.Value<string>();
                if (uri is null || !uri.StartsWith("midl-winmd:", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!localUris.TryGetValue(uri, out string? localUri))
                {
                    string content = await getDecompiledText(uri);
                    if (string.IsNullOrWhiteSpace(content))
                        throw new InvalidOperationException($"No decompiled content was returned for {uri}.");

                    string folder = Path.Combine(Path.GetTempPath(), "midl-decompiled");
                    Directory.CreateDirectory(folder);
                    using SHA256 sha = SHA256.Create();
                    string name = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(uri)))
                        .Replace("-", string.Empty).ToLowerInvariant();
                    string filePath = Path.Combine(folder, name + ".idlrt");
                    File.WriteAllText(filePath, content);
                    localUri = new Uri(filePath).AbsoluteUri;
                    localUris.Add(uri, localUri);
                }

                location[propertyName] = localUri;
            }
        }

        private static async Task<string> GetDecompiledTextAsync(string uri)
        {
            MidlLanguageClient client = MidlLanguageClient.ActiveInstance
                ?? throw new InvalidOperationException("MIDL language server is not running.");
            JToken result = await client.RequestAsync(
                "workspace/textDocumentContent",
                new Dictionary<string, object> { ["uri"] = uri },
                CancellationToken.None);

            return result is JObject response && response["text"]?.Type == JTokenType.String
                ? response.Value<string>("text")!
                : throw new InvalidOperationException($"MIDL language server could not decompile {uri}.");
        }

        /// <inheritdoc/>
        public Task HandleNotificationAsync(string methodName, JToken methodParam, Func<JToken, Task> sendNotification) =>
            sendNotification(methodParam);

        private static async Task AugmentHoverAsync(JToken? response)
        {
            if (response is not JObject hover || hover["contents"] is not JObject contents
                || contents["value"] is not { Type: JTokenType.String } valueToken)
            {
                return;
            }

            string text = valueToken.Value<string>()!;
            bool isPlaintext = contents["kind"]?.Value<string>() == "plaintext";
            string? learnUrl = ExtractLearnUrl(text);
            if (learnUrl == null)
            {
                return;
            }

            string updated = isPlaintext
                ? text
                : LearnLinkRegex.Replace(
                    text,
                    _ => $"[\U0001F4D6 View on Microsoft Learn]({learnUrl})",
                    1);

            string? description = await GetDescriptionAsync(learnUrl);
            if (!string.IsNullOrEmpty(description))
            {
                if (isPlaintext)
                {
                    int anchor = updated.IndexOf("View on Microsoft Learn:", StringComparison.Ordinal);
                    updated = updated.Insert(anchor, description + "\n\n");
                }
                else
                {
                    updated = InsertAnchorRegex.Replace(
                        updated,
                        m => $"{m.Groups[1].Value}{description}\n\n{m.Groups[2].Value}",
                        1);
                }
            }

            if (updated != text)
            {
                contents["value"] = updated;
            }
        }

        internal static string? ExtractLearnUrl(string text)
        {
            Match plain = PlaintextLearnLinkRegex.Match(text);
            if (plain.Success)
            {
                return plain.Groups[1].Value;
            }

            Match match = LearnLinkRegex.Match(text);
            if (!match.Success) return null;

            try
            {
                string decoded = Uri.UnescapeDataString(match.Groups[1].Value);
                JArray args = JArray.Parse(decoded);
                string? url = args.Count > 0 ? args[0].ToString() : null;
                return Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) &&
                    uri.Scheme == Uri.UriSchemeHttps &&
                    uri.Host.Equals("learn.microsoft.com", StringComparison.OrdinalIgnoreCase)
                        ? uri.AbsoluteUri
                        : null;
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                MidlLanguageClient.LogWarning($"Invalid Microsoft Learn link in hover: {ex.Message}");
                return null;
            }
        }

        internal static async Task<string?> GetDescriptionAsync(
            string learnUrl, Func<string, Task<string>>? getHtml = null)
        {
            if (DescriptionCache.TryGetValue(learnUrl, out string cached))
            {
                return cached.Length == 0 ? null : cached;
            }

            string? description = null;
            try
            {
                string html = await (getHtml is null
                    ? HttpClient.GetStringAsync(learnUrl)
                    : getHtml(learnUrl));
                Match meta = DescriptionMetaRegex.Match(html);
                if (!meta.Success)
                {
                    meta = DescriptionMetaReversedRegex.Match(html);
                }

                if (meta.Success)
                {
                    string decoded = WebUtility.HtmlDecode(meta.Groups[1].Value);
                    description = decoded.Length > MaxDescriptionLength
                        ? decoded.Substring(0, MaxDescriptionLength - 3) + "..."
                        : decoded;
                }
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

            DescriptionCache[learnUrl] = description ?? string.Empty;

            return description;
        }
    }
}
