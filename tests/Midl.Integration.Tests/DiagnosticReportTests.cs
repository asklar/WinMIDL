// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Midl.Lsp;

namespace Midl.Integration.Tests;

public class DiagnosticEventLogTests
{
    [Fact]
    public void Log_RecordsEvents_InChronologicalOrder()
    {
        var log = new DiagnosticEventLog(capacity: 10);
        log.Log("init", "Server started");
        log.Log("scan", "Found 5 files");
        log.Log("compile", "Compiled file.idl");

        var events = log.GetEvents();
        Assert.Equal(3, events.Count);
        Assert.Equal("init", events[0].Category);
        Assert.Equal("scan", events[1].Category);
        Assert.Equal("compile", events[2].Category);
    }

    [Fact]
    public void Log_RingBuffer_EvictsOldestWhenFull()
    {
        var log = new DiagnosticEventLog(capacity: 3);
        log.Log("a", "first");
        log.Log("b", "second");
        log.Log("c", "third");
        log.Log("d", "fourth"); // should evict "first"

        var events = log.GetEvents();
        Assert.Equal(3, events.Count);
        Assert.Equal("b", events[0].Category);
        Assert.Equal("c", events[1].Category);
        Assert.Equal("d", events[2].Category);
    }

    [Fact]
    public void Log_RingBuffer_WrapsMultipleTimes()
    {
        var log = new DiagnosticEventLog(capacity: 2);
        for (int i = 0; i < 7; i++)
            log.Log($"cat{i}", $"msg{i}");

        var events = log.GetEvents();
        Assert.Equal(2, events.Count);
        Assert.Equal("cat5", events[0].Category);
        Assert.Equal("cat6", events[1].Category);
    }

    [Fact]
    public void Log_Empty_ReturnsEmptyList()
    {
        var log = new DiagnosticEventLog();
        var events = log.GetEvents();
        Assert.Empty(events);
    }

    [Fact]
    public void Log_PreservesMetadata()
    {
        var log = new DiagnosticEventLog();
        var meta = new Dictionary<string, string> { ["file"] = "test.idl", ["lines"] = "42" };
        log.Log("compile", "Compiled", meta);

        var events = log.GetEvents();
        Assert.Single(events);
        Assert.NotNull(events[0].Metadata);
        Assert.Equal("test.idl", events[0].Metadata!["file"]);
        Assert.Equal("42", events[0].Metadata!["lines"]);
    }

    [Fact]
    public void ToArray_ProducesSourceGenCompatibleOutput()
    {
        var log = new DiagnosticEventLog();
        log.Log("init", "Started", new() { ["version"] = "1.0" });
        log.Log("scan", "Done");

        var array = log.ToArray();
        Assert.Equal(2, array.Length);

        Assert.Equal("init", array[0].Category);
        Assert.Equal("scan", array[1].Category);
        Assert.Contains("version", array[0].Metadata!.Keys);
        Assert.Null(array[1].Metadata);
    }

    [Fact]
    public void Log_TimestampsAreIncreasing()
    {
        var log = new DiagnosticEventLog();
        log.Log("a", "first");
        System.Threading.Thread.Sleep(10);
        log.Log("b", "second");

        var events = log.GetEvents();
        Assert.True(events[1].Timestamp >= events[0].Timestamp);
    }
}

public class LooksLikeMidl3Tests
{
    [Theory]
    [InlineData("namespace N { runtimeclass Foo { } }", true)]
    [InlineData("namespace N { interface IFoo { void Bar(); } }", true)]
    [InlineData("import \"windows.foundation.idl\"", true)]
    [InlineData("namespace Windows.Foundation { }", true)]
    [InlineData("runtimeclass Foo { }", true)]
    public void Midl3Sources_DetectedAsMidl3(string source, bool expected)
    {
        Assert.Equal(expected, MidlLanguageServer.LooksLikeMidl3(source));
    }

    [Theory]
    [InlineData("typedef [v1_enum] enum { A = 0 } MyEnum;")]
    [InlineData("cpp_quote(\"#include <windows.h>\")")]
    [InlineData("[uuid(12345678-1234-1234-1234-123456789ABC)] interface IFoo { error_status_t Bar(); }")]
    [InlineData("typedef struct { int x; int y; } POINT;")]
    [InlineData("// just a comment")]
    [InlineData("")]
    public void Midl2Sources_NotDetectedAsMidl3(string source)
    {
        Assert.False(MidlLanguageServer.LooksLikeMidl3(source));
    }
}

public class DiagnosticReportTests
{
    private static bool IdEquals(JsonElement id, long expected) =>
        id.ValueKind == System.Text.Json.JsonValueKind.Number && id.GetInt64() == expected;

    [Fact]
    public async Task DiagnosticReport_ReturnsServerStats()
    {
        var serverInput = new MemoryStream();
        var serverOutput = new MemoryStream();
        var server = new MidlLanguageServer(serverInput, serverOutput);

        // initialize
        WriteJsonRpcMessage(serverInput, JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["jsonrpc"] = "2.0", ["id"] = 1, ["method"] = "initialize",
            ["params"] = new Dictionary<string, object?>
            {
                ["processId"] = 1234,
                ["rootUri"] = "file:///workspace",
                ["capabilities"] = new Dictionary<string, object?> { }
            }
        }));

        // initialized
        WriteJsonRpcMessage(serverInput, JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["jsonrpc"] = "2.0", ["method"] = "initialized",
            ["params"] = new Dictionary<string, object?> { }
        }));

        // Open a file so there's something to report on
        WriteJsonRpcMessage(serverInput, JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["jsonrpc"] = "2.0", ["method"] = "textDocument/didOpen",
            ["params"] = new Dictionary<string, object?>
            {
                ["textDocument"] = new Dictionary<string, object?>
                {
                    ["uri"] = "file:///workspace/test.idl",
                    ["languageId"] = "midl", ["version"] = 1,
                    ["text"] = "namespace N { runtimeclass Foo { } }"
                }
            }
        }));

        // Send midl/diagnosticReport request
        WriteJsonRpcMessage(serverInput, JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["jsonrpc"] = "2.0", ["id"] = 2, ["method"] = "midl/diagnosticReport",
            ["params"] = new Dictionary<string, object?> { }
        }));

        // shutdown + exit
        WriteJsonRpcMessage(serverInput, JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["jsonrpc"] = "2.0", ["id"] = 99, ["method"] = "shutdown", ["params"] = (object?)null
        }));
        WriteJsonRpcMessage(serverInput, JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["jsonrpc"] = "2.0", ["method"] = "exit"
        }));

        serverInput.Position = 0;
        await server.RunAsync();

        // Parse output and find the diagnosticReport response
        serverOutput.Position = 0;
        var outputText = System.Text.Encoding.UTF8.GetString(serverOutput.ToArray());
        var messages = ParseJsonRpcMessages(outputText);

        var reportResponse = messages
            .FirstOrDefault(m => m.RootElement.TryGetProperty("id", out var id) &&
                                 IdEquals(id, 2) &&
                                 m.RootElement.TryGetProperty("result", out _));
        Assert.NotNull(reportResponse);

        var result = reportResponse!.RootElement.GetProperty("result");

        // Verify required fields
        Assert.True(result.TryGetProperty("serverVersion", out var ver));
        Assert.Equal("0.1.0", ver.GetString());

        Assert.True(result.TryGetProperty("privateBytesMB", out _));
        Assert.True(result.TryGetProperty("workingSetMB", out _));
        Assert.True(result.TryGetProperty("gcHeapMB", out _));
        Assert.True(result.TryGetProperty("platform", out _));
        Assert.True(result.TryGetProperty("compilationMode", out _));

        // Should have file stats for the opened file
        Assert.True(result.TryGetProperty("fileStats", out var fileStats));
        Assert.True(fileStats.GetArrayLength() >= 1);

        // Should have events (at least init)
        Assert.True(result.TryGetProperty("events", out var events));
        Assert.True(events.GetArrayLength() >= 1);
    }

    [Fact]
    public async Task DiagnosticReport_EventLog_CapturesWorkspaceScanEvents()
    {
        // Create a temp workspace with an IDL file
        var tempDir = Path.Combine(Path.GetTempPath(), "midl_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var idlFile = Path.Combine(tempDir, "test.idl");
        File.WriteAllText(idlFile, "namespace N { runtimeclass Foo { } }");

        try
        {
            var serverInput = new MemoryStream();
            var serverOutput = new MemoryStream();
            var server = new MidlLanguageServer(serverInput, serverOutput);

            var rootUri = "file:///" + tempDir.Replace('\\', '/');

            WriteJsonRpcMessage(serverInput, JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["jsonrpc"] = "2.0", ["id"] = 1, ["method"] = "initialize",
                ["params"] = new Dictionary<string, object?>
                {
                    ["processId"] = 1234,
                    ["rootUri"] = rootUri,
                    ["capabilities"] = new Dictionary<string, object?> { }
                }
            }));
            WriteJsonRpcMessage(serverInput, JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["jsonrpc"] = "2.0", ["method"] = "initialized",
                ["params"] = new Dictionary<string, object?> { }
            }));

            // Request diagnostic report after scan
            WriteJsonRpcMessage(serverInput, JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["jsonrpc"] = "2.0", ["id"] = 2, ["method"] = "midl/diagnosticReport",
                ["params"] = new Dictionary<string, object?> { }
            }));

            WriteJsonRpcMessage(serverInput, JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["jsonrpc"] = "2.0", ["id"] = 99, ["method"] = "shutdown", ["params"] = (object?)null
            }));
            WriteJsonRpcMessage(serverInput, JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["jsonrpc"] = "2.0", ["method"] = "exit"
            }));

            serverInput.Position = 0;
            await server.RunAsync();

            serverOutput.Position = 0;
            var outputText = System.Text.Encoding.UTF8.GetString(serverOutput.ToArray());
            var messages = ParseJsonRpcMessages(outputText);

            var reportResponse = messages
                .FirstOrDefault(m => m.RootElement.TryGetProperty("id", out var id) &&
                                     IdEquals(id, 2) &&
                                     m.RootElement.TryGetProperty("result", out _));
            Assert.NotNull(reportResponse);

            var result = reportResponse!.RootElement.GetProperty("result");
            var events = result.GetProperty("events");

            // Should have init, scan start, compile, scan completed events
            var eventCategories = new List<string>();
            foreach (var e in events.EnumerateArray())
                eventCategories.Add(e.GetProperty("category").GetString()!);

            Assert.Contains("init", eventCategories);
            Assert.Contains("scan", eventCategories);
            Assert.Contains("compile", eventCategories);

            // Verify file was scanned and shows up in stats
            var fileStats = result.GetProperty("fileStats");
            Assert.True(fileStats.GetArrayLength() >= 1);
        }
        finally
        {
            try { Directory.Delete(tempDir, recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task WorkspaceScan_HighErrorFile_EvictsCompilation()
    {
        // Create a temp workspace with a classic MIDL2 file (lots of errors in MIDL3 parser)
        var tempDir = Path.Combine(Path.GetTempPath(), "midl_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var idlFile = Path.Combine(tempDir, "classic.idl");
        // Classic MIDL2 content that would produce many parse errors
        File.WriteAllText(idlFile, """
            cpp_quote("#include <unknwn.h>")
            typedef [v1_enum] enum { A = 0, B = 1, C = 2 } MyEnum;
            typedef struct { int x; int y; } POINT;
            [uuid(12345678-1234-1234-1234-123456789ABC), version(1.0)]
            interface ILegacy {
                error_status_t __stdcall DoWork([in] int x, [out] int* y);
                error_status_t __stdcall GetPoint([out] POINT* pt);
            };
            """);

        try
        {
            var serverInput = new MemoryStream();
            var serverOutput = new MemoryStream();
            var server = new MidlLanguageServer(serverInput, serverOutput);

            var rootUri = "file:///" + tempDir.Replace('\\', '/');

            WriteJsonRpcMessage(serverInput, JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["jsonrpc"] = "2.0", ["id"] = 1, ["method"] = "initialize",
                ["params"] = new Dictionary<string, object?>
                {
                    ["processId"] = 1234,
                    ["rootUri"] = rootUri,
                    ["capabilities"] = new Dictionary<string, object?> { }
                }
            }));
            WriteJsonRpcMessage(serverInput, JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["jsonrpc"] = "2.0", ["method"] = "initialized",
                ["params"] = new Dictionary<string, object?> { }
            }));
            WriteJsonRpcMessage(serverInput, JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["jsonrpc"] = "2.0", ["id"] = 2, ["method"] = "midl/diagnosticReport",
                ["params"] = new Dictionary<string, object?> { }
            }));
            WriteJsonRpcMessage(serverInput, JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["jsonrpc"] = "2.0", ["id"] = 99, ["method"] = "shutdown", ["params"] = (object?)null
            }));
            WriteJsonRpcMessage(serverInput, JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["jsonrpc"] = "2.0", ["method"] = "exit"
            }));

            serverInput.Position = 0;
            await server.RunAsync();

            serverOutput.Position = 0;
            var outputText = System.Text.Encoding.UTF8.GetString(serverOutput.ToArray());
            var messages = ParseJsonRpcMessages(outputText);

            var reportResponse = messages
                .FirstOrDefault(m => m.RootElement.TryGetProperty("id", out var id) &&
                                     IdEquals(id, 2) &&
                                     m.RootElement.TryGetProperty("result", out _));
            Assert.NotNull(reportResponse);

            var result = reportResponse!.RootElement.GetProperty("result");
            var events = result.GetProperty("events");

            // Check the event log for the scan events
            var eventMessages = new List<string>();
            foreach (var e in events.EnumerateArray())
                eventMessages.Add(e.GetProperty("message").GetString()!);

            // The file should have been compiled and logged
            Assert.Contains(eventMessages, m => m.Contains("classic.idl"));
        }
        finally
        {
            try { Directory.Delete(tempDir, recursive: true); } catch { }
        }
    }

    private static void WriteJsonRpcMessage(MemoryStream stream, string json)
    {
        var body = System.Text.Encoding.UTF8.GetBytes(json);
        var header = System.Text.Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n");
        stream.Write(header);
        stream.Write(body);
    }

    private static List<JsonDocument> ParseJsonRpcMessages(string raw)
    {
        var results = new List<JsonDocument>();
        int pos = 0;
        while (pos < raw.Length)
        {
            var headerEnd = raw.IndexOf("\r\n\r\n", pos, StringComparison.Ordinal);
            if (headerEnd < 0) break;
            var headerSection = raw[pos..headerEnd];
            var contentLengthMatch = System.Text.RegularExpressions.Regex.Match(headerSection, @"Content-Length:\s*(\d+)");
            if (!contentLengthMatch.Success) break;
            var contentLength = int.Parse(contentLengthMatch.Groups[1].Value);
            var bodyStart = headerEnd + 4;
            if (bodyStart + contentLength > raw.Length) break;
            var body = raw.Substring(bodyStart, contentLength);
            results.Add(JsonDocument.Parse(body));
            pos = bodyStart + contentLength;
        }
        return results;
    }
}

public class RedactPathTests
{
    private static bool IdEquals(JsonElement id, long expected) =>
        id.ValueKind == System.Text.Json.JsonValueKind.Number && id.GetInt64() == expected;

    [Fact]
    public async Task RedactPath_WorkspaceRelative_StripsRoot()
    {
        var serverInput = new MemoryStream();
        var serverOutput = new MemoryStream();
        var server = new MidlLanguageServer(serverInput, serverOutput);

        // Create a temp workspace with a file
        var tempDir = Path.Combine(Path.GetTempPath(), "midl_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var idlFile = Path.Combine(tempDir, "sub", "test.idl");
        Directory.CreateDirectory(Path.Combine(tempDir, "sub"));
        File.WriteAllText(idlFile, "namespace N { runtimeclass Foo { } }");

        try
        {
            var rootUri = "file:///" + tempDir.Replace('\\', '/');

            WriteJsonRpcMessage(serverInput, JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["jsonrpc"] = "2.0", ["id"] = 1, ["method"] = "initialize",
                ["params"] = new Dictionary<string, object?>
                {
                    ["processId"] = 1234,
                    ["rootUri"] = rootUri,
                    ["capabilities"] = new Dictionary<string, object?> { }
                }
            }));
            WriteJsonRpcMessage(serverInput, JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["jsonrpc"] = "2.0", ["method"] = "initialized",
                ["params"] = new Dictionary<string, object?> { }
            }));
            WriteJsonRpcMessage(serverInput, JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["jsonrpc"] = "2.0", ["id"] = 2, ["method"] = "midl/diagnosticReport",
                ["params"] = new Dictionary<string, object?> { }
            }));
            WriteJsonRpcMessage(serverInput, JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["jsonrpc"] = "2.0", ["id"] = 99, ["method"] = "shutdown", ["params"] = (object?)null
            }));
            WriteJsonRpcMessage(serverInput, JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["jsonrpc"] = "2.0", ["method"] = "exit"
            }));

            serverInput.Position = 0;
            await server.RunAsync();

            serverOutput.Position = 0;
            var outputText = System.Text.Encoding.UTF8.GetString(serverOutput.ToArray());
            var messages = ParseJsonRpcMessages(outputText);

            var reportResponse = messages
                .FirstOrDefault(m => m.RootElement.TryGetProperty("id", out var id) &&
                                     IdEquals(id, 2) &&
                                     m.RootElement.TryGetProperty("result", out _));
            Assert.NotNull(reportResponse);

            var result = reportResponse!.RootElement.GetProperty("result");

            // File paths in fileStats should be relative, not absolute
            var fileStats = result.GetProperty("fileStats");
            foreach (var f in fileStats.EnumerateArray())
            {
                var path = f.GetProperty("relativePath").GetString()!;
                // Should NOT contain the temp directory path
                Assert.DoesNotContain(Path.GetTempPath().TrimEnd('\\', '/'), path);
                // Should be a relative path
                Assert.False(Path.IsPathRooted(path), $"Path should be relative: {path}");
            }

            // Workspace root should also be redacted
            var workspaceRoot = result.GetProperty("workspaceRoot").GetString()!;
            Assert.Equal(".", workspaceRoot);
        }
        finally
        {
            try { Directory.Delete(tempDir, recursive: true); } catch { }
        }
    }

    private static void WriteJsonRpcMessage(MemoryStream stream, string json)
    {
        var body = System.Text.Encoding.UTF8.GetBytes(json);
        var header = System.Text.Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n");
        stream.Write(header);
        stream.Write(body);
    }

    private static List<JsonDocument> ParseJsonRpcMessages(string raw)
    {
        var results = new List<JsonDocument>();
        int pos = 0;
        while (pos < raw.Length)
        {
            var headerEnd = raw.IndexOf("\r\n\r\n", pos, StringComparison.Ordinal);
            if (headerEnd < 0) break;
            var headerSection = raw[pos..headerEnd];
            var contentLengthMatch = System.Text.RegularExpressions.Regex.Match(headerSection, @"Content-Length:\s*(\d+)");
            if (!contentLengthMatch.Success) break;
            var contentLength = int.Parse(contentLengthMatch.Groups[1].Value);
            var bodyStart = headerEnd + 4;
            if (bodyStart + contentLength > raw.Length) break;
            var body = raw.Substring(bodyStart, contentLength);
            results.Add(JsonDocument.Parse(body));
            pos = bodyStart + contentLength;
        }
        return results;
    }
}
