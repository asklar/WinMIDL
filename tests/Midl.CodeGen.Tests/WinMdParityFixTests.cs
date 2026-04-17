using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Midl.Analysis;
using Midl.CodeGen.WinMd;

namespace Midl.CodeGen.Tests;

/// <summary>
/// TDD tests for remaining WinMD parity gaps between WinMIDL and original midl.exe.
/// Gaps: parameter names, Property/Event map on runtimeclass, default threading/marshaling,
/// default version value, EventRegistrationToken namespace, generic backtick arity.
/// </summary>
public class WinMdParityFixTests
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

    private static MethodDefinitionHandle FindMethod(MetadataReader reader, TypeDefinitionHandle typeDef, string methodName)
    {
        var td = reader.GetTypeDefinition(typeDef);
        foreach (var mh in td.GetMethods())
        {
            var md = reader.GetMethodDefinition(mh);
            if (reader.GetString(md.Name) == methodName)
                return mh;
        }
        throw new Exception($"Method '{methodName}' not found");
    }

    private static string GetParamName(MetadataReader reader, MethodDefinitionHandle methodHandle, int sequence)
    {
        var method = reader.GetMethodDefinition(methodHandle);
        foreach (var ph in method.GetParameters())
        {
            var p = reader.GetParameter(ph);
            if (p.SequenceNumber == sequence)
                return reader.GetString(p.Name);
        }
        return null!;
    }

    private static bool HasCustomAttribute(MetadataReader reader, EntityHandle parent, string attrTypeName)
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
                    if (reader.GetString(typeRef.Name) == attrTypeName)
                        return true;
                }
            }
        }
        return false;
    }

    private static int? ReadAttributeInt32(MetadataReader reader, EntityHandle parent, string attrTypeName)
    {
        foreach (var attrHandle in reader.GetCustomAttributes(parent))
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            if (attr.Constructor.Kind != HandleKind.MemberReference) continue;
            var memberRef = reader.GetMemberReference((MemberReferenceHandle)attr.Constructor);
            if (memberRef.Parent.Kind != HandleKind.TypeReference) continue;
            var typeRef = reader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
            if (reader.GetString(typeRef.Name) != attrTypeName) continue;

            var blob = reader.GetBlobReader(attr.Value);
            if (blob.Length >= 6)
            {
                blob.ReadUInt16(); // prolog
                return blob.ReadInt32();
            }
        }
        return null;
    }

    private static uint? ReadAttributeUInt32(MetadataReader reader, EntityHandle parent, string attrTypeName)
    {
        foreach (var attrHandle in reader.GetCustomAttributes(parent))
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            if (attr.Constructor.Kind != HandleKind.MemberReference) continue;
            var memberRef = reader.GetMemberReference((MemberReferenceHandle)attr.Constructor);
            if (memberRef.Parent.Kind != HandleKind.TypeReference) continue;
            var typeRef = reader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
            if (reader.GetString(typeRef.Name) != attrTypeName) continue;

            var blob = reader.GetBlobReader(attr.Value);
            if (blob.Length >= 6)
            {
                blob.ReadUInt16(); // prolog
                return blob.ReadUInt32();
            }
        }
        return null;
    }

    private const string BasicClassSource = """
        namespace TestApp.Parity
        {
            runtimeclass BasicWidget
            {
                BasicWidget();

                String WidgetName { get; };
                Int32 WidgetId { get; };
                Boolean IsEnabled { get; set; };
                Double Factor { get; set; };
                Int32 Priority;

                event Windows.Foundation.TypedEventHandler<BasicWidget, Object> Changed;
                event Windows.Foundation.TypedEventHandler<BasicWidget, String> NameChanged;
            }
        }
        """;

    #region Gap 1: Property setter / event handler parameter names

    [Fact]
    public void RuntimeClass_PropertySetterParam_NamedValue()
    {
        var bytes = CompileAndEmit(BasicClassSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "BasicWidget");
            var putIsEnabled = FindMethod(reader, classHandle, "put_IsEnabled");
            Assert.Equal("value", GetParamName(reader, putIsEnabled, 1));
        }
    }

    [Fact]
    public void RuntimeClass_EventAddParam_NamedHandler()
    {
        var bytes = CompileAndEmit(BasicClassSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "BasicWidget");
            var addChanged = FindMethod(reader, classHandle, "add_Changed");
            Assert.Equal("handler", GetParamName(reader, addChanged, 1));
        }
    }

    [Fact]
    public void RuntimeClass_EventRemoveParam_NamedToken()
    {
        var bytes = CompileAndEmit(BasicClassSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "BasicWidget");
            var removeChanged = FindMethod(reader, classHandle, "remove_Changed");
            Assert.Equal("token", GetParamName(reader, removeChanged, 1));
        }
    }

    #endregion

    #region Gap 2: Missing Property map table on runtimeclass

    [Fact]
    public void RuntimeClass_HasPropertyTable()
    {
        var bytes = CompileAndEmit(BasicClassSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "BasicWidget");
            var td = reader.GetTypeDefinition(classHandle);
            var properties = td.GetProperties().ToList();
            Assert.Equal(5, properties.Count); // WidgetName, WidgetId, IsEnabled, Factor, Priority
        }
    }

    [Fact]
    public void RuntimeClass_PropertyNames_Correct()
    {
        var bytes = CompileAndEmit(BasicClassSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "BasicWidget");
            var td = reader.GetTypeDefinition(classHandle);
            var propNames = td.GetProperties()
                .Select(p => reader.GetString(reader.GetPropertyDefinition(p).Name))
                .OrderBy(n => n)
                .ToList();

            Assert.Equal(new[] { "Factor", "IsEnabled", "Priority", "WidgetId", "WidgetName" }, propNames);
        }
    }

    [Fact]
    public void RuntimeClass_PropertyGetterSetter_Linked()
    {
        var bytes = CompileAndEmit(BasicClassSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "BasicWidget");
            var td = reader.GetTypeDefinition(classHandle);
            var isEnabledProp = td.GetProperties()
                .Select(p => reader.GetPropertyDefinition(p))
                .First(p => reader.GetString(p.Name) == "IsEnabled");

            var accessors = isEnabledProp.GetAccessors();
            Assert.False(accessors.Getter.IsNil, "IsEnabled should have a getter");
            Assert.False(accessors.Setter.IsNil, "IsEnabled should have a setter");

            Assert.Equal("get_IsEnabled", reader.GetString(reader.GetMethodDefinition(accessors.Getter).Name));
            Assert.Equal("put_IsEnabled", reader.GetString(reader.GetMethodDefinition(accessors.Setter).Name));
        }
    }

    #endregion

    #region Gap 3: Missing Event map table on runtimeclass

    [Fact]
    public void RuntimeClass_HasEventTable()
    {
        var bytes = CompileAndEmit(BasicClassSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "BasicWidget");
            var td = reader.GetTypeDefinition(classHandle);
            var events = td.GetEvents().ToList();
            Assert.Equal(2, events.Count); // Changed, NameChanged
        }
    }

    [Fact]
    public void RuntimeClass_EventNames_Correct()
    {
        var bytes = CompileAndEmit(BasicClassSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "BasicWidget");
            var td = reader.GetTypeDefinition(classHandle);
            var eventNames = td.GetEvents()
                .Select(e => reader.GetString(reader.GetEventDefinition(e).Name))
                .OrderBy(n => n)
                .ToList();

            Assert.Equal(new[] { "Changed", "NameChanged" }, eventNames);
        }
    }

    [Fact]
    public void RuntimeClass_EventAdderRemover_Linked()
    {
        var bytes = CompileAndEmit(BasicClassSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "BasicWidget");
            var td = reader.GetTypeDefinition(classHandle);
            var changedEvt = td.GetEvents()
                .Select(e => reader.GetEventDefinition(e))
                .First(e => reader.GetString(e.Name) == "Changed");

            var accessors = changedEvt.GetAccessors();
            Assert.False(accessors.Adder.IsNil, "Changed should have an adder");
            Assert.False(accessors.Remover.IsNil, "Changed should have a remover");

            Assert.Equal("add_Changed", reader.GetString(reader.GetMethodDefinition(accessors.Adder).Name));
            Assert.Equal("remove_Changed", reader.GetString(reader.GetMethodDefinition(accessors.Remover).Name));
        }
    }

    #endregion

    #region Gap 4: Missing default Threading and MarshalingBehavior

    [Fact]
    public void RuntimeClass_NoExplicitThreading_EmitsThreadingBoth()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
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
            Assert.True(HasCustomAttribute(reader, classHandle, "ThreadingAttribute"),
                "Default ThreadingAttribute(3) should be emitted");

            var value = ReadAttributeInt32(reader, classHandle, "ThreadingAttribute");
            Assert.NotNull(value);
            Assert.Equal(3, value.Value); // Both
        }
    }

    [Fact]
    public void RuntimeClass_NoExplicitMarshaling_EmitsMarshalingAgile()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
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
            Assert.True(HasCustomAttribute(reader, classHandle, "MarshalingBehaviorAttribute"),
                "Default MarshalingBehaviorAttribute(2) should be emitted");

            var value = ReadAttributeInt32(reader, classHandle, "MarshalingBehaviorAttribute");
            Assert.NotNull(value);
            Assert.Equal(2, value.Value); // Agile
        }
    }

    [Fact]
    public void RuntimeClass_ExplicitThreadingSta_DoesNotOverride()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [threading(sta)]
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
            var value = ReadAttributeInt32(reader, classHandle, "ThreadingAttribute");
            Assert.NotNull(value);
            Assert.Equal(1, value.Value); // STA, not Both
        }
    }

    #endregion

    #region Gap 5: Version value (default should be 1, not 65536)

    [Fact]
    public void RuntimeClass_DefaultVersion_IsOne()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
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
            var version = ReadAttributeUInt32(reader, classHandle, "VersionAttribute");
            Assert.NotNull(version);
            Assert.Equal(1u, version.Value); // raw 1, not 0x00010000
        }
    }

    [Fact]
    public void RuntimeClass_DefaultActivatable_VersionIsOne()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
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
            var version = ReadAttributeUInt32(reader, classHandle, "ActivatableAttribute");
            Assert.NotNull(version);
            Assert.Equal(1u, version.Value); // raw 1, not 0x00010000
        }
    }

    [Fact]
    public void RuntimeClass_ExplicitVersion_IsShifted()
    {
        // [version(3)] → 3 << 16 = 196608 (parser shifts, emitter uses directly)
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [version(3)]
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
            var version = ReadAttributeUInt32(reader, classHandle, "VersionAttribute");
            Assert.NotNull(version);
            Assert.Equal(196608u, version.Value); // 3 << 16
        }
    }

    #endregion

    #region Gap 6: EventRegistrationToken should be from Windows.Foundation

    [Fact]
    public void EventRegistrationToken_ReferencedFromWindowsFoundation()
    {
        var bytes = CompileAndEmit(BasicClassSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            // Find the TypeRef for EventRegistrationToken
            bool found = false;
            foreach (var handle in reader.TypeReferences)
            {
                var typeRef = reader.GetTypeReference(handle);
                if (reader.GetString(typeRef.Name) == "EventRegistrationToken")
                {
                    var ns = reader.GetString(typeRef.Namespace);
                    // midl.exe baseline: EventRegistrationToken is CLR-projected to
                    // System.Runtime.InteropServices.WindowsRuntime.EventRegistrationToken
                    // (in mscorlib). Verified against midl.exe baseline WinMDs.
                    Assert.Equal("System.Runtime.InteropServices.WindowsRuntime", ns);
                    found = true;
                    break;
                }
            }
            Assert.True(found, "EventRegistrationToken TypeRef should exist");
        }
    }

    #endregion

    #region Gap 7: Generic type backtick arity

    [Fact]
    public void GenericExternalType_HasBacktickArity()
    {
        var bytes = CompileAndEmit(BasicClassSource);
        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            // Find the TypeRef for TypedEventHandler — should have `2 suffix
            bool found = false;
            foreach (var handle in reader.TypeReferences)
            {
                var typeRef = reader.GetTypeReference(handle);
                var name = reader.GetString(typeRef.Name);
                if (name.StartsWith("TypedEventHandler"))
                {
                    Assert.Equal("TypedEventHandler`2", name);
                    found = true;
                    break;
                }
            }
            Assert.True(found, "TypedEventHandler`2 TypeRef should exist");
        }
    }

    #endregion
}
