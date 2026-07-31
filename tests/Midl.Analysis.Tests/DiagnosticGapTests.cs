// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.Analysis.Diagnostics;
namespace Midl.Analysis.Tests;
public class DiagnosticGapTests
{
    // ── MIDL1078: Circular requires ──────────────────────────────────────

    [Fact]
    public void MIDL1078_CircularRequires_DirectCycle()
    {
        var c = CompilationHelper.Compile("""
            namespace N
            {
                interface IA requires IB {}
                interface IB requires IA {}
            }
            """);
        Assert.Contains(c.Diagnostics, d => d.Code == "MIDL1078");
    }

    [Fact]
    public void MIDL1078_CircularRequires_IndirectCycle()
    {
        var c = CompilationHelper.Compile("""
            namespace N
            {
                interface IA requires IB {}
                interface IB requires IC {}
                interface IC requires IA {}
            }
            """);
        Assert.Contains(c.Diagnostics, d => d.Code == "MIDL1078");
    }

    [Fact]
    public void MIDL1078_NonCircularRequires_NoDiag()
    {
        var c = CompilationHelper.Compile("""
            namespace N
            {
                interface IA requires IB {}
                interface IB {}
            }
            """);
        Assert.DoesNotContain(c.Diagnostics, d => d.Code == "MIDL1078");
    }

    [Fact]
    public void MIDL1078_SelfRequires()
    {
        var c = CompilationHelper.Compile("""
            namespace N
            {
                interface IA requires IA {}
            }
            """);
        Assert.Contains(c.Diagnostics, d => d.Code == "MIDL1078");
    }

    // ── MIDL1081: UUID format validation ─────────────────────────────────

    [Fact]
    public void MIDL1081_ValidUuid_NoDiag()
    {
        var c = CompilationHelper.Compile("""
            namespace N
            {
                [uuid("12345678-1234-1234-1234-123456789abc")]
                interface IFoo {}
            }
            """);
        Assert.DoesNotContain(c.Diagnostics, d => d.Code == "MIDL1081");
    }

    [Fact]
    public void MIDL1081_InvalidUuid_BadFormat()
    {
        var c = CompilationHelper.Compile("""
            namespace N
            {
                [uuid("not-a-uuid")]
                interface IFoo {}
            }
            """);
        Assert.Contains(c.Diagnostics, d => d.Code == "MIDL1081");
    }

    [Fact]
    public void MIDL1081_InvalidUuid_WrongLength()
    {
        var c = CompilationHelper.Compile("""
            namespace N
            {
                [uuid("1234")]
                interface IFoo {}
            }
            """);
        Assert.Contains(c.Diagnostics, d => d.Code == "MIDL1081");
    }

    [Fact]
    public void MIDL1081_InvalidUuid_MissingDashes()
    {
        var c = CompilationHelper.Compile("""
            namespace N
            {
                [uuid("12345678123412341234123456789abc")]
                interface IFoo {}
            }
            """);
        // Guid.TryParse accepts GUIDs without dashes, so this should be valid
        Assert.DoesNotContain(c.Diagnostics, d => d.Code == "MIDL1081");
    }

    // ── MIDL1083: Void property type ─────────────────────────────────────

    [Fact]
    public void MIDL1083_VoidProperty_Error()
    {
        var c = CompilationHelper.Compile("""
            namespace N
            {
                interface IFoo { void X { get; }; }
            }
            """);
        Assert.Contains(c.Diagnostics, d => d.Code == "MIDL1083");
    }

    [Fact]
    public void MIDL1083_NonVoidProperty_NoDiag()
    {
        var c = CompilationHelper.Compile("""
            namespace N
            {
                interface IFoo { Int32 X { get; }; }
            }
            """);
        Assert.DoesNotContain(c.Diagnostics, d => d.Code == "MIDL1083");
    }

    // ── MIDL1084: Static class instance members ──────────────────────────

    [Fact]
    public void MIDL1084_StaticClass_InstanceMethod_Error()
    {
        var c = CompilationHelper.Compile("""
            namespace N
            {
                static runtimeclass C
                {
                    void DoWork();
                }
            }
            """);
        Assert.Contains(c.Diagnostics, d => d.Code == "MIDL1084");
    }

    [Fact]
    public void MIDL1084_StaticClass_InstanceProperty_Error()
    {
        var c = CompilationHelper.Compile("""
            namespace N
            {
                static runtimeclass C
                {
                    Int32 Value { get; };
                }
            }
            """);
        Assert.Contains(c.Diagnostics, d => d.Code == "MIDL1084");
    }

    [Fact]
    public void MIDL1084_StaticClass_StaticMembers_NoDiag()
    {
        var c = CompilationHelper.Compile("""
            namespace N
            {
                static runtimeclass C
                {
                    static void DoWork();
                    static Int32 Value { get; };
                }
            }
            """);
        Assert.DoesNotContain(c.Diagnostics, d => d.Code == "MIDL1084");
    }

    [Fact]
    public void MIDL1084_StaticClass_InstanceEvent_Error()
    {
        var c = CompilationHelper.Compile("""
            namespace N
            {
                delegate void MyHandler();
                static runtimeclass C
                {
                    event MyHandler Changed;
                }
            }
            """);
        Assert.Contains(c.Diagnostics, d => d.Code == "MIDL1084");
    }

    [Fact]
    public void MIDL1084_NonStaticClass_InstanceMembers_NoDiag()
    {
        var c = CompilationHelper.Compile("""
            namespace N
            {
                runtimeclass C
                {
                    void DoWork();
                    Int32 Value { get; };
                }
            }
            """);
        Assert.DoesNotContain(c.Diagnostics, d => d.Code == "MIDL1084");
    }
}
