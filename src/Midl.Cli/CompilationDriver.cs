using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.CodeGen.DllData;
using Midl.CodeGen.Header;
using Midl.CodeGen.Stub;
using Midl.CodeGen.WinMd;

using IidEmitter = Midl.CodeGen.Iid.IidEmitter;

namespace Midl.Cli;

/// <summary>
/// Orchestrates the full MIDL compilation pipeline.
/// </summary>
public class CompilationDriver
{
    public const string Version = "4.0.0";
    public const string Banner = $"Microsoft (R) MIDL Compiler Version {Version} (.NET 10 Rewrite)\nCopyright (c) Microsoft Corporation. All rights reserved.";

    private readonly TextWriter _out;
    private readonly TextWriter _err;

    public CompilationDriver() : this(Console.Out, Console.Error) { }

    public CompilationDriver(TextWriter stdOut, TextWriter stdErr)
    {
        _out = stdOut;
        _err = stdErr;
    }

    public int Run(CommandLineOptions options)
    {
        // Banner
        if (!options.NoLogo)
            _out.WriteLine(Banner);

        // Help
        if (options.ShowHelp)
        {
            PrintHelp();
            return 0;
        }

        // Version
        if (options.ShowVersion)
        {
            _out.WriteLine(Version);
            return 0;
        }

        // Parse errors
        if (options.Errors.Count > 0)
        {
            foreach (var err in options.Errors)
                _err.WriteLine($"error: {err}");
            return 2;
        }

        // Unknown switch warnings
        foreach (var sw in options.UnknownSwitches)
            _err.WriteLine($"warning MIDL2000: unknown switch '{sw}'");

        // Confirm mode — print effective options and exit
        if (options.Confirm)
        {
            PrintConfirm(options);
            return 0;
        }

        // Validate input
        if (string.IsNullOrEmpty(options.InputFile))
        {
            _err.WriteLine("error MIDL1000: no input file specified");
            return 2;
        }

        if (!File.Exists(options.InputFile))
        {
            _err.WriteLine($"error MIDL1001: cannot open input file '{options.InputFile}'");
            return 2;
        }

        // Read input (preprocessing skipped for now)
        var source = File.ReadAllText(options.InputFile);

        return RunFromSource(source, options);
    }

    /// <summary>
    /// Compile from source text directly (used for testing without file I/O).
    /// </summary>
    public int RunFromSource(string source, CommandLineOptions options)
    {
        // Lex → Parse → Bind
        Compilation compilation;
        try
        {
            compilation = CompilationHelper.Compile(source, options.References.Count > 0 ? options.References : null, options.Mode);
        }
        catch (Exception ex)
        {
            _err.WriteLine($"error MIDL1002: compilation failed: {ex.Message}");
            return 2;
        }

        // Analyze mode — rich console output with source context
        if (options.AnalyzeMode)
        {
            return RunAnalyzeMode(source, compilation, options);
        }

        // Report diagnostics (standard midl-compatible format)
        int errorCount = 0;
        int warningCount = 0;

        foreach (var diag in compilation.Diagnostics)
        {
            var prefix = diag.Severity switch
            {
                DiagnosticSeverity.Error => "error",
                DiagnosticSeverity.Warning => "warning",
                _ => "info"
            };

            if (diag.Severity == DiagnosticSeverity.Warning && options.NoWarnings)
                continue;

            if (diag.Severity == DiagnosticSeverity.Warning && options.WarningsAsErrors)
            {
                _err.WriteLine($"error {diag.Code}: {diag.Message}");
                errorCount++;
            }
            else
            {
                if (diag.Severity == DiagnosticSeverity.Error)
                {
                    _err.WriteLine($"{prefix} {diag.Code}: {diag.Message}");
                    errorCount++;
                }
                else if (diag.Severity == DiagnosticSeverity.Warning)
                {
                    _err.WriteLine($"{prefix} {diag.Code}: {diag.Message}");
                    warningCount++;
                }
            }
        }

        // Syntax check only
        if (options.SyntaxCheckOnly)
        {
            PrintSummary(errorCount, warningCount);
            return errorCount > 0 ? 2 : 0;
        }

        if (errorCount > 0)
        {
            PrintSummary(errorCount, warningCount);
            return 2;
        }

        // Generate outputs
        var outDir = options.OutputDirectory ?? ".";
        var baseName = options.InputFile != null
            ? Path.GetFileNameWithoutExtension(options.InputFile)
            : "output";
        var filesWritten = new List<string>();

        if (!options.NoHeader)
        {
            var headerEmitter = new HeaderEmitter();
            var headerContent = headerEmitter.Emit(compilation);
            var headerPath = options.HeaderFile ?? $"{baseName}.h";
            {
                var path = Path.Combine(outDir, headerPath);
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? outDir);
                File.WriteAllText(path, headerContent);
                filesWritten.Add(path);
            }
        }

        if (!options.NoIid)
        {
            var iidEmitter = new IidEmitter();
            var iidContent = iidEmitter.Emit(compilation);
            var iidPath = options.IidFile ?? $"{baseName}_i.c";
            {
                var path = Path.Combine(outDir, iidPath);
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? outDir);
                File.WriteAllText(path, iidContent);
                filesWritten.Add(path);
            }
        }

        if (options.WinRtMode)
        {
            var winmdEmitter = new WinMdEmitter();
            var winmdPath = options.WinMdFile ?? $"{baseName}.winmd";
            var path = Path.Combine(outDir, winmdPath);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? outDir);
            winmdEmitter.Emit(compilation, path);
            filesWritten.Add(path);
        }

        if (!options.NoDllData)
        {
            var dllDataEmitter = new DllDataEmitter();
            var dllDataContent = dllDataEmitter.Emit(compilation);
            if (options.DllDataFile != null)
            {
                var path = Path.Combine(outDir, options.DllDataFile);
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? outDir);
                File.WriteAllText(path, dllDataContent);
            }
        }

        if (!options.NoProxy)
        {
            var stubEmitter = new StubEmitter();
            var stubContent = stubEmitter.Emit(compilation);
            if (options.ProxyFile != null)
            {
                var path = Path.Combine(outDir, options.ProxyFile);
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? outDir);
                File.WriteAllText(path, stubContent);
            }
        }

        // Print what was generated
        if (filesWritten.Count > 0 && !options.NoLogo)
        {
            foreach (var f in filesWritten)
                _out.WriteLine($"  -> {f}");
        }

        PrintSummary(errorCount, warningCount);
        return warningCount > 0 ? 1 : 0;
    }

    /// <summary>
    /// Compile source and return generated header content (for testing without disk I/O).
    /// </summary>
    public static string? CompileToHeader(string source)
    {
        var compilation = CompilationHelper.Compile(source);
        if (compilation.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            return null;
        var emitter = new HeaderEmitter();
        return emitter.Emit(compilation);
    }

    private void PrintSummary(int errors, int warnings)
    {
        _out.WriteLine($"MIDL: {errors} error(s), {warnings} warning(s)");
    }

    private static int[] BuildLineStarts(string text)
    {
        var starts = new List<int> { 0 };
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
                starts.Add(i + 1);
        }
        return starts.ToArray();
    }

    private static (int line, int col) OffsetToLineCol(int[] lineStarts, int offset)
    {
        int line = Array.BinarySearch(lineStarts, offset);
        if (line < 0) line = ~line - 1;
        if (line < 0) line = 0;
        int col = offset - lineStarts[line];
        return (line, col);
    }

    /// <summary>
    /// Rich analyze mode: Rust/C#-style diagnostic output with source context and squiggly underlines.
    /// Usage: midl --analyze input.idl
    /// </summary>
    private int RunAnalyzeMode(string source, Compilation compilation, CommandLineOptions options)
    {
        var lineStarts = BuildLineStarts(source);
        var lines = source.Split('\n');
        int errorCount = 0;
        int warningCount = 0;

        if (compilation.Diagnostics.Count == 0)
        {
            _out.WriteLine("  \u2713 No diagnostics. File is valid MIDL3.");
            PrintSymbolSummary(compilation);
            return 0;
        }

        foreach (var diag in compilation.Diagnostics)
        {
            if (diag.Severity == DiagnosticSeverity.Warning && options.NoWarnings)
                continue;

            var (line, col) = OffsetToLineCol(lineStarts, diag.Position);
            var isError = diag.Severity == DiagnosticSeverity.Error ||
                         (diag.Severity == DiagnosticSeverity.Warning && options.WarningsAsErrors);

            if (isError) errorCount++; else warningCount++;

            // Header
            var label = isError ? "error" : "warning";
            _err.WriteLine($"{label}[{diag.Code}]: {diag.Message}");

            // Location
            var fileName = options.InputFile ?? "<stdin>";
            _err.WriteLine($"  --> {fileName}:{line + 1}:{col + 1}");

            // Source snippet
            if (line >= 0 && line < lines.Length)
            {
                var lineNum = (line + 1).ToString();
                var pad = new string(' ', lineNum.Length);
                var sourceLine = lines[line].TrimEnd('\r');

                _err.WriteLine($"  {pad} |");
                _err.WriteLine($"  {lineNum} | {sourceLine}");

                // Squiggly underline
                var spanLen = Math.Max(1, Math.Min(diag.Length, Math.Max(0, sourceLine.Length - col)));
                if (col >= 0 && col <= sourceLine.Length)
                {
                    var marker = new string(' ', col) + new string(isError ? '^' : '~', spanLen);
                    _err.WriteLine($"  {pad} | {marker}");
                }

                // Hint for fixable diagnostics
                if (diag.Data != null && diag.Data.TryGetValue("replacement", out var replacement))
                {
                    _err.WriteLine($"  {pad} = help: replace with `{replacement}`");
                }

                if (diag.HelpUri != null)
                {
                    _err.WriteLine($"  {pad} = see: {diag.HelpUri}");
                }
            }

            _err.WriteLine();
        }

        // Summary
        _err.Write("  ");
        if (errorCount > 0) _err.Write($"{errorCount} error(s)");
        if (errorCount > 0 && warningCount > 0) _err.Write(", ");
        if (warningCount > 0) _err.Write($"{warningCount} warning(s)");
        _err.WriteLine(" emitted");

        if (errorCount == 0)
            PrintSymbolSummary(compilation);

        return errorCount > 0 ? 2 : (warningCount > 0 ? 1 : 0);
    }

    private void PrintSymbolSummary(Compilation compilation)
    {
        var types = compilation.GetAllTypes().ToList();
        if (types.Count > 0)
        {
            _out.WriteLine();
            _out.WriteLine("  Symbols:");
            foreach (var t in types)
                _out.WriteLine($"    {t.Kind,-15} {t.FullName}");
        }
    }

    private void PrintConfirm(CommandLineOptions options)
    {
        _out.WriteLine("Effective options:");
        if (options.InputFile != null) _out.WriteLine($"  Input file  : {options.InputFile}");
        if (options.OutputDirectory != null) _out.WriteLine($"  Output dir  : {options.OutputDirectory}");
        if (options.HeaderFile != null) _out.WriteLine($"  Header file : {options.HeaderFile}");
        if (options.WinRtMode) _out.WriteLine("  WinRT mode  : yes");
        _out.WriteLine($"  MIDL3 mode  : {(options.Mode == Midl.Analysis.CompilationMode.Strict ? "strict" : "compat")}");
        if (options.SyntaxCheckOnly) _out.WriteLine("  Syntax only : yes");
        _out.WriteLine($"  Warning lvl : {options.WarningLevel}");
        if (options.Defines.Count > 0) _out.WriteLine($"  Defines     : {string.Join(", ", options.Defines)}");
        if (options.IncludePaths.Count > 0) _out.WriteLine($"  Includes    : {string.Join(", ", options.IncludePaths)}");
        if (options.References.Count > 0) _out.WriteLine($"  References  : {string.Join(", ", options.References)}");
    }

    private void PrintHelp()
    {
        _out.WriteLine();
        _out.WriteLine("Usage: midl [options] <input.idl>");
        _out.WriteLine();
        _out.WriteLine("Options:");
        _out.WriteLine("  /h, /header <file>     Specify header file name");
        _out.WriteLine("  /iid <file>            Specify IID file name");
        _out.WriteLine("  /proxy <file>          Specify proxy file name");
        _out.WriteLine("  /dlldata <file>        Specify DLL data file name");
        _out.WriteLine("  /winmd <file>          Specify WinMD output file");
        _out.WriteLine("  /out <directory>       Specify output directory");
        _out.WriteLine("  /acf <file>            Specify ACF file");
        _out.WriteLine("  /winrt                 Enable WinRT mode");
        _out.WriteLine("  /Zs, /syntax_check     Syntax check only");
        _out.WriteLine("  /D <name>[=value]      Define preprocessor symbol");
        _out.WriteLine("  /I <path>              Add include path");
        _out.WriteLine("  /U <name>              Undefine preprocessor symbol");
        _out.WriteLine("  /reference <file>      Add WinMD reference");
        _out.WriteLine("  /cpp_cmd <command>     Specify C preprocessor command");
        _out.WriteLine("  /cpp_opt <options>     Specify C preprocessor options");
        _out.WriteLine("  /no_cpp                Do not run C preprocessor");
        _out.WriteLine("  /env <target>          Target environment (win32|win64|x64|arm64)");
        _out.WriteLine("  /target <system>       Target system (NT60..WIN11)");
        _out.WriteLine("  /W0../W6               Set warning level (default: 1)");
        _out.WriteLine("  /WX                    Treat warnings as errors");
        _out.WriteLine("  /no_warn               Suppress all warnings");
        _out.WriteLine("  /nologo                Suppress banner");
        _out.WriteLine("  /confirm               Print effective options and exit");
        _out.WriteLine("  /no_header             Do not generate header file");
        _out.WriteLine("  /no_iid                Do not generate IID file");
        _out.WriteLine("  /noproxy               Do not generate proxy file");
        _out.WriteLine("  /no_dlldata            Do not generate DLL data file");
        _out.WriteLine("  /metadata_dir <dir>    Specify metadata directory");
        _out.WriteLine("  /nomidl                Do not invoke MIDL");
        _out.WriteLine("  /strict                Pure MIDL3 mode (default) — warn on classic constructs");
        _out.WriteLine("  /no_strict, /compat    Compatibility mode — accept classic MIDL syntax");
        _out.WriteLine("  /version               Print version");
        _out.WriteLine("  /?, /help              Show this help");
        _out.WriteLine("  --lsp                  Start Language Server Protocol mode");
        _out.WriteLine("  --analyze              Rich analysis output with source context");
        _out.WriteLine("  @<file>                Read options from response file");
    }
}
