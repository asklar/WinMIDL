// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Lsp;

namespace Midl.Integration.Tests;

public class WinMdTextDocumentContentTests
{
    [Fact]
    public async Task InvalidWinMdUri_ReturnsRequestError()
    {
        using var input = new MemoryStream();
        await new JsonRpcWriter(input).SendRequestAsync(
            1,
            "workspace/textDocumentContent",
            new Dictionary<string, object?> { ["uri"] = "file:///example.idl" });
        input.Position = 0;

        using var output = new MemoryStream();
        await new MidlLanguageServer(input, output).RunAsync();
        output.Position = 0;

        using var response = await new JsonRpcReader(output).ReadMessageAsync();
        Assert.NotNull(response);
        Assert.Equal(-32603, response.RootElement.GetProperty("error").GetProperty("code").GetInt32());
    }
}
