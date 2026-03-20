using System.Text;
using System.Text.Json;

namespace Midl.Lsp;

/// <summary>
/// Writes JSON-RPC messages to an LSP-compliant output stream (Content-Length headers + JSON body).
/// Uses Utf8JsonWriter for NativeAOT-compatible, reflection-free JSON serialization.
/// </summary>
public sealed class JsonRpcWriter
{
    private readonly Stream _output;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public JsonRpcWriter(Stream output)
    {
        _output = output;
    }

    public async Task SendResponseAsync(object? id, object? result)
    {
        var msg = new Dictionary<string, object?>
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id,
            ["result"] = result
        };
        await WriteMessageAsync(msg);
    }

    public async Task SendErrorAsync(object? id, int code, string message)
    {
        var msg = new Dictionary<string, object?>
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id,
            ["error"] = new Dictionary<string, object?>
            {
                ["code"] = code,
                ["message"] = message
            }
        };
        await WriteMessageAsync(msg);
    }

    public async Task SendNotificationAsync(string method, object? @params)
    {
        var msg = new Dictionary<string, object?>
        {
            ["jsonrpc"] = "2.0",
            ["method"] = method
        };
        if (@params is not null)
        {
            msg["params"] = @params;
        }
        await WriteMessageAsync(msg);
    }

    private async Task WriteMessageAsync(object message)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = false }))
        {
            WriteJsonValue(writer, message);
        }

        var body = buffer.ToArray();
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

    internal static void WriteJsonValue(Utf8JsonWriter writer, object? value)
    {
        switch (value)
        {
            case null:
                writer.WriteNullValue();
                break;
            case string s:
                writer.WriteStringValue(s);
                break;
            case bool b:
                writer.WriteBooleanValue(b);
                break;
            case int i:
                writer.WriteNumberValue(i);
                break;
            case long l:
                writer.WriteNumberValue(l);
                break;
            case double d:
                writer.WriteNumberValue(d);
                break;
            case JsonElement je:
                je.WriteTo(writer);
                break;
            case IDictionary<string, object?> dict:
                writer.WriteStartObject();
                foreach (var (key, val) in dict)
                {
                    writer.WritePropertyName(key);
                    WriteJsonValue(writer, val);
                }
                writer.WriteEndObject();
                break;
            case System.Collections.IEnumerable enumerable:
                writer.WriteStartArray();
                foreach (var item in enumerable)
                    WriteJsonValue(writer, item);
                writer.WriteEndArray();
                break;
            default:
                throw new NotSupportedException(
                    $"JsonRpcWriter cannot serialize type '{value.GetType().FullName}'. " +
                    "Use Dictionary<string, object?> instead of anonymous types for AOT compatibility.");
        }
    }
}
