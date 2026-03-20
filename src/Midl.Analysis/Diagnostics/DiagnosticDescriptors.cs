namespace Midl.Analysis.Diagnostics;

public static class DiagnosticDescriptors
{
    public static Diagnostic UndefinedType(string typeName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1001",
            $"Undefined type '{typeName}'", position, typeName.Length);

    public static Diagnostic DuplicateDefinition(string name, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1002",
            $"Duplicate definition '{name}'", position, name.Length);

    public static Diagnostic NamespaceRequired(string typeName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1003",
            $"Namespace required for WinRT type '{typeName}'", position, typeName.Length);

    public static Diagnostic InvalidBaseClass(string typeName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1004",
            $"Invalid base class '{typeName}' (not unsealed)", position, typeName.Length);

    public static Diagnostic StructFieldNotValueType(string fieldName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1005",
            $"Struct field '{fieldName}' must be a value type, string, or IReference<T>",
            position, fieldName.Length);

    public static Diagnostic EnumUnderlyingType(string typeName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1006",
            $"Enum '{typeName}' underlying type must be Int32 or UInt32",
            position, typeName.Length);

    public static Diagnostic StaticClassInstanceMember(string memberName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1007",
            $"Static class cannot have instance member '{memberName}'",
            position, memberName.Length);

    public static Diagnostic FlagsEnumMustBeUInt32(string enumName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1008",
            $"[flags] enum '{enumName}' must have UInt32 underlying type",
            position, enumName.Length);

    public static Diagnostic EmptyStruct(string name, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1009",
            $"Empty struct '{name}' — WinRT structs must have at least one field",
            position, name.Length);

    public static Diagnostic CaseInsensitiveTypeCollision(string name, string existing, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1010",
            $"Type '{name}' conflicts with '{existing}' (WinRT type names are case-insensitive)",
            position, name.Length);

    public static Diagnostic SameArityOverload(string name, string other, int position) =>
        new(DiagnosticSeverity.Warning, "MIDL1011",
            $"Method '{name}' has same arity as '{other}' — use [default_overload] to disambiguate",
            position, name.Length);

    public static Diagnostic WriteOnlyProperty(string name, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1012",
            $"Property '{name}' cannot be write-only — WinRT does not support write-only properties",
            position, name.Length);

    public static Diagnostic ReservedParameterName(string name, int position) =>
        new(DiagnosticSeverity.Warning, "MIDL1013",
            $"Reserved parameter name '{name}' — use [return_name(\"alt\")] to override",
            position, name.Length);

    // Strict mode diagnostics (MIDL2xxx — "2" for "MIDL2/classic constructs not allowed")
    public const string MIDL2001_Code = "MIDL2001";
    public const string MIDL2001_Message = "'{0}' is a classic MIDL construct and is not allowed in strict MIDL3 mode. Use /compat to enable classic syntax.";

    public const string MIDL2002_Code = "MIDL2002";
    public const string MIDL2002_Message = "HRESULT return type is not used in MIDL3. Methods should return the actual type directly.";

    public const string MIDL2003_Code = "MIDL2003";
    public const string MIDL2003_Message = "'[{0}]' is a classic MIDL attribute. In MIDL3, use the equivalent modern syntax.";

    public const string MIDL2004_Code = "MIDL2004";
    public const string MIDL2004_Message = "Pointer types ('{0}*') are not used in MIDL3. Reference types are passed directly.";

    public const string MIDL2005_Code = "MIDL2005";
    public const string MIDL2005_Message = "Lowercase '{0}' is MIDL2 syntax. Use '{1}' (capitalized) for MIDL3.";

    private const string StrictModeHelpUri = "https://learn.microsoft.com/en-us/uwp/midl-3/";

    public static Diagnostic ClassicConstruct(string constructName, int position) =>
        new(DiagnosticSeverity.Warning, MIDL2001_Code,
            string.Format(MIDL2001_Message, constructName), position, constructName.Length,
            HelpUri: StrictModeHelpUri);

    public static Diagnostic HResultReturnType(int position) =>
        new(DiagnosticSeverity.Warning, MIDL2002_Code,
            MIDL2002_Message, position, "HRESULT".Length,
            HelpUri: StrictModeHelpUri);

    public static Diagnostic ClassicAttribute(string attrName, int position) =>
        new(DiagnosticSeverity.Warning, MIDL2003_Code,
            string.Format(MIDL2003_Message, attrName), position, attrName.Length,
            HelpUri: StrictModeHelpUri);

    public static Diagnostic PointerType(string typeName, int position) =>
        new(DiagnosticSeverity.Warning, MIDL2004_Code,
            string.Format(MIDL2004_Message, typeName), position, typeName.Length,
            HelpUri: StrictModeHelpUri);

    public static Diagnostic LowercaseType(string lowercaseName, string modernName, int position) =>
        new(DiagnosticSeverity.Warning, MIDL2005_Code,
            string.Format(MIDL2005_Message, lowercaseName, modernName), position, lowercaseName.Length,
            HelpUri: StrictModeHelpUri,
            Data: new() { ["replacement"] = modernName });

    public static Diagnostic ClassicMidlType(string typeName, string suggestion, int position) =>
        new(DiagnosticSeverity.Warning, MIDL2005_Code,
            $"'{typeName}' is a classic MIDL type. Use '{suggestion}' in MIDL3.", position, typeName.Length,
            HelpUri: StrictModeHelpUri,
            Data: new() { ["replacement"] = suggestion });

    public static Diagnostic StrictModeTypeWarning(string typeName, string advice, int position) =>
        new(DiagnosticSeverity.Warning, MIDL2005_Code,
            $"'{typeName}': {advice}", position, typeName.Length,
            HelpUri: StrictModeHelpUri);

    // Version/contract propagation diagnostics
    public static Diagnostic InterfaceVersionTooEarly(string ifaceName, int ifaceVersion, int classVersion, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1014",
            $"Interface '{ifaceName}' version {ifaceVersion} is earlier than containing class version {classVersion}",
            position, ifaceName.Length);

    public static Diagnostic ContractFamilyMismatch(string ifaceName, string ifaceContract, string className, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1015",
            $"Interface '{ifaceName}' uses contract '{ifaceContract}' which is not in the contract history of runtimeclass '{className}'",
            position, ifaceName.Length);

    public static Diagnostic UndefinedContract(string contractName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1016",
            $"Undefined contract '{contractName}'", position, contractName.Length);

    public static Diagnostic EnumMemberVersionTooEarly(string memberName, int memberVersion, int enumVersion, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1017",
            $"Enum member '{memberName}' version {memberVersion} is earlier than containing enum version {enumVersion}",
            position, memberName.Length);

    // Attribute semantic validation diagnostics (MIDL1020–MIDL1028)

    public static Diagnostic InvalidThreadingModel(string value, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1020",
            $"Invalid threading model '{value}'. Must be 'both', 'sta', or 'mta'.",
            position, value.Length);

    public static Diagnostic InvalidMarshalingBehavior(string value, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1021",
            $"Invalid marshaling behavior '{value}'.",
            position, value.Length);

    public static Diagnostic InvalidComposableAccess(string value, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1022",
            $"Invalid composable access '{value}'.",
            position, value.Length);

    public static Diagnostic ActivatableFactoryNotExclusiveTo(string factoryName, string className, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1023",
            $"Activatable factory interface '{factoryName}' must be [exclusiveto({className})].",
            position, factoryName.Length);

    public static Diagnostic StaticInterfaceNotExclusiveTo(string ifaceName, string className, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1024",
            $"Static interface '{ifaceName}' must be [exclusiveto({className})].",
            position, ifaceName.Length);

    public static Diagnostic ComposableAndActivatable(string className, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1025",
            "Cannot have both [composable] and [activatable] on the same runtime class.",
            position, className.Length);

    public static Diagnostic InvalidDeprecationType(string value, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1026",
            $"Invalid deprecation type '{value}'.",
            position, value.Length);

    public static Diagnostic ThreadingOnNonActivatable(string attrName, int position) =>
        new(DiagnosticSeverity.Warning, "MIDL1027",
            $"[{attrName}] is only meaningful on activatable or composable runtime classes.",
            position, attrName.Length);

    public static Diagnostic AttributeUsageNotOnAttribute(string typeName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1028",
            $"[attributeusage] can only appear on attribute declarations, not on '{typeName}'.",
            position, typeName.Length);
}
