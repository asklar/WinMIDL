using Midl.Lsp;
using Midl.Syntax;

namespace Midl.Cli;

class Program
{
    static int Main(string[] args)
    {
        var options = CommandLineParser.Parse(args);

        if (options.LspMode)
        {
            MidlTrace.InitFromEnvironment();
            var server = new MidlLanguageServer(
                Console.OpenStandardInput(),
                Console.OpenStandardOutput());

            // Auto-discover Windows SDK for LSP mode
            if (OperatingSystem.IsWindows())
            {
                var sdk = WindowsSdkLocator.Discover();
                if (sdk != null)
                {
                    server.SetSdkIncludePaths(sdk.IncludePaths);
                    if (sdk.WindowsWinMdPath != null)
                        server.SetReferencePaths(new List<string> { sdk.WindowsWinMdPath });
                }
            }

            server.RunAsync().GetAwaiter().GetResult();
            return 0;
        }

        var driver = new CompilationDriver();
        return driver.Run(options);
    }
}
