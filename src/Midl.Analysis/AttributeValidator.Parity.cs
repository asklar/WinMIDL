// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis;

/// <summary>
/// Parity validations that match MIDL.exe behavior.
/// Kept in a partial class to avoid merge conflicts with the main AttributeValidator.
/// </summary>
public partial class AttributeValidator
{
    /// <summary>
    /// Second-pass validations for parity with MIDL.exe.
    /// Covers: duplicate [uuid], duplicate [default] markers, [exclusiveto] self-reference,
    /// [deprecated] argument count, [uuid] format, and contract version bounds.
    /// </summary>
    internal void ValidateParityRules()
    {
        // Build API contract version lookup
        var contractVersions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var type in _compilation.GetAllTypes())
        {
            if (type is ApiContractSymbol ac && ac.ContractVersion.HasValue)
            {
                contractVersions[ac.Name] = ac.ContractVersion.Value;
                contractVersions[ac.FullName] = ac.ContractVersion.Value;
            }
        }

        foreach (var type in _compilation.GetAllTypes())
        {
            // Types reached through an import report at offsets in their own file.
            int diagStart = _diagnostics.Count;
            switch (type)
            {
                case RuntimeClassSymbol rc:
                    ValidateDuplicateUuid(rc.Attributes, rc.Name);
                    ValidateDeprecatedArgCount(rc.Attributes);
                    ValidateUuidArgFormat(rc.Attributes);
                    ValidateDuplicateDefaultMarkers(rc);
                    ValidateContractExceedsMax(rc.Attributes, contractVersions);
                    ValidateMemberContractExceedsMax(rc.Members, contractVersions);
                    ValidateMemberDeprecatedArgs(rc.Members);
                    break;

                case InterfaceSymbol iface when !iface.IsSynthesized:
                    ValidateDuplicateUuid(iface.Attributes, iface.Name);
                    ValidateDeprecatedArgCount(iface.Attributes);
                    ValidateUuidArgFormat(iface.Attributes);
                    ValidateExclusiveToSelf(iface);
                    ValidateContractExceedsMax(iface.Attributes, contractVersions);
                    ValidateMemberDeprecatedArgs(iface.Members);
                    break;

                case DelegateSymbol del:
                    ValidateDuplicateUuid(del.Attributes, del.Name);
                    ValidateDeprecatedArgCount(del.Attributes);
                    ValidateUuidArgFormat(del.Attributes);
                    break;

                case EnumSymbol en:
                    ValidateDeprecatedArgCount(en.Attributes);
                    break;

                case StructSymbol st:
                    ValidateDeprecatedArgCount(st.Attributes);
                    break;
            }
            DiagnosticSourceTagger.Tag(_diagnostics, diagStart, type.SourceFile, _unitSourceMaps);
        }
    }

    /// <summary>Detect duplicate [uuid] attributes on the same type.</summary>
    private void ValidateDuplicateUuid(List<AttributeData> attributes, string typeName)
    {
        int uuidCount = 0;
        int secondPos = 0;
        foreach (var attr in attributes)
        {
            if (attr.Name.Equals("uuid", StringComparison.OrdinalIgnoreCase))
            {
                uuidCount++;
                if (uuidCount == 2)
                    secondPos = attr.Position;
            }
        }
        if (uuidCount > 1)
        {
            _diagnostics.Add(DiagnosticDescriptors.DuplicateUuidAttribute(typeName, secondPos));
        }
    }

    /// <summary>Detect [deprecated] with wrong argument count (must be 3 or 4).</summary>
    private void ValidateDeprecatedArgCount(List<AttributeData> attributes)
    {
        foreach (var attr in attributes)
        {
            if (attr.Name.Equals("deprecated", StringComparison.OrdinalIgnoreCase))
            {
                if (attr.Arguments.Count < 3 || attr.Arguments.Count > 4)
                {
                    _diagnostics.Add(DiagnosticDescriptors.DeprecatedWrongArgCount(
                        attr.Arguments.Count, attr.Position));
                }
            }
        }
    }

    /// <summary>Validate deprecated arg count on member attributes.</summary>
    private void ValidateMemberDeprecatedArgs(IReadOnlyList<ISymbol> members)
    {
        foreach (var member in members)
        {
            var attrs = member switch
            {
                MethodSymbol m => m.Attributes,
                PropertySymbol p => p.Attributes,
                EventSymbol e => e.Attributes,
                ConstructorSymbol c => c.Attributes,
                _ => null,
            };
            if (attrs != null)
                ValidateDeprecatedArgCount(attrs);
        }
    }

    /// <summary>Detect [uuid] with a non-GUID argument (e.g. integer).</summary>
    private void ValidateUuidArgFormat(List<AttributeData> attributes)
    {
        foreach (var attr in attributes)
        {
            if (!attr.Name.Equals("uuid", StringComparison.OrdinalIgnoreCase))
                continue;
            if (attr.Arguments.Count == 0)
                continue;

            var arg = attr.Arguments[0];
            if (arg.Kind == ArgKind.Guid)
                continue; // Valid GUID
            if (arg.Kind == ArgKind.String && Guid.TryParse(arg.StringValue, out _))
                continue; // Valid GUID in string form

            // Non-GUID argument (integer, malformed string, etc.)
            var displayValue = arg.Kind == ArgKind.Integer
                ? arg.IntValue.ToString()
                : arg.StringValue ?? "?";
            _diagnostics.Add(DiagnosticDescriptors.UuidWrongFormat(displayValue, attr.Position));
        }
    }

    /// <summary>Detect [exclusiveto] referencing the interface itself.</summary>
    private void ValidateExclusiveToSelf(InterfaceSymbol iface)
    {
        if (iface.ExclusiveToClass == null)
            return;
        if (string.Equals(iface.ExclusiveToClass, iface.FullName, StringComparison.Ordinal))
        {
            var attr = iface.Attributes.FirstOrDefault(
                a => a.Name.Equals("exclusiveto", StringComparison.OrdinalIgnoreCase));
            int pos = attr?.Position ?? 0;
            _diagnostics.Add(DiagnosticDescriptors.ExclusiveToSelf(iface.Name, pos));
        }
    }

    /// <summary>Detect multiple [default] markers on interface list.</summary>
    private void ValidateDuplicateDefaultMarkers(RuntimeClassSymbol rc)
    {
        int defaultCount = 0;

        // BaseClass can also have [default] marker
        if (rc.BaseClass?.IsDefaultInterface == true)
            defaultCount++;

        foreach (var iface in rc.Interfaces)
        {
            if (iface.IsDefaultInterface)
                defaultCount++;
        }

        if (defaultCount > 1)
        {
            int pos = rc.SourcePosition >= 0 ? rc.SourcePosition : 0;
            _diagnostics.Add(DiagnosticDescriptors.DuplicateDefaultMarker(rc.Name, pos));
        }
    }

    /// <summary>Check that [contract] version doesn't exceed the API contract's max.</summary>
    private void ValidateContractExceedsMax(
        List<AttributeData> attributes,
        Dictionary<string, int> contractVersions)
    {
        foreach (var attr in attributes)
        {
            if (!attr.Name.Equals("contract", StringComparison.OrdinalIgnoreCase))
                continue;

            var contractName = attr.GetTypeName(0) ?? attr.GetIdentifier(0);
            if (contractName == null)
                continue;

            if (!contractVersions.TryGetValue(contractName, out int maxVersion))
                continue;

            var version = attr.GetVersion(1);
            if (version.HasValue && (int)version.Value > maxVersion)
            {
                var versionArg = attr.Arguments.Count > 1 ? attr.Arguments[1] : null;
                var pos = (versionArg != null && versionArg.Position > 0) ? versionArg.Position : attr.Position;
                var len = (versionArg != null && versionArg.Length > 0) ? versionArg.Length : attr.Name.Length;
                _diagnostics.Add(DiagnosticDescriptors.ContractExceedsMax(
                    contractName, (int)version.Value, maxVersion, pos, len));
            }
        }
    }

    /// <summary>Check contract version bounds on member attributes.</summary>
    private void ValidateMemberContractExceedsMax(
        IReadOnlyList<ISymbol> members,
        Dictionary<string, int> contractVersions)
    {
        foreach (var member in members)
        {
            // Check inline [contract] attributes on the member
            var attrs = member switch
            {
                MethodSymbol m => m.Attributes,
                PropertySymbol p => p.Attributes,
                EventSymbol e => e.Attributes,
                ConstructorSymbol c => c.Attributes,
                _ => null,
            };
            // Only check inline [contract] attributes on the member itself,
            // NOT propagated ContractVersion from the parent class (that's already
            // validated at the class level by ValidateContractExceedsMax)
            if (attrs != null)
                ValidateContractExceedsMax(attrs, contractVersions);
        }
    }
}
