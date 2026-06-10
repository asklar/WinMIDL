#if FEATURE_STAGING
using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis.Tests;

public class FeatureNameTests
{
    [Fact]
    public void FeatureName_AlwaysEnabled_True_ParsesWithoutError()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                feature_name Foo = {AlwaysEnabled, TRUE};
            }
            """, CompilationMode.Compat);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        Assert.True(compilation.FeatureNames.ContainsKey("Foo"));
        var info = compilation.FeatureNames["Foo"];
        Assert.Equal(FeatureStage.AlwaysEnabled, info.Stage);
        Assert.True(info.ValidInAllBranches);
    }

    [Fact]
    public void FeatureName_DisabledByDefault_False_ParsesCorrectly()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                feature_name Bar = {DisabledByDefault, FALSE};
            }
            """, CompilationMode.Compat);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        Assert.True(compilation.FeatureNames.ContainsKey("Bar"));
        var info = compilation.FeatureNames["Bar"];
        Assert.Equal(FeatureStage.DisabledByDefault, info.Stage);
        Assert.False(info.ValidInAllBranches);
    }

    [Fact]
    public void FeatureName_EnabledByDefault_ParsesCorrectly()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                feature_name Baz = {EnabledByDefault, TRUE};
            }
            """, CompilationMode.Compat);

        Assert.True(compilation.FeatureNames.ContainsKey("Baz"));
        var info = compilation.FeatureNames["Baz"];
        Assert.Equal(FeatureStage.EnabledByDefault, info.Stage);
        Assert.True(info.ValidInAllBranches);
    }

    [Fact]
    public void FeatureName_AlwaysDisabled_ParsesCorrectly()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                feature_name Qux = {AlwaysDisabled, FALSE};
            }
            """, CompilationMode.Compat);

        Assert.True(compilation.FeatureNames.ContainsKey("Qux"));
        var info = compilation.FeatureNames["Qux"];
        Assert.Equal(FeatureStage.AlwaysDisabled, info.Stage);
        Assert.False(info.ValidInAllBranches);
    }

    [Fact]
    public void FeatureName_DeclaredFeature_NoMIDL1076()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                feature_name Foo = {AlwaysEnabled, TRUE};

                [feature(Foo)]
                runtimeclass Widget
                {
                    Widget();
                };
            }
            """, CompilationMode.Compat);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1076");
    }

    [Fact]
    public void FeatureName_InvalidStage_ProducesError()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                feature_name Bad = {InvalidStage, TRUE};
            }
            """, CompilationMode.Compat);

        Assert.Contains(compilation.Diagnostics, d => d.Code == "MIDL1103");
    }

    [Fact]
    public void FeatureName_MultipleDeclarations_AllRegistered()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                feature_name Feature_A = {AlwaysEnabled, TRUE};
                feature_name Feature_B = {DisabledByDefault, FALSE};
                feature_name Feature_C = {EnabledByDefault, TRUE};
            }
            """, CompilationMode.Compat);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        Assert.Equal(3, compilation.FeatureNames.Count);
        Assert.True(compilation.FeatureNames.ContainsKey("Feature_A"));
        Assert.True(compilation.FeatureNames.ContainsKey("Feature_B"));
        Assert.True(compilation.FeatureNames.ContainsKey("Feature_C"));
    }

    [Fact]
    public void FeatureName_FeatureStagingPattern_ParsesCorrectly()
    {
        // Simplified version of the OS MoIdlFeature.0.idl fixture pattern
        var compilation = CompilationHelper.Compile("""
            namespace MoIdlFeatureStaging
            {
                feature_name FeatureStaging_AlwaysAlwaysEnabled = {AlwaysEnabled, TRUE};
                feature_name FeatureStaging_AlwaysDisabledByDefault = {DisabledByDefault, TRUE};
                feature_name FeatureStaging_AlwaysEnabledByDefault = {EnabledByDefault, TRUE};
                feature_name FeatureStaging_AlwaysAlwaysDisabled = {AlwaysDisabled, TRUE};

                apicontract TestContract {};

                [feature(FeatureStaging_AlwaysAlwaysEnabled)]
                [contract(TestContract, 1)]
                [default_interface]
                runtimeclass EnabledWidget
                {
                    EnabledWidget();
                };

                [feature(FeatureStaging_AlwaysDisabledByDefault)]
                [contract(TestContract, 1)]
                [default_interface]
                runtimeclass DisabledWidget
                {
                    DisabledWidget();
                };
            }
            """, CompilationMode.Compat);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        Assert.Equal(4, compilation.FeatureNames.Count);
        Assert.Equal(FeatureStage.AlwaysEnabled, compilation.FeatureNames["FeatureStaging_AlwaysAlwaysEnabled"].Stage);
        Assert.Equal(FeatureStage.DisabledByDefault, compilation.FeatureNames["FeatureStaging_AlwaysDisabledByDefault"].Stage);
        Assert.Equal(FeatureStage.EnabledByDefault, compilation.FeatureNames["FeatureStaging_AlwaysEnabledByDefault"].Stage);
        Assert.Equal(FeatureStage.AlwaysDisabled, compilation.FeatureNames["FeatureStaging_AlwaysAlwaysDisabled"].Stage);
    }
}
#endif
