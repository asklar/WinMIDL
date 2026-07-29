// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.Analysis.Diagnostics;

namespace Midl.Integration.Tests;

/// <summary>
/// End-to-end quick fix tests: original IDL → diagnostic with replacement → apply fix → verify result.
/// </summary>
public class QuickFixEndToEndTests
{
    /// <summary>
    /// Applies all available quick fixes to the source and returns the fixed source.
    /// Simulates what VS Code does when you click "fix all".
    /// </summary>
    private static string ApplyFixes(string source)
    {
        var compilation = CompilationHelper.Compile(source);
        // Collect all fixable diagnostics, sorted by position descending
        // (apply from end to start so positions don't shift)
        var fixes = compilation.Diagnostics
            .Where(d => d.Data != null && d.Data.ContainsKey("replacement"))
            .OrderByDescending(d => d.Position)
            .ToList();

        var result = source;
        foreach (var fix in fixes)
        {
            var replacement = fix.Data!["replacement"];
            if (fix.Position >= 0 && fix.Position + fix.Length <= result.Length)
            {
                result = result[..fix.Position] + replacement + result[(fix.Position + fix.Length)..];
            }
        }
        return result;
    }

    [Fact]
    public void Fix_int_to_Int32()
    {
        var source = "namespace N { runtimeclass C { int GetValue(); } }";
        var expected = "namespace N { runtimeclass C { Int32 GetValue(); } }";
        Assert.Equal(expected, ApplyFixes(source));
    }

    [Fact]
    public void Fix_boolean_to_Boolean()
    {
        var source = "namespace N { runtimeclass C { boolean IsEnabled { get; }; } }";
        var expected = "namespace N { runtimeclass C { Boolean IsEnabled { get; }; } }";
        Assert.Equal(expected, ApplyFixes(source));
    }

    [Fact]
    public void Fix_float_to_Single()
    {
        var source = "namespace N { runtimeclass C { float GetRatio(); } }";
        var expected = "namespace N { runtimeclass C { Single GetRatio(); } }";
        Assert.Equal(expected, ApplyFixes(source));
    }

    [Fact]
    public void Fix_double_to_Double()
    {
        var source = "namespace N { runtimeclass C { double GetPrecise(); } }";
        var expected = "namespace N { runtimeclass C { Double GetPrecise(); } }";
        Assert.Equal(expected, ApplyFixes(source));
    }

    [Fact]
    public void Fix_byte_to_UInt8()
    {
        var source = "namespace N { struct S { byte Value; }; }";
        var expected = "namespace N { struct S { UInt8 Value; }; }";
        Assert.Equal(expected, ApplyFixes(source));
    }

    [Fact]
    public void Fix_short_to_Int16()
    {
        var source = "namespace N { runtimeclass C { short GetSmall(); } }";
        var expected = "namespace N { runtimeclass C { Int16 GetSmall(); } }";
        Assert.Equal(expected, ApplyFixes(source));
    }

    [Fact]
    public void Fix_long_to_Int32()
    {
        var source = "namespace N { runtimeclass C { long GetBig(); } }";
        var expected = "namespace N { runtimeclass C { Int32 GetBig(); } }";
        Assert.Equal(expected, ApplyFixes(source));
    }

    [Fact]
    public void Fix_Multiple_Types_In_One_File()
    {
        var source = "namespace N { runtimeclass C { int X(); float Y(); boolean Z { get; }; } }";
        var expected = "namespace N { runtimeclass C { Int32 X(); Single Y(); Boolean Z { get; }; } }";
        Assert.Equal(expected, ApplyFixes(source));
    }

    [Fact]
    public void Fix_Preserves_Valid_Code()
    {
        var source = "namespace N { runtimeclass C { Int32 GetValue(); String Name { get; }; } }";
        // No fixes needed — source should be unchanged
        Assert.Equal(source, ApplyFixes(source));
    }

    [Fact]
    public void Fixed_File_Has_No_Strict_Warnings()
    {
        var source = "namespace N { runtimeclass C { int X(); float Y(); boolean Z { get; }; } }";
        var fixed_ = ApplyFixes(source);

        // Compile the fixed source — should have no MIDL2005 warnings
        var compilation = CompilationHelper.Compile(fixed_);
        var strictWarnings = compilation.Diagnostics
            .Where(d => d.Code == "MIDL2005")
            .ToList();
        Assert.Empty(strictWarnings);
    }
}
