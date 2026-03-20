namespace Midl.CodeGen.WinMd;

/// <summary>
/// Constants used in WinRT metadata (.winmd) generation.
/// </summary>
public static class MetadataConstants
{
    public const string MscorlibAssembly = "mscorlib";
    public const string WindowsFoundationAssembly = "Windows.Foundation.FoundationContract";
    public const string VersionString = "Windows Runtime 1.2";

    // Base type names (in mscorlib)
    public const string SystemNamespace = "System";
    public const string SystemObject = "Object";
    public const string SystemEnum = "Enum";
    public const string SystemValueType = "ValueType";
    public const string SystemMulticastDelegate = "MulticastDelegate";
    public const string SystemAttribute = "Attribute";

    // Attribute namespaces
    public const string WindowsFoundationMetadataNamespace = "Windows.Foundation.Metadata";

    // WinRT TypeAttributes flag (not in the public enum)
    public const int TdWindowsRuntime = 0x4000;
}
