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

        // First item in base list is the base class (IFoo), marked as default
        Assert.NotNull(rc.BaseClass);
        Assert.Equal("IFoo", rc.BaseClass!.Name);
        Assert.True(rc.BaseClass.IsDefaultInterface);
    }
}
