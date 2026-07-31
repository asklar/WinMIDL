// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Midl.Analysis;

/// <summary>
/// Represents a single extracted attribute with its name and typed arguments.
/// </summary>
public class AttributeData
{
    public string Name { get; init; } = "";
    public int Position { get; init; }
    public List<AttributeArgValue> Arguments { get; } = new();

    /// <summary>Get argument at index as string, or null.</summary>
    public string? GetString(int index) =>
        index < Arguments.Count && Arguments[index].Kind == ArgKind.String ? Arguments[index].StringValue : null;

    /// <summary>Get argument at index as integer, or null.</summary>
    public long? GetInt(int index) =>
        index < Arguments.Count && Arguments[index].Kind == ArgKind.Integer ? Arguments[index].IntValue : null;

    /// <summary>Get argument at index as type name, or null.</summary>
    public string? GetTypeName(int index) =>
        index < Arguments.Count && Arguments[index].Kind == ArgKind.TypeName ? Arguments[index].StringValue : null;

    /// <summary>Get argument at index as identifier/enum value, or null.</summary>
    public string? GetIdentifier(int index) =>
        index < Arguments.Count && Arguments[index].Kind == ArgKind.Identifier ? Arguments[index].StringValue : null;

    /// <summary>Get argument at index as GUID, or null.</summary>
    public Guid? GetGuid(int index) =>
        index < Arguments.Count && Arguments[index].Kind == ArgKind.Guid ? Arguments[index].GuidValue : null;

    /// <summary>
    /// Get argument at index as an encoded MIDL version (major &lt;&lt; 16 | minor).
    /// Handles integer, float ("1.0"), and hex ("0x00010000") forms.
    /// </summary>
    public uint? GetVersion(int index)
    {
        if (index >= Arguments.Count) return null;
        var arg = Arguments[index];
        if (arg.Kind is ArgKind.Integer or ArgKind.Float)
            return VersionEncoding.FromAttributeArg(arg);
        return null;
    }
}

public enum ArgKind { String, Integer, Float, Identifier, TypeName, Guid, Expression }

public class AttributeArgValue
{
    public ArgKind Kind { get; init; }
    public string? StringValue { get; init; }
    public long IntValue { get; init; }
    public double FloatValue { get; init; }
    public Guid GuidValue { get; init; }
    /// <summary>Source position of this argument (for accurate diagnostic squiggles).</summary>
    public int Position { get; init; }
    /// <summary>Length of this argument in source.</summary>
    public int Length { get; init; }
}
