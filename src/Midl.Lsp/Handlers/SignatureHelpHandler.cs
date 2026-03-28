using System.Text.Json;
using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.Syntax;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Handles textDocument/signatureHelp requests — provides parameter hints
/// when typing inside parentheses of a method call or constructor.
/// </summary>
public static partial class SignatureHelpHandler
{
    public static object? Handle(JsonElement? @params, DocumentManager docManager)
    {
        if (@params is null) return null;

        var textDocument = @params.Value.GetProperty("textDocument");
        var uri = textDocument.GetProperty("uri").GetString()!;
        var position = @params.Value.GetProperty("position");
        int line = position.GetProperty("line").GetInt32();
        int character = position.GetProperty("character").GetInt32();

        var text = docManager.GetDocumentText(uri);
        if (text is null) return null;

        return HandleCore(text, line, character);
    }

    /// <summary>
    /// Core logic, testable without LSP JSON plumbing.
    /// </summary>
    public static object? HandleCore(string sourceText, int line, int character)
    {
        var lineStarts = PositionHelper.BuildLineStarts(sourceText);
        int offset = PositionHelper.LineCharToOffset(lineStarts, line, character);

        // Walk backwards from cursor to find the opening '(' and the identifier before it
        int parenDepth = 0;
        int commaCount = 0;
        int openParenPos = -1;

        for (int i = offset - 1; i >= 0; i--)
        {
            char c = sourceText[i];
            if (c == ')')
            {
                parenDepth++;
            }
            else if (c == '(')
            {
                if (parenDepth == 0)
                {
                    openParenPos = i;
                    break;
                }
                parenDepth--;
            }
            else if (c == ',' && parenDepth == 0)
            {
                commaCount++;
            }
        }

        if (openParenPos < 0) return null;

        // Check if we're inside an attribute bracket — e.g. [composable(...)]
        var attrResult = TryBuildAttributeSignature(sourceText, openParenPos, commaCount);
        if (attrResult is not null) return attrResult;

        // Find the identifier token just before the '('
        var lexer = new Lexer.Lexer(sourceText);
        var tokens = lexer.Lex();

        string? methodName = FindIdentifierBeforeOffset(tokens, openParenPos);
        if (methodName is null) return null;

        Compilation compilation;
        try
        {
            compilation = CompilationHelper.Compile(sourceText);
        }
        catch
        {
            return null;
        }

        // Find the method or constructor symbol
        var callable = FindCallable(compilation, methodName);
        if (callable is null) return null;

        var (label, parameters) = BuildSignatureInfo(callable);

        return new Dictionary<string, object?>
        {
            ["signatures"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["label"] = label,
                    ["parameters"] = parameters.Select(p => (object)new Dictionary<string, object?> { ["label"] = p }).ToArray()
                }
            },
            ["activeSignature"] = 0,
            ["activeParameter"] = commaCount
        };
    }

    private static string? FindIdentifierBeforeOffset(IReadOnlyList<SyntaxToken> tokens, int openParenOffset)
    {
        int pos = 0;
        string? lastIdentifier = null;
        int lastIdentifierEnd = 0;

        foreach (var token in tokens)
        {
            int leadingWidth = token.LeadingTrivia.Sum(t => t.FullWidth);
            int textStart = pos + leadingWidth;
            int textEnd = textStart + token.Text.Length;
            int trailingWidth = token.TrailingTrivia.Sum(t => t.FullWidth);

            if (textStart >= openParenOffset)
                break;

            if (token.Kind == SyntaxKind.IdentifierToken ||
                IsTypeKeyword(token.Kind))
            {
                lastIdentifier = token.Text;
                lastIdentifierEnd = textEnd;
            }

            pos = textEnd + trailingWidth;
        }

        // The identifier should be right before the '(' (possibly with whitespace)
        if (lastIdentifier is not null && lastIdentifierEnd <= openParenOffset)
            return lastIdentifier;

        return null;
    }

    private static bool IsTypeKeyword(SyntaxKind kind) => kind is
        SyntaxKind.BooleanKeyword or SyntaxKind.CharKeyword or
        SyntaxKind.Int16Keyword or SyntaxKind.Int32Keyword or SyntaxKind.Int64Keyword or
        SyntaxKind.UInt8Keyword or SyntaxKind.UInt16Keyword or SyntaxKind.UInt32Keyword or SyntaxKind.UInt64Keyword or
        SyntaxKind.SingleKeyword or SyntaxKind.DoubleKeyword or
        SyntaxKind.StringKeyword or SyntaxKind.GuidKeyword or SyntaxKind.ObjectKeyword;

    private static ISymbol? FindCallable(Compilation compilation, string name)
    {
        return FindCallableRecursive(compilation.GlobalNamespace, name);
    }

    private static ISymbol? FindCallableRecursive(NamespaceSymbol ns, string name)
    {
        foreach (var member in ns.Members)
        {
            // Check if this type has a constructor matching the name
            if (member.Name == name && member is RuntimeClassSymbol rc)
            {
                var ctor = rc.Members.OfType<ConstructorSymbol>().FirstOrDefault();
                if (ctor is not null) return ctor;
            }

            // Check members for methods
            var children = member switch
            {
                RuntimeClassSymbol r => r.Members,
                InterfaceSymbol iface => iface.Members,
                _ => null as IReadOnlyList<ISymbol>
            };

            if (children is not null)
            {
                foreach (var child in children)
                {
                    if (child.Name == name && child is MethodSymbol)
                        return child;
                    if (child.Name == name && child is ConstructorSymbol)
                        return child;
                }
            }

            // Check delegates
            if (member.Name == name && member is DelegateSymbol)
                return member;

            if (member is NamespaceSymbol childNs)
            {
                var found = FindCallableRecursive(childNs, name);
                if (found is not null) return found;
            }
        }
        return null;
    }

    private static (string Label, List<string> Parameters) BuildSignatureInfo(ISymbol callable)
    {
        return callable switch
        {
            MethodSymbol method => (
                $"{method.ReturnType.DisplayName} {method.Name}({FormatParams(method.Parameters)})",
                method.Parameters.Select(p => $"{p.Type.DisplayName} {p.Name}").ToList()
            ),
            ConstructorSymbol ctor => (
                $"{ctor.Name}({FormatParams(ctor.Parameters)})",
                ctor.Parameters.Select(p => $"{p.Type.DisplayName} {p.Name}").ToList()
            ),
            DelegateSymbol del => (
                $"{del.ReturnType.DisplayName} {del.Name}({FormatParams(del.Parameters)})",
                del.Parameters.Select(p => $"{p.Type.DisplayName} {p.Name}").ToList()
            ),
            _ => (callable.Name, new List<string>())
        };
    }

    private static string FormatParams(List<ParameterSymbol> parameters)
    {
        return string.Join(", ", parameters.Select(p => $"{p.Type.DisplayName} {p.Name}"));
    }
}
