using System.Text;
using System.Text.RegularExpressions;

namespace Midl.Lexer;

/// <summary>
/// Lightweight C preprocessor subset that runs before the lexer.
/// Handles #define, #undef, #ifdef/#ifndef/#if/#elif/#else/#endif,
/// #include, #pragma, and simple macro substitution.
/// </summary>
public class Preprocessor
{
    private readonly Dictionary<string, string> _defines = new(StringComparer.Ordinal);
    private readonly Dictionary<string, FunctionMacro> _functionMacros = new(StringComparer.Ordinal);
    private readonly List<string> _includePaths = new();
    private readonly string? _currentFilePath;
    private readonly HashSet<string> _pragmaOnceFiles = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _includeStack = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Regex> _macroRegexCache = new(StringComparer.Ordinal);

    private static readonly Regex s_definedParenRegex = new(@"defined\s*\(\s*(\w+)\s*\)", RegexOptions.Compiled);
    private static readonly Regex s_definedSpaceRegex = new(@"defined\s+(\w+)", RegexOptions.Compiled);

    /// <summary>True if any #include directive was encountered during processing.</summary>
    public bool HasIncludeDirectives { get; private set; }

    /// <summary>Errors from preprocessing (#error, unresolved includes, etc.).</summary>
    public List<string> Errors { get; } = new();

    /// <summary>Warnings from preprocessing (#warning directives).</summary>
    public List<string> Warnings { get; } = new();

    private record FunctionMacro(string[] Params, string Body, bool IsVariadic = false);

    public Preprocessor(IReadOnlyList<string>? defines = null,
                        IReadOnlyList<string>? includePaths = null,
                        string? currentFilePath = null)
    {
        _currentFilePath = currentFilePath;

        if (includePaths != null)
        {
            _includePaths.AddRange(includePaths);
        }

        // Pre-defined macros
        _defines["NTDDI_VERSION"] = "0x0A000000";
        _defines["NTDDI_WIN10"] = "0x0A000000";
        _defines["NTDDI_WIN8"] = "0x06020000";
        _defines["NTDDI_WINBLUE"] = "0x06030000";
        _defines["NTDDI_WIN10_RS1"] = "0x0A000100";
        _defines["_WIN32"] = "1";
        _defines["_WIN64"] = "1";
        _defines["__midl"] = "1";
        _defines["MIDL_PASS"] = "1";
        _defines["_MSC_VER"] = "1900";
        _defines["_WIN32_WINNT"] = "0x0A00";
        _defines["WINVER"] = "0x0A00";
        // Don't predefine WINAPI_FAMILY — let winapifamily.h define it
        // with the correct WINAPI_FAMILY_DESKTOP_APP constant (100)

        // Apply CLI /D defines (override pre-defined if needed)
        if (defines != null)
        {
            foreach (var d in defines)
            {
                var eqIndex = d.IndexOf('=');
                if (eqIndex >= 0)
                {
                    _defines[d[..eqIndex]] = d[(eqIndex + 1)..];
                }
                else
                {
                    _defines[d] = "1";
                }
            }
        }
    }

    public string Process(string source)
    {
        return ProcessInternal(source, _currentFilePath);
    }

#if WITH_COMPAT
    /// <summary>
    /// Preprocess a file using an external C preprocessor (e.g. cl.exe /E).
    /// This gives full C preprocessor semantics including #if expressions.
    /// </summary>
    public static string? ProcessWithExternalCpp(string filePath,
        IReadOnlyList<string>? includePaths = null,
        IReadOnlyList<string>? defines = null,
        string cppCmd = "cl.exe")
    {
        var args = new System.Text.StringBuilder();
        // /TP = treat as C++ (so .idl extension works), /E = preprocess to stdout
        args.Append("/E /nologo /c /TP ");

        // Always define MIDL/WinRT macros
        // DO_NO_IMPORTS: skip import blocks in SDK headers (our parser handles imports)
        args.Append("/D__midl /DMIDL_PASS /D_WIN32 /D_WIN64 ");
        args.Append("/DNTDDI_VERSION=0x0A000000 /D_APISET_WINDOWS_VERSION=0x0602 ");
        args.Append("/DUNICODE /D_UNICODE ");

        if (defines != null)
        {
            foreach (var d in defines)
                args.Append($"/D{d} ");
        }

        if (includePaths != null)
        {
            foreach (var p in includePaths)
                args.Append($"/I\"{p}\" ");
        }

        args.Append($"\"{filePath}\"");

        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = cppCmd,
                Arguments = args.ToString(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using var proc = System.Diagnostics.Process.Start(psi);
            if (proc == null) return null;

            var output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit(30_000);

            if (proc.ExitCode != 0) return null;

            // cl.exe /E outputs #line directives — strip them
            var sb = new System.Text.StringBuilder(output.Length);
            foreach (var line in output.Split('\n'))
            {
                var trimmed = line.TrimStart();
                if (trimmed.StartsWith("#line") || trimmed.StartsWith("#pragma"))
                    continue;
                sb.AppendLine(line);
            }
            return sb.ToString();
        }
        catch
        {
            return null; // cl.exe not found or failed
        }
    }
#endif

    private string ProcessInternal(string source, string? filePath)
    {
        var lines = SplitLines(source);

        // Join backslash-continuation lines: lines ending with \ are joined with the next line
        lines = JoinContinuationLines(lines);

        var output = new StringBuilder(source.Length);
        var ifStack = new Stack<IfState>();

        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var trimmed = line.TrimStart();

            if (trimmed.StartsWith('#'))
            {
                var directive = ParseDirective(trimmed);

                switch (directive.Name)
                {
                    case "define":
                        if (IsActive(ifStack))
                            HandleDefine(directive.Body);
                        output.AppendLine();
                        break;

                    case "undef":
                        if (IsActive(ifStack))
                            HandleUndef(directive.Body);
                        output.AppendLine();
                        break;

                    case "ifdef":
                        HandleIfdef(ifStack, directive.Body, negate: false);
                        output.AppendLine();
                        break;

                    case "ifndef":
                        HandleIfdef(ifStack, directive.Body, negate: true);
                        output.AppendLine();
                        break;

                    case "if":
                        HandleIf(ifStack, directive.Body);
                        output.AppendLine();
                        break;

                    case "elif":
                        HandleElif(ifStack, directive.Body);
                        output.AppendLine();
                        break;

                    case "else":
                        HandleElse(ifStack);
                        output.AppendLine();
                        break;

                    case "endif":
                        HandleEndif(ifStack);
                        output.AppendLine();
                        break;

                    case "include":
                        if (IsActive(ifStack))
                        {
                            var included = HandleInclude(directive.Body, filePath);
                            if (included != null)
                                output.Append(included);
                            else
                                output.AppendLine();
                        }
                        else
                        {
                            output.AppendLine();
                        }
                        break;

                    case "pragma":
                        if (IsActive(ifStack))
                            HandlePragma(directive.Body, filePath, output);
                        else
                            output.AppendLine();
                        break;

                    case "error":
                        if (IsActive(ifStack))
                            Errors.Add($"#error {directive.Body.Trim()}");
                        output.AppendLine();
                        break;

                    case "warning":
                        // #warning is informational — just drop the line
                        output.AppendLine();
                        break;

                    default:
                        // Unknown directive — report error and drop
                        if (IsActive(ifStack))
                            Errors.Add($"Unknown preprocessor directive: '#{directive.Name}'");
                        output.AppendLine();
                        break;
                }
            }
            else
            {
                if (IsActive(ifStack))
                {
                    output.AppendLine(SubstituteMacros(line));
                }
                else
                {
                    output.AppendLine();
                }
            }
        }

        // Remove trailing newline added by final AppendLine
        if (output.Length >= Environment.NewLine.Length)
        {
            output.Length -= Environment.NewLine.Length;
        }

        return output.ToString();
    }

    private void HandleDefine(string rest)
    {
        if (string.IsNullOrWhiteSpace(rest)) return;

        // Check for function-like macro: NAME(param1, param2) body
        var parenIdx = rest.IndexOf('(');
        var spaceIdx = rest.IndexOfAny([' ', '\t']);

        if (parenIdx > 0 && (spaceIdx < 0 || parenIdx < spaceIdx))
        {
            // Function-like macro
            var name = rest[..parenIdx].Trim();
            var closeIdx = rest.IndexOf(')', parenIdx);
            if (closeIdx > parenIdx)
            {
                var paramStr = rest[(parenIdx + 1)..closeIdx];
                var parameters = paramStr.Split(',')
                    .Select(p => p.Trim())
                    .Where(p => p.Length > 0)
                    .ToArray();
                var body = rest[(closeIdx + 1)..].Trim();

                // Detect variadic macro: last param is "..."
                bool isVariadic = parameters.Length > 0 && parameters[^1] == "...";
                if (isVariadic)
                {
                    // Remove "..." from the parameter list; variadic args are accessed via __VA_ARGS__
                    parameters = parameters[..^1];
                }
                _functionMacros[name] = new FunctionMacro(parameters, body, isVariadic);
                return;
            }
        }

        // Simple object-like macro
        if (spaceIdx < 0)
        {
            _defines[rest.Trim()] = "1";
        }
        else
        {
            var name = rest[..spaceIdx].Trim();
            var value = rest[(spaceIdx + 1)..].Trim();
            // Strip inline comments: 100 /* comment */ → 100
            var commentIdx = value.IndexOf("/*");
            if (commentIdx >= 0)
                value = value[..commentIdx].Trim();
            var lineCommentIdx = value.IndexOf("//");
            if (lineCommentIdx >= 0)
                value = value[..lineCommentIdx].Trim();
            _defines[name] = value;
        }
    }

    private void HandleUndef(string rest)
    {
        var name = rest.Trim();
        if (!string.IsNullOrEmpty(name))
        {
            _defines.Remove(name);
            _functionMacros.Remove(name);
        }
    }

    private void HandleIfdef(Stack<IfState> ifStack, string rest, bool negate)
    {
        var parentActive = IsActive(ifStack);
        var name = rest.Trim();
        var defined = _defines.ContainsKey(name);
        var condition = negate ? !defined : defined;
        ifStack.Push(new IfState
        {
            Active = parentActive && condition,
            ParentActive = parentActive,
            BranchTaken = parentActive && condition
        });
    }

    private void HandleIf(Stack<IfState> ifStack, string rest)
    {
        var parentActive = IsActive(ifStack);
        bool condition;
        if (parentActive)
        {
            condition = EvaluateExpression(rest.Trim());
        }
        else
        {
            condition = false;
        }
        ifStack.Push(new IfState
        {
            Active = parentActive && condition,
            ParentActive = parentActive,
            BranchTaken = parentActive && condition
        });
    }

    private void HandleElif(Stack<IfState> ifStack, string rest)
    {
        if (ifStack.Count == 0) return;

        var state = ifStack.Peek();
        if (state.BranchTaken || !state.ParentActive)
        {
            // A branch was already taken, or parent is inactive
            state.Active = false;
        }
        else
        {
            var condition = EvaluateExpression(rest.Trim());
            state.Active = condition;
            if (condition) state.BranchTaken = true;
        }
    }

    private void HandleElse(Stack<IfState> ifStack)
    {
        if (ifStack.Count == 0) return;

        var state = ifStack.Peek();
        if (state.BranchTaken || !state.ParentActive)
        {
            state.Active = false;
        }
        else
        {
            state.Active = true;
            state.BranchTaken = true;
        }
    }

    private void HandleEndif(Stack<IfState> ifStack)
    {
        if (ifStack.Count > 0)
            ifStack.Pop();
    }

    private string? HandleInclude(string rest, string? parentFilePath)
    {
        HasIncludeDirectives = true;

        var path = ExtractIncludePath(rest.Trim());
        if (path == null) return null;

        var resolved = ResolveIncludePath(path, parentFilePath);
        if (resolved == null)
        {
            Errors.Add($"#include file not found: '{path}'");
            return "\n";
        }

        var fullPath = Path.GetFullPath(resolved);

        // #pragma once check
        if (_pragmaOnceFiles.Contains(fullPath))
            return null;

        // Infinite recursion check
        if (_includeStack.Contains(fullPath))
            return "\n";

        _includeStack.Add(fullPath);
        try
        {
            var content = File.ReadAllText(resolved);
            var processed = ProcessInternal(content, resolved);
            if (!processed.EndsWith('\n'))
                return processed + "\n";
            return processed;
        }
        catch
        {
            return "\n";
        }
        finally
        {
            _includeStack.Remove(fullPath);
        }
    }

    private void HandlePragma(string rest, string? filePath, StringBuilder output)
    {
        var trimmed = rest.Trim();
        if (trimmed.Equals("once", StringComparison.OrdinalIgnoreCase))
        {
            if (filePath != null)
                _pragmaOnceFiles.Add(Path.GetFullPath(filePath));
            output.AppendLine();
        }
        else
        {
            // Unknown pragma — drop the directive line
            output.AppendLine();
        }
    }

    private static string? ExtractIncludePath(string rest)
    {
        // #include "file" or #include <file>
        if (rest.Length < 2) return null;

        if (rest[0] == '"')
        {
            var end = rest.IndexOf('"', 1);
            return end > 1 ? rest[1..end] : null;
        }
        if (rest[0] == '<')
        {
            var end = rest.IndexOf('>', 1);
            return end > 1 ? rest[1..end] : null;
        }
        return null;
    }

    private string? ResolveIncludePath(string path, string? parentFilePath)
    {
        // 1. Relative to current/parent file
        if (parentFilePath != null)
        {
            var dir = Path.GetDirectoryName(parentFilePath);
            if (dir != null)
            {
                var candidate = Path.Combine(dir, path);
                if (File.Exists(candidate)) return candidate;
            }
        }

        // 2. Search include paths
        foreach (var incDir in _includePaths)
        {
            var candidate = Path.Combine(incDir, path);
            if (File.Exists(candidate)) return candidate;
        }

        return null;
    }

    private Regex GetMacroRegex(string name)
    {
        if (!_macroRegexCache.TryGetValue(name, out var regex))
        {
            regex = new Regex(@"(?<!\w)" + Regex.Escape(name) + @"(?!\w)", RegexOptions.Compiled);
            _macroRegexCache[name] = regex;
        }
        return regex;
    }

    private string SubstituteMacros(string line)
    {
        if (_defines.Count == 0 && _functionMacros.Count == 0) return line;

        // Expand function-like macros first: NAME(arg1, arg2)
        foreach (var (name, macro) in _functionMacros)
        {
            int idx;
            while ((idx = line.IndexOf(name + "(", StringComparison.Ordinal)) >= 0)
            {
                // Verify it's a whole-word match
                if (idx > 0 && IsIdentChar(line[idx - 1])) break;

                var argsStart = idx + name.Length + 1;
                var argsEnd = FindMatchingParen(line, argsStart - 1);
                if (argsEnd < 0) break;

                var argsStr = line[argsStart..argsEnd];
                var args = SplitMacroArgs(argsStr);

                // Substitute parameters in body
                var expanded = macro.Body;

                // Process # (stringize) operator before regular substitution.
                // Match standalone # followed by a parameter name, but not ## (token paste).
                for (int p = 0; p < macro.Params.Length && p < args.Length; p++)
                {
                    var stringizePattern = new Regex(
                        @"(?<!#)#(?!#)(\s*)" + Regex.Escape(macro.Params[p]) + @"(?!\w)");
                    expanded = stringizePattern.Replace(expanded,
                        "\"" + args[p].Trim().Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"");
                }

                for (int p = 0; p < macro.Params.Length && p < args.Length; p++)
                {
                    expanded = GetMacroRegex(macro.Params[p]).Replace(expanded, args[p].Trim());
                }

                // Handle variadic __VA_ARGS__ substitution
                if (macro.IsVariadic)
                {
                    var vaArgs = "";
                    if (args.Length > macro.Params.Length)
                    {
                        vaArgs = string.Join(", ", args.Skip(macro.Params.Length).Select(a => a.Trim()));
                    }
                    else if (macro.Params.Length == 0 && args.Length == 1 && string.IsNullOrWhiteSpace(args[0]))
                    {
                        // ALL() with empty parens — zero variadic args
                        vaArgs = "";
                    }
                    expanded = GetMacroRegex("__VA_ARGS__").Replace(expanded, vaArgs);
                }

                // Process ## token-paste operator after parameter substitution
                expanded = ProcessTokenPaste(expanded);

                line = line[..idx] + expanded + line[(argsEnd + 1)..];
            }
        }

        // Simple object-like macros — loop until fixpoint for recursive expansion
        // (e.g. WINAPI_FAMILY → WINAPI_FAMILY_DESKTOP_APP → 100)
        for (int pass = 0; pass < 10; pass++)
        {
            var prev = line;
            foreach (var (name, value) in _defines)
            {
                if (line.Contains(name, StringComparison.Ordinal))
                {
                    line = GetMacroRegex(name).Replace(line, value);
                }
            }
            // Process ## token-paste operator after macro substitution
            line = ProcessTokenPaste(line);
            if (line == prev) break; // No more substitutions
        }
        return line;
    }

    private static readonly Regex TokenPasteRegex = new(@"\s*##\s*", RegexOptions.Compiled);

    private static string ProcessTokenPaste(string text)
    {
        return TokenPasteRegex.Replace(text, string.Empty);
    }

    private static int FindMatchingParen(string text, int openIdx)
    {
        int depth = 0;
        for (int i = openIdx; i < text.Length; i++)
        {
            if (text[i] == '(') depth++;
            else if (text[i] == ')')
            {
                depth--;
                if (depth == 0) return i;
            }
        }
        return -1;
    }

    private static string[] SplitMacroArgs(string argsStr)
    {
        var args = new List<string>();
        int depth = 0;
        int start = 0;
        for (int i = 0; i < argsStr.Length; i++)
        {
            if (argsStr[i] == '(') depth++;
            else if (argsStr[i] == ')') depth--;
            else if (argsStr[i] == ',' && depth == 0)
            {
                args.Add(argsStr[start..i]);
                start = i + 1;
            }
        }
        args.Add(argsStr[start..]);
        return args.ToArray();
    }

    private bool EvaluateExpression(string expr)
    {
        if (string.IsNullOrWhiteSpace(expr)) return true;

        expr = expr.Trim();

        // Remove single-line comments
        var commentIdx = expr.IndexOf("//");
        if (commentIdx >= 0) expr = expr[..commentIdx].Trim();

        // Handle defined(NAME) / defined NAME
        expr = s_definedParenRegex.Replace(expr, m =>
            (_defines.ContainsKey(m.Groups[1].Value) || _functionMacros.ContainsKey(m.Groups[1].Value)) ? "1" : "0");
        expr = s_definedSpaceRegex.Replace(expr, m =>
            (_defines.ContainsKey(m.Groups[1].Value) || _functionMacros.ContainsKey(m.Groups[1].Value)) ? "1" : "0");

        // Substitute macros in the expression
        expr = SubstituteMacros(expr);

        // Try to evaluate the expression
        try
        {
            var result = EvalExpr(expr.Trim());
            return result != 0;
        }
        catch
        {
            // For complex expressions we can't parse, default to true
            return true;
        }
    }

    /// <summary>
    /// Simple recursive descent expression evaluator supporting:
    /// ||, &&, ==, !=, <, >, <=, >=, +, -, *, /, !, ~, parentheses, hex/dec literals
    /// </summary>
    private long EvalExpr(string expr)
    {
        var pos = 0;
        var result = ParseOr(expr, ref pos);
        return result;
    }

    private long ParseOr(string expr, ref int pos)
    {
        var left = ParseAnd(expr, ref pos);
        SkipWhitespace(expr, ref pos);
        while (pos < expr.Length - 1 && expr[pos] == '|' && expr[pos + 1] == '|')
        {
            pos += 2;
            var right = ParseAnd(expr, ref pos);
            left = (left != 0 || right != 0) ? 1 : 0;
            SkipWhitespace(expr, ref pos);
        }
        return left;
    }

    private long ParseAnd(string expr, ref int pos)
    {
        var left = ParseEquality(expr, ref pos);
        SkipWhitespace(expr, ref pos);
        while (pos < expr.Length - 1 && expr[pos] == '&' && expr[pos + 1] == '&')
        {
            pos += 2;
            var right = ParseEquality(expr, ref pos);
            left = (left != 0 && right != 0) ? 1 : 0;
            SkipWhitespace(expr, ref pos);
        }
        return left;
    }

    private long ParseEquality(string expr, ref int pos)
    {
        var left = ParseComparison(expr, ref pos);
        SkipWhitespace(expr, ref pos);
        while (pos < expr.Length - 1)
        {
            if (expr[pos] == '=' && expr[pos + 1] == '=')
            {
                pos += 2;
                var right = ParseComparison(expr, ref pos);
                left = left == right ? 1 : 0;
                SkipWhitespace(expr, ref pos);
            }
            else if (expr[pos] == '!' && expr[pos + 1] == '=')
            {
                pos += 2;
                var right = ParseComparison(expr, ref pos);
                left = left != right ? 1 : 0;
                SkipWhitespace(expr, ref pos);
            }
            else break;
        }
        return left;
    }

    private long ParseComparison(string expr, ref int pos)
    {
        var left = ParseAddSub(expr, ref pos);
        SkipWhitespace(expr, ref pos);
        while (pos < expr.Length)
        {
            if (pos < expr.Length - 1 && expr[pos] == '>' && expr[pos + 1] == '=')
            {
                pos += 2;
                var right = ParseAddSub(expr, ref pos);
                left = left >= right ? 1 : 0;
                SkipWhitespace(expr, ref pos);
            }
            else if (pos < expr.Length - 1 && expr[pos] == '<' && expr[pos + 1] == '=')
            {
                pos += 2;
                var right = ParseAddSub(expr, ref pos);
                left = left <= right ? 1 : 0;
                SkipWhitespace(expr, ref pos);
            }
            else if (expr[pos] == '>')
            {
                pos++;
                var right = ParseAddSub(expr, ref pos);
                left = left > right ? 1 : 0;
                SkipWhitespace(expr, ref pos);
            }
            else if (expr[pos] == '<')
            {
                pos++;
                var right = ParseAddSub(expr, ref pos);
                left = left < right ? 1 : 0;
                SkipWhitespace(expr, ref pos);
            }
            else break;
        }
        return left;
    }

    private long ParseAddSub(string expr, ref int pos)
    {
        var left = ParseMulDiv(expr, ref pos);
        SkipWhitespace(expr, ref pos);
        while (pos < expr.Length)
        {
            if (expr[pos] == '+')
            {
                pos++;
                var right = ParseMulDiv(expr, ref pos);
                left += right;
                SkipWhitespace(expr, ref pos);
            }
            else if (expr[pos] == '-')
            {
                pos++;
                var right = ParseMulDiv(expr, ref pos);
                left -= right;
                SkipWhitespace(expr, ref pos);
            }
            else break;
        }
        return left;
    }

    private long ParseMulDiv(string expr, ref int pos)
    {
        var left = ParseUnary(expr, ref pos);
        SkipWhitespace(expr, ref pos);
        while (pos < expr.Length)
        {
            if (expr[pos] == '*')
            {
                pos++;
                var right = ParseUnary(expr, ref pos);
                left *= right;
                SkipWhitespace(expr, ref pos);
            }
            else if (expr[pos] == '/')
            {
                pos++;
                var right = ParseUnary(expr, ref pos);
                if (right != 0) left /= right;
                SkipWhitespace(expr, ref pos);
            }
            else break;
        }
        return left;
    }

    private long ParseUnary(string expr, ref int pos)
    {
        SkipWhitespace(expr, ref pos);
        if (pos < expr.Length && expr[pos] == '!')
        {
            pos++;
            var val = ParseUnary(expr, ref pos);
            return val == 0 ? 1 : 0;
        }
        if (pos < expr.Length && expr[pos] == '~')
        {
            pos++;
            var val = ParseUnary(expr, ref pos);
            return ~val;
        }
        return ParsePrimary(expr, ref pos);
    }

    private long ParsePrimary(string expr, ref int pos)
    {
        SkipWhitespace(expr, ref pos);
        if (pos >= expr.Length)
            throw new FormatException("Unexpected end of expression");

        // Parenthesized expression
        if (expr[pos] == '(')
        {
            pos++;
            var val = ParseOr(expr, ref pos);
            SkipWhitespace(expr, ref pos);
            if (pos < expr.Length && expr[pos] == ')')
                pos++;
            return val;
        }

        // Hex literal: 0x...
        if (pos < expr.Length - 1 && expr[pos] == '0' && (expr[pos + 1] == 'x' || expr[pos + 1] == 'X'))
        {
            pos += 2;
            var start = pos;
            while (pos < expr.Length && IsHexDigit(expr[pos]))
                pos++;
            if (pos == start)
                throw new FormatException("Invalid hex literal");
            return Convert.ToInt64(expr[start..pos], 16);
        }

        // Decimal literal
        if (expr[pos] >= '0' && expr[pos] <= '9')
        {
            var start = pos;
            while (pos < expr.Length && expr[pos] >= '0' && expr[pos] <= '9')
                pos++;
            // Skip trailing L/U suffixes
            while (pos < expr.Length && (expr[pos] == 'L' || expr[pos] == 'l' ||
                                          expr[pos] == 'U' || expr[pos] == 'u'))
                pos++;
            return long.Parse(expr[start..pos].TrimEnd('L', 'l', 'U', 'u'));
        }

        // Identifier — treat undefined identifiers as 0
        if (IsIdentChar(expr[pos]) && !(expr[pos] >= '0' && expr[pos] <= '9'))
        {
            var start = pos;
            while (pos < expr.Length && IsIdentChar(expr[pos]))
                pos++;
            var ident = expr[start..pos];
            if (_defines.TryGetValue(ident, out var val))
            {
                // Recursively evaluate the macro value
                try { return EvalExpr(val.Trim()); }
                catch { return 0; }
            }
            return 0;
        }

        throw new FormatException($"Unexpected character '{expr[pos]}' at position {pos}");
    }

    private static void SkipWhitespace(string s, ref int pos)
    {
        while (pos < s.Length && (s[pos] == ' ' || s[pos] == '\t'))
            pos++;
    }

    private static bool IsHexDigit(char c) =>
        (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');

    private static bool IsIdentChar(char c) =>
        (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_';

    private static bool IsActive(Stack<IfState> ifStack) =>
        ifStack.Count == 0 || ifStack.Peek().Active;

    private static List<string> SplitLines(string text)
    {
        var lines = new List<string>();
        int start = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\r')
            {
                lines.Add(text[start..i]);
                if (i + 1 < text.Length && text[i + 1] == '\n')
                    i++;
                start = i + 1;
            }
            else if (text[i] == '\n')
            {
                lines.Add(text[start..i]);
                start = i + 1;
            }
        }
        lines.Add(text[start..]);
        return lines;
    }

    /// <summary>Join lines ending with backslash (C line continuation).</summary>
    private static List<string> JoinContinuationLines(List<string> lines)
    {
        var result = new List<string>(lines.Count);
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var trimmedEnd = line.TrimEnd();
            // Don't apply line continuation inside comments (// @ref: D:\path)
            var trimmedStart = trimmedEnd.TrimStart();
            if (trimmedEnd.EndsWith('\\') && !trimmedStart.StartsWith("//"))
            {
                // Strip the backslash and join with next line
                sb.Append(trimmedEnd[..^1]);
                sb.Append(' ');
            }
            else
            {
                sb.Append(line);
                result.Add(sb.ToString());
                sb.Clear();
            }
        }
        if (sb.Length > 0)
            result.Add(sb.ToString());
        return result;
    }

    private static (string Name, string Body) ParseDirective(string trimmedLine)
    {
        // trimmedLine starts with '#'
        var afterHash = trimmedLine[1..].TrimStart();
        var spaceIdx = afterHash.IndexOfAny([' ', '\t']);
        if (spaceIdx < 0)
            return (afterHash.Trim(), string.Empty);
        return (afterHash[..spaceIdx], afterHash[(spaceIdx + 1)..]);
    }

    private class IfState
    {
        public bool Active;      // Is the current branch active?
        public bool ParentActive; // Was the parent scope active?
        public bool BranchTaken;  // Has any branch in this if/elif/else been taken?
    }
}
