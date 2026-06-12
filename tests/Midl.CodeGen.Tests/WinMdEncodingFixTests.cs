// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.WinMd;

namespace Midl.CodeGen.Tests;

public class WinMdEncodingFixTests
{
    private static byte[] CompileAndEmit(string source, bool arraysEnabled = false)
    {
        var compilation = CompilationHelper.Compile(source, referencePaths: null, mode: CompilationMode.Strict, includePaths: null, currentFilePath: null, arraysEnabled: arraysEnabled);
        Assert.Empty(compilation.Diagnostics);
        var emitter = new WinMdEmitter();
        return emitter.EmitToBytes(compilation);
    }

    private static MetadataReader CreateReader(byte[] bytes, out PEReader peReader)
    {
        peReader = new PEReader(new MemoryStream(bytes));
        return peReader.GetMetadataReader();
    }

    private static TypeDefinitionHandle FindTypeDef(MetadataReader reader, string name)
    {
        foreach (var handle in reader.TypeDefinitions)
        {
            var td = reader.GetTypeDefinition(handle);
            if (reader.GetString(td.Name) == name)
                return handle;
        }
        throw new Exception($"TypeDef '{name}' not found");
    }

    /// <summary>
    /// Task 1: Composable (unsealed) runtimeclass .ctor should omit
    /// the IInspectable* outer and IInspectable** inner params from the WinMD signature.
    /// </summary>
    [Fact]
    public void ComposableCtorOmitsInspectableParams()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [default_interface]
                unsealed runtimeclass Widget
                {
                    Widget(String name);
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Widget");
            var td = reader.GetTypeDefinition(handle);

            // Find the .ctor method
            var methods = td.GetMethods().ToList();
            MethodDefinitionHandle ctorHandle = default;
            foreach (var mh in methods)
            {
                var md = reader.GetMethodDefinition(mh);
                if (reader.GetString(md.Name) == ".ctor")
                {
                    ctorHandle = mh;
                    break;
                }
            }
            Assert.False(ctorHandle.IsNil, "Should have a .ctor method");

            var ctor = reader.GetMethodDefinition(ctorHandle);
            var sigBlob = reader.GetBlobReader(ctor.Signature);

            // Read method signature: calling convention byte, then param count
            var callingConv = sigBlob.ReadByte();
            var paramCount = sigBlob.ReadCompressedInteger();

            // Unsealed class has composable constructor — outer/inner should be stripped.
            // Original has 1 user param (String name) + 2 composable params = 3 total in source.
            // But in WinMD, only the user param should appear.
            Assert.Equal(1, paramCount);
        }
    }

    /// <summary>
    /// Task 2: Array param in WinMD should omit the implicit UInt32 length param.
    /// </summary>
    [Fact]
    public void ArrayParamOmitsLengthInSignature()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                interface IWidget
                {
                    void ProcessItems(UInt32 itemsSize, Int32[] items);
                }
            }
            """, arraysEnabled: true);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "IWidget");
            var td = reader.GetTypeDefinition(handle);
            var methods = td.GetMethods().ToList();

            MethodDefinitionHandle methodHandle = default;
            foreach (var mh in methods)
            {
                var md = reader.GetMethodDefinition(mh);
                if (reader.GetString(md.Name) == "ProcessItems")
                {
                    methodHandle = mh;
                    break;
                }
            }
            Assert.False(methodHandle.IsNil, "Should have ProcessItems method");

            var method = reader.GetMethodDefinition(methodHandle);
            var sigBlob = reader.GetBlobReader(method.Signature);

            var callingConv = sigBlob.ReadByte();
            var paramCount = sigBlob.ReadCompressedInteger();

            // The UInt32 length param before the array should be omitted.
            // Only the array param (Int32[]) should remain.
            Assert.Equal(1, paramCount);
        }
    }

    /// <summary>
    /// Task 3: Non-void method return value should have auto-generated "result" param name.
    /// </summary>
    [Fact]
    public void RetvalParamNamedResult()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                runtimeclass Widget
                {
                    Int32 GetValue();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            // Check the synthesized interface method (IWidget.GetValue)
            var handle = FindTypeDef(reader, "IWidget");
            var td = reader.GetTypeDefinition(handle);
            var methods = td.GetMethods().ToList();

            MethodDefinitionHandle methodHandle = default;
            foreach (var mh in methods)
            {
                var md = reader.GetMethodDefinition(mh);
                if (reader.GetString(md.Name) == "GetValue")
                {
                    methodHandle = mh;
                    break;
                }
            }
            Assert.False(methodHandle.IsNil, "Should have GetValue method");

            var method = reader.GetMethodDefinition(methodHandle);
            var parameters = method.GetParameters().ToList();

            // Should have a retval param at sequence 0 named "result"
            Assert.True(parameters.Count >= 1, "Should have at least one param entry (retval)");

            var retvalParam = reader.GetParameter(parameters[0]);
            Assert.Equal(0, retvalParam.SequenceNumber);
            Assert.Equal("result", reader.GetString(retvalParam.Name));
        }
    }

    /// <summary>
    /// Task 3: Async-returning method should have retval named "operation".
    /// We use a method named with IAsyncAction return type style, but since 
    /// IAsyncAction is external, we test using a custom interface that starts
    /// with "IAsync" and verifying the auto-naming via a non-void return.
    /// Instead, we verify the naming logic through GetRetvalParamName by using
    /// a runtimeclass method that returns a type resolvable in the compilation.
    /// </summary>
    [Fact]
    public void AsyncRetvalParamNamedOperation()
    {
        // Test that the GetRetvalParamName logic correctly identifies async types.
        // Use an interface with a method returning another local interface to verify
        // retval param name is "result" for normal types.
        // Then also use a delegate-based approach to test "operation" name for IAsyncAction.
        var bytes = CompileAndEmit("""
            namespace Test
            {
                interface IAsyncAction
                {
                    void GetResults();
                }
                runtimeclass Widget
                {
                    IAsyncAction StartAsync();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "IWidget");
            var td = reader.GetTypeDefinition(handle);
            var methods = td.GetMethods().ToList();

            MethodDefinitionHandle methodHandle = default;
            foreach (var mh in methods)
            {
                var md = reader.GetMethodDefinition(mh);
                if (reader.GetString(md.Name) == "StartAsync")
                {
                    methodHandle = mh;
                    break;
                }
            }
            Assert.False(methodHandle.IsNil, "Should have StartAsync method");

            var method = reader.GetMethodDefinition(methodHandle);
            var parameters = method.GetParameters().ToList();

            Assert.True(parameters.Count >= 1, "Should have at least one param entry (retval)");

            var retvalParam = reader.GetParameter(parameters[0]);
            Assert.Equal(0, retvalParam.SequenceNumber);
            Assert.Equal("operation", reader.GetString(retvalParam.Name));
        }
    }
}
