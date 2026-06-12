// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Cryptography;
using System.Text;
using Midl.Analysis.Symbols;

namespace Midl.Analysis;

/// <summary>
/// Generates deterministic IIDs for parameterized interface instances per the WinRT spec.
/// Uses UUID v5 (SHA-1) with the WinRT pinterface namespace GUID.
/// </summary>
public static class PiidGenerator
{
    /// <summary>Well-known namespace UUID for WinRT parameterized interfaces.</summary>
    private static readonly Guid WrtPinterfaceNamespace = new("11f47ad5-7b73-42c0-abae-878b1e16adee");

    /// <summary>
    /// Generates a deterministic IID for a parameterized interface instance.
    /// </summary>
    /// <param name="piid">The GUID of the parameterized interface (e.g., IVector`1).</param>
    /// <param name="typeArgSignatures">Type signatures of the type arguments.</param>
    public static Guid GenerateIid(string piid, IReadOnlyList<string> typeArgSignatures)
    {
        var signature = $"pinterface({{{piid}}};{string.Join(";", typeArgSignatures)})";
        return GenerateUuidV5(WrtPinterfaceNamespace, signature);
    }

    /// <summary>
    /// Gets the WinRT type signature string for a symbol, used in PIID computation.
    /// </summary>
    public static string GetTypeSignature(ISymbol type)
    {
        return type switch
        {
            InterfaceSymbol iface => iface.Uuid.HasValue
                ? $"{{{iface.Uuid.Value}}}"
                : $"{{{Guid.Empty}}}",

            DelegateSymbol del => del.Uuid.HasValue
                ? $"delegate({{{del.Uuid.Value}}})"
                : $"delegate({{{Guid.Empty}}})",

            RuntimeClassSymbol rc => GetRuntimeClassSignature(rc),
            StructSymbol st => GetStructSignature(st),
            EnumSymbol en => GetEnumSignature(en),
            _ => type.FullName,
        };
    }

    /// <summary>
    /// Gets the WinRT type signature for a primitive/built-in type by name.
    /// </summary>
    public static string GetPrimitiveTypeSignature(string typeName)
    {
        return typeName switch
        {
            "UInt8" or "Byte" => "u1",
            "Int16" => "i2",
            "UInt16" => "u2",
            "Int32" => "i4",
            "UInt32" => "u4",
            "Int64" => "i8",
            "UInt64" => "u8",
            "Single" => "f4",
            "Double" => "f8",
            "Boolean" => "b1",
            "Char16" or "Char" => "c2",
            "String" => "string",
            "Guid" => "g16",
            "Object" => "cinterface(IInspectable)",
            _ => typeName,
        };
    }

    private static string GetRuntimeClassSignature(RuntimeClassSymbol rc)
    {
        var defaultIfaceSig = rc.DefaultInterface != null
            ? GetTypeSignature(rc.DefaultInterface)
            : $"{{{Guid.Empty}}}";
        return $"rc({rc.FullName};{defaultIfaceSig})";
    }

    private static string GetStructSignature(StructSymbol st)
    {
        var fieldSigs = st.Fields.Select(f => GetPrimitiveTypeSignature(f.Type.Name));
        return $"struct({st.FullName};{string.Join(";", fieldSigs)})";
    }

    private static string GetEnumSignature(EnumSymbol en)
    {
        var underlying = GetPrimitiveTypeSignature(en.UnderlyingType);
        return $"enum({en.FullName};{underlying})";
    }

    /// <summary>
    /// Computes a UUID v5 (SHA-1 based) from a namespace UUID and a name string.
    /// </summary>
    private static Guid GenerateUuidV5(Guid namespaceId, string name)
    {
        // Convert namespace GUID to big-endian bytes (RFC 4122 format)
        var nsBytes = namespaceId.ToByteArray();
        SwapGuidEndianness(nsBytes);

        var nameBytes = Encoding.UTF8.GetBytes(name);

        var input = new byte[nsBytes.Length + nameBytes.Length];
        Buffer.BlockCopy(nsBytes, 0, input, 0, nsBytes.Length);
        Buffer.BlockCopy(nameBytes, 0, input, nsBytes.Length, nameBytes.Length);

        var hash = SHA1.HashData(input);

        // Set version (5) and variant (RFC 4122)
        hash[6] = (byte)((hash[6] & 0x0F) | 0x50); // version 5
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80); // variant 10

        // Convert back from big-endian to GUID
        SwapGuidEndianness(hash);

        return new Guid(
            BitConverter.ToUInt32(hash, 0),
            BitConverter.ToUInt16(hash, 4),
            BitConverter.ToUInt16(hash, 6),
            hash[8], hash[9], hash[10], hash[11],
            hash[12], hash[13], hash[14], hash[15]);
    }

    /// <summary>
    /// Swaps the endianness of the first 8 bytes of a GUID byte array
    /// between .NET little-endian and RFC 4122 big-endian format.
    /// </summary>
    private static void SwapGuidEndianness(byte[] bytes)
    {
        // Swap first 4 bytes (UInt32)
        (bytes[0], bytes[3]) = (bytes[3], bytes[0]);
        (bytes[1], bytes[2]) = (bytes[2], bytes[1]);
        // Swap bytes 4-5 (UInt16)
        (bytes[4], bytes[5]) = (bytes[5], bytes[4]);
        // Swap bytes 6-7 (UInt16)
        (bytes[6], bytes[7]) = (bytes[7], bytes[6]);
    }
}
