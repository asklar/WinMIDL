using Midl.Analysis;
using Midl.Syntax;

namespace Midl.Analysis.Tests;

public class DocCommentTests
{
    #region Phase 1 — Lexer: DocCommentTrivia vs SingleLineCommentTrivia

    [Fact]
    public void TripleSlash_ProducesDocCommentTrivia()
    {
        var lexer = new Lexer.Lexer("/// <summary>text</summary>\nint x;");
        var tokens = lexer.Lex();

        var firstToken = tokens[0];
        Assert.Contains(firstToken.LeadingTrivia,
            t => t.Kind == SyntaxKind.DocCommentTrivia);
    }

    [Fact]
    public void DoubleSlash_ProducesSingleLineCommentTrivia()
    {
        var lexer = new Lexer.Lexer("// regular comment\nint x;");
        var tokens = lexer.Lex();

        var firstToken = tokens[0];
        Assert.Contains(firstToken.LeadingTrivia,
            t => t.Kind == SyntaxKind.SingleLineCommentTrivia);
        Assert.DoesNotContain(firstToken.LeadingTrivia,
            t => t.Kind == SyntaxKind.DocCommentTrivia);
    }

    [Fact]
    public void ConsecutiveTripleSlash_EachProducesDocCommentTrivia()
    {
        var lexer = new Lexer.Lexer("/// line1\n/// line2\n/// line3\nint x;");
        var tokens = lexer.Lex();

        var docTriviaCount = tokens[0].LeadingTrivia.Count(
            t => t.Kind == SyntaxKind.DocCommentTrivia);
        Assert.Equal(3, docTriviaCount);
    }

    [Fact]
    public void FourSlashes_ProducesSingleLineCommentTrivia()
    {
        // //// is not a doc comment — it's a regular comment starting with ////
        var lexer = new Lexer.Lexer("//// not a doc comment\nint x;");
        var tokens = lexer.Lex();

        var firstToken = tokens[0];
        Assert.Contains(firstToken.LeadingTrivia,
            t => t.Kind == SyntaxKind.SingleLineCommentTrivia);
        Assert.DoesNotContain(firstToken.LeadingTrivia,
            t => t.Kind == SyntaxKind.DocCommentTrivia);
    }

    #endregion

    #region Phase 2 — Binder: DocComment extraction

    [Fact]
    public void DocComment_ExtractedOnRuntimeClass()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                /// <summary>A widget class.</summary>
                runtimeclass Widget
                {
                    void DoStuff();
                }
            }
            """);

        var widget = compilation.GetAllTypes()
            .OfType<Symbols.RuntimeClassSymbol>()
            .FirstOrDefault(t => t.Name == "Widget");
        Assert.NotNull(widget);
        Assert.NotNull(widget!.DocComment);
        Assert.Equal("A widget class.", widget.DocComment!.Summary);
    }

    [Fact]
    public void DocComment_MethodWithParams()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                runtimeclass Widget
                {
                    /// <summary>Does stuff.</summary>
                    /// <param name="count">The count.</param>
                    /// <returns>A result.</returns>
                    Int32 DoStuff(Int32 count);
                }
            }
            """);

        var widget = compilation.GetAllTypes()
            .OfType<Symbols.RuntimeClassSymbol>()
            .First(t => t.Name == "Widget");
        var method = widget.Members.OfType<Symbols.MethodSymbol>()
            .First(m => m.Name == "DoStuff");
        Assert.NotNull(method.DocComment);
        Assert.Equal("Does stuff.", method.DocComment!.Summary);
        Assert.True(method.DocComment.Params.ContainsKey("count"));
        Assert.Equal("The count.", method.DocComment.Params["count"]);
        Assert.Equal("A result.", method.DocComment.Returns);
    }

    [Fact]
    public void NoDocComment_IsNull()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                runtimeclass Widget
                {
                    void DoStuff();
                }
            }
            """);

        var widget = compilation.GetAllTypes()
            .OfType<Symbols.RuntimeClassSymbol>()
            .First(t => t.Name == "Widget");
        Assert.Null(widget.DocComment);
    }

    [Fact]
    public void DocComment_MultiLineConcatenation()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                /// <summary>
                /// A multi-line
                /// summary.
                /// </summary>
                /// <remarks>Some remarks.</remarks>
                runtimeclass Widget
                {
                    void DoStuff();
                }
            }
            """);

        var widget = compilation.GetAllTypes()
            .OfType<Symbols.RuntimeClassSymbol>()
            .First(t => t.Name == "Widget");
        Assert.NotNull(widget.DocComment);
        Assert.Contains("multi-line", widget.DocComment!.Summary);
        Assert.Contains("summary.", widget.DocComment!.Summary);
        Assert.Equal("Some remarks.", widget.DocComment!.Remarks);
    }

    [Fact]
    public void DocComment_OnInterface()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                /// <summary>A contract.</summary>
                interface IWidget
                {
                    void Do();
                }
            }
            """);

        var iface = compilation.GetAllTypes()
            .OfType<Symbols.InterfaceSymbol>()
            .First(t => t.Name == "IWidget");
        Assert.NotNull(iface.DocComment);
        Assert.Equal("A contract.", iface.DocComment!.Summary);
    }

    [Fact]
    public void DocComment_OnProperty()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                runtimeclass Widget
                {
                    /// <summary>The name.</summary>
                    String Name;
                }
            }
            """);

        var widget = compilation.GetAllTypes()
            .OfType<Symbols.RuntimeClassSymbol>()
            .First(t => t.Name == "Widget");
        var prop = widget.Members.OfType<Symbols.PropertySymbol>()
            .First(p => p.Name == "Name");
        Assert.NotNull(prop.DocComment);
        Assert.Equal("The name.", prop.DocComment!.Summary);
    }

    [Fact]
    public void DocComment_OnEvent()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                runtimeclass Widget
                {
                    /// <summary>Raised on click.</summary>
                    event Windows.Foundation.EventHandler<Object> Clicked;
                }
            }
            """);

        var widget = compilation.GetAllTypes()
            .OfType<Symbols.RuntimeClassSymbol>()
            .First(t => t.Name == "Widget");
        var ev = widget.Members.OfType<Symbols.EventSymbol>()
            .First(e => e.Name == "Clicked");
        Assert.NotNull(ev.DocComment);
        Assert.Equal("Raised on click.", ev.DocComment!.Summary);
    }

    #endregion

    #region Phase 6 — Quality diagnostic: MIDL3020

    [Fact]
    public void UndocumentedPublicRuntimeClass_EmitsMIDL3020()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                runtimeclass Widget
                {
                    void Do();
                }
            }
            """, referencePaths: null, mode: CompilationMode.Strict,
            includePaths: null, currentFilePath: null, emitQualityHints: true);

        Assert.Contains(compilation.Diagnostics, d => d.Code == "MIDL3020");
    }

    [Fact]
    public void DocumentedRuntimeClass_NoMIDL3020()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                /// <summary>Documented.</summary>
                runtimeclass Widget
                {
                    void Do();
                }
            }
            """, referencePaths: null, mode: CompilationMode.Strict,
            includePaths: null, currentFilePath: null, emitQualityHints: true);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL3020");
    }

    [Fact]
    public void SynthesizedInterface_NoMIDL3020()
    {
        // Synthesized interfaces like IWidgetFactory should not trigger MIDL3020
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                /// <summary>Documented.</summary>
                runtimeclass Widget
                {
                    Widget(Int32 x);
                }
            }
            """, referencePaths: null, mode: CompilationMode.Strict,
            includePaths: null, currentFilePath: null, emitQualityHints: true);

        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Code == "MIDL3020" && d.Message.Contains("IWidget"));
    }

    [Fact]
    public void QualityHintsDisabled_NoMIDL3020()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                runtimeclass Widget
                {
                    void Do();
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL3020");
    }

    #endregion

    #region Doc Comment Syntax Highlighting

    [Fact]
    public void SemanticTokens_DocComment_SplitsIntoTagsAndContent()
    {
        var source = "/// <summary>A widget.</summary>\nnamespace N { }";
        var compilation = CompilationHelper.Compile(source);
        var (data, types, _) = Midl.Lsp.Handlers.SemanticTokensHandler.HandleCore(source, compilation);

        int commentType = Array.IndexOf(types, "comment");
        int decoratorType = Array.IndexOf(types, "decorator");
        int stringType = Array.IndexOf(types, "string");

        // First token on line 0 should be "///" as comment
        Assert.True(data.Length >= 5);
        Assert.Equal(0, data[0]); // deltaLine
        Assert.Equal(3, data[2]); // length of "///"
        Assert.Equal(commentType, data[3]); // type = comment

        // Should also have decorator tokens for <summary> and </summary>
        bool hasDecorator = false;
        bool hasString = false;
        for (int i = 0; i < data.Length; i += 5)
        {
            if (data[i + 3] == decoratorType) hasDecorator = true;
            if (data[i + 3] == stringType && i > 0) hasString = true; // content between tags
        }
        Assert.True(hasDecorator, "Doc comment should have decorator tokens for XML tags");
        Assert.True(hasString, "Doc comment should have string tokens for content");
    }

    #endregion

    #region XmlTagParser tests

    [Fact]
    public void XmlTagParser_BasicTag()
    {
        var result = XmlTagParser.Parse("<summary>text</summary>");
        Assert.Single(result.Tags);
        Assert.Equal("summary", result.Tags[0].Name);
        Assert.Equal("text", result.Tags[0].Content);
        Assert.False(result.Tags[0].IsSelfClosing);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void XmlTagParser_TagWithAttributes()
    {
        var result = XmlTagParser.Parse("<param name=\"x\">desc</param>");
        Assert.Single(result.Tags);
        Assert.Equal("param", result.Tags[0].Name);
        Assert.Equal("desc", result.Tags[0].Content);
        Assert.True(result.Tags[0].Attributes.ContainsKey("name"));
        Assert.Equal("x", result.Tags[0].Attributes["name"]);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void XmlTagParser_SelfClosing()
    {
        var result = XmlTagParser.Parse("<see cref=\"Widget\"/>");
        Assert.Single(result.Tags);
        Assert.True(result.Tags[0].IsSelfClosing);
        Assert.True(result.Tags[0].Attributes.ContainsKey("cref"));
        Assert.Equal("Widget", result.Tags[0].Attributes["cref"]);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void XmlTagParser_UnclosedTag()
    {
        var result = XmlTagParser.Parse("<summary>text");
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void XmlTagParser_MismatchedTag()
    {
        var result = XmlTagParser.Parse("<summary>text</param>");
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void XmlTagParser_MultipleTags()
    {
        var result = XmlTagParser.Parse("<summary>s</summary><returns>r</returns>");
        Assert.Equal(2, result.Tags.Count);
        Assert.Equal("summary", result.Tags[0].Name);
        Assert.Equal("s", result.Tags[0].Content);
        Assert.Equal("returns", result.Tags[1].Name);
        Assert.Equal("r", result.Tags[1].Content);
        Assert.Empty(result.Errors);
    }

    #endregion

    #region Phase 7 — MIDL3021 malformed doc comment diagnostic

    [Fact]
    public void MIDL3021_MalformedDocComment()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                /// <summary>unclosed
                runtimeclass Widget
                {
                    void Do();
                }
            }
            """);

        Assert.Contains(compilation.Diagnostics, d => d.Code == "MIDL3021");
    }

    [Fact]
    public void MIDL3021_ValidDocComment_NoWarning()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                /// <summary>ok</summary>
                runtimeclass Widget
                {
                    void Do();
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL3021");
    }

    #endregion
}
