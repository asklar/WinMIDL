using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis;

/// <summary>
/// Resolves TypeRef objects against the symbol table.
/// </summary>
public class TypeResolver
{
    private readonly SymbolTable _symbolTable;

    public TypeResolver(SymbolTable symbolTable)
    {
        _symbolTable = symbolTable;
    }

    public void ResolveAll(
        IReadOnlyList<(TypeRef TypeRef, NamespaceSymbol ContainingNamespace)> unresolvedRefs,
        List<Diagnostic> diagnostics)
    {
        foreach (var (typeRef, ns) in unresolvedRefs)
        {
            Resolve(typeRef, ns, diagnostics);
        }
    }

    public void Resolve(TypeRef typeRef, NamespaceSymbol containingNamespace, List<Diagnostic> diagnostics)
    {
        if (typeRef.IsBuiltIn || typeRef.ResolvedSymbol != null)
            return;

        if (typeRef.Name == "?")
            return;

        // Resolve type arguments first
        foreach (var arg in typeRef.TypeArguments)
        {
            Resolve(arg, containingNamespace, diagnostics);
        }

        // Qualified name: split and walk
        if (typeRef.Name.Contains('.'))
        {
            var symbol = _symbolTable.LookupQualified(typeRef.Name);
            if (symbol != null)
            {
                typeRef.ResolvedSymbol = symbol;
                return;
            }

            diagnostics.Add(DiagnosticDescriptors.UndefinedType(typeRef.Name, typeRef.Position));
            return;
        }

        // Try lookup in containing namespace chain
        var result = LookupInNamespaceChain(typeRef.Name, containingNamespace);
        if (result != null)
        {
            typeRef.ResolvedSymbol = result;
            return;
        }

        // Try shorthand (IVector → Windows.Foundation.Collections.IVector)
        result = _symbolTable.LookupWithShorthand(typeRef.Name);
        if (result != null)
        {
            typeRef.ResolvedSymbol = result;
            return;
        }

        // Try type aliases (HSTRING → String, DWORD → UInt32, etc.)
        var aliasTarget = _symbolTable.ResolveAlias(typeRef.Name);
        if (aliasTarget != null)
        {
            // Try to resolve the alias target as a symbol
            var targetSymbol = _symbolTable.LookupQualified(aliasTarget)
                            ?? LookupInNamespaceChain(aliasTarget, containingNamespace);
            if (targetSymbol != null)
            {
                typeRef.ResolvedSymbol = targetSymbol;
                return;
            }

            // Alias targets a built-in primitive type (String, UInt32, etc.)
            if (TypeRef.IsBuiltInType(aliasTarget))
                return;
        }

        diagnostics.Add(DiagnosticDescriptors.UndefinedType(typeRef.Name, typeRef.Position));
    }

    private ISymbol? LookupInNamespaceChain(string name, NamespaceSymbol ns)
    {
        NamespaceSymbol? current = ns;
        while (current != null)
        {
            // Check types in this namespace
            foreach (var member in current.Members)
            {
                if (member.Name == name && member.Kind != SymbolKind.Namespace)
                    return member;
            }

            // Check child namespaces
            if (current.ChildNamespaces.TryGetValue(name, out var childNs))
                return childNs;

            current = current.ContainingSymbol as NamespaceSymbol;
        }

        return null;
    }
}
