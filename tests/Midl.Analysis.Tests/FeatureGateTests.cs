// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#if FEATURE_STAGING
using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis.Tests;

public class FeatureGateTests
{
    // ─── Test 1: [feature(MyFeature)] runtimeclass → FeatureName set correctly ───

    [Fact]
    public void FeatureGatedRuntimeClass_FeatureNameSet()
    {
        var compilation = CompilationHelper.Compile("""
            feature_name MyFeature = { DisabledByDefault, false };
            namespace N
            {
                [feature(MyFeature)]
                runtimeclass Preview
                {
                    void DoStuff();
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        var ns = compilation.GlobalNamespace.ChildNamespaces["N"];
        var rc = ns.Members.OfType<RuntimeClassSymbol>().Single();
        Assert.Equal("MyFeature", rc.FeatureName);
    }

    // ─── Test 2: Non-feature type referencing feature-gated return type → MIDL1105 ───

    [Fact]
    public void NonFeatureClass_FeatureGatedReturnType_Warning()
    {
        var compilation = CompilationHelper.Compile("""
            feature_name MyFeature = { DisabledByDefault, false };
            namespace N
            {
                [feature(MyFeature)]
                runtimeclass Preview { }

                runtimeclass Stable
                {
                    Preview GetPreview();
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1105");
        Assert.Equal(DiagnosticSeverity.Warning, diag.Severity);
        Assert.Contains("Stable", diag.Message);
        Assert.Contains("Preview", diag.Message);
        Assert.Contains("MyFeature", diag.Message);
    }

    // ─── Test 3: Non-feature type referencing feature-gated param type → MIDL1105 ───

    [Fact]
    public void NonFeatureClass_FeatureGatedParamType_Warning()
    {
        var compilation = CompilationHelper.Compile("""
            feature_name MyFeature = { DisabledByDefault, false };
            namespace N
            {
                [feature(MyFeature)]
                runtimeclass Preview { }

                runtimeclass Stable
                {
                    void UsePreview(Preview p);
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1105");
        Assert.Equal(DiagnosticSeverity.Warning, diag.Severity);
        Assert.Contains("Stable", diag.Message);
        Assert.Contains("Preview", diag.Message);
    }

    // ─── Test 4: Non-feature type implementing feature-gated interface → MIDL1105 ───

    [Fact]
    public void NonFeatureClass_ImplementsFeatureGatedInterface_Warning()
    {
        var compilation = CompilationHelper.Compile("""
            feature_name MyFeature = { DisabledByDefault, false };
            namespace N
            {
                [feature(MyFeature)]
                [uuid("12345678-1234-1234-1234-123456789abc")]
                interface IPreview
                {
                    void DoStuff();
                };

                runtimeclass Stable : [default] IPreview
                {
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1105");
        Assert.Equal(DiagnosticSeverity.Warning, diag.Severity);
        Assert.Contains("Stable", diag.Message);
        Assert.Contains("IPreview", diag.Message);
    }

    // ─── Test 5: Same-feature type referencing same-feature type → no warning ───

    [Fact]
    public void SameFeature_ReferencingSameFeature_NoWarning()
    {
        var compilation = CompilationHelper.Compile("""
            feature_name MyFeature = { DisabledByDefault, false };
            namespace N
            {
                [feature(MyFeature)]
                runtimeclass Preview { }

                [feature(MyFeature)]
                runtimeclass AlsoPreview
                {
                    Preview GetPreview();
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1105");
    }

    // ─── Test 6: Feature-gated type referencing non-feature type → no warning ───

    [Fact]
    public void FeatureGated_ReferencesStableType_NoWarning()
    {
        var compilation = CompilationHelper.Compile("""
            feature_name MyFeature = { DisabledByDefault, false };
            namespace N
            {
                runtimeclass Stable { }

                [feature(MyFeature)]
                runtimeclass Preview
                {
                    Stable GetStable();
                }
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1105");
    }

    // ─── Test 7: Different feature gates → MIDL1105 warning ───

    [Fact]
    public void DifferentFeatureGates_Warning()
    {
        var compilation = CompilationHelper.Compile("""
            feature_name FeatureA = { DisabledByDefault, false };
            feature_name FeatureB = { EnabledByDefault, true };
            namespace N
            {
                [feature(FeatureB)]
                runtimeclass PreviewB { }

                [feature(FeatureA)]
                runtimeclass PreviewA
                {
                    PreviewB GetB();
                }
            }
            """);

        var diag = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL1105");
        Assert.Equal(DiagnosticSeverity.Warning, diag.Severity);
        Assert.Contains("PreviewA", diag.Message);
        Assert.Contains("PreviewB", diag.Message);
        Assert.Contains("FeatureB", diag.Message);
    }

    // ─── Test 8: WinMD round-trip: emit FeatureAttribute → read back → verify FeatureName ───

    [Fact]
    public void WinMdRoundTrip_FeatureAttribute()
    {
        var compilation = CompilationHelper.Compile("""
            feature_name MyFeature = { DisabledByDefault, false };
            namespace TestNs
            {
                [feature(MyFeature)]
                runtimeclass FeatureGated
                {
                    void DoStuff();
                }
            }
            """);

        Assert.False(compilation.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
            "IDL compilation should not have errors: " +
            string.Join("; ", compilation.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => d.Message)));

        var emitter = new Midl.CodeGen.WinMd.WinMdEmitter();
        var winmdBytes = emitter.EmitToBytes(compilation);

        var reader = new WinMdReader();
        var types = reader.ReadTypes(winmdBytes);

        var rc = types.OfType<RuntimeClassSymbol>().FirstOrDefault(t => t.Name == "FeatureGated");
        Assert.NotNull(rc);
        Assert.NotNull(rc.FeatureName);
    }
}
#endif
