// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Midl.CodeGen.WinMd;

/// <summary>
/// Constants used in WinRT metadata (.winmd) generation.
/// </summary>
public static class MetadataConstants
{
    public const string MscorlibAssembly = "mscorlib";
    public const string WindowsFoundationAssembly = "Windows.Foundation";
    public const string VersionString = "WindowsRuntime 1.4";

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

    /// <summary>
    /// CLR System.AttributeTargets enum values.
    /// WinMD files store these in AttributeUsageAttribute blobs (matching MIDL.exe behavior).
    /// </summary>
    public static class ClrAttributeTargets
    {
        public const int Class = 0x0004;        // → target_runtimeclass
        public const int Struct = 0x0008;       // → target_struct
        public const int Enum = 0x0010;         // → target_enum
        public const int Method = 0x0040;       // → target_method
        public const int Parameter = 0x0080;    // → target_parameter (note: different from WinRT=128)
        public const int Property = 0x0100;     // → target_property
        public const int Field = 0x0200;        // → target_field
        public const int Interface = 0x0400;    // → target_interface
        public const int Event = 0x0800;        // → target_event (note: different from WinRT=4)
        public const int Delegate = 0x1000;     // → target_delegate
        public const int All = 0x7FFF;

        /// <summary>
        /// Converts IDL target strings (e.g. "target_runtimeclass") to CLR System.AttributeTargets flags.
        /// </summary>
        public static int FromTargetStrings(IEnumerable<string> targets)
        {
            int flags = 0;
            foreach (var target in targets)
            {
                flags |= target.ToLowerInvariant() switch
                {
                    "target_runtimeclass" => Class,
                    "target_interface" => Interface,
                    "target_delegate" => Delegate,
                    "target_enum" => Enum,
                    "target_struct" => Struct,
                    "target_field" => Field,
                    "target_method" => Method,
                    "target_property" => Property,
                    "target_event" => Event,
                    "target_parameter" => Parameter,
                    "target_all" => All,
                    _ => 0,
                };
            }
            return flags;
        }

        /// <summary>
        /// Converts CLR System.AttributeTargets flags to IDL target strings.
        /// </summary>
        public static List<string> ToTargetStrings(int flags)
        {
            var result = new List<string>();
            if (flags == -1 || flags == All) return result; // All — don't emit
            if ((flags & Delegate) != 0) result.Add("target_delegate");
            if ((flags & Enum) != 0) result.Add("target_enum");
            if ((flags & Event) != 0) result.Add("target_event");
            if ((flags & Field) != 0) result.Add("target_field");
            if ((flags & Interface) != 0) result.Add("target_interface");
            if ((flags & Method) != 0) result.Add("target_method");
            if ((flags & Parameter) != 0) result.Add("target_parameter");
            if ((flags & Property) != 0) result.Add("target_property");
            if ((flags & Class) != 0) result.Add("target_runtimeclass");
            if ((flags & Struct) != 0) result.Add("target_struct");
            return result;
        }
    }
}
