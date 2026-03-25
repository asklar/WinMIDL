using Midl.Analysis;
using Midl.CodeGen.Header;

namespace Midl.Integration.Tests;

/// <summary>
/// End-to-end integration tests for standard header boilerplate.
/// Verifies the full boilerplate is present when compiling real IDL fixtures.
/// </summary>
public partial class HeaderBoilerplateIntegrationTests
{
    private static readonly string FixturesDir = Path.GetFullPath(
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "fixtures", "valid"));

    private readonly HeaderEmitter _emitter = new();

    private string CompileFixtureToHeader(string fixtureName)
    {
        var idlPath = Path.Combine(FixturesDir, $"{fixtureName}.idl");
        Assert.True(File.Exists(idlPath), $"IDL fixture not found: {idlPath}");

        var source = File.ReadAllText(idlPath);
        var compilation = CompilationHelper.Compile(source);

        var errors = compilation.Diagnostics
            .Where(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(errors);

        return _emitter.Emit(compilation);
    }

    [Fact]
    public void ParityBasicClass_HasFullBoilerplate()
    {
        var header = CompileFixtureToHeader("parity_basic_class");

        // Banner
        Assert.StartsWith("/* Header file automatically generated", header);
        Assert.Contains("File built with WinMIDL Compiler", header);

        // Warning suppression
        Assert.Contains("#pragma warning( disable: 4049 )", header);

        // RPC version checks
        Assert.Contains("#define __REQUIRED_RPCNDR_H_VERSION__ 500", header);
        Assert.Contains("#define __REQUIRED_RPCSAL_H_VERSION__ 100", header);

        // RPC includes
        Assert.Contains("#include <rpc.h>", header);
        Assert.Contains("#include <rpcndr.h>", header);

        // COM includes guard
        Assert.Contains("#ifndef COM_NO_WINDOWS_H", header);
        Assert.Contains("#include <windows.h>", header);
        Assert.Contains("#include <ole2.h>", header);

        // Double include guard
        Assert.Contains("#ifndef __TestApp_Parity_h__", header);
        Assert.Contains("#define __TestApp_Parity_h__", header);
        Assert.Contains("#ifndef __TestApp_Parity_p_h__", header);
        Assert.Contains("#define __TestApp_Parity_p_h__", header);

        // #pragma once after guards
        var innerGuardPos = header.IndexOf("#define __TestApp_Parity_p_h__");
        var pragmaPos = header.IndexOf("#pragma once");
        Assert.True(pragmaPos > innerGuardPos);

        // NS prefix check
        Assert.Contains("CHECK_NS_PREFIX_STATE", header);

        // MIDL_CONST_ID
        Assert.Contains("#pragma push_macro(\"MIDL_CONST_ID\")", header);
        Assert.Contains("#define MIDL_CONST_ID const __declspec(selectany)", header);

        // winrtbase include
        Assert.Contains("#include \"winrtbase.h\"", header);

        // Footer
        Assert.Contains("#pragma pop_macro(\"MIDL_CONST_ID\")", header);
        Assert.Contains("#endif // __TestApp_Parity_p_h__", header);
        Assert.Contains("#endif // __TestApp_Parity_h__", header);
    }

    [Fact]
    public void ParityStructs_HasFullBoilerplate()
    {
        var header = CompileFixtureToHeader("parity_structs");

        Assert.Contains("#pragma warning( disable: 4049 )", header);
        Assert.Contains("#include <rpc.h>", header);
        Assert.Contains("#include \"winrtbase.h\"", header);
        Assert.Contains("#pragma pop_macro(\"MIDL_CONST_ID\")", header);
    }

    [Fact]
    public void ParityEnumsFlags_BoilerplateBeforeEnums()
    {
        var header = CompileFixtureToHeader("parity_enums_flags");

        var boilerplateEnd = header.IndexOf("#include \"winrtbase.h\"");
        var firstEnum = header.IndexOf("enum ");

        Assert.True(boilerplateEnd > 0, "winrtbase.h include not found");
        Assert.True(firstEnum > 0, "No enum found");
        Assert.True(boilerplateEnd < firstEnum, "Boilerplate must appear before enums");
    }

    [Fact]
    public void ParityInterfaces_BoilerplateBeforeForwardDecls()
    {
        var header = CompileFixtureToHeader("parity_interfaces");

        var boilerplateEnd = header.IndexOf("#include \"winrtbase.h\"");
        var forwardDecl = header.IndexOf("/* Forward Declarations */");

        Assert.True(boilerplateEnd > 0, "winrtbase.h include not found");
        Assert.True(forwardDecl > 0, "Forward Declarations not found");
        Assert.True(boilerplateEnd < forwardDecl, "Boilerplate must appear before forward declarations");
    }

    [Fact]
    public void Boilerplate_OrderIsCorrect()
    {
        var header = CompileFixtureToHeader("parity_basic_class");

        // Verify ordering of key sections
        var positions = new[]
        {
            ("banner", header.IndexOf("/* Header file automatically generated")),
            ("warning4049", header.IndexOf("#pragma warning( disable: 4049 )")),
            ("rpcndr_req", header.IndexOf("#define __REQUIRED_RPCNDR_H_VERSION__")),
            ("rpc_include", header.IndexOf("#include <rpc.h>")),
            ("rpcndr_check", header.IndexOf("#ifndef __RPCNDR_H_VERSION__")),
            ("com_guard", header.IndexOf("#ifndef COM_NO_WINDOWS_H")),
            ("outer_guard", header.IndexOf("#ifndef __TestApp_Parity_h__")),
            ("inner_guard", header.IndexOf("#ifndef __TestApp_Parity_p_h__")),
            ("pragma_once", header.IndexOf("#pragma once")),
            ("ns_prefix", header.IndexOf("CHECK_NS_PREFIX_STATE")),
            ("push_macro", header.IndexOf("#pragma push_macro(\"MIDL_CONST_ID\")")),
            ("winrtbase", header.IndexOf("#include \"winrtbase.h\"")),
        };

        for (int i = 0; i < positions.Length; i++)
        {
            Assert.True(positions[i].Item2 >= 0, $"{positions[i].Item1} not found in header");
            if (i > 0)
            {
                Assert.True(positions[i].Item2 > positions[i - 1].Item2,
                    $"{positions[i].Item1} (pos {positions[i].Item2}) should come after {positions[i - 1].Item1} (pos {positions[i - 1].Item2})");
            }
        }
    }

    [Theory]
    [InlineData("parity_empty_types")]
    [InlineData("parity_delegates")]
    [InlineData("parity_deep_namespace")]
    public void VariousFixtures_AllHaveBoilerplate(string fixture)
    {
        var header = CompileFixtureToHeader(fixture);

        Assert.Contains("#pragma warning( disable: 4049 )", header);
        Assert.Contains("#include <rpc.h>", header);
        Assert.Contains("#include \"winrtbase.h\"", header);
        Assert.Contains("#pragma push_macro(\"MIDL_CONST_ID\")", header);
        Assert.Contains("#pragma pop_macro(\"MIDL_CONST_ID\")", header);
    }
}
