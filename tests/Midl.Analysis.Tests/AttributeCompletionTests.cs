// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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

    // ─── Attribute argument completion tests ────────────────────────────

    [Fact]
    public void AttrArg_ContractArg0_OffersApiContractNames()
    {
        // Compile a valid IDL to get symbols
        var validIdl = @"
            namespace TestNs
            {
                apicontract PalettesContract { }
                runtimeclass Widget { }
            }";
        var compilation = CompilationHelper.Compile(validIdl);

        // Detect context from incomplete text (as user is typing)
        var editingIdl = @"namespace TestNs { [contract( runtimeclass Widget { } }";
        var ctx = AttributeArgCompletionProvider.DetectAttributeArgContext(editingIdl, editingIdl.IndexOf("[contract(") + "[contract(".Length);
        Assert.NotNull(ctx);
        Assert.Equal("contract", ctx!.Value.attrName);
        Assert.Equal(0, ctx!.Value.argPosition);

        var items = AttributeArgCompletionProvider.BuildAttributeArgCompletions(ctx.Value.attrName, ctx.Value.argPosition, compilation);
        Assert.NotNull(items);
        var labels = items!.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.Contains("PalettesContract", labels);
    }

    [Fact]
    public void AttrArg_ComposableArg0_OffersInterfaceNames()
    {
        var validIdl = @"
            namespace TestNs
            {
                interface IWidgetFactory { }
                interface IWidgetStatics { }
                runtimeclass Widget { }
            }";
        var compilation = CompilationHelper.Compile(validIdl);

        var editingIdl = @"namespace TestNs { [composable( runtimeclass Widget { } }";
        var ctx = AttributeArgCompletionProvider.DetectAttributeArgContext(editingIdl, editingIdl.IndexOf("[composable(") + "[composable(".Length);
        Assert.NotNull(ctx);
        Assert.Equal("composable", ctx!.Value.attrName);
        Assert.Equal(0, ctx!.Value.argPosition);

        var items = AttributeArgCompletionProvider.BuildAttributeArgCompletions(ctx.Value.attrName, ctx.Value.argPosition, compilation);
        Assert.NotNull(items);
        var labels = items!.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.Contains("IWidgetFactory", labels);
        Assert.Contains("IWidgetStatics", labels);
    }

    [Fact]
    public void AttrArg_ComposableArg1_OffersPublicProtected()
    {
        var editingIdl = "namespace TestNs { [composable(IWidgetFactory, runtimeclass Widget { } }";
        int cursorPos = editingIdl.IndexOf("[composable(IWidgetFactory, ") + "[composable(IWidgetFactory, ".Length;
        var ctx = AttributeArgCompletionProvider.DetectAttributeArgContext(editingIdl, cursorPos);
        Assert.NotNull(ctx);
        Assert.Equal("composable", ctx!.Value.attrName);
        Assert.Equal(1, ctx!.Value.argPosition);

        var items = AttributeArgCompletionProvider.BuildAttributeArgCompletions(ctx.Value.attrName, ctx.Value.argPosition, null);
        Assert.NotNull(items);
        var labels = items!.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.Contains("public", labels);
        Assert.Contains("protected", labels);
        Assert.Equal(2, labels.Count);
    }

    [Fact]
    public void AttrArg_ThreadingArg0_OffersBothStaMta()
    {
        var idl = @"
            namespace TestNs
            {
                [threading(
                runtimeclass Widget { }
            }";

        var ctx = AttributeArgCompletionProvider.DetectAttributeArgContext(idl, idl.IndexOf("[threading(") + "[threading(".Length);
        Assert.NotNull(ctx);
        Assert.Equal("threading", ctx!.Value.attrName);
        Assert.Equal(0, ctx!.Value.argPosition);

        var items = AttributeArgCompletionProvider.BuildAttributeArgCompletions(ctx.Value.attrName, ctx.Value.argPosition, null);
        Assert.NotNull(items);
        var labels = items!.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.Contains("both", labels);
        Assert.Contains("sta", labels);
        Assert.Contains("mta", labels);
        Assert.Equal(3, labels.Count);
    }

    [Fact]
    public void AttrArg_ExclusivetoArg0_OffersRuntimeClassNames()
    {
        var validIdl = @"
            namespace TestNs
            {
                runtimeclass Widget { }
                runtimeclass Gadget { }
                interface IWidgetInternal { }
            }";
        var compilation = CompilationHelper.Compile(validIdl);

        var editingIdl = @"namespace TestNs { [exclusiveto( interface IWidgetInternal { } }";
        var ctx = AttributeArgCompletionProvider.DetectAttributeArgContext(editingIdl, editingIdl.IndexOf("[exclusiveto(") + "[exclusiveto(".Length);
        Assert.NotNull(ctx);
        Assert.Equal("exclusiveto", ctx!.Value.attrName);
        Assert.Equal(0, ctx!.Value.argPosition);

        var items = AttributeArgCompletionProvider.BuildAttributeArgCompletions(ctx.Value.attrName, ctx.Value.argPosition, compilation);
        Assert.NotNull(items);
        var labels = items!.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.Contains("Widget", labels);
        Assert.Contains("Gadget", labels);
    }

    [Fact]
    public void AttrArg_AttributeusageArg0_OffersTargetChoices()
    {
        var idl = @"
            namespace TestNs
            {
                [attributeusage(
                attribute MyAttr { }
            }";

        var ctx = AttributeArgCompletionProvider.DetectAttributeArgContext(idl, idl.IndexOf("[attributeusage(") + "[attributeusage(".Length);
        Assert.NotNull(ctx);
        Assert.Equal("attributeusage", ctx!.Value.attrName);
        Assert.Equal(0, ctx!.Value.argPosition);

        var items = AttributeArgCompletionProvider.BuildAttributeArgCompletions(ctx.Value.attrName, ctx.Value.argPosition, null);
        Assert.NotNull(items);
        var labels = items!.Cast<Dictionary<string, object?>>().Select(d => (string)d["label"]!).ToList();
        Assert.Contains("target_runtimeclass", labels);
        Assert.Contains("target_interface", labels);
        Assert.Contains("target_enum", labels);
        Assert.Contains("target_struct", labels);
        Assert.Contains("target_delegate", labels);
        Assert.Contains("target_method", labels);
        Assert.Contains("target_property", labels);
        Assert.Contains("target_event", labels);
        Assert.Contains("target_parameter", labels);
    }

    [Fact]
    public void AttrArg_NotInsideAttributeArgs_ReturnsNull()
    {
        var idl = @"
            namespace TestNs
            {
                runtimeclass Widget { }
            }";

        // Cursor in the middle of "Widget" — not inside any attribute parens
        int cursorPos = idl.IndexOf("Widget") + 3;
        var ctx = AttributeArgCompletionProvider.DetectAttributeArgContext(idl, cursorPos);
        Assert.Null(ctx);
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
