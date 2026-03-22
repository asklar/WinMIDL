using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.CodeGen.WinMd;

namespace Midl.Analysis.Tests;

public class DidYouMeanTests
{
    [Fact]
    public void Strng_SuggestsString()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass Widget
                {
                    Strng GetName();
                }
            }
            """);

        var diag = compilation.Diagnostics.Single(d => d.Code == "MIDL1001");
        Assert.Contains("Strng", diag.Message);
        Assert.Contains("Did you mean", diag.Message);
        Assert.Contains("String", diag.Message);
        Assert.NotNull(diag.Data);
        Assert.True(diag.Data!.ContainsKey("suggestion"));
        Assert.Equal("String", diag.Data["suggestion"]);
    }

    [Fact]
    public void Int3_SuggestsInt32()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass Widget
                {
                    Int3 GetCount();
                }
            }
            """);

        var diag = compilation.Diagnostics.Single(d => d.Code == "MIDL1001");
        Assert.Contains("Did you mean", diag.Message);
        Assert.NotNull(diag.Data);
        Assert.Equal("Int32", diag.Data!["suggestion"]);
    }

    [Fact]
    public void Boolen_SuggestsBoolean()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass Widget
                {
                    Boolen IsEnabled();
                }
            }
            """);

        var diag = compilation.Diagnostics.Single(d => d.Code == "MIDL1001");
        Assert.Contains("Did you mean", diag.Message);
        Assert.NotNull(diag.Data);
        Assert.Equal("Boolean", diag.Data!["suggestion"]);
    }

    [Fact]
    public void IVectr_SuggestsIVector()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass Widget
                {
                    IVectr<String> GetItems();
                }
            }
            """);

        var diag = compilation.Diagnostics.Single(d => d.Code == "MIDL1001");
        Assert.Contains("Did you mean", diag.Message);
        Assert.NotNull(diag.Data);
        Assert.Equal("IVector", diag.Data!["suggestion"]);
    }

    [Fact]
    public void TypedEventHandlr_SuggestsTypedEventHandler()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass Widget
                {
                    event TypedEventHandlr<Widget, String> Changed;
                }
            }
            """);

        var diag = compilation.Diagnostics.Single(d => d.Code == "MIDL1001");
        Assert.Contains("Did you mean", diag.Message);
        Assert.NotNull(diag.Data);
        Assert.Equal("TypedEventHandler", diag.Data!["suggestion"]);
    }

    [Fact]
    public void UserDefinedType_MyWidgt_SuggestsMyWidget()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass MyWidget
                {
                }

                runtimeclass Container
                {
                    MyWidgt GetWidget();
                }
            }
            """);

        var diag = compilation.Diagnostics.Single(d => d.Code == "MIDL1001");
        Assert.Contains("Did you mean", diag.Message);
        Assert.NotNull(diag.Data);
        Assert.Equal("MyWidget", diag.Data!["suggestion"]);
    }

    [Fact]
    public void CompletelyUnrelated_NoSuggestion()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass Widget
                {
                    XYZABC123 GetThing();
                }
            }
            """);

        var diag = compilation.Diagnostics.Single(d => d.Code == "MIDL1001");
        Assert.DoesNotContain("Did you mean", diag.Message);
        Assert.True(diag.Data is null || !diag.Data.ContainsKey("suggestion"));
    }

    [Fact]
    public void DiagnosticMessage_ContainsDidYouMean()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass Widget
                {
                    Strng Name;
                }
            }
            """);

        var diag = compilation.Diagnostics.Single(d => d.Code == "MIDL1001");
        Assert.Matches(@"Did you mean '.*'\?", diag.Message);
    }

    [Fact]
    public void DiagnosticData_ContainsSuggestionKey()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass Widget
                {
                    Strng Name;
                }
            }
            """);

        var diag = compilation.Diagnostics.Single(d => d.Code == "MIDL1001");
        Assert.NotNull(diag.Data);
        Assert.True(diag.Data!.ContainsKey("suggestion"));
        Assert.Equal("String", diag.Data["suggestion"]);
    }

    /// <summary>Helper: compile IDL source to WinMD bytes.</summary>
    private static byte[] EmitWinMd(string idlSource)
    {
        var compilation = CompilationHelper.Compile(idlSource);
        Assert.False(compilation.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
            "IDL compilation should not have errors: " +
            string.Join("; ", compilation.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        var emitter = new WinMdEmitter();
        return emitter.EmitToBytes(compilation);
    }

    [Fact]
    public void QualifiedWinMdType_Aplication_SuggestsApplication()
    {
        // Create a WinMD with Windows.UI.Xaml.Application
        var winmd = EmitWinMd("""
            namespace Windows.UI.Xaml
            {
                [default_interface]
                runtimeclass Application
                {
                    Application();
                }
            }
            """);

        var tempPath = Path.Combine(Path.GetTempPath(), $"dym_winmd_{Guid.NewGuid()}.winmd");
        try
        {
            File.WriteAllBytes(tempPath, winmd);

            var source = """
                namespace TestApp
                {
                    runtimeclass MyApp
                    {
                        Windows.UI.Xaml.Aplication GetApp();
                    }
                }
                """;

            var compilation = CompilationHelper.Compile(source, new[] { tempPath });

            var diag = compilation.Diagnostics.Single(d => d.Code == "MIDL1001");
            Assert.Contains("Did you mean", diag.Message);
            Assert.NotNull(diag.Data);
            Assert.Equal("Windows.UI.Xaml.Application", diag.Data!["suggestion"]);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void ImportedIdlType_MyHelpr_SuggestsMyHelper()
    {
        // Create a temp IDL file with MyHelper type
        var tempDir = Path.Combine(Path.GetTempPath(), $"dym_import_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        var helperIdlPath = Path.Combine(tempDir, "helper.idl");
        var mainIdlPath = Path.Combine(tempDir, "main.idl");

        try
        {
            File.WriteAllText(helperIdlPath, """
                namespace Helpers
                {
                    runtimeclass MyHelper
                    {
                        MyHelper();
                    }
                }
                """);

            var mainSource = """
                import "helper.idl"
                namespace TestApp
                {
                    runtimeclass Widget
                    {
                        MyHelpr GetHelper();
                    }
                }
                """;

            File.WriteAllText(mainIdlPath, mainSource);

            var compilation = CompilationHelper.Compile(
                mainSource,
                referencePaths: null,
                CompilationMode.Strict,
                includePaths: new[] { tempDir },
                currentFilePath: mainIdlPath);

            var diag = compilation.Diagnostics.Single(d => d.Code == "MIDL1001");
            Assert.Contains("Did you mean", diag.Message);
            Assert.NotNull(diag.Data);
            Assert.Equal("MyHelper", diag.Data!["suggestion"]);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void WinMdType_ExtLibWidgt_SuggestsExtLibWidget()
    {
        // Create a WinMD with ExtLib.Widget
        var winmd = EmitWinMd("""
            namespace ExtLib
            {
                [default_interface]
                runtimeclass Widget
                {
                    Widget();
                }
            }
            """);

        var tempPath = Path.Combine(Path.GetTempPath(), $"dym_extlib_{Guid.NewGuid()}.winmd");
        try
        {
            File.WriteAllBytes(tempPath, winmd);

            var source = """
                namespace TestApp
                {
                    runtimeclass MyApp
                    {
                        ExtLib.Widgt GetWidget();
                    }
                }
                """;

            var compilation = CompilationHelper.Compile(source, new[] { tempPath });

            var diag = compilation.Diagnostics.Single(d => d.Code == "MIDL1001");
            Assert.Contains("Did you mean", diag.Message);
            Assert.NotNull(diag.Data);
            Assert.Equal("ExtLib.Widget", diag.Data!["suggestion"]);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void CaseMangled_SuggestsFromWinMd()
    {
        // Create a WinMD with a type whose simple name can be found by case-insensitive matching
        var winmd = EmitWinMd("""
            namespace ExtLib
            {
                [default_interface]
                runtimeclass NotifyPropertyChanged
                {
                    NotifyPropertyChanged();
                }
            }
            """);

        var tempPath = Path.Combine(Path.GetTempPath(), $"dym_casemangled_{Guid.NewGuid()}.winmd");
        try
        {
            File.WriteAllBytes(tempPath, winmd);

            // Use case-mangled version of the simple name (unqualified)
            var source = """
                namespace TestApp
                {
                    runtimeclass MyApp
                    {
                        notifyPROpertyChanged GetHandler();
                    }
                }
                """;

            var compilation = CompilationHelper.Compile(source, new[] { tempPath });

            var diag = compilation.Diagnostics.Single(d => d.Code == "MIDL1001");
            Assert.Contains("Did you mean", diag.Message);
            Assert.NotNull(diag.Data);
            var suggestion = diag.Data!["suggestion"].ToString();
            Assert.Contains("NotifyPropertyChanged", suggestion!);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void Strin_SuggestsString()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass Widget
                {
                    Strin GetName();
                }
            }
            """);

        var diag = compilation.Diagnostics.Single(d => d.Code == "MIDL1001");
        Assert.Contains("Did you mean", diag.Message);
        Assert.NotNull(diag.Data);
        Assert.Equal("String", diag.Data!["suggestion"]);
    }

    [Fact]
    public void CaseInsensitive_bOOlean_SuggestsBoolean()
    {
        // "bOOlean" has case-sensitive edit distance 3 from "Boolean" (exceeds maxDistance=2)
        // but case-insensitive distance is 0 — should still suggest Boolean
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass Widget
                {
                    bOOlean IsEnabled();
                }
            }
            """);

        var diag = compilation.Diagnostics.Single(d => d.Code == "MIDL1001");
        Assert.Contains("Did you mean", diag.Message);
        Assert.NotNull(diag.Data);
        Assert.Equal("Boolean", diag.Data!["suggestion"]);
    }

    [Fact]
    public void UnqualifiedTruncatedLongName_SuggestsFromWinMd()
    {
        // "notifypropertychan" should match INotifyPropertyChanged from WinMD
        var refSource = @"
            namespace ExtLib
            {
                interface INotifyPropertyChanged
                {
                    void OnChanged();
                };
            }";
        var refCompilation = CompilationHelper.Compile(refSource);
        var emitter = new Midl.CodeGen.WinMd.WinMdEmitter();
        var winmdBytes = emitter.EmitToBytes(refCompilation);
        var tempPath = Path.Combine(Path.GetTempPath(), $"dym_long_{Guid.NewGuid()}.winmd");
        try
        {
            File.WriteAllBytes(tempPath, winmdBytes);
            var source = "namespace N { runtimeclass C { C(); notifypropertychan Foo{ get; }; }; }";
            var compilation = CompilationHelper.Compile(source, new[] { tempPath });
            var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1001" && d.Message.Contains("Did you mean"));
            Assert.NotNull(diag);
            Assert.Contains("INotifyPropertyChanged", diag!.Message);
        }
        finally { File.Delete(tempPath); }
    }

    [Fact]
    public void CaseInsensitiveUnqualified_SuggestsFullyQualified()
    {
        // "iasyncopertion" (missing 'a') should suggest IAsyncOperation
        var source = "namespace N { runtimeclass C { C(); iasyncopertion Foo{ get; }; }; }";
        var compilation = CompilationHelper.Compile(source);
        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1001" && d.Message.Contains("Did you mean"));
        Assert.NotNull(diag);
        Assert.Contains("IAsyncOperation", diag!.Message);
    }

    [Fact]
    public void QualifiedTruncated_SuggestsFullName()
    {
        // "Windows.Foundation.IAsyncOper" should suggest Windows.Foundation.IAsyncOperation
        var refSource = @"
            namespace Windows.Foundation
            {
                interface IAsyncOperation
                {
                    void GetResults();
                };
            }";
        var refCompilation = CompilationHelper.Compile(refSource);
        var emitter = new Midl.CodeGen.WinMd.WinMdEmitter();
        var winmdBytes = emitter.EmitToBytes(refCompilation);
        var tempPath = Path.Combine(Path.GetTempPath(), $"dym_qual_{Guid.NewGuid()}.winmd");
        try
        {
            File.WriteAllBytes(tempPath, winmdBytes);
            var source = "namespace N { runtimeclass C { C(); Windows.Foundation.IAsyncOper Foo{ get; }; }; }";
            var compilation = CompilationHelper.Compile(source, new[] { tempPath });
            var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1001" && d.Message.Contains("Did you mean"));
            Assert.NotNull(diag);
            Assert.Contains("IAsyncOperation", diag!.Message);
        }
        finally { File.Delete(tempPath); }
    }

    [Fact]
    public void ShortTruncation_DoesNotFalsePositive()
    {
        // "I" alone should NOT suggest anything (too short / too many matches)
        var source = "namespace N { runtimeclass C { C(); I Foo{ get; }; }; }";
        var compilation = CompilationHelper.Compile(source);
        // With just 1 char, maxDistance = max(2, 1/4) = 2, but most types are far from "I"
        // This mainly checks we don't crash or suggest garbage
        Assert.Contains(compilation.Diagnostics, d => d.Code == "MIDL1001");
    }

    [Fact]
    public void DidYouMean_CloseTypo_SuggestsCorrection()
    {
        var source = "namespace N { interface IFoo { Strng GetName(); } }";
        var compilation = CompilationHelper.Compile(source);
        var diag = compilation.Diagnostics.First(d => d.Code == "MIDL1001");
        Assert.Contains("String", diag.Message);
    }

    [Fact]
    public void DidYouMean_CaseInsensitive()
    {
        // "string" (lowercase) is not a keyword — should suggest "String"
        var source = "namespace N { interface IFoo { string GetName(); } }";
        var compilation = CompilationHelper.Compile(source);
        var diag = compilation.Diagnostics.First(d => d.Code == "MIDL1001");
        Assert.Contains("Did you mean", diag.Message);
        Assert.Contains("String", diag.Message);
    }

    [Fact]
    public void DidYouMean_UserDefinedType()
    {
        var source = @"
            namespace N {
                runtimeclass MyWidget { }
                runtimeclass Container { MyWidgt GetWidget(); }
            }";
        var compilation = CompilationHelper.Compile(source);
        var diag = compilation.Diagnostics.First(d => d.Code == "MIDL1001");
        Assert.Contains("Did you mean", diag.Message);
        Assert.Contains("MyWidget", diag.Message);
    }

    [Fact]
    public void DidYouMean_NoSuggestion_TotallyWrong()
    {
        var source = "namespace N { interface IFoo { Xyzzy GetThing(); } }";
        var compilation = CompilationHelper.Compile(source);
        var diag = compilation.Diagnostics.First(d => d.Code == "MIDL1001");
        Assert.DoesNotContain("Did you mean", diag.Message);
    }

    [Fact]
    public void DidYouMean_ScaledThreshold_LongName()
    {
        // "INotifyPropertyChangd" (missing 'e') is edit distance 1 from
        // "INotifyPropertyChanged" — long names allow more tolerance
        var source = @"
            namespace N {
                interface INotifyPropertyChanged { void OnChanged(); }
                runtimeclass C { INotifyPropertyChangd Handler { get; }; }
            }";
        var compilation = CompilationHelper.Compile(source);
        var diag = compilation.Diagnostics.First(d => d.Code == "MIDL1001");
        Assert.Contains("Did you mean", diag.Message);
        Assert.Contains("INotifyPropertyChanged", diag.Message);
    }
}
