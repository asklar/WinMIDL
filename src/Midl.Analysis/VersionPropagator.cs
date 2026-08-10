// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis;

/// <summary>
/// Propagates version/contract info through the symbol hierarchy and validates
/// version constraints after interface synthesis.
/// </summary>
public class VersionPropagator
{
    private readonly List<Diagnostic> _diagnostics;

    public VersionPropagator(List<Diagnostic> diagnostics)
    {
        _diagnostics = diagnostics;
    }

    public void Propagate(Compilation compilation)
    {
        var _unitSourceMaps = compilation.UnitSourceMaps;
        // Build a lookup of all ApiContract symbols (by simple name and full name)
        var contracts = new Dictionary<string, ApiContractSymbol>(StringComparer.OrdinalIgnoreCase);
        foreach (var type in compilation.GetAllTypes())
        {
            if (type is ApiContractSymbol ac)
            {
                contracts[ac.Name] = ac;
                contracts[ac.FullName] = ac;
            }
        }

        foreach (var type in compilation.GetAllTypes().ToList())
        {
            // Types reached through an import report at offsets in their own file.
            int diagStart = _diagnostics.Count;
            switch (type)
            {
                case RuntimeClassSymbol rc:
                    ValidateContractReference(rc.ContractName, rc.Attributes, contracts);
                    PropagateToSynthesizedInterfaces(rc);
                    PropagateToMembers(rc);
                    ValidateRuntimeClassInterfaces(rc, contracts);
                    break;

                case EnumSymbol es:
                    ValidateContractReference(es.ContractName, es.Attributes, contracts);
                    ValidateEnumMemberVersions(es, contracts);
                    break;

                case InterfaceSymbol iface when !iface.IsSynthesized:
                    ValidateContractReference(iface.ContractName, iface.Attributes, contracts);
                    break;
            }
            DiagnosticSourceTagger.Tag(_diagnostics, diagStart, type.SourceFile, _unitSourceMaps);
        }
    }

    /// <summary>
    /// Rule 1: Validate that a referenced contract name exists in the symbol table.
    /// Only validates contracts that could be locally defined (not fully-qualified external references).
    /// </summary>
    private void ValidateContractReference(
        string? contractName, List<AttributeData> attributes,
        Dictionary<string, ApiContractSymbol> contracts)
    {
        if (contractName == null) return;
        if (contracts.ContainsKey(contractName)) return;

        // Fully-qualified contract names (containing '.') typically refer to external
        // contracts from .winmd references — skip validation for those.
        if (contractName.Contains('.')) return;

        // Find the position from the contract attribute
        var contractAttr = attributes.FirstOrDefault(
            a => a.Name.Equals("contract", StringComparison.OrdinalIgnoreCase));
        int position = contractAttr?.Position ?? 0;

        _diagnostics.Add(DiagnosticDescriptors.UndefinedContract(contractName, position));
    }

    /// <summary>
    /// Rule 2: Synthesized interfaces inherit the class's contract/version if they don't have their own.
    /// </summary>
    private static void PropagateToSynthesizedInterfaces(RuntimeClassSymbol rc)
    {
        if (rc.ContractName == null) return;

        foreach (var iface in rc.ImplementedInterfaces)
        {
            if (iface.IsSynthesized && iface.ContractName == null)
            {
                iface.ContractName = rc.ContractName;
                iface.ContractVersion = rc.ContractVersion;
            }
        }
    }

    /// <summary>
    /// Rule 3: Members without explicit [contract] inherit the enclosing class's contract.
    /// </summary>
    private static void PropagateToMembers(RuntimeClassSymbol rc)
    {
        if (rc.ContractName == null) return;

        foreach (var member in rc.Members)
        {
            switch (member)
            {
                case MethodSymbol m when m.ContractName == null:
                    m.ContractName = rc.ContractName;
                    m.ContractVersion = rc.ContractVersion;
                    break;
                case PropertySymbol p when p.ContractName == null:
                    p.ContractName = rc.ContractName;
                    p.ContractVersion = rc.ContractVersion;
                    break;
                case EventSymbol e when e.ContractName == null:
                    e.ContractName = rc.ContractName;
                    e.ContractVersion = rc.ContractVersion;
                    break;
                case ConstructorSymbol c when c.ContractName == null:
                    c.ContractName = rc.ContractName;
                    c.ContractVersion = rc.ContractVersion;
                    break;
            }
        }
    }

    /// <summary>
    /// Rules 5 and 6: Validate interface versions and contract families for a runtimeclass.
    /// </summary>
    private void ValidateRuntimeClassInterfaces(
        RuntimeClassSymbol rc, Dictionary<string, ApiContractSymbol> contracts)
    {
        if (rc.ContractName == null) return;

        // Collect all contracts the class uses (from its own attributes)
        var classContracts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        classContracts.Add(rc.ContractName);
        foreach (var attr in rc.Attributes)
        {
            if (attr.Name.Equals("contract", StringComparison.OrdinalIgnoreCase))
            {
                var name = attr.GetTypeName(0) ?? attr.GetIdentifier(0);
                if (name != null) classContracts.Add(name);
            }
        }

        // Check synthesized interfaces
        foreach (var iface in rc.ImplementedInterfaces)
        {
            ValidateInterfaceForClass(iface, rc, classContracts);
        }

        // Check declared interfaces from the base list
        if (rc.BaseClass?.ResolvedSymbol is InterfaceSymbol baseIface)
        {
            ValidateInterfaceForClass(baseIface, rc, classContracts);
        }

        foreach (var typeRef in rc.Interfaces)
        {
            if (typeRef.ResolvedSymbol is InterfaceSymbol declaredIface)
            {
                ValidateInterfaceForClass(declaredIface, rc, classContracts);
            }
        }
    }

    private void ValidateInterfaceForClass(
        InterfaceSymbol iface, RuntimeClassSymbol rc, HashSet<string> classContracts)
    {
        if (iface.ContractName == null) return;

        int ifacePosition = 0;
        var ifaceContractAttr = iface.Attributes.FirstOrDefault(
            a => a.Name.Equals("contract", StringComparison.OrdinalIgnoreCase));
        if (ifaceContractAttr != null)
            ifacePosition = ifaceContractAttr.Position;

        // Rule 5: Interface version >= class version (same contract family)
        if (iface.ContractName.Equals(rc.ContractName, StringComparison.OrdinalIgnoreCase)
            && iface.ContractVersion.HasValue && rc.ContractVersion.HasValue
            && iface.ContractVersion.Value < rc.ContractVersion.Value)
        {
            _diagnostics.Add(DiagnosticDescriptors.InterfaceVersionTooEarly(
                iface.Name, iface.ContractVersion.Value, rc.ContractVersion.Value, ifacePosition));
        }

        // Rule 6: Contract family consistency
        if (!classContracts.Contains(iface.ContractName))
        {
            _diagnostics.Add(DiagnosticDescriptors.ContractFamilyMismatch(
                iface.Name, iface.ContractName, rc.Name, ifacePosition));
        }
    }

    /// <summary>
    /// Rule 7: Enum member version >= enum version.
    /// </summary>
    private void ValidateEnumMemberVersions(
        EnumSymbol es, Dictionary<string, ApiContractSymbol> contracts)
    {
        if (!es.ContractVersion.HasValue) return;

        foreach (var member in es.EnumMembers)
        {
            ValidateContractReference(member.ContractName, member.Attributes, contracts);

            if (member.ContractVersion.HasValue
                && member.ContractVersion.Value < es.ContractVersion.Value)
            {
                int position = 0;
                var memberAttr = member.Attributes.FirstOrDefault(
                    a => a.Name.Equals("contract", StringComparison.OrdinalIgnoreCase));
                if (memberAttr != null) position = memberAttr.Position;

                _diagnostics.Add(DiagnosticDescriptors.EnumMemberVersionTooEarly(
                    member.Name, member.ContractVersion.Value, es.ContractVersion.Value, position));
            }
        }
    }
}
