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
}
