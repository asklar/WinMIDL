// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;

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

    /// <summary>True if any #include directive was encountered during processing.</summary>
    public bool HasIncludeDirectives { get; private set; }

    /// <summary>Positions of #include directives in the original source (for diagnostic squiggles).</summary>
    public List<int> IncludeDirectivePositions { get; } = new();

    /// <summary>
    /// Raw source-map segment data produced during preprocessing.
    /// Each tuple: (expandedStart, expandedEnd, filePath, originalStart).
    /// </summary>
    public List<(int ExpandedStart, int ExpandedEnd, string FilePath, int OriginalStart)> SourceMapSegments { get; } = new();

    /// <summary>Errors from preprocessing (#error, unresolved includes, etc.).</summary>
    public List<(string Message, int Position)> Errors { get; } = new();

    /// <summary>Warnings from preprocessing (#warning directives).</summary>
    public List<(string Message, int Position)> Warnings { get; } = new();

    /// <summary>Ranges of inactive preprocessor regions in the original source (start offset, end offset).</summary>
    public List<(int Start, int End)> InactiveRegions { get; } = new();

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

    /// <summary>
    /// The newline sequence a file uses.
    ///
    /// Positions map back to original offsets linearly within a source-map segment, so
    /// re-emitting a file's lines with a different newline than it was written with shifts
    /// every subsequent position by the difference — one character per line for LF source
    /// on Windows.
    /// </summary>
    private static string DetectNewline(string text)
    {
        int lf = text.IndexOf('\n');
        if (lf < 0) return Environment.NewLine;
        return (lf > 0 && text[lf - 1] == '\r') ? "\r\n" : "\n";
    }

    private string ProcessInternal(string source, string? filePath, int baseOffset = 0, bool isIncludedFile = false)
    {
        // Re-emit this file's lines with the newline it actually uses, not the platform's.
        string nl = DetectNewline(source);

        var rawLines = SplitLines(source);

        // Compute original character offsets for each raw line
        var rawLineOffsets = new int[rawLines.Count + 1];
        {
            int o = 0;
            for (int ri = 0; ri < rawLines.Count; ri++)
            {
                rawLineOffsets[ri] = o;
                o += rawLines[ri].Length;
                // Account for actual newline chars (\r\n = 2, \n = 1)
                if (o < source.Length && source[o] == '\r')
                    o++; // skip \r
                if (o < source.Length && source[o] == '\n')
                    o++; // skip \n
            }
            rawLineOffsets[rawLines.Count] = o;
        }

        // Join backslash-continuation lines and track original line indices
        var (lines, origLineIndices) = JoinContinuationLinesTracked(rawLines);

        var output = new StringBuilder(source.Length);
        var ifStack = new Stack<IfState>();

        // Source map tracking: current segment in the expanded output
        int segExpandedStart = baseOffset;
        int segOriginalStart = 0;
        bool hasIncludes = false;
        bool hasIfdefGaps = false;
        bool hasMacroDeltas = false;
        bool hasDirectiveGaps = false;
        bool wasActive = true; // track active/inactive transitions for #ifdef gap tracking
        int inactiveRegionStart = -1; // offset in original source where current inactive region began

        #pragma warning disable CS0219
        int macroDelta = 0;
        #pragma warning restore CS0219

        // Track /* ... */ block comments spanning multiple source lines.
        bool inBlockComment = false;

        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var trimmed = line.TrimStart();

            if (trimmed.StartsWith('#'))
            {
                var directive = ParseDirective(trimmed);
                bool isInclude = directive.Name.Equals("include", StringComparison.OrdinalIgnoreCase);

                // Directive lines produce an empty AppendLine() in expanded text but have
                // content in the original (e.g. "#define FOO bar"), breaking the source map's
                // linear mapping within a segment. Flush a segment boundary before the directive.
                // #include has its own segment handling inside its case.
                if (!isInclude && filePath != null && wasActive)
                {
                    int expandedPos = baseOffset + output.Length;
                    if (expandedPos > segExpandedStart)
                    {
                        SourceMapSegments.Add((segExpandedStart, expandedPos, filePath, segOriginalStart));
                    }
                }

                switch (directive.Name)
                {
                    case "define":
                        if (IsActive(ifStack))
                            HandleDefine(directive.Body);
                        output.Append(nl);
                        break;

                    case "undef":
                        if (IsActive(ifStack))
                            HandleUndef(directive.Body);
                        output.Append(nl);
                        break;

                    case "ifdef":
                        HandleIfdef(ifStack, directive.Body, negate: false);
                        output.Append(nl);
                        break;

                    case "ifndef":
                        HandleIfdef(ifStack, directive.Body, negate: true);
                        output.Append(nl);
                        break;

                    case "if":
                        HandleIf(ifStack, directive.Body);
                        output.Append(nl);
                        break;

                    case "elif":
                        HandleElif(ifStack, directive.Body);
                        output.Append(nl);
                        break;

                    case "else":
                        HandleElse(ifStack);
                        output.Append(nl);
                        break;

                    case "endif":
                        HandleEndif(ifStack);
                        output.Append(nl);
                        break;

                    case "include":
                        if (IsActive(ifStack))
                        {
                            int origIdx = origLineIndices[i];

                            // Record position for diagnostic squiggles — only for the
                            // user's file, not nested includes inside SDK headers.
                            if (!isIncludedFile)
                            {
                                IncludeDirectivePositions.Add(rawLineOffsets[origIdx]);
                            }

                            // Flush current segment before calling HandleInclude
                            // (HandleInclude's recursive ProcessInternal will add child segments first)
                            int expandedPos = baseOffset + output.Length;
                            bool flushed = false;
                            if (expandedPos > segExpandedStart && filePath != null)
                            {
                                SourceMapSegments.Add((segExpandedStart, expandedPos, filePath, segOriginalStart));
                                flushed = true;
                            }

                            var included = HandleInclude(directive.Body, filePath, expandedPos, rawLineOffsets[origIdx], nl);
                            if (included != null)
                            {
                                output.Append(included);
                                hasIncludes = true;

                                // Start new segment after include
                                segExpandedStart = baseOffset + output.Length;
                                if (i + 1 < origLineIndices.Length)
                                    segOriginalStart = rawLineOffsets[origLineIndices[i + 1]];
                                else
                                    segOriginalStart = rawLineOffsets[rawLines.Count];
                            }
                            else
                            {
                                output.Append(nl);
                                // Remove the prematurely flushed segment since nothing was included
                                if (flushed)
                                {
                                    var last = SourceMapSegments[^1];
                                    SourceMapSegments.RemoveAt(SourceMapSegments.Count - 1);
                                    segExpandedStart = last.ExpandedStart;
                                    segOriginalStart = last.OriginalStart;
                                }
                            }
                        }
                        else
                        {
                            output.Append(nl);
                        }
                        break;

                    case "pragma":
                        if (IsActive(ifStack))
                            HandlePragma(directive.Body, filePath, output, nl);
                        else
                            output.Append(nl);
                        break;

                    case "error":
                        if (IsActive(ifStack))
                            Errors.Add(($"#error {directive.Body.Trim()}", rawLineOffsets[origLineIndices[i]]));
                        output.Append(nl);
                        break;

                    case "warning":
                        // #warning is informational — just drop the line
                        output.Append(nl);
                        break;

                    default:
                        // Unknown directive — report error and drop
                        if (IsActive(ifStack))
                            Errors.Add(($"Unknown preprocessor directive: '#{directive.Name}'", rawLineOffsets[origLineIndices[i]]));
                        output.Append(nl);
                        break;
                }

                // Start new segment after directive line, pointing to next original line
                if (!isInclude && filePath != null && wasActive)
                {
                    segExpandedStart = baseOffset + output.Length;
                    int nextRaw = (i + 1 < origLineIndices.Length) ? origLineIndices[i + 1] : rawLines.Count;
                    segOriginalStart = rawLineOffsets[Math.Min(nextRaw, rawLines.Count)];
                    hasDirectiveGaps = true;
                }
            }
            else
            {
                if (IsActive(ifStack))
                {
                    var substituted = SubstituteMacrosPreservingComments(line, ref inBlockComment);
                    int lenDiff = line.Length - substituted.Length;
                    if (lenDiff != 0 && filePath != null)
                    {
                        // Macro substitution changed the line length.
                        // Flush the current segment up to this point (may be empty after #ifdef transition).
                        int expandedPos = baseOffset + output.Length;
                        if (expandedPos > segExpandedStart)
                            SourceMapSegments.Add((segExpandedStart, expandedPos, filePath, segOriginalStart));

                        // Emit the substituted line
                        output.Append(substituted).Append(nl);
                        hasMacroDeltas = true;

                        // Start a new segment AFTER this line.
                        segExpandedStart = baseOffset + output.Length;
                        int nextRaw = (i + 1 < origLineIndices.Length) ? origLineIndices[i + 1] : rawLines.Count;
                        segOriginalStart = rawLineOffsets[Math.Min(nextRaw, rawLines.Count)];
                    }
                    else
                    {
                        output.Append(substituted).Append(nl);
                    }
                }
                else
                {
                    output.Append(nl);
                }
            }

            // Backslash line continuation: if this logical line consumed multiple raw lines,
            // emit extra empty lines to preserve line count between original and expanded text.
            {
                int rawStart = origLineIndices[i];
                int rawEnd = (i + 1 < origLineIndices.Length) ? origLineIndices[i + 1] : rawLines.Count;
                int extraLines = rawEnd - rawStart - 1;
                for (int j = 0; j < extraLines; j++)
                    output.Append(nl);
            }

            // Track active/inactive transitions for #ifdef gap source mapping
            bool nowActive = IsActive(ifStack);
            if (wasActive && !nowActive)
            {
                // Record start of inactive region in original source
                int origIdx = origLineIndices[i];
                inactiveRegionStart = rawLineOffsets[origIdx];

                if (filePath != null)
                {
                    // Transitioning to inactive: flush current segment
                    int expandedPos = baseOffset + output.Length;
                    if (expandedPos > segExpandedStart)
                    {
                        SourceMapSegments.Add((segExpandedStart, expandedPos, filePath, segOriginalStart));
                        hasIfdefGaps = true;
                    }
                }
            }
            else if (!wasActive && nowActive)
            {
                int origIdx = (i + 1 < origLineIndices.Length) ? origLineIndices[i + 1] : rawLines.Count;

                // Record end of inactive region
                if (inactiveRegionStart >= 0)
                {
                    InactiveRegions.Add((inactiveRegionStart, rawLineOffsets[origIdx]));
                    inactiveRegionStart = -1;
                }

                if (filePath != null)
                {
                    // Transitioning to active: start a new segment
                    segExpandedStart = baseOffset + output.Length;
                    segOriginalStart = rawLineOffsets[origIdx];
                    hasIfdefGaps = true;
                }
            }
            wasActive = nowActive;
        }

        // Close any unclosed inactive region at end of file
        if (inactiveRegionStart >= 0)
        {
            InactiveRegions.Add((inactiveRegionStart, rawLineOffsets[rawLines.Count]));
        }

        // Remove the newline the final AppendLine added — but only the newline. The output
        // can end with a lone "\n" (HandleInclude normalises included content that way), so
        // subtracting Environment.NewLine.Length blindly would eat a real character: the
        // closing brace of a file ending in an #include, leaving it unterminated.
        if (output.Length > 0 && output[^1] == '\n')
        {
            output.Length -= (output.Length >= 2 && output[^2] == '\r') ? 2 : 1;
        }

        // Flush final segment
        // Record when: file had includes (need segments for content between them),
        // or this is a recursive call for an included file,
        // or there were #ifdef gaps that shifted positions
        bool needsSegments = hasIncludes || hasIfdefGaps || hasMacroDeltas || hasDirectiveGaps || isIncludedFile;
        if (needsSegments && filePath != null)
        {
            int finalExpandedPos = baseOffset + output.Length;
            if (finalExpandedPos > segExpandedStart)
            {
                SourceMapSegments.Add((segExpandedStart, finalExpandedPos, filePath, segOriginalStart));
            }
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
            // #define FOO (no value) — expands to empty string in code,
            // but #ifdef FOO / defined(FOO) still returns true (ContainsKey).
            _defines[rest.Trim()] = "";
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

    private string? HandleInclude(string rest, string? parentFilePath, int baseOffset, int originalPosition, string nl)
    {
        HasIncludeDirectives = true;

        var path = ExtractIncludePath(rest.Trim());
        if (path == null) return null;

        var resolved = ResolveIncludePath(path, parentFilePath);
        if (resolved == null)
        {
            Errors.Add(($"#include file not found: '{path}'", originalPosition));
            return nl;
        }

        var fullPath = Path.GetFullPath(resolved);

        // #pragma once check
        if (_pragmaOnceFiles.Contains(fullPath))
            return null;

        // Infinite recursion check
        if (_includeStack.Contains(fullPath))
            return nl;

        _includeStack.Add(fullPath);
        try
        {
            var content = File.ReadAllText(resolved);
            var processed = ProcessInternal(content, resolved, baseOffset, isIncludedFile: true);
            // Terminate with the same newline the rest of the output uses. Returning a
            // lone "\n" here is what made the caller's fixed-length newline strip eat a
            // real character; keeping the buffer internally consistent removes the trap
            // rather than relying on every consumer to survive it.
            if (!processed.EndsWith('\n'))
                return processed + nl;
            return processed;
        }
        catch
        {
            return nl;
        }
        finally
        {
            _includeStack.Remove(fullPath);
        }
    }

    private void HandlePragma(string rest, string? filePath, StringBuilder output, string nl)
    {
        var trimmed = rest.Trim();
        if (trimmed.Equals("once", StringComparison.OrdinalIgnoreCase))
        {
            if (filePath != null)
                _pragmaOnceFiles.Add(Path.GetFullPath(filePath));
            output.Append(nl);
        }
        else
        {
            // Unknown pragma — drop the directive line
            output.Append(nl);
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

    private static string ReplaceWholeWord(string text, string word, string replacement)
    {
        var sb = new StringBuilder(text.Length);
        int i = 0;
        while (i < text.Length)
        {
            int idx = text.IndexOf(word, i, StringComparison.Ordinal);
            if (idx < 0) { sb.Append(text, i, text.Length - i); break; }
            bool leftOk = idx == 0 || !IsIdentChar(text[idx - 1]);
            bool rightOk = idx + word.Length >= text.Length || !IsIdentChar(text[idx + word.Length]);
            sb.Append(text, i, idx - i);
            if (leftOk && rightOk) sb.Append(replacement);
            else sb.Append(text, idx, word.Length);
            i = idx + word.Length;
        }
        return sb.ToString();
    }

    /// <summary>
    /// Replace #paramName (stringize operator) — matches standalone # (not ##)
    /// followed by optional whitespace and the parameter name as a whole word.
    /// </summary>
    private static string ReplaceStringize(string text, string paramName, string replacement)
    {
        var sb = new StringBuilder(text.Length);
        int i = 0;
        while (i < text.Length)
        {
            if (text[i] == '#' && (i == 0 || text[i - 1] != '#'))
            {
                int j = i + 1;
                // not ## (token paste)
                if (j < text.Length && text[j] == '#')
                {
                    sb.Append(text[i]);
                    i++;
                    continue;
                }
                while (j < text.Length && text[j] == ' ') j++; // skip whitespace
                if (j + paramName.Length <= text.Length &&
                    text.AsSpan(j, paramName.Length).SequenceEqual(paramName.AsSpan()) &&
                    (j + paramName.Length >= text.Length || !IsIdentChar(text[j + paramName.Length])))
                {
                    sb.Append(replacement);
                    i = j + paramName.Length;
                    continue;
                }
            }
            sb.Append(text[i]);
            i++;
        }
        return sb.ToString();
    }

    /// <summary>
    /// Macro-expand a source line while preserving comments verbatim in the output.
    /// Comments are kept so that doc comments (///) survive for the lexer and column
    /// offsets remain correct. Only the code portions between comments are expanded.
    /// This prevents catastrophic expansion of SAL macros inside documentation comments
    /// in Windows SDK headers (specstrings.h allocated ~28 GB without this).
    /// </summary>
    private string SubstituteMacrosPreservingComments(string line, ref bool inBlockComment)
    {
        if (_defines.Count == 0 && _functionMacros.Count == 0) return line;

        var sb = new StringBuilder(line.Length);
        int i = 0;

        // If we're inside a block comment from a previous line, find the end
        if (inBlockComment)
        {
            int end = line.IndexOf("*/", StringComparison.Ordinal);
            if (end < 0)
                return line; // entire line is inside a block comment — emit as-is
            // Preserve comment text through the closing */
            sb.Append(line, 0, end + 2);
            i = end + 2;
            inBlockComment = false;
        }

        int codeStart = i;
        while (i < line.Length)
        {
            if (i + 1 < line.Length && line[i] == '/' && line[i + 1] == '/')
            {
                // Line comment: expand code before it, preserve the rest verbatim
                if (i > codeStart)
                    sb.Append(SubstituteMacros(line[codeStart..i]));
                sb.Append(line, i, line.Length - i);
                return sb.ToString();
            }
            if (i + 1 < line.Length && line[i] == '/' && line[i + 1] == '*')
            {
                // Block comment start: expand code before it
                if (i > codeStart)
                    sb.Append(SubstituteMacros(line[codeStart..i]));
                int end = line.IndexOf("*/", i + 2, StringComparison.Ordinal);
                if (end < 0)
                {
                    // Unterminated — preserve rest of line, mark state
                    inBlockComment = true;
                    sb.Append(line, i, line.Length - i);
                    return sb.ToString();
                }
                // Preserve the entire /* ... */ comment verbatim
                sb.Append(line, i, end + 2 - i);
                i = end + 2;
                codeStart = i;
                continue;
            }
            i++;
        }

        // Remaining code after the last comment (or entire line if no comments)
        if (codeStart == 0 && i == line.Length)
            return SubstituteMacros(line); // fast path: no comments at all
        if (i > codeStart)
            sb.Append(SubstituteMacros(line[codeStart..i]));
        return sb.ToString();
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
                    var quoted = "\"" + args[p].Trim().Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
                    expanded = ReplaceStringize(expanded, macro.Params[p], quoted);
                }

                for (int p = 0; p < macro.Params.Length && p < args.Length; p++)
                {
                    expanded = ReplaceWholeWord(expanded, macro.Params[p], args[p].Trim());
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
                    expanded = ReplaceWholeWord(expanded, "__VA_ARGS__", vaArgs);
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
                    line = ReplaceWholeWord(line, name, value);
                }
            }
            // Process ## token-paste operator after macro substitution
            line = ProcessTokenPaste(line);
            if (line == prev) break; // No more substitutions
        }
        return line;
    }

    private static string ProcessTokenPaste(string text)
    {
        var sb = new StringBuilder(text.Length);
        int i = 0;
        while (i < text.Length)
        {
            int idx = text.IndexOf("##", i);
            if (idx < 0)
            {
                sb.Append(text, i, text.Length - i);
                break;
            }
            // Trim trailing whitespace before ##
            int end = idx;
            while (end > i && char.IsWhiteSpace(text[end - 1])) end--;
            sb.Append(text, i, end - i);
            // Skip ## and leading whitespace after it
            i = idx + 2;
            while (i < text.Length && char.IsWhiteSpace(text[i])) i++;
        }
        return sb.ToString();
    }

    /// <summary>
    /// Replaces all occurrences of <c>defined(NAME)</c> and <c>defined NAME</c>
    /// with the result of <paramref name="evaluator"/>.
    /// </summary>
    private static string ReplaceDefined(string expr, Func<string, string> evaluator)
    {
        var sb = new StringBuilder(expr.Length);
        int i = 0;
        while (i < expr.Length)
        {
            int idx = expr.IndexOf("defined", i, StringComparison.Ordinal);
            if (idx < 0)
            {
                sb.Append(expr, i, expr.Length - i);
                break;
            }
            // Make sure "defined" is not part of a larger identifier
            if (idx > 0 && IsWordChar(expr[idx - 1]))
            {
                sb.Append(expr, i, idx + 7 - i);
                i = idx + 7;
                continue;
            }
            int afterDefined = idx + 7; // length of "defined"
            if (afterDefined < expr.Length && IsWordChar(expr[afterDefined])
                && expr[afterDefined] != '(')
            {
                // "defined" followed immediately by word char (not '(') → not our pattern, 
                // but actually this IS the "defined NAME" pattern without space.
                // The regex required \s+, so this is NOT a match. Skip.
                sb.Append(expr, i, afterDefined - i);
                i = afterDefined;
                continue;
            }

            sb.Append(expr, i, idx - i);
            int j = afterDefined;

            // Skip whitespace
            while (j < expr.Length && char.IsWhiteSpace(expr[j])) j++;

            if (j < expr.Length && expr[j] == '(')
            {
                // defined(NAME) form
                j++; // skip '('
                while (j < expr.Length && char.IsWhiteSpace(expr[j])) j++;
                int nameStart = j;
                while (j < expr.Length && IsWordChar(expr[j])) j++;
                if (j > nameStart)
                {
                    var name = expr[nameStart..j];
                    while (j < expr.Length && char.IsWhiteSpace(expr[j])) j++;
                    if (j < expr.Length && expr[j] == ')')
                    {
                        j++; // skip ')'
                        sb.Append(evaluator(name));
                        i = j;
                        continue;
                    }
                }
                // Malformed — emit as-is
                sb.Append("defined");
                i = afterDefined;
            }
            else
            {
                // defined NAME form — require at least one whitespace char was skipped
                if (j > afterDefined && j < expr.Length && IsWordChar(expr[j]))
                {
                    int nameStart = j;
                    while (j < expr.Length && IsWordChar(expr[j])) j++;
                    var name = expr[nameStart..j];
                    sb.Append(evaluator(name));
                    i = j;
                }
                else
                {
                    sb.Append("defined");
                    i = afterDefined;
                }
            }
        }
        return sb.ToString();
    }

    private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';

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
        expr = ReplaceDefined(expr, name =>
            (_defines.ContainsKey(name) || _functionMacros.ContainsKey(name)) ? "1" : "0");

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

    /// <summary>
    /// Join continuation lines and also return the original (raw) line index for each joined line.
    /// </summary>
    private static (List<string> Lines, int[] OriginalLineIndices) JoinContinuationLinesTracked(List<string> lines)
    {
        var result = new List<string>(lines.Count);
        var origIndices = new List<int>();
        var sb = new System.Text.StringBuilder();
        int groupStart = -1;
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var trimmedEnd = line.TrimEnd();
            var trimmedStart = trimmedEnd.TrimStart();
            if (trimmedEnd.EndsWith('\\') && !trimmedStart.StartsWith("//"))
            {
                if (groupStart < 0) groupStart = i;
                sb.Append(trimmedEnd[..^1]);
                sb.Append(' ');
            }
            else
            {
                if (groupStart < 0) groupStart = i;
                sb.Append(line);
                result.Add(sb.ToString());
                origIndices.Add(groupStart);
                sb.Clear();
                groupStart = -1;
            }
        }
        if (sb.Length > 0)
        {
            result.Add(sb.ToString());
            origIndices.Add(groupStart >= 0 ? groupStart : lines.Count - 1);
        }
        return (result, origIndices.ToArray());
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
