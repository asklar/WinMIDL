namespace Midl.Analysis.Diagnostics;

public static class DiagnosticDescriptors
{
    public static Diagnostic UndefinedType(string typeName, int position, string? suggestion = null) =>
        new(DiagnosticSeverity.Error, "MIDL1001",
            suggestion != null
                ? $"Undefined type '{typeName}'. Did you mean '{suggestion}'?"
                : $"Undefined type '{typeName}'",
            position, typeName.Length,
            Data: suggestion != null ? new() { ["suggestion"] = suggestion, ["replacement"] = suggestion } : null);

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
        new(DiagnosticSeverity.Error, "MIDL1011",
            $"Method '{name}' has same arity as '{other}' — use [default_overload] to disambiguate",
            position, name.Length);

    public static Diagnostic WriteOnlyProperty(string name, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1012",
            $"Property '{name}' cannot be write-only — WinRT does not support write-only properties",
            position, name.Length);

    public static Diagnostic ReservedMethodParameterName(string name, int position) =>
        new(DiagnosticSeverity.Warning, "MIDL1013",
            $"Parameter name '{name}' is reserved in methods — use [return_name(\"alt\")] to override",
            position, name.Length);

    public static Diagnostic ReservedConstructorParameterName(string name, int position) =>
        new(DiagnosticSeverity.Warning, "MIDL1013",
            $"Parameter name '{name}' is reserved in constructors",
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
        new(DiagnosticSeverity.Error, MIDL2001_Code,
            string.Format(MIDL2001_Message, constructName), position, constructName.Length,
            HelpUri: StrictModeHelpUri);

    public static Diagnostic HResultReturnType(int position) =>
        new(DiagnosticSeverity.Error, MIDL2002_Code,
            MIDL2002_Message, position, "HRESULT".Length,
            HelpUri: StrictModeHelpUri);

    public static Diagnostic ClassicAttribute(string attrName, int position) =>
        new(DiagnosticSeverity.Error, MIDL2003_Code,
            string.Format(MIDL2003_Message, attrName), position, attrName.Length,
            HelpUri: StrictModeHelpUri);

    public static Diagnostic PointerType(string typeName, int position) =>
        new(DiagnosticSeverity.Error, MIDL2004_Code,
            string.Format(MIDL2004_Message, typeName), position, typeName.Length,
            HelpUri: StrictModeHelpUri);

    public static Diagnostic LowercaseType(string lowercaseName, string modernName, int position) =>
        new(DiagnosticSeverity.Error, MIDL2005_Code,
            string.Format(MIDL2005_Message, lowercaseName, modernName), position, lowercaseName.Length,
            HelpUri: StrictModeHelpUri,
            Data: new() { ["replacement"] = modernName });

    public static Diagnostic ClassicMidlType(string typeName, string suggestion, int position) =>
        new(DiagnosticSeverity.Error, MIDL2005_Code,
            $"'{typeName}' is a classic MIDL type. Use '{suggestion}' in MIDL3.", position, typeName.Length,
            HelpUri: StrictModeHelpUri,
            Data: new() { ["replacement"] = suggestion });

    public static Diagnostic StrictModeTypeWarning(string typeName, string advice, int position) =>
        new(DiagnosticSeverity.Error, MIDL2005_Code,
            $"'{typeName}': {advice}", position, typeName.Length,
            HelpUri: StrictModeHelpUri);

    public static Diagnostic IncludeDirectiveInStrictMode(int position) =>
        new(DiagnosticSeverity.Warning, "MIDL2006",
            "#include pastes file contents into the compilation unit. Consider using 'import \"file.idl\";' to reference types without inlining.",
            position, 8,
            HelpUri: StrictModeHelpUri);

    // Version/contract propagation diagnostics
    public static Diagnostic InterfaceVersionTooEarly(string ifaceName, int ifaceVersion, int classVersion, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1014",
            $"Interface '{ifaceName}' version {VersionEncoding.Display((uint)ifaceVersion)} is earlier than containing class version {VersionEncoding.Display((uint)classVersion)}",
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
            $"Enum member '{memberName}' version {VersionEncoding.Display((uint)memberVersion)} is earlier than containing enum version {VersionEncoding.Display((uint)enumVersion)}",
            position, memberName.Length);

    // Attribute semantic validation diagnostics (MIDL1020–MIDL1028)

    public static Diagnostic InvalidThreadingModel(string value, int position, int length = 0) =>
        new(DiagnosticSeverity.Error, "MIDL1020",
            $"Invalid threading model '{value}'. Must be 'both', 'sta', or 'mta'.",
            position, length > 0 ? length : value.Length);

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

    public static Diagnostic NonInterfaceOnRequiresList(string name, string kind, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1038",
            $"'{name}' is a {kind}. Only interfaces are allowed in 'requires' clauses.",
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
            $"Methods '{name}' have same name but different signatures. The first overload needs no attributes; subsequent same-arity overloads need [default_overload] and [method_name(\"UniqueName\")] to disambiguate.",
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
    public static Diagnostic InterfaceMemberNameCollision(string memberName, string interfaceName, int position) => new(DiagnosticSeverity.Error, "MIDL1054", $"Interface '{interfaceName}' has duplicate member name '{memberName}'.", position, Math.Max(memberName.Length, 1));
    public static Diagnostic SameArityNeedsDefaultOverload(string methodName, int arity, int position) => new(DiagnosticSeverity.Warning, "MIDL1055", $"Methods named '{methodName}' with {arity} parameter(s) need [default_overload].", position, Math.Max(methodName.Length, 1));
    public static Diagnostic DeprecatedBaseClassInheritance(string className, string baseName, int position) => new(DiagnosticSeverity.Warning, "MIDL1056", $"Base class '{baseName}' is deprecated. Consider marking '{className}' as deprecated too.", position, Math.Max(className.Length, 1));
    public static Diagnostic InterfaceMethodNotImplemented(string className, string interfaceName, string methodName, int position) => new(DiagnosticSeverity.Info, "MIDL1057", $"Runtimeclass '{className}' implements '{interfaceName}' which declares '{methodName}'.", position, Math.Max(className.Length, 1));
    public static Diagnostic DuplicateMethodNameOverride(string methodNameValue, string interfaceName, int position) => new(DiagnosticSeverity.Error, "MIDL1058", $"Duplicate method_name value '{methodNameValue}' in interface '{interfaceName}'.", position, Math.Max(methodNameValue.Length, 1));
    public static Diagnostic DefaultOverloadNotExactlyOne(string methodName, int arity, int count, int position) => new(DiagnosticSeverity.Error, "MIDL1059", $"Methods named '{methodName}' with {arity} parameter(s) have {count} default_overload.", position, Math.Max(methodName.Length, 1));
    public static Diagnostic ConstructorArityConflict(string className, int arity, int position) => new(DiagnosticSeverity.Error, "MIDL1060", $"Runtimeclass '{className}' has multiple constructors with {arity} parameter(s) on the same contract version.", position, Math.Max(className.Length, 1));
    public static Diagnostic StaticUnsealedConflict(string className, int position) => new(DiagnosticSeverity.Error, "MIDL1061", $"Runtimeclass '{className}' cannot be both 'static' and 'unsealed'.", position, Math.Max(className.Length, 1));

    // Empty runtimeclass without default_interface (MIDL1062)
    public static Diagnostic EmptyRuntimeClassNoInterface(string className, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1062",
            $"Runtimeclass '{className}' must implement at least one interface or have [default_interface].",
            position, Math.Max(className.Length, 1));

    // No optional/default/vararg params in WinRT (MIDL1063)
    public static Diagnostic OptionalParamNotAllowed(string paramName, string methodName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1063",
            $"Parameter '{paramName}' in '{methodName}' cannot be optional. WinRT does not support optional, default, or vararg parameters.",
            position, Math.Max(paramName.Length, 1));

    // Runtimeclass used as param needs default interface (MIDL1077)
    public static Diagnostic ParamClassNeedsDefaultInterface(string paramName, string className, string methodName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1077",
            $"Parameter '{paramName}' in '{methodName}': runtimeclass '{className}' has no default interface and cannot be used as a parameter type.",
            position, Math.Max(className.Length, 1));

    // Custom attribute argument type validation (MIDL1064)
    public static Diagnostic AttributeArgumentTypeMismatch(string attrName, int argIndex, string fieldName, string expectedType, string actualKind, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1064",
            $"Attribute '{attrName}' argument {argIndex + 1} ('{fieldName}'): expected {expectedType} but got {actualKind}",
            position, Math.Max(attrName.Length, 1));

    // Unknown attribute (MIDL1065)
    public static Diagnostic UnknownAttribute(string attrName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1065",
            $"Unknown attribute '{attrName}'. If this is a custom attribute, declare it with 'attribute {attrName} {{ ... }};'.",
            position, Math.Max(attrName.Length, 1));

    // Attribute argument out of range (MIDL1066)
    public static Diagnostic AttributeArgumentOutOfRange(string attrName, int argIndex, string fieldName, string fieldType, long value, string validRange, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1066",
            $"Attribute '{attrName}' argument {argIndex + 1} ('{fieldName}'): value {value} is out of range for {fieldType} (valid: {validRange})",
            position, Math.Max(attrName.Length, 1));

    // Enum value must be integer (MIDL1067)
    public static Diagnostic EnumValueNotInteger(string memberName, string enumName, string valueText, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1067",
            $"Enum member '{memberName}' in '{enumName}': value '{valueText}' must be an integer literal",
            position, Math.Max(valueText.Length, 1));

    // Enum value out of range (MIDL1068)
    public static Diagnostic EnumValueOutOfRange(string memberName, string enumName, string valueText, string underlyingType, string validRange, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1068",
            $"Enum member '{memberName}' in '{enumName}': value {valueText} is out of range for {underlyingType} (valid: {validRange})",
            position, Math.Max(valueText.Length, 1));

    // ExclusiveTo violation (MIDL1069)
    public static Diagnostic ExclusiveToViolation(string interfaceName, string exclusiveToClass, string implementingClass, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1069",
            $"Interface '{interfaceName}' is [exclusiveto({exclusiveToClass})] and cannot be implemented by '{implementingClass}'.",
            position, Math.Max(interfaceName.Length, 1));

    // Missing default interface (MIDL1070)
    public static Diagnostic MissingDefaultInterface(string className, int interfaceCount, int position) =>
        new(DiagnosticSeverity.Warning, "MIDL1070",
            $"Runtimeclass '{className}' implements {interfaceCount} interfaces but none is marked [default]. The first interface will be used as default.",
            position, Math.Max(className.Length, 1));

    // WinMD namespace mismatch (MIDL1071)
    public static Diagnostic WinMdNamespaceMismatch(string typeName, string typeNamespace, string winmdName, int position) =>
        new(DiagnosticSeverity.Warning, "MIDL1071",
            $"Type '{typeName}' is in namespace '{typeNamespace}' but WinMD file is named '{winmdName}'. WinMD file name should match the root namespace.",
            position, Math.Max(typeName.Length, 1));

    // Protected accessor not valid in MIDL3 (MIDL1072)
    public static Diagnostic ProtectedAccessorNotValid(string propName, string accessor, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1072",
            $"'{accessor}' cannot be marked 'protected' individually. Apply 'protected' to the whole property '{propName}' instead.",
            position, Math.Max(propName.Length, 1));

    // Protected member on interface (MIDL1073)
    public static Diagnostic ProtectedMemberOnInterface(string memberName, string interfaceName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1073",
            $"Member '{memberName}' on interface '{interfaceName}' cannot be 'protected'. Interface members are always public.",
            position, Math.Max(memberName.Length, 1));

    // [contract] argument not an apicontract (MIDL1075)
    public static Diagnostic ContractArgNotApiContract(string attrTarget, string contractName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1075",
            $"[contract] on '{attrTarget}': '{contractName}' is not declared as an apicontract.",
            position, Math.Max(contractName.Length, 1));

    // [feature] argument not a feature_name (MIDL1076)
#if WINDOWS_INTERNAL
    public static Diagnostic FeatureArgNotFeatureName(string attrTarget, string featureName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1076",
            $"[feature] on '{attrTarget}': '{featureName}' is not declared via feature_name.",
            position, Math.Max(featureName.Length, 1));
#endif

    // Circular interface requires (MIDL1078)
    public static Diagnostic CircularRequires(string interfaceName, string cycle, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1078",
            $"Interface '{interfaceName}' has a circular 'requires' dependency: {cycle}.",
            position, Math.Max(interfaceName.Length, 1));

    // ─── Parity diagnostics (MIDL1079–MIDL1094) ────────────────────

    public static Diagnostic DuplicateParameterName(string paramName, string methodName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1079",
            $"Duplicate parameter name '{paramName}' in '{methodName}'.",
            position, Math.Max(paramName.Length, 1));

    public static Diagnostic VersionedBlockConflict(int innerVersion, int outerVersion, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1080",
            $"Versioned block version {VersionEncoding.Display((uint)innerVersion)} is lower than containing type's contract version {VersionEncoding.Display((uint)outerVersion)}.",
            position, 1);

    public static Diagnostic UuidWrongFormat(string value, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1081",
            $"Invalid UUID format '{value}': expected 8-4-4-4-12 hexadecimal digits.",
            position, Math.Max(value.Length, 1));

    public static Diagnostic DefaultInterfaceOnNonClass(string typeName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1082",
            $"[default_interface] is only valid on runtimeclass, not on '{typeName}'.",
            position, Math.Max(typeName.Length, 1));

    public static Diagnostic PropertyVoidType(string propName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1083",
            $"Property '{propName}' cannot have void type.",
            position, Math.Max(propName.Length, 1));

    public static Diagnostic StaticClassInstanceMethod(string memberName, string className, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1084",
            $"Static runtimeclass '{className}' cannot have instance member '{memberName}'.",
            position, Math.Max(memberName.Length, 1));

    public static Diagnostic DuplicatePropertyName(string propName, string typeName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1085",
            $"Duplicate property '{propName}' in '{typeName}'.",
            position, Math.Max(propName.Length, 1));

    public static Diagnostic DuplicateUuidAttribute(string typeName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1086",
            $"Type '{typeName}' has multiple [uuid] attributes.",
            position, Math.Max(typeName.Length, 1));

    public static Diagnostic DuplicateOverloadName(string overloadName, string interfaceName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1087",
            $"Duplicate [overload] name '{overloadName}' in '{interfaceName}'.",
            position, Math.Max(overloadName.Length, 1));

    public static Diagnostic MarshalingBehaviorOnNonClass(string typeName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1088",
            $"[marshaling_behavior] is only valid on runtimeclass, not on '{typeName}'.",
            position, Math.Max(typeName.Length, 1));

    public static Diagnostic FieldInRuntimeClass(string fieldName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1089",
            $"Fields are not allowed in runtimeclass. Use a property for '{fieldName}'.",
            position, Math.Max(fieldName.Length, 1));

    public static Diagnostic ThreadingOnNonClass(string typeName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1090",
            $"[threading] is only valid on runtimeclass, not on '{typeName}'.",
            position, Math.Max(typeName.Length, 1));

    public static Diagnostic ConstructorWithReturnType(string name, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1091",
            $"Constructor '{name}' cannot have a return type.",
            position, Math.Max(name.Length, 1));

    public static Diagnostic ExclusiveToSelf(string interfaceName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1092",
            $"[exclusiveto] on '{interfaceName}' must reference a runtimeclass, not the interface itself.",
            position, Math.Max(interfaceName.Length, 1));

    public static Diagnostic ContractExceedsMax(string contractName, int version, int maxVersion, int position, int length = 0) =>
        new(DiagnosticSeverity.Error, "MIDL1093",
            $"Contract version {version} exceeds '{contractName}' contractversion {maxVersion}.",
            position, length > 0 ? length : 8); // "contract".Length

    public static Diagnostic UuidOnNonInterface(string typeName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1094",
            $"[uuid] is only valid on interfaces and delegates, not on '{typeName}'.",
            position, Math.Max(typeName.Length, 1));

    public static Diagnostic ActivatableOnNonClass(string typeName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1095",
            $"[activatable] is only valid on runtimeclass, not on '{typeName}'.",
            position, Math.Max(typeName.Length, 1));

    public static Diagnostic DeprecatedWrongArgCount(int actual, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1096",
            $"[deprecated] requires 3 or 4 arguments (message, type, [contract,] version), got {actual}.",
            position, "deprecated".Length);

    public static Diagnostic OverloadOnPropertyOrEvent(string memberName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1097",
            $"[overload] may not be applied to properties or events. Found on '{memberName}'.",
            position, Math.Max(memberName.Length, 1));

    // ─── Code Quality Hints (MIDL3001+) ─────────────────────────────

    public static Diagnostic InterfaceNamingConvention(string name, string suggestion, int position) =>
        new(DiagnosticSeverity.Info, "MIDL3001",
            $"Interface '{name}' should follow 'I' + PascalCase naming convention (e.g. '{suggestion}').",
            position, Math.Max(name.Length, 1),
            Data: new() { ["replacement"] = suggestion });

    public static Diagnostic EmptyRuntimeClass(string name, int position) =>
        new(DiagnosticSeverity.Info, "MIDL3002",
            $"Runtimeclass '{name}' has no members. Consider adding constructors, methods, properties, or events.",
            position, Math.Max(name.Length, 1));

    public static Diagnostic EmptyInterface(string name, int position) =>
        new(DiagnosticSeverity.Info, "MIDL3003",
            $"Interface '{name}' has no members.",
            position, Math.Max(name.Length, 1));

    public static Diagnostic RuntimeClassNoConstructor(string name, int position) =>
        new(DiagnosticSeverity.Info, "MIDL3005",
            $"Runtimeclass '{name}' does not appear to be activatable: no constructor and not marked [activatable]. " +
            "If instances are obtained from another type's method, add a comment: // [obtained-from:Namespace.Type.Method]",
            position, Math.Max(name.Length, 1));

    public static Diagnostic EventHandlerNamingConvention(string name, string typeName, int position) =>
        new(DiagnosticSeverity.Info, "MIDL3006",
            $"Event '{name}' on '{typeName}': consider using TypedEventHandler<TSender, TArgs> for new APIs.",
            position, Math.Max(name.Length, 1));

    public static Diagnostic PropertySetterOnly(string name, string typeName, int position) =>
        new(DiagnosticSeverity.Info, "MIDL3007",
            $"Property '{name}' on '{typeName}' has a setter but no getter. This is unusual in WinRT.",
            position, Math.Max(name.Length, 1));

    public static Diagnostic MethodNameClash(string methodName, string typeName, int position) =>
        new(DiagnosticSeverity.Info, "MIDL3008",
            $"Method '{methodName}' on '{typeName}' shares a name with the type. Consider renaming to avoid confusion.",
            position, Math.Max(methodName.Length, 1));

    public static Diagnostic NamespaceCasingConvention(string name, int position) =>
        new(DiagnosticSeverity.Info, "MIDL3009",
            $"Namespace segment '{name}' should use PascalCase (e.g. '{char.ToUpperInvariant(name[0])}{name[1..]}').",
            position, Math.Max(name.Length, 1),
            Data: new() { ["replacement"] = $"{char.ToUpperInvariant(name[0])}{name[1..]}" });

    public static Diagnostic AttributeNamingConvention(string name, int position) =>
        new(DiagnosticSeverity.Info, "MIDL3010",
            $"Attribute '{name}' should end with 'Attribute' by convention (e.g. '{name}Attribute'). Use [attributename(\"{name}\")] to keep the short name.",
            position, Math.Max(name.Length, 1),
            Data: new() { ["replacement"] = name + "Attribute" });

    public static Diagnostic DelegateNamingConvention(string name, int position) =>
        new(DiagnosticSeverity.Info, "MIDL3011",
            $"Delegate '{name}' should end with 'Handler' or 'Callback' by convention.",
            position, Math.Max(name.Length, 1));

    public static Diagnostic EventArgsNamingConvention(string name, int position) =>
        new(DiagnosticSeverity.Info, "MIDL3012",
            $"Struct '{name}' used as event args should end with 'EventArgs' by convention.",
            position, Math.Max(name.Length, 1));

    public static Diagnostic AsyncMethodNamingConvention(string methodName, string typeName, int position) =>
        new(DiagnosticSeverity.Info, "MIDL3013",
            $"Method '{methodName}' on '{typeName}' returns an async type but doesn't end with 'Async'.",
            position, Math.Max(methodName.Length, 1),
            Data: new() { ["replacement"] = methodName + "Async" });

    public static Diagnostic TypeNamePascalCase(string name, int position) =>
        new(DiagnosticSeverity.Info, "MIDL3014",
            $"Type name '{name}' should use PascalCase without underscores.",
            position, Math.Max(name.Length, 1),
            Data: new() { ["replacement"] = ConvertToPascalCase(name) });

    public static Diagnostic MemberNamePascalCase(string name, string typeName, int position) =>
        new(DiagnosticSeverity.Info, "MIDL3015",
            $"Member '{name}' on '{typeName}' should use PascalCase.",
            position, Math.Max(name.Length, 1),
            Data: new() { ["replacement"] = char.ToUpperInvariant(name[0]) + name[1..] });

    private static string ConvertToPascalCase(string name)
    {
        var parts = name.Split('_', StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(parts.Select(p =>
            p.Length > 0 ? char.ToUpperInvariant(p[0]) + p[1..] : ""));
    }

    public static Diagnostic PropertyCanUseShorthand(string propName, string typeName, int position) =>
        new(DiagnosticSeverity.Info, "MIDL3016",
            $"Property '{propName}' on '{typeName}' has both get and set. Consider using shorthand: '{propName};' instead of '{{ get; set; }}'.",
            position, Math.Max(propName.Length, 1));

    public static Diagnostic RedundantDefaultAttribute(string attrName, string defaultValue, int position, int length) =>
        new(DiagnosticSeverity.Info, "MIDL3017",
            $"[{attrName}({defaultValue})] is the default and can be omitted.",
            position, Math.Max(length, 1));

    public static Diagnostic ObsoleteAttribute(string attrName, int position, int length) =>
        new(DiagnosticSeverity.Info, "MIDL3018",
            $"[{attrName}] has no effect in modern WinRT and can be removed.",
            position, Math.Max(length, 1));

    public static Diagnostic ClassCouldBeStatic(string name, int position) =>
        new(DiagnosticSeverity.Info, "MIDL3019",
            $"Runtimeclass '{name}' consists only of static members. Did you intend it to be a 'static runtimeclass'? " +
            "If not, add [default_interface] to allow creating instances.",
            position, Math.Max(name.Length, 1));

    // Missing doc comment (MIDL3020)
    public static Diagnostic MissingDocComment(string name, int position) =>
        new(DiagnosticSeverity.Info, "MIDL3020",
            $"Public type '{name}' has no documentation comment. Add `/// <summary>` above the declaration.",
            position, Math.Max(name.Length, 1));

    // Malformed doc comment (MIDL3021)
    public static Diagnostic MalformedDocComment(string error, int position) =>
        new(DiagnosticSeverity.Warning, "MIDL3021",
            $"Malformed doc comment: {error}",
            position, 3); // length of "///"

    // Empty enum (MIDL1098)
    public static Diagnostic EmptyEnum(string enumName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1098",
            $"Enum '{enumName}' must have at least one member.",
            position, Math.Max(enumName.Length, 1));

    // Member name same as containing type (MIDL1099)
    public static Diagnostic MemberNameSameAsType(string memberName, string typeName, string memberKind, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1099",
            $"{memberKind} '{memberName}' cannot have the same name as containing type '{typeName}'. Only constructors can use the type name.",
            position, Math.Max(memberName.Length, 1));

    // Static modifier on non-runtimeclass (MIDL1100)
    public static Diagnostic StaticModifierOnNonClass(string keyword, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1100",
            $"The 'static' modifier is not valid on '{keyword}' declarations. Only runtimeclass supports modifiers.",
            position, "static".Length);

    // Generic WinRT type used without type arguments (MIDL1101)
    public static Diagnostic GenericTypeRequiresArguments(string typeName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1101",
            $"Generic type '{typeName}' requires type arguments.",
            position, Math.Max(typeName.Length, 1));

    // @ref: directive points to a file or folder that doesn't exist (MIDL1102)
    public static Diagnostic RefTargetNotFound(string path, int position, int length) =>
        new(DiagnosticSeverity.Error, "MIDL1102",
            $"Referenced path '{path}' not found. Check the file path in the @ref directive.",
            position, length);

    // feature_name with invalid stage (MIDL1103)
#if WINDOWS_INTERNAL
    public static Diagnostic InvalidFeatureStage(string featureName, string stage, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1103",
            $"feature_name '{featureName}': '{stage}' is not a valid FeatureStage. Expected AlwaysDisabled, DisabledByDefault, EnabledByDefault, or AlwaysEnabled.",
            position, Math.Max(stage.Length, 1));
#endif

    // Non-experimental type references experimental type (MIDL1104)
    public static Diagnostic ExperimentalTypeReference(string typeName, string experimentalTypeName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1104",
            $"Type '{typeName}' references experimental type '{experimentalTypeName}'. Mark '{typeName}' as [experimental] or remove the reference.",
            position, Math.Max(experimentalTypeName.Length, 1));

#if WINDOWS_INTERNAL
    // Non-feature-gated type references feature-gated type (MIDL1105)
    public static Diagnostic FeatureGatedTypeReference(string typeName, string featureGatedType, string featureName, int position) =>
        new(DiagnosticSeverity.Warning, "MIDL1105",
            $"Type '{typeName}' references feature-gated type '{featureGatedType}' ([feature({featureName})]). Consider adding [feature({featureName})] to '{typeName}'.",
            position, Math.Max(featureGatedType.Length, 1));
#endif

    // Duplicate [default] marker on interface list (MIDL1106)
    public static Diagnostic DuplicateDefaultMarker(string className, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1106",
            $"Runtimeclass '{className}' has multiple [default] markers on its interface list.",
            position, Math.Max(className.Length, 1));

    // Invalid version value (MIDL1107)
    public static Diagnostic InvalidVersionValue(string attrName, string value, int position, int length = 0) =>
        new(DiagnosticSeverity.Error, "MIDL1107",
            $"[{attrName}] version argument '{value}' is invalid. Expected a version number (major.minor, each 0–65535).",
            position, length > 0 ? length : Math.Max(attrName.Length, 1));

    // Array syntax not enabled (MIDL1108)
    public static Diagnostic ArraySyntaxNotEnabled(int position) =>
        new(DiagnosticSeverity.Error, "MIDL1108",
            "Array syntax (T[]) requires the /arrays flag or \"arrays\": true in winmidl.json.",
            position, 2); // length covers []

    // Array of interface not allowed (MIDL1109)
    public static Diagnostic ArrayOfInterfaceNotAllowed(string typeName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1109",
            $"Array of interface type '{typeName}' is not supported. Arrays may only contain value types, strings, structs, or enums.",
            position, Math.Max(typeName.Length, 1));

    // Array of delegate not allowed (MIDL1110)
    public static Diagnostic ArrayOfDelegateNotAllowed(string typeName, int position) =>
        new(DiagnosticSeverity.Error, "MIDL1110",
            $"Array of delegate type '{typeName}' is not supported. Arrays may only contain value types, strings, structs, or enums.",
            position, Math.Max(typeName.Length, 1));

    // Nested arrays not allowed (MIDL1111)
    public static Diagnostic NestedArrayNotAllowed(int position) =>
        new(DiagnosticSeverity.Error, "MIDL1111",
            "Nested arrays (T[][]) are not supported. Use a single-dimensional array.",
            position, 2);
}