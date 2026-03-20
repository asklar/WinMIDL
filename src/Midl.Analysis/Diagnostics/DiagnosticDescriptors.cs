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

    public static Diagnostic AttributeWrongTarget(string attrName, string targetKind, string validTargets, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1029",
            $"Attribute '{attrName}' is not valid on {targetKind}. Valid targets: {validTargets}",
            position, attrName.Length);

    public static Diagnostic AttributeArgCountMismatch(string attrName, int expected, int actual, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1030",
            $"Attribute '{attrName}' expects {expected} arguments but got {actual}",
            position, attrName.Length);

    public static Diagnostic PartialConflictingModifiers(string name, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1031",
            $"Partial runtimeclass '{name}' has conflicting modifiers (sealed vs unsealed)",
            position, name.Length);

    public static Diagnostic PartialConflictingBaseClass(string name, string existing, string incoming, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1032",
            $"Partial runtimeclass '{name}' has conflicting base class ('{existing}' vs '{incoming}')",
            position, name.Length);

    // Inheritance / access modifier validation (MIDL1033–MIDL1038)

    public static Diagnostic ProtectedOrOverridableOnSealedClass(string memberName, string modifier, string className, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1033",
            $"'{memberName}' cannot be {modifier} in sealed runtimeclass '{className}'. Use 'unsealed runtimeclass' to allow inheritance.",
            position, memberName.Length);

    public static Diagnostic UndefinedRequiredInterface(string name, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1034",
            $"Required interface '{name}' is undefined.",
            position, name.Length);

    public static Diagnostic BaseNotRuntimeClass(string name, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1035",
            $"Base type '{name}' is not a runtimeclass. Use ':' for implemented interfaces.",
            position, name.Length);

    public static Diagnostic BaseClassSealed(string name, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1036",
            $"Cannot derive from sealed runtimeclass '{name}'. The base class must be declared as 'unsealed runtimeclass'.",
            position, name.Length);

    public static Diagnostic RuntimeClassOnRequiresList(string name, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1038",
            $"'{name}' is a runtimeclass. Only interfaces are allowed in 'requires' clauses.",
            position, name.Length);

    // MIDL3 construct validation (MIDL1039–MIDL1047)

    public static Diagnostic ConstructorInInterface(string name, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1039",
            "Constructors are not allowed in interfaces. Only runtimeclasses can have constructors.",
            position, name.Length);

    public static Diagnostic StaticMemberInInterface(string memberName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1040",
            $"Static members are not allowed in interfaces.",
            position, memberName.Length);

    public static Diagnostic PropertyInStruct(string name, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1041",
            "Properties are not allowed in structs. Structs can only contain fields.",
            position, name.Length);

    public static Diagnostic EventInStruct(string name, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1041",
            "Events are not allowed in structs.",
            position, name.Length);

    public static Diagnostic EventWithNonDelegateType(string typeName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1042",
            $"Event type '{typeName}' is not a delegate type.",
            position, typeName.Length);

    public static Diagnostic VoidParameterType(int position) =>
        new(DiagnosticSeverity.Error, "MIDL1043",
            "'void' is not valid as a parameter type.",
            position, "void".Length);

    public static Diagnostic DuplicateMethodNameWithoutOverload(string name, int position) =>
        new(DiagnosticSeverity.Warning, "MIDL1044",
            $"Methods '{name}' have same name but different signatures. Use [overload(\"name\")] and [method_name(\"name\")] to disambiguate.",
            position, name.Length);

    public static Diagnostic CircularStructReference(string structA, string structB, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1045",
            $"Circular reference between struct '{structA}' and struct '{structB}'.",
            position, structA.Length);

    public static Diagnostic DuplicateEnumMember(string memberName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1046",
            $"Duplicate enum member '{memberName}'.",
            position, memberName.Length);

    public static Diagnostic ImportFileNotFound(string fileName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1047",
            $"Cannot find imported file '{fileName}'.",
            position, fileName.Length);

    public static Diagnostic WinRtTypeNotAllowed(string typeName, int position) => new(DiagnosticSeverity.Error, "MIDL1048", $"Type '{typeName}' is not a valid WinRT type in strict mode.", position, Math.Max(typeName.Length, 1));
    public static Diagnostic NoNestedTypes(string innerType, string outerType, int position) => new(DiagnosticSeverity.Error, "MIDL1049", $"Type '{innerType}' cannot be declared inside '{outerType}'. WinRT does not support nested types.", position, Math.Max(innerType.Length, 1));
    public static Diagnostic NoBidirectionalParams(string paramName, int position) => new(DiagnosticSeverity.Warning, "MIDL1050", $"Parameter '{paramName}' uses 'ref' (bidirectional). WinRT only supports 'in' or 'out' direction.", position, Math.Max(paramName.Length, 1));
    public static Diagnostic NoArrayOfArrays(string paramName, int position) => new(DiagnosticSeverity.Error, "MIDL1051", $"Parameter '{paramName}' is an array of arrays. WinRT does not support jagged arrays.", position, Math.Max(paramName.Length, 1));
    public static Diagnostic PropertyTypeInconsistency(string propertyName, string getterType, string setterType, int position) => new(DiagnosticSeverity.Error, "MIDL1052", $"Property '{propertyName}' getter returns '{getterType}' but setter accepts '{setterType}'.", position, Math.Max(propertyName.Length, 1));
    public static Diagnostic DelegateReturnMustBeVoid(string delegateName, string returnType, int position) => new(DiagnosticSeverity.Error, "MIDL1053", $"Delegate '{delegateName}' must return void but returns '{returnType}'.", position, Math.Max(delegateName.Length, 1));
    public static Diagnostic InterfaceMemberNameCollision(string memberName, string interfaceName, int position) => new(DiagnosticSeverity.Warning, "MIDL1054", $"Interface '{interfaceName}' has duplicate member name '{memberName}'.", position, Math.Max(memberName.Length, 1));
    public static Diagnostic SameArityNeedsDefaultOverload(string methodName, int arity, int position) => new(DiagnosticSeverity.Warning, "MIDL1055", $"Methods named '{methodName}' with {arity} parameter(s) need [default_overload].", position, Math.Max(methodName.Length, 1));
    public static Diagnostic DeprecatedBaseClassInheritance(string className, string baseName, int position) => new(DiagnosticSeverity.Warning, "MIDL1056", $"Base class '{baseName}' is deprecated. Consider marking '{className}' as deprecated too.", position, Math.Max(className.Length, 1));
    public static Diagnostic InterfaceMethodNotImplemented(string className, string interfaceName, string methodName, int position) => new(DiagnosticSeverity.Info, "MIDL1057", $"Runtimeclass '{className}' implements '{interfaceName}' which declares '{methodName}'.", position, Math.Max(className.Length, 1));
    public static Diagnostic DuplicateMethodNameOverride(string methodNameValue, string interfaceName, int position) => new(DiagnosticSeverity.Error, "MIDL1058", $"Duplicate method_name value '{methodNameValue}' in interface '{interfaceName}'.", position, Math.Max(methodNameValue.Length, 1));
    public static Diagnostic DefaultOverloadNotExactlyOne(string methodName, int arity, int count, int position) => new(DiagnosticSeverity.Error, "MIDL1059", $"Methods named '{methodName}' with {arity} parameter(s) have {count} default_overload.", position, Math.Max(methodName.Length, 1));
    public static Diagnostic ConstructorArityConflict(string className, int arity, int position) => new(DiagnosticSeverity.Error, "MIDL1060", $"Runtimeclass '{className}' has multiple constructors with {arity} parameter(s) on the same contract version.", position, Math.Max(className.Length, 1));
    public static Diagnostic StaticUnsealedConflict(string className, int position) => new(DiagnosticSeverity.Error, "MIDL1061", $"Runtimeclass '{className}' cannot be both 'static' and 'unsealed'.", position, Math.Max(className.Length, 1));

    // Custom attribute argument type validation (MIDL1064)
    public static Diagnostic AttributeArgumentTypeMismatch(string attrName, int argIndex, string fieldName, string expectedType, string actualKind, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1064",
            $"Attribute '{attrName}' argument {argIndex + 1} ('{fieldName}'): expected {expectedType} but got {actualKind}",
            position, Math.Max(attrName.Length, 1));

    // Unknown attribute (MIDL1065)
    public static Diagnostic UnknownAttribute(string attrName, int position) =>
        new(DiagnosticSeverity.Warning, "MIDL1065",
            $"Unknown attribute '{attrName}'. If this is a custom attribute, declare it with 'attribute {attrName} {{ ... }};'.",
            position, Math.Max(attrName.Length, 1));
}