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
}

public enum ArgKind { String, Integer, Float, Identifier, TypeName, Guid, Expression }

public class AttributeArgValue
{
    public ArgKind Kind { get; init; }
    public string? StringValue { get; init; }
    public long IntValue { get; init; }
    public double FloatValue { get; init; }
    public Guid GuidValue { get; init; }
}
