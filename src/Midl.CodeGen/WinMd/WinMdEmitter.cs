using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Midl.Analysis;
using Midl.Analysis.Symbols;

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
    internal TypeReferenceHandle SystemGuidRef { get; private set; }

    // Pre-computed TypeDef handles
    private readonly Dictionary<string, TypeDefinitionHandle> _typeDefHandles = new();
    private readonly Dictionary<string, TypeDefinitionHandle> _synthInterfaceHandles = new();

    internal IReadOnlyDictionary<string, TypeDefinitionHandle> TypeDefHandles => _typeDefHandles;
    internal IReadOnlyDictionary<string, TypeDefinitionHandle> SynthInterfaceHandles => _synthInterfaceHandles;

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
        _fieldRow = 1;
        _methodRow = 1;
        _paramRow = 1;

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
        SystemGuidRef = AddMscorlibTypeRef(MetadataConstants.SystemNamespace, "Guid");

        // 5. <Module> type (required first row)
        _metadata.AddTypeDefinition(
            default,
            default,
            _metadata.GetOrAddString("<Module>"),
            default,
            MetadataTokens.FieldDefinitionHandle(1),
            MetadataTokens.MethodDefinitionHandle(1));

        // 6. Pre-compute TypeDef handles
        var allTypes = compilation.GetAllTypes().ToList();
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

    /// <summary>Resolve a symbol to its TypeDef handle.</summary>
    internal EntityHandle ResolveSymbolHandle(ISymbol symbol)
    {
        if (_typeDefHandles.TryGetValue(symbol.FullName, out var handle))
            return handle;
        return default;
    }

    private TypeReferenceHandle AddMscorlibTypeRef(string ns, string name)
    {
        return _metadata.AddTypeReference(
            MscorlibRef,
            _metadata.GetOrAddString(ns),
            _metadata.GetOrAddString(name));
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
