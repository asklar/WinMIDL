// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace Midl.Lsp;

/// <summary>
/// Reads JSON-RPC messages from an LSP-compliant input stream (Content-Length headers + JSON body).
/// </summary>
public sealed class JsonRpcReader
{
    private readonly Stream _input;

    public JsonRpcReader(Stream input)
    {
        _input = input;
    }

    public async Task<JsonDocument?> ReadMessageAsync(CancellationToken ct = default)
    {
        int contentLength = await ReadHeadersAsync(ct);
        if (contentLength <= 0) return null;

        var buffer = new byte[contentLength];
        int totalRead = 0;
        while (totalRead < contentLength)
        {
            int read = await _input.ReadAsync(buffer.AsMemory(totalRead, contentLength - totalRead), ct);
            if (read == 0) return null; // stream closed
            totalRead += read;
        }

        return JsonDocument.Parse(buffer);
    }

    private async Task<int> ReadHeadersAsync(CancellationToken ct)
    {
        int contentLength = -1;
        while (true)
        {
            string? line = await ReadLineAsync(ct);
            if (line is null) return -1; // stream closed
            if (line.Length == 0) break;  // empty line = end of headers

            if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
            {
                var value = line["Content-Length:".Length..].Trim();
                if (int.TryParse(value, out var len))
                    contentLength = len;
            }
            // Content-Type and other headers are ignored per LSP spec
        }
        return contentLength;
    }

    private async Task<string?> ReadLineAsync(CancellationToken ct)
    {
        var bytes = new List<byte>();
        var buf = new byte[1];
        while (true)
        {
            int read = await _input.ReadAsync(buf, ct);
            if (read == 0)
                return bytes.Count > 0 ? System.Text.Encoding.UTF8.GetString(bytes.ToArray()) : null;

            if (buf[0] == (byte)'\n')
            {
                // strip trailing \r if present
                if (bytes.Count > 0 && bytes[^1] == (byte)'\r')
                    bytes.RemoveAt(bytes.Count - 1);
                return System.Text.Encoding.UTF8.GetString(bytes.ToArray());
            }
            bytes.Add(buf[0]);
        }
    }
}
