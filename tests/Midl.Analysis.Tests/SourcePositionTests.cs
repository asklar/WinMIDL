using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis.Tests;

/// <summary>
/// Tests that diagnostic positions point to the correct source location
/// when multiple declarations share the same name.
/// </summary>
public class SourcePositionTests
{
    [Fact]
    public void DiagnosticPointsToRuntimeClassNotNamespace_WhenNamesSame()
    {
        // namespace X { runtimeclass X { } }
        // MIDL1062 (empty runtimeclass) should squiggle the runtimeclass X, not the namespace X
        var idl = "namespace X { runtimeclass X { } }";

        var compilation = CompilationHelper.Compile(idl);

        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1062");
        Assert.NotNull(diag);

        // The namespace "X" is at index 10, but the runtimeclass "X" is at index 26
        int nsPos = idl.IndexOf("X");           // first X = namespace name
        int rcPos = idl.IndexOf("X", nsPos + 1); // second X = runtimeclass name

        Assert.True(diag.Position >= rcPos,
            $"Diagnostic position {diag.Position} should be at runtimeclass 'X' (pos {rcPos}), not namespace 'X' (pos {nsPos})");
    }

    [Fact]
    public void DuplicateDefinitionPointsToSecondOccurrence()
    {
        // namespace N { runtimeclass Foo { void M(); } runtimeclass Foo { void N(); } }
        // MIDL1002 (duplicate) should point to the SECOND Foo, not the first
        var idl = "namespace N { runtimeclass Foo { void M(); } runtimeclass Foo { void N(); } }";

        var compilation = CompilationHelper.Compile(idl);

        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1002");
        Assert.NotNull(diag);

        int firstFoo = idl.IndexOf("Foo");
        int secondFoo = idl.IndexOf("Foo", firstFoo + 1);
        Assert.True(secondFoo > firstFoo, "Test setup: should have two 'Foo' tokens");

        Assert.True(diag.Position >= secondFoo,
            $"Diagnostic position {diag.Position} should be at second 'Foo' (pos {secondFoo}), not first 'Foo' (pos {firstFoo})");
    }

    [Fact]
    public void CrossNamespaceDiagnosticsPointToCorrectPositions()
    {
        // namespace A { runtimeclass B { } } namespace B { runtimeclass A { } }
        // Each type's diagnostics should point to the type declaration, not the namespace
        var idl = "namespace A { runtimeclass B { } } namespace B { runtimeclass A { } }";

        var compilation = CompilationHelper.Compile(idl);

        // Both runtimeclasses should produce MIDL1062 (empty runtimeclass)
        var diags = compilation.Diagnostics.Where(d => d.Code == "MIDL1062").ToList();
        Assert.True(diags.Count >= 2, $"Expected at least 2 MIDL1062 diagnostics, got {diags.Count}");

        // runtimeclass B is inside "namespace A { runtimeclass B ... }"
        int rcB = idl.IndexOf("runtimeclass B") + "runtimeclass ".Length;
        // runtimeclass A is inside "namespace B { runtimeclass A ... }"
        int rcA = idl.IndexOf("runtimeclass A") + "runtimeclass ".Length;

        // Find the diagnostic for type "B" and type "A"
        var diagB = diags.FirstOrDefault(d => d.Message.Contains("'B'"));
        var diagA = diags.FirstOrDefault(d => d.Message.Contains("'A'"));
        Assert.NotNull(diagB);
        Assert.NotNull(diagA);

        Assert.Equal(rcB, diagB.Position);
        Assert.Equal(rcA, diagA.Position);
    }

    [Fact]
    public void SymbolSourcePositionIsSetDuringBinding()
    {
        var idl = "namespace N { runtimeclass Foo { void M(); } }";

        var compilation = CompilationHelper.Compile(idl);

        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single();

        int expectedPos = idl.IndexOf("Foo");
        Assert.True(rc.SourcePosition >= 0, "SourcePosition should be set during binding");
        Assert.Equal(expectedPos, rc.SourcePosition);
    }

    [Fact]
    public void InterfaceSourcePositionIsSet()
    {
        var idl = "namespace N { interface IFoo { void M(); } }";

        var compilation = CompilationHelper.Compile(idl);

        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var iface = ns.Members.OfType<InterfaceSymbol>().Single();

        int expectedPos = idl.IndexOf("IFoo");
        Assert.True(iface.SourcePosition >= 0, "SourcePosition should be set during binding");
        Assert.Equal(expectedPos, iface.SourcePosition);
    }

    [Fact]
    public void EnumSourcePositionIsSet()
    {
        var idl = "namespace N { enum E { A, B } }";

        var compilation = CompilationHelper.Compile(idl);

        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var en = ns.Members.OfType<EnumSymbol>().Single();

        int expectedPos = idl.IndexOf("E ");
        Assert.True(en.SourcePosition >= 0, "SourcePosition should be set during binding");
        Assert.Equal(expectedPos, en.SourcePosition);
    }

    [Fact]
    public void StructSourcePositionIsSet()
    {
        var idl = "namespace N { struct S { Int32 X; } }";

        var compilation = CompilationHelper.Compile(idl);

        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var s = ns.Members.OfType<StructSymbol>().Single();

        int expectedPos = idl.IndexOf("S ");
        Assert.True(s.SourcePosition >= 0, "SourcePosition should be set during binding");
        Assert.Equal(expectedPos, s.SourcePosition);
    }

    [Fact]
    public void DelegateSourcePositionIsSet()
    {
        var idl = "namespace N { delegate void MyDel(); }";

        var compilation = CompilationHelper.Compile(idl);

        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var d = ns.Members.OfType<DelegateSymbol>().Single();

        int expectedPos = idl.IndexOf("MyDel");
        Assert.True(d.SourcePosition >= 0, "SourcePosition should be set during binding");
        Assert.Equal(expectedPos, d.SourcePosition);
    }

    [Fact]
    public void StaticUnsealedConflictPointsToRuntimeClass()
    {
        // MIDL1061: static + unsealed conflict should point to the runtimeclass name
        var idl = "namespace X { static unsealed runtimeclass X { static void M(); } }";

        var compilation = CompilationHelper.Compile(idl);

        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1061");
        Assert.NotNull(diag);

        // Should point to the runtimeclass X, not the namespace X
        int nsX = idl.IndexOf("X");
        int rcX = idl.IndexOf("X", nsX + 1);

        Assert.True(diag.Position >= rcX,
            $"Diagnostic position {diag.Position} should be at runtimeclass 'X' (pos {rcX}), not namespace 'X' (pos {nsX})");
    }
}
