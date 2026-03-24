using Midl.Analysis;
using Midl.CodeGen.Header;

namespace Midl.Integration.Tests;

/// <summary>
/// Golden-file tests that compile parity IDL fixtures and compare
/// the emitted C/C++ header against checked-in expected output.
/// </summary>
public partial class HeaderParityTests
{
    private static readonly string FixturesDir = Path.GetFullPath(
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "fixtures", "valid"));

    private static readonly string GoldenDir = Path.GetFullPath(
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "fixtures", "golden-headers"));

    private readonly HeaderEmitter _emitter = new();

    /// <summary>
    /// Core helper: compile an IDL fixture and compare emitted header against golden file.
    /// </summary>
    private void AssertHeaderMatchesGolden(string fixtureName)
    {
        var idlPath = Path.Combine(FixturesDir, $"{fixtureName}.idl");
        var goldenPath = Path.Combine(GoldenDir, $"{fixtureName}.h");

        Assert.True(File.Exists(idlPath), $"IDL fixture not found: {idlPath}");
        Assert.True(File.Exists(goldenPath), $"Golden header not found: {goldenPath}");

        var source = File.ReadAllText(idlPath);
        var compilation = CompilationHelper.Compile(source);

        // Ensure no compilation errors
        var errors = compilation.Diagnostics
            .Where(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(errors);

        var actual = _emitter.Emit(compilation);
        var expected = File.ReadAllText(goldenPath);

        Assert.Equal(Normalize(expected), Normalize(actual));
    }

    /// <summary>
    /// Core helper for fixtures that need SDK references (generics, async, etc.).
    /// </summary>
    private void AssertHeaderMatchesGoldenWithSdk(string fixtureName)
    {
        var winmd = Midl.Lsp.Handlers.WinMdNavigator.FindWindowsWinMd();
        Skip.If(winmd == null, "Windows SDK not available");

        var idlPath = Path.Combine(FixturesDir, $"{fixtureName}.idl");
        var goldenPath = Path.Combine(GoldenDir, $"{fixtureName}.h");

        Assert.True(File.Exists(idlPath), $"IDL fixture not found: {idlPath}");
        Assert.True(File.Exists(goldenPath), $"Golden header not found: {goldenPath}");

        var source = File.ReadAllText(idlPath);
        var compilation = CompilationHelper.Compile(source, new List<string> { winmd! }, CompilationMode.Strict);

        var errors = compilation.Diagnostics
            .Where(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(errors);

        var actual = _emitter.Emit(compilation);
        var expected = File.ReadAllText(goldenPath);

        Assert.Equal(Normalize(expected), Normalize(actual));
    }

    private static string Normalize(string text)
    {
        // Normalize line endings, trim trailing whitespace per line, trim trailing newlines
        var lines = text.Replace("\r\n", "\n").Split('\n')
            .Select(l => l.TrimEnd())
            .ToArray();

        // Remove trailing empty lines
        var lastNonEmpty = Array.FindLastIndex(lines, l => l.Length > 0);
        if (lastNonEmpty >= 0)
            lines = lines.Take(lastNonEmpty + 1).ToArray();

        return string.Join("\n", lines);
    }

    // ── Simple fixtures (no SDK needed) ──────────────────────────────

    [Fact]
    public void Parity_Structs() => AssertHeaderMatchesGolden("parity_structs");

    [Fact]
    public void Parity_EnumsFlags() => AssertHeaderMatchesGolden("parity_enums_flags");

    [Fact]
    public void Parity_EmptyTypes() => AssertHeaderMatchesGolden("parity_empty_types");

    [Fact]
    public void Parity_Interfaces() => AssertHeaderMatchesGolden("parity_interfaces");

    [Fact]
    public void Parity_EmptyInterface() => AssertHeaderMatchesGolden("parity_empty_interface");

    [Fact]
    public void Parity_AllPrimitives() => AssertHeaderMatchesGolden("parity_all_primitives");

    [Fact]
    public void Parity_EnumVariations() => AssertHeaderMatchesGolden("parity_enum_variations");

    [Fact]
    public void Parity_MultipleEnumsStructs() => AssertHeaderMatchesGolden("parity_multiple_enums_structs");

    [Fact]
    public void Parity_NestedStructs() => AssertHeaderMatchesGolden("parity_nested_structs");

    [Fact]
    public void Parity_ComplexStruct() => AssertHeaderMatchesGolden("parity_complex_struct");

    [Fact]
    public void Parity_LargeEnum() => AssertHeaderMatchesGolden("parity_large_enum");

    [Fact]
    public void Parity_Uint64Enum() => AssertHeaderMatchesGolden("parity_uint64_enum");

    [Fact]
    public void Parity_StructWithEnumField() => AssertHeaderMatchesGolden("parity_struct_with_enum_field");

    [Fact]
    public void Parity_StringEnumStructCombo() => AssertHeaderMatchesGolden("parity_string_enum_struct_combo");

    // ── Interface / class fixtures ───────────────────────────────────

    [Fact]
    public void Parity_InterfaceOnly() => AssertHeaderMatchesGolden("parity_interface_only");

    [Fact]
    public void Parity_InterfaceMethodVariety() => AssertHeaderMatchesGolden("parity_interface_method_variety");

    [Fact]
    public void Parity_InterfaceHierarchy() => AssertHeaderMatchesGolden("parity_interface_hierarchy");

    [Fact]
    public void Parity_InterfaceMultiRequires() => AssertHeaderMatchesGolden("parity_interface_multi_requires");

    [Fact]
    public void Parity_InterfaceInheritanceDiamond() => AssertHeaderMatchesGolden("parity_interface_inheritance_diamond");

    [Fact]
    public void Parity_ManyMethodsInterface() => AssertHeaderMatchesGolden("parity_many_methods_interface");

    [Fact]
    public void Parity_BasicClass() => AssertHeaderMatchesGolden("parity_basic_class");

    [Fact]
    public void Parity_DefaultInterface() => AssertHeaderMatchesGolden("parity_default_interface");

    [Fact]
    public void Parity_DefaultInterfacePatterns() => AssertHeaderMatchesGolden("parity_default_interface_patterns");

    [Fact]
    public void Parity_Delegates() => AssertHeaderMatchesGolden("parity_delegates");

    [Fact]
    public void Parity_DelegateVarieties() => AssertHeaderMatchesGolden("parity_delegate_varieties");

    [Fact]
    public void Parity_DelegateReturnTypes() => AssertHeaderMatchesGolden("parity_delegate_return_types");

    [Fact]
    public void Parity_MultiParamDelegate() => AssertHeaderMatchesGolden("parity_multi_param_delegate");

    [Fact]
    public void Parity_MultipleDelegatesSameSig() => AssertHeaderMatchesGolden("parity_multiple_delegates_same_sig");

    [Fact]
    public void Parity_DelegateInEvent() => AssertHeaderMatchesGolden("parity_delegate_in_event");

    [Fact]
    public void Parity_CustomDelegateEvents() => AssertHeaderMatchesGolden("parity_custom_delegate_events");

    [Fact]
    public void Parity_Overloads() => AssertHeaderMatchesGolden("parity_overloads");

    [Fact]
    public void Parity_OverloadVarieties() => AssertHeaderMatchesGolden("parity_overload_varieties");

    [Fact]
    public void Parity_ReturnTypes() => AssertHeaderMatchesGolden("parity_return_types");

    [Fact]
    public void Parity_OutParams() => AssertHeaderMatchesGolden("parity_out_params");

    [Fact]
    public void Parity_MultiOutParams() => AssertHeaderMatchesGolden("parity_multi_out_params");

    [Fact]
    public void Parity_ComplexParams() => AssertHeaderMatchesGolden("parity_complex_params");

    [Fact]
    public void Parity_ObjectParams() => AssertHeaderMatchesGolden("parity_object_params");

    [Fact]
    public void Parity_PropertyPatterns() => AssertHeaderMatchesGolden("parity_property_patterns");

    [Fact]
    public void Parity_StaticMembers() => AssertHeaderMatchesGolden("parity_static_members");

    [Fact]
    public void Parity_StaticAndInstance() => AssertHeaderMatchesGolden("parity_static_and_instance");

    [Fact]
    public void Parity_MultiStatic() => AssertHeaderMatchesGolden("parity_multi_static");

    [Fact]
    public void Parity_EventOnlyClass() => AssertHeaderMatchesGolden("parity_event_only_class");

    [Fact]
    public void Parity_EventPatterns() => AssertHeaderMatchesGolden("parity_event_patterns");

    [Fact]
    public void Parity_GenericEvent() => AssertHeaderMatchesGolden("parity_generic_event");

    [Fact]
    public void Parity_ManyMembers() => AssertHeaderMatchesGolden("parity_many_members");

    [Fact]
    public void Parity_MethodChaining() => AssertHeaderMatchesGolden("parity_method_chaining");

    [Fact]
    public void Parity_MultipleConstructors() => AssertHeaderMatchesGolden("parity_multiple_constructors");

    [Fact]
    public void Parity_ExplicitInterfaces() => AssertHeaderMatchesGolden("parity_explicit_interfaces");

    [Fact]
    public void Parity_Exclusiveto() => AssertHeaderMatchesGolden("parity_exclusiveto");

    [Fact]
    public void Parity_SealedWithInterfaces() => AssertHeaderMatchesGolden("parity_sealed_with_interfaces");

    [Fact]
    public void Parity_ClassManyInterfaces() => AssertHeaderMatchesGolden("parity_class_many_interfaces");

    [Fact]
    public void Parity_DeepInheritance() => AssertHeaderMatchesGolden("parity_deep_inheritance");

    [Fact]
    public void Parity_DerivedNewInterfaces() => AssertHeaderMatchesGolden("parity_derived_new_interfaces");

    [Fact]
    public void Parity_UnsealedInheritance() => AssertHeaderMatchesGolden("parity_unsealed_inheritance");

    [Fact]
    public void Parity_ForwardDeclaration() => AssertHeaderMatchesGolden("parity_forward_declaration");

    [Fact]
    public void Parity_MixedAccess() => AssertHeaderMatchesGolden("parity_mixed_access");

    [Fact]
    public void Parity_Composable() => AssertHeaderMatchesGolden("parity_composable");

    [Fact]
    public void Parity_MultiActivatable() => AssertHeaderMatchesGolden("parity_multi_activatable");

    [Fact]
    public void Parity_Attributes() => AssertHeaderMatchesGolden("parity_attributes");

    [Fact]
    public void Parity_AllClassAttributes() => AssertHeaderMatchesGolden("parity_all_class_attributes");

    [Fact]
    public void Parity_GuidUsage() => AssertHeaderMatchesGolden("parity_guid_usage");

    [Fact]
    public void Parity_DeprecatedTypes() => AssertHeaderMatchesGolden("parity_deprecated_types");

    [Fact]
    public void Parity_DeprecatedMembersMixed() => AssertHeaderMatchesGolden("parity_deprecated_members_mixed");

    [Fact]
    public void Parity_ContractDeprecatedMethod() => AssertHeaderMatchesGolden("parity_contract_deprecated_method");

    [Fact]
    public void Parity_DeepNamespace() => AssertHeaderMatchesGolden("parity_deep_namespace");

    [Fact]
    public void Parity_MultiNamespace() => AssertHeaderMatchesGolden("parity_multi_namespace");

    [Fact]
    public void Parity_CrossNamespaceImpl() => AssertHeaderMatchesGolden("parity_cross_namespace_impl");

    [Fact]
    public void Parity_CrossNamespaceInherit() => AssertHeaderMatchesGolden("parity_cross_namespace_inherit");

    [Fact]
    public void Parity_VersionedInterface() => AssertHeaderMatchesGolden("parity_versioned_interface");

    [Fact]
    public void Parity_VersionedMembers() => AssertHeaderMatchesGolden("parity_versioned_members");

    [Fact]
    public void Parity_NullableTypes() => AssertHeaderMatchesGolden("parity_nullable_types");

    [Fact]
    public void Parity_CharType() => AssertHeaderMatchesGolden("parity_char_type");

    // ── Fixtures requiring SDK (generics, async, collections) ────────

    [SkippableFact]
    public void Parity_Generics() => AssertHeaderMatchesGoldenWithSdk("parity_generics");

    [SkippableFact]
    public void Parity_NestedGenerics() => AssertHeaderMatchesGoldenWithSdk("parity_nested_generics");

    [SkippableFact]
    public void Parity_AsyncGenerics() => AssertHeaderMatchesGoldenWithSdk("parity_async_generics");

    [SkippableFact]
    public void Parity_GenericCollections() => AssertHeaderMatchesGoldenWithSdk("parity_generic_collections");

    [SkippableFact]
    public void Parity_MapVectorProperties() => AssertHeaderMatchesGoldenWithSdk("parity_map_vector_properties");

    [SkippableFact]
    public void Parity_ObservableCollections() => AssertHeaderMatchesGoldenWithSdk("parity_observable_collections");

    [SkippableFact]
    public void Parity_ArrayParams() => AssertHeaderMatchesGoldenWithSdk("parity_array_params");

    // ── Contract / API contract fixtures ─────────────────────────────

    [Fact]
    public void Parity_Apicontracts() => AssertHeaderMatchesGolden("parity_apicontracts");

    [Fact]
    public void Parity_ApicontractVersioning() => AssertHeaderMatchesGolden("parity_apicontract_versioning");

    // ── Parameterized discovery test ─────────────────────────────────

    [Theory]
    [MemberData(nameof(GetAllParityFixtures))]
    public void HeaderParity_AllFixtures(string fixtureName, string _idlPath, string _goldenPath)
    {
        _ = _idlPath;
        _ = _goldenPath;
        // This theory discovers all parity fixtures automatically.
        // Individual [Fact] tests above cover specific cases;
        // this catches any new fixtures that aren't explicitly listed.
        AssertHeaderMatchesGolden(fixtureName);
    }

    public static IEnumerable<object[]> GetAllParityFixtures()
    {
        var fixturesDir = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "fixtures", "valid"));
        var goldenDir = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "fixtures", "golden-headers"));

        if (!Directory.Exists(fixturesDir) || !Directory.Exists(goldenDir))
            yield break;

        foreach (var idlFile in Directory.GetFiles(fixturesDir, "parity_*.idl").OrderBy(f => f))
        {
            var name = Path.GetFileNameWithoutExtension(idlFile);
            var goldenFile = Path.Combine(goldenDir, $"{name}.h");
            if (File.Exists(goldenFile))
                yield return new object[] { name, idlFile, goldenFile };
        }
    }
}
