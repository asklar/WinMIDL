using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis.Tests;

/// <summary>Tests for ImportResolver (Task 1: idl-imports).</summary>
public class ImportResolverTests
{
    [Fact]
    public void ResolveRelativePath()
    {
        // Create temp files
        var dir = Path.Combine(Path.GetTempPath(), $"midl_import_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(dir);
        try
        {
            var mainFile = Path.Combine(dir, "main.idl");
            var importedFile = Path.Combine(dir, "types.idl");
            File.WriteAllText(mainFile, "import \"types.idl\";");
            File.WriteAllText(importedFile, "namespace Ext { runtimeclass Foo { } }");

            var resolver = new ImportResolver(includePaths: null, currentFilePath: mainFile);
            var resolved = resolver.ResolveImportPath("types.idl");

            Assert.NotNull(resolved);
            Assert.Equal(Path.GetFullPath(importedFile), resolved);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ResolveFromIncludePath()
    {
        var dir1 = Path.Combine(Path.GetTempPath(), $"midl_import_test1_{Guid.NewGuid()}");
        var dir2 = Path.Combine(Path.GetTempPath(), $"midl_import_test2_{Guid.NewGuid()}");
        Directory.CreateDirectory(dir1);
        Directory.CreateDirectory(dir2);
        try
        {
            var mainFile = Path.Combine(dir1, "main.idl");
            var importedFile = Path.Combine(dir2, "types.idl");
            File.WriteAllText(mainFile, "import \"types.idl\";");
            File.WriteAllText(importedFile, "namespace Ext { runtimeclass Bar { } }");

            var resolver = new ImportResolver(includePaths: new[] { dir2 }, currentFilePath: mainFile);
            var resolved = resolver.ResolveImportPath("types.idl");

            Assert.NotNull(resolved);
            Assert.Equal(Path.GetFullPath(importedFile), resolved);
        }
        finally
        {
            Directory.Delete(dir1, true);
            Directory.Delete(dir2, true);
        }
    }

    [Fact]
    public void RedundantImportSkipped()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"midl_import_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(dir);
        try
        {
            var importedFile = Path.Combine(dir, "types.idl");
            File.WriteAllText(importedFile, "namespace Ext { runtimeclass Qux { } }");

            var resolver = new ImportResolver(includePaths: null, currentFilePath: Path.Combine(dir, "main.idl"));
            var resolved = resolver.ResolveImportPath("types.idl");
            Assert.NotNull(resolved);

            Assert.False(resolver.IsAlreadyImported(resolved!));
            resolver.MarkImported(resolved!);
            Assert.True(resolver.IsAlreadyImported(resolved!));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ImportedTypesResolvable()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"midl_import_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(dir);
        try
        {
            var mainFile = Path.Combine(dir, "main.idl");
            var importedFile = Path.Combine(dir, "types.idl");
            File.WriteAllText(importedFile,
                "namespace External { runtimeclass Widget { } }");

            var mainSource = @"
                import ""types.idl"";
                namespace MyApp
                {
                    runtimeclass Consumer
                    {
                        External.Widget GetWidget();
                    }
                }";
            File.WriteAllText(mainFile, mainSource);

            var compilation = CompilationHelper.Compile(
                mainSource,
                referencePaths: null,
                CompilationMode.Strict,
                includePaths: null,
                currentFilePath: mainFile);

            // The External.Widget type should be resolved from the import
            var errors = compilation.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList();
            Assert.Empty(errors);

            var widget = compilation.GetTypeByName("External.Widget");
            Assert.NotNull(widget);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }
}

/// <summary>Tests for partial runtimeclass merging (Task 2: partial-merge).</summary>
public class PartialMergeTests
{
    [Fact]
    public void TwoPartialsMerged()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                partial runtimeclass Widget
                {
                    void DoWork();
                }
                partial runtimeclass Widget
                {
                    String Name { get; };
                }
            }
            """);

        var errors = compilation.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(errors);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var widgets = ns.Members.OfType<RuntimeClassSymbol>().Where(m => m.Name == "Widget").ToList();
        Assert.Single(widgets); // Only one merged symbol
    }

    [Fact]
    public void PartialMembersFromBothPresent()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                partial runtimeclass Widget
                {
                    void DoWork();
                }
                partial runtimeclass Widget
                {
                    String Name { get; };
                }
            }
            """);

        var ns = compilation.GlobalNamespace.ChildNamespaces["TestApp"];
        var widget = ns.Members.OfType<RuntimeClassSymbol>().Single(m => m.Name == "Widget");

        var methods = widget.Members.OfType<MethodSymbol>().ToList();
        Assert.Contains(methods, m => m.Name == "DoWork");

        var props = widget.Members.OfType<PropertySymbol>().ToList();
        Assert.Contains(props, p => p.Name == "Name");
    }

    [Fact]
    public void ConflictingBaseClassDiagnostic()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass Base1 { }
                runtimeclass Base2 { }
                partial runtimeclass Widget : Base1
                {
                    void DoWork();
                }
                partial runtimeclass Widget : Base2
                {
                    String Name { get; };
                }
            }
            """);

        var errors = compilation.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        Assert.Contains(errors, e => e.Code == "MIDL1032" && e.Message.Contains("conflicting base class"));
    }
}

/// <summary>Tests for PIID algorithm (Task 3: piid-algorithm).</summary>
public class PiidGeneratorTests
{
    [Fact]
    public void Int32Signature()
    {
        Assert.Equal("i4", PiidGenerator.GetPrimitiveTypeSignature("Int32"));
    }

    [Fact]
    public void StringSignature()
    {
        Assert.Equal("string", PiidGenerator.GetPrimitiveTypeSignature("String"));
    }

    [Fact]
    public void IVectorStringIid()
    {
        // IVector<String> has a well-known IID per the WinRT spec
        // IVector`1 piid = {913337e9-11a1-4345-a3a2-4e7f956e222d}
        var piid = "913337e9-11a1-4345-a3a2-4e7f956e222d";
        var typeArgs = new[] { "string" };

        var iid = PiidGenerator.GenerateIid(piid, typeArgs);

        // The expected IID for IVector<String> per UUID v5 with WinRT namespace:
        // {98b9acc1-4b56-532e-ac73-03d5291cca90}
        Assert.Equal(new Guid("98b9acc1-4b56-532e-ac73-03d5291cca90"), iid);
    }

    [Fact]
    public void NestedPinterfaceSignature()
    {
        // Test that nested pinterface signatures can be computed
        // IVector<IVector<String>> builds on IVector<String>'s IID
        var innerPiid = "913337e9-11a1-4345-a3a2-4e7f956e222d";
        var innerIid = PiidGenerator.GenerateIid(innerPiid, new[] { "string" });
        var outerSig = $"pinterface({{{innerPiid}}};string)";

        // Now compute IVector<IVector<String>> using the inner result
        var outerTypeArgs = new[] { $"pinterface({{{innerPiid}}};string)" };
        var outerIid = PiidGenerator.GenerateIid(innerPiid, outerTypeArgs);

        // Should be deterministic and not equal to the inner IID
        Assert.NotEqual(innerIid, outerIid);
        // Should be deterministic (same inputs = same output)
        var outerIid2 = PiidGenerator.GenerateIid(innerPiid, outerTypeArgs);
        Assert.Equal(outerIid, outerIid2);
    }
}

/// <summary>Tests for WinMD custom attribute reading (Task 4: winmd-read-attrs).</summary>
public class WinMdCustomAttributeTests
{
    /// <summary>Helper: compile IDL source to WinMD bytes using the existing emitter.</summary>
    private static byte[] EmitWinMd(string idlSource)
    {
        var compilation = CompilationHelper.Compile(idlSource);
        Assert.False(compilation.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
            "IDL compilation should not have errors: " +
            string.Join("; ", compilation.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => d.Message)));
        var emitter = new Midl.CodeGen.WinMd.WinMdEmitter();
        return emitter.EmitToBytes(compilation);
    }

    [Fact]
    public void ReadUuidFromImported()
    {
        var winmd = EmitWinMd(@"
            namespace TestNs
            {
                [uuid(""AABBCCDD-EEFF-0011-2233-445566778899"")]
                interface IMyInterface
                {
                    void DoSomething();
                };
            }");

        var reader = new WinMdReader();
        var types = reader.ReadTypes(winmd);

        var iface = types.OfType<InterfaceSymbol>().FirstOrDefault(i => i.Name == "IMyInterface");
        Assert.NotNull(iface);
        Assert.NotNull(iface.Uuid);
        Assert.Equal(new Guid("AABBCCDD-EEFF-0011-2233-445566778899"), iface.Uuid);
    }

    [Fact]
    public void ReadVersionFromImported()
    {
        var winmd = EmitWinMd(@"
            namespace TestNs
            {
                [version(0x00020000)]
                [uuid(""12345678-1234-1234-1234-123456789abc"")]
                interface IVersioned
                {
                    void Foo();
                };
            }");

        var reader = new WinMdReader();
        var types = reader.ReadTypes(winmd);

        var iface = types.OfType<InterfaceSymbol>().FirstOrDefault(i => i.Name == "IVersioned");
        Assert.NotNull(iface);
        // Version should be populated from the [version] attribute
        Assert.NotNull(iface.Version);
    }

    [Fact]
    public void ReadDeprecatedFromImported()
    {
        var winmd = EmitWinMd(@"
            namespace TestNs
            {
                [deprecated(""Use INewInterface instead"", deprecate, 0x00010000)]
                [uuid(""12345678-ABCD-EFEF-1234-123456789abc"")]
                interface ILegacy
                {
                    void OldMethod();
                };
            }");

        var reader = new WinMdReader();
        var types = reader.ReadTypes(winmd);

        var iface = types.OfType<InterfaceSymbol>().FirstOrDefault(i => i.Name == "ILegacy");
        Assert.NotNull(iface);
        // DeprecatedMessage should be populated
        Assert.NotNull(iface.DeprecatedMessage);
        Assert.Contains("Use INewInterface instead", iface.DeprecatedMessage);
    }
}
