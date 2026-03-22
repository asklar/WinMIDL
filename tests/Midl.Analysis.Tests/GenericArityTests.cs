using Midl.Analysis;
using Midl.Analysis.Diagnostics;

namespace Midl.Analysis.Tests;

public class GenericArityTests
{
    // 1. IAsyncOperation without args as return type → should error
    [Fact]
    public void MIDL1101_IAsyncOperationNoArgs_ReturnType_Errors()
    {
        var c = CompilationHelper.Compile("namespace N { interface IFoo { IAsyncOperation GetResult(); } }");
        Assert.Contains(c.Diagnostics, d => d.Code == "MIDL1101");
    }

    // 2. IVector without args as property type → should error
    [Fact]
    public void MIDL1101_IVectorNoArgs_PropertyType_Errors()
    {
        var c = CompilationHelper.Compile("namespace N { runtimeclass C { IVector Items { get; }; } }");
        Assert.Contains(c.Diagnostics, d => d.Code == "MIDL1101");
    }

    // 3. IMap without args as parameter type → should error
    [Fact]
    public void MIDL1101_IMapNoArgs_ParameterType_Errors()
    {
        var c = CompilationHelper.Compile("namespace N { runtimeclass C { void DoWork(IMap map); } }");
        Assert.Contains(c.Diagnostics, d => d.Code == "MIDL1101");
    }

    // 4. TypedEventHandler without args as event type → should error
    [Fact]
    public void MIDL1101_TypedEventHandlerNoArgs_EventType_Errors()
    {
        var c = CompilationHelper.Compile("namespace N { runtimeclass C { event TypedEventHandler Changed; } }");
        Assert.Contains(c.Diagnostics, d => d.Code == "MIDL1101");
    }

    // 5. IAsyncOperation<String> WITH args → should NOT error
    [Fact]
    public void MIDL1101_IAsyncOperationWithArgs_NoDiag()
    {
        var c = CompilationHelper.Compile("namespace N { interface IFoo { IAsyncOperation<String> GetResult(); } }");
        Assert.DoesNotContain(c.Diagnostics, d => d.Code == "MIDL1101");
    }

    // 6. IVector<Int32> WITH args → should NOT error
    [Fact]
    public void MIDL1101_IVectorWithArgs_NoDiag()
    {
        var c = CompilationHelper.Compile("namespace N { runtimeclass C { IVector<Int32> Items { get; }; } }");
        Assert.DoesNotContain(c.Diagnostics, d => d.Code == "MIDL1101");
    }

    // 7. IIterable without args → should error
    [Fact]
    public void MIDL1101_IIterableNoArgs_Errors()
    {
        var c = CompilationHelper.Compile("namespace N { interface IFoo { IIterable GetItems(); } }");
        Assert.Contains(c.Diagnostics, d => d.Code == "MIDL1101");
    }

    // 8. IKeyValuePair without args → should error
    [Fact]
    public void MIDL1101_IKeyValuePairNoArgs_Errors()
    {
        var c = CompilationHelper.Compile("namespace N { interface IFoo { IKeyValuePair GetPair(); } }");
        Assert.Contains(c.Diagnostics, d => d.Code == "MIDL1101");
    }

    // 9. IReference without args → should error
    [Fact]
    public void MIDL1101_IReferenceNoArgs_Errors()
    {
        var c = CompilationHelper.Compile("namespace N { interface IFoo { IReference GetRef(); } }");
        Assert.Contains(c.Diagnostics, d => d.Code == "MIDL1101");
    }

    // 10. EventHandler without args → should error
    [Fact]
    public void MIDL1101_EventHandlerNoArgs_Errors()
    {
        var c = CompilationHelper.Compile("namespace N { runtimeclass C { event EventHandler OnEvent; } }");
        Assert.Contains(c.Diagnostics, d => d.Code == "MIDL1101");
    }
}
