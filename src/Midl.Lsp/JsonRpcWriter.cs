using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Midl.Lsp;

/// <summary>
/// Writes JSON-RPC messages to an LSP-compliant output stream (Content-Length headers + JSON body).
/// </summary>
public sealed class JsonRpcWriter
{
    private readonly Stream _output;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public JsonRpcWriter(Stream output)
    {
        _output = output;
    }

    public async Task SendResponseAsync(object? id, object? result)
    {
        var msg = new { jsonrpc = "2.0", id, result };
        await WriteMessageAsync(msg);
    }

    public async Task SendErrorAsync(object? id, int code, string message)
    {
        var msg = new { jsonrpc = "2.0", id, error = new { code, message } };
        await WriteMessageAsync(msg);
    }

    public async Task SendNotificationAsync(string method, object? @params)
    {
        var msg = new { jsonrpc = "2.0", method, @params };
        await WriteMessageAsync(msg);
    }

    private async Task WriteMessageAsync(object message)
    {
        var json = JsonSerializer.Serialize(message, s_jsonOptions);
        var body = Encoding.UTF8.GetBytes(json);
        var header = Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n");

        await _writeLock.WaitAsync();
        try
        {
            await _output.WriteAsync(header);
            await _output.WriteAsync(body);
            await _output.FlushAsync();
        }
        finally
        {
            _writeLock.Release();
        }
    }
}
