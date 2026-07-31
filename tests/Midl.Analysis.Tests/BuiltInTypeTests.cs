// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis.Tests;

public class BuiltInTypeTests
{
#if WITH_COMPAT
    [Fact]
    public void IInspectableResolves()
    {
        // IInspectable as a base interface should not produce MIDL1001
        var source = """
            namespace Test
            {
                interface IFoo requires IInspectable
                {
                    void DoStuff();
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Compat);

        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Code == "MIDL1001" && d.Message.Contains("IInspectable"));
    }

    [Fact]
    public void HSTRINGResolves()
    {
        var source = """
            namespace Test
            {
                interface IFoo
                {
                    void SetName(HSTRING name);
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Compat);

        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Code == "MIDL1001" && d.Message.Contains("HSTRING"));
    }

    [Fact]
    public void EventRegistrationTokenResolves()
    {
        var source = """
            namespace Test
            {
                interface IFoo
                {
                    void Unregister(EventRegistrationToken token);
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Compat);

        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Code == "MIDL1001" && d.Message.Contains("EventRegistrationToken"));
    }
#endif

    [Fact]
    public void WindowsFoundationIAsyncActionResolves()
    {
        var source = """
            namespace Test
            {
                interface IFoo
                {
                    Windows.Foundation.IAsyncAction DoAsync();
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source);

        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Code == "MIDL1001" && d.Message.Contains("IAsyncAction"));
    }

    [Fact]
    public void IVectorShorthandResolves()
    {
        var source = """
            namespace Test
            {
                interface IFoo
                {
                    IVector<String> GetItems();
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source);

        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Code == "MIDL1001" && d.Message.Contains("IVector"));
    }

    [Fact]
    public void WindowsFoundationUriResolves()
    {
        var source = """
            namespace Test
            {
                interface IFoo
                {
                    Windows.Foundation.Uri GetUri();
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source);

        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Code == "MIDL1001" && d.Message.Contains("Uri"));
    }

#if WITH_COMPAT
    [Fact]
    public void UINT32AliasResolves()
    {
        var source = """
            namespace Test
            {
                interface IFoo
                {
                    void SetValue(UINT32 val);
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Compat);

        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Code == "MIDL1001" && d.Message.Contains("UINT32"));
    }

    [Fact]
    public void DWORDAliasResolves()
    {
        var source = """
            namespace Test
            {
                interface IFoo
                {
                    void SetFlags(DWORD flags);
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Compat);

        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Code == "MIDL1001" && d.Message.Contains("DWORD"));
    }

    [Fact]
    public void GUIDAliasResolves()
    {
        var source = """
            namespace Test
            {
                interface IFoo
                {
                    void SetId(GUID id);
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Compat);

        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Code == "MIDL1001" && d.Message.Contains("GUID"));
    }

    [Fact]
    public void CompatModeDoubleResolves()
    {
        var source = """
            namespace Test
            {
                interface IFoo
                {
                    void SetValue(double val);
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Compat);

        // In compat mode, double should resolve without any errors
        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Code == "MIDL1001" && d.Message.Contains("double"));
        // Should also not have strict-mode warning in compat mode
        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Code == "MIDL2005" && d.Message.Contains("double"));
    }
#endif

    [Fact]
    public void StrictModeDoubleStillWarns()
    {
        var source = """
            namespace Test
            {
                interface IFoo
                {
                    void SetValue(double val);
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Strict);

        // Should have MIDL2005 warning (lowercase type)
        Assert.Contains(compilation.Diagnostics,
            d => d.Code == "MIDL2005" && d.Message.Contains("double"));
        // But should NOT have MIDL1001 error (undefined type)
        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Code == "MIDL1001" && d.Message.Contains("double"));
    }

#if WITH_COMPAT
    [Fact]
    public void BuiltInTypesAreExternal()
    {
        // Compile something that references built-in types and verify they're external
        var source = """
            namespace Test
            {
                interface IFoo requires IInspectable
                {
                    Windows.Foundation.IAsyncAction DoAsync();
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Compat);

        // IInspectable should resolve to an external InterfaceSymbol
        var iface = compilation.GlobalNamespace.ChildNamespaces["Test"]
            .Members.OfType<InterfaceSymbol>().Single();
        var iInspectable = iface.RequiredInterfaces.FirstOrDefault();
        Assert.NotNull(iInspectable?.ResolvedSymbol);
        Assert.True(iInspectable!.ResolvedSymbol!.IsExternal);
        Assert.Equal("<built-in>", iInspectable.ResolvedSymbol.SourceWinMdPath);

        // IAsyncAction should also be external
        var method = iface.Members.OfType<MethodSymbol>().Single();
        Assert.NotNull(method.ReturnType.ResolvedSymbol);
        Assert.True(method.ReturnType.ResolvedSymbol!.IsExternal);
        Assert.Equal("<built-in>", method.ReturnType.ResolvedSymbol.SourceWinMdPath);
    }
#endif
}
