using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;
using Midl.CodeGen.WinMd;

namespace Midl.CodeGen.Tests;

public class WinMdEmitterTests
{
    private static byte[] CompileAndEmit(string source)
    {
        var compilation = CompilationHelper.Compile(source);
        Assert.Empty(compilation.Diagnostics);
        var emitter = new WinMdEmitter();
        return emitter.EmitToBytes(compilation);
    }

    private static MetadataReader CreateReader(byte[] bytes, out PEReader peReader)
    {
        peReader = new PEReader(new MemoryStream(bytes));
        return peReader.GetMetadataReader();
    }

    private static TypeDefinitionHandle FindTypeDef(MetadataReader reader, string name)
    {
        foreach (var handle in reader.TypeDefinitions)
        {
            var td = reader.GetTypeDefinition(handle);
            if (reader.GetString(td.Name) == name)
                return handle;
        }
        throw new Exception($"TypeDef '{name}' not found");
    }

    private static bool HasCustomAttribute(MetadataReader reader, EntityHandle parent, string attributeTypeName)
    {
        foreach (var attrHandle in reader.GetCustomAttributes(parent))
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            if (attr.Constructor.Kind == HandleKind.MemberReference)
            {
                var memberRef = reader.GetMemberReference((MemberReferenceHandle)attr.Constructor);
                if (memberRef.Parent.Kind == HandleKind.TypeReference)
                {
                    var typeRef = reader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
                    var fullName = $"{reader.GetString(typeRef.Namespace)}.{reader.GetString(typeRef.Name)}";
                    if (fullName == attributeTypeName)
                        return true;
                }
            }
        }
        return false;
    }

    [Fact]
    public void EmitSimpleEnum()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                enum Color
                {
                    Red,
                    Green,
                    Blue,
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Color");
            var td = reader.GetTypeDefinition(handle);

            Assert.Equal("Test", reader.GetString(td.Namespace));
            // Public | Sealed | WindowsRuntime
            Assert.True(td.Attributes.HasFlag(TypeAttributes.Public));
            Assert.True(td.Attributes.HasFlag(TypeAttributes.Sealed));
            Assert.True(((int)td.Attributes & MetadataConstants.TdWindowsRuntime) != 0);

            // Base type should be System.Enum
            Assert.Equal(HandleKind.TypeReference, td.BaseType.Kind);
            var baseRef = reader.GetTypeReference((TypeReferenceHandle)td.BaseType);
            Assert.Equal("Enum", reader.GetString(baseRef.Name));
            Assert.Equal("System", reader.GetString(baseRef.Namespace));

            // Should have 4 fields: value__ + Red, Green, Blue
            var fields = td.GetFields().Select(f => reader.GetFieldDefinition(f)).ToList();
            Assert.Equal(4, fields.Count);

            // First field: value__
            Assert.Equal("value__", reader.GetString(fields[0].Name));
            Assert.True(fields[0].Attributes.HasFlag(FieldAttributes.SpecialName));

            // Member fields
            Assert.Equal("Red", reader.GetString(fields[1].Name));
            Assert.Equal("Green", reader.GetString(fields[2].Name));
            Assert.Equal("Blue", reader.GetString(fields[3].Name));

            foreach (var field in fields.Skip(1))
            {
                Assert.True(field.Attributes.HasFlag(FieldAttributes.Static));
                Assert.True(field.Attributes.HasFlag(FieldAttributes.Literal));
                Assert.True(field.Attributes.HasFlag(FieldAttributes.HasDefault));
            }

            // Verify constants exist with correct values
            var constantValues = fields.Skip(1).Select(f =>
            {
                var constant = reader.GetConstant(f.GetDefaultValue());
                var blobReader = reader.GetBlobReader(constant.Value);
                return blobReader.ReadInt32();
            }).ToList();
            Assert.Equal(new[] { 0, 1, 2 }, constantValues);
        }
    }

    [Fact]
    public void EmitFlagsEnum()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [flags]
                enum Perms
                {
                    None,
                    Read,
                    Write,
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Perms");
            var td = reader.GetTypeDefinition(handle);

            // Should have FlagsAttribute
            Assert.True(HasCustomAttribute(reader, handle, "System.FlagsAttribute"));

            // value__ field should be UInt32 for flags enum
            var fields = td.GetFields().Select(f => reader.GetFieldDefinition(f)).ToList();
            Assert.True(fields.Count >= 1);

            // value__ signature check: field sig byte, then UInt32 type (0x09)
            var valueFieldSig = reader.GetBlobReader(fields[0].Signature);
            Assert.Equal(0x06, valueFieldSig.ReadByte()); // FIELD calling convention
            Assert.Equal(0x09, valueFieldSig.ReadByte()); // ELEMENT_TYPE_U4 (UInt32)

            // Constant values should be UInt32
            var constant = reader.GetConstant(fields[1].GetDefaultValue());
            Assert.Equal(ConstantTypeCode.UInt32, constant.TypeCode);
        }
    }

    [Fact]
    public void EmitStruct()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                struct Point
                {
                    Int32 X;
                    Int32 Y;
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Point");
            var td = reader.GetTypeDefinition(handle);

            Assert.Equal("Test", reader.GetString(td.Namespace));
            Assert.True(td.Attributes.HasFlag(TypeAttributes.Sealed));
            Assert.True(td.Attributes.HasFlag(TypeAttributes.SequentialLayout));
            Assert.True(((int)td.Attributes & MetadataConstants.TdWindowsRuntime) != 0);

            // Base type should be System.ValueType
            var baseRef = reader.GetTypeReference((TypeReferenceHandle)td.BaseType);
            Assert.Equal("ValueType", reader.GetString(baseRef.Name));

            // Should have 2 fields
            var fields = td.GetFields().Select(f => reader.GetFieldDefinition(f)).ToList();
            Assert.Equal(2, fields.Count);
            Assert.Equal("X", reader.GetString(fields[0].Name));
            Assert.Equal("Y", reader.GetString(fields[1].Name));

            // Both should be public
            Assert.True(fields[0].Attributes.HasFlag(FieldAttributes.Public));
            Assert.True(fields[1].Attributes.HasFlag(FieldAttributes.Public));
        }
    }

    [Fact]
    public void EmitInterface()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                interface IFoo
                {
                    void Bar();
                    String Name { get; };
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "IFoo");
            var td = reader.GetTypeDefinition(handle);

            Assert.Equal("Test", reader.GetString(td.Namespace));
            Assert.True(td.Attributes.HasFlag(TypeAttributes.Interface));
            Assert.True(td.Attributes.HasFlag(TypeAttributes.Abstract));
            Assert.True(((int)td.Attributes & MetadataConstants.TdWindowsRuntime) != 0);

            // Methods: Bar, get_Name
            var methods = td.GetMethods().Select(m => reader.GetMethodDefinition(m)).ToList();
            Assert.Equal(2, methods.Count);

            var methodNames = methods.Select(m => reader.GetString(m.Name)).ToList();
            Assert.Contains("Bar", methodNames);
            Assert.Contains("get_Name", methodNames);

            // Bar should be Virtual | Abstract | Public
            var bar = methods.First(m => reader.GetString(m.Name) == "Bar");
            Assert.True(bar.Attributes.HasFlag(MethodAttributes.Virtual));
            Assert.True(bar.Attributes.HasFlag(MethodAttributes.Abstract));
            Assert.True(bar.Attributes.HasFlag(MethodAttributes.Public));
        }
    }

    [Fact]
    public void EmitRuntimeClass()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                runtimeclass MyClass
                {
                    MyClass();
                    void DoWork();
                    String Name { get; };
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            // Verify runtime class TypeDef
            var classHandle = FindTypeDef(reader, "MyClass");
            var classTd = reader.GetTypeDefinition(classHandle);

            Assert.Equal("Test", reader.GetString(classTd.Namespace));
            Assert.True(classTd.Attributes.HasFlag(TypeAttributes.Public));
            Assert.True(classTd.Attributes.HasFlag(TypeAttributes.Sealed));
            Assert.True(((int)classTd.Attributes & MetadataConstants.TdWindowsRuntime) != 0);

            // Base type is System.Object
            var baseRef = reader.GetTypeReference((TypeReferenceHandle)classTd.BaseType);
            Assert.Equal("Object", reader.GetString(baseRef.Name));

            // ActivatableAttribute must be present
            Assert.True(HasCustomAttribute(reader, classHandle,
                "Windows.Foundation.Metadata.ActivatableAttribute"));

            // Verify synthesized interface exists
            var ifaceHandle = FindTypeDef(reader, "IMyClass");
            var ifaceTd = reader.GetTypeDefinition(ifaceHandle);
            Assert.True(ifaceTd.Attributes.HasFlag(TypeAttributes.Interface));

            // Synthesized interface should have the methods
            var ifaceMethods = ifaceTd.GetMethods()
                .Select(m => reader.GetString(reader.GetMethodDefinition(m).Name))
                .ToList();
            Assert.Contains("DoWork", ifaceMethods);
            Assert.Contains("get_Name", ifaceMethods);

            // Runtime class should have .ctor and runtime method stubs
            var classMethods = classTd.GetMethods()
                .Select(m => reader.GetString(reader.GetMethodDefinition(m).Name))
                .ToList();
            Assert.Contains(".ctor", classMethods);
            Assert.Contains("DoWork", classMethods);
            Assert.Contains("get_Name", classMethods);

            // Verify InterfaceImpl exists on the class
            var ifaceImpls = classTd.GetInterfaceImplementations()
                .Select(h => reader.GetInterfaceImplementation(h))
                .ToList();
            Assert.True(ifaceImpls.Count >= 1);
        }
    }

    [Fact]
    public void EmitDelegate()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                delegate void Handler(Int32 x);
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Handler");
            var td = reader.GetTypeDefinition(handle);

            Assert.Equal("Test", reader.GetString(td.Namespace));
            Assert.True(td.Attributes.HasFlag(TypeAttributes.Public));
            Assert.True(td.Attributes.HasFlag(TypeAttributes.Sealed));
            Assert.True(((int)td.Attributes & MetadataConstants.TdWindowsRuntime) != 0);

            // Base type should be System.MulticastDelegate
            var baseRef = reader.GetTypeReference((TypeReferenceHandle)td.BaseType);
            Assert.Equal("MulticastDelegate", reader.GetString(baseRef.Name));

            // Methods: .ctor and Invoke
            var methods = td.GetMethods().Select(m => reader.GetMethodDefinition(m)).ToList();
            var methodNames = methods.Select(m => reader.GetString(m.Name)).ToList();
            Assert.Contains(".ctor", methodNames);
            Assert.Contains("Invoke", methodNames);

            // Invoke should have 1 parameter
            var invoke = methods.First(m => reader.GetString(m.Name) == "Invoke");
            var parameters = invoke.GetParameters()
                .Select(p => reader.GetParameter(p))
                .ToList();
            Assert.Single(parameters);
            Assert.Equal("x", reader.GetString(parameters[0].Name));

            // GuidAttribute should be present
            Assert.True(HasCustomAttribute(reader, handle,
                "Windows.Foundation.Metadata.GuidAttribute"));
        }
    }

    [Fact]
    public void EmitToFile()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.winmd");
        try
        {
            var compilation = CompilationHelper.Compile("""
                namespace TestFile
                {
                    enum Status
                    {
                        Active,
                        Inactive,
                    }

                    struct Size
                    {
                        Int32 Width;
                        Int32 Height;
                    }

                    interface IWidget
                    {
                        void Show();
                    }

                    [default_interface]
                    runtimeclass Widget
                    {
                        Widget();
                    }

                    delegate void Callback();
                }
                """);
            Assert.Empty(compilation.Diagnostics);

            var emitter = new WinMdEmitter();
            emitter.Emit(compilation, tempPath);

            Assert.True(File.Exists(tempPath));

            // Read back and verify
            using var stream = File.OpenRead(tempPath);
            using var pe = new PEReader(stream);
            var reader = pe.GetMetadataReader();

            // Verify version string
            Assert.Equal(MetadataConstants.VersionString, reader.MetadataVersion);

            // Count user types (excluding <Module>)
            var userTypes = reader.TypeDefinitions
                .Select(h => reader.GetTypeDefinition(h))
                .Where(td => reader.GetString(td.Name) != "<Module>")
                .Select(td => reader.GetString(td.Name))
                .ToList();

            // Should have: Status, Size, IWidget, IWidget (synth for Widget), Widget, Callback = 6
            Assert.Contains("Status", userTypes);
            Assert.Contains("Size", userTypes);
            Assert.Contains("IWidget", userTypes);
            Assert.Contains("Widget", userTypes);
            Assert.Contains("Callback", userTypes);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    [Fact]
    public void EmitPerNamespace_SingleNamespace()
    {
        var compilation = CompilationHelper.Compile("namespace MyApp { runtimeclass Foo { Foo(); } }");
        var emitter = new WinMdEmitter();
        var result = emitter.EmitPerNamespace(compilation);
        Assert.Single(result);
        Assert.True(result.ContainsKey("MyApp"));
    }

    [Fact]
    public void EmitPerNamespace_NestedNamespaces_CommonPrefix()
    {
        var compilation = CompilationHelper.Compile("""
            namespace MyApp.Widgets { runtimeclass Button { Button(); } }
            namespace MyApp.Helpers { struct Point { Int32 X; Int32 Y; }; }
            """);
        var emitter = new WinMdEmitter();
        var result = emitter.EmitPerNamespace(compilation);
        Assert.Single(result); // Both share MyApp prefix
        Assert.True(result.ContainsKey("MyApp"));
    }

    [Fact]
    public void EmitPerNamespace_DisjointNamespaces_MultipleFiles()
    {
        var compilation = CompilationHelper.Compile("""
            namespace Foo { enum Color { Red, Green } }
            namespace Bar { struct Size { Int32 W; Int32 H; }; }
            """);
        var emitter = new WinMdEmitter();
        var result = emitter.EmitPerNamespace(compilation);
        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("Foo"));
        Assert.True(result.ContainsKey("Bar"));

        // Each file should contain valid WinMD bytes
        foreach (var bytes in result.Values)
        {
            using var pe = new PEReader(new MemoryStream(bytes));
            var reader = pe.GetMetadataReader();
            Assert.True(reader.TypeDefinitions.Count > 0);
        }
    }

    [Fact]
    public void GetCommonNamespacePrefix_Tests()
    {
        Assert.Equal("MyApp", WinMdEmitter.GetCommonNamespacePrefix(new[] { "MyApp.Widgets", "MyApp.Helpers" }));
        Assert.Equal("MyApp.Widgets", WinMdEmitter.GetCommonNamespacePrefix(new[] { "MyApp.Widgets.Button", "MyApp.Widgets.Slider" }));
        Assert.Equal("", WinMdEmitter.GetCommonNamespacePrefix(new[] { "Foo", "Bar" }));
        Assert.Equal("Single", WinMdEmitter.GetCommonNamespacePrefix(new[] { "Single" }));
        Assert.Equal("", WinMdEmitter.GetCommonNamespacePrefix(Array.Empty<string>()));
    }

    [Fact]
    public void EmitPerNamespace_ValidWinMDContent()
    {
        var compilation = CompilationHelper.Compile("""
            namespace MyApp.Widgets { runtimeclass Button { Button(); } }
            """);
        var emitter = new WinMdEmitter();
        var result = emitter.EmitPerNamespace(compilation);

        Assert.Single(result);
        Assert.True(result.ContainsKey("MyApp.Widgets"));
        var bytes = result["MyApp.Widgets"];

        using var pe = new PEReader(new MemoryStream(bytes));
        var reader = pe.GetMetadataReader();

        // Assembly name should reflect the root namespace
        var assembly = reader.GetAssemblyDefinition();
        var assemblyName = reader.GetString(assembly.Name);
        Assert.Equal("MyApp.Widgets", assemblyName);
    }

    [Fact]
    public void ImportedTypesNotEmittedToWinMd()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"midl_import_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "Imported.idl"),
                "namespace Lib { enum Status { Idle, Active }; }");
            File.WriteAllText(Path.Combine(tempDir, "Main.idl"),
                "import \"Imported.idl\";\nnamespace App { runtimeclass Widget { Widget(); Lib.Status GetStatus(); } }");

            var source = File.ReadAllText(Path.Combine(tempDir, "Main.idl"));
            var compilation = CompilationHelper.Compile(source, referencePaths: null, CompilationMode.Strict,
                new[] { tempDir }, Path.Combine(tempDir, "Main.idl"));
            Assert.False(compilation.Diagnostics.Any(d => d.Severity == Midl.Analysis.Diagnostics.DiagnosticSeverity.Error),
                string.Join("; ", compilation.Diagnostics));

            // Main types should exist and not be external
            var widget = compilation.GetAllTypes().FirstOrDefault(t => t.Name == "Widget");
            Assert.NotNull(widget);
            Assert.False(widget!.IsExternal);

            // Imported types should be marked external
            var status = compilation.GetAllTypes().FirstOrDefault(t => t.Name == "Status");
            Assert.NotNull(status);
            Assert.True(status!.IsExternal, "Imported type 'Status' should be marked IsExternal");

            // WinMD should only contain main file types, not imported types
            var emitter = new WinMdEmitter();
            var bytes = emitter.EmitToBytes(compilation);
            var reader = CreateReader(bytes, out var pe);
            using (pe)
            {
                var typeNames = new List<string>();
                foreach (var handle in reader.TypeDefinitions)
                {
                    var td = reader.GetTypeDefinition(handle);
                    var name = reader.GetString(td.Name);
                    if (name != "<Module>")
                        typeNames.Add(name);
                }

                Assert.Contains("Widget", typeNames);
                Assert.DoesNotContain("Status", typeNames);
            }
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void EmitExperimentalAttribute_OnRuntimeClass()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [experimental, default_interface]
                runtimeclass Preview
                {
                    void DoStuff();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Preview");
            Assert.True(HasCustomAttribute(reader, handle,
                "Windows.Foundation.Metadata.ExperimentalAttribute"));
        }
    }

    [Fact]
    public void EmitExperimentalAttribute_OnInterface()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [experimental]
                interface IPreview
                {
                    void DoStuff();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "IPreview");
            Assert.True(HasCustomAttribute(reader, handle,
                "Windows.Foundation.Metadata.ExperimentalAttribute"));
        }
    }

    [Fact]
    public void EmitExperimentalAttribute_OnEnum()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [experimental]
                enum PreviewOptions
                {
                    None,
                    Option1
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "PreviewOptions");
            Assert.True(HasCustomAttribute(reader, handle,
                "Windows.Foundation.Metadata.ExperimentalAttribute"));
        }
    }

    [Fact]
    public void EmitExperimentalAttribute_OnDelegate()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [experimental]
                delegate void PreviewHandler();
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "PreviewHandler");
            Assert.True(HasCustomAttribute(reader, handle,
                "Windows.Foundation.Metadata.ExperimentalAttribute"));
        }
    }

    [Fact]
    public void EmitExperimentalAttribute_OnStruct()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [experimental]
                struct PreviewData
                {
                    Int32 Value;
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "PreviewData");
            Assert.True(HasCustomAttribute(reader, handle,
                "Windows.Foundation.Metadata.ExperimentalAttribute"));
        }
    }

    [Fact]
    public void NoExperimentalAttribute_WhenNotMarked()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [default_interface]
                runtimeclass Stable
                {
                    void DoStuff();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Stable");
            Assert.False(HasCustomAttribute(reader, handle,
                "Windows.Foundation.Metadata.ExperimentalAttribute"));
        }
    }
}
