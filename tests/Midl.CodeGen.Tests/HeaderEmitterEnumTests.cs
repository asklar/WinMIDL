using Midl.Analysis;
using Midl.CodeGen.Header;

namespace Midl.CodeGen.Tests;

public partial class HeaderEmitterTests
{
    [Fact]
    public void EmitEnum_SequentialValues_EmitsZeroBasedIndex()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                enum Direction
                {
                    North,
                    South,
                    East,
                    West
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("Direction_North = 0,", header);
        Assert.Contains("Direction_South = 1,", header);
        Assert.Contains("Direction_East = 2,", header);
        Assert.Contains("Direction_West = 3,", header);
    }

    [Fact]
    public void EmitEnum_ExplicitValues_EmitsAssignedValues()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                enum Severity
                {
                    None = 0,
                    Low = 10,
                    Medium = 100,
                    High = 1000,
                    Max = 10000
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("Severity_None = 0,", header);
        Assert.Contains("Severity_Low = 10,", header);
        Assert.Contains("Severity_Medium = 100,", header);
        Assert.Contains("Severity_High = 1000,", header);
        Assert.Contains("Severity_Max = 10000,", header);
    }

    [Fact]
    public void EmitEnum_HexFlagValues_EmitsDecimal()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                [flags]
                enum Permissions
                {
                    None    = 0x0,
                    Read    = 0x1,
                    Write   = 0x2,
                    Execute = 0x4,
                    Delete  = 0x8,
                    Admin   = 0x10,
                    All     = 0x1F
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("Permissions_None = 0,", header);
        Assert.Contains("Permissions_Read = 1,", header);
        Assert.Contains("Permissions_Write = 2,", header);
        Assert.Contains("Permissions_Execute = 4,", header);
        Assert.Contains("Permissions_Delete = 8,", header);
        Assert.Contains("Permissions_Admin = 16,", header);
        Assert.Contains("Permissions_All = 31,", header);
    }

    [Fact]
    public void EmitEnum_TrailingCommaOnEveryMember()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                enum Pair
                {
                    First = 100,
                    Second = 200
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("Pair_First = 100,", header);
        Assert.Contains("Pair_Second = 200,", header);
    }

    [Fact]
    public void EmitEnum_MixedExplicitAndImplicit_UsesCorrectValues()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                enum Planet
                {
                    Mercury = 1,
                    Venus = 2,
                    Earth = 3,
                    Mars = 4
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("Planet_Mercury = 1,", header);
        Assert.Contains("Planet_Venus = 2,", header);
        Assert.Contains("Planet_Earth = 3,", header);
        Assert.Contains("Planet_Mars = 4,", header);
    }
}
