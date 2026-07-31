// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Header;
using Midl.CodeGen.WinMd;
using IidEmitter = Midl.CodeGen.Iid.IidEmitter;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Cross-output IID consistency (issue #50).
///
/// The same interface must get the same IID in all three generated artifacts:
///   * the C header's <c>MIDL_INTERFACE("...")</c>, which drives <c>__uuidof</c>
///   * the <c>_i.c</c>'s <c>IID_*</c> constant
///   * the WinMD's <c>[Guid]</c> custom attribute
///
/// A mismatch means <c>__uuidof(IFoo) != IID_IFoo</c>, so QueryInterface fails at
/// runtime against a server whose registration came from the WinMD or _i.c.
/// </summary>
public class IidConsistencyTests
{
    private static Compilation Compile(string source)
    {
        var compilation = CompilationHelper.Compile(source);
        Assert.All(compilation.Diagnostics,
            d => Assert.NotEqual(DiagnosticSeverity.Error, d.Severity));
        return compilation;
    }

    /// <summary>Pulls the GUID out of the header's MIDL_INTERFACE("...") for a mangled type.</summary>
    private static Guid HeaderIid(string header, string mangledName)
    {
        // The MIDL_INTERFACE line precedes the "<mangled> : public I..." line.
        var lines = header.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (!lines[i].TrimStart().StartsWith($"{mangledName} :")) continue;
            for (int j = i - 1; j >= 0 && j >= i - 5; j--)
            {
                var m = System.Text.RegularExpressions.Regex.Match(
                    lines[j], @"MIDL_INTERFACE\(""([0-9a-fA-F-]{36})""\)");
                if (m.Success) return Guid.Parse(m.Groups[1].Value);
            }
        }
        Assert.Fail($"No MIDL_INTERFACE found for {mangledName}");
        return default;
    }

    /// <summary>Pulls the GUID out of the _i.c's MIDL_DEFINE_GUID for a mangled type.</summary>
    private static Guid IidFileIid(string iidFile, string mangledName)
    {
        var m = System.Text.RegularExpressions.Regex.Match(
            iidFile,
            $@"MIDL_DEFINE_GUID\(IID,\s*IID_{System.Text.RegularExpressions.Regex.Escape(mangledName)},\s*"
            + @"0x([0-9a-fA-F]{8}),0x([0-9a-fA-F]{4}),0x([0-9a-fA-F]{4}),"
            + @"((?:0x[0-9a-fA-F]{2},?){8})\)");
        Assert.True(m.Success, $"No MIDL_DEFINE_GUID found for IID_{mangledName}");

        var bytes = m.Groups[4].Value.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(b => Convert.ToByte(b.Trim(), 16)).ToArray();
        return new Guid(
            Convert.ToUInt32(m.Groups[1].Value, 16),
            Convert.ToUInt16(m.Groups[2].Value, 16),
            Convert.ToUInt16(m.Groups[3].Value, 16),
            bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5], bytes[6], bytes[7]);
    }

    /// <summary>Reads the [Guid] custom attribute off a named TypeDef in the WinMD.</summary>
    private static Guid WinMdIid(byte[] winmd, string typeName)
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
                if (reader.GetString(reader.GetTypeReference((TypeReferenceHandle)ctor.Parent).Name)
                    != "GuidAttribute") continue;

                var blob = reader.GetBlobBytes(attr.Value);
                return new Guid(
                    BitConverter.ToUInt32(blob, 2),
                    BitConverter.ToUInt16(blob, 6),
                    BitConverter.ToUInt16(blob, 8),
                    blob[10], blob[11], blob[12], blob[13],
                    blob[14], blob[15], blob[16], blob[17]);
            }
            Assert.Fail($"No [Guid] on {typeName}");
        }
        Assert.Fail($"TypeDef {typeName} not found");
        return default;
    }

    private static (Guid header, Guid iidFile, Guid winmd) EmitAll(string source, string typeName)
    {
        var compilation = Compile(source);
        var mangled = HeaderEmitter.MangleName(
            compilation.GetAllTypes().First(t => t.Name == typeName).FullName);

        return (
            HeaderIid(new HeaderEmitter().Emit(compilation), mangled),
            IidFileIid(new IidEmitter().Emit(compilation), mangled),
            WinMdIid(new WinMdEmitter().EmitToBytes(compilation), typeName));
    }

    [Fact]
    public void DerivedIid_IsConsistentAcrossHeaderIidFileAndWinMd()
    {
        var (header, iidFile, winmd) = EmitAll("""
            namespace TestApp
            {
                interface IWidget
                {
                    Int32 Bar(Int32 a);
                }
            }
            """, "IWidget");

        Assert.Equal(winmd, iidFile);
        Assert.Equal(winmd, header);
    }

    [Fact]
    public void ExplicitIid_IsConsistentAcrossHeaderIidFileAndWinMd()
    {
        var (header, iidFile, winmd) = EmitAll("""
            namespace TestApp
            {
                [uuid(11111111-1111-1111-1111-111111111111)]
                interface IWidget
                {
                    Int32 Bar(Int32 a);
                }
            }
            """, "IWidget");

        var expected = Guid.Parse("11111111-1111-1111-1111-111111111111");
        Assert.Equal(expected, winmd);
        Assert.Equal(expected, iidFile);
        Assert.Equal(expected, header);
    }

    [Fact]
    public void DelegateIid_IsConsistentAcrossIidFileAndWinMd()
    {
        var compilation = Compile("""
            namespace TestApp
            {
                delegate void MyHandler(Int32 value);
            }
            """);
        var mangled = HeaderEmitter.MangleName("TestApp.MyHandler");

        Assert.Equal(
            WinMdIid(new WinMdEmitter().EmitToBytes(compilation), "MyHandler"),
            IidFileIid(new IidEmitter().Emit(compilation), mangled));
    }

    [Fact]
    public void HeaderIid_MatchesAbiGuidComputer()
    {
        // The header must use the WinRT ABI-signature algorithm (what midl.exe uses),
        // not a name-only hash — so that an ABI change produces a new IID.
        var compilation = Compile("""
            namespace TestApp
            {
                interface IWidget
                {
                    Int32 Bar(Int32 a);
                }
            }
            """);
        var iface = compilation.GetAllTypes().OfType<InterfaceSymbol>().Single();

        Assert.Equal(
            AbiGuidComputer.ComputeGuid(iface),
            HeaderIid(new HeaderEmitter().Emit(compilation), HeaderEmitter.MangleName(iface.FullName)));
    }

    [Fact]
    public void HeaderIid_ChangesWhenAbiChanges()
    {
        // A name-only hash would return the same IID for both of these.
        Guid IidFor(string members) => HeaderIid(
            new HeaderEmitter().Emit(Compile($$"""
                namespace TestApp
                {
                    interface IWidget
                    {
                        {{members}}
                    }
                }
                """)),
            HeaderEmitter.MangleName("TestApp.IWidget"));

        Assert.NotEqual(IidFor("Int32 Bar(Int32 a);"), IidFor("String Bar(Double a, Boolean b);"));
    }
}
