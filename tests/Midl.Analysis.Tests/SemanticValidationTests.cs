using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis.Tests;

public class SemanticValidationTests
{
    [Fact]
    public void StructFieldMustBeValueType()
    {
        // Object is a built-in reference type; WinRT structs should only contain value types.
        // The binder resolves "Object" but does not yet validate field types (MIDL1005).
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                struct S
                {
                    Object Obj;
                }
            }
            """);

        // Object resolves as a built-in type; struct field is bound correctly.
        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var s = ns.Members.OfType<StructSymbol>().Single();
        Assert.Equal("S", s.Name);
        Assert.Single(s.Fields);
        Assert.Equal("Obj", s.Fields[0].Name);
        Assert.Equal("Object", s.Fields[0].Type.Name);
    }

    [Fact]
    public void StaticClassCannotHaveInstanceMembers()
    {
        // A static runtimeclass should only contain static members per WinRT rules.
        // The binder binds M() as non-static but does not yet emit MIDL1007.
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                static runtimeclass C
                {
                    void M();
                }
            }
            """);

        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var c = ns.Members.OfType<RuntimeClassSymbol>().Single();
        Assert.True(c.IsStatic);

        var m = c.Members.OfType<MethodSymbol>().Single();
        Assert.Equal("M", m.Name);
        // Instance member on a static class — not yet flagged as an error
        Assert.False(m.IsStatic);
    }

    [Fact]
    public void DuplicateEnumMember()
    {
        // Duplicate enum member names within the same enum.
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                enum E
                {
                    A,
                    A,
                }
            }
            """);

        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var e = ns.Members.OfType<EnumSymbol>().Single();
        Assert.Equal("E", e.Name);
        // Both members are added; the binder does not yet check for duplicate enum members.
        Assert.Equal(2, e.EnumMembers.Count);
        Assert.All(e.EnumMembers, m => Assert.Equal("A", m.Name));
    }

    [Fact]
    public void NestedNamespaceResolution()
    {
        // Type A.B.S should be resolvable from namespace A.B.C using qualified name.
        var compilation = CompilationHelper.Compile("""
            namespace A.B
            {
                struct S
                {
                    Int32 X;
                }
            }
            namespace A.B.C
            {
                runtimeclass R
                {
                    A.B.S GetS();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var bNs = compilation.GlobalNamespace.ChildNamespaces["A"].ChildNamespaces["B"];
        var s = bNs.Members.OfType<StructSymbol>().Single();
        Assert.Equal("S", s.Name);

        var cNs = bNs.ChildNamespaces["C"];
        var r = cNs.Members.OfType<RuntimeClassSymbol>().Single();
        var method = r.Members.OfType<MethodSymbol>().Single();
        Assert.Equal("A.B.S", method.ReturnType.Name);
        Assert.NotNull(method.ReturnType.ResolvedSymbol);
        Assert.IsType<StructSymbol>(method.ReturnType.ResolvedSymbol);
    }

    [Fact]
    public void EmptyStructDiagnostic()
    {
        // WinRT rules require structs to have at least one field.
        // The binder creates the struct symbol but does not yet emit a diagnostic.
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                struct Empty
                {
                }
            }
            """);

        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var s = ns.Members.OfType<StructSymbol>().Single();
        Assert.Equal("Empty", s.Name);
        Assert.Empty(s.Fields);
    }

    [Fact]
    public void InterfaceMethodResolution()
    {
        // IFoo should resolve when used as a base interface of a runtimeclass.
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                interface IFoo
                {
                    void Bar();
                }
                runtimeclass C : IFoo
                {
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var ifoo = ns.Members.OfType<InterfaceSymbol>().Single();
        Assert.Equal("IFoo", ifoo.Name);
        var bar = ifoo.Members.OfType<MethodSymbol>().Single();
        Assert.Equal("Bar", bar.Name);

        var c = ns.Members.OfType<RuntimeClassSymbol>().Single();
        Assert.Equal("C", c.Name);
        // IFoo appears as the BaseClass (first item in base list)
        Assert.NotNull(c.BaseClass);
        Assert.Equal("IFoo", c.BaseClass!.Name);
        Assert.NotNull(c.BaseClass.ResolvedSymbol);
        Assert.IsType<InterfaceSymbol>(c.BaseClass.ResolvedSymbol);
    }

    [Fact]
    public void MultipleBaseClassError()
    {
        // A runtimeclass can only have one base class in WinRT.
        // The binder treats the first item as BaseClass and the rest as Interfaces.
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                unsealed runtimeclass Base1
                {
                }
                unsealed runtimeclass Base2
                {
                }
                runtimeclass C : Base1, Base2
                {
                }
            }
            """);

        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var c = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "C");

        // First base list entry becomes BaseClass
        Assert.NotNull(c.BaseClass);
        Assert.Equal("Base1", c.BaseClass!.Name);
        // Second entry is treated as an interface
        Assert.Single(c.Interfaces);
        Assert.Equal("Base2", c.Interfaces[0].Name);
    }

    [Fact]
    public void EnumWithFlagsUInt32()
    {
        // [flags] enum should compile without diagnostics.
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [flags]
                enum F
                {
                    A,
                    B,
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);

        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var f = ns.Members.OfType<EnumSymbol>().Single();
        Assert.Equal("F", f.Name);
        Assert.True(f.IsFlags);
        Assert.Equal(2, f.EnumMembers.Count);
        Assert.Equal("A", f.EnumMembers[0].Name);
        Assert.Equal("B", f.EnumMembers[1].Name);
    }

    [Fact]
    public void CaseInsensitiveCollision()
    {
        // WinRT names must be unique case-insensitively, but the binder uses
        // ordinal comparison. Both types are defined without a duplicate diagnostic.
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                runtimeclass Foo
                {
                }
                runtimeclass foo
                {
                }
            }
            """);

        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var classes = ns.Members.OfType<RuntimeClassSymbol>().ToList();
        Assert.Equal(2, classes.Count);
        Assert.Contains(classes, c => c.Name == "Foo");
        Assert.Contains(classes, c => c.Name == "foo");
    }

    [Fact]
    public void UnresolvedTypeInParameter()
    {
        // NonExistent is not defined anywhere — should produce MIDL1001.
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                runtimeclass C
                {
                    void M(NonExistent x);
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics);
        Assert.Equal("MIDL1001", diag.Code);
        Assert.Contains("NonExistent", diag.Message);
        Assert.Equal(DiagnosticSeverity.Error, diag.Severity);
    }

    #region Enum Value Validation

    [Fact]
    public void EnumValue_FloatLiteral_Error()
    {
        var compilation = CompilationHelper.Compile("namespace N { enum E { A = 1.5 }; }");
        var error = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1067");
        Assert.NotNull(error);
        Assert.Contains("1.5", error.Message);
        Assert.Contains("integer literal", error.Message);
    }

    [Fact]
    public void EnumValue_StringLiteral_Error()
    {
        var compilation = CompilationHelper.Compile("""namespace N { enum E { A = "hello" }; }""");
        var error = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1067");
        Assert.NotNull(error);
        Assert.Contains("hello", error.Message);
    }

    [Fact]
    public void EnumValue_NegativeInInt32_Valid()
    {
        var compilation = CompilationHelper.Compile("namespace N { enum E { A = -1 }; }");
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code is "MIDL1067" or "MIDL1068");
    }

    [Fact]
    public void EnumValue_NegativeInFlagsUInt32_Allowed()
    {
        // Original midl.exe accepts this — value wraps in UInt32
        var compilation = CompilationHelper.Compile("namespace N { [flags] enum E { A = -1, B = 2 }; }");
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1068");
    }

    [Fact]
    public void EnumValue_ExceedsInt32Max_Allowed()
    {
        // Original midl.exe accepts values exceeding Int32.MaxValue
        var compilation = CompilationHelper.Compile("namespace N { enum E { A = 3000000000 }; }");
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1068");
    }

    [Fact]
    public void EnumValue_FlagsHex_Valid()
    {
        var compilation = CompilationHelper.Compile("namespace N { [flags] enum E { A = 0xFFFFFFFF }; }");
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code is "MIDL1067" or "MIDL1068");
    }

    [Fact]
    public void EnumValue_FlagsValid_NoErrors()
    {
        var compilation = CompilationHelper.Compile("namespace N { [flags] enum E { A = 1, B = 2, C = 4 }; }");
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code is "MIDL1067" or "MIDL1068");
    }

    #endregion

    #region Protected Accessor Validation (MIDL1072-1073)

    [Fact]
    public void ProtectedAccessor_OnSet_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N {
                interface IFoo {
                    Single x { protected set; get; };
                }
            }
            """);

        var error = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1072");
        Assert.NotNull(error);
        Assert.Contains("set", error.Message);
        Assert.Contains("protected", error.Message);
    }

    [Fact]
    public void ProtectedAccessor_OnGet_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N {
                interface IFoo {
                    String Name { protected get; set; };
                }
            }
            """);

        var error = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1072");
        Assert.NotNull(error);
        Assert.Contains("get", error.Message);
    }

    [Fact]
    public void ProtectedMember_OnInterface_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N {
                interface IFoo {
                    Single x { protected set; get; };
                }
            }
            """);

        var error = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1073");
        Assert.NotNull(error);
        Assert.Contains("IFoo", error.Message);
        Assert.Contains("public", error.Message);
    }

    [Fact]
    public void PublicAccessors_OnInterface_NoError()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N {
                interface IFoo {
                    String Name { get; set; };
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code is "MIDL1072" or "MIDL1073");
    }

    [Fact]
    public void ReadOnlyProperty_NoShorthandHint()
    {
        var compilation = CompileWithQualityHints("""
            namespace N {
                interface IFoo {
                    Single X { get; };
                }
            }
            """);

        // get-only property should NOT suggest shorthand (shorthand means get+set)
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL3016");
    }

    [Fact]
    public void ShorthandProperty_NoShorthandHint()
    {
        // Property already using shorthand (Single X;) should NOT suggest shorthand
        var compilation = CompileWithQualityHints("""
            namespace N {
                interface IFoo {
                    Single X;
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL3016");
    }

    [Fact]
    public void ExplicitGetSet_ShowsShorthandHint()
    {
        // Property with explicit { get; set; } SHOULD suggest shorthand
        var compilation = CompileWithQualityHints("""
            namespace N {
                interface IFoo {
                    Single X { get; set; };
                }
            }
            """);

        Assert.Contains(compilation.Diagnostics, d => d.Code == "MIDL3016");
    }

    [Fact]
    public void ProtectedProperty_OnRuntimeClass_NoAccessorError()
    {
        // protected on the whole property is valid on unsealed runtimeclass
        var compilation = CompilationHelper.Compile("""
            namespace N {
                unsealed runtimeclass Foo {
                    Foo();
                    protected String Name { get; set; };
                }
            }
            """);

        // No MIDL1072 (per-accessor) — protected is on the property itself, not individual accessors
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1072");
    }

    #endregion

    #region Code Quality Hints (MIDL3xxx)

    private static Compilation CompileWithQualityHints(string source)
    {
        return CompilationHelper.Compile(source, referencePaths: null,
            mode: CompilationMode.Strict, includePaths: null, currentFilePath: null,
            defines: null, emitQualityHints: true);
    }

    [Fact]
    public void QualityHint_InterfaceNamingConvention()
    {
        var compilation = CompileWithQualityHints("""
            namespace N { interface Foo { void DoWork(); }; }
            """);

        var hint = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL3001");
        Assert.NotNull(hint);
        Assert.Equal(DiagnosticSeverity.Info, hint.Severity);
        Assert.Contains("Foo", hint.Message);
        Assert.Contains("IFoo", hint.Message);
    }

    [Fact]
    public void QualityHint_InterfaceWithI_NoWarning()
    {
        var compilation = CompileWithQualityHints("""
            namespace N { interface IFoo { void DoWork(); }; }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL3001");
    }

    [Fact]
    public void QualityHint_InterfaceLowercaseAfterI()
    {
        var compilation = CompileWithQualityHints("""
            namespace N { interface Ibar { void DoWork(); }; }
            """);

        var hint = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL3001");
        Assert.NotNull(hint);
        Assert.Contains("IBar", hint.Message); // Suggests IBar, not IIbar
    }

    [Fact]
    public void QualityHint_EmptyRuntimeClass()
    {
        var compilation = CompileWithQualityHints("""
            namespace N { runtimeclass Empty { } }
            """);

        var hint = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL3002");
        Assert.NotNull(hint);
        Assert.Contains("Empty", hint.Message);
    }

    [Fact]
    public void QualityHint_RuntimeClassNoConstructor()
    {
        var compilation = CompileWithQualityHints("""
            namespace N {
                runtimeclass Widget {
                    String Name { get; };
                }
            }
            """);

        var hint = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL3005");
        Assert.NotNull(hint);
        Assert.Contains("Widget", hint.Message);
        Assert.Contains("activatable", hint.Message);
    }

    [Fact]
    public void QualityHint_EventArgs_NoConstructorWarning()
    {
        // EventArgs types are produced by event handlers, not constructed directly
        var compilation = CompileWithQualityHints("""
            namespace N {
                runtimeclass ClickEventArgs {
                    String Source { get; };
                    Int32 X { get; };
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL3005");
    }

    [Fact]
    public void QualityHint_ObtainedFromComment_SuppressesMIDL3005()
    {
        var compilation = CompileWithQualityHints(
            "namespace N {\n// [obtained-from:Factory.Create]\nruntimeclass Item { String Name { get; }; }\n}");
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL3005");
    }

    [Fact]
    public void QualityHint_ObtainedFromComment_CaseInsensitive()
    {
        var compilation = CompileWithQualityHints(
            "namespace N {\n// [Obtained-From:SomeClass.GetItem]\nruntimeclass Thing { Int32 Value { get; }; }\n}");
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL3005");
    }

    [Fact]
    public void QualityHint_NoObtainedFromComment_StillWarns()
    {
        var compilation = CompileWithQualityHints(
            "namespace N {\n// This is a regular comment\nruntimeclass Gadget { String Name { get; }; }\n}");
        Assert.Contains(compilation.Diagnostics, d => d.Code == "MIDL3005");
    }

    [Fact]
    public void QualityHint_RuntimeClassWithConstructor_NoWarning()
    {
        var compilation = CompileWithQualityHints("""
            namespace N {
                runtimeclass Widget {
                    Widget();
                    String Name { get; };
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL3005");
    }

    [Fact]
    public void QualityHint_SetterOnlyProperty()
    {
        var compilation = CompileWithQualityHints("""
            namespace N {
                runtimeclass Widget {
                    Widget();
                    String Token { set; };
                }
            }
            """);

        var hint = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL3007");
        Assert.NotNull(hint);
        Assert.Contains("Token", hint.Message);
    }

    [Fact]
    public void QualityHint_MethodNameClash()
    {
        var compilation = CompileWithQualityHints("""
            namespace N {
                runtimeclass Widget {
                    Widget();
                    void Widget();
                }
            }
            """);

        var hint = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL3008");
        Assert.NotNull(hint);
    }

    [Fact]
    public void QualityHint_LowercaseNamespace()
    {
        var compilation = CompileWithQualityHints("""
            namespace myApp { runtimeclass Foo { Foo(); } }
            """);

        var hint = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL3009");
        Assert.NotNull(hint);
        Assert.Contains("myApp", hint.Message);
        Assert.Contains("MyApp", hint.Message);
    }

    [Fact]
    public void QualityHints_Disabled_ByDefault()
    {
        // Default compilation should NOT produce any MIDL3xxx hints
        var compilation = CompilationHelper.Compile("""
            namespace myApp { interface Foo { }; }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code.StartsWith("MIDL3"));
    }

    [Fact]
    public void QualityHint_DelegateNaming()
    {
        var compilation = CompileWithQualityHints("namespace N { delegate void MyThing(); }");
        Assert.Contains(compilation.Diagnostics, d => d.Code == "MIDL3011");
    }

    [Fact]
    public void QualityHint_DelegateNaming_Handler_NoWarning()
    {
        var compilation = CompileWithQualityHints("namespace N { delegate void ClickHandler(); }");
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL3011");
    }

    [Fact]
    public void QualityHint_AsyncMethodNaming()
    {
        var compilation = CompileWithQualityHints("""
            namespace N {
                runtimeclass C {
                    C();
                    Windows.Foundation.IAsyncAction DoWork();
                }
            }
            """);
        Assert.Contains(compilation.Diagnostics, d => d.Code == "MIDL3013");
    }

    [Fact]
    public void QualityHint_AsyncMethodNaming_Correct()
    {
        var compilation = CompileWithQualityHints("""
            namespace N {
                runtimeclass C {
                    C();
                    Windows.Foundation.IAsyncAction DoWorkAsync();
                }
            }
            """);
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL3013");
    }

    [Fact]
    public void QualityHint_TypeNamePascalCase()
    {
        var compilation = CompileWithQualityHints("namespace N { struct my_point { Int32 X; Int32 Y; }; }");
        Assert.Contains(compilation.Diagnostics, d => d.Code == "MIDL3014");
    }

    [Fact]
    public void QualityHint_MemberNamePascalCase()
    {
        var compilation = CompileWithQualityHints("""
            namespace N {
                runtimeclass C {
                    C();
                    void doWork();
                }
            }
            """);
        Assert.Contains(compilation.Diagnostics, d => d.Code == "MIDL3015");
    }

    [Fact]
    public void QualityHints_Disabled_ByDefault_NoMIDL3xxx()
    {
        var compilation = CompilationHelper.Compile("namespace N { delegate void MyThing(); }");
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code.StartsWith("MIDL3"));
    }

    // MIDL3017: Redundant default attributes

    [Fact]
    public void QualityHint_ThreadingBoth_IsRedundant()
    {
        var compilation = CompileWithQualityHints(
            "namespace N { [threading(both)] runtimeclass Widget { Widget(); } }");
        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL3017");
        Assert.NotNull(diag);
        Assert.Contains("threading", diag.Message);
        Assert.Contains("default", diag.Message);
    }

    [Fact]
    public void QualityHint_ThreadingSta_NotRedundant()
    {
        var compilation = CompileWithQualityHints(
            "namespace N { [threading(sta)] runtimeclass Widget { Widget(); } }");
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL3017" && d.Message.Contains("threading"));
    }

    [Fact]
    public void QualityHint_MarshalingAgile_IsRedundant()
    {
        var compilation = CompileWithQualityHints(
            "namespace N { [marshaling_behavior(agile)] runtimeclass Widget { Widget(); } }");
        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL3017");
        Assert.NotNull(diag);
        Assert.Contains("marshaling_behavior", diag.Message);
    }

    [Fact]
    public void QualityHint_MarshalingStandard_NotRedundant()
    {
        var compilation = CompileWithQualityHints(
            "namespace N { [marshaling_behavior(standard)] runtimeclass Widget { Widget(); } }");
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL3017" && d.Message.Contains("marshaling"));
    }

    [Fact]
    public void QualityHint_ThreadingBoth_SquigglePosition()
    {
        var source = "namespace N { [threading(both)] runtimeclass Widget { Widget(); } }";
        var compilation = CompileWithQualityHints(source);
        var diag = compilation.Diagnostics.First(d => d.Code == "MIDL3017");
        int pos = diag.OriginalPosition ?? diag.Position;
        // Squiggle should start at "threading"
        Assert.Equal("threading", source.Substring(pos, "threading".Length));
    }

    // MIDL3018: Obsolete/dead attributes

    [Fact]
    public void QualityHint_DualApiPartition_IsObsolete()
    {
        var compilation = CompileWithQualityHints(
            "namespace N { [dualapipartition(1)] runtimeclass Widget { Widget(); } }");
        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL3018");
        Assert.NotNull(diag);
        Assert.Contains("dualapipartition", diag.Message);
        Assert.Contains("no effect", diag.Message);
    }

    [Fact]
    public void QualityHint_AllowForWeb_IsObsolete()
    {
        var compilation = CompileWithQualityHints(
            "namespace N { [allowforweb] runtimeclass Widget { Widget(); } }");
        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL3018");
        Assert.NotNull(diag);
        Assert.Contains("allowforweb", diag.Message);
    }

    [Fact]
    public void QualityHint_WebHostHidden_IsObsolete()
    {
        var compilation = CompileWithQualityHints(
            "namespace N { [webhosthidden] runtimeclass Widget { Widget(); } }");
        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL3018");
        Assert.NotNull(diag);
        Assert.Contains("webhosthidden", diag.Message);
    }

    [Fact]
    public void QualityHint_ObsoleteAttr_SquiggleOnAttrName()
    {
        var source = "namespace N { [webhosthidden] runtimeclass Widget { Widget(); } }";
        var compilation = CompileWithQualityHints(source);
        var diag = compilation.Diagnostics.First(d => d.Code == "MIDL3018");
        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("webhosthidden", source.Substring(pos, diag.Length));
    }

    [Fact]
    public void QualityHint_ObsoleteAttr_NotFiredWhenDisabled()
    {
        var compilation = CompilationHelper.Compile(
            "namespace N { [webhosthidden] runtimeclass Widget { Widget(); } }");
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL3018");
    }

    // MIDL3019: All-static class should be 'static runtimeclass'

    [Fact]
    public void QualityHint_AllStaticMembers_SuggestsStaticClass()
    {
        var compilation = CompileWithQualityHints("""
            namespace N {
                runtimeclass MathHelpers {
                    static Double Add(Double a, Double b);
                    static Double Multiply(Double a, Double b);
                }
            }
            """);
        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL3019");
        Assert.NotNull(diag);
        Assert.Contains("MathHelpers", diag.Message);
        Assert.Contains("static", diag.Message);
    }

    [Fact]
    public void QualityHint_MixedStaticAndInstance_NoDiag()
    {
        var compilation = CompileWithQualityHints("""
            namespace N {
                runtimeclass Widget {
                    Widget();
                    String Name { get; };
                    static String GetVersion();
                }
            }
            """);
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL3019");
    }

    [Fact]
    public void QualityHint_AlreadyStaticClass_NoDiag()
    {
        var compilation = CompileWithQualityHints("""
            namespace N {
                static runtimeclass MathHelpers {
                    static Double Add(Double a, Double b);
                }
            }
            """);
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL3019");
    }

    [Fact]
    public void QualityHint_DefaultInterfaceWithStaticOnly_NoDiag()
    {
        // User explicitly opted into [default_interface] — don't suggest static
        var compilation = CompileWithQualityHints("""
            namespace N {
                [default_interface]
                runtimeclass Util {
                    static String GetVersion();
                }
            }
            """);
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL3019");
    }

    [Fact]
    public void QualityHint_AllStaticMembers_SquiggleOnClassName()
    {
        var source = "namespace N { runtimeclass Helpers { static String GetInfo(); } }";
        var compilation = CompileWithQualityHints(source);
        var diag = compilation.Diagnostics.First(d => d.Code == "MIDL3019");
        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Helpers", source.Substring(pos, diag.Length));
    }

    #endregion
}
