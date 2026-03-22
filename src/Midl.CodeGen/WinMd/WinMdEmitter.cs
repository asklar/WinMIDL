using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
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

#if WINDOWS_INTERNAL
    internal IReadOnlyDictionary<string, FeatureNameInfo>? FeatureNames { get; private set; }
#endif

    /// <summary>Resolve a synthesized interface handle, creating a local TypeRef for WinMD compliance.</summary>
    internal EntityHandle ResolveSynthInterfaceHandle(string rcFullName)
    {
        if (_synthInterfaceHandles.TryGetValue(rcFullName, out var handle))
        {
            // Derive synth interface name: "Ns.MyClass" → "Ns.IMyClass"
            var lastDot = rcFullName.LastIndexOf('.');
            var synthFullName = lastDot >= 0
                ? $"{rcFullName[..lastDot]}.I{rcFullName[(lastDot + 1)..]}"
                : $"I{rcFullName}";
            return GetTypeRefForTypeDef(synthFullName, handle);
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
        var bytes = EmitToBytes(compilation);
        File.WriteAllBytes(outputPath, bytes);
    }

    /// <summary>Write WinMD to a stream.</summary>
    public void Emit(Compilation compilation, Stream stream)
    {
        var bytes = EmitToBytes(compilation);
        stream.Write(bytes);
    }

    /// <summary>Emit and return the raw WinMD PE bytes.</summary>
    public byte[] EmitToBytes(Compilation compilation)
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
#if WINDOWS_INTERNAL
        FeatureNames = compilation.FeatureNames;
#endif

        // 1. Module
        _metadata.AddModule(
            0,
            _metadata.GetOrAddString("<Module>"),
            _metadata.GetOrAddGuid(Guid.NewGuid()),
            default, default);

        // 2. Assembly (name = root namespace, WindowsRuntime flag)
        var rootNs = GetRootNamespace(compilation);
        _metadata.AddAssembly(
            _metadata.GetOrAddString(rootNs),
            new Version(1, 0, 0, 0),
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

        // EventRegistrationToken is in Windows.Foundation, referenced via a Windows assembly
        var windowsRef = _metadata.AddAssemblyReference(
            _metadata.GetOrAddString("Windows"),
            new Version(255, 255, 255, 255),
            default, default, default, default);
        _externalAssemblyRefs["Windows"] = windowsRef;
        EventRegistrationTokenRef = _metadata.AddTypeReference(
            windowsRef,
            _metadata.GetOrAddString("Windows.Foundation"),
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

        foreach (var type in allTypes)
        {
            // Synthesized default interfaces come before their runtime class
            if (type is RuntimeClassSymbol rc && TypeDefEmitter.HasDirectMembers(rc))
            {
                _synthInterfaceHandles[rc.FullName] = MetadataTokens.TypeDefinitionHandle(typeRow++);
            }
            _typeDefHandles[type.FullName] = MetadataTokens.TypeDefinitionHandle(typeRow++);
        }

        // 7. Emit types
        var customAttrEmitter = new CustomAttributeEmitter(this);
        var typeDefEmitter = new TypeDefEmitter(this, customAttrEmitter);

        foreach (var type in allTypes)
        {
            // Emit synthesized interface first
            if (type is RuntimeClassSymbol rc && TypeDefEmitter.HasDirectMembers(rc))
            {
                typeDefEmitter.EmitSynthesizedInterface(rc);
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
            return ResolveSymbolHandle(typeRef.ResolvedSymbol);
        return default;
    }

    /// <summary>Resolve a symbol to its TypeDef handle, wrapped as a local TypeRef for WinMD compliance.</summary>
    internal EntityHandle ResolveSymbolHandle(ISymbol symbol)
    {
        if (_typeDefHandles.TryGetValue(symbol.FullName, out var handle))
            return GetTypeRefForTypeDef(symbol.FullName, handle);

        // External/imported types are not emitted as TypeDefs; create a TypeRef
        // pointing to an AssemblyRef for the type's root namespace.
        if (symbol.IsExternal)
            return GetExternalTypeRef(symbol.FullName);

        return default;
    }

    /// <summary>
    /// Creates a TypeRef pointing to an external assembly for an imported type.
    /// WinRT convention: the assembly name is the root namespace of the type.
    /// </summary>
    private TypeReferenceHandle GetExternalTypeRef(string fullName)
    {
        if (_localTypeRefs.TryGetValue(fullName, out var existing))
            return existing;

        var lastDot = fullName.LastIndexOf('.');
        var ns = lastDot >= 0 ? fullName[..lastDot] : "";
        var name = lastDot >= 0 ? fullName[(lastDot + 1)..] : fullName;

        var rootNs = ns.Split('.')[0];
        if (string.IsNullOrEmpty(rootNs)) rootNs = name;

        if (!_externalAssemblyRefs.TryGetValue(rootNs, out var assemblyRef))
        {
            assemblyRef = _metadata.AddAssemblyReference(
                _metadata.GetOrAddString(rootNs),
                new Version(255, 255, 255, 255),
                default, default, default, default);
            _externalAssemblyRefs[rootNs] = assemblyRef;
        }

        var typeRef = _metadata.AddTypeReference(
            assemblyRef,
            _metadata.GetOrAddString(ns),
            _metadata.GetOrAddString(name));
        _localTypeRefs[fullName] = typeRef;
        return typeRef;
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
#if WINDOWS_INTERNAL
        FeatureNames = compilation.FeatureNames;
#endif

        // 1. Module
        _metadata.AddModule(
            0,
            _metadata.GetOrAddString("<Module>"),
            _metadata.GetOrAddGuid(Guid.NewGuid()),
            default, default);

        // 2. Assembly — name derived from the types' common namespace
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

        _metadata.AddAssembly(
            _metadata.GetOrAddString(rootNs),
            new Version(1, 0, 0, 0),
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

        // EventRegistrationToken is in Windows.Foundation, referenced via a Windows assembly
        var windowsRef2 = _metadata.AddAssemblyReference(
            _metadata.GetOrAddString("Windows"),
            new Version(255, 255, 255, 255),
            default, default, default, default);
        _externalAssemblyRefs["Windows"] = windowsRef2;
        EventRegistrationTokenRef = _metadata.AddTypeReference(
            windowsRef2,
            _metadata.GetOrAddString("Windows.Foundation"),
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
        int typeRow = 2;
        foreach (var type in types)
        {
            if (type is RuntimeClassSymbol rc && TypeDefEmitter.HasDirectMembers(rc))
            {
                _synthInterfaceHandles[rc.FullName] = MetadataTokens.TypeDefinitionHandle(typeRow++);
            }
            _typeDefHandles[type.FullName] = MetadataTokens.TypeDefinitionHandle(typeRow++);
        }

        // 7. Emit types
        var customAttrEmitter = new CustomAttributeEmitter(this);
        var typeDefEmitter = new TypeDefEmitter(this, customAttrEmitter);

        foreach (var type in types)
        {
            if (type is RuntimeClassSymbol rc && TypeDefEmitter.HasDirectMembers(rc))
            {
                typeDefEmitter.EmitSynthesizedInterface(rc);
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

    private byte[] SerializePE()
    {
        var metadataRootBuilder = new MetadataRootBuilder(_metadata, MetadataConstants.VersionString);

        var peHeaderBuilder = new PEHeaderBuilder(
            imageCharacteristics: Characteristics.ExecutableImage | Characteristics.Dll);

        var peBuilder = new ManagedPEBuilder(
            peHeaderBuilder,
            metadataRootBuilder,
            _ilStream);

        var blobBuilder = new BlobBuilder();
        peBuilder.Serialize(blobBuilder);
        return blobBuilder.ToArray();
    }
}
