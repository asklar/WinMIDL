using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;
using Midl.Syntax;

namespace Midl.Analysis;

/// <summary>
/// Walks a green syntax tree and produces a bound compilation with symbols.
/// </summary>
public class Binder
{
    private readonly List<Diagnostic> _diagnostics = [];
    private readonly NamespaceSymbol _globalNamespace = new("");
    private readonly List<(TypeRef TypeRef, NamespaceSymbol ContainingNamespace)> _unresolvedRefs = [];
    private readonly HashSet<string> _definedTypes = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _definedTypesCaseInsensitive = new(StringComparer.OrdinalIgnoreCase);
    private readonly CompilationMode _mode;
    private readonly bool _emitQualityHints;
    private TokenPositionMap _posMap = null!;
    private GreenNode _root = null!;

    private SymbolTable _symbolTable = null!;

    public Binder(CompilationMode mode = CompilationMode.Strict, bool emitQualityHints = false)
    {
        _mode = mode;
        _emitQualityHints = emitQualityHints;
    }

    public Compilation Bind(GreenNode compilationUnit, IReadOnlyList<SyntaxToken> tokens,
        IReadOnlyList<ISymbol>? externalTypes = null,
        IReadOnlyList<(GreenNode Tree, IReadOnlyList<SyntaxToken> Tokens)>? importedTrees = null,
        IReadOnlyList<LazyWinMdProvider>? lazyProviders = null)
    {
        MidlTrace.Binder("Binding compilation unit");
        _symbolTable = new SymbolTable(_globalNamespace);
        _posMap = new TokenPositionMap(tokens);
        _root = compilationUnit;

        // Register built-in WinRT types before any user code
        BuiltInTypes.Register(_symbolTable, _mode);

        // Register lazy providers for on-demand WinMD type resolution
        if (lazyProviders != null)
        {
            foreach (var provider in lazyProviders)
                _symbolTable.AddLazyProvider(provider);
        }

        // Import external types before binding user code
        if (externalTypes is { Count: > 0 })
            _symbolTable.ImportExternalTypes(externalTypes);

        // Bind imported trees first (they provide types for the main file)
        if (importedTrees is { Count: > 0 })
        {
            foreach (var (importedTree, importedTokens) in importedTrees)
            {
                var savedPosMap = _posMap;
                var savedRoot = _root;
                _posMap = new TokenPositionMap(importedTokens);
                _root = importedTree;
                BindCompilationUnit(importedTree, _globalNamespace);
                _posMap = savedPosMap;
                _root = savedRoot;
            }
        }

        // Pass 1: create all symbols
        BindCompilationUnit(compilationUnit, _globalNamespace);

        // Pass 2: resolve type references
        var resolver = new TypeResolver(_symbolTable);
        resolver.ResolveAll(_unresolvedRefs, _diagnostics);

        // Pass 3: semantic validation (inheritance, access modifiers, requires)
        ValidateSemantics(_globalNamespace);

        if (MidlTrace.IsEnabled)
        {
            var types = new Compilation(_globalNamespace, _diagnostics).GetAllTypes().Count();
            MidlTrace.Binder($"Binding complete: {types} types, {_diagnostics.Count} diagnostics");
        }

        return new Compilation(_globalNamespace, _diagnostics, lazyProviders);
    }

    private void BindCompilationUnit(GreenNode node, NamespaceSymbol ns)
    {
        // Strict mode: scan all tokens for classic MIDL types
        if (_mode == CompilationMode.Strict)
        {
            CheckStrictModeTokens(node);
        }

        for (int i = 0; i < node.SlotCount; i++)
        {
            var child = node.GetSlot(i);
            if (child == null) continue;
            BindDeclaration(child, ns);
        }
    }

    private void BindDeclaration(GreenNode node, NamespaceSymbol ns)
    {
        // Strict mode: warn on classic MIDL constructs
        if (_mode == CompilationMode.Strict)
        {
            CheckStrictModeViolation(node);
        }

        // WinRT types must be in a namespace (not at global scope)
        if (ns.Name == "" && IsTypeDeclaration(node.Kind))
        {
            var typeName = ExtractTypeName(node);
            if (typeName != null)
            {
                int pos = _posMap.GetPositionOf(typeName);
                _diagnostics.Add(DiagnosticDescriptors.NamespaceRequired(typeName, pos));
            }
        }

        switch (node.Kind)
        {
            case SyntaxKind.NamespaceDeclaration:
                MidlTrace.Binder($"Binding {node.Kind} at declaration level");
                BindNamespaceDeclaration(node, ns);
                break;
            case SyntaxKind.RuntimeClassDeclaration:
                MidlTrace.Binder($"Binding {node.Kind} at declaration level");
                BindRuntimeClassDeclaration(node, ns);
                break;
            case SyntaxKind.InterfaceDeclaration:
                MidlTrace.Binder($"Binding {node.Kind} at declaration level");
                BindInterfaceDeclaration(node, ns);
                break;
            case SyntaxKind.DelegateDeclaration:
                MidlTrace.Binder($"Binding {node.Kind} at declaration level");
                BindDelegateDeclaration(node, ns);
                break;
            case SyntaxKind.StructDeclaration:
                MidlTrace.Binder($"Binding {node.Kind} at declaration level");
                BindStructDeclaration(node, ns);
                break;
            case SyntaxKind.EnumDeclaration:
                MidlTrace.Binder($"Binding {node.Kind} at declaration level");
                BindEnumDeclaration(node, ns);
                break;
            case SyntaxKind.ApiContractDeclaration:
                MidlTrace.Binder($"Binding {node.Kind} at declaration level");
                BindApiContractDeclaration(node, ns);
                break;
            case SyntaxKind.AttributeDeclaration:
                MidlTrace.Binder($"Binding {node.Kind} at declaration level");
                BindAttributeDeclaration(node, ns);
                break;
            case SyntaxKind.DeclareBlock:
                BindDeclareBlock(node, ns);
                break;
        }
    }

    #region Namespace

    private void BindNamespaceDeclaration(GreenNode node, NamespaceSymbol parentNs)
    {
        // Find name: first DottedName or IdentifierToken after NamespaceKeyword
        int keywordIdx = FindSlotIndex(node, SyntaxKind.NamespaceKeyword);
        if (keywordIdx < 0) return;

        var nameNode = node.GetSlot(keywordIdx + 1);
        var nameText = ExtractDottedName(nameNode);
        if (string.IsNullOrEmpty(nameText)) return;

        MidlTrace.Binder($"Entering namespace '{nameText}'");

        // Create nested namespace hierarchy for dotted names
        var parts = nameText.Split('.');
        var current = parentNs;
        foreach (var part in parts)
        {
            current = current.GetOrCreateChildNamespace(part);
        }

        // Bind declarations inside namespace body
        int openBraceIdx = FindSlotIndex(node, SyntaxKind.OpenBraceToken);
        int closeBraceIdx = FindLastSlotIndex(node, SyntaxKind.CloseBraceToken);
        if (openBraceIdx < 0 || closeBraceIdx < 0) return;

        for (int i = openBraceIdx + 1; i < closeBraceIdx; i++)
        {
            var child = node.GetSlot(i);
            if (child != null)
                BindDeclaration(child, current);
        }
    }

    #endregion

    #region RuntimeClass

    private void BindRuntimeClassDeclaration(GreenNode node, NamespaceSymbol ns)
    {
        int keywordIdx = FindSlotIndex(node, SyntaxKind.RuntimeclassKeyword);
        if (keywordIdx < 0) return;

        var nameToken = node.GetSlot(keywordIdx + 1) as SyntaxToken;
        var name = nameToken?.Text ?? "";
        if (string.IsNullOrEmpty(name)) return;

        // Check if this is a partial class
        bool isPartial = false;
        bool isStatic = false;
        bool isUnsealed = false;
        var modList = FindChild(node, SyntaxKind.ModifierList);
        if (modList != null)
        {
            for (int i = 0; i < modList.SlotCount; i++)
            {
                if (modList.GetSlot(i) is SyntaxToken mod)
                {
                    switch (mod.Kind)
                    {
                        case SyntaxKind.StaticKeyword: isStatic = true; break;
                        case SyntaxKind.PartialKeyword: isPartial = true; break;
                        case SyntaxKind.UnsealedKeyword: isUnsealed = true; break;
                    }
                }
            }
        }

        // Partial merging: if partial and an existing symbol with the same name exists, merge into it
        if (isPartial)
        {
            var existing = ns.Members.FirstOrDefault(m => m.Name == name && m is RuntimeClassSymbol) as RuntimeClassSymbol;
            if (existing != null)
            {
                // Validate matching modifiers
                if (existing.IsUnsealed != isUnsealed)
                {
                    int pos = _posMap.GetPositionOf(name);
                    _diagnostics.Add(new Diagnostics.Diagnostic(
                        Diagnostics.DiagnosticSeverity.Error, "MIDL1031",
                        $"Partial runtimeclass '{name}' has conflicting modifiers (sealed vs unsealed)",
                        pos, name.Length));
                }

                // Validate no conflicting base classes
                var baseList = FindChild(node, SyntaxKind.BaseList);
                if (baseList != null)
                {
                    bool first = true;
                    for (int i = 1; i < baseList.SlotCount; i++)
                    {
                        var slot = baseList.GetSlot(i);
                        if (slot == null || slot.Kind == SyntaxKind.CommaToken || slot.Kind == SyntaxKind.ColonToken)
                            continue;

                        var typeRef = ExtractTypeRef(slot);
                        TrackTypeRef(typeRef, ns);

                        if (first)
                        {
                            if (existing.BaseClass != null && existing.BaseClass.Name != typeRef.Name)
                            {
                                int pos = _posMap.GetPositionOf(name);
                                _diagnostics.Add(new Diagnostics.Diagnostic(
                                    Diagnostics.DiagnosticSeverity.Error, "MIDL1032",
                                    $"Partial runtimeclass '{name}' has conflicting base class ('{existing.BaseClass.Name}' vs '{typeRef.Name}')",
                                    pos, name.Length));
                            }
                            else if (existing.BaseClass == null)
                            {
                                existing.BaseClass = typeRef;
                            }
                            first = false;
                        }
                        else
                        {
                            existing.Interfaces.Add(typeRef);
                        }
                    }
                }

                // Merge attributes
                var rcAttrList = FindChild(node, SyntaxKind.AttributeList);
                if (rcAttrList != null)
                {
                    if (TryExtractUuidAttribute(rcAttrList, out var rcUuid))
                        existing.Uuid ??= rcUuid;

                    var attrs = ExtractAttributes(rcAttrList);
                    existing.Attributes.AddRange(attrs);
                    ApplyWellKnownAttributes(existing, attrs);
                }

                // Merge members
                _symbolTable.EnterScope(existing);
                BindClassMembers(node, existing, ns);
                _symbolTable.ExitScope();
                return;
            }
        }

        if (!TryDefineType(ns, name)) return;

        var symbol = new RuntimeClassSymbol(name, ns);
        ns.AddMember(symbol);
        _symbolTable.Define(symbol);

        // Extract [uuid("...")] if present
        var attrList = FindChild(node, SyntaxKind.AttributeList);
        if (attrList != null && TryExtractUuidAttribute(attrList, out var uuid))
        {
            symbol.Uuid = uuid;
        }

        // Extract all attributes
        if (attrList != null)
        {
            var attrs = ExtractAttributes(attrList);
            symbol.Attributes.AddRange(attrs);
            ApplyWellKnownAttributes(symbol, attrs);
        }

        symbol.IsStatic = isStatic;
        symbol.IsPartial = isPartial;
        symbol.IsUnsealed = isUnsealed;

        // Base list
        var mainBaseList = FindChild(node, SyntaxKind.BaseList);
        if (mainBaseList != null)
        {
            bool first = true;
            for (int i = 1; i < mainBaseList.SlotCount; i++)
            {
                var slot = mainBaseList.GetSlot(i);
                if (slot == null || slot.Kind == SyntaxKind.CommaToken || slot.Kind == SyntaxKind.ColonToken)
                    continue;

                var typeRef = ExtractTypeRef(slot);
                TrackTypeRef(typeRef, ns);

                if (first)
                {
                    symbol.BaseClass = typeRef;
                    first = false;
                }
                else
                {
                    symbol.Interfaces.Add(typeRef);
                }
            }
        }

        // Bind members
        _symbolTable.EnterScope(symbol);
        BindClassMembers(node, symbol, ns);
        _symbolTable.ExitScope();
    }

    private void BindClassMembers(GreenNode node, RuntimeClassSymbol classSymbol, NamespaceSymbol ns)
    {
        int openBraceIdx = FindSlotIndex(node, SyntaxKind.OpenBraceToken);
        int closeBraceIdx = FindLastSlotIndex(node, SyntaxKind.CloseBraceToken);
        if (openBraceIdx < 0 || closeBraceIdx < 0) return;

        for (int i = openBraceIdx + 1; i < closeBraceIdx; i++)
        {
            var child = node.GetSlot(i);
            if (child == null) continue;

            switch (child.Kind)
            {
                case SyntaxKind.ConstructorDeclaration:
                    classSymbol.AddMember(BindConstructor(child, classSymbol, ns));
                    break;
                case SyntaxKind.MethodDeclaration:
                    var method = BindMethod(child, classSymbol, ns);
                    CheckSameArityOverload(classSymbol.Members, method);
                    classSymbol.AddMember(method);
                    break;
                case SyntaxKind.PropertyDeclaration:
                    var prop = BindProperty(child, classSymbol, ns);
                    CheckWriteOnlyProperty(prop);
                    classSymbol.AddMember(prop);
                    break;
                case SyntaxKind.EventDeclaration:
                    classSymbol.AddMember(BindEvent(child, classSymbol, ns));
                    break;
                case SyntaxKind.VersionedBlock:
                    BindVersionedBlockMembers(child, classSymbol, ns);
                    break;
            }
        }
    }

    private void BindVersionedBlockMembers(GreenNode node, RuntimeClassSymbol classSymbol, NamespaceSymbol ns)
    {
        // Extract block-level attributes (e.g. [contract(C, N)] { ... })
        List<AttributeData>? blockAttrs = null;
        var blockAttrList = FindChild(node, SyntaxKind.AttributeList);
        if (blockAttrList != null)
        {
            blockAttrs = ExtractAttributes(blockAttrList);
        }

        for (int i = 0; i < node.SlotCount; i++)
        {
            var child = node.GetSlot(i);
            if (child == null) continue;

            ISymbol? member = null;
            switch (child.Kind)
            {
                case SyntaxKind.ConstructorDeclaration:
                    member = BindConstructor(child, classSymbol, ns);
                    classSymbol.AddMember(member);
                    break;
                case SyntaxKind.MethodDeclaration:
                    member = BindMethod(child, classSymbol, ns);
                    classSymbol.AddMember(member);
                    break;
                case SyntaxKind.PropertyDeclaration:
                    member = BindProperty(child, classSymbol, ns);
                    classSymbol.AddMember(member);
                    break;
                case SyntaxKind.EventDeclaration:
                    member = BindEvent(child, classSymbol, ns);
                    classSymbol.AddMember(member);
                    break;
            }

            // Apply block-level attributes to the member
            if (member != null && blockAttrs != null)
            {
                ApplyWellKnownAttributes(member, blockAttrs);
            }
        }
    }

    #endregion

    #region Interface

    private void BindInterfaceDeclaration(GreenNode node, NamespaceSymbol ns)
    {
        int keywordIdx = FindSlotIndex(node, SyntaxKind.InterfaceKeyword);
        if (keywordIdx < 0) return;

        var nameToken = node.GetSlot(keywordIdx + 1) as SyntaxToken;
        var name = nameToken?.Text ?? "";
        if (string.IsNullOrEmpty(name)) return;

        if (!TryDefineType(ns, name)) return;

        var symbol = new InterfaceSymbol(name, ns);
        ns.AddMember(symbol);
        _symbolTable.Define(symbol);

        // Extract [uuid("...")] if present
        var attrList = FindChild(node, SyntaxKind.AttributeList);
        if (attrList != null && TryExtractUuidAttribute(attrList, out var uuid))
        {
            symbol.Uuid = uuid;
        }

        // Extract all attributes
        if (attrList != null)
        {
            var attrs = ExtractAttributes(attrList);
            symbol.Attributes.AddRange(attrs);
            ApplyWellKnownAttributes(symbol, attrs);
        }

        // Requires clause
        var requiresClause = FindChild(node, SyntaxKind.RequiresClause);
        if (requiresClause != null)
        {
            for (int i = 1; i < requiresClause.SlotCount; i++)
            {
                var slot = requiresClause.GetSlot(i);
                if (slot == null || slot.Kind == SyntaxKind.CommaToken) continue;
                var typeRef = ExtractTypeRef(slot);
                TrackTypeRef(typeRef, ns);
                symbol.RequiredInterfaces.Add(typeRef);
            }
        }

        // Base list
        var baseList = FindChild(node, SyntaxKind.BaseList);
        if (baseList != null)
        {
            for (int i = 1; i < baseList.SlotCount; i++)
            {
                var slot = baseList.GetSlot(i);
                if (slot == null || slot.Kind == SyntaxKind.CommaToken || slot.Kind == SyntaxKind.ColonToken)
                    continue;
                var typeRef = ExtractTypeRef(slot);
                TrackTypeRef(typeRef, ns);
                symbol.RequiredInterfaces.Add(typeRef);
            }
        }

        // Bind members
        _symbolTable.EnterScope(symbol);
        BindInterfaceMembers(node, symbol, ns);
        _symbolTable.ExitScope();
    }

    private void BindInterfaceMembers(GreenNode node, InterfaceSymbol ifaceSymbol, NamespaceSymbol ns)
    {
        int openBraceIdx = FindSlotIndex(node, SyntaxKind.OpenBraceToken);
        int closeBraceIdx = FindLastSlotIndex(node, SyntaxKind.CloseBraceToken);
        if (openBraceIdx < 0 || closeBraceIdx < 0) return;

        for (int i = openBraceIdx + 1; i < closeBraceIdx; i++)
        {
            var child = node.GetSlot(i);
            if (child == null) continue;

            switch (child.Kind)
            {
                case SyntaxKind.ConstructorDeclaration:
                {
                    // MIDL1039: Constructors are not allowed in interfaces
                    var ctorName = FindNameIdentifier(child);
                    if (!string.IsNullOrEmpty(ctorName))
                        _diagnostics.Add(DiagnosticDescriptors.ConstructorInInterface(ctorName, _posMap.GetPositionOf(ctorName)));
                    break;
                }
                case SyntaxKind.MethodDeclaration:
                {
                    var method = BindMethod(child, ifaceSymbol, ns);
                    // MIDL1040: Static members are not allowed in interfaces
                    if (method.IsStatic && !string.IsNullOrEmpty(method.Name))
                        _diagnostics.Add(DiagnosticDescriptors.StaticMemberInInterface(method.Name, _posMap.GetPositionOf(method.Name)));
                    ifaceSymbol.AddMember(method);
                    break;
                }
                case SyntaxKind.PropertyDeclaration:
                {
                    var prop = BindProperty(child, ifaceSymbol, ns);
                    if (prop.IsStatic && !string.IsNullOrEmpty(prop.Name))
                        _diagnostics.Add(DiagnosticDescriptors.StaticMemberInInterface(prop.Name, _posMap.GetPositionOf(prop.Name)));
                    ifaceSymbol.AddMember(prop);
                    break;
                }
                case SyntaxKind.EventDeclaration:
                {
                    var evt = BindEvent(child, ifaceSymbol, ns);
                    if (evt.IsStatic && !string.IsNullOrEmpty(evt.Name))
                        _diagnostics.Add(DiagnosticDescriptors.StaticMemberInInterface(evt.Name, _posMap.GetPositionOf(evt.Name)));
                    ifaceSymbol.AddMember(evt);
                    break;
                }
                case SyntaxKind.VersionedBlock:
                    BindInterfaceVersionedBlockMembers(child, ifaceSymbol, ns);
                    break;
            }
        }
    }

    private void BindInterfaceVersionedBlockMembers(GreenNode node, InterfaceSymbol ifaceSymbol, NamespaceSymbol ns)
    {
        for (int i = 0; i < node.SlotCount; i++)
        {
            var child = node.GetSlot(i);
            if (child == null) continue;

            switch (child.Kind)
            {
                case SyntaxKind.MethodDeclaration:
                    ifaceSymbol.AddMember(BindMethod(child, ifaceSymbol, ns));
                    break;
                case SyntaxKind.PropertyDeclaration:
                    ifaceSymbol.AddMember(BindProperty(child, ifaceSymbol, ns));
                    break;
                case SyntaxKind.EventDeclaration:
                    ifaceSymbol.AddMember(BindEvent(child, ifaceSymbol, ns));
                    break;
            }
        }
    }

    #endregion

    #region Delegate

    private void BindDelegateDeclaration(GreenNode node, NamespaceSymbol ns)
    {
        int keywordIdx = FindSlotIndex(node, SyntaxKind.DelegateKeyword);
        if (keywordIdx < 0) return;

        // After delegate keyword: TypeReference, IdentifierToken, ParameterList
        var returnTypeNode = node.GetSlot(keywordIdx + 1);
        var nameToken = node.GetSlot(keywordIdx + 2) as SyntaxToken;
        var name = nameToken?.Text ?? "";
        if (string.IsNullOrEmpty(name)) return;

        if (!TryDefineType(ns, name)) return;

        var symbol = new DelegateSymbol(name, ns);
        symbol.ReturnType = ExtractTypeRef(returnTypeNode);
        TrackTypeRef(symbol.ReturnType, ns);
        ns.AddMember(symbol);
        _symbolTable.Define(symbol);

        // Extract all attributes
        var delAttrList = FindChild(node, SyntaxKind.AttributeList);
        if (delAttrList != null)
        {
            var attrs = ExtractAttributes(delAttrList);
            symbol.Attributes.AddRange(attrs);
            ApplyWellKnownAttributes(symbol, attrs);
        }

        // Parameters
        var paramList = FindChild(node, SyntaxKind.ParameterList);
        if (paramList != null)
        {
            symbol.Parameters.AddRange(BindParameters(paramList, symbol, ns));
        }
    }

    #endregion

    #region Struct

    private void BindStructDeclaration(GreenNode node, NamespaceSymbol ns)
    {
        int keywordIdx = FindSlotIndex(node, SyntaxKind.StructKeyword);
        if (keywordIdx < 0) return;

        var nameToken = node.GetSlot(keywordIdx + 1) as SyntaxToken;
        var name = nameToken?.Text ?? "";
        if (string.IsNullOrEmpty(name)) return;

        if (!TryDefineType(ns, name)) return;

        var symbol = new StructSymbol(name, ns);
        ns.AddMember(symbol);
        _symbolTable.Define(symbol);

        // Extract all attributes
        var structAttrList = FindChild(node, SyntaxKind.AttributeList);
        if (structAttrList != null)
        {
            var attrs = ExtractAttributes(structAttrList);
            symbol.Attributes.AddRange(attrs);
            ApplyWellKnownAttributes(symbol, attrs);
        }

        // Fields between braces
        int openBraceIdx = FindSlotIndex(node, SyntaxKind.OpenBraceToken);
        int closeBraceIdx = FindLastSlotIndex(node, SyntaxKind.CloseBraceToken);
        if (openBraceIdx < 0 || closeBraceIdx < 0) return;

        for (int i = openBraceIdx + 1; i < closeBraceIdx; i++)
        {
            var child = node.GetSlot(i);
            if (child?.Kind == SyntaxKind.FieldDeclaration)
            {
                symbol.AddField(BindField(child, symbol, ns));
            }
            // MIDL1041: Properties and events are not allowed in structs
            else if (child?.Kind == SyntaxKind.PropertyDeclaration)
            {
                var propName = FindNameIdentifier(child);
                if (!string.IsNullOrEmpty(propName))
                    _diagnostics.Add(DiagnosticDescriptors.PropertyInStruct(propName, _posMap.GetPositionOf(propName)));
            }
            else if (child?.Kind == SyntaxKind.EventDeclaration)
            {
                var evtName = FindNameIdentifier(child);
                if (!string.IsNullOrEmpty(evtName))
                    _diagnostics.Add(DiagnosticDescriptors.EventInStruct(evtName, _posMap.GetPositionOf(evtName)));
            }
        }

        // MIDL1009: Empty struct
        if (symbol.Fields.Count == 0)
        {
            _diagnostics.Add(DiagnosticDescriptors.EmptyStruct(name, _posMap.GetPositionOf(name)));
        }
    }

    private FieldSymbol BindField(GreenNode node, ISymbol containingSymbol, NamespaceSymbol ns)
    {
        // Slots: [AttributeList], TypeReference, IdentifierToken, SemicolonToken
        var typeNode = FindChild(node, SyntaxKind.TypeReference)
            ?? FindChild(node, SyntaxKind.GenericName)
            ?? FindChild(node, SyntaxKind.QualifiedName)
            ?? FindChild(node, SyntaxKind.ArrayType);

        // Find name: last identifier token before semicolon
        var name = FindNameIdentifier(node);

        var field = new FieldSymbol(name, containingSymbol);
        if (typeNode != null)
        {
            field.Type = ExtractTypeRef(typeNode);
            TrackTypeRef(field.Type, ns);
        }
        return field;
    }

    #endregion

    #region Attribute (user-defined)

    private void BindAttributeDeclaration(GreenNode node, NamespaceSymbol ns)
    {
        int keywordIdx = FindSlotIndex(node, SyntaxKind.AttributeKeyword);
        if (keywordIdx < 0) return;

        var nameToken = node.GetSlot(keywordIdx + 1) as SyntaxToken;
        var name = nameToken?.Text ?? "";
        if (string.IsNullOrEmpty(name)) return;

        if (!TryDefineType(ns, name)) return;

        var symbol = new AttributeTypeSymbol(name, ns);
        ns.AddMember(symbol);
        _symbolTable.Define(symbol);

        // Extract attributes on the attribute declaration itself
        var attrList = FindChild(node, SyntaxKind.AttributeList);
        if (attrList != null)
        {
            var attrs = ExtractAttributes(attrList);
            symbol.Attributes.AddRange(attrs);

            foreach (var attr in attrs)
            {
                switch (attr.Name.ToLowerInvariant())
                {
                    case "attributeusage":
                        foreach (var arg in attr.Arguments)
                        {
                            if (arg.Kind == ArgKind.Identifier && arg.StringValue != null)
                                symbol.ValidTargets.Add(arg.StringValue);
                        }
                        break;

                    case "attributename":
                        symbol.AttributeNameOverride = attr.GetString(0);
                        break;

                    case "allowmultiple":
                        symbol.AllowMultiple = true;
                        break;
                }
            }
        }

        // Fields between braces
        int openBraceIdx = FindSlotIndex(node, SyntaxKind.OpenBraceToken);
        int closeBraceIdx = FindLastSlotIndex(node, SyntaxKind.CloseBraceToken);
        if (openBraceIdx < 0 || closeBraceIdx < 0) return;

        for (int i = openBraceIdx + 1; i < closeBraceIdx; i++)
        {
            var child = node.GetSlot(i);
            if (child?.Kind == SyntaxKind.FieldDeclaration)
            {
                symbol.AddField(BindField(child, symbol, ns));
            }
        }
    }

    #endregion

    #region Enum

    private void BindEnumDeclaration(GreenNode node, NamespaceSymbol ns)
    {
        int keywordIdx = FindSlotIndex(node, SyntaxKind.EnumKeyword);
        if (keywordIdx < 0) return;

        var nameToken = node.GetSlot(keywordIdx + 1) as SyntaxToken;
        var name = nameToken?.Text ?? "";
        if (string.IsNullOrEmpty(name)) return;

        if (!TryDefineType(ns, name)) return;

        var symbol = new EnumSymbol(name, ns);
        ns.AddMember(symbol);
        _symbolTable.Define(symbol);

        // Check for [flags] attribute
        var enumAttrList = FindChild(node, SyntaxKind.AttributeList);
        if (enumAttrList != null && HasAttribute(enumAttrList, "flags"))
        {
            symbol.IsFlags = true;
            // Per WinRT spec: [flags] enums have UInt32 underlying type
            symbol.UnderlyingType = "UInt32";
        }

        // Extract all attributes on the enum
        if (enumAttrList != null)
        {
            var attrs = ExtractAttributes(enumAttrList);
            symbol.Attributes.AddRange(attrs);
            ApplyWellKnownAttributes(symbol, attrs);
        }

        // Enum members between braces
        int openBraceIdx = FindSlotIndex(node, SyntaxKind.OpenBraceToken);
        int closeBraceIdx = FindLastSlotIndex(node, SyntaxKind.CloseBraceToken);
        if (openBraceIdx < 0 || closeBraceIdx < 0) return;

        var enumMemberNames = new HashSet<string>(StringComparer.Ordinal);

        for (int i = openBraceIdx + 1; i < closeBraceIdx; i++)
        {
            var child = node.GetSlot(i);
            if (child?.Kind == SyntaxKind.EnumMemberDeclaration)
            {
                var memberName = FindFirstIdentifierText(child);
                if (!string.IsNullOrEmpty(memberName))
                {
                    // MIDL1046: Duplicate enum member
                    if (!enumMemberNames.Add(memberName))
                    {
                        _diagnostics.Add(DiagnosticDescriptors.DuplicateEnumMember(memberName, _posMap.GetPositionOf(memberName)));
                    }

                    var member = new EnumMemberSymbol(memberName, symbol);

                    // Extract explicit value (Name = expr)
                    ExtractEnumMemberValue(child, member, symbol);

                    // Extract attributes on enum member
                    var memberAttrList = FindChild(child, SyntaxKind.AttributeList);
                    if (memberAttrList != null)
                    {
                        var memberAttrs = ExtractAttributes(memberAttrList);
                        member.Attributes.AddRange(memberAttrs);
                        ApplyWellKnownAttributes(member, memberAttrs);
                    }

                    symbol.AddMember(member);
                }
            }
        }
    }

    private void ExtractEnumMemberValue(GreenNode memberNode, EnumMemberSymbol member, EnumSymbol parentEnum)
    {
        int eqIdx = FindSlotIndex(memberNode, SyntaxKind.EqualsToken);
        if (eqIdx < 0) return;

        for (int j = eqIdx + 1; j < memberNode.SlotCount; j++)
        {
            var valSlot = memberNode.GetSlot(j);
            if (valSlot == null) continue;
            if (valSlot is SyntaxToken { Kind: SyntaxKind.CommaToken }) break;

            // Unwrap expression nodes to find the actual token
            var token = FindFirstToken(valSlot);
            if (token == null) break;

            member.ExplicitValueText = token.Text;

            if (token.Kind == SyntaxKind.NumericLiteralToken)
            {
                long v = 0;
                bool parsed = false;
                var txt = token.Text;
                bool neg = txt.StartsWith('-');
                var s = neg ? txt.AsSpan(1) : txt.AsSpan();
                if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
                    long.TryParse(s[2..], System.Globalization.NumberStyles.HexNumber, null, out v))
                { if (neg) v = -v; parsed = true; }
                else if (long.TryParse(txt, out v))
                { parsed = true; }

                if (parsed)
                {
                    member.ExplicitValue = v;
                    ValidateEnumValueRange(v, txt, member.Name, parentEnum);
                }
            }
            else if (token.Kind is SyntaxKind.FloatLiteralToken or SyntaxKind.StringLiteralToken)
            {
                _diagnostics.Add(DiagnosticDescriptors.EnumValueNotInteger(
                    member.Name, parentEnum.Name, token.Text, _posMap.GetPositionOf(token.Text)));
            }
            break;
        }
    }

    private static SyntaxToken? FindFirstToken(GreenNode node)
    {
        if (node is SyntaxToken t) return t;
        for (int i = 0; i < node.SlotCount; i++)
        {
            var slot = node.GetSlot(i);
            if (slot == null) continue;
            var result = FindFirstToken(slot);
            if (result != null) return result;
        }
        return null;
    }

    private void ValidateEnumValueRange(long value, string valueText, string memberName, EnumSymbol parentEnum)
    {
        if (parentEnum.UnderlyingType == "UInt32")
        {
            if (value < 0 || value > uint.MaxValue)
                _diagnostics.Add(DiagnosticDescriptors.EnumValueOutOfRange(
                    memberName, parentEnum.Name, valueText, "UInt32", $"0..{uint.MaxValue}",
                    _posMap.GetPositionOf(valueText)));
        }
        else
        {
            if (value < int.MinValue || value > int.MaxValue)
                _diagnostics.Add(DiagnosticDescriptors.EnumValueOutOfRange(
                    memberName, parentEnum.Name, valueText, "Int32", $"{int.MinValue}..{int.MaxValue}",
                    _posMap.GetPositionOf(valueText)));
        }
    }

    #endregion

    #region ApiContract

    private void BindApiContractDeclaration(GreenNode node, NamespaceSymbol ns)
    {
        int keywordIdx = FindSlotIndex(node, SyntaxKind.ApicontractKeyword);
        if (keywordIdx < 0) return;

        var nameToken = node.GetSlot(keywordIdx + 1) as SyntaxToken;
        var name = nameToken?.Text ?? "";
        if (string.IsNullOrEmpty(name)) return;

        if (!TryDefineType(ns, name)) return;

        var symbol = new ApiContractSymbol(name, ns);
        ns.AddMember(symbol);
        _symbolTable.Define(symbol);

        // Extract all attributes
        var acAttrList = FindChild(node, SyntaxKind.AttributeList);
        if (acAttrList != null)
        {
            var attrs = ExtractAttributes(acAttrList);
            symbol.Attributes.AddRange(attrs);
            ApplyWellKnownAttributes(symbol, attrs);
        }
    }

    #endregion

    #region DeclareBlock

    private void BindDeclareBlock(GreenNode node, NamespaceSymbol ns)
    {
        int openBraceIdx = FindSlotIndex(node, SyntaxKind.OpenBraceToken);
        int closeBraceIdx = FindLastSlotIndex(node, SyntaxKind.CloseBraceToken);
        if (openBraceIdx < 0 || closeBraceIdx < 0) return;

        for (int i = openBraceIdx + 1; i < closeBraceIdx; i++)
        {
            var child = node.GetSlot(i);
            if (child != null)
                BindDeclaration(child, ns);
        }
    }

    #endregion

    #region Members

    private ConstructorSymbol BindConstructor(GreenNode node, ISymbol containingSymbol, NamespaceSymbol ns)
    {
        var name = FindNameIdentifier(node);
        var symbol = new ConstructorSymbol(name, containingSymbol);

        // Extract all attributes
        var ctorAttrList = FindChild(node, SyntaxKind.AttributeList);
        if (ctorAttrList != null)
        {
            var attrs = ExtractAttributes(ctorAttrList);
            symbol.Attributes.AddRange(attrs);
            ApplyWellKnownAttributes(symbol, attrs);
        }

        var paramList = FindChild(node, SyntaxKind.ParameterList);
        if (paramList != null)
        {
            symbol.Parameters.AddRange(BindParameters(paramList, symbol, ns));
        }

        return symbol;
    }

    private MethodSymbol BindMethod(GreenNode node, ISymbol containingSymbol, NamespaceSymbol ns)
    {
        // Slots: [AttributeList], [ModifierList], TypeReference, IdentifierToken, ParameterList, SemicolonToken
        // Find the TypeReference, then the next identifier after it
        var (returnTypeNode, nameText) = ExtractReturnTypeAndName(node);

        var symbol = new MethodSymbol(nameText, containingSymbol);
        symbol.ReturnType = ExtractTypeRef(returnTypeNode);
        TrackTypeRef(symbol.ReturnType, ns);

        // Extract all attributes
        var methodAttrList = FindChild(node, SyntaxKind.AttributeList);
        if (methodAttrList != null)
        {
            var attrs = ExtractAttributes(methodAttrList);
            symbol.Attributes.AddRange(attrs);
            ApplyWellKnownAttributes(symbol, attrs);
        }

        // Check modifiers
        var modList = FindChild(node, SyntaxKind.ModifierList);
        if (modList != null && HasModifier(modList, SyntaxKind.StaticKeyword))
        {
            symbol.IsStatic = true;
        }
        if (modList != null && HasModifier(modList, SyntaxKind.ProtectedKeyword))
        {
            symbol.IsProtected = true;
        }
        if (modList != null && HasModifier(modList, SyntaxKind.OverridableKeyword))
        {
            symbol.IsOverridable = true;
        }

        var paramList = FindChild(node, SyntaxKind.ParameterList);
        if (paramList != null)
        {
            symbol.Parameters.AddRange(BindParameters(paramList, symbol, ns));
        }

        return symbol;
    }

    private PropertySymbol BindProperty(GreenNode node, ISymbol containingSymbol, NamespaceSymbol ns)
    {
        var (typeNode, nameText) = ExtractReturnTypeAndName(node);

        var symbol = new PropertySymbol(nameText, containingSymbol);
        symbol.Type = ExtractTypeRef(typeNode);
        TrackTypeRef(symbol.Type, ns);

        // Extract all attributes
        var propAttrList = FindChild(node, SyntaxKind.AttributeList);
        if (propAttrList != null)
        {
            var attrs = ExtractAttributes(propAttrList);
            symbol.Attributes.AddRange(attrs);
            ApplyWellKnownAttributes(symbol, attrs);
        }

        // Check modifiers
        var modList = FindChild(node, SyntaxKind.ModifierList);
        if (modList != null && HasModifier(modList, SyntaxKind.StaticKeyword))
        {
            symbol.IsStatic = true;
        }
        if (modList != null && HasModifier(modList, SyntaxKind.ProtectedKeyword))
        {
            symbol.IsProtected = true;
        }
        if (modList != null && HasModifier(modList, SyntaxKind.OverridableKeyword))
        {
            symbol.IsOverridable = true;
        }

        // Check accessors
        var accessors = FindChild(node, SyntaxKind.PropertyAccessors);
        if (accessors != null)
        {
            symbol.HasGetter = HasChildToken(accessors, SyntaxKind.GetKeyword);
            symbol.HasSetter = HasChildToken(accessors, SyntaxKind.SetKeyword);
        }
        else
        {
            // No explicit accessors means get+set
            symbol.HasGetter = true;
            symbol.HasSetter = true;
        }

        return symbol;
    }

    private EventSymbol BindEvent(GreenNode node, ISymbol containingSymbol, NamespaceSymbol ns)
    {
        // Slots: [AttributeList], EventKeyword, TypeReference, IdentifierToken, SemicolonToken
        int eventIdx = FindSlotIndex(node, SyntaxKind.EventKeyword);
        if (eventIdx < 0) return new EventSymbol("", containingSymbol);

        var typeNode = node.GetSlot(eventIdx + 1);
        var nameToken = node.GetSlot(eventIdx + 2) as SyntaxToken;
        var name = nameToken?.Text ?? "";

        var symbol = new EventSymbol(name, containingSymbol);
        symbol.DelegateType = ExtractTypeRef(typeNode);
        TrackTypeRef(symbol.DelegateType, ns);

        // Extract all attributes
        var eventAttrList = FindChild(node, SyntaxKind.AttributeList);
        if (eventAttrList != null)
        {
            var attrs = ExtractAttributes(eventAttrList);
            symbol.Attributes.AddRange(attrs);
            ApplyWellKnownAttributes(symbol, attrs);
        }

        // Check modifiers
        var modList = FindChild(node, SyntaxKind.ModifierList);
        if (modList != null && HasModifier(modList, SyntaxKind.StaticKeyword))
        {
            symbol.IsStatic = true;
        }
        if (modList != null && HasModifier(modList, SyntaxKind.ProtectedKeyword))
        {
            symbol.IsProtected = true;
        }
        if (modList != null && HasModifier(modList, SyntaxKind.OverridableKeyword))
        {
            symbol.IsOverridable = true;
        }

        return symbol;
    }

    private List<ParameterSymbol> BindParameters(GreenNode paramList, ISymbol containingSymbol, NamespaceSymbol ns)
    {
        var result = new List<ParameterSymbol>();

        for (int i = 0; i < paramList.SlotCount; i++)
        {
            var child = paramList.GetSlot(i);
            if (child?.Kind != SyntaxKind.Parameter) continue;

            var param = BindParameter(child, containingSymbol, ns);
            result.Add(param);
        }

        return result;
    }

    private ParameterSymbol BindParameter(GreenNode node, ISymbol containingSymbol, NamespaceSymbol ns)
    {
        // Slots: [AttributeList...], [OutKeyword|RefKeyword], TypeReference, IdentifierToken
        var direction = ParameterDirection.In;
        GreenNode? typeNode = null;
        string name = "";

        for (int i = 0; i < node.SlotCount; i++)
        {
            var slot = node.GetSlot(i);
            if (slot == null) continue;

            if (slot is SyntaxToken token)
            {
                switch (token.Kind)
                {
                    case SyntaxKind.OutKeyword:
                        direction = ParameterDirection.Out;
                        break;
                    case SyntaxKind.RefKeyword:
                        direction = ParameterDirection.Ref;
                        break;
                    case SyntaxKind.IdentifierToken:
                    case var k when IsTypeKeywordOrNameable(k):
                        // Last identifier-like token is the name
                        name = token.Text;
                        break;
                }
            }
            else if (IsTypeNode(slot.Kind))
            {
                typeNode = slot;
            }
        }

        var param = new ParameterSymbol(name, containingSymbol) { Direction = direction };
        if (typeNode != null)
        {
            param.Type = ExtractTypeRef(typeNode);
            TrackTypeRef(param.Type, ns);
            param.IsArray = param.Type.IsArray;
        }

        // MIDL1043: 'void' is not valid as a parameter type
        if (param.Type.Name is "void" or "Void")
        {
            _diagnostics.Add(DiagnosticDescriptors.VoidParameterType(_posMap.GetPositionOf("void")));
        }

        // MIDL1013: Reserved parameter names
        if (name is "result" or "operation" or "value")
        {
            _diagnostics.Add(DiagnosticDescriptors.ReservedParameterName(name, _posMap.GetPositionOf(name)));
        }

        return param;
    }

    #endregion

    #region Type Reference Extraction

    private TypeRef ExtractTypeRef(GreenNode? node)
    {
        if (node == null) return TypeRef.Unknown;

        switch (node.Kind)
        {
            case SyntaxKind.TypeReference:
            {
                var inner = node.GetSlot(0);
                if (inner is SyntaxToken tok)
                    return new TypeRef(tok.Text) { Position = _posMap.GetPositionOf(tok.Text) };
                if (inner != null)
                    return ExtractTypeRef(inner);
                return TypeRef.Unknown;
            }

            case SyntaxKind.GenericName:
            {
                var nameToken = node.GetSlot(0) as SyntaxToken;
                var typeArgList = node.GetSlot(1);
                var typeArgs = ExtractTypeArguments(typeArgList);
                return new TypeRef(nameToken?.Text ?? "", typeArgs) { Position = _posMap.GetPositionOf(nameToken?.Text ?? "") };
            }

            case SyntaxKind.QualifiedName:
                return ExtractQualifiedTypeRef(node);

            case SyntaxKind.ArrayType:
            {
                var elemRef = ExtractTypeRef(node.GetSlot(0));
                return elemRef.WithArray();
            }

            case SyntaxKind.PointerType:
            {
                return ExtractTypeRef(node.GetSlot(0));
            }

            default:
                if (node is SyntaxToken t)
                    return new TypeRef(t.Text) { Position = _posMap.GetPositionOf(t.Text) };
                return TypeRef.Unknown;
        }
    }

    private TypeRef ExtractQualifiedTypeRef(GreenNode node)
    {
        var parts = new List<string>();
        List<TypeRef>? typeArgs = null;

        for (int i = 0; i < node.SlotCount; i += 2)
        {
            var slot = node.GetSlot(i);
            if (slot is SyntaxToken tok)
            {
                parts.Add(tok.Text);
            }
            else if (slot?.Kind == SyntaxKind.GenericName)
            {
                var gName = slot.GetSlot(0) as SyntaxToken;
                parts.Add(gName?.Text ?? "");
                typeArgs = ExtractTypeArguments(slot.GetSlot(1));
            }
        }

        return new TypeRef(string.Join(".", parts), typeArgs ?? []) { Position = _posMap.GetPositionOf(parts.FirstOrDefault() ?? "") };
    }

    private List<TypeRef> ExtractTypeArguments(GreenNode? typeArgList)
    {
        if (typeArgList == null || typeArgList.Kind != SyntaxKind.TypeArgumentList)
            return [];

        var args = new List<TypeRef>();
        for (int i = 1; i < typeArgList.SlotCount - 1; i++)
        {
            var arg = typeArgList.GetSlot(i);
            if (arg == null || arg.Kind == SyntaxKind.CommaToken) continue;
            args.Add(ExtractTypeRef(arg));
        }
        return args;
    }

    #endregion

    #region Name Extraction

    private static string ExtractDottedName(GreenNode? node)
    {
        if (node is SyntaxToken token) return token.Text;
        if (node == null) return "";

        if (node.Kind == SyntaxKind.DottedName)
        {
            var parts = new List<string>();
            for (int i = 0; i < node.SlotCount; i += 2)
            {
                if (node.GetSlot(i) is SyntaxToken t)
                    parts.Add(t.Text);
            }
            return string.Join(".", parts);
        }

        return "";
    }

    /// <summary>
    /// Extract return type node and name from a method/property declaration.
    /// Pattern: [attrs], [mods], TypeReference, IdentifierToken, ...
    /// </summary>
    private static (GreenNode? TypeNode, string Name) ExtractReturnTypeAndName(GreenNode node)
    {
        GreenNode? typeNode = null;
        string name = "";

        for (int i = 0; i < node.SlotCount; i++)
        {
            var slot = node.GetSlot(i);
            if (slot == null) continue;

            if (IsTypeNode(slot.Kind))
            {
                typeNode = slot;
                // Next slot should be the name
                var next = node.GetSlot(i + 1);
                if (next is SyntaxToken nt)
                    name = nt.Text;
                break;
            }
        }

        return (typeNode, name);
    }

    private static string FindNameIdentifier(GreenNode node)
    {
        // Find the last identifier-like token that isn't followed by another identifier
        // For constructors: [attrs], [mods], IdentifierToken, ParameterList, Semicolon
        // For fields: [attrs], TypeReference, IdentifierToken, Semicolon
        string lastIdent = "";
        for (int i = 0; i < node.SlotCount; i++)
        {
            var slot = node.GetSlot(i);
            if (slot is SyntaxToken t && IsIdentifierLike(t.Kind))
            {
                lastIdent = t.Text;
            }
            else if (slot?.Kind == SyntaxKind.ParameterList
                     || slot?.Kind == SyntaxKind.SemicolonToken)
            {
                break;
            }
        }
        return lastIdent;
    }

    private static string FindFirstIdentifierText(GreenNode node)
    {
        for (int i = 0; i < node.SlotCount; i++)
        {
            var slot = node.GetSlot(i);
            if (slot is SyntaxToken t && IsIdentifierLike(t.Kind))
                return t.Text;
        }
        return "";
    }

    #endregion

    #region Strict Mode Checks

    private void CheckStrictModeViolation(GreenNode node)
    {
        switch (node.Kind)
        {
            case SyntaxKind.CoclassDeclaration:
                _diagnostics.Add(DiagnosticDescriptors.ClassicConstruct("coclass", FindKeywordPosition(node, SyntaxKind.CoclassKeyword)));
                break;
            case SyntaxKind.LibraryDeclaration:
                _diagnostics.Add(DiagnosticDescriptors.ClassicConstruct("library", FindKeywordPosition(node, SyntaxKind.LibraryKeyword)));
                break;
            case SyntaxKind.ModuleDeclaration:
                _diagnostics.Add(DiagnosticDescriptors.ClassicConstruct("module", FindKeywordPosition(node, SyntaxKind.ModuleKeyword)));
                break;
            case SyntaxKind.TypedefDeclaration:
                _diagnostics.Add(DiagnosticDescriptors.ClassicConstruct("typedef", FindKeywordPosition(node, SyntaxKind.TypedefKeyword)));
                break;
            case SyntaxKind.DispinterfaceDeclaration:
                _diagnostics.Add(DiagnosticDescriptors.ClassicConstruct("dispinterface", FindKeywordPosition(node, SyntaxKind.DispinterfaceKeyword)));
                break;
            case SyntaxKind.CppQuoteDirective:
                _diagnostics.Add(DiagnosticDescriptors.ClassicConstruct("cpp_quote", FindKeywordPosition(node, SyntaxKind.CppQuoteKeyword)));
                break;
            case SyntaxKind.ImportlibDirective:
                _diagnostics.Add(DiagnosticDescriptors.ClassicConstruct("importlib", FindKeywordPosition(node, SyntaxKind.ImportlibKeyword)));
                break;
            case SyntaxKind.UnionDeclaration:
                _diagnostics.Add(DiagnosticDescriptors.ClassicConstruct("union", FindKeywordPosition(node, SyntaxKind.UnionKeyword)));
                break;
        }
    }

    private int FindKeywordPosition(GreenNode node, SyntaxKind keywordKind)
    {
        for (int i = 0; i < node.SlotCount; i++)
        {
            var slot = node.GetSlot(i);
            if (slot is SyntaxToken t && t.Kind == keywordKind)
                return _posMap.GetPositionOf(t.Text);
        }
        return _posMap.GetPositionOfKind(keywordKind);
    }

    private void CheckStrictModeTokens(GreenNode node)
    {
        // Walk the tree, but use _posMap to get positions for tokens
        CheckStrictModeTokensRecursive(node, 0);
    }

    private int _strictTokenIndex = 0;

    private void CheckStrictModeTokensRecursive(GreenNode node, int depth)
    {
        for (int i = 0; i < node.SlotCount; i++)
        {
            var slot = node.GetSlot(i);
            if (slot == null) continue;

            if (slot is SyntaxToken token)
            {
                // Find this token's position in the token stream
                int pos = _posMap.GetPositionOf(token.Text, _strictTokenIndex);
                CheckStrictModeToken(token, pos);
            }
            else
            {
                // Check pointer types
                if (slot.Kind == SyntaxKind.PointerType)
                {
                    var innerType = slot.GetSlot(0);
                    var typeName = innerType is SyntaxToken t ? t.Text : "?";
                    int ptrPos = _posMap.GetPositionOf(typeName);
                    _diagnostics.Add(DiagnosticDescriptors.PointerType(typeName, ptrPos));
                }

                // Check for classic [in]/[out] attributes on parameters
                if (slot.Kind == SyntaxKind.AttributeList)
                {
                    CheckStrictModeAttributes(slot);
                }

                CheckStrictModeTokensRecursive(slot, depth + 1);
            }
        }
    }

    private void CheckStrictModeToken(SyntaxToken token, int position)
    {
        switch (token.Kind)
        {
            case SyntaxKind.HResultKeyword:
                _diagnostics.Add(DiagnosticDescriptors.HResultReturnType(position));
                break;
            case SyntaxKind.ByteKeyword:
                _diagnostics.Add(DiagnosticDescriptors.LowercaseType("byte", "UInt8", position));
                break;
            case SyntaxKind.ShortKeyword:
                _diagnostics.Add(DiagnosticDescriptors.LowercaseType("short", "Int16", position));
                break;
            case SyntaxKind.LongKeyword:
                _diagnostics.Add(DiagnosticDescriptors.LowercaseType("long", "Int32", position));
                break;
            case SyntaxKind.IntKeyword:
                _diagnostics.Add(DiagnosticDescriptors.LowercaseType("int", "Int32", position));
                break;
            case SyntaxKind.FloatKeyword:
                _diagnostics.Add(DiagnosticDescriptors.LowercaseType("float", "Single", position));
                break;
            case SyntaxKind.HyperKeyword:
                _diagnostics.Add(DiagnosticDescriptors.LowercaseType("hyper", "Int64", position));
                break;
            case SyntaxKind.SmallKeyword:
                _diagnostics.Add(DiagnosticDescriptors.LowercaseType("small", "UInt8", position));
                break;
            case SyntaxKind.WcharTKeyword:
                _diagnostics.Add(DiagnosticDescriptors.LowercaseType("wchar_t", "Char", position));
                break;
            case SyntaxKind.BooleanKeyword when token.Text == "boolean":
                _diagnostics.Add(DiagnosticDescriptors.LowercaseType("boolean", "Boolean", position));
                break;
            case SyntaxKind.DoubleKeyword when token.Text == "double":
                _diagnostics.Add(DiagnosticDescriptors.LowercaseType("double", "Double", position));
                break;
            case SyntaxKind.CharKeyword when token.Text == "char":
                _diagnostics.Add(DiagnosticDescriptors.LowercaseType("char", "Char", position));
                break;
            case SyntaxKind.UnsignedKeyword:
                _diagnostics.Add(DiagnosticDescriptors.StrictModeTypeWarning("unsigned",
                    "use UInt8/UInt16/UInt32/UInt64 instead", position));
                break;
            case SyntaxKind.SignedKeyword:
                _diagnostics.Add(DiagnosticDescriptors.StrictModeTypeWarning("signed",
                    "use Int16/Int32/Int64 instead", position));
                break;
            case SyntaxKind.HandleTKeyword:
                _diagnostics.Add(DiagnosticDescriptors.StrictModeTypeWarning("handle_t",
                    "handle_t is not used in MIDL3", position));
                break;
            case SyntaxKind.ErrorStatusTKeyword:
                _diagnostics.Add(DiagnosticDescriptors.StrictModeTypeWarning("error_status_t",
                    "error_status_t is not used in MIDL3", position));
                break;
            case SyntaxKind.IdentifierToken:
                CheckStrictModeIdentifier(token, position);
                break;
        }
    }

    private void CheckStrictModeIdentifier(SyntaxToken token, int position)
    {
        switch (token.Text)
        {
            case "BSTR":
                _diagnostics.Add(DiagnosticDescriptors.ClassicMidlType("BSTR", "String", position));
                break;
            case "LPSTR":
                _diagnostics.Add(DiagnosticDescriptors.ClassicMidlType("LPSTR", "String", position));
                break;
            case "LPWSTR":
                _diagnostics.Add(DiagnosticDescriptors.ClassicMidlType("LPWSTR", "String", position));
                break;
            case "VARIANT":
                _diagnostics.Add(DiagnosticDescriptors.ClassicMidlType("VARIANT", "Object", position));
                break;
        }
    }

    private void CheckStrictModeAttributes(GreenNode attrList)
    {
        for (int i = 0; i < attrList.SlotCount; i++)
        {
            var slot = attrList.GetSlot(i);
            if (slot == null) continue;

            if (slot.Kind == SyntaxKind.AttributeList)
            {
                CheckStrictModeAttributes(slot);
                continue;
            }

            if (slot.Kind == SyntaxKind.Attribute)
            {
                var nameNode = slot.GetSlot(0);
                var name = nameNode is SyntaxToken t ? t.Text : ExtractDottedName(nameNode);
                if (string.Equals(name, "in", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(name, "out", StringComparison.OrdinalIgnoreCase))
                {
                    _diagnostics.Add(DiagnosticDescriptors.ClassicAttribute(name, _posMap.GetPositionOf(name)));
                }
            }
        }
    }

    #endregion

    #region Helpers

    private static void ApplyWellKnownAttributes(ISymbol symbol, List<AttributeData> attrs)
    {
        foreach (var attr in attrs)
        {
            switch (attr.Name.ToLowerInvariant())
            {
                case "contract":
                    var contractName = attr.GetTypeName(0) ?? attr.GetIdentifier(0);
                    var contractVersion = (int?)attr.GetInt(1);
                    if (symbol is RuntimeClassSymbol rc) { rc.ContractName = contractName; rc.ContractVersion = contractVersion; }
                    if (symbol is InterfaceSymbol intf) { intf.ContractName = contractName; intf.ContractVersion = contractVersion; }
                    if (symbol is EnumSymbol en) { en.ContractName = contractName; en.ContractVersion = contractVersion; }
                    if (symbol is EnumMemberSymbol em) { em.ContractName = contractName; em.ContractVersion = contractVersion; }
                    if (symbol is MethodSymbol cm) { cm.ContractName = contractName; cm.ContractVersion = contractVersion; }
                    if (symbol is PropertySymbol cp) { cp.ContractName = contractName; cp.ContractVersion = contractVersion; }
                    if (symbol is EventSymbol ce) { ce.ContractName = contractName; ce.ContractVersion = contractVersion; }
                    if (symbol is ConstructorSymbol cc) { cc.ContractName = contractName; cc.ContractVersion = contractVersion; }
                    break;
                case "contractversion":
                    if (symbol is ApiContractSymbol ac) ac.ContractVersion = (int?)attr.GetInt(0);
                    break;
                case "version":
                    if (symbol is RuntimeClassSymbol rc2) rc2.Version = (int?)attr.GetInt(0);
                    if (symbol is InterfaceSymbol intf2) intf2.Version = (int?)attr.GetInt(0);
                    break;
                case "threading":
                    if (symbol is RuntimeClassSymbol rc3) rc3.ThreadingModel = attr.GetIdentifier(0);
                    break;
                case "marshaling_behavior":
                    if (symbol is RuntimeClassSymbol rc4) rc4.MarshalingBehavior = attr.GetIdentifier(0);
                    break;
                case "deprecated":
                    if (symbol is RuntimeClassSymbol rc5) rc5.DeprecatedMessage = attr.GetString(0);
                    if (symbol is InterfaceSymbol intf3) intf3.DeprecatedMessage = attr.GetString(0);
                    if (symbol is DelegateSymbol del2) del2.DeprecatedMessage = attr.GetString(0);
                    break;
                case "method_name":
                    if (symbol is MethodSymbol m) m.MethodNameOverride = attr.GetString(0);
                    break;
                case "overload":
                    if (symbol is MethodSymbol m2) m2.OverloadGroup = attr.GetString(0);
                    break;
                case "default_overload":
                    if (symbol is MethodSymbol m3) m3.IsDefaultOverload = true;
                    break;
                case "return_name":
                    if (symbol is MethodSymbol m4) m4.ReturnNameOverride = attr.GetString(0);
                    break;
                case "remote_async":
                    if (symbol is MethodSymbol m5) m5.IsRemoteAsync = true;
                    break;
                case "default_interface":
                    if (symbol is RuntimeClassSymbol rc6) rc6.IsDefaultInterface = true;
                    break;
            }
        }
    }

    private void TrackTypeRef(TypeRef typeRef, NamespaceSymbol ns)
    {
        if (!typeRef.IsBuiltIn && typeRef.Name != "?")
        {
            _unresolvedRefs.Add((typeRef, ns));
        }
        foreach (var arg in typeRef.TypeArguments)
        {
            TrackTypeRef(arg, ns);
        }
    }

    private bool TryDefineType(NamespaceSymbol ns, string name)
    {
        int pos = _posMap.GetPositionOf(name);
        var fullName = ns.FullName.Length > 0 ? $"{ns.FullName}.{name}" : name;
        if (!_definedTypes.Add(fullName))
        {
            _diagnostics.Add(DiagnosticDescriptors.DuplicateDefinition(name, pos));
            return false;
        }

        // Case-insensitive collision check (WinRT type names are case-insensitive)
        if (_definedTypesCaseInsensitive.TryGetValue(fullName, out var existing))
        {
            if (existing != fullName)
            {
                var existingShortName = existing.Contains('.') ? existing[(existing.LastIndexOf('.') + 1)..] : existing;
                _diagnostics.Add(DiagnosticDescriptors.CaseInsensitiveTypeCollision(name, existingShortName, pos));
            }
        }
        else
        {
            _definedTypesCaseInsensitive[fullName] = fullName;
        }

        return true;
    }

    private static int FindSlotIndex(GreenNode node, SyntaxKind kind)
    {
        for (int i = 0; i < node.SlotCount; i++)
        {
            var slot = node.GetSlot(i);
            if (slot != null && slot.Kind == kind) return i;
        }
        return -1;
    }

    private static int FindLastSlotIndex(GreenNode node, SyntaxKind kind)
    {
        for (int i = node.SlotCount - 1; i >= 0; i--)
        {
            var slot = node.GetSlot(i);
            if (slot != null && slot.Kind == kind) return i;
        }
        return -1;
    }

    private static GreenNode? FindChild(GreenNode node, SyntaxKind kind)
    {
        for (int i = 0; i < node.SlotCount; i++)
        {
            var slot = node.GetSlot(i);
            if (slot != null && slot.Kind == kind) return slot;
        }
        return null;
    }

    /// <summary>
    /// Extract attributes from a green node, computing accurate source positions
    /// from the green tree FullWidth accumulation (not text-based lookup).
    /// </summary>
    private List<AttributeData> ExtractAttributes(GreenNode? attrListNode)
    {
        if (attrListNode == null) return [];
        int offset = AttributeExtractor.FindNodeOffset(_root, attrListNode) ?? 0;
        return AttributeExtractor.Extract(attrListNode, offset);
    }

    private static bool HasModifier(GreenNode modList, SyntaxKind modKind)
    {
        for (int i = 0; i < modList.SlotCount; i++)
        {
            if (modList.GetSlot(i) is SyntaxToken t && t.Kind == modKind) return true;
        }
        return false;
    }

    private static bool HasAttribute(GreenNode attrListNode, string attrName)
    {
        for (int i = 0; i < attrListNode.SlotCount; i++)
        {
            var slot = attrListNode.GetSlot(i);
            if (slot == null) continue;

            // Could be nested: AttributeList > AttributeList > Attribute
            if (slot.Kind == SyntaxKind.AttributeList)
            {
                if (HasAttribute(slot, attrName)) return true;
                continue;
            }

            if (slot.Kind == SyntaxKind.Attribute)
            {
                var nameNode = slot.GetSlot(0);
                var name = nameNode is SyntaxToken t ? t.Text : ExtractDottedName(nameNode);
                if (string.Equals(name, attrName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }
        return false;
    }

    private static bool TryExtractUuidAttribute(GreenNode attrListNode, out Guid uuid)
    {
        uuid = default;
        for (int i = 0; i < attrListNode.SlotCount; i++)
        {
            var slot = attrListNode.GetSlot(i);
            if (slot == null) continue;

            if (slot.Kind == SyntaxKind.AttributeList)
            {
                if (TryExtractUuidAttribute(slot, out uuid)) return true;
                continue;
            }

            if (slot.Kind == SyntaxKind.Attribute)
            {
                var nameNode = slot.GetSlot(0);
                var name = nameNode is SyntaxToken t ? t.Text : ExtractDottedName(nameNode);
                if (!string.Equals(name, "uuid", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Find AttributeArgumentList, then extract first string/guid literal
                for (int j = 1; j < slot.SlotCount; j++)
                {
                    var argList = slot.GetSlot(j);
                    if (argList == null || argList.Kind != SyntaxKind.AttributeArgumentList)
                        continue;

                    var guidText = ExtractFirstLiteralText(argList);
                    if (guidText != null && Guid.TryParse(guidText, out uuid))
                        return true;
                }
            }
        }
        return false;
    }

    private static string? ExtractFirstLiteralText(GreenNode node)
    {
        for (int i = 0; i < node.SlotCount; i++)
        {
            var slot = node.GetSlot(i);
            if (slot == null) continue;

            if (slot is SyntaxToken token)
            {
                if (token.Kind is SyntaxKind.StringLiteralToken or SyntaxKind.GuidLiteralToken)
                {
                    var text = token.Text;
                    // Strip surrounding quotes if present
                    if (text.Length >= 2 && text[0] == '"' && text[^1] == '"')
                        text = text[1..^1];
                    return text;
                }
            }

            // Recurse into AttributeArgument nodes
            var result = ExtractFirstLiteralText(slot);
            if (result != null) return result;
        }
        return null;
    }

    private static bool HasChildToken(GreenNode node, SyntaxKind kind)
    {
        for (int i = 0; i < node.SlotCount; i++)
        {
            if (node.GetSlot(i) is SyntaxToken t && t.Kind == kind) return true;
        }
        return false;
    }

    private void CheckSameArityOverload(IReadOnlyList<ISymbol> existingMembers, MethodSymbol newMethod)
    {
        if (string.IsNullOrEmpty(newMethod.Name)) return;
        foreach (var member in existingMembers)
        {
            if (member is MethodSymbol existing &&
                existing.Name == newMethod.Name &&
                existing.Parameters.Count == newMethod.Parameters.Count)
            {
                _diagnostics.Add(DiagnosticDescriptors.SameArityOverload(newMethod.Name, existing.Name, _posMap.GetPositionOf(newMethod.Name)));
                return;
            }
        }
    }

    private void CheckWriteOnlyProperty(PropertySymbol prop)
    {
        if (prop.HasSetter && !prop.HasGetter && !string.IsNullOrEmpty(prop.Name))
        {
            _diagnostics.Add(DiagnosticDescriptors.WriteOnlyProperty(prop.Name, _posMap.GetPositionOf(prop.Name)));
        }
    }

    private static bool IsTypeNode(SyntaxKind kind) =>
        kind is SyntaxKind.TypeReference or SyntaxKind.GenericName
            or SyntaxKind.QualifiedName or SyntaxKind.ArrayType
            or SyntaxKind.PointerType;

    private static bool IsIdentifierLike(SyntaxKind kind) =>
        kind == SyntaxKind.IdentifierToken || IsTypeKeywordOrNameable(kind);

    private static bool IsTypeKeywordOrNameable(SyntaxKind kind) =>
        kind is SyntaxKind.BooleanKeyword or SyntaxKind.CharKeyword
            or SyntaxKind.Int16Keyword or SyntaxKind.Int32Keyword or SyntaxKind.Int64Keyword
            or SyntaxKind.UInt8Keyword or SyntaxKind.UInt16Keyword or SyntaxKind.UInt32Keyword
            or SyntaxKind.UInt64Keyword or SyntaxKind.SingleKeyword or SyntaxKind.DoubleKeyword
            or SyntaxKind.StringKeyword or SyntaxKind.GuidKeyword or SyntaxKind.ObjectKeyword
            or SyntaxKind.VoidKeyword or SyntaxKind.HResultKeyword
            or SyntaxKind.EventKeyword or SyntaxKind.GetKeyword or SyntaxKind.SetKeyword
            or SyntaxKind.RequiresKeyword or SyntaxKind.ContractKeyword;

    #endregion

    #region Semantic validation (Pass 3)

    private void ValidateSemantics(NamespaceSymbol ns)
    {
        // Code quality: namespace casing
        if (_emitQualityHints) ValidateNamespaceCasing(ns);

        foreach (var member in ns.Members)
        {
            switch (member)
            {
                case RuntimeClassSymbol rc when !rc.IsExternal:
                    ValidateAccessModifiers(rc);
                    ValidateBaseClass(rc);
                    ValidateEventDelegateTypes(rc.Members);
                    ValidateDuplicateMethodNames(rc.Members);
                    ValidateStaticUnsealedConflict(rc);
                    ValidateDeprecatedInheritance(rc);
                    ValidateConstructorArityPerVersion(rc);
                    ValidateMethodParameters(rc.Members);
                    // Code quality hints
                    if (_emitQualityHints) ValidateRuntimeClassQuality(rc);
                    break;
                case InterfaceSymbol iface when !iface.IsExternal && !iface.IsSynthesized:
                    ValidateRequiredInterfaces(iface);
                    ValidateEventDelegateTypes(iface.Members);
                    ValidateInterfaceMemberNameUniqueness(iface);
                    ValidateMethodNameOverrideUniqueness(iface);
                    ValidateDefaultOverloadExactlyOne(iface.Members, iface.Name);
                    ValidateMethodParameters(iface.Members);
                    // Code quality hints
                    if (_emitQualityHints) ValidateInterfaceQuality(iface);
                    break;
                case StructSymbol s when !s.IsExternal:
                    ValidateStructFieldTypes(s);
                    break;
                case EnumSymbol en when !en.IsExternal:
                    if (en.IsFlags && en.UnderlyingType != "UInt32")
                        _diagnostics.Add(DiagnosticDescriptors.FlagsEnumMustBeUInt32(
                            en.Name, _posMap.GetPositionOf(en.Name)));
                    break;
                case DelegateSymbol d when !d.IsExternal:
                    ValidateDelegateReturnType(d);
                    ValidateMethodParametersForDelegate(d);
                    break;
            }
        }

        ValidateCircularStructReferences(ns);

        foreach (var child in ns.ChildNamespaces.Values)
        {
            ValidateSemantics(child);
        }
    }

    private void ValidateAccessModifiers(RuntimeClassSymbol rc)
    {
        if (rc.IsUnsealed) return;

        foreach (var member in rc.Members)
        {
            switch (member)
            {
                case MethodSymbol m:
                    if (m.IsProtected)
                        _diagnostics.Add(DiagnosticDescriptors.ProtectedOrOverridableOnSealedClass(
                            m.Name, "protected", rc.Name, _posMap.GetPositionOf(m.Name)));
                    if (m.IsOverridable)
                        _diagnostics.Add(DiagnosticDescriptors.ProtectedOrOverridableOnSealedClass(
                            m.Name, "overridable", rc.Name, _posMap.GetPositionOf(m.Name)));
                    break;
                case PropertySymbol p:
                    if (p.IsProtected)
                        _diagnostics.Add(DiagnosticDescriptors.ProtectedOrOverridableOnSealedClass(
                            p.Name, "protected", rc.Name, _posMap.GetPositionOf(p.Name)));
                    if (p.IsOverridable)
                        _diagnostics.Add(DiagnosticDescriptors.ProtectedOrOverridableOnSealedClass(
                            p.Name, "overridable", rc.Name, _posMap.GetPositionOf(p.Name)));
                    break;
                case EventSymbol e:
                    if (e.IsProtected)
                        _diagnostics.Add(DiagnosticDescriptors.ProtectedOrOverridableOnSealedClass(
                            e.Name, "protected", rc.Name, _posMap.GetPositionOf(e.Name)));
                    if (e.IsOverridable)
                        _diagnostics.Add(DiagnosticDescriptors.ProtectedOrOverridableOnSealedClass(
                            e.Name, "overridable", rc.Name, _posMap.GetPositionOf(e.Name)));
                    break;
            }
        }
    }

    private void ValidateBaseClass(RuntimeClassSymbol rc)
    {
        if (rc.BaseClass == null || rc.BaseClass.ResolvedSymbol == null) return;

        var baseSymbol = rc.BaseClass.ResolvedSymbol;
        if (baseSymbol is RuntimeClassSymbol baseRc)
        {
            if (!baseRc.IsUnsealed)
                _diagnostics.Add(DiagnosticDescriptors.BaseClassSealed(
                    rc.BaseClass.Name, rc.BaseClass.Position));
        }
        else if (baseSymbol is not InterfaceSymbol)
        {
            // Base resolves to a struct, enum, delegate, etc. — not valid
            _diagnostics.Add(DiagnosticDescriptors.BaseNotRuntimeClass(
                rc.BaseClass.Name, rc.BaseClass.Position));
        }
    }

    private void ValidateRequiredInterfaces(InterfaceSymbol iface)
    {
        foreach (var req in iface.RequiredInterfaces)
        {
            if (req.ResolvedSymbol == null)
            {
                // Only emit if TypeResolver didn't already report MIDL1001
                // Check: if the name is not built-in and not "?", it was already reported
                // We emit our own more specific diagnostic
                _diagnostics.Add(DiagnosticDescriptors.UndefinedRequiredInterface(
                    req.Name, req.Position));
            }
            else if (req.ResolvedSymbol is RuntimeClassSymbol)
            {
                _diagnostics.Add(DiagnosticDescriptors.RuntimeClassOnRequiresList(
                    req.Name, req.Position));
            }
        }
    }

    /// <summary>MIDL1042: Validate that event types resolve to delegate types.</summary>
    private void ValidateEventDelegateTypes(IReadOnlyList<ISymbol> members)
    {
        foreach (var member in members)
        {
            if (member is EventSymbol evt)
            {
                // Built-in types (Int32, String, etc.) are never delegates
                if (evt.DelegateType.IsBuiltIn)
                {
                    _diagnostics.Add(DiagnosticDescriptors.EventWithNonDelegateType(
                        evt.DelegateType.Name, evt.DelegateType.Position));
                }
                // Resolved to a non-delegate symbol
                else if (evt.DelegateType.ResolvedSymbol != null
                    && evt.DelegateType.ResolvedSymbol is not DelegateSymbol)
                {
                    _diagnostics.Add(DiagnosticDescriptors.EventWithNonDelegateType(
                        evt.DelegateType.Name, evt.DelegateType.Position));
                }
            }
        }
    }

    /// <summary>MIDL1044: Warn when methods share a name but neither has [overload].</summary>
    private void ValidateDuplicateMethodNames(IReadOnlyList<ISymbol> members)
    {
        var methodsByName = new Dictionary<string, List<MethodSymbol>>(StringComparer.Ordinal);
        foreach (var member in members)
        {
            if (member is MethodSymbol m && !string.IsNullOrEmpty(m.Name))
            {
                if (!methodsByName.TryGetValue(m.Name, out var list))
                {
                    list = [];
                    methodsByName[m.Name] = list;
                }
                list.Add(m);
            }
        }

        foreach (var (name, methods) in methodsByName)
        {
            if (methods.Count > 1 && methods.TrueForAll(m => string.IsNullOrEmpty(m.OverloadGroup)))
            {
                // Emit on the second method (first duplicate)
                _diagnostics.Add(DiagnosticDescriptors.DuplicateMethodNameWithoutOverload(
                    name, _posMap.GetPositionOf(name)));
            }
        }
    }

    /// <summary>MIDL1045: Detect circular references between struct field types.</summary>
    private void ValidateCircularStructReferences(NamespaceSymbol ns)
    {
        var structs = ns.Members.OfType<StructSymbol>().Where(s => !s.IsExternal).ToList();
        if (structs.Count == 0) return;

        var visiting = new HashSet<string>();
        var visited = new HashSet<string>();
        var reported = new HashSet<string>();

        foreach (var s in structs)
        {
            DetectStructCycle(s, visiting, visited, reported);
        }
    }

    private void DetectStructCycle(StructSymbol current, HashSet<string> visiting,
        HashSet<string> visited, HashSet<string> reported)
    {
        if (visited.Contains(current.FullName)) return;
        if (!visiting.Add(current.FullName)) return; // Already visiting — cycle found elsewhere

        foreach (var field in current.Fields)
        {
            if (field.Type.ResolvedSymbol is StructSymbol fieldStruct && !fieldStruct.IsExternal)
            {
                if (visiting.Contains(fieldStruct.FullName))
                {
                    // Cycle detected
                    var key = string.Compare(current.FullName, fieldStruct.FullName, StringComparison.Ordinal) < 0
                        ? $"{current.FullName}<>{fieldStruct.FullName}"
                        : $"{fieldStruct.FullName}<>{current.FullName}";
                    if (reported.Add(key))
                    {
                        _diagnostics.Add(DiagnosticDescriptors.CircularStructReference(
                            current.Name, fieldStruct.Name, _posMap.GetPositionOf(current.Name)));
                    }
                }
                else
                {
                    DetectStructCycle(fieldStruct, visiting, visited, reported);
                }
            }
        }

        visiting.Remove(current.FullName);
        visited.Add(current.FullName);
    }


    private static readonly HashSet<string> AllowedStructFieldBuiltIns = new(StringComparer.Ordinal) { "Boolean","Char","Int16","Int32","Int64","UInt8","UInt16","UInt32","UInt64","Single","Double","String","Guid" };
    private void ValidateStructFieldTypes(StructSymbol s) { foreach (var field in s.Fields) if (!IsAllowedStructFieldType(field.Type)) _diagnostics.Add(DiagnosticDescriptors.StructFieldNotValueType(field.Name, _posMap.GetPositionOf(field.Name))); }
    private static bool IsAllowedStructFieldType(TypeRef typeRef) { if (AllowedStructFieldBuiltIns.Contains(typeRef.Name)) return true; if (typeRef.Name == "IReference" && typeRef.TypeArguments.Count > 0) return true; if (typeRef.ResolvedSymbol is EnumSymbol or StructSymbol) return true; return false; }
    private void ValidateMethodParameters(IReadOnlyList<ISymbol> members) { foreach (var member in members) { List<ParameterSymbol>? p = null; TypeRef? rt = null; if (member is MethodSymbol m) { p = m.Parameters; rt = m.ReturnType; } else if (member is ConstructorSymbol c) { p = c.Parameters; } if (p != null) foreach (var param in p) ValidateParameter(param); if (_mode == CompilationMode.Strict && rt != null) CheckWinRtTypeCompatibility(rt); } }
    private void ValidateMethodParametersForDelegate(DelegateSymbol d) { foreach (var param in d.Parameters) ValidateParameter(param); if (_mode == CompilationMode.Strict) CheckWinRtTypeCompatibility(d.ReturnType); }
    private void ValidateParameter(ParameterSymbol param) { if (param.Direction == ParameterDirection.Ref) _diagnostics.Add(DiagnosticDescriptors.NoBidirectionalParams(param.Name, _posMap.GetPositionOf(param.Name))); if (param.IsArray && param.Type.TypeArguments.Count > 0 && param.Type.TypeArguments.Any(a => a.IsArray)) _diagnostics.Add(DiagnosticDescriptors.NoArrayOfArrays(param.Name, _posMap.GetPositionOf(param.Name))); if (_mode == CompilationMode.Strict) CheckWinRtTypeCompatibility(param.Type); }
    private static readonly HashSet<string> COnlyTypes = new(StringComparer.Ordinal) { "wchar_t","handle_t","error_status_t" };
    private void CheckWinRtTypeCompatibility(TypeRef typeRef) { if (typeRef == TypeRef.Void || typeRef == TypeRef.Unknown) return; if (COnlyTypes.Contains(typeRef.Name)) _diagnostics.Add(DiagnosticDescriptors.WinRtTypeNotAllowed(typeRef.Name, typeRef.Position)); }
    private void ValidateDelegateReturnType(DelegateSymbol d) { if (d.ReturnType.Name is "void" or "Void") return; if (_mode == CompilationMode.Compat && d.ReturnType.Name is "HRESULT") return; _diagnostics.Add(DiagnosticDescriptors.DelegateReturnMustBeVoid(d.Name, d.ReturnType.DisplayName, _posMap.GetPositionOf(d.Name))); }
    private void ValidateInterfaceMemberNameUniqueness(InterfaceSymbol iface) { var seen = new Dictionary<string, ISymbol>(StringComparer.Ordinal); foreach (var member in iface.Members) { var name = member.Name; if (string.IsNullOrEmpty(name)) continue; if (member is MethodSymbol m && !string.IsNullOrEmpty(m.OverloadGroup)) continue; if (seen.TryGetValue(name, out var existing)) { if (existing is MethodSymbol em && !string.IsNullOrEmpty(em.OverloadGroup)) continue; _diagnostics.Add(DiagnosticDescriptors.InterfaceMemberNameCollision(name, iface.Name, _posMap.GetPositionOf(name))); } else seen[name] = member; } }
    private void ValidateDefaultOverloadExactlyOne(IReadOnlyList<ISymbol> members, string cn) { var g = new Dictionary<(string,int), List<MethodSymbol>>(); foreach (var member in members) if (member is MethodSymbol m && !string.IsNullOrEmpty(m.Name)) { var k = (m.Name, m.Parameters.Count); if (!g.TryGetValue(k, out var l)) { l = []; g[k] = l; } l.Add(m); } foreach (var (k, ms) in g) { if (ms.Count < 2) continue; var dc = ms.Count(m => m.IsDefaultOverload); if (dc == 0) _diagnostics.Add(DiagnosticDescriptors.SameArityNeedsDefaultOverload(k.Item1, k.Item2, _posMap.GetPositionOf(k.Item1))); else if (dc > 1) _diagnostics.Add(DiagnosticDescriptors.DefaultOverloadNotExactlyOne(k.Item1, k.Item2, dc, _posMap.GetPositionOf(k.Item1))); } }
    private void ValidateDeprecatedInheritance(RuntimeClassSymbol rc) { if (rc.BaseClass?.ResolvedSymbol is not RuntimeClassSymbol baseRc) return; if (string.IsNullOrEmpty(baseRc.DeprecatedMessage)) return; if (!string.IsNullOrEmpty(rc.DeprecatedMessage)) return; _diagnostics.Add(DiagnosticDescriptors.DeprecatedBaseClassInheritance(rc.Name, baseRc.Name, _posMap.GetPositionOf(rc.Name))); }
    private void ValidateMethodNameOverrideUniqueness(InterfaceSymbol iface) { var seen = new Dictionary<string, string>(StringComparer.Ordinal); foreach (var member in iface.Members) { if (member is not MethodSymbol m || string.IsNullOrEmpty(m.MethodNameOverride)) continue; if (seen.ContainsKey(m.MethodNameOverride)) _diagnostics.Add(DiagnosticDescriptors.DuplicateMethodNameOverride(m.MethodNameOverride, iface.Name, _posMap.GetPositionOf(m.Name))); else seen[m.MethodNameOverride] = m.Name; } }
    private void ValidateConstructorArityPerVersion(RuntimeClassSymbol rc) { var ctors = rc.Members.OfType<ConstructorSymbol>().ToList(); if (ctors.Count < 2) return; var g = new Dictionary<(int,int?), List<ConstructorSymbol>>(); foreach (var ctor in ctors) { var k = (ctor.Parameters.Count, ctor.ContractVersion); if (!g.TryGetValue(k, out var l)) { l = []; g[k] = l; } l.Add(ctor); } foreach (var (k, l) in g) if (l.Count > 1) _diagnostics.Add(DiagnosticDescriptors.ConstructorArityConflict(rc.Name, k.Item1, _posMap.GetPositionOf(rc.Name))); }
    private void ValidateStaticUnsealedConflict(RuntimeClassSymbol rc) { if (rc.IsStatic && rc.IsUnsealed) _diagnostics.Add(DiagnosticDescriptors.StaticUnsealedConflict(rc.Name, _posMap.GetPositionOf(rc.Name))); }

        private static bool IsTypeDeclaration(SyntaxKind kind) => kind is
        SyntaxKind.RuntimeClassDeclaration or SyntaxKind.InterfaceDeclaration
        or SyntaxKind.DelegateDeclaration or SyntaxKind.StructDeclaration
        or SyntaxKind.EnumDeclaration or SyntaxKind.ApiContractDeclaration
        or SyntaxKind.AttributeDeclaration;

    private static string? ExtractTypeName(GreenNode node)
    {
        // Type declarations typically have: [attrs] keyword Name { ... }
        // Find the identifier after the keyword
        for (int i = 0; i < node.SlotCount - 1; i++)
        {
            var slot = node.GetSlot(i);
            if (slot is SyntaxToken { Kind: SyntaxKind.RuntimeclassKeyword or SyntaxKind.InterfaceKeyword
                or SyntaxKind.DelegateKeyword or SyntaxKind.StructKeyword
                or SyntaxKind.EnumKeyword or SyntaxKind.ApicontractKeyword
                or SyntaxKind.AttributeKeyword })
            {
                var next = node.GetSlot(i + 1) as SyntaxToken;
                if (next is { Kind: SyntaxKind.IdentifierToken })
                    return next.Text;
            }
        }
        return null;
    }

    #endregion

    #region Code Quality Hints

    private void ValidateNamespaceCasing(NamespaceSymbol ns)
    {
        if (string.IsNullOrEmpty(ns.Name) || ns.IsExternal) return;
        foreach (var segment in ns.Name.Split('.'))
        {
            if (segment.Length > 0 && char.IsLower(segment[0]))
            {
                _diagnostics.Add(DiagnosticDescriptors.NamespaceCasingConvention(
                    segment, _posMap.GetPositionOf(segment)));
            }
        }
    }

    private void ValidateInterfaceQuality(InterfaceSymbol iface)
    {
        // MIDL3001: Interface should start with 'I'
        if (iface.Name.Length > 0 && iface.Name[0] != 'I')
        {
            _diagnostics.Add(DiagnosticDescriptors.InterfaceNamingConvention(
                iface.Name, _posMap.GetPositionOf(iface.Name)));
        }
        else if (iface.Name.Length > 1 && iface.Name[0] == 'I' && char.IsLower(iface.Name[1]))
        {
            // "Ifoo" — the 'I' prefix should be followed by uppercase
            _diagnostics.Add(DiagnosticDescriptors.InterfaceNamingConvention(
                iface.Name, _posMap.GetPositionOf(iface.Name)));
        }

        // MIDL3003: Empty interface
        if (iface.Members.Count == 0)
        {
            _diagnostics.Add(DiagnosticDescriptors.EmptyInterface(
                iface.Name, _posMap.GetPositionOf(iface.Name)));
        }
    }

    private void ValidateRuntimeClassQuality(RuntimeClassSymbol rc)
    {
        bool hasMembers = rc.Members.Count > 0;
        bool hasConstructors = rc.Members.OfType<ConstructorSymbol>().Any();
        bool hasActivatable = rc.Attributes.Any(a =>
            a.Name.Equals("activatable", StringComparison.OrdinalIgnoreCase));

        // MIDL3002: Empty runtimeclass
        if (!hasMembers && !rc.IsStatic)
        {
            _diagnostics.Add(DiagnosticDescriptors.EmptyRuntimeClass(
                rc.Name, _posMap.GetPositionOf(rc.Name)));
        }

        // MIDL3005: No constructor and not activatable
        if (!hasConstructors && !hasActivatable && !rc.IsStatic && hasMembers)
        {
            _diagnostics.Add(DiagnosticDescriptors.RuntimeClassNoConstructor(
                rc.Name, _posMap.GetPositionOf(rc.Name)));
        }

        // MIDL3007: Setter-only property
        foreach (var prop in rc.Members.OfType<PropertySymbol>())
        {
            if (prop.HasSetter && !prop.HasGetter)
            {
                _diagnostics.Add(DiagnosticDescriptors.PropertySetterOnly(
                    prop.Name, rc.Name, _posMap.GetPositionOf(prop.Name)));
            }
        }

        // MIDL3008: Method name clashes with type name
        foreach (var method in rc.Members.OfType<MethodSymbol>())
        {
            if (string.Equals(method.Name, rc.Name, StringComparison.OrdinalIgnoreCase))
            {
                _diagnostics.Add(DiagnosticDescriptors.MethodNameClash(
                    method.Name, rc.Name, _posMap.GetPositionOf(method.Name)));
            }
        }
    }

    #endregion
}
