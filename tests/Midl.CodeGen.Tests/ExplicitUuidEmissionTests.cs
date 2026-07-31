// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.CodeGen.WinMd;

namespace Midl.CodeGen.Tests;

/// <summary>
/// WinMD emission coverage for issue #48 — an author-specified <c>[uuid(...)]</c>
/// must reach the emitted <c>[Guid]</c> custom attribute, in both the unquoted
/// (canonical MIDL) and quoted spellings.
///
/// Ground truth from midl.exe 10.0.26100.0 for
/// <c>[uuid(11111111-1111-1111-1111-111111111111)] interface IFoo { Int32 Bar(Int32 a); }</c>
/// is <c>[Guid(11111111-1111-1111-1111-111111111111)]</c>.
/// </summary>
public class ExplicitUuidEmissionTests
{
    private static byte[] CompileAndEmit(string source)
    {
        var compilation = CompilationHelper.Compile(source);
        Assert.All(compilation.Diagnostics,
            d => Assert.NotEqual(DiagnosticSeverity.Error, d.Severity));
        return new WinMdEmitter().EmitToBytes(compilation);
    }

    /// <summary>Reads the [Guid] custom-attribute value off a named TypeDef.</summary>
    private static Guid ReadGuidAttribute(byte[] winmd, string typeName)
    {
        using var pe = new PEReader(new MemoryStream(winmd));
        var reader = pe.GetMetadataReader();

        foreach (var h in reader.TypeDefinitions)
        {
            var td = reader.GetTypeDefinition(h);
            if (reader.GetString(td.Name) != typeName) continue;

            foreach (var ah in td.GetCustomAttributes())
            {
                var attr = reader.GetCustomAttribute(ah);
                if (attr.Constructor.Kind != HandleKind.MemberReference) continue;

                var ctor = reader.GetMemberReference((MemberReferenceHandle)attr.Constructor);
                if (ctor.Parent.Kind != HandleKind.TypeReference) continue;

                var tr = reader.GetTypeReference((TypeReferenceHandle)ctor.Parent);
                if (reader.GetString(tr.Name) != "GuidAttribute") continue;

                // Guid ctor blob: prolog (2 bytes) then 4-2-2 + 8 raw bytes
                var blob = reader.GetBlobBytes(attr.Value);
                var a = BitConverter.ToUInt32(blob, 2);
                var b = BitConverter.ToUInt16(blob, 6);
                var c = BitConverter.ToUInt16(blob, 8);
                return new Guid(a, b, c,
                    blob[10], blob[11], blob[12], blob[13],
                    blob[14], blob[15], blob[16], blob[17]);
            }
            Assert.Fail($"No [Guid] attribute found on {typeName}");
        }
        Assert.Fail($"TypeDef {typeName} not found");
        return default;
    }

    [Theory]
    [InlineData("11111111-1111-1111-1111-111111111111")]
    [InlineData("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
    [InlineData("12345678-1234-5678-9abc-def012345678")]
    [InlineData("05fd4fdb-1111-2222-3333-444455556666")]
    [InlineData("ABCDEF12-3456-7890-ABCD-EF1234567890")]
    public void UnquotedUuid_ReachesEmittedGuidAttribute(string guidText)
    {
        var winmd = CompileAndEmit($$"""
            namespace TestApp
            {
                [uuid({{guidText}})]
                interface IWidget
                {
                    Int32 Bar(Int32 a);
                }
            }
            """);

        Assert.Equal(Guid.Parse(guidText), ReadGuidAttribute(winmd, "IWidget"));
    }

    [Fact]
    public void QuotedUuid_ReachesEmittedGuidAttribute()
    {
        var winmd = CompileAndEmit("""
            namespace TestApp
            {
                [uuid("99999999-9999-9999-9999-999999999999")]
                interface IWidget
                {
                    Int32 Bar(Int32 a);
                }
            }
            """);

        Assert.Equal(Guid.Parse("99999999-9999-9999-9999-999999999999"),
            ReadGuidAttribute(winmd, "IWidget"));
    }

    [Fact]
    public void UnquotedUuid_OnDelegate_ReachesEmittedGuidAttribute()
    {
        var winmd = CompileAndEmit("""
            namespace TestApp
            {
                [uuid(7f9c1a20-3b4d-4e5f-8091-a2b3c4d5e6f7)]
                delegate void MyHandler(Int32 value);
            }
            """);

        Assert.Equal(Guid.Parse("7f9c1a20-3b4d-4e5f-8091-a2b3c4d5e6f7"),
            ReadGuidAttribute(winmd, "MyHandler"));
    }

    [Fact]
    public void NoExplicitUuid_StillDerivesFromSignature()
    {
        var winmd = CompileAndEmit("""
            namespace TestApp
            {
                interface IWidget
                {
                    Int32 Bar(Int32 a);
                }
            }
            """);

        var emitted = ReadGuidAttribute(winmd, "IWidget");
        Assert.NotEqual(Guid.Empty, emitted);
        Assert.Equal(
            AbiGuidComputer.ComputeGuid("TestApp.IWidget",
                CompilationHelper.Compile("""
                    namespace TestApp
                    {
                        interface IWidget
                        {
                            Int32 Bar(Int32 a);
                        }
                    }
                    """).GlobalNamespace.ChildNamespaces["TestApp"]
                    .Members.OfType<Midl.Analysis.Symbols.InterfaceSymbol>().Single()
                    .Members.ToList()),
            emitted);
    }
}
