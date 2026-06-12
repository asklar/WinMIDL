// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis;

/// <summary>
/// Resolves TypeRef objects against the symbol table.
/// </summary>
public class TypeResolver
{
    private readonly SymbolTable _symbolTable;

    // Cached candidate set for FindClosestMatch (built once per TypeResolver instance)
    private HashSet<string>? _cachedCandidates;
    // Pre-grouped candidates by (firstCharLower, length) for fast filtering
    private Dictionary<(char, int), List<string>>? _candidatesByCharLen;

    /// <summary>
    /// Known WinRT parameterized types and their expected generic arity.
    /// </summary>
    private static readonly Dictionary<string, int> s_parameterizedTypeArity = new(StringComparer.Ordinal)
    {
        ["Windows.Foundation.IAsyncOperation"] = 1,
        ["Windows.Foundation.IAsyncOperationWithProgress"] = 2,
        ["Windows.Foundation.IAsyncActionWithProgress"] = 1,
        ["Windows.Foundation.Collections.IVector"] = 1,
        ["Windows.Foundation.Collections.IVectorView"] = 1,
        ["Windows.Foundation.Collections.IIterable"] = 1,
        ["Windows.Foundation.Collections.IIterator"] = 1,
        ["Windows.Foundation.Collections.IMap"] = 2,
        ["Windows.Foundation.Collections.IMapView"] = 2,
        ["Windows.Foundation.Collections.IKeyValuePair"] = 2,
        ["Windows.Foundation.Collections.IObservableVector"] = 1,
        ["Windows.Foundation.Collections.IObservableMap"] = 2,
        ["Windows.Foundation.IReference"] = 1,
        ["Windows.Foundation.EventHandler"] = 1,
        ["Windows.Foundation.TypedEventHandler"] = 2,
    };

    /// <summary>Built-in WinRT primitive type names for suggestion matching.</summary>
    private static readonly string[] s_builtInTypeNames =
    [
        "Boolean", "Char", "Char16", "Int16", "Int32", "Int64",
        "UInt8", "UInt16", "UInt32", "UInt64",
        "Single", "Double", "String", "Guid", "Object",
    ];

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

        // Validate that parameterized types have type arguments
        foreach (var (typeRef, _) in unresolvedRefs)
        {
            if (typeRef.ResolvedSymbol is { IsExternal: true } &&
                typeRef.TypeArguments.Count == 0 &&
                s_parameterizedTypeArity.ContainsKey(typeRef.ResolvedSymbol.FullName))
            {
                diagnostics.Add(DiagnosticDescriptors.GenericTypeRequiresArguments(
                    typeRef.Name, typeRef.Position));
            }
        }
    }

    public void Resolve(TypeRef typeRef, NamespaceSymbol containingNamespace, List<Diagnostic> diagnostics)
    {
        if (typeRef.IsBuiltIn || typeRef.ResolvedSymbol != null)
            return;

        if (typeRef.Name == "?" || string.IsNullOrEmpty(typeRef.Name))
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

            diagnostics.Add(DiagnosticDescriptors.UndefinedType(typeRef.Name, typeRef.Position,
                FindClosestMatch(typeRef.Name)));
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

        // Try lazy WinMD providers by walking up the namespace chain
        result = _symbolTable.TryResolveLazyBySimpleName(typeRef.Name, containingNamespace);
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

        diagnostics.Add(DiagnosticDescriptors.UndefinedType(typeRef.Name, typeRef.Position,
            FindClosestMatch(typeRef.Name)));
    }

    /// <summary>
    /// Finds the closest matching type name using Levenshtein edit distance.
    /// Returns null if no match is close enough.
    /// Caches the candidate set so repeated calls don't rebuild it.
    /// </summary>
    internal string? FindClosestMatch(string name)
    {
        // Scale threshold proportionally to name length (~20-25% error rate)
        int maxDistance = Math.Max(2, name.Length / 4);
        string? bestMatch = null;
        int bestDist = int.MaxValue;

        EnsureCandidatesCached();
        var candidates = _cachedCandidates!;

        // For qualified names, match the type part against types in the same namespace
        if (name.Contains('.'))
        {
            var lastDot = name.LastIndexOf('.');
            var namespacePart = name[..lastDot];
            var typePart = name[(lastDot + 1)..];
            var typePartLower = typePart.ToLowerInvariant();

            foreach (var candidate in candidates)
            {
                if (!candidate.Contains('.')) continue;
                var candDot = candidate.LastIndexOf('.');
                var candNs = candidate[..candDot];
                var candType = candidate[(candDot + 1)..];

                if (string.Equals(candNs, namespacePart, StringComparison.OrdinalIgnoreCase))
                {
                    var effectiveMaxQ = Math.Max(3, Math.Max(typePart.Length, candType.Length) / 3);
                    // Early exit on length difference
                    if (Math.Abs(typePart.Length - candType.Length) > effectiveMaxQ) continue;
                    var dist = EditDistance(typePartLower, candType.ToLowerInvariant(), effectiveMaxQ);
                    if (dist <= effectiveMaxQ && dist <= Math.Max(typePart.Length / 2, 2) && dist < bestDist)
                    {
                        bestDist = dist;
                        bestMatch = candidate;
                    }
                }
            }
        }

        // Also try simple name matching (case-insensitive) across plausible candidates
        var nameLower = name.Contains('.') ? name[(name.LastIndexOf('.') + 1)..].ToLowerInvariant() : name.ToLowerInvariant();

        // Use pre-grouped candidates: only check candidates within plausible length range
        var nameLen = nameLower.Length;
        var grouped = _candidatesByCharLen!;
        foreach (var candidate in candidates)
        {
            if (string.Equals(candidate, name, StringComparison.Ordinal)) continue;

            // Compare case-insensitively using simple name
            var candSimple = candidate.Contains('.') ? candidate[(candidate.LastIndexOf('.') + 1)..] : candidate;

            // Early exit on length difference before computing edit distance
            var effectiveMax = Math.Max(3, Math.Max(nameLen, candSimple.Length) / 3);
            if (Math.Abs(nameLen - candSimple.Length) > effectiveMax) continue;

            var candLower = candSimple.ToLowerInvariant();
            var dist = EditDistance(nameLower, candLower, effectiveMax);
            if (dist <= effectiveMax && dist < Math.Max(nameLower.Length / 2, 2) && dist < bestDist)
            {
                bestDist = dist;
                bestMatch = candidate;
            }
        }


        return bestMatch;
    }

    /// <summary>
    /// Builds and caches the candidate set on first use.
    /// The type universe doesn't change during one compilation.
    /// </summary>
    private void EnsureCandidatesCached()
    {
        if (_cachedCandidates != null) return;

        _cachedCandidates = new HashSet<string>(StringComparer.Ordinal);

        // Built-in primitive types
        foreach (var builtIn in s_builtInTypeNames)
            _cachedCandidates.Add(builtIn);

        // Shorthand types, simple names of defined types, and aliases
        foreach (var candidate in _symbolTable.GetAllCandidateTypeNames())
            _cachedCandidates.Add(candidate);

        // Fully-qualified names from defined types (for qualified name matching)
        foreach (var candidate in _symbolTable.GetAllDefinedTypeFullNames())
            _cachedCandidates.Add(candidate);

        // Fully-qualified names from lazy WinMD providers
        foreach (var candidate in _symbolTable.GetAllLazyTypeNames())
            _cachedCandidates.Add(candidate);

        // Pre-group by simple name first char and length for fast filtering
        _candidatesByCharLen = new Dictionary<(char, int), List<string>>();
        foreach (var candidate in _cachedCandidates)
        {
            var simple = candidate.Contains('.') ? candidate[(candidate.LastIndexOf('.') + 1)..] : candidate;
            if (simple.Length == 0) continue;
            var key = (char.ToLowerInvariant(simple[0]), simple.Length);
            if (!_candidatesByCharLen.TryGetValue(key, out var list))
            {
                list = new List<string>();
                _candidatesByCharLen[key] = list;
            }
            list.Add(candidate);
        }
    }

    /// <summary>
    /// Computes Levenshtein edit distance with early termination.
    /// </summary>
    internal static int EditDistance(string a, string b, int maxDist = 3)
    {
        var lenA = a.Length;
        var lenB = b.Length;

        if (Math.Abs(lenA - lenB) > maxDist)
            return maxDist + 1;

        var prev = new int[lenB + 1];
        var curr = new int[lenB + 1];

        for (int j = 0; j <= lenB; j++)
            prev[j] = j;

        for (int i = 1; i <= lenA; i++)
        {
            curr[0] = i;
            int rowMin = curr[0];

            for (int j = 1; j <= lenB; j++)
            {
                int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                curr[j] = Math.Min(
                    Math.Min(prev[j] + 1, curr[j - 1] + 1),
                    prev[j - 1] + cost);
                rowMin = Math.Min(rowMin, curr[j]);
            }

            if (rowMin > maxDist)
                return maxDist + 1;

            (prev, curr) = (curr, prev);
        }

        return prev[lenB];
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
