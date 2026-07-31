// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis.Tests;

public class ExperimentalAttributeTests
{
    // ─── Test 1: [experimental] runtimeclass parses with no error ───

    [Fact]
    public void ExperimentalRuntimeClass_NoError()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [experimental]
                runtimeclass Preview
                {
                    void DoStuff();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single();
        Assert.True(rc.IsExperimental);
    }

    // ─── Test 2: Non-experimental class with experimental return type → MIDL1104 ───

    [Fact]
    public void NonExperimentalClass_ExperimentalReturnType_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [experimental]
                runtimeclass Preview { }

                runtimeclass Stable
                {
                    Preview GetPreview();
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1104");
        Assert.Contains("Stable", diag.Message);
        Assert.Contains("Preview", diag.Message);
    }

    // ─── Test 3: Non-experimental class with experimental parameter type → MIDL1104 ───

    [Fact]
    public void NonExperimentalClass_ExperimentalParameterType_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [experimental]
                runtimeclass Preview { }

                runtimeclass Stable
                {
                    void Process(Preview p);
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1104");
        Assert.Contains("Stable", diag.Message);
        Assert.Contains("Preview", diag.Message);
    }

    // ─── Test 4: Non-experimental class implementing experimental interface → MIDL1104 ───

    [Fact]
    public void NonExperimentalClass_ImplementsExperimentalInterface_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [experimental]
                interface IPreview
                {
                    void DoStuff();
                }

                runtimeclass Stable : IPreview { }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1104");
        Assert.Contains("Stable", diag.Message);
        Assert.Contains("IPreview", diag.Message);
    }

    // ─── Test 5: Non-experimental class deriving from experimental base → MIDL1104 ───

    [Fact]
    public void NonExperimentalClass_ExperimentalBaseClass_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [experimental]
                unsealed runtimeclass PreviewBase { }

                runtimeclass Stable : PreviewBase { }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1104");
        Assert.Contains("Stable", diag.Message);
        Assert.Contains("PreviewBase", diag.Message);
    }

    // ─── Test 6: Experimental class referencing experimental type → no error ───

    [Fact]
    public void ExperimentalClass_ReferencesExperimentalType_NoError()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [experimental, default_interface]
                runtimeclass Preview { }

                [experimental, default_interface]
                runtimeclass AlsoPreview
                {
                    Preview GetPreview();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
    }

    // ─── Test 7: Experimental class referencing non-experimental type → no error ───

    [Fact]
    public void ExperimentalClass_ReferencesStableType_NoError()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [default_interface]
                runtimeclass Stable { }

                [experimental, default_interface]
                runtimeclass Preview
                {
                    Stable GetStable();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
    }

    // ─── Test 8: [experimental] on interface → tracked correctly ───

    [Fact]
    public void ExperimentalInterface_IsTracked()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [experimental]
                interface IPreview
                {
                    void DoStuff();
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var iface = ns.Members.OfType<InterfaceSymbol>().Single();
        Assert.True(iface.IsExperimental);
    }

    // ─── Test 9: [experimental] on enum → tracked correctly ───

    [Fact]
    public void ExperimentalEnum_IsTracked()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [experimental]
                enum PreviewOptions
                {
                    None,
                    Option1
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var en = ns.Members.OfType<EnumSymbol>().Single();
        Assert.True(en.IsExperimental);
    }

    // ─── Test 10: [experimental] on struct → tracked correctly ───

    [Fact]
    public void ExperimentalStruct_IsTracked()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [experimental]
                struct PreviewData
                {
                    Int32 Value;
                }
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var s = ns.Members.OfType<StructSymbol>().Single();
        Assert.True(s.IsExperimental);
    }

    // ─── Test 11: [experimental] on delegate → tracked correctly ───

    [Fact]
    public void ExperimentalDelegate_IsTracked()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [experimental]
                delegate void PreviewHandler();
            }
            """);

        Assert.Empty(compilation.Diagnostics);
        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var d = ns.Members.OfType<DelegateSymbol>().Single();
        Assert.True(d.IsExperimental);
    }

    // ─── Test 12: Non-experimental class with experimental property type → MIDL1104 ───

    [Fact]
    public void NonExperimentalClass_ExperimentalPropertyType_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [experimental]
                enum PreviewOptions
                {
                    None
                }

                runtimeclass Stable
                {
                    PreviewOptions Options { get; };
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1104");
        Assert.Contains("Stable", diag.Message);
        Assert.Contains("PreviewOptions", diag.Message);
    }

    // ─── Test 13: Non-experimental class with experimental event delegate → MIDL1104 ───

    [Fact]
    public void NonExperimentalClass_ExperimentalEventDelegate_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [experimental]
                delegate void PreviewHandler();

                runtimeclass Stable
                {
                    event PreviewHandler Changed;
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1104");
        Assert.Contains("Stable", diag.Message);
        Assert.Contains("PreviewHandler", diag.Message);
    }

    // ─── Test 14: Non-experimental interface with experimental requires → MIDL1104 ───

    [Fact]
    public void NonExperimentalInterface_ExperimentalRequires_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [experimental]
                interface IPreview
                {
                    void DoStuff();
                }

                interface IStable requires IPreview
                {
                    void DoOther();
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1104");
        Assert.Contains("IStable", diag.Message);
        Assert.Contains("IPreview", diag.Message);
    }

    // ─── Test 15: Multiple errors from one type ───

    [Fact]
    public void NonExperimentalClass_MultipleExperimentalRefs_MultipleErrors()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [experimental]
                runtimeclass Preview { }

                [experimental]
                enum PreviewEnum { None }

                runtimeclass Stable
                {
                    Preview GetPreview();
                    void Process(PreviewEnum e);
                }
            }
            """);

        var errors = compilation.Diagnostics.Where(d => d.Code == "MIDL1104").ToList();
        Assert.Equal(2, errors.Count);
    }

    // ─── Test 16: Non-experimental struct with experimental field type → MIDL1104 ───

    [Fact]
    public void NonExperimentalStruct_ExperimentalFieldType_Error()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [experimental]
                enum PreviewEnum
                {
                    None
                }

                struct StableData
                {
                    PreviewEnum Value;
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1104");
        Assert.Contains("StableData", diag.Message);
        Assert.Contains("PreviewEnum", diag.Message);
    }
}
