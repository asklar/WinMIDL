namespace Midl.Analysis;

/// <summary>
/// Controls which MIDL syntax flavors are accepted.
/// </summary>
public enum CompilationMode
{
    /// <summary>
    /// Pure MIDL3 mode (default). Rejects classic MIDL/MIDLRT constructs.
    /// Encourages modern WinRT authoring.
    /// </summary>
    Strict,

    /// <summary>
    /// Compatibility mode. Accepts both MIDL3 and classic MIDL/MIDLRT syntax.
    /// Use for existing codebases with mixed syntax.
    /// </summary>
    Compat
}
