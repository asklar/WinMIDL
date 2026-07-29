// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for async return type and runtimeclass parameter marshaling.
/// Verifies that runtimeclass-typed parameters marshal via the default interface IID,
/// and that async types (IAsyncOperation, IAsyncAction, etc.) marshal as interfaces.
/// </summary>
public class AsyncAndRuntimeClassTests
{
    private readonly ProxyGenerator _proxyGen = new();
    private readonly StubGenerator _stubGen = new();

    private static Compilation Compile(string source) => CompilationHelper.Compile(source);

    // ═══════════════════════════════════════════════════════════════
    // RuntimeClass-typed parameter marshaling
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void ProxyGenerator_RuntimeClassParam_MarshaledViaDefaultInterface()
    {
        var idl = @"
            namespace TestApp {
                interface IAnimal { String Name { get; }; };
                [default_interface]
                runtimeclass Animal : IAnimal { Animal(); }
                interface IZoo {
                    void AddAnimal(Animal animal);
                    Animal GetAnimal();
                }
            }";
        var compilation = Compile(idl);
        var output = _proxyGen.Generate(compilation);

        // Should marshal via IAnimal's IID, not some "Animal" IID
        Assert.Contains("marshal_interface", output);
        Assert.Contains("IID___x_TestApp_IAnimal", output);
        Assert.DoesNotContain("IID___x_TestApp_Animal", output);
        Assert.DoesNotContain("// TODO", output);
    }

    [Fact]
    public void StubGenerator_RuntimeClassParam_MarshaledViaDefaultInterface()
    {
        var idl = @"
            namespace TestApp {
                interface IAnimal { String Name { get; }; };
                [default_interface]
                runtimeclass Animal : IAnimal { Animal(); }
                interface IZoo {
                    void AddAnimal(Animal animal);
                    Animal GetAnimal();
                }
            }";
        var compilation = Compile(idl);
        var output = _stubGen.Generate(compilation);

        Assert.Contains("unmarshal_interface", output);
        Assert.Contains("IID___x_TestApp_IAnimal", output);
        Assert.DoesNotContain("IID___x_TestApp_Animal", output);
    }

    [Fact]
    public void ProxyGenerator_RuntimeClassReturn_MarshaledViaDefaultInterface()
    {
        var idl = @"
            namespace TestApp {
                interface IAnimal { String Name { get; }; };
                [default_interface]
                runtimeclass Animal : IAnimal { Animal(); }
                interface IZoo {
                    Animal GetAnimal();
                }
            }";
        var compilation = Compile(idl);
        var output = _proxyGen.Generate(compilation);

        // Proxy reads reply via unmarshal_interface using default iface IID
        Assert.Contains("unmarshal_interface", output);
        Assert.Contains("IID___x_TestApp_IAnimal", output);
        Assert.DoesNotContain("IID___x_TestApp_Animal", output);
    }

    // ═══════════════════════════════════════════════════════════════
    // WireFormat unit tests for RuntimeClassSymbol
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void WireFormat_RuntimeClassParam_IsInterfaceType()
    {
        var ns = new NamespaceSymbol("TestApp", null);
        var rcSym = new RuntimeClassSymbol("Animal", ns);
        var typeRef = new TypeRef("Animal") { ResolvedSymbol = rcSym };

        Assert.True(WireFormat.IsInterfaceType(typeRef));
    }

    [Fact]
    public void WireFormat_RuntimeClassParam_IsNotFixedSize()
    {
        var ns = new NamespaceSymbol("TestApp", null);
        var rcSym = new RuntimeClassSymbol("Animal", ns);
        var typeRef = new TypeRef("Animal") { ResolvedSymbol = rcSym };

        Assert.False(WireFormat.IsFixedSize(typeRef));
    }

    [Fact]
    public void WireFormat_RuntimeClassParam_IidResolvesToDefaultInterface()
    {
        var ns = new NamespaceSymbol("TestApp", null);
        var defaultIface = new InterfaceSymbol("IAnimal", ns);
        var rcSym = new RuntimeClassSymbol("Animal", ns) { DefaultInterface = defaultIface };
        var typeRef = new TypeRef("Animal") { ResolvedSymbol = rcSym };

        var iidName = WireFormat.GetInterfaceIidName(typeRef);

        Assert.Equal("IID___x_TestApp_IAnimal", iidName);
    }

    [Fact]
    public void WireFormat_RuntimeClassParam_IidDoesNotUseClassName()
    {
        var ns = new NamespaceSymbol("TestApp", null);
        var defaultIface = new InterfaceSymbol("IAnimal", ns);
        var rcSym = new RuntimeClassSymbol("Animal", ns) { DefaultInterface = defaultIface };
        var typeRef = new TypeRef("Animal") { ResolvedSymbol = rcSym };

        var iidName = WireFormat.GetInterfaceIidName(typeRef);

        Assert.DoesNotContain("Animal", iidName.Replace("IAnimal", ""));
    }

    // ═══════════════════════════════════════════════════════════════
    // Async return type marshaling
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void ProxyGenerator_AsyncOperation_MarshaledAsInterface()
    {
        var idl = @"
            namespace TestApp {
                interface IWidget {
                    Windows.Foundation.IAsyncOperation<String> LoadAsync();
                    Windows.Foundation.IAsyncAction SaveAsync();
                }
            }";
        var compilation = Compile(idl);
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("marshal_interface", output);
        Assert.DoesNotContain("// TODO", output);
    }

    [Fact]
    public void StubGenerator_AsyncOperation_MarshaledAsInterface()
    {
        var idl = @"
            namespace TestApp {
                interface IWidget {
                    Windows.Foundation.IAsyncOperation<String> LoadAsync();
                    Windows.Foundation.IAsyncAction SaveAsync();
                }
            }";
        var compilation = Compile(idl);
        var output = _stubGen.Generate(compilation);

        Assert.Contains("marshal_interface", output);
        Assert.DoesNotContain("// TODO", output);
    }

    [Fact]
    public void ProxyGenerator_AsyncOperationWithProgress_MarshaledAsInterface()
    {
        var idl = @"
            namespace TestApp {
                interface IWidget {
                    Windows.Foundation.IAsyncOperationWithProgress<String, UInt32> DownloadAsync();
                }
            }";
        var compilation = Compile(idl);
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("marshal_interface", output);
        Assert.DoesNotContain("// TODO", output);
    }

    [Fact]
    public void ProxyGenerator_AsyncActionWithProgress_MarshaledAsInterface()
    {
        var idl = @"
            namespace TestApp {
                interface IWidget {
                    Windows.Foundation.IAsyncActionWithProgress<UInt32> UploadAsync();
                }
            }";
        var compilation = Compile(idl);
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("marshal_interface", output);
        Assert.DoesNotContain("// TODO", output);
    }

    // ═══════════════════════════════════════════════════════════════
    // WireFormat PIID tests for async types
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void WireFormat_AsyncOperation_HasComputedPIID()
    {
        // IAsyncOperation<String> should produce a computed PIID via the PIID algorithm
        var idl = @"
            namespace TestApp {
                interface IWidget {
                    Windows.Foundation.IAsyncOperation<String> LoadAsync();
                }
            }";
        var compilation = Compile(idl);
        var genericTypes = WireFormat.CollectGenericInterfaceTypes(compilation);

        // Should find IAsyncOperation<String> as a generic interface type
        Assert.Contains(genericTypes, t =>
            t.ResolvedSymbol?.Name == "IAsyncOperation" && t.TypeArguments.Count == 1);

        // Should compute a non-null IID
        var asyncOpType = genericTypes.First(t => t.ResolvedSymbol?.Name == "IAsyncOperation");
        var iid = WireFormat.ComputeGenericIid(asyncOpType);
        Assert.NotNull(iid);
    }

    [Fact]
    public void WireFormat_AsyncOperationWithProgress_HasComputedPIID()
    {
        var idl = @"
            namespace TestApp {
                interface IWidget {
                    Windows.Foundation.IAsyncOperationWithProgress<String, UInt32> DownloadAsync();
                }
            }";
        var compilation = Compile(idl);
        var genericTypes = WireFormat.CollectGenericInterfaceTypes(compilation);

        Assert.Contains(genericTypes, t =>
            t.ResolvedSymbol?.Name == "IAsyncOperationWithProgress" && t.TypeArguments.Count == 2);

        var asyncOpType = genericTypes.First(t => t.ResolvedSymbol?.Name == "IAsyncOperationWithProgress");
        var iid = WireFormat.ComputeGenericIid(asyncOpType);
        Assert.NotNull(iid);
    }

    [Fact]
    public void WireFormat_AsyncActionWithProgress_HasComputedPIID()
    {
        var idl = @"
            namespace TestApp {
                interface IWidget {
                    Windows.Foundation.IAsyncActionWithProgress<UInt32> UploadAsync();
                }
            }";
        var compilation = Compile(idl);
        var genericTypes = WireFormat.CollectGenericInterfaceTypes(compilation);

        Assert.Contains(genericTypes, t =>
            t.ResolvedSymbol?.Name == "IAsyncActionWithProgress" && t.TypeArguments.Count == 1);

        var asyncOpType = genericTypes.First(t => t.ResolvedSymbol?.Name == "IAsyncActionWithProgress");
        var iid = WireFormat.ComputeGenericIid(asyncOpType);
        Assert.NotNull(iid);
    }

    [Fact]
    public void WireFormat_IAsyncAction_NotParameterized_StillMarshaledAsInterface()
    {
        // IAsyncAction is not parameterized — it has a fixed IID, not a PIID.
        // It should still be recognized as an interface type for marshaling.
        var idl = @"
            namespace TestApp {
                interface IWidget {
                    Windows.Foundation.IAsyncAction SaveAsync();
                }
            }";
        var compilation = Compile(idl);
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("marshal_interface", output);
        Assert.DoesNotContain("// TODO", output);

        // IAsyncAction should NOT appear in the generic types list (not parameterized)
        var genericTypes = WireFormat.CollectGenericInterfaceTypes(compilation);
        Assert.DoesNotContain(genericTypes, t => t.ResolvedSymbol?.Name == "IAsyncAction");
    }

    // ═══════════════════════════════════════════════════════════════
    // Combined scenario: runtimeclass + async
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void ProxyGenerator_RuntimeClassAndAsync_Combined()
    {
        var idl = @"
            namespace TestApp {
                interface IAnimal { String Name { get; }; };
                [default_interface]
                runtimeclass Animal : IAnimal { Animal(); }
                interface IZoo {
                    void AddAnimal(Animal animal);
                    Windows.Foundation.IAsyncOperation<String> SearchAsync();
                }
            }";
        var compilation = Compile(idl);
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("marshal_interface", output);
        Assert.Contains("IID___x_TestApp_IAnimal", output);
        Assert.DoesNotContain("IID___x_TestApp_Animal", output);
        Assert.DoesNotContain("// TODO", output);
    }
}
