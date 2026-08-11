// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection.Metadata;
using Midl.Analysis;
using Midl.CodeGen.WinMd;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Classic MIDL2 constructs are parsed in compat mode so that one classic declaration costs
/// one diagnostic instead of derailing the rest of the file. This fixture covers the other
/// half of that promise: the declarations that follow them must still reach the WinMD.
///
/// Real RPC headers mix classic and modern declarations in one file, so a construct the
/// binder ignores must not leave the emitter with a half-built compilation.
/// </summary>
public class ClassicConstructEmissionTests
{
    /// <summary>Every classic construct added for real RPC IDL, ahead of a real runtimeclass.</summary>
    private const string ClassicThenModern = """
        typedef long error_status_t;

        error_status_t __stdcall setCallback(long cb);

        typedef error_status_t (__stdcall *FnType)([in] long a);

        interface IClassic
        {
            long getLength([in] long b) const;
            [callback] long onMark([in] long m);
        }

        namespace Test
        {
            runtimeclass Widget
            {
                Int32 Value { get; };
            }
        }
        """;

    private static byte[] CompileAndEmit(string source)
    {
        var compilation = CompilationHelper.Compile(source, referencePaths: null, mode: CompilationMode.Compat);
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Severity == Midl.Analysis.Diagnostics.DiagnosticSeverity.Error);
        return new WinMdEmitter().EmitToBytes(compilation);
    }

    private static List<string> TypeNames(byte[] winmd)
    {
        using var stream = new MemoryStream(winmd);
        using var peReader = new System.Reflection.PortableExecutable.PEReader(stream);
        var reader = peReader.GetMetadataReader();
        return reader.TypeDefinitions
            .Select(h => reader.GetString(reader.GetTypeDefinition(h).Name))
            .ToList();
    }

    [Fact]
    public void ClassicConstructsDoNotStopTheRuntimeclassFromBeingEmitted()
    {
        Assert.Contains("Widget", TypeNames(CompileAndEmit(ClassicThenModern)));
    }

    [Fact]
    public void ClassicConstructsEmitNoTypesOfTheirOwn()
    {
        // A file-scope function and a function-pointer typedef have no WinRT equivalent,
        // so they must not leak into the metadata as types.
        var names = TypeNames(CompileAndEmit(ClassicThenModern));

        Assert.DoesNotContain("setCallback", names);
        Assert.DoesNotContain("FnType", names);
    }

    [Fact]
    public void TheRuntimeclassMemberSurvives()
    {
        using var stream = new MemoryStream(CompileAndEmit(ClassicThenModern));
        using var peReader = new System.Reflection.PortableExecutable.PEReader(stream);
        var reader = peReader.GetMetadataReader();

        var methods = reader.MethodDefinitions
            .Select(h => reader.GetString(reader.GetMethodDefinition(h).Name))
            .ToList();

        Assert.Contains("get_Value", methods);
    }
}
