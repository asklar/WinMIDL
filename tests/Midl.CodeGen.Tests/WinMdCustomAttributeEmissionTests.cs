using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;
using Midl.CodeGen.WinMd;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests that custom attributes NOT handled by explicit well-known
/// attribute calls are still emitted to the WinMD output.
/// </summary>
public class WinMdCustomAttributeEmissionTests
{
    private static byte[] CompileAndEmit(string source)
    {
        var compilation = CompilationHelper.Compile(source);
        var errors = compilation.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.True(errors.Count == 0, $"Compilation errors: {string.Join(", ", errors.Select(e => e.Message))}");
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
    public void Emit_ContentPropertyAttribute_InWinMd()
    {
        var bytes = CompileAndEmit("""
            namespace N
            {
                [contentproperty("Content")]
                runtimeclass Widget
                {
                    Widget();
                    Object Content;
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "Widget");
            Assert.True(HasCustomAttribute(reader, classHandle,
                "Windows.Foundation.Metadata.ContentPropertyAttribute"),
                "ContentPropertyAttribute should be emitted on Widget");
        }
    }

    [Fact]
    public void Emit_WebHostHiddenAttribute_InWinMd()
    {
        var bytes = CompileAndEmit("""
            namespace N
            {
                [webhosthidden]
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "Widget");
            Assert.True(HasCustomAttribute(reader, classHandle,
                "Windows.Foundation.Metadata.WebHostHiddenAttribute"),
                "WebHostHiddenAttribute should be emitted on Widget");
        }
    }

    [Fact]
    public void Emit_WebHostHiddenAttribute_OnInterface()
    {
        var bytes = CompileAndEmit("""
            namespace N
            {
                [webhosthidden]
                interface IWidget
                {
                    void DoWork();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var ifaceHandle = FindTypeDef(reader, "IWidget");
            Assert.True(HasCustomAttribute(reader, ifaceHandle,
                "Windows.Foundation.Metadata.WebHostHiddenAttribute"),
                "WebHostHiddenAttribute should be emitted on interface");
        }
    }

    [Fact]
    public void Emit_ContentPropertyAttribute_HasStringArgument()
    {
        var bytes = CompileAndEmit("""
            namespace N
            {
                [contentproperty("MyContent")]
                runtimeclass Widget
                {
                    Widget();
                    Object MyContent;
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "Widget");

            // Find the ContentPropertyAttribute and verify its blob contains the string arg
            foreach (var attrHandle in reader.GetCustomAttributes(classHandle))
            {
                var attr = reader.GetCustomAttribute(attrHandle);
                if (attr.Constructor.Kind != HandleKind.MemberReference) continue;

                var memberRef = reader.GetMemberReference((MemberReferenceHandle)attr.Constructor);
                if (memberRef.Parent.Kind != HandleKind.TypeReference) continue;

                var typeRef = reader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
                var name = reader.GetString(typeRef.Name);
                if (name != "ContentPropertyAttribute") continue;

                // Read the blob value — the string "MyContent" should be in there
                var blobReader = reader.GetBlobReader(attr.Value);
                var prolog = blobReader.ReadUInt16(); // 0x0001
                Assert.Equal(0x0001, prolog);

                var strValue = blobReader.ReadSerializedString();
                Assert.Equal("MyContent", strValue);
                return;
            }

            Assert.Fail("ContentPropertyAttribute not found on Widget");
        }
    }

    [Fact]
    public void Emit_AllowForWebAttribute_InWinMd()
    {
        var bytes = CompileAndEmit("""
            namespace N
            {
                [allowforweb]
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "Widget");
            Assert.True(HasCustomAttribute(reader, classHandle,
                "Windows.Foundation.Metadata.AllowForWebAttribute"),
                "AllowForWebAttribute should be emitted on Widget");
        }
    }

    [Fact]
    public void WellKnown_Attributes_Not_Duplicated()
    {
        // Attributes that ARE handled by explicit calls (e.g., version, threading)
        // should NOT be emitted twice
        var bytes = CompileAndEmit("""
            namespace N
            {
                [threading(both)]
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "Widget");
            int threadingCount = 0;
            foreach (var attrHandle in reader.GetCustomAttributes(classHandle))
            {
                var attr = reader.GetCustomAttribute(attrHandle);
                if (attr.Constructor.Kind != HandleKind.MemberReference) continue;
                var memberRef = reader.GetMemberReference((MemberReferenceHandle)attr.Constructor);
                if (memberRef.Parent.Kind != HandleKind.TypeReference) continue;
                var typeRef = reader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
                if (reader.GetString(typeRef.Name) == "ThreadingAttribute")
                    threadingCount++;
            }
            Assert.Equal(1, threadingCount);
        }
    }
}
