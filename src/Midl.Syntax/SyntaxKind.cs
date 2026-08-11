// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Midl.Syntax;

/// <summary>
/// Enumerates all syntax node and token kinds in the MIDL language.
/// </summary>
public enum SyntaxKind : ushort
{
    None = 0,

    // Tokens - Trivia
    WhitespaceTrivia,
    EndOfLineTrivia,
    SingleLineCommentTrivia,
    DocCommentTrivia,
    MultiLineCommentTrivia,
    SkippedTokensTrivia,

    // Tokens - Punctuation
    OpenBraceToken,         // {
    CloseBraceToken,        // }
    OpenParenToken,         // (
    CloseParenToken,        // )
    OpenBracketToken,       // [
    CloseBracketToken,      // ]
    SemicolonToken,         // ;
    ColonToken,             // :
    CommaToken,             // ,
    DotToken,               // .
    EqualsToken,            // =
    PlusToken,              // +
    MinusToken,             // -
    AsteriskToken,          // *
    SlashToken,             // /
    PercentToken,           // %
    AmpersandToken,         // &
    PipeToken,              // |
    CaretToken,             // ^
    TildeToken,             // ~
    ExclamationToken,       // !
    LessThanToken,          // <
    GreaterThanToken,       // >
    LessThanLessThanToken,  // <<
    GreaterThanGreaterThanToken, // >>
    AmpersandAmpersandToken, // &&
    PipePipeToken,          // ||
    EqualsEqualsToken,      // ==
    ExclamationEqualsToken, // !=
    LessThanEqualsToken,    // <=
    GreaterThanEqualsToken, // >=
    PlusPlusToken,          // ++
    MinusMinusToken,        // --
    ArrowToken,             // ->
    ColonColonToken,        // ::

    // Tokens - Literals
    NumericLiteralToken,
    FloatLiteralToken,
    StringLiteralToken,
    GuidLiteralToken,

    // Tokens - Identifiers and keywords
    IdentifierToken,
    /// <summary>
    /// A classic MIDL2/C calling convention (<c>__stdcall</c>, <c>STDMETHODCALLTYPE</c>, ...)
    /// appearing between a method's return type and its name. Kept as its own kind so the
    /// binder can skip it when locating the method name, and strict mode can report it
    /// structurally rather than by matching text in two places.
    /// </summary>
    CallingConventionToken,
    EndOfFileToken,
    ErrorToken,

    // Keywords - MIDL3 type keywords
    BooleanKeyword,
    CharKeyword,
    Int16Keyword,
    Int32Keyword,
    Int64Keyword,
    UInt8Keyword,
    UInt16Keyword,
    UInt32Keyword,
    UInt64Keyword,
    SingleKeyword,
    DoubleKeyword,
    StringKeyword,
    GuidKeyword,
    ObjectKeyword,
    VoidKeyword,

    // Keywords - Declaration keywords
    NamespaceKeyword,
    RuntimeclassKeyword,
    InterfaceKeyword,
    DelegateKeyword,
    StructKeyword,
    EnumKeyword,
    AttributeKeyword,
    ApicontractKeyword,
    EventKeyword,
    ImportKeyword,
    DeclareKeyword,

    // Keywords - Modifier keywords
    StaticKeyword,
    PartialKeyword,
    UnsealedKeyword,
    ProtectedKeyword,
    OverridableKeyword,
    OutKeyword,
    RefKeyword,
    ConstKeyword,
    GetKeyword,
    SetKeyword,

    // Keywords - Classic MIDL
    CoclassKeyword,
    LibraryKeyword,
    ModuleKeyword,
    TypedefKeyword,
    UnionKeyword,
    SwitchKeyword,
    CaseKeyword,
    DefaultKeyword,
    DispinterfaceKeyword,
    TrueKeyword,
    FalseKeyword,
    NullKeyword,
    SafeArrayKeyword,
    PipeKeyword,
    HResultKeyword,
    CppQuoteKeyword,
    ByteKeyword,
    ShortKeyword,
    LongKeyword,
    IntKeyword,
    FloatKeyword,
    HyperKeyword,
    UnsignedKeyword,
    SignedKeyword,
    SmallKeyword,
    WcharTKeyword,
    HandleTKeyword,
    ErrorStatusTKeyword,
    ImportlibKeyword,
    InKeyword,
    RetvalKeyword,

    // Nodes - Top level
    CompilationUnit,
    NamespaceDeclaration,
    ImportDirective,
    DeclareBlock,
    CppQuoteDirective,
    ImportlibDirective,

    // Nodes - Type declarations
    RuntimeClassDeclaration,
    InterfaceDeclaration,
    DelegateDeclaration,
    StructDeclaration,
    EnumDeclaration,
    ApiContractDeclaration,
    AttributeDeclaration,
    CoclassDeclaration,
    DispinterfaceDeclaration,
    /// <summary>A classic RPC free function declared at file scope, outside any interface.</summary>
    FunctionDeclaration,
    LibraryDeclaration,
    ModuleDeclaration,
    TypedefDeclaration,
    UnionDeclaration,

    // Nodes - Members
    ConstructorDeclaration,
    MethodDeclaration,
    PropertyDeclaration,
    EventDeclaration,
    FieldDeclaration,
    EnumMemberDeclaration,
    CaseLabel,
    DefaultLabel,
    UnionArm,
    SwitchBody,

    // Keywords - Contextual
    RequiresKeyword,
    ContractKeyword,
    FlagsKeyword,

    // Nodes - Clauses
    BaseList,
    BaseType,
    AttributeList,
    Attribute,
    AttributeArgument,
    AttributeArgumentList,
    ParameterList,
    Parameter,
    TypeReference,
    GenericName,
    QualifiedName,
    TypeArgumentList,
    ArrayType,
    VersionedBlock,
    PropertyAccessors,
    RequiresClause,
    SimpleName,
    DottedName,
    ModifierList,
    PointerType,

    // Nodes - Expressions
    NumericLiteralExpression,
    StringLiteralExpression,
    IdentifierExpression,
    BinaryExpression,
    UnaryExpression,
    ParenthesizedExpression,
    GuidExpression,

    // Nodes - Error recovery
    SkippedTokens,
    MissingToken,
}
