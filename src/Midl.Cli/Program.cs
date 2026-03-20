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
            server.RunAsync().GetAwaiter().GetResult();
            return 0;
        }

        var driver = new CompilationDriver();
        return driver.Run(options);
    }
}
