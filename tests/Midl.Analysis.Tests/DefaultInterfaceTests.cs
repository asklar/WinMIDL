using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis.Tests;

/// <summary>Tests for [default] attribute in runtimeclass interface lists.</summary>
public class DefaultInterfaceTests
{
    [Fact]
    public void DefaultInterface_FirstPosition_NoErrors()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                interface IFoo { void DoFoo(); }
                interface IBar { void DoBar(); }
                runtimeclass C : [default] IFoo, IBar { }
            }
            """);

        var errors = compilation.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void DefaultInterface_SecondPosition_NoErrors()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                interface IFoo { void DoFoo(); }
                interface IBar { void DoBar(); }
                runtimeclass C : IFoo, [default] IBar { }
            }
            """);

        var errors = compilation.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void DefaultInterface_SingleInterface_NoErrors()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                interface IFoo { void DoFoo(); }
                runtimeclass C : [default] IFoo { }
            }
            """);

        var errors = compilation.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void DefaultInterface_SetsIsDefaultFlag()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                interface IFoo { void DoFoo(); }
                interface IBar { void DoBar(); }
                runtimeclass C : [default] IFoo, IBar { }
            }
            """);

        var errors = compilation.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.Empty(errors);

        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single();

        // After reclassification, an interface in the base list is promoted into
        // Interfaces (with IsDefaultInterface flag preserved). BaseClass stays null
        // so the runtimeclass derives from Object.
        Assert.Null(rc.BaseClass);
        var defaultIface = rc.Interfaces.FirstOrDefault(i => i.IsDefaultInterface);
        Assert.NotNull(defaultIface);
        Assert.Equal("IFoo", defaultIface!.Name);
    }

    [Fact]
    public void DefaultInterface_InBaseList_SetsFlag()
    {
        var source = @"
            namespace N {
                interface IFoo { void DoWork(); }
                interface IBar { void Other(); }
                runtimeclass Foo : [default] IFoo, IBar {}
            }";
        var compilation = CompilationHelper.Compile(source);
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single();
        Assert.Null(rc.BaseClass);
        var defaultIface = rc.Interfaces.FirstOrDefault(i => i.IsDefaultInterface);
        Assert.NotNull(defaultIface);
        Assert.Equal("IFoo", defaultIface!.Name);
    }

    [Fact]
    public void DefaultInterface_WithoutAttribute_FirstIsDefault()
    {
        var source = "namespace N { interface IFoo {} interface IBar {} runtimeclass Foo : IFoo, IBar {} }";
        var compilation = CompilationHelper.Compile(source);
        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single();

        // Without [default], no interface carries the IsDefaultInterface flag and
        // the class has no base class (derives from Object).
        Assert.Null(rc.BaseClass);
        Assert.All(rc.Interfaces, i => Assert.False(i.IsDefaultInterface));
        Assert.Contains(rc.Interfaces, i => i.Name == "IFoo");
        Assert.Contains(rc.Interfaces, i => i.Name == "IBar");
    }
}
