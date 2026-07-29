// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.Syntax;

namespace Midl.CodeGen.WinMd;

/// <summary>
/// Emits a WinRT metadata (.winmd) file from a compiled MIDL Compilation.
/// </summary>
public class WinMdEmitter
{
    private MetadataBuilder _metadata = null!;
    private BlobBuilder _ilStream = null!;

    // When true, emit a reproducible .winmd: a content-derived module MVID (not
    // Guid.NewGuid()) and a fixed PE timestamp, so two builds of the same input
    // produce byte-identical output. Enabled by the /deterministic CLI switch.
    private readonly bool _deterministic;

    // Fixed PE COFF TimeDateStamp used in deterministic mode: 2000-01-01 00:00:00
    // UTC (Unix seconds 946684800 = 0x386D4380).
    private const uint DeterministicTimestamp = 0x386D4380;

    /// <summary>Create a WinMD emitter.</summary>
    /// <param name="deterministic">
    /// When true, produce reproducible output (content-derived MVID + fixed
    /// timestamp) instead of a random MVID and the current build time.
    /// </param>
    public WinMdEmitter(bool deterministic = false)
    {
        _deterministic = deterministic;
    }

    // Assembly references
    internal AssemblyReferenceHandle MscorlibRef { get; private set; }
    internal AssemblyReferenceHandle WfmRef { get; private set; }

    // Base type references (in mscorlib)
    internal TypeReferenceHandle SystemObjectRef { get; private set; }
    internal TypeReferenceHandle SystemEnumRef { get; private set; }
    internal TypeReferenceHandle SystemValueTypeRef { get; private set; }
    internal TypeReferenceHandle SystemMulticastDelegateRef { get; private set; }
    internal TypeReferenceHandle SystemAttributeRef { get; private set; }
    internal TypeReferenceHandle SystemGuidRef { get; private set; }
    internal TypeReferenceHandle EventRegistrationTokenRef { get; private set; }

    // Pre-computed TypeDef handles
    private readonly Dictionary<string, TypeDefinitionHandle> _typeDefHandles = new();
    private readonly Dictionary<string, TypeDefinitionHandle> _synthInterfaceHandles = new();

    internal IReadOnlyDictionary<string, TypeDefinitionHandle> TypeDefHandles => _typeDefHandles;
    internal IReadOnlyDictionary<string, TypeDefinitionHandle> SynthInterfaceHandles => _synthInterfaceHandles;

#if FEATURE_STAGING
    internal IReadOnlyDictionary<string, FeatureNameInfo>? FeatureNames { get; private set; }
#endif

    /// <summary>Resolve a synthesized interface handle, creating a local TypeRef for WinMD compliance.</summary>
    internal EntityHandle ResolveSynthInterfaceHandle(string ifaceFullName)
    {
        if (_synthInterfaceHandles.TryGetValue(ifaceFullName, out var handle))
        {
            return GetTypeRefForTypeDef(ifaceFullName, handle);
        }
        return default;
    }

    // Running row counters (1-based)
    private int _fieldRow = 1;
    private int _methodRow = 1;
    private int _paramRow = 1;

    internal MetadataBuilder Metadata => _metadata;

    internal FieldDefinitionHandle NextFieldHandle => MetadataTokens.FieldDefinitionHandle(_fieldRow);
    internal MethodDefinitionHandle NextMethodHandle => MetadataTokens.MethodDefinitionHandle(_methodRow);
    internal ParameterHandle NextParamHandle => MetadataTokens.ParameterHandle(_paramRow);

    internal void IncrementFieldRow() => _fieldRow++;
    internal void IncrementMethodRow() => _methodRow++;
    internal void IncrementParamRow() => _paramRow++;

    /// <summary>Write WinMD to a file on disk.</summary>
    public void Emit(Compilation compilation, string outputPath)
    {
        MidlTrace.CodeGen($"Emitting WinMD to '{outputPath}'");
        var bytes = EmitToBytes(compilation, outputPath);
        File.WriteAllBytes(outputPath, bytes);
    }

    /// <summary>Write WinMD to a stream.</summary>
    public void Emit(Compilation compilation, Stream stream)
    {
        var bytes = EmitToBytes(compilation, null);
        stream.Write(bytes);
    }

    /// <summary>Emit and return the raw WinMD PE bytes.</summary>
    public byte[] EmitToBytes(Compilation compilation, string? outputPath = null)
    {
        _metadata = new MetadataBuilder();
        _ilStream = new BlobBuilder();
        _typeDefHandles.Clear();
        _synthInterfaceHandles.Clear();
        _localTypeRefs.Clear();
        _externalAssemblyRefs.Clear();
        _fieldRow = 1;
        _methodRow = 1;
        _paramRow = 1;
#if FEATURE_STAGING
        FeatureNames = compilation.FeatureNames;
#endif

        // Derive assembly/module name from output path, fallback to root namespace
        var assemblyName = !string.IsNullOrEmpty(outputPath)
            ? Path.GetFileNameWithoutExtension(outputPath)
            : GetRootNamespace(compilation);
        var moduleName = !string.IsNullOrEmpty(outputPath)
            ? Path.GetFileName(outputPath)
            : $"{assemblyName}.winmd";

        // 1. Module
        _metadata.AddModule(
            0,
            _metadata.GetOrAddString(moduleName),
            _metadata.GetOrAddGuid(_deterministic ? ComputeDeterministicMvid(compilation) : Guid.NewGuid()),
            default, default);

        // 2. Assembly (name = output filename or root namespace, WindowsRuntime flag)
        _metadata.AddAssembly(
            _metadata.GetOrAddString(assemblyName),
            new Version(255, 255, 255, 255),
            default, default,
            (AssemblyFlags)0x0200, // WindowsRuntime
            default);

        // 3. Assembly references
        MscorlibRef = _metadata.AddAssemblyReference(
            _metadata.GetOrAddString(MetadataConstants.MscorlibAssembly),
            new Version(255, 255, 255, 255),
            default, default, default, default);

        WfmRef = _metadata.AddAssemblyReference(
            _metadata.GetOrAddString(MetadataConstants.WindowsFoundationAssembly),
            new Version(255, 255, 255, 255),
            default, default, default, default);

        // 4. TypeRefs for base types
        SystemObjectRef = AddMscorlibTypeRef(MetadataConstants.SystemNamespace, MetadataConstants.SystemObject);
        SystemEnumRef = AddMscorlibTypeRef(MetadataConstants.SystemNamespace, MetadataConstants.SystemEnum);
        SystemValueTypeRef = AddMscorlibTypeRef(MetadataConstants.SystemNamespace, MetadataConstants.SystemValueType);
        SystemMulticastDelegateRef = AddMscorlibTypeRef(MetadataConstants.SystemNamespace, MetadataConstants.SystemMulticastDelegate);
        SystemAttributeRef = AddMscorlibTypeRef(MetadataConstants.SystemNamespace, MetadataConstants.SystemAttribute);
        SystemGuidRef = AddMscorlibTypeRef(MetadataConstants.SystemNamespace, "Guid");

        // EventRegistrationToken — midl.exe emits this CLR-projected, in mscorlib.
        EventRegistrationTokenRef = _metadata.AddTypeReference(
            MscorlibRef,
            _metadata.GetOrAddString("System.Runtime.InteropServices.WindowsRuntime"),
            _metadata.GetOrAddString("EventRegistrationToken"));

        // 5. <Module> type (required first row)
        _metadata.AddTypeDefinition(
            default,
            default,
            _metadata.GetOrAddString("<Module>"),
            default,
            MetadataTokens.FieldDefinitionHandle(1),
            MetadataTokens.MethodDefinitionHandle(1));

        // 6. Pre-compute TypeDef handles (exclude imported/external types)
        var allTypes = compilation.GetAllTypes().Where(t => !t.IsExternal).ToList();
        int typeRow = 2; // row 1 is <Module>

        // Collect ALL synthesized interfaces (default, protected, overridable) to skip from allTypes
        var synthInterfaces = new HashSet<ISymbol>();
        foreach (var type in allTypes)
        {
            if (type is RuntimeClassSymbol rc)
            {
                if (rc.DefaultInterface is { IsSynthesized: true })
                    synthInterfaces.Add(rc.DefaultInterface);
                if (rc.ProtectedInterface is { IsSynthesized: true })
                    synthInterfaces.Add(rc.ProtectedInterface);
                if (rc.OverridesInterface is { IsSynthesized: true })
                    synthInterfaces.Add(rc.OverridesInterface);
            }
        }

        foreach (var type in allTypes)
        {
            // Skip synthesized interfaces (handled inline before their runtime class)
            if (synthInterfaces.Contains(type))
                continue;

            // Reserve TypeDef handles for synthesized interfaces before the runtime class
            if (type is RuntimeClassSymbol rc)
            {
                if (rc.DefaultInterface is { IsSynthesized: true })
                    _synthInterfaceHandles[rc.DefaultInterface.FullName] = MetadataTokens.TypeDefinitionHandle(typeRow++);
                if (rc.ProtectedInterface is { IsSynthesized: true } && rc.ProtectedInterface != rc.DefaultInterface)
                    _synthInterfaceHandles[rc.ProtectedInterface.FullName] = MetadataTokens.TypeDefinitionHandle(typeRow++);
                if (rc.OverridesInterface is { IsSynthesized: true } && rc.OverridesInterface != rc.DefaultInterface)
                    _synthInterfaceHandles[rc.OverridesInterface.FullName] = MetadataTokens.TypeDefinitionHandle(typeRow++);
            }
            _typeDefHandles[type.FullName] = MetadataTokens.TypeDefinitionHandle(typeRow++);
        }

        // 7. Emit types
        var customAttrEmitter = new CustomAttributeEmitter(this);
        var typeDefEmitter = new TypeDefEmitter(this, customAttrEmitter);

        foreach (var type in allTypes)
        {
            // Skip synthesized interfaces (handled inline before their runtime class)
            if (synthInterfaces.Contains(type))
                continue;

            // Emit synthesized interfaces before the runtime class
            if (type is RuntimeClassSymbol rc)
            {
                if (rc.DefaultInterface is { IsSynthesized: true })
                    typeDefEmitter.EmitSynthesizedInterface(rc, rc.DefaultInterface, rc.FullName);
                if (rc.ProtectedInterface is { IsSynthesized: true } && rc.ProtectedInterface != rc.DefaultInterface)
                    typeDefEmitter.EmitSynthesizedInterface(rc, rc.ProtectedInterface, rc.FullName);
                if (rc.OverridesInterface is { IsSynthesized: true } && rc.OverridesInterface != rc.DefaultInterface)
                    typeDefEmitter.EmitSynthesizedInterface(rc, rc.OverridesInterface, rc.FullName);
            }
            typeDefEmitter.Emit(type);
        }

        MidlTrace.CodeGen($"WinMD complete: {allTypes.Count} types");

        // 8. Serialize PE
        return SerializePE();
    }

    /// <summary>Resolve a TypeRef to a TypeDef handle (for types in this compilation).</summary>
    internal EntityHandle ResolveTypeHandle(TypeRef typeRef)
    {
        if (typeRef.ResolvedSymbol != null)
            return ResolveSymbolHandle(typeRef.ResolvedSymbol, typeRef.TypeArguments.Count);
        // Fallback: unresolved TypeRefs (e.g. parameter types copied verbatim from an
        // external interface's WinMD blob when we flatten its members) still need a
        // TypeRef row. Treat the name as an external reference.
        if (!string.IsNullOrEmpty(typeRef.Name))
            return GetExternalTypeRef(typeRef.Name, typeRef.TypeArguments.Count);
        return default;
    }

    /// <summary>Resolve an external type by its fully-qualified name.</summary>
    internal EntityHandle ResolveExternalByName(string fullName, int genericArity = 0)
        => GetExternalTypeRef(fullName, genericArity);

    /// <summary>Resolve a symbol to its TypeDef handle, wrapped as a local TypeRef for WinMD compliance.</summary>
    internal EntityHandle ResolveSymbolHandle(ISymbol symbol)
    {
        return ResolveSymbolHandle(symbol, 0);
    }

    /// <summary>Resolve a symbol to its TypeDef handle with generic arity suffix for WinMD compliance.</summary>
    internal EntityHandle ResolveSymbolHandle(ISymbol symbol, int genericArity)
    {
        var cacheKey = genericArity > 0 ? $"{symbol.FullName}`{genericArity}" : symbol.FullName;

        if (_typeDefHandles.TryGetValue(symbol.FullName, out var handle))
        {
            var typeRefName = genericArity > 0 ? $"{symbol.FullName}`{genericArity}" : symbol.FullName;
            return GetTypeRefForTypeDef(typeRefName, handle);
        }

        // External/imported types are not emitted as TypeDefs; create a TypeRef
        // pointing to an AssemblyRef for the type's root namespace.
        if (symbol.IsExternal)
            return GetExternalTypeRef(symbol.FullName, genericArity);

        return default;
    }

    /// <summary>
    /// Creates a TypeRef pointing to an external assembly for an imported type.
    /// Applies CLR projection so that WinRT names like Windows.Foundation.DateTime
    /// become System.DateTimeOffset in the emitted metadata (matching midlrt behaviour).
    /// Projected names always live in mscorlib; other Windows.* types use the
    /// Windows.Foundation assembly ref; otherwise the first namespace segment names
    /// the assembly.
    /// </summary>
    private TypeReferenceHandle GetExternalTypeRef(string fullName, int genericArity = 0)
    {
        var cacheKey = genericArity > 0 ? $"{fullName}`{genericArity}" : fullName;
        if (_localTypeRefs.TryGetValue(cacheKey, out var existing))
            return existing;

        // Apply CLR projection (e.g. Windows.Foundation.DateTime -> System.DateTimeOffset).
        // The projection map is keyed by the fully-qualified name including the generic
        // arity suffix (e.g. IVector`1 -> IList`1), so compose that form before lookup.
        var projectionKey = genericArity > 0 ? $"{fullName}`{genericArity}" : fullName;
        var projected = ClrProjectionMap.ToClrName(projectionKey);
        bool wasProjected = !ReferenceEquals(projected, projectionKey) && projected != projectionKey;

        string emittedFull;
        int emittedArity;
        if (wasProjected)
        {
            // Split off the `N suffix added by the projection map.
            var backtick = projected.IndexOf('`');
            if (backtick >= 0)
            {
                emittedFull = projected[..backtick];
                emittedArity = int.TryParse(projected[(backtick + 1)..], out var a) ? a : genericArity;
            }
            else
            {
                emittedFull = projected;
                emittedArity = 0;
            }
        }
        else
        {
            emittedFull = fullName;
            emittedArity = genericArity;
        }

        var lastDot = emittedFull.LastIndexOf('.');
        var ns = lastDot >= 0 ? emittedFull[..lastDot] : "";
        var name = lastDot >= 0 ? emittedFull[(lastDot + 1)..] : emittedFull;

        if (emittedArity > 0)
            name = $"{name}`{emittedArity}";

        AssemblyReferenceHandle assemblyRef;
        if (wasProjected)
        {
            // CLR-projected names live in mscorlib in classic WinMDs. This matches the
            // assemblies that .NET's MetadataReader expects when resolving projected types.
            assemblyRef = MscorlibRef;
        }
        else if (ns.StartsWith("Windows."))
        {
            assemblyRef = WfmRef;
        }
        else
        {
            var rootNs = ns.Split('.')[0];
            if (string.IsNullOrEmpty(rootNs)) rootNs = name;

            if (!_externalAssemblyRefs.TryGetValue(rootNs, out assemblyRef))
            {
                assemblyRef = _metadata.AddAssemblyReference(
                    _metadata.GetOrAddString(rootNs),
                    new Version(255, 255, 255, 255),
                    default, default, default, default);
                _externalAssemblyRefs[rootNs] = assemblyRef;
            }
        }

        var typeRef2 = _metadata.AddTypeReference(
            assemblyRef,
            _metadata.GetOrAddString(ns),
            _metadata.GetOrAddString(name));
        _localTypeRefs[cacheKey] = typeRef2;
        return typeRef2;
    }

    // Cache for assembly references to external namespaces
    private readonly Dictionary<string, AssemblyReferenceHandle> _externalAssemblyRefs = new();

    /// <summary>
    /// Creates a TypeRef that points to a TypeDef in the same module.
    /// System WinMDs use TypeRef for ALL type references, even same-file types.
    /// </summary>
    internal TypeReferenceHandle GetTypeRefForTypeDef(string fullName, TypeDefinitionHandle typeDef)
    {
        if (_localTypeRefs.TryGetValue(fullName, out var existing))
            return existing;

        var lastDot = fullName.LastIndexOf('.');
        var ns = lastDot >= 0 ? fullName[..lastDot] : "";
        var name = lastDot >= 0 ? fullName[(lastDot + 1)..] : fullName;

        var assemblyScope = MetadataTokens.EntityHandle(TableIndex.Module, 1);
        var typeRef = _metadata.AddTypeReference(
            assemblyScope,
            _metadata.GetOrAddString(ns),
            _metadata.GetOrAddString(name));
        _localTypeRefs[fullName] = typeRef;
        return typeRef;
    }

    // Cache for local TypeRef handles pointing to same-file TypeDefs
    private readonly Dictionary<string, TypeReferenceHandle> _localTypeRefs = new();

    private TypeReferenceHandle AddMscorlibTypeRef(string ns, string name)
    {
        return _metadata.AddTypeReference(
            MscorlibRef,
            _metadata.GetOrAddString(ns),
            _metadata.GetOrAddString(name));
    }

    /// <summary>
    /// Emit one WinMD file per root namespace.
    /// Returns a dictionary of namespace → WinMD bytes.
    /// </summary>
    public Dictionary<string, byte[]> EmitPerNamespace(Compilation compilation)
    {
        var allTypes = compilation.GetAllTypes().Where(t => !t.IsExternal).ToList();

        // Collect all distinct namespaces from types
        var namespaces = allTypes
            .Select(t => GetTypeNamespace(t))
            .Where(ns => !string.IsNullOrEmpty(ns))
            .Distinct()
            .ToList();

        if (namespaces.Count == 0)
        {
            // No namespaced types — fall back to single emission
            return new Dictionary<string, byte[]>
            {
                ["WinRT"] = EmitToBytes(compilation)
            };
        }

        var commonPrefix = GetCommonNamespacePrefix(namespaces);

        if (!string.IsNullOrEmpty(commonPrefix))
        {
            // All types share a common root — single WinMD
            return new Dictionary<string, byte[]>
            {
                [commonPrefix] = EmitForTypes(compilation, allTypes)
            };
        }

        // Multiple disjoint root namespaces — group by first segment
        var groups = namespaces.GroupBy(ns => ns.Split('.')[0]).ToList();
        var result = new Dictionary<string, byte[]>();

        foreach (var group in groups)
        {
            var rootNs = group.Key;
            var groupNamespaces = group.ToList();
            var groupPrefix = GetCommonNamespacePrefix(groupNamespaces);
            var key = !string.IsNullOrEmpty(groupPrefix) ? groupPrefix : rootNs;

            // Filter types belonging to this root namespace
            var filteredTypes = allTypes
                .Where(t => GetTypeNamespace(t).Split('.')[0] == rootNs)
                .ToList();

            result[key] = EmitForTypes(compilation, filteredTypes);
        }

        return result;
    }

    /// <summary>
    /// Compute the longest common dotted namespace prefix.
    /// </summary>
    public static string GetCommonNamespacePrefix(IEnumerable<string> namespaces)
    {
        var list = namespaces.ToList();
        if (list.Count == 0) return "";
        if (list.Count == 1) return list[0];

        var segmentArrays = list.Select(ns => ns.Split('.')).ToList();
        var minLen = segmentArrays.Min(s => s.Length);
        var commonSegments = new List<string>();

        for (int i = 0; i < minLen; i++)
        {
            var segment = segmentArrays[0][i];
            if (segmentArrays.All(s => s[i] == segment))
                commonSegments.Add(segment);
            else
                break;
        }

        return string.Join(".", commonSegments);
    }

    private static string GetTypeNamespace(ISymbol type)
    {
        var fullName = type.FullName;
        var lastDot = fullName.LastIndexOf('.');
        return lastDot >= 0 ? fullName[..lastDot] : "";
    }

    /// <summary>Emit WinMD bytes for a specific subset of types.</summary>
    private byte[] EmitForTypes(Compilation compilation, List<ISymbol> types)
    {
        _metadata = new MetadataBuilder();
        _ilStream = new BlobBuilder();
        _typeDefHandles.Clear();
        _synthInterfaceHandles.Clear();
        _localTypeRefs.Clear();
        _externalAssemblyRefs.Clear();
        _fieldRow = 1;
        _methodRow = 1;
        _paramRow = 1;
#if FEATURE_STAGING
        FeatureNames = compilation.FeatureNames;
#endif

        // Derive assembly name from the types' common namespace
        var typeNamespaces = types
            .Select(t => GetTypeNamespace(t))
            .Where(ns => !string.IsNullOrEmpty(ns))
            .Distinct()
            .ToList();
        var rootNs = typeNamespaces.Count > 0
            ? GetCommonNamespacePrefix(typeNamespaces)
            : "WinRT";
        if (string.IsNullOrEmpty(rootNs) && typeNamespaces.Count > 0)
            rootNs = typeNamespaces[0].Split('.')[0];

        // 1. Module
        _metadata.AddModule(
            0,
            _metadata.GetOrAddString($"{rootNs}.winmd"),
            _metadata.GetOrAddGuid(_deterministic ? ComputeDeterministicMvid(compilation) : Guid.NewGuid()),
            default, default);

        // 2. Assembly
        _metadata.AddAssembly(
            _metadata.GetOrAddString(rootNs),
            new Version(255, 255, 255, 255),
            default, default,
            (AssemblyFlags)0x0200, // WindowsRuntime
            default);

        // 3. Assembly references
        MscorlibRef = _metadata.AddAssemblyReference(
            _metadata.GetOrAddString(MetadataConstants.MscorlibAssembly),
            new Version(255, 255, 255, 255),
            default, default, default, default);

        WfmRef = _metadata.AddAssemblyReference(
            _metadata.GetOrAddString(MetadataConstants.WindowsFoundationAssembly),
            new Version(255, 255, 255, 255),
            default, default, default, default);

        // 4. TypeRefs for base types
        SystemObjectRef = AddMscorlibTypeRef(MetadataConstants.SystemNamespace, MetadataConstants.SystemObject);
        SystemEnumRef = AddMscorlibTypeRef(MetadataConstants.SystemNamespace, MetadataConstants.SystemEnum);
        SystemValueTypeRef = AddMscorlibTypeRef(MetadataConstants.SystemNamespace, MetadataConstants.SystemValueType);
        SystemMulticastDelegateRef = AddMscorlibTypeRef(MetadataConstants.SystemNamespace, MetadataConstants.SystemMulticastDelegate);
        SystemAttributeRef = AddMscorlibTypeRef(MetadataConstants.SystemNamespace, MetadataConstants.SystemAttribute);
        SystemGuidRef = AddMscorlibTypeRef(MetadataConstants.SystemNamespace, "Guid");

        // EventRegistrationToken — midl.exe emits this CLR-projected, in mscorlib.
        EventRegistrationTokenRef = _metadata.AddTypeReference(
            MscorlibRef,
            _metadata.GetOrAddString("System.Runtime.InteropServices.WindowsRuntime"),
            _metadata.GetOrAddString("EventRegistrationToken"));

        // 5. <Module> type (required first row)
        _metadata.AddTypeDefinition(
            default,
            default,
            _metadata.GetOrAddString("<Module>"),
            default,
            MetadataTokens.FieldDefinitionHandle(1),
            MetadataTokens.MethodDefinitionHandle(1));

        // 6. Pre-compute TypeDef handles
        var synthInterfaces2 = new HashSet<ISymbol>();
        foreach (var type in types)
        {
            if (type is RuntimeClassSymbol rc)
            {
                if (rc.DefaultInterface is { IsSynthesized: true })
                    synthInterfaces2.Add(rc.DefaultInterface);
                if (rc.ProtectedInterface is { IsSynthesized: true })
                    synthInterfaces2.Add(rc.ProtectedInterface);
                if (rc.OverridesInterface is { IsSynthesized: true })
                    synthInterfaces2.Add(rc.OverridesInterface);
            }
        }

        int typeRow = 2;
        foreach (var type in types)
        {
            if (synthInterfaces2.Contains(type))
                continue;
            if (type is RuntimeClassSymbol rc)
            {
                if (rc.DefaultInterface is { IsSynthesized: true })
                    _synthInterfaceHandles[rc.DefaultInterface.FullName] = MetadataTokens.TypeDefinitionHandle(typeRow++);
                if (rc.ProtectedInterface is { IsSynthesized: true } && rc.ProtectedInterface != rc.DefaultInterface)
                    _synthInterfaceHandles[rc.ProtectedInterface.FullName] = MetadataTokens.TypeDefinitionHandle(typeRow++);
                if (rc.OverridesInterface is { IsSynthesized: true } && rc.OverridesInterface != rc.DefaultInterface)
                    _synthInterfaceHandles[rc.OverridesInterface.FullName] = MetadataTokens.TypeDefinitionHandle(typeRow++);
            }
            _typeDefHandles[type.FullName] = MetadataTokens.TypeDefinitionHandle(typeRow++);
        }

        // 7. Emit types
        var customAttrEmitter = new CustomAttributeEmitter(this);
        var typeDefEmitter = new TypeDefEmitter(this, customAttrEmitter);

        foreach (var type in types)
        {
            if (synthInterfaces2.Contains(type))
                continue;
            if (type is RuntimeClassSymbol rc)
            {
                if (rc.DefaultInterface is { IsSynthesized: true })
                    typeDefEmitter.EmitSynthesizedInterface(rc, rc.DefaultInterface, rc.FullName);
                if (rc.ProtectedInterface is { IsSynthesized: true } && rc.ProtectedInterface != rc.DefaultInterface)
                    typeDefEmitter.EmitSynthesizedInterface(rc, rc.ProtectedInterface, rc.FullName);
                if (rc.OverridesInterface is { IsSynthesized: true } && rc.OverridesInterface != rc.DefaultInterface)
                    typeDefEmitter.EmitSynthesizedInterface(rc, rc.OverridesInterface, rc.FullName);
            }
            typeDefEmitter.Emit(type);
        }

        // 8. Serialize PE
        return SerializePE();
    }

    private static string GetRootNamespace(Compilation compilation)
    {
        var ns = compilation.GlobalNamespace;
        if (ns.ChildNamespaces.Count == 1)
            return ns.ChildNamespaces.Values.First().FullName;
        return ns.ChildNamespaces.Count > 0
            ? ns.ChildNamespaces.Values.First().FullName
            : "WinRT";
    }

    /// <summary>
    /// Computes a deterministic module MVID from the compilation's public API
    /// surface (sorted type names + interface/delegate IIDs). This makes two
    /// builds of the same input produce a byte-identical .winmd, and makes the
    /// MVID change only when the API changes -- matching the approach already used
    /// for interface IIDs (AbiGuidComputer) and the downstream
    /// mvid.exe /makeDeterministic post-processing. Only used in /deterministic mode.
    /// </summary>
    private static Guid ComputeDeterministicMvid(Compilation compilation)
    {
        var lines = new List<string>();
        foreach (var type in compilation.GetAllTypes())
        {
            if (type.IsExternal) continue;
            var iid = type switch
            {
                InterfaceSymbol iface => (iface.Uuid ?? AbiGuidComputer.ComputeGuid(iface)).ToString(),
                DelegateSymbol del => (del.Uuid ?? AbiGuidComputer.ComputeGuid(del)).ToString(),
                _ => string.Empty,
            };
            lines.Add($"{type.FullName}|{iid}");
        }
        lines.Sort(StringComparer.Ordinal);

        var input = Encoding.UTF8.GetBytes(string.Join('\n', lines));
        var hash = SHA256.HashData(input);
        // Use the first 16 bytes of the content hash as the MVID (same approach as
        // mvid.exe /makeDeterministic).
        return new Guid(hash.AsSpan(0, 16));
    }

    private byte[] SerializePE()
    {
        var metadataRootBuilder = new MetadataRootBuilder(_metadata, MetadataConstants.VersionString);

        var peHeaderBuilder = new PEHeaderBuilder(
            imageCharacteristics: Characteristics.ExecutableImage | Characteristics.Dll);

        var peBuilder = new ManagedPEBuilder(
            peHeaderBuilder,
            metadataRootBuilder,
            _ilStream,
            // In deterministic mode, pin the PE COFF TimeDateStamp to a fixed value
            // (2000-01-01) instead of the current build time. (The MVID is made
            // deterministic separately via the Module table above.)
            deterministicIdProvider: _deterministic
                ? (_ => new BlobContentId(Guid.Empty, DeterministicTimestamp))
                : null);

        var blobBuilder = new BlobBuilder();
        peBuilder.Serialize(blobBuilder);
        return blobBuilder.ToArray();
    }
}
