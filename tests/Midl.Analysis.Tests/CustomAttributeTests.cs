using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;
using Midl.CodeGen.WinMd;

namespace Midl.Analysis.Tests;

public class CustomAttributeTests
{
    #region Parsing

    [Fact]
    public void ParseAttributeDeclaration()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                attribute MyAttr
                {
                    Int32 Level;
                };
            }
            """);

        Assert.Empty(compilation.Diagnostics);
    }

    #endregion

    #region Binding

    [Fact]
    public void BindAttributeType()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                attribute MyAttr
                {
                    Int32 Level;
                };
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var attrType = ns.Members.OfType<AttributeTypeSymbol>().SingleOrDefault();
        Assert.NotNull(attrType);
        Assert.Equal("MyAttr", attrType.Name);
        Assert.Equal("TestApp.MyAttr", attrType.FullName);
        Assert.Equal(SymbolKind.Attribute, attrType.Kind);
        Assert.Single(attrType.Fields);
        Assert.Equal("Level", attrType.Fields[0].Name);
    }

    [Fact]
    public void AttributeWithMultipleFields()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                attribute A
                {
                    Int32 X;
                    String Y;
                };
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var attrType = ns.Members.OfType<AttributeTypeSymbol>().Single();
        Assert.Equal(2, attrType.Fields.Count);
        Assert.Equal("X", attrType.Fields[0].Name);
        Assert.Equal("Y", attrType.Fields[1].Name);
    }

    #endregion

    #region Attribute Metadata Extraction

    [Fact]
    public void AttributeUsageExtracted()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr
                {
                    Int32 Level;
                };
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var attrType = ns.Members.OfType<AttributeTypeSymbol>().Single();
        Assert.Contains("target_runtimeclass", attrType.ValidTargets);
    }

    [Fact]
    public void AttributeNameOverride()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributename("MyCustom")]
                attribute MyCustomAttribute
                {
                    Int32 Level;
                };
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var attrType = ns.Members.OfType<AttributeTypeSymbol>().Single();
        Assert.Equal("MyCustom", attrType.AttributeNameOverride);
    }

    [Fact]
    public void AllowMultipleExtracted()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [allowmultiple]
                attribute MyAttr
                {
                    Int32 Level;
                };
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var attrType = ns.Members.OfType<AttributeTypeSymbol>().Single();
        Assert.True(attrType.AllowMultiple);
    }

    #endregion

    #region Validation

    [Fact]
    public void WrongTargetDiagnostic()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr
                {
                    Int32 Level;
                };

                [MyAttr(42)]
                struct MyStruct
                {
                    Int32 Value;
                };
            }
            """);

        var error = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1029");
        Assert.NotNull(error);
        Assert.Contains("not valid on target_struct", error.Message);
    }

    [Fact]
    public void WrongArgCountDiagnostic()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr
                {
                    Int32 X;
                    String Y;
                };

                [MyAttr(42)]
                runtimeclass MyClass
                {
                    MyClass();
                }
            }
            """);

        var error = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1030");
        Assert.NotNull(error);
        Assert.Contains("expects 2 arguments but got 1", error.Message);
    }

    [Fact]
    public void ValidAttributeUsageNoDiagnostic()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute AwesomeAttribute
                {
                    Int32 Level;
                    String Description;
                };

                [Awesome(42, "very cool")]
                runtimeclass MyClass
                {
                    MyClass();
                }
            }
            """);

        // No MIDL1029 or MIDL1030 errors
        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Code == "MIDL1029" || d.Code == "MIDL1030");
    }

    #endregion

    #region WinMD Round-Trip

    [Fact]
    public void AttributeInWinMdRoundTrip()
    {
        var idl = """
            namespace TestNs
            {
                attribute TestAttr
                {
                    Int32 Level;
                    String Description;
                };
            }
            """;

        var compilation = CompilationHelper.Compile(idl);
        Assert.Empty(compilation.Diagnostics);

        // Emit to WinMD
        var emitter = new WinMdEmitter();
        var winmdBytes = emitter.EmitToBytes(compilation);

        // Read back
        var reader = new WinMdReader();
        var types = reader.ReadTypes(winmdBytes);

        var attrSymbol = types.OfType<AttributeTypeSymbol>().FirstOrDefault(a => a.Name == "TestAttr");
        Assert.NotNull(attrSymbol);
        Assert.Equal("TestNs.TestAttr", attrSymbol.FullName);
        Assert.True(attrSymbol.IsExternal);
        Assert.Equal(2, attrSymbol.Fields.Count);
        Assert.Equal("Level", attrSymbol.Fields[0].Name);
        Assert.Equal("Description", attrSymbol.Fields[1].Name);
    }

    #endregion
}
