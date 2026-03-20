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

        // No MIDL1029, MIDL1030, or MIDL1064 errors
        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Code == "MIDL1029" || d.Code == "MIDL1030" || d.Code == "MIDL1064");
    }

    [Fact]
    public void WrongArgCountDiagnosticPointsToUsageSite()
    {
        // Bug fix: MIDL1030 squiggly must appear at the usage site [MyAttr(42)],
        // not at the attribute definition "attribute MyAttr"
        var idl = """
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
            """;

        var compilation = CompilationHelper.Compile(idl);

        var error = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1030");
        Assert.NotNull(error);
        Assert.Contains("expects 2 arguments but got 1", error.Message);

        // The position must be AFTER the attribute definition, at the usage site
        // The attribute definition "attribute MyAttr" is near position ~80
        // The usage "[MyAttr(42)]" is near position ~180+
        int defPos = idl.IndexOf("attribute MyAttr");
        Assert.True(error.Position > defPos,
            $"Diagnostic position {error.Position} should be after the attribute definition at {defPos}");

        // Position should be near the usage
        int usagePos = idl.IndexOf("[MyAttr(42)]");
        Assert.True(error.Position >= usagePos,
            $"Diagnostic position {error.Position} should be at or after usage site at {usagePos}");
    }

    [Fact]
    public void WrongArgTypeDiagnostic_StringInsteadOfInt()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr
                {
                    Int32 Level;
                };

                [MyAttr("hello")]
                runtimeclass MyClass
                {
                    MyClass();
                }
            }
            """);

        var error = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1064");
        Assert.NotNull(error);
        Assert.Contains("Level", error.Message);
        Assert.Contains("Int32", error.Message);
        Assert.Contains("string literal", error.Message);
    }

    [Fact]
    public void WrongArgTypeDiagnostic_IntInsteadOfString()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr
                {
                    String Name;
                };

                [MyAttr(42)]
                runtimeclass MyClass
                {
                    MyClass();
                }
            }
            """);

        var error = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1064");
        Assert.NotNull(error);
        Assert.Contains("Name", error.Message);
        Assert.Contains("String", error.Message);
        Assert.Contains("numeric literal", error.Message);
    }

    [Fact]
    public void WrongArgTypeDiagnostic_SwappedArgs()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr
                {
                    Int32 Level;
                    String Description;
                };

                [MyAttr("hello", 42)]
                runtimeclass MyClass
                {
                    MyClass();
                }
            }
            """);

        // Both arguments have wrong types
        var errors = compilation.Diagnostics.Where(d => d.Code == "MIDL1064").ToList();
        Assert.Equal(2, errors.Count);
        Assert.Contains(errors, e => e.Message.Contains("Level"));
        Assert.Contains(errors, e => e.Message.Contains("Description"));
    }

    [Fact]
    public void CorrectArgTypes_NoDiagnostic()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr
                {
                    Int32 Level;
                    String Description;
                };

                [MyAttr(42, "very cool")]
                runtimeclass MyClass
                {
                    MyClass();
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1064");
    }

    [Fact]
    public void FloatLiteral_ForIntField_ProducesError()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_interface)]
                attribute foo
                {
                    Int32 x;
                };

                [foo(1.1)]
                interface bar
                {
                    Single x { set; get; };
                }
            }
            """);

        var error = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1064");
        Assert.NotNull(error);
        Assert.Contains("x", error.Message);
        Assert.Contains("Int32", error.Message);
        Assert.Contains("float literal", error.Message);
    }

    [Fact]
    public void FloatLiteral_ForSingleField_NoDiagnostic()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr
                {
                    Single Value;
                };

                [MyAttr(3.14)]
                runtimeclass MyClass
                {
                    MyClass();
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1064");
    }

    [Fact]
    public void FloatLiteral_ForDoubleField_NoDiagnostic()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr
                {
                    Double Value;
                };

                [MyAttr(2.718)]
                runtimeclass MyClass
                {
                    MyClass();
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1064");
    }

    [Fact]
    public void FloatLiteral_ForStringField_ProducesError()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr
                {
                    String Name;
                };

                [MyAttr(9.99)]
                runtimeclass MyClass
                {
                    MyClass();
                }
            }
            """);

        var error = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1064");
        Assert.NotNull(error);
        Assert.Contains("Name", error.Message);
        Assert.Contains("String", error.Message);
        Assert.Contains("float literal", error.Message);
    }

    [Fact]
    public void IntegerLiteral_ForSingleField_IsAccepted()
    {
        // Integer literal should be accepted for float fields (implicit widening)
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr
                {
                    Single Value;
                };

                [MyAttr(42)]
                runtimeclass MyClass
                {
                    MyClass();
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1064");
    }

    [Fact]
    public void NegativeInteger_ForIntField_Accepted()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr { Int32 Value; };
                [MyAttr(-1)]
                runtimeclass Foo { Foo(); }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code is "MIDL1064" or "MIDL1066");
    }

    [Fact]
    public void NegativeFloat_ForDoubleField_Accepted()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr { Double Value; };
                [MyAttr(-3.14)]
                runtimeclass Foo { Foo(); }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code is "MIDL1064" or "MIDL1066");
    }

    [Fact]
    public void NegativeInteger_ForUInt8_OutOfRange()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr { UInt8 Value; };
                [MyAttr(-1)]
                runtimeclass Foo { Foo(); }
            }
            """);

        var error = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1066");
        Assert.NotNull(error);
        Assert.Contains("-1", error.Message);
        Assert.Contains("0..255", error.Message);
    }

    [Fact]
    public void Overflow_UInt8_256()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr { UInt8 Value; };
                [MyAttr(256)]
                runtimeclass Foo { Foo(); }
            }
            """);

        var error = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1066");
        Assert.NotNull(error);
        Assert.Contains("256", error.Message);
    }

    [Fact]
    public void Overflow_UInt32_Negative()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr { UInt32 Value; };
                [MyAttr(-1)]
                runtimeclass Foo { Foo(); }
            }
            """);

        var error = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1066");
        Assert.NotNull(error);
        Assert.Contains("-1", error.Message);
    }

    [Fact]
    public void Overflow_Int16_40000()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr { Int16 Value; };
                [MyAttr(40000)]
                runtimeclass Foo { Foo(); }
            }
            """);

        var error = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1066");
        Assert.NotNull(error);
    }

    [Fact]
    public void InRange_UInt8_255_NoError()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr { UInt8 Value; };
                [MyAttr(255)]
                runtimeclass Foo { Foo(); }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1066");
    }

    [Fact]
    public void HexLiteral_UInt8_0xFF_Accepted()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr { UInt8 Value; };
                [MyAttr(0xFF)]
                runtimeclass Foo { Foo(); }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code is "MIDL1064" or "MIDL1066");
    }

    [Fact]
    public void HexLiteral_UInt8_0x100_OutOfRange()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr { UInt8 Value; };
                [MyAttr(0x100)]
                runtimeclass Foo { Foo(); }
            }
            """);

        var error = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1066");
        Assert.NotNull(error);
        Assert.Contains("256", error.Message);
        Assert.Contains("0..255", error.Message);
    }

    [Fact]
    public void HexLiteral_UInt32_0xFFFFFFFF_Accepted()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr { UInt32 Value; };
                [MyAttr(0xFFFFFFFF)]
                runtimeclass Foo { Foo(); }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code is "MIDL1064" or "MIDL1066");
    }

    [Fact]
    public void HexLiteral_UInt16_0xFFFF_Accepted()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr { UInt16 Value; };
                [MyAttr(0xFFFF)]
                runtimeclass Foo { Foo(); }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code is "MIDL1064" or "MIDL1066");
    }

    [Fact]
    public void HexLiteral_UInt16_0x10000_OutOfRange()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr { UInt16 Value; };
                [MyAttr(0x10000)]
                runtimeclass Foo { Foo(); }
            }
            """);

        var error = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1066");
        Assert.NotNull(error);
    }

    [Fact]
    public void IdentifierArgForUnknownFieldType_NoDiagnostic()
    {
        // For enum or other non-primitive field types, identifiers should be accepted
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                enum MyEnum { A, B };

                [attributeusage(target_runtimeclass)]
                attribute MyAttr
                {
                    MyEnum Value;
                };

                [MyAttr(A)]
                runtimeclass MyClass
                {
                    MyClass();
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1064");
    }

    [Fact]
    public void UnknownAttributeProducesWarning()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [foo]
                interface bar
                {
                    Single x { set; get; };
                }
            }
            """);

        var warning = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1065");
        Assert.NotNull(warning);
        Assert.Contains("foo", warning.Message);
    }

    [Fact]
    public void KnownAttributeNoWarning()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [uuid("12345678-1234-1234-1234-123456789abc")]
                interface bar
                {
                    Single x { set; get; };
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1065");
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
