namespace Midl.Analysis;

/// <summary>
/// MIDL version encoding: (major &lt;&lt; 16) | minor.
/// e.g. version 2.5 → (2 &lt;&lt; 16) | 5 = 0x00020005 = 131077.
/// </summary>
public static class VersionEncoding
{
    public static uint Encode(int major, int minor) =>
        (uint)((major << 16) | (minor & 0xFFFF));

    public static (int major, int minor) Decode(uint encoded) =>
        ((int)(encoded >> 16), (int)(encoded & 0xFFFF));

    /// <summary>
    /// Display a decoded version: "major" if minor==0, "major.minor" otherwise.
    /// </summary>
    public static string Display(uint encoded)
    {
        var (major, minor) = Decode(encoded);
        return minor == 0 ? major.ToString() : $"{major}.{minor}";
    }

    /// <summary>
    /// Parse a version from an attribute argument value.
    /// - Integer: encode as (N &lt;&lt; 16) unless it looks like an already-encoded hex value.
    /// - Float text "M.N": encode as (M &lt;&lt; 16) | N.
    /// - Hex integer (0x...): pass through as-is (already encoded).
    /// </summary>
    public static uint FromAttributeArg(AttributeArgValue arg)
    {
        if (arg.Kind == ArgKind.Float && arg.StringValue != null)
        {
            return ParseVersionText(arg.StringValue);
        }

        if (arg.Kind == ArgKind.Integer)
        {
            // Hex literals are already encoded (e.g. 0x00010000)
            if (arg.StringValue != null &&
                arg.StringValue.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return (uint)arg.IntValue;
            }

            // Plain decimal integer: treat as major version
            return Encode((int)arg.IntValue, 0);
        }

        return Encode(1, 0); // fallback default
    }

    /// <summary>
    /// Parse version text like "1.0", "2.5", "3" into encoded form.
    /// </summary>
    internal static uint ParseVersionText(string text)
    {
        // Strip optional 'f'/'F' suffix
        if (text.Length > 0 && text[^1] is 'f' or 'F')
            text = text[..^1];

        var dotIndex = text.IndexOf('.');
        if (dotIndex >= 0)
        {
            if (int.TryParse(text.AsSpan(0, dotIndex), out var major) &&
                int.TryParse(text.AsSpan(dotIndex + 1), out var minor))
            {
                return Encode(major, minor);
            }
        }

        if (int.TryParse(text, out var val))
            return Encode(val, 0);

        return Encode(1, 0);
    }
}
