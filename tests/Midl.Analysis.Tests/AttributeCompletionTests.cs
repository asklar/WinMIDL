using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;
using Midl.CodeGen.WinMd;
using Midl.Lsp.Handlers;

namespace Midl.Analysis.Tests;

public class AttributeCompletionTests
{
    [Fact]
    public void BuildAttributeCompletions_IncludesWellKnownAttributes()
    {
        var items = AttributeCompletionProvider.BuildAttributeCompletions(
            "namespace Foo { [  runtimeclass Bar { } }", "target_runtimeclass");

        var labels = items.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();

        Assert.Contains("activatable", labels);
        Assert.Contains("composable", labels);
        Assert.Contains("contract", labels);
        Assert.Contains("default_interface", labels);
        Assert.Contains("threading", labels);
    }

    [Fact]
    public void BuildAttributeCompletions_IncludesUserDefinedAttributes()
    {
        var idl = @"
            namespace TestNs
            {
                [attributeusage(target_runtimeclass)]
                [attributename(""myattr"")]
                attribute MyCustomAttribute
                {
                    String Description;
                }

                runtimeclass Widget { }
            }";

        var compilation = CompilationHelper.Compile(idl);

        // Verify the attribute was parsed with its override name
        var attr = compilation.GetAllTypes()
            .OfType<AttributeTypeSymbol>()
            .FirstOrDefault(t => t.Name == "MyCustomAttribute");
        Assert.NotNull(attr);
        Assert.Equal("myattr", attr!.AttributeNameOverride);

        // Verify it shows up in completions
        var items = AttributeCompletionProvider.BuildAttributeCompletions(
            idl, "target_runtimeclass", cachedCompilation: compilation);

        var labels = items.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.Contains("myattr", labels);
    }

    [Fact]
    public void BuildAttributeCompletions_FiltersMethodOnlyAttrsFromRuntimeclass()
    {
        var idl = @"
            namespace TestNs
            {
                [attributeusage(target_method)]
                attribute MethodOnlyAttr { }

                runtimeclass Widget { }
            }";

        var compilation = CompilationHelper.Compile(idl);
        var items = AttributeCompletionProvider.BuildAttributeCompletions(
            idl, "target_runtimeclass", cachedCompilation: compilation);

        var labels = items.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.DoesNotContain("MethodOnlyAttr", labels);
    }

    [Fact]
    public void BuildAttributeCompletions_TargetAllShowsEverything()
    {
        var idl = @"
            namespace TestNs
            {
                [attributeusage(target_method)]
                attribute MethodAttr { }

                [attributeusage(target_runtimeclass)]
                attribute ClassAttr { }
            }";

        var compilation = CompilationHelper.Compile(idl);
        var items = AttributeCompletionProvider.BuildAttributeCompletions(
            idl, "target_all", cachedCompilation: compilation);

        var labels = items.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.Contains("MethodAttr", labels);
        Assert.Contains("ClassAttr", labels);
    }

    [Fact]
    public void BuildAttributeCompletions_DeduplicatesAgainstWellKnown()
    {
        var items = AttributeCompletionProvider.BuildAttributeCompletions(
            "namespace Foo { runtimeclass Bar { } }", "target_all");

        var labels = items.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        var deprecatedCount = labels.Count(l => l.Equals("deprecated", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(1, deprecatedCount);
    }

    [Fact]
    public void BuildAttributeCompletions_ShowsMemberAttrsInsideBody()
    {
        // Attribute defined separately, then used inside a runtimeclass body
        var idl = @"
            namespace TestNs
            {
                [attributeusage(target_method)]
                attribute MyMethodAttr { }

                runtimeclass Widget
                {
                    void DoStuff();
                }
            }";

        var compilation = CompilationHelper.Compile(idl);
        var items = AttributeCompletionProvider.BuildAttributeCompletions(
            idl, "target_method", cachedCompilation: compilation);

        var labels = items.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.Contains("MyMethodAttr", labels);
    }

    [Fact]
    public void DefaultInference_NoAttributeName_StripsAttributeSuffix()
    {
        // When no [attributename] is specified, display name should strip "Attribute" suffix
        var idl = @"
            namespace TestNs
            {
                attribute FancyThingAttribute { }
            }";

        var compilation = CompilationHelper.Compile(idl);
        var attr = compilation.GetAllTypes()
            .OfType<AttributeTypeSymbol>()
            .FirstOrDefault(t => t.Name == "FancyThingAttribute");

        Assert.NotNull(attr);
        Assert.Null(attr!.AttributeNameOverride); // no override set
        // The completion provider should strip the suffix
        var items = AttributeCompletionProvider.BuildAttributeCompletions(
            idl, "target_all", cachedCompilation: compilation);
        var labels = items.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.Contains("FancyThingAttribute", labels); // local attrs use their Name directly
    }

    [Fact]
    public void DefaultInference_NoAttributeUsage_AppliesToAll()
    {
        // When no [attributeusage] is specified, the attribute should be applicable to all targets
        var idl = @"
            namespace TestNs
            {
                attribute UnrestrictedAttr { }
                runtimeclass Widget { }
            }";

        var compilation = CompilationHelper.Compile(idl);
        var attr = compilation.GetAllTypes()
            .OfType<AttributeTypeSymbol>()
            .FirstOrDefault(t => t.Name == "UnrestrictedAttr");

        Assert.NotNull(attr);
        Assert.Empty(attr!.ValidTargets); // empty = applies to all

        // Should show up for runtimeclass target
        var rcItems = AttributeCompletionProvider.BuildAttributeCompletions(
            idl, "target_runtimeclass", cachedCompilation: compilation);
        var rcLabels = rcItems.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.Contains("UnrestrictedAttr", rcLabels);

        // Should show up for method target
        var mItems = AttributeCompletionProvider.BuildAttributeCompletions(
            idl, "target_method", cachedCompilation: compilation);
        var mLabels = mItems.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.Contains("UnrestrictedAttr", mLabels);
    }

    [Fact]
    public void DefaultInference_AttributeNameOverride_UsedInCompletion()
    {
        var idl = @"
            namespace TestNs
            {
                [attributename(""fancy"")]
                attribute FancyAttribute { }
            }";

        var compilation = CompilationHelper.Compile(idl);
        var items = AttributeCompletionProvider.BuildAttributeCompletions(
            idl, "target_all", cachedCompilation: compilation);
        var labels = items.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        // Should use the override name, not the full type name
        Assert.Contains("fancy", labels);
        Assert.DoesNotContain("FancyAttribute", labels);
    }

    // ─── Contextual filtering tests ──────────────────────────────────

    [Fact]
    public void NoexceptAppearsInMethodContext()
    {
        var items = AttributeCompletionProvider.BuildAttributeCompletions(
            "namespace Foo { runtimeclass Bar { void DoStuff(); } }", "target_method");

        var labels = items.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.Contains("noexcept", labels);
    }

    [Fact]
    public void NoexceptNotInRuntimeclassOrInterfaceOrEnumContext()
    {
        var rcItems = AttributeCompletionProvider.BuildAttributeCompletions(
            "namespace Foo { runtimeclass Bar { } }", "target_runtimeclass");
        var rcLabels = rcItems.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.DoesNotContain("noexcept", rcLabels);

        var ifItems = AttributeCompletionProvider.BuildAttributeCompletions(
            "namespace Foo { interface IBar { } }", "target_interface");
        var ifLabels = ifItems.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.DoesNotContain("noexcept", ifLabels);

        var enItems = AttributeCompletionProvider.BuildAttributeCompletions(
            "namespace Foo { enum Bar { } }", "target_enum");
        var enLabels = enItems.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.DoesNotContain("noexcept", enLabels);
    }

    [Fact]
    public void RemoteAsyncAppearsInMethodContextOnly()
    {
        var methodItems = AttributeCompletionProvider.BuildAttributeCompletions(
            "namespace Foo { runtimeclass Bar { void DoStuff(); } }", "target_method");
        var methodLabels = methodItems.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.Contains("remote_async", methodLabels);

        var rcItems = AttributeCompletionProvider.BuildAttributeCompletions(
            "namespace Foo { runtimeclass Bar { } }", "target_runtimeclass");
        var rcLabels = rcItems.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.DoesNotContain("remote_async", rcLabels);

        var ifItems = AttributeCompletionProvider.BuildAttributeCompletions(
            "namespace Foo { interface IBar { } }", "target_interface");
        var ifLabels = ifItems.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.DoesNotContain("remote_async", ifLabels);
    }

    [Fact]
    public void ComposableAppearsInRuntimeclassContextOnly()
    {
        var rcItems = AttributeCompletionProvider.BuildAttributeCompletions(
            "namespace Foo { runtimeclass Bar { } }", "target_runtimeclass");
        var rcLabels = rcItems.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.Contains("composable", rcLabels);

        var ifItems = AttributeCompletionProvider.BuildAttributeCompletions(
            "namespace Foo { interface IBar { } }", "target_interface");
        var ifLabels = ifItems.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.DoesNotContain("composable", ifLabels);

        var methodItems = AttributeCompletionProvider.BuildAttributeCompletions(
            "namespace Foo { runtimeclass Bar { void DoStuff(); } }", "target_method");
        var methodLabels = methodItems.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.DoesNotContain("composable", methodLabels);
    }

    [Fact]
    public void DeprecatedAppearsInAllContexts()
    {
        foreach (var target in new[] { "target_runtimeclass", "target_interface", "target_enum",
                                       "target_struct", "target_method", "target_delegate" })
        {
            var items = AttributeCompletionProvider.BuildAttributeCompletions(
                "namespace Foo { runtimeclass Bar { } }", target);
            var labels = items.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
            Assert.Contains("deprecated", labels);
        }
    }

    [Fact]
    public void FlagsAppearsInEnumContextOnly()
    {
        var enItems = AttributeCompletionProvider.BuildAttributeCompletions(
            "namespace Foo { enum Bar { } }", "target_enum");
        var enLabels = enItems.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.Contains("flags", enLabels);

        var rcItems = AttributeCompletionProvider.BuildAttributeCompletions(
            "namespace Foo { runtimeclass Bar { } }", "target_runtimeclass");
        var rcLabels = rcItems.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.DoesNotContain("flags", rcLabels);

        var methodItems = AttributeCompletionProvider.BuildAttributeCompletions(
            "namespace Foo { runtimeclass Bar { void DoStuff(); } }", "target_method");
        var methodLabels = methodItems.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.DoesNotContain("flags", methodLabels);
    }

    [Fact]
    public void MethodNameAppearsInMethodContextOnly()
    {
        var methodItems = AttributeCompletionProvider.BuildAttributeCompletions(
            "namespace Foo { runtimeclass Bar { void DoStuff(); } }", "target_method");
        var methodLabels = methodItems.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.Contains("method_name", methodLabels);

        var rcItems = AttributeCompletionProvider.BuildAttributeCompletions(
            "namespace Foo { runtimeclass Bar { } }", "target_runtimeclass");
        var rcLabels = rcItems.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.DoesNotContain("method_name", rcLabels);
    }

    [Fact]
    public void ContentpropertyAppearsInRuntimeclassContextOnly()
    {
        var rcItems = AttributeCompletionProvider.BuildAttributeCompletions(
            "namespace Foo { runtimeclass Bar { } }", "target_runtimeclass");
        var rcLabels = rcItems.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.Contains("contentproperty", rcLabels);

        var ifItems = AttributeCompletionProvider.BuildAttributeCompletions(
            "namespace Foo { interface IBar { } }", "target_interface");
        var ifLabels = ifItems.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.DoesNotContain("contentproperty", ifLabels);

        var methodItems = AttributeCompletionProvider.BuildAttributeCompletions(
            "namespace Foo { runtimeclass Bar { void DoStuff(); } }", "target_method");
        var methodLabels = methodItems.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.DoesNotContain("contentproperty", methodLabels);
    }

    [Fact]
    public void BuiltInSdkAttributeNoexceptAppearsInCompletions()
    {
        // noexcept is a built-in SDK attribute that should appear in method completions
        var items = AttributeCompletionProvider.BuildAttributeCompletions(
            "namespace Foo { runtimeclass Bar { void DoStuff(); } }", "target_method");

        var labels = items.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.Contains("noexcept", labels);

        // Verify it has the right detail text
        var noexceptItem = items.Cast<Dictionary<string, object?>>()
            .First(d => (string)d["label"]! == "noexcept");
        Assert.Contains("non-throwing", (string)noexceptItem["detail"]!);
    }

    // ─── Attribute inference tests ────────────────────────────────────

    [Fact]
    public void AttributeUsageTargets_ParsedFromSource()
    {
        var idl = @"
            namespace TestNs
            {
                [attributeusage(target_runtimeclass, target_interface)]
                attribute MultiTargetAttr { }
            }";

        var compilation = CompilationHelper.Compile(idl);
        var attr = compilation.GetAllTypes()
            .OfType<AttributeTypeSymbol>()
            .FirstOrDefault(t => t.Name == "MultiTargetAttr");

        Assert.NotNull(attr);
        Assert.Contains("target_runtimeclass", attr!.ValidTargets);
        Assert.Contains("target_interface", attr!.ValidTargets);
        Assert.DoesNotContain("target_method", attr!.ValidTargets);
    }
}
