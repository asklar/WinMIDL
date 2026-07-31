// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis.Symbols;

namespace Midl.Analysis.Tests;

/// <summary>
/// Regression coverage for issue #48 — <c>[uuid(...)]</c> written in the canonical
/// unquoted form was silently dropped, causing winmidl to emit a signature-derived
/// IID instead of the author-specified one.
///
/// midl.exe honors the unquoted form; ground truth for
/// <c>[uuid(11111111-1111-1111-1111-111111111111)]</c> is
/// <c>[Guid(11111111-1111-1111-1111-111111111111)]</c> in the emitted WinMD.
/// </summary>
public class UnquotedGuidTests
{
    private static InterfaceSymbol CompileInterfaceWithUuid(string uuidArg, out Compilation compilation)
    {
        compilation = CompilationHelper.Compile($$"""
            namespace TestApp
            {
                [uuid({{uuidArg}})]
                interface IWidget
                {
                    void DoWork();
                }
            }
            """);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        return ns.Members.OfType<InterfaceSymbol>().Single();
    }

    [Theory]
    // Every spelling midl.exe accepts must round-trip to the same Guid.
    [InlineData("11111111-1111-1111-1111-111111111111")]   // all digits
    [InlineData("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]   // all hex letters
    [InlineData("12345678-1234-5678-9abc-def012345678")]   // mixed
    [InlineData("abcd1234-5678-90ab-cdef-1234567890ab")]   // leading letter
    [InlineData("05fd4fdb-1111-2222-3333-444455556666")]   // leading zero
    [InlineData("ABCDEF12-3456-7890-ABCD-EF1234567890")]   // uppercase
    public void UnquotedUuid_IsHonored(string guidText)
    {
        var iface = CompileInterfaceWithUuid(guidText, out var compilation);

        Assert.Empty(compilation.Diagnostics);
        Assert.Equal(Guid.Parse(guidText), iface.Uuid);
    }

    [Fact]
    public void QuotedUuid_StillHonored()
    {
        var iface = CompileInterfaceWithUuid("\"99999999-9999-9999-9999-999999999999\"", out var compilation);

        Assert.Empty(compilation.Diagnostics);
        Assert.Equal(Guid.Parse("99999999-9999-9999-9999-999999999999"), iface.Uuid);
    }

    [Fact]
    public void UnquotedUuid_OnDelegate_IsHonored()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [uuid(7f9c1a20-3b4d-4e5f-8091-a2b3c4d5e6f7)]
                delegate void MyHandler(Int32 value);
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var del = compilation.GlobalNamespace.ChildNamespaces["TestApp"]
            .Members.OfType<DelegateSymbol>().Single();

        Assert.Equal(Guid.Parse("7f9c1a20-3b4d-4e5f-8091-a2b3c4d5e6f7"), del.Uuid);
    }

    [Fact]
    public void UnquotedUuid_OverridesSignatureDerivedIid()
    {
        // Without an explicit uuid the IID is derived from the ABI signature.
        var derived = CompilationHelper.Compile("""
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                }
            }
            """).GlobalNamespace.ChildNamespaces["TestApp"]
            .Members.OfType<InterfaceSymbol>().Single();

        Assert.Null(derived.Uuid);

        var pinned = CompileInterfaceWithUuid("11111111-1111-1111-1111-111111111111", out _);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), pinned.Uuid);
        Assert.NotEqual(AbiGuidComputer.ComputeGuid(derived), pinned.Uuid!.Value);
    }

    [Fact]
    public void UnquotedGuid_SurvivesAsGenericAttributeArgument()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr { Guid Value; };

                [MyAttr(cafebabe-1111-2222-3333-444455556666)]
                runtimeclass RC { RC(); Int32 Go(); }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Severity == Diagnostics.DiagnosticSeverity.Error);
        var rc = compilation.GlobalNamespace.ChildNamespaces["TestApp"]
            .Members.OfType<RuntimeClassSymbol>().Single();

        var attr = rc.Attributes.Single(a => a.Name == "MyAttr");
        Assert.Equal(Guid.Parse("cafebabe-1111-2222-3333-444455556666"), attr.GetGuid(0));
    }
}
