// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.CodeGen.WinMd;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Thorough TDD/regression coverage for real-world classic-MIDL runtimeclass
/// WinMD parity fixes:
///
///   Bug 1: first base-list entry that is an interface → Interfaces, not BaseType
///   Bug 2: external TypeRefs are CLR-projected (EventHandler → System.EventHandler, etc.)
///   Bug 3: runtimeclass members are flattened from the transitive closure of its
///          required external interfaces
///   Bug 4: parameterized constructors emit one versioned [Activatable], not a pair
///   [Default] marker is applied to an explicitly-declared default interface
///   Overload retention: signature-based dedupe preserves overloads
///   External TypeRef fallback: unresolved external types in flattened method
///          signatures encode as proper TypeRefs, not Object
/// </summary>
public class WinMdParityRealWorldTests : IDisposable
{
    private readonly List<string> _tempFiles = [];

    public void Dispose()
    {
        foreach (var f in _tempFiles)
        {
            try { File.Delete(f); } catch { }
        }
    }

    private string MakeTempWinMd(string source)
    {
        var compilation = CompilationHelper.Compile(source);
        Assert.All(compilation.Diagnostics,
            d => Assert.NotEqual(DiagnosticSeverity.Error, d.Severity));
        var bytes = new WinMdEmitter().EmitToBytes(compilation);
        var path = Path.Combine(Path.GetTempPath(), $"parity-{Guid.NewGuid():N}.winmd");
        File.WriteAllBytes(path, bytes);
        _tempFiles.Add(path);
        return path;
    }

    private static byte[] CompileAndEmit(string source, string[]? references = null)
    {
        var compilation = references is null
            ? CompilationHelper.Compile(source)
            : CompilationHelper.Compile(source, references);
        Assert.All(compilation.Diagnostics,
            d => Assert.NotEqual(DiagnosticSeverity.Error, d.Severity));
        return new WinMdEmitter().EmitToBytes(compilation);
    }

    private static MetadataReader Read(byte[] bytes, out PEReader pe)
    {
        pe = new PEReader(new MemoryStream(bytes));
        return pe.GetMetadataReader();
    }

    private static TypeDefinition FindTypeDef(MetadataReader reader, string name)
    {
        foreach (var h in reader.TypeDefinitions)
        {
            var td = reader.GetTypeDefinition(h);
            if (reader.GetString(td.Name) == name) return td;
        }
        throw new Xunit.Sdk.XunitException($"TypeDef '{name}' not found");
    }

    private static string FullName(MetadataReader reader, EntityHandle h)
    {
        if (h.IsNil) return "";
        switch (h.Kind)
        {
            case HandleKind.TypeReference:
                var tr = reader.GetTypeReference((TypeReferenceHandle)h);
                var ns = reader.GetString(tr.Namespace);
                var n = reader.GetString(tr.Name);
                return string.IsNullOrEmpty(ns) ? n : $"{ns}.{n}";
            case HandleKind.TypeDefinition:
                var td = reader.GetTypeDefinition((TypeDefinitionHandle)h);
                var tns = reader.GetString(td.Namespace);
                var tn = reader.GetString(td.Name);
                return string.IsNullOrEmpty(tns) ? tn : $"{tns}.{tn}";
            default:
                return "";
        }
    }

    // ────────── Bug 1: base-list reclassification ──────────

    [Fact]
    public void BaseList_InterfaceOnly_GoesToInterfaces()
    {
        // `runtimeclass C : IBar` → IBar is an implemented interface.
        var bytes = CompileAndEmit("""
            namespace N
            {
                interface IBar { void DoIt(); };
                runtimeclass C : IBar { };
            }
            """);
        var reader = Read(bytes, out var pe); using var _ = pe;
        var c = FindTypeDef(reader, "C");

        // Base should NOT be IBar.
        Assert.DoesNotContain("IBar", FullName(reader, c.BaseType));

        // IBar must appear as InterfaceImpl.
        var impls = c.GetInterfaceImplementations()
            .Select(h => FullName(reader, reader.GetInterfaceImplementation(h).Interface))
            .ToList();
        Assert.Contains(impls, s => s.EndsWith(".IBar"));
    }

    [Fact]
    public void BaseList_InterfaceFirstThenInterface_BothFlattenToInterfaces()
    {
        var bytes = CompileAndEmit("""
            namespace N
            {
                interface IA { void A(); };
                interface IB { void B(); };
                runtimeclass C : IA, IB { };
            }
            """);
        var reader = Read(bytes, out var pe); using var _ = pe;
        var c = FindTypeDef(reader, "C");

        Assert.DoesNotContain("IA", FullName(reader, c.BaseType));
        var impls = c.GetInterfaceImplementations()
            .Select(h => FullName(reader, reader.GetInterfaceImplementation(h).Interface))
            .ToList();
        Assert.Contains(impls, s => s.EndsWith(".IA"));
        Assert.Contains(impls, s => s.EndsWith(".IB"));
    }

    [Fact]
    public void BaseList_UnsealedRuntimeClassBase_StaysAsBaseType()
    {
        // When the first entry is a runtimeclass (not an interface), it's a real base.
        var bytes = CompileAndEmit("""
            namespace N
            {
                interface IMarker { void M(); };
                unsealed runtimeclass Base : IMarker { };
                runtimeclass Derived : Base, IMarker { };
            }
            """);
        var reader = Read(bytes, out var pe); using var _ = pe;
        var derived = FindTypeDef(reader, "Derived");

        Assert.EndsWith(".Base", FullName(reader, derived.BaseType));
    }

    // ────────── Bug 4: Activatable duplicate ──────────

    [Fact]
    public void ParameterizedConstructor_EmitsExactlyOneActivatableAttribute()
    {
        // Regression: winmidl used to emit both [Activatable(1)] and
        // [Activatable(factory, version)]. midl.exe emits only the latter.
        var bytes = CompileAndEmit("""
            namespace N
            {
                [contractversion(1)] apicontract FooContract {}
                interface IMarker { void M(); };
                [contract(N.FooContract, 1)]
                runtimeclass C : IMarker
                {
                    C(Int32 x);
                };
            }
            """);
        var reader = Read(bytes, out var pe); using var _ = pe;
        var c = FindTypeDef(reader, "C");

        int activatableCount = 0;
        foreach (var ah in c.GetCustomAttributes())
        {
            var a = reader.GetCustomAttribute(ah);
            var ctorParent = a.Constructor.Kind == HandleKind.MemberReference
                ? reader.GetMemberReference((MemberReferenceHandle)a.Constructor).Parent
                : default;
            var name = FullName(reader, ctorParent);
            if (name.EndsWith(".ActivatableAttribute")) activatableCount++;
        }

        Assert.Equal(1, activatableCount);
    }

    [Fact]
    public void DefaultConstructor_EmitsOneUnversionedActivatable()
    {
        var bytes = CompileAndEmit("""
            namespace N
            {
                interface IMarker { void M(); };
                runtimeclass C : IMarker { C(); };
            }
            """);
        var reader = Read(bytes, out var pe); using var _ = pe;
        var c = FindTypeDef(reader, "C");

        int activatableCount = 0;
        foreach (var ah in c.GetCustomAttributes())
        {
            var a = reader.GetCustomAttribute(ah);
            var parent = a.Constructor.Kind == HandleKind.MemberReference
                ? reader.GetMemberReference((MemberReferenceHandle)a.Constructor).Parent
                : default;
            if (FullName(reader, parent).EndsWith(".ActivatableAttribute")) activatableCount++;
        }

        Assert.Equal(1, activatableCount);
    }

    // ────────── [Default] marker on explicit interface ──────────

    [Fact]
    public void ExplicitDefaultInterface_GetsDefaultAttribute()
    {
        var bytes = CompileAndEmit("""
            namespace N
            {
                interface IA { void A(); };
                interface IB { void B(); };
                runtimeclass C : [default] IB, IA
                {
                    void C_method();
                };
            }
            """);
        var reader = Read(bytes, out var pe); using var _ = pe;
        var c = FindTypeDef(reader, "C");

        // Find InterfaceImpl for IB and check it has [Default]
        InterfaceImplementation? ibImpl = null;
        foreach (var ih in c.GetInterfaceImplementations())
        {
            var impl = reader.GetInterfaceImplementation(ih);
            if (FullName(reader, impl.Interface).EndsWith(".IB")) { ibImpl = impl; break; }
        }
        Assert.NotNull(ibImpl);

        // CustomAttributes on an InterfaceImpl aren't directly enumerable; check
        // via GetCustomAttributes(EntityHandle).
        // Simplest: scan all attributes, find ones whose Parent == this InterfaceImpl,
        // and look for DefaultAttribute.
        bool hasDefault = false;
        foreach (var ah in reader.CustomAttributes)
        {
            var a = reader.GetCustomAttribute(ah);
            if (a.Parent.Kind != HandleKind.InterfaceImplementation) continue;
            var parentImpl = reader.GetInterfaceImplementation((InterfaceImplementationHandle)a.Parent);
            if (!FullName(reader, parentImpl.Interface).EndsWith(".IB")) continue;
            var ctorParent = a.Constructor.Kind == HandleKind.MemberReference
                ? reader.GetMemberReference((MemberReferenceHandle)a.Constructor).Parent
                : default;
            if (FullName(reader, ctorParent).EndsWith(".DefaultAttribute"))
            {
                hasDefault = true;
                break;
            }
        }
        Assert.True(hasDefault, "Explicit [default] interface should receive DefaultAttribute");
    }

    // ────────── Overload retention ──────────

    [Fact]
    public void DirectOverloads_DifferentArities_AllPreserved()
    {
        var bytes = CompileAndEmit("""
            namespace N
            {
                runtimeclass C
                {
                    void Do();
                    void Do(String s);
                    void Do(String s, Int32 i);
                };
            }
            """);
        var reader = Read(bytes, out var pe); using var _ = pe;
        var c = FindTypeDef(reader, "C");
        int n = c.GetMethods().Select(h => reader.GetString(reader.GetMethodDefinition(h).Name))
            .Count(s => s == "Do");
        Assert.Equal(3, n);
    }

    // ────────── Bug 3 + external TypeRef resolution (via temp-winmd round-trip) ──────────

    [Fact]
    public void FlattensExternalInterfaceMembers_OntoRuntimeClass()
    {
        // Compile one IDL to a .winmd, then reference it from a second compilation.
        // The second compilation's runtimeclass should have the referenced interface's
        // members "flattened" (emitted as stubs on the class's MethodList).
        var refWinMd = MakeTempWinMd("""
            namespace Ext
            {
                interface IExt
                {
                    void Alpha();
                    String Beta(Int32 n);
                };
            }
            """);

        var bytes = CompileAndEmit("""
            namespace N
            {
                runtimeclass C : Ext.IExt
                {
                    void Local();
                };
            }
            """, [refWinMd]);

        var reader = Read(bytes, out var pe); using var _ = pe;
        var c = FindTypeDef(reader, "C");
        var methodNames = c.GetMethods()
            .Select(h => reader.GetString(reader.GetMethodDefinition(h).Name))
            .ToList();

        Assert.Contains("Local", methodNames);      // direct member
        Assert.Contains("Alpha", methodNames);      // flattened from external IExt
        Assert.Contains("Beta", methodNames);       // flattened from external IExt
    }

    [Fact]
    public void FlattensTransitiveRequiredInterfaces()
    {
        var refWinMd = MakeTempWinMd("""
            namespace Ext
            {
                interface IBase { void BaseMethod(); };
                [uuid(11111111-2222-3333-4444-555555555555)]
                interface IChild requires IBase { void ChildMethod(); };
            }
            """);

        var bytes = CompileAndEmit("""
            namespace N
            {
                runtimeclass C : Ext.IChild { };
            }
            """, [refWinMd]);

        var reader = Read(bytes, out var pe); using var _ = pe;
        var c = FindTypeDef(reader, "C");
        var names = c.GetMethods()
            .Select(h => reader.GetString(reader.GetMethodDefinition(h).Name))
            .ToList();

        Assert.Contains("ChildMethod", names);   // from IChild directly
        Assert.Contains("BaseMethod", names);    // via IChild requires IBase
    }

    [Fact]
    public void FlattenedExternalMembers_EncodeRealParamTypes_NotObject()
    {
        // When we flatten an external interface's methods onto the class, the param
        // and return types must encode as proper TypeRefs — not fall through to Object.
        var refWinMd = MakeTempWinMd("""
            namespace Ext
            {
                enum Color { Red, Green, Blue };
                interface IPainter
                {
                    Ext.Color GetColor();
                    void SetName(String name);
                };
            }
            """);

        var bytes = CompileAndEmit("""
            namespace N
            {
                runtimeclass Canvas : Ext.IPainter { };
            }
            """, [refWinMd]);

        var reader = Read(bytes, out var pe); using var _ = pe;
        var canvas = FindTypeDef(reader, "Canvas");

        var getColor = canvas.GetMethods()
            .Select(h => reader.GetMethodDefinition(h))
            .Single(m => reader.GetString(m.Name) == "GetColor");
        var setName = canvas.GetMethods()
            .Select(h => reader.GetMethodDefinition(h))
            .Single(m => reader.GetString(m.Name) == "SetName");

        var sigGet = getColor.DecodeSignature(new FqSignatureProvider(reader), null);
        var sigSet = setName.DecodeSignature(new FqSignatureProvider(reader), null);

        // Return type of GetColor should be Ext.Color — NOT Object.
        Assert.Equal("Ext.Color", sigGet.ReturnType);
        Assert.NotEqual("Object", sigGet.ReturnType);

        // Parameter of SetName should be String — not Object.
        Assert.Single(sigSet.ParameterTypes);
        Assert.Equal("String", sigSet.ParameterTypes[0]);
    }

    // ────────── CLR projection of external TypeRefs ──────────

    [Fact]
    public void EventRegistrationToken_IsStoredAsWindowsFoundationRawType()
    {
        // Build a class that uses an event — this causes EventRegistrationToken to
        // appear in add_/remove_ accessor signatures.
        var bytes = CompileAndEmit("""
            namespace N
            {
                delegate void MyHandler(Int32 x);
                runtimeclass C
                {
                    event MyHandler Changed;
                };
            }
            """);
        // Read raw (no WinRT projection): cppwinrt/mdmerge consume the raw metadata, which
        // must hold Windows.Foundation.EventRegistrationToken. Reading with projection on
        // would rewrite it to the mscorlib name and hide a regression.
        using var pe = new PEReader(ImmutableArray.Create(bytes));
        var reader = pe.GetMetadataReader(MetadataReaderOptions.None);

        bool found = false;
        foreach (var h in reader.TypeReferences)
        {
            var tr = reader.GetTypeReference(h);
            if (reader.GetString(tr.Name) != "EventRegistrationToken") continue;
            found = true;
            Assert.Equal("Windows.Foundation", reader.GetString(tr.Namespace));
        }
        Assert.True(found, "EventRegistrationToken TypeRef should exist in metadata");
    }

    [Theory]
    [InlineData("Windows.Foundation.DateTime", "Windows.Foundation.DateTime")]
    [InlineData("Windows.Foundation.TimeSpan", "Windows.Foundation.TimeSpan")]
    [InlineData("Windows.Foundation.Collections.IVector<Int32>", "Windows.Foundation.Collections.IVector`1")]
    [InlineData("Windows.Foundation.Collections.IVectorView<Int32>", "Windows.Foundation.Collections.IVectorView`1")]
    [InlineData("Windows.Foundation.Collections.IMap<String, Int32>", "Windows.Foundation.Collections.IMap`2")]
    [InlineData("Windows.Foundation.Collections.IMapView<String, Int32>", "Windows.Foundation.Collections.IMapView`2")]
    public void ProjectedTypes_AreStoredRawNotClrProjected(string idlType, string expectedRawFullName)
    {
        // Projected WinRT types (DateTime, IVector, IMap, ...) must be stored raw; the CLR
        // projection is a read-time transform, never stored. Read raw so a regression that
        // stores the projected form is not masked.
        var bytes = CompileAndEmit($$"""
            namespace N
            {
                runtimeclass C
                {
                    {{idlType}} Get();
                };
            }
            """);
        using var pe = new PEReader(ImmutableArray.Create(bytes));
        var reader = pe.GetMetadataReader(MetadataReaderOptions.None);

        var rawNames = reader.TypeReferences
            .Select(reader.GetTypeReference)
            .Select(tr =>
            {
                var ns = reader.GetString(tr.Namespace);
                var nm = reader.GetString(tr.Name);
                return string.IsNullOrEmpty(ns) ? nm : ns + "." + nm;
            })
            .ToList();

        Assert.Contains(expectedRawFullName, rawNames);
        // The CLR-projected forms must NOT appear in the raw metadata.
        Assert.DoesNotContain("System.DateTimeOffset", rawNames);
        Assert.DoesNotContain("System.Collections.Generic.IList`1", rawNames);
        Assert.DoesNotContain("System.Collections.Generic.IReadOnlyList`1", rawNames);
        Assert.DoesNotContain("System.Collections.Generic.IDictionary`2", rawNames);
        Assert.DoesNotContain("System.Collections.Generic.IReadOnlyDictionary`2", rawNames);
    }

    // ────────── Helpers ──────────

    private sealed class FqSignatureProvider : ISignatureTypeProvider<string, object?>
    {
        private readonly MetadataReader _reader;
        public FqSignatureProvider(MetadataReader r) => _reader = r;

        public string GetPrimitiveType(PrimitiveTypeCode code) => code switch
        {
            PrimitiveTypeCode.Void => "Void",
            PrimitiveTypeCode.Boolean => "Boolean",
            PrimitiveTypeCode.Char => "Char",
            PrimitiveTypeCode.SByte => "Int8",
            PrimitiveTypeCode.Byte => "UInt8",
            PrimitiveTypeCode.Int16 => "Int16",
            PrimitiveTypeCode.UInt16 => "UInt16",
            PrimitiveTypeCode.Int32 => "Int32",
            PrimitiveTypeCode.UInt32 => "UInt32",
            PrimitiveTypeCode.Int64 => "Int64",
            PrimitiveTypeCode.UInt64 => "UInt64",
            PrimitiveTypeCode.Single => "Single",
            PrimitiveTypeCode.Double => "Double",
            PrimitiveTypeCode.String => "String",
            PrimitiveTypeCode.Object => "Object",
            _ => code.ToString(),
        };
        public string GetTypeFromDefinition(MetadataReader r, TypeDefinitionHandle h, byte _)
        {
            var td = r.GetTypeDefinition(h);
            var ns = r.GetString(td.Namespace);
            var n = r.GetString(td.Name);
            return string.IsNullOrEmpty(ns) ? n : $"{ns}.{n}";
        }
        public string GetTypeFromReference(MetadataReader r, TypeReferenceHandle h, byte _)
        {
            var tr = r.GetTypeReference(h);
            var ns = r.GetString(tr.Namespace);
            var n = r.GetString(tr.Name);
            return string.IsNullOrEmpty(ns) ? n : $"{ns}.{n}";
        }
        public string GetSZArrayType(string e) => $"{e}[]";
        public string GetArrayType(string e, ArrayShape s) => $"{e}[]";
        public string GetGenericInstantiation(string g, ImmutableArray<string> a)
            => $"{g}<{string.Join(",", a)}>";
        public string GetByReferenceType(string e) => e + "&";
        public string GetPointerType(string e) => e + "*";
        public string GetPinnedType(string e) => e;
        public string GetModifiedType(string m, string u, bool r) => u;
        public string GetFunctionPointerType(MethodSignature<string> s) => "fnptr";
        public string GetGenericMethodParameter(object? c, int i) => $"!!{i}";
        public string GetGenericTypeParameter(object? c, int i) => $"!{i}";
        public string GetTypeFromSpecification(MetadataReader r, object? c, TypeSpecificationHandle h, byte _)
            => r.GetTypeSpecification(h).DecodeSignature(this, c);
    }

    // ────────── HRESULT return type in /winrt compat mode ──────────

    [Fact]
    public void HResult_ReturnType_Encodes_As_WindowsFoundationHResult_Projected()
    {
        // In /winrt compat mode, methods may declare HRESULT return types. These must
        // be encoded in WinMD as Windows.Foundation.HResult (value struct), which the
        // CLR projection in System.Reflection.Metadata projects to System.Exception
        // when the WinMD is read. This matches midl.exe output byte-for-byte
        // semantically (both produce System.Exception after projection applied).
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [uuid("00000000-0000-0000-0000-000000000001")]
                interface IFoo
                {
                    HRESULT DoIt();
                    HRESULT DoItWith(Int32 x);
                };
            }
            """, referencePaths: null, mode: CompilationMode.Compat);
        Assert.All(compilation.Diagnostics,
            d => Assert.NotEqual(DiagnosticSeverity.Error, d.Severity));
        var bytes = new WinMdEmitter().EmitToBytes(compilation);
        var reader = Read(bytes, out var pe); using var _ = pe;

        var iface = FindTypeDef(reader, "IFoo");
        var methods = iface.GetMethods().Select(h => reader.GetMethodDefinition(h)).ToList();
        Assert.Equal(2, methods.Count);

        foreach (var m in methods)
        {
            var sig = m.DecodeSignature(new FqSignatureProvider(reader), null);
            // After CLR projection, Windows.Foundation.HResult → System.Exception.
            Assert.Equal("System.Exception", sig.ReturnType);
        }

        // And the raw TypeRef must NOT be a bare "HRESULT" name — the whole point of
        // the fix is that we emit a proper external TypeRef (projected to Exception).
        foreach (var trh in reader.TypeReferences)
        {
            var tr = reader.GetTypeReference(trh);
            var n = reader.GetString(tr.Name);
            Assert.NotEqual("HRESULT", n);
        }
    }

    [Fact]
    public void HResult_In_NonCompat_Is_Undefined()
    {
        // Sanity check: in strict mode HRESULT is not a recognized type alias.
        var comp = CompilationHelper.Compile("""
            namespace N
            {
                [uuid("00000000-0000-0000-0000-000000000002")]
                interface IFoo { HRESULT DoIt(); };
            }
            """);
        Assert.Contains(comp.Diagnostics,
            d => d.Severity == DiagnosticSeverity.Error);
    }
}
