using Midl.Analysis;
using Midl.CodeGen.Header;

namespace Midl.CodeGen.Tests;

public class HeaderEmitterArrayTests
{
    private readonly HeaderEmitter _emitter = new();

    private static Compilation CompileWithArrays(string source) =>
        CompilationHelper.Compile(source, referencePaths: null,
            mode: CompilationMode.Strict, includePaths: null, currentFilePath: null,
            arraysEnabled: true);

    [Fact]
    public void PassArray_ExpandsToSizeAndPointer()
    {
        var compilation = CompileWithArrays("""
            namespace TestApp
            {
                interface IWidget
                {
                    void SetItems(Int32[] items);
                }
            }
            """);

        var header = _emitter.Emit(compilation);

        Assert.Contains("UINT32 __itemsSize", header);
        Assert.Contains("INT32* items", header);
        Assert.DoesNotContain("INT32[] items", header);
    }

    [Fact]
    public void ReceiveArray_Out_ExpandsToSizePtrAndDoublePointer()
    {
        var compilation = CompileWithArrays("""
            namespace TestApp
            {
                interface IWidget
                {
                    void GetItems(out Int32[] items);
                }
            }
            """);

        var header = _emitter.Emit(compilation);

        Assert.Contains("UINT32* __itemsSize", header);
        Assert.Contains("INT32** items", header);
    }

    [Fact]
    public void FillArray_ExpandsToCapacityPointerAndActual()
    {
        var compilation = CompileWithArrays("""
            namespace TestApp
            {
                interface IWidget
                {
                    void FillItems(ref Int32[] items);
                }
            }
            """);

        var header = _emitter.Emit(compilation);

        Assert.Contains("UINT32 __itemsCapacity", header);
        Assert.Contains("INT32* items", header);
        Assert.Contains("UINT32* __itemsActual", header);
    }

    [Fact]
    public void ArrayReturnType_ExpandsToRetvalPair()
    {
        var compilation = CompileWithArrays("""
            namespace TestApp
            {
                interface IWidget
                {
                    Int32[] GetItems();
                }
            }
            """);

        var header = _emitter.Emit(compilation);

        Assert.Contains("UINT32* __returnSize", header);
        Assert.Contains("INT32** __returnItems", header);
        Assert.DoesNotContain("INT32[]* returnValue", header);
    }

    [Fact]
    public void StringArray_UsesHSTRING()
    {
        var compilation = CompileWithArrays("""
            namespace TestApp
            {
                interface IWidget
                {
                    void SetNames(String[] names);
                }
            }
            """);

        var header = _emitter.Emit(compilation);

        Assert.Contains("UINT32 __namesSize", header);
        Assert.Contains("HSTRING* names", header);
    }

    [Fact]
    public void MixedParams_ArrayAndNonArray()
    {
        var compilation = CompileWithArrays("""
            namespace TestApp
            {
                interface IWidget
                {
                    void Process(Int32 count, UInt8[] data, String label);
                }
            }
            """);

        var header = _emitter.Emit(compilation);

        // Non-array params preserved
        Assert.Contains("INT32 count", header);
        Assert.Contains("HSTRING label", header);
        // Array param expanded
        Assert.Contains("UINT32 __dataSize", header);
        Assert.Contains("BYTE* data", header);
    }
}
