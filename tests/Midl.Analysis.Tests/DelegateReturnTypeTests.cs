using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis.Tests;

/// <summary>Tests for non-void delegate return types.</summary>
public class DelegateReturnTypeTests
{
    [Fact]
    public void Delegate_BooleanReturn_NoErrors()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                delegate Boolean Predicate(String item);
            }
            """);

        var errors = compilation.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Delegate_StringReturn_NoErrors()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                delegate String Converter(Object source);
            }
            """);

        var errors = compilation.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Delegate_Int32Return_NoErrors()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                delegate Int32 Calculator(Int32 a, Int32 b);
            }
            """);

        var errors = compilation.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Delegate_VoidReturn_StillWorks()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                delegate void Callback(String msg);
            }
            """);

        var errors = compilation.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Delegate_NonVoidReturnType_IsBound()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                delegate Boolean Predicate(String item);
            }
            """);

        var errors = compilation.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.Empty(errors);

        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var d = ns.Members.OfType<DelegateSymbol>().Single();
        Assert.Equal("Predicate", d.Name);
        Assert.Equal("Boolean", d.ReturnType.Name);
    }
}
