// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json;

namespace Midl.Lsp;

public sealed partial class MidlLanguageServer
{
    /// <summary>
    /// Handles midl/decompileDefinition — finds a type in a WinMD and returns
    /// the decompiled namespace text + definition position.
    /// Params: { winmdPath, typeName, currentNamespace? }
    /// Returns: { namespace, text, line, character } or null
    /// </summary>
    private object? HandleDecompileDefinition(JsonElement? @params)
    {
        if (@params is null) return null;
        var winmdPath = @params.Value.GetProperty("winmdPath").GetString();
        var typeName = @params.Value.GetProperty("typeName").GetString();
        var currentNs = @params.Value.TryGetProperty("currentNamespace", out var nsVal) ? nsVal.GetString() : null;

        if (string.IsNullOrEmpty(winmdPath) || string.IsNullOrEmpty(typeName)) return null;

        // Build candidates
        var candidates = new List<string>();
        if (typeName!.Contains('.')) candidates.Add(typeName);
        else if (currentNs != null) candidates.Add($"{currentNs}.{typeName}");

        // Search the WinMD
        try
        {
            using var fs = File.OpenRead(winmdPath!);
            using var peReader = new PEReader(fs);
            var reader = peReader.GetMetadataReader();

            // FQN match first
            foreach (var handle in reader.TypeDefinitions)
            {
                var td = reader.GetTypeDefinition(handle);
                var name = reader.GetString(td.Name);
                var ns = reader.GetString(td.Namespace);
                var backtick = name.IndexOf('`');
                if (backtick >= 0) name = name[..backtick];
                var fullName = string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";

                bool matched = candidates.Any(c => fullName.Equals(c, StringComparison.Ordinal));
                if (!matched && name.Equals(typeName.Contains('.') ? typeName[(typeName.LastIndexOf('.') + 1)..] : typeName, StringComparison.Ordinal))
                    matched = true;
                if (!matched) continue;

                // Found — decompile and find definition
                var decompiler = new Midl.CodeGen.Decompiler.WinMdDecompiler();
                var decompiled = decompiler.Decompile(winmdPath, ns);

                int defLine = 0, defChar = 0;
                foreach (var kw in new[] { "runtimeclass", "interface", "struct", "enum", "delegate" })
                {
                    var pattern = $"{kw} {name}";
                    var idx = decompiled.IndexOf(pattern, StringComparison.Ordinal);
                    if (idx >= 0)
                    {
                        idx += kw.Length + 1;
                        var before = decompiled[..idx];
                        defLine = before.Count(c => c == '\n');
                        var lastNl = before.LastIndexOf('\n');
                        defChar = lastNl >= 0 ? idx - lastNl - 1 : idx;
                        break;
                    }
                }

                return new Dictionary<string, object?>
                {
                    ["namespace"] = ns,
                    ["text"] = decompiled,
                    ["line"] = defLine,
                    ["character"] = defChar,
                };
            }
        }
        catch { }

        return null;
    }

    /// <summary>
    /// Handles workspace/textDocumentContent — returns the decompiled IDL text for a midl-winmd virtual document URI.
    /// </summary>
    /// <param name="params">The request parameters, expected to contain the virtual document <c>uri</c>.</param>
    /// <returns>An object exposing a <c>text</c> property with the decompiled IDL.</returns>
    private object HandleTextDocumentContent(JsonElement? @params)
    {
        if (@params is null)
            throw new ArgumentException("A WinMD document URI is required.", nameof(@params));

        string? uri = @params.Value.TryGetProperty("uri", out var uriProp) ? uriProp.GetString() : null;
        if (string.IsNullOrEmpty(uri) || !uri.StartsWith("midl-winmd:", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Expected a midl-winmd document URI.", nameof(@params));

        // URI format: midl-winmd:///path/to/file.winmd/Namespace
        string rawPath = Uri.UnescapeDataString(uri["midl-winmd:".Length..].TrimStart('/'));
        int winmdIdx = rawPath.IndexOf(".winmd", StringComparison.OrdinalIgnoreCase);
        if (winmdIdx < 0)
            throw new ArgumentException("The URI must contain a .winmd path.", nameof(@params));

        string winmdPath = rawPath[..(winmdIdx + 6)];
        string? nsFilter = null;
        string remainder = rawPath[(winmdIdx + 6)..];
        if (remainder.StartsWith('/') && remainder.Length > 1)
            nsFilter = remainder[1..];

        Midl.CodeGen.Decompiler.WinMdDecompiler decompiler = new();
        string decompiled = decompiler.Decompile(winmdPath, nsFilter);
        return new Dictionary<string, object?>
        {
            ["text"] = decompiled,
        };
    }
}
