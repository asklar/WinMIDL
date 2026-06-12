// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.Analysis.Symbols;

namespace Midl.Analysis.Tests;

public class VersionEncodingTests
{
    [Theory]
    [InlineData(1, 0, 0x00010000u)]
    [InlineData(2, 5, 0x00020005u)]
    [InlineData(3, 0, 0x00030000u)]
    [InlineData(0, 0, 0u)]
    public void Encode_MajorMinor(int major, int minor, uint expected)
    {
        Assert.Equal(expected, VersionEncoding.Encode(major, minor));
    }

    [Theory]
    [InlineData(0x00010000u, 1, 0)]
    [InlineData(0x00020005u, 2, 5)]
    [InlineData(0x00030000u, 3, 0)]
    public void Decode_Roundtrip(uint encoded, int major, int minor)
    {
        var (m, n) = VersionEncoding.Decode(encoded);
        Assert.Equal(major, m);
        Assert.Equal(minor, n);
    }

    [Theory]
    [InlineData(0x00010000u, "1")]
    [InlineData(0x00020005u, "2.5")]
    [InlineData(0x00030000u, "3")]
    public void Display_FormatsCorrectly(uint encoded, string expected)
    {
        Assert.Equal(expected, VersionEncoding.Display(encoded));
    }

    [Theory]
    [InlineData("1.0", 0x00010000u)]
    [InlineData("2.5", 0x00020005u)]
    [InlineData("3", 0x00030000u)]
    [InlineData("1.0f", 0x00010000u)]
    public void ParseVersionText(string text, uint expected)
    {
        Assert.Equal(expected, VersionEncoding.ParseVersionText(text));
    }

    [Fact]
    public void FromAttributeArg_Integer_EncodesAsMajor()
    {
        var arg = new AttributeArgValue { Kind = ArgKind.Integer, IntValue = 3, StringValue = "3" };
        Assert.Equal(0x00030000u, VersionEncoding.FromAttributeArg(arg));
    }

    [Fact]
    public void FromAttributeArg_HexInteger_PassThrough()
    {
        var arg = new AttributeArgValue { Kind = ArgKind.Integer, IntValue = 0x00010000, StringValue = "0x00010000" };
        Assert.Equal(0x00010000u, VersionEncoding.FromAttributeArg(arg));
    }

    [Fact]
    public void FromAttributeArg_Float_ParsesMajorMinor()
    {
        var arg = new AttributeArgValue { Kind = ArgKind.Float, FloatValue = 2.5, StringValue = "2.5" };
        Assert.Equal(0x00020005u, VersionEncoding.FromAttributeArg(arg));
    }

    // --- Integration tests: version encoding through compilation ---

    [Fact]
    public void ContractVersion_Integer_EncodedCorrectly()
    {
        // [contractversion(1)] → encoded as (1 << 16) = 65536
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [contractversion(1)]
                apicontract TestContract { }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var contract = ns.Members.OfType<ApiContractSymbol>().Single();

        Assert.Equal(65536, contract.ContractVersion);
    }

    [Fact]
    public void ContractVersion_FloatOneDotZero_SameAsInteger()
    {
        // [contractversion(1.0)] → same as [contractversion(1)] = 65536
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [contractversion(1.0)]
                apicontract TestContract { }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var contract = ns.Members.OfType<ApiContractSymbol>().Single();

        Assert.Equal(65536, contract.ContractVersion);
    }

    [Fact]
    public void ContractVersion_TwoDotFive_EncodedCorrectly()
    {
        // [contractversion(2.5)] → (2 << 16) | 5 = 131077
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [contractversion(2.5)]
                apicontract TestContract { }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var contract = ns.Members.OfType<ApiContractSymbol>().Single();

        Assert.Equal(131077, contract.ContractVersion);
    }

    [Fact]
    public void Version_Integer_EncodedCorrectly()
    {
        // [version(3)] → (3 << 16) = 196608
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [version(3), default_interface]
                runtimeclass Widget { }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var widget = ns.Members.OfType<RuntimeClassSymbol>().Single();

        Assert.Equal(196608, widget.Version);
    }

    [Fact]
    public void Contract_VersionArg_EncodedCorrectly()
    {
        // [contract(C, 2)] → version = (2 << 16) = 131072
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [contractversion(5)]
                apicontract TestContract { }

                [contract(TestContract, 2), default_interface]
                runtimeclass Widget { }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics,
            d => d.Severity == Midl.Analysis.Diagnostics.DiagnosticSeverity.Error);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var widget = ns.Members.OfType<RuntimeClassSymbol>().Single();

        Assert.Equal(131072, widget.ContractVersion);
    }

    [Fact]
    public void Version_HexPassthrough_EncodedCorrectly()
    {
        // [version(0x00010000)] → pass-through = 65536
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [version(0x00010000), default_interface]
                runtimeclass Widget { }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var widget = ns.Members.OfType<RuntimeClassSymbol>().Single();

        Assert.Equal(65536, widget.Version);
    }

    [Fact]
    public void ContractVersion_Three_EncodedCorrectly()
    {
        // [contractversion(3)] → (3 << 16) = 196608
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                [contractversion(3)]
                apicontract TestContract { }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var contract = ns.Members.OfType<ApiContractSymbol>().Single();

        Assert.Equal(196608, contract.ContractVersion);
    }
}
