using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Midl.VsExtension
{
    /// <summary>
    /// Formats a <c>midl/diagnosticReport</c> response into a GitHub issue body.
    /// </summary>
    internal static class DiagnosticReportFormatter
    {
        private const int MaxBodyLength = 6000;
        private const int MaxIdlContentLength = 5000;
        private const int MaxEvents = 50;

        /// <summary>
        /// Builds a GitHub issue body from a diagnostic report.
        /// </summary>
        /// <param name="report">The <c>midl/diagnosticReport</c> response object.</param>
        /// <param name="includeIdlFiles">Whether to include IDL file contents in the body.</param>
        /// <param name="ideVersion">The Visual Studio version string.</param>
        /// <param name="extensionVersion">The MIDL extension version string.</param>
        /// <returns>The issue body, truncated to a URL-safe length.</returns>
        public static string BuildIssueBody(JObject report, bool includeIdlFiles, string ideVersion, string extensionVersion)
        {
            List<string> sections = [];

            AppendEnvironment(sections, report, ideVersion, extensionVersion);
            AppendFileStatistics(sections, report);
            AppendEventLog(sections, report);

            if (includeIdlFiles)
            {
                AppendIdlFiles(sections, report);
            }

            sections.Add("<h2>Description</h2>");
            sections.Add(string.Empty);
            sections.Add("_Please describe the issue_");
            sections.Add(string.Empty);

            return Truncate(string.Join("\n", sections));
        }

        internal static string BuildIssueUrl(string body) =>
            $"https://github.com/asklar/WinMIDL/issues/new?body={Uri.EscapeDataString(body)}&labels=bug";

        /// <summary>
        /// Builds a minimal GitHub issue body for when the language server is not running.
        /// </summary>
        /// <param name="logTail">The tail of the persisted server log, or <see langword="null"/> when unavailable.</param>
        /// <param name="ideVersion">The Visual Studio version string.</param>
        /// <param name="extensionVersion">The MIDL extension version string.</param>
        /// <returns>The issue body, truncated to a URL-safe length.</returns>
        public static string BuildFallbackBody(string logTail, string ideVersion, string extensionVersion)
        {
            List<string> sections =
            [
                "<h2>Environment</h2>",
                string.Empty,
                $"- Visual Studio: {ideVersion}",
                $"- OS: {Environment.OSVersion}",
                $"- Extension: {extensionVersion}",
                "- Server: not running",
                string.Empty,
            ];

            if (!string.IsNullOrEmpty(logTail))
            {
                sections.Add("<details><summary>Server Log (last 100 lines from previous session)</summary>");
                sections.Add(string.Empty);
                sections.Add("```");
                sections.Add(logTail);
                sections.Add("```");
                sections.Add("</details>");
                sections.Add(string.Empty);
            }

            sections.Add("<h2>Description</h2>");
            sections.Add(string.Empty);
            sections.Add("_Please describe the issue_");

            return Truncate(string.Join("\n", sections));
        }

        private static void AppendEnvironment(List<string> sections, JObject report, string ideVersion, string extensionVersion)
        {
            sections.Add("<h2>Environment</h2>");
            sections.Add(string.Empty);
            sections.Add("| Key | Value |");
            sections.Add("|-----|-------|");
            sections.Add($"| Visual Studio | {ideVersion} |");
            sections.Add($"| OS | {Str(report, "platform")} |");
            sections.Add($"| Architecture | {Str(report, "architecture")} |");
            sections.Add($"| Runtime | {Str(report, "runtime")} |");
            sections.Add($"| Extension | {extensionVersion} |");
            sections.Add($"| Server | {Str(report, "serverVersion")} |");
            sections.Add($"| Memory (private) | {Num(report, "privateBytesMB")} MB |");
            sections.Add($"| Memory (working set) | {Num(report, "workingSetMB")} MB |");
            sections.Add($"| Memory (GC heap) | {Num(report, "gcHeapMB")} MB |");
            sections.Add($"| Compilation mode | {Str(report, "compilationMode")} |");
            sections.Add($"| Files scanned | {Num(report, "filesScanned")} |");
            sections.Add($"| Files open | {Num(report, "filesOpen")} |");
            sections.Add($"| Compilations cached | {Num(report, "compilationsCached")} |");
            sections.Add($"| WinMD providers cached | {Num(report, "winmdProvidersCached")} |");
            sections.Add($"| Project file | {(Bool(report, "hasProjectFile") ? "yes" : "no")} |");
            sections.Add(string.Empty);
        }

        private static void AppendFileStatistics(List<string> sections, JObject report)
        {
            if (!TryGetArray(report, "fileStats", out JArray fileStats) || fileStats.Count == 0)
            {
                return;
            }

            sections.Add("<h2>File Statistics</h2>");
            sections.Add(string.Empty);
            sections.Add("| File | Size | Lines | User Types | Resolved Types | Errors | Warnings | MIDL3 |");
            sections.Add("|------|------|-------|------------|----------------|--------|----------|-------|");

            foreach (JToken f in fileStats)
            {
                string resolvedCell = FormatResolved(f);
                string midl3 = Bool(f, "isMidl3") ? "\u2713" : "\u2717";
                sections.Add($"| {Str(f, "relativePath")} | {Num(f, "sizeBytes")}B | {Num(f, "lines")} | {Num(f, "userTypes")} | {resolvedCell} | {Num(f, "errors")} | {Num(f, "warnings")} | {midl3} |");
            }

            sections.Add(string.Empty);
        }

        private static string FormatResolved(JToken fileStat)
        {
            long resolvedTypes = Num(fileStat, "resolvedTypes");
            if (resolvedTypes == 0)
            {
                return "0";
            }

            if (!TryGetObject(fileStat, "resolvedFrom", out JObject resolvedFrom))
            {
                return resolvedTypes.ToString();
            }

            List<string> parts = [];
            foreach (JProperty source in resolvedFrom.Properties())
            {
                parts.Add($"{source.Value} from {source.Name}");
            }

            return $"{resolvedTypes} ({string.Join(", ", parts)})";
        }

        private static void AppendEventLog(List<string> sections, JObject report)
        {
            if (!TryGetArray(report, "events", out JArray events) || events.Count == 0)
            {
                return;
            }

            List<JToken> recent = TakeLast(events, MaxEvents);

            sections.Add($"<details><summary>Event Log (last {recent.Count} events)</summary>");
            sections.Add(string.Empty);
            sections.Add("```");

            foreach (JToken e in recent)
            {
                string meta = TryGetObject(e, "metadata", out JObject metadata) ? " " + metadata.ToString(Formatting.None) : string.Empty;
                sections.Add($"[{Str(e, "timestamp")}] [{Str(e, "category")}] {Str(e, "message")}{meta}");
            }

            sections.Add("```");
            sections.Add("</details>");
            sections.Add(string.Empty);
        }

        private static void AppendIdlFiles(List<string> sections, JObject report)
        {
            if (!TryGetArray(report, "fileStats", out JArray fileStats))
            {
                return;
            }

            List<string> idlContents = [];
            foreach (JToken f in fileStats)
            {
                if (f is not JObject file || file["content"] is not JValue { Type: JTokenType.String } contentElement)
                {
                    continue;
                }

                string content = contentElement.Value<string>();
                if (string.IsNullOrEmpty(content))
                {
                    continue;
                }

                string truncated = content.Length > MaxIdlContentLength
                    ? content.Substring(0, MaxIdlContentLength) + "\n... (truncated)"
                    : content;

                idlContents.Add($"<details><summary>{Str(f, "relativePath")} ({Num(f, "sizeBytes")}B, {Num(f, "lines")} lines)</summary>\n\n```idl\n{truncated}\n```\n</details>");
            }

            if (idlContents.Count > 0)
            {
                sections.Add("<h2>IDL Files</h2>");
                sections.Add(string.Empty);
                sections.Add(string.Join("\n", idlContents));
                sections.Add(string.Empty);
            }
        }

        private static string Truncate(string body) =>
            body.Length > MaxBodyLength
                ? body.Substring(0, MaxBodyLength) + "\n\n... (truncated \u2014 please paste additional details manually)"
                : body;

        private static List<JToken> TakeLast(JArray array, int count) =>
            array.Skip(Math.Max(0, array.Count - count)).ToList();

        private static bool TryGetArray(JToken parent, string name, out JArray value)
        {
            if (parent is JObject obj && obj[name] is JArray array)
            {
                value = array;
                return true;
            }

            value = null!;
            return false;
        }

        private static bool TryGetObject(JToken parent, string name, out JObject value)
        {
            if (parent is JObject obj && obj[name] is JObject nested)
            {
                value = nested;
                return true;
            }

            value = null!;
            return false;
        }

        private static string Str(JToken parent, string name, string fallback = "unknown") =>
            parent is JObject obj && obj[name] is JValue { Type: JTokenType.String } value
                ? value.Value<string>() ?? fallback
                : fallback;

        private static long Num(JToken parent, string name) =>
            parent is JObject obj && obj[name] is JValue { Type: JTokenType.Integer } value
                ? value.Value<long>()
                : 0;

        private static bool Bool(JToken parent, string name) =>
            parent is JObject obj && obj[name] is JValue { Type: JTokenType.Boolean } value
            && value.Value<bool>();
    }
}
