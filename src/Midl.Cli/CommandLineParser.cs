namespace Midl.Cli;

/// <summary>
/// Parsed MIDL command-line options.
/// </summary>
public class CommandLineOptions
{
    public string? InputFile { get; set; }
    public string? OutputDirectory { get; set; }      // /out
    public string? HeaderFile { get; set; }            // /h, /header
    public string? IidFile { get; set; }               // /iid
    public string? ProxyFile { get; set; }             // /proxy
    public string? CppProxyFile { get; set; }          // /cppproxy
    public string? DllDataFile { get; set; }           // /dlldata
    public string? WinMdFile { get; set; }             // /winmd
    public string? AcfFile { get; set; }               // /acf
#if WITH_COMPAT
    public string? CppCommand { get; set; }            // /cpp_cmd
#endif
    public string? CppOptions { get; set; }            // /cpp_opt
    public bool NoCpp { get; set; }                    // /no_cpp
    public bool WinRtMode { get; set; }                // /winrt
    public bool SyntaxCheckOnly { get; set; }          // /Zs, /syntax_check
    public bool NoLogo { get; set; }                   // /nologo
    public bool ShowHelp { get; set; }                 // /?, /help
    public bool ShowVersion { get; set; }              // /version
    public bool LspMode { get; set; }                  // --lsp
    public int WarningLevel { get; set; } = 1;         // /W0../W6
    public bool WarningsAsErrors { get; set; }         // /WX
    public bool NoWarnings { get; set; }               // /no_warn
    public string? TargetEnvironment { get; set; }     // /env win32|win64|x64|arm64
    public string? TargetSystem { get; set; }          // /target NT60..WIN11
    public List<string> Defines { get; } = new();      // /D
    public List<string> IncludePaths { get; } = new(); // /I
    public List<string> Undefines { get; } = new();    // /U
    public List<string> References { get; } = new();   // /reference (WinMD refs)
    public string? ResponseFile { get; set; }          // @file
    public bool Confirm { get; set; }                  // /confirm
    public bool NoHeader { get; set; }                 // /no_header
    public bool NoIid { get; set; }                    // /no_iid
    public bool NoProxy { get; set; }                  // /noproxy
    public bool NoDllData { get; set; }                // /no_dlldata
    public string? MetadataDir { get; set; }           // /metadata_dir
    public bool NoMidl { get; set; }                   // /nomidl
    public Midl.Analysis.CompilationMode Mode { get; set; } = Midl.Analysis.CompilationMode.Strict;
    public bool AnalyzeMode { get; set; }              // --analyze (rich console output)
    public bool DecompileMode { get; set; }             // --decompile (WinMD to IDL)
    public string? DecompileNamespace { get; set; }      // --namespace (filter decompile to one namespace)
    public bool DumpWinMd { get; set; }                 // --dump-winmd (dump WinMD metadata)
    public bool DebugTrace { get; set; }               // /debug (enable trace output)
    public bool NoDefIdir { get; set; }                // /no_def_idir — don't add default include dirs
    public bool Profile { get; set; }                  // /profile (emit timing data)
    public List<string> UnknownSwitches { get; } = new();
    public List<string> Errors { get; } = new();
}

/// <summary>
/// Parses MIDL-style command-line arguments into <see cref="CommandLineOptions"/>.
/// </summary>
public static class CommandLineParser
{
    public static CommandLineOptions Parse(string[] args)
    {
        var expanded = ExpandArgs(args);
        var options = new CommandLineOptions();
        int i = 0;

        while (i < expanded.Count)
        {
            var arg = expanded[i];

            if (arg.StartsWith('@'))
            {
                // Response file should have been expanded already; record the path
                options.ResponseFile ??= arg[1..];
                i++;
                continue;
            }

            if (arg is "--lsp")
            {
                options.LspMode = true;
                i++;
                continue;
            }

            if (arg is "--decompile")
            {
                options.DecompileMode = true;
                i++;
                continue;
            }

            if (arg is "--namespace" && i + 1 < args.Length)
            {
                options.DecompileNamespace = args[i + 1];
                i += 2;
                continue;
            }

            if (arg is "--dump-winmd")
            {
                options.DumpWinMd = true;
                i++;
                continue;
            }

            if (arg.StartsWith('/') || arg.StartsWith('-'))
            {
                var switchName = arg[1..];

                // Normalize: strip leading '-' if double-dash (e.g. --lsp already handled above)
                if (switchName.StartsWith('-'))
                    switchName = switchName[1..];

                // Handle concatenated single-char switches: /IC:\path → "I" + "C:\path"
                // /DFOO=1 → "D" + "FOO=1", /Ufoo → "U" + "foo"
                if (switchName.Length > 1)
                {
                    var firstChar = char.ToUpperInvariant(switchName[0]);
                    if (firstChar is 'I' or 'D' or 'U')
                    {
                        var rest = switchName[1..];
                        var asSwitch = switchName.ToLowerInvariant();
                        // Only split if it doesn't match a known multi-char switch
                        if (asSwitch is not ("iid" or "dlldata"))
                        {
                            expanded.Insert(i + 1, rest);
                            switchName = switchName[..1];
                        }
                    }
                }

                if (!TryParseSwitch(switchName, expanded, ref i, options))
                {
                    options.UnknownSwitches.Add(arg);
                    i++;
                }
            }
            else
            {
                // Positional argument → input file
                options.InputFile ??= arg;
                i++;
            }
        }

        return options;
    }

    private static bool TryParseSwitch(string switchName, List<string> args, ref int i, CommandLineOptions options)
    {
        switch (switchName.ToLowerInvariant())
        {
            case "out":
                if (TryConsumeValue(args, ref i, out var outDir))
                    options.OutputDirectory = outDir;
                else
                    options.Errors.Add("Missing value for /out");
                return true;

            case "h":
            case "header":
                if (TryConsumeValue(args, ref i, out var headerFile))
                    options.HeaderFile = headerFile;
                else
                    options.Errors.Add("Missing value for /h");
                return true;

            case "iid":
                if (TryConsumeValue(args, ref i, out var iidFile))
                    options.IidFile = iidFile;
                else
                    options.Errors.Add("Missing value for /iid");
                return true;

            case "proxy":
                if (TryConsumeValue(args, ref i, out var proxyFile))
                    options.ProxyFile = proxyFile;
                else
                    options.Errors.Add("Missing value for /proxy");
                return true;

            case "cppproxy":
                if (TryConsumeValue(args, ref i, out var cppProxyFile))
                    options.CppProxyFile = cppProxyFile;
                else
                    options.Errors.Add("Missing value for /cppproxy");
                return true;

            case "dlldata":
                if (TryConsumeValue(args, ref i, out var dllDataFile))
                    options.DllDataFile = dllDataFile;
                else
                    options.Errors.Add("Missing value for /dlldata");
                return true;

            case "winmd":
                if (TryConsumeValue(args, ref i, out var winmdFile))
                    options.WinMdFile = winmdFile;
                else
                    options.Errors.Add("Missing value for /winmd");
                return true;

            case "acf":
                if (TryConsumeValue(args, ref i, out var acfFile))
                    options.AcfFile = acfFile;
                else
                    options.Errors.Add("Missing value for /acf");
                return true;

#if WITH_COMPAT
            case "cpp_cmd":
                if (TryConsumeValue(args, ref i, out var cppCmd))
                    options.CppCommand = cppCmd;
                else
                    options.Errors.Add("Missing value for /cpp_cmd");
                return true;
#endif

            case "cpp_opt":
                if (TryConsumeValue(args, ref i, out var cppOpt))
                    options.CppOptions = cppOpt;
                else
                    options.Errors.Add("Missing value for /cpp_opt");
                return true;

            case "no_cpp":
                options.NoCpp = true;
                i++;
                return true;

            case "winrt":
                options.WinRtMode = true;
                i++;
                return true;

            case "zs":
            case "syntax_check":
                options.SyntaxCheckOnly = true;
                i++;
                return true;

            case "nologo":
                options.NoLogo = true;
                i++;
                return true;

            case "?":
            case "help":
                options.ShowHelp = true;
                i++;
                return true;

            case "version":
                options.ShowVersion = true;
                i++;
                return true;

            case "wx":
                options.WarningsAsErrors = true;
                i++;
                return true;

            case "no_warn":
                options.NoWarnings = true;
                i++;
                return true;

            case "w0":
            case "w1":
            case "w2":
            case "w3":
            case "w4":
            case "w5":
            case "w6":
                options.WarningLevel = switchName[^1] - '0';
                i++;
                return true;

            case "env":
                if (TryConsumeValue(args, ref i, out var env))
                    options.TargetEnvironment = env;
                else
                    options.Errors.Add("Missing value for /env");
                return true;

            case "target":
                if (TryConsumeValue(args, ref i, out var target))
                    options.TargetSystem = target;
                else
                    options.Errors.Add("Missing value for /target");
                return true;

            case "d":
                if (TryConsumeValue(args, ref i, out var define))
                    options.Defines.Add(define);
                else
                    options.Errors.Add("Missing value for /D");
                return true;

            case "i":
                if (TryConsumeValue(args, ref i, out var incPath))
                    options.IncludePaths.Add(incPath);
                else
                    options.Errors.Add("Missing value for /I");
                return true;

            case "u":
                if (TryConsumeValue(args, ref i, out var undef))
                    options.Undefines.Add(undef);
                else
                    options.Errors.Add("Missing value for /U");
                return true;

            case "reference":
                if (TryConsumeValue(args, ref i, out var refPath))
                    options.References.Add(refPath);
                else
                    options.Errors.Add("Missing value for /reference");
                return true;

            case "confirm":
                options.Confirm = true;
                i++;
                return true;

            case "no_header":
                options.NoHeader = true;
                i++;
                return true;

            case "no_iid":
                options.NoIid = true;
                i++;
                return true;

            case "noproxy":
                options.NoProxy = true;
                i++;
                return true;

            case "no_dlldata":
                options.NoDllData = true;
                i++;
                return true;

            case "metadata_dir":
                if (TryConsumeValue(args, ref i, out var metaDir))
                    options.MetadataDir = metaDir;
                else
                    options.Errors.Add("Missing value for /metadata_dir");
                return true;

            case "nomidl":
                options.NoMidl = true;
                i++;
                return true;

            case "strict":
                options.Mode = Midl.Analysis.CompilationMode.Strict;
                i++;
                return true;

#if WITH_COMPAT
            case "compat":
            case "no_strict":
                options.Mode = Midl.Analysis.CompilationMode.Compat;
                i++;
                return true;
#endif

            case "analyze":
                options.AnalyzeMode = true;
                options.SyntaxCheckOnly = true; // analyze implies no codegen
                i++;
                return true;

            case "decompile":
                options.DecompileMode = true;
                i++;
                return true;

            case "dump-winmd":
                options.DumpWinMd = true;
                i++;
                return true;

            case "debug":
                options.DebugTrace = true;
                i++;
                return true;
            case "profile":
                options.Profile = true;
                i++;
                return true;

            case "no_def_idir":
                options.NoDefIdir = true;
                i++;
                return true;

            default:
                return false;
        }
    }

    private static bool TryConsumeValue(List<string> args, ref int i, out string value)
    {
        if (i + 1 < args.Count && !args[i + 1].StartsWith('/') && !args[i + 1].StartsWith('-'))
        {
            i += 2;
            value = args[i - 1];
            return true;
        }

        // Check for value joined with '=' (e.g., /out=dir)
        value = string.Empty;
        i++;
        return false;
    }

    private static List<string> ExpandArgs(string[] args)
    {
        var result = new List<string>();
        foreach (var arg in args)
        {
            if (arg.StartsWith('@'))
            {
                var filePath = arg[1..];
                if (File.Exists(filePath))
                {
                    var lines = File.ReadAllText(filePath);
                    var fileArgs = SplitResponseFileContent(lines);
                    // Record that we expanded a response file (we'll also keep the @token)
                    result.Add(arg);
                    result.AddRange(fileArgs);
                }
                else
                {
                    // Keep it so we can report the path; error handled later
                    result.Add(arg);
                }
            }
            else
            {
                result.Add(arg);
            }
        }
        return result;
    }

    private static List<string> SplitResponseFileContent(string content)
    {
        var tokens = new List<string>();
        int pos = 0;
        while (pos < content.Length)
        {
            // Skip whitespace
            while (pos < content.Length && char.IsWhiteSpace(content[pos]))
                pos++;

            if (pos >= content.Length)
                break;

            if (content[pos] == '"')
            {
                // Quoted token
                pos++;
                int start = pos;
                while (pos < content.Length && content[pos] != '"')
                    pos++;
                tokens.Add(content[start..pos]);
                if (pos < content.Length) pos++; // skip closing quote
            }
            else
            {
                int start = pos;
                while (pos < content.Length && !char.IsWhiteSpace(content[pos]))
                    pos++;
                tokens.Add(content[start..pos]);
            }
        }
        return tokens;
    }
}
