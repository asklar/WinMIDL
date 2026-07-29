// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.Analysis.Symbols;

namespace Midl.Analysis.Tests;

public class EnumExpressionEvalTests
{
    [Fact]
    public void EnumExprBasicValues()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                enum E
                {
                    A = 1,
                    B = 2,
                }
            }
            """);
        Assert.Empty(compilation.Diagnostics);
        var e = compilation.GetAllTypes().OfType<EnumSymbol>().First();
        Assert.Equal(1L, e.EnumMembers.First(m => m.Name == "A").ExplicitValue);
        Assert.Equal(2L, e.EnumMembers.First(m => m.Name == "B").ExplicitValue);
    }

    [Fact]
    public void EnumExprArithmetic()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                enum E
                {
                    A = 1 + 2,
                }
            }
            """);
        Assert.Empty(compilation.Diagnostics);
        var e = compilation.GetAllTypes().OfType<EnumSymbol>().First();
        Assert.Equal(3L, e.EnumMembers.First(m => m.Name == "A").ExplicitValue);
    }

    [Fact]
    public void EnumExprBitwiseOr()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                enum E
                {
                    A = 0x1,
                    B = 0x2,
                    C = A | B,
                }
            }
            """);
        Assert.Empty(compilation.Diagnostics);
        var e = compilation.GetAllTypes().OfType<EnumSymbol>().First();
        Assert.Equal(1L, e.EnumMembers.First(m => m.Name == "A").ExplicitValue);
        Assert.Equal(2L, e.EnumMembers.First(m => m.Name == "B").ExplicitValue);
        Assert.Equal(3L, e.EnumMembers.First(m => m.Name == "C").ExplicitValue);
    }

    [Fact]
    public void EnumExprShift()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                enum E
                {
                    A = 1 << 4,
                }
            }
            """);
        Assert.Empty(compilation.Diagnostics);
        var e = compilation.GetAllTypes().OfType<EnumSymbol>().First();
        Assert.Equal(16L, e.EnumMembers.First(m => m.Name == "A").ExplicitValue);
    }

    [Fact]
    public void EnumExprReferencePrevious()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                enum E
                {
                    A = 10,
                    B = A + 1,
                }
            }
            """);
        Assert.Empty(compilation.Diagnostics);
        var e = compilation.GetAllTypes().OfType<EnumSymbol>().First();
        Assert.Equal(10L, e.EnumMembers.First(m => m.Name == "A").ExplicitValue);
        Assert.Equal(11L, e.EnumMembers.First(m => m.Name == "B").ExplicitValue);
    }

    [Fact]
    public void EnumExprComplex()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                enum E
                {
                    A = (1 << 2) | 0x10,
                }
            }
            """);
        Assert.Empty(compilation.Diagnostics);
        var e = compilation.GetAllTypes().OfType<EnumSymbol>().First();
        Assert.Equal(20L, e.EnumMembers.First(m => m.Name == "A").ExplicitValue);
    }

    [Fact]
    public void EnumExprUnaryBitwiseNot()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                enum E
                {
                    A = ~0,
                }
            }
            """);
        Assert.Empty(compilation.Diagnostics);
        var e = compilation.GetAllTypes().OfType<EnumSymbol>().First();
        Assert.Equal(-1L, e.EnumMembers.First(m => m.Name == "A").ExplicitValue);
    }

    [Fact]
    public void EnumExprNestedParens()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                enum E
                {
                    A = ((1 + 2) * 3),
                }
            }
            """);
        Assert.Empty(compilation.Diagnostics);
        var e = compilation.GetAllTypes().OfType<EnumSymbol>().First();
        Assert.Equal(9L, e.EnumMembers.First(m => m.Name == "A").ExplicitValue);
    }

    [Fact]
    public void EnumExprFlagsPattern()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [flags]
                enum F
                {
                    None = 0,
                    A = 1,
                    B = 2,
                    C = 4,
                    All = A | B | C,
                }
            }
            """);
        Assert.Empty(compilation.Diagnostics);
        var f = compilation.GetAllTypes().OfType<EnumSymbol>().First();
        Assert.True(f.IsFlags);
        Assert.Equal(0L, f.EnumMembers.First(m => m.Name == "None").ExplicitValue);
        Assert.Equal(1L, f.EnumMembers.First(m => m.Name == "A").ExplicitValue);
        Assert.Equal(2L, f.EnumMembers.First(m => m.Name == "B").ExplicitValue);
        Assert.Equal(4L, f.EnumMembers.First(m => m.Name == "C").ExplicitValue);
        Assert.Equal(7L, f.EnumMembers.First(m => m.Name == "All").ExplicitValue);
    }
}
