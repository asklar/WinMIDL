// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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

    #region Doxygen — Lexer: DocCommentTrivia for Doxygen comment styles

    [Fact]
    public void DoxygenSlashBang_ProducesDocCommentTrivia()
    {
        var lexer = new Lexer.Lexer("//! A doc comment\nint x;");
        var tokens = lexer.Lex();
        Assert.Contains(tokens[0].LeadingTrivia,
            t => t.Kind == SyntaxKind.DocCommentTrivia);
    }

    [Fact]
    public void DoxygenBlockStarStar_ProducesDocCommentTrivia()
    {
        var lexer = new Lexer.Lexer("/** A doc comment */ int x;");
        var tokens = lexer.Lex();
        Assert.Contains(tokens[0].LeadingTrivia,
            t => t.Kind == SyntaxKind.DocCommentTrivia);
    }

    [Fact]
    public void DoxygenBlockBangStar_ProducesDocCommentTrivia()
    {
        var lexer = new Lexer.Lexer("/*! A doc comment */ int x;");
        var tokens = lexer.Lex();
        Assert.Contains(tokens[0].LeadingTrivia,
            t => t.Kind == SyntaxKind.DocCommentTrivia);
    }

    [Fact]
    public void TripleStarBlock_ProducesMultiLineCommentTrivia()
    {
        // /*** is NOT a doc comment — 3+ stars
        var lexer = new Lexer.Lexer("/*** not a doc comment */ int x;");
        var tokens = lexer.Lex();
        Assert.Contains(tokens[0].LeadingTrivia,
            t => t.Kind == SyntaxKind.MultiLineCommentTrivia);
        Assert.DoesNotContain(tokens[0].LeadingTrivia,
            t => t.Kind == SyntaxKind.DocCommentTrivia);
    }

    [Fact]
    public void DoxygenBlockEmpty_ProducesDocCommentTrivia()
    {
        // /** */ is a valid doc comment (empty)
        var lexer = new Lexer.Lexer("/**/ int x;");
        var tokens = lexer.Lex();
        // /**/ is actually just a regular comment since there's no content after /**
        // It's "/*" + "*/" which is a regular multiline comment
        Assert.Contains(tokens[0].LeadingTrivia,
            t => t.Kind == SyntaxKind.MultiLineCommentTrivia);
    }

    [Fact]
    public void DoxygenBlockMultiLine_ProducesDocCommentTrivia()
    {
        var source = "/**\n * @brief Summary\n * @param x desc\n */\nint x;";
        var lexer = new Lexer.Lexer(source);
        var tokens = lexer.Lex();
        Assert.Contains(tokens[0].LeadingTrivia,
            t => t.Kind == SyntaxKind.DocCommentTrivia);
    }

    #endregion

    #region Doxygen — DocComment.Parse: Doxygen tag translation

    [Fact]
    public void DoxygenParse_BriefAt()
    {
        var doc = DocComment.Parse("@brief This is a summary");
        Assert.NotNull(doc);
        Assert.Equal("This is a summary", doc!.Summary);
    }

    [Fact]
    public void DoxygenParse_BriefBackslash()
    {
        var doc = DocComment.Parse("\\brief Same thing");
        Assert.NotNull(doc);
        Assert.Equal("Same thing", doc!.Summary);
    }

    [Fact]
    public void DoxygenParse_ParamAt()
    {
        var doc = DocComment.Parse("@param name The description");
        Assert.NotNull(doc);
        Assert.True(doc!.Params.ContainsKey("name"));
        Assert.Equal("The description", doc.Params["name"]);
    }

    [Fact]
    public void DoxygenParse_ParamBackslash()
    {
        var doc = DocComment.Parse("\\param name The description");
        Assert.NotNull(doc);
        Assert.True(doc!.Params.ContainsKey("name"));
        Assert.Equal("The description", doc.Params["name"]);
    }

    [Fact]
    public void DoxygenParse_ReturnAt()
    {
        var doc = DocComment.Parse("@return The result");
        Assert.NotNull(doc);
        Assert.Equal("The result", doc!.Returns);
    }

    [Fact]
    public void DoxygenParse_ReturnsAt()
    {
        var doc = DocComment.Parse("@returns The result");
        Assert.NotNull(doc);
        Assert.Equal("The result", doc!.Returns);
    }

    [Fact]
    public void DoxygenParse_ReturnBackslash()
    {
        var doc = DocComment.Parse("\\return The result");
        Assert.NotNull(doc);
        Assert.Equal("The result", doc!.Returns);
    }

    [Fact]
    public void DoxygenParse_ReturnsBackslash()
    {
        var doc = DocComment.Parse("\\returns The result");
        Assert.NotNull(doc);
        Assert.Equal("The result", doc!.Returns);
    }

    [Fact]
    public void DoxygenParse_SeeAt()
    {
        var doc = DocComment.Parse("@see SomeType");
        Assert.NotNull(doc);
        Assert.NotNull(doc!.RawXml);
        // @see translates to <seealso cref="SomeType"/>
        Assert.Contains("SomeType", doc.RawXml);
    }

    [Fact]
    public void DoxygenParse_NoteAt()
    {
        var doc = DocComment.Parse("@note Important note");
        Assert.NotNull(doc);
        Assert.Equal("Important note", doc!.Remarks);
    }

    [Fact]
    public void DoxygenParse_DeprecatedAt()
    {
        var doc = DocComment.Parse("@deprecated Use NewThing instead");
        Assert.NotNull(doc);
        Assert.NotNull(doc!.Remarks);
        Assert.Contains("Deprecated:", doc.Remarks);
        Assert.Contains("Use NewThing instead", doc.Remarks);
    }

    [Fact]
    public void DoxygenParse_Mixed()
    {
        var doc = DocComment.Parse("@brief Summary text\n@param x The x param\n@returns The result");
        Assert.NotNull(doc);
        Assert.Equal("Summary text", doc!.Summary);
        Assert.True(doc.Params.ContainsKey("x"));
        Assert.Equal("The x param", doc.Params["x"]);
        Assert.Equal("The result", doc.Returns);
    }

    [Fact]
    public void DoxygenParse_PlainText_BecomesSummary()
    {
        var doc = DocComment.Parse("Just a plain description");
        Assert.NotNull(doc);
        Assert.Equal("Just a plain description", doc!.Summary);
    }

    [Fact]
    public void DoxygenParse_XdcStillWorks()
    {
        // Existing XDC format must not regress
        var doc = DocComment.Parse("<summary>XML summary</summary>");
        Assert.NotNull(doc);
        Assert.Equal("XML summary", doc!.Summary);
    }

    [Fact]
    public void DoxygenParse_MultipleParams()
    {
        var doc = DocComment.Parse("@brief Do something\n@param x First\n@param y Second");
        Assert.NotNull(doc);
        Assert.Equal("Do something", doc!.Summary);
        Assert.Equal(2, doc.Params.Count);
        Assert.Equal("First", doc.Params["x"]);
        Assert.Equal("Second", doc.Params["y"]);
    }

    #endregion

    #region Doxygen — Binder: extraction from Doxygen comments

    [Fact]
    public void DocComment_DoxygenBlockOnRuntimeClass()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                /**
                 * @brief A widget class.
                 */
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
    public void DocComment_DoxygenBlockWithoutLeadingAsterisks()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                /**
                @brief A widget without leading asterisks.
                @param name The widget name.
                */
                runtimeclass Widget
                {
                    void DoStuff(String name);
                }
            }
            """);

        var widget = compilation.GetAllTypes()
            .OfType<Symbols.RuntimeClassSymbol>()
            .FirstOrDefault(t => t.Name == "Widget");
        Assert.NotNull(widget);
        Assert.NotNull(widget!.DocComment);
        Assert.Equal("A widget without leading asterisks.", widget.DocComment!.Summary);
    }

    [Fact]
    public void DocComment_DoxygenBlockMethodWithParams()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                runtimeclass Widget
                {
                    /**
                     * @brief Does stuff.
                     * @param count The count.
                     * @returns A result.
                     */
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
    public void DocComment_DoxygenSlashBangOnClass()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                //! @brief A bang widget.
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
        Assert.Equal("A bang widget.", widget.DocComment!.Summary);
    }

    [Fact]
    public void DocComment_DoxygenTripleSlashTags()
    {
        // Doxygen tags with /// prefix should also work
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                /// @brief A class.
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
        Assert.Equal("A class.", widget.DocComment!.Summary);
    }

    [Fact]
    public void DocComment_DoxygenExclamationBlockOnClass()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                /*!
                 * @brief An excl widget.
                 */
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
        Assert.Equal("An excl widget.", widget.DocComment!.Summary);
    }

    #endregion

    #region Doxygen — Semantic tokens

    [Fact]
    public void SemanticTokens_DoxygenTags_ClassifiedAsDecorator()
    {
        var source = "/// @brief A widget.\nnamespace N { }";
        var compilation = CompilationHelper.Compile(source);
        var (data, types, _) = Midl.Lsp.Handlers.SemanticTokensHandler.HandleCore(source, compilation);

        int decoratorType = Array.IndexOf(types, "decorator");

        // Should have a decorator token for "@brief"
        bool hasDecorator = false;
        for (int i = 0; i < data.Length; i += 5)
        {
            if (data[i + 3] == decoratorType) hasDecorator = true;
        }
        Assert.True(hasDecorator, "Doxygen @brief tag should produce a decorator semantic token");
    }

    [Fact]
    public void SemanticTokens_DoxygenSlashBang_ClassifiedCorrectly()
    {
        var source = "//! @brief A widget.\nnamespace N { }";
        var compilation = CompilationHelper.Compile(source);
        var (data, types, _) = Midl.Lsp.Handlers.SemanticTokensHandler.HandleCore(source, compilation);

        int commentType = Array.IndexOf(types, "comment");
        int decoratorType = Array.IndexOf(types, "decorator");

        // First token should be "//!" as comment
        Assert.True(data.Length >= 5);
        Assert.Equal(commentType, data[3]); // type = comment

        // Should have decorator for @brief
        bool hasDecorator = false;
        for (int i = 0; i < data.Length; i += 5)
        {
            if (data[i + 3] == decoratorType) hasDecorator = true;
        }
        Assert.True(hasDecorator, "Doxygen @brief in //! comment should produce decorator token");
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

    #region CR fix 1 — @param[in] direction modifier

    [Fact]
    public void DoxygenParse_ParamIn()
    {
        var doc = DocComment.Parse("@param[in] x The input param");
        Assert.NotNull(doc);
        Assert.True(doc!.Params.ContainsKey("x"));
        Assert.Equal("The input param", doc.Params["x"]);
    }

    [Fact]
    public void DoxygenParse_ParamOut()
    {
        var doc = DocComment.Parse("@param[out] y The output param");
        Assert.NotNull(doc);
        Assert.True(doc!.Params.ContainsKey("y"));
        Assert.Equal("The output param", doc.Params["y"]);
    }

    [Fact]
    public void DoxygenParse_ParamInOut()
    {
        var doc = DocComment.Parse("@param[in,out] z The bidirectional param");
        Assert.NotNull(doc);
        Assert.True(doc!.Params.ContainsKey("z"));
        Assert.Equal("The bidirectional param", doc.Params["z"]);
    }

    [Fact]
    public void DoxygenParse_ParamIn_Backslash()
    {
        var doc = DocComment.Parse("\\param[in] w A param");
        Assert.NotNull(doc);
        Assert.True(doc!.Params.ContainsKey("w"));
        Assert.Equal("A param", doc.Params["w"]);
    }

    #endregion

    #region CR fix 2 — Multiline /** */ semantic tokens for Doxygen tags

    [Fact]
    public void SemanticTokens_DoxygenBlockComment_HasDecoratorTokens()
    {
        var source = "/** @brief A widget. */\nnamespace N { }";
        var compilation = CompilationHelper.Compile(source);
        var (data, types, _) = Midl.Lsp.Handlers.SemanticTokensHandler.HandleCore(source, compilation);

        int decoratorType = Array.IndexOf(types, "decorator");

        bool hasDecorator = false;
        for (int i = 0; i < data.Length; i += 5)
        {
            if (data[i + 3] == decoratorType) hasDecorator = true;
        }
        Assert.True(hasDecorator, "Doxygen @brief in /** */ block should produce a decorator semantic token");
    }

    #endregion

    #region CR fix 3 — Prefix stripping: *@brief without space

    [Fact]
    public void DocComment_StarAtBrief_NoSpace()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                /**
                 *@brief A widget without space.
                 */
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
        Assert.Equal("A widget without space.", widget.DocComment!.Summary);
    }

    [Fact]
    public void DocComment_StarBackslashBrief_NoSpace()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                /**
                 *\brief A backslash widget.
                 */
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
        Assert.Equal("A backslash widget.", widget.DocComment!.Summary);
    }

    #endregion

    #region CR fix 4 — /***/ edge case

    [Fact]
    public void TripleStarSlash_ProducesMultiLineCommentTrivia()
    {
        // /***/ — 3 stars then close — should NOT be a doc comment
        var lexer = new Lexer.Lexer("/***/ int x;");
        var tokens = lexer.Lex();
        Assert.Contains(tokens[0].LeadingTrivia,
            t => t.Kind == SyntaxKind.MultiLineCommentTrivia);
        Assert.DoesNotContain(tokens[0].LeadingTrivia,
            t => t.Kind == SyntaxKind.DocCommentTrivia);
    }

    #endregion

    #region CR fix 5 — Integration: hover + XDC with Doxygen comments

    [Fact]
    public void Hover_DoxygenDocComment_ReturnsSummary()
    {
        var uri = "file:///test_doxygen_hover.midl";
        var source = """
            namespace N
            {
                /** @brief A cool widget. */
                runtimeclass Widget
                {
                    void Do();
                }
            }
            """;
        var dm = new Midl.Lsp.DocumentManager();
        dm.OpenDocument(uri, source);

        // Find line/char of "Widget" in "runtimeclass Widget"
        var lines = source.Split('\n');
        int widgetLine = -1, widgetChar = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            int idx = lines[i].IndexOf("runtimeclass Widget");
            if (idx >= 0)
            {
                widgetLine = i;
                widgetChar = idx + "runtimeclass ".Length;
                break;
            }
        }
        Assert.True(widgetLine >= 0);

        var posParams = System.Text.Json.JsonDocument.Parse($$"""
            {
                "textDocument": { "uri": "{{uri}}" },
                "position": { "line": {{widgetLine}}, "character": {{widgetChar}} }
            }
            """).RootElement;

        var result = Midl.Lsp.Handlers.HoverHandler.Handle(posParams, dm);
        Assert.NotNull(result);

        var json = System.Text.Json.JsonSerializer.Serialize(result, Midl.Lsp.Protocol.LspJsonContext.Default.HoverResult);
        Assert.Contains("A cool widget.", json);
    }

    [Fact]
    public void XdcEmitter_DoxygenComments_ProducesOutput()
    {
        var compilation = CompilationHelper.Compile("""
            namespace MyLib
            {
                /**
                 * @brief A Doxygen widget.
                 */
                runtimeclass Widget
                {
                    /**
                     * @brief Does stuff.
                     * @param count The count.
                     * @returns A result.
                     */
                    Int32 DoStuff(Int32 count);
                }
            }
            """);

        var emitter = new Midl.CodeGen.Xdc.XdcEmitter();
        var xml = emitter.Emit(compilation, "MyLib");

        Assert.Contains("T:MyLib.Widget", xml);
        Assert.Contains("A Doxygen widget.", xml);
        Assert.Contains("M:MyLib.Widget.DoStuff(Int32)", xml);
        Assert.Contains("Does stuff.", xml);
        Assert.Contains("count", xml);
    }

    #endregion
}
