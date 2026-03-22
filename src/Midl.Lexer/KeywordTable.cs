namespace Midl.Lexer;

using Midl.Syntax;

/// <summary>
/// Maps keyword strings to their corresponding <see cref="SyntaxKind"/>.
/// </summary>
public static class KeywordTable
{
    private static readonly Dictionary<string, (SyntaxKind Kind, string Text)> s_keywords = new(StringComparer.Ordinal)
    {
        // MIDL3 type keywords
        ["Boolean"] = (SyntaxKind.BooleanKeyword, "Boolean"),
        ["Char"] = (SyntaxKind.CharKeyword, "Char"),
        ["Int16"] = (SyntaxKind.Int16Keyword, "Int16"),
        ["Int32"] = (SyntaxKind.Int32Keyword, "Int32"),
        ["Int64"] = (SyntaxKind.Int64Keyword, "Int64"),
        ["UInt8"] = (SyntaxKind.UInt8Keyword, "UInt8"),
        ["UInt16"] = (SyntaxKind.UInt16Keyword, "UInt16"),
        ["UInt32"] = (SyntaxKind.UInt32Keyword, "UInt32"),
        ["UInt64"] = (SyntaxKind.UInt64Keyword, "UInt64"),
        ["Single"] = (SyntaxKind.SingleKeyword, "Single"),
        ["Double"] = (SyntaxKind.DoubleKeyword, "Double"),
        ["String"] = (SyntaxKind.StringKeyword, "String"),
        ["Guid"] = (SyntaxKind.GuidKeyword, "Guid"),
        ["Object"] = (SyntaxKind.ObjectKeyword, "Object"),

        // Declaration keywords
        ["namespace"] = (SyntaxKind.NamespaceKeyword, "namespace"),
        ["runtimeclass"] = (SyntaxKind.RuntimeclassKeyword, "runtimeclass"),
        ["interface"] = (SyntaxKind.InterfaceKeyword, "interface"),
        ["delegate"] = (SyntaxKind.DelegateKeyword, "delegate"),
        ["struct"] = (SyntaxKind.StructKeyword, "struct"),
        ["enum"] = (SyntaxKind.EnumKeyword, "enum"),
        ["attribute"] = (SyntaxKind.AttributeKeyword, "attribute"),
        ["apicontract"] = (SyntaxKind.ApicontractKeyword, "apicontract"),
        ["event"] = (SyntaxKind.EventKeyword, "event"),
        ["import"] = (SyntaxKind.ImportKeyword, "import"),
        ["declare"] = (SyntaxKind.DeclareKeyword, "declare"),
#if WITH_COMPAT
        ["coclass"] = (SyntaxKind.CoclassKeyword, "coclass"),
        ["library"] = (SyntaxKind.LibraryKeyword, "library"),
        ["module"] = (SyntaxKind.ModuleKeyword, "module"),
        ["typedef"] = (SyntaxKind.TypedefKeyword, "typedef"),
        ["union"] = (SyntaxKind.UnionKeyword, "union"),
        ["dispinterface"] = (SyntaxKind.DispinterfaceKeyword, "dispinterface"),
#endif
        ["contract"] = (SyntaxKind.ContractKeyword, "contract"),

        // Modifier keywords
        ["static"] = (SyntaxKind.StaticKeyword, "static"),
        ["partial"] = (SyntaxKind.PartialKeyword, "partial"),
        ["unsealed"] = (SyntaxKind.UnsealedKeyword, "unsealed"),
        ["protected"] = (SyntaxKind.ProtectedKeyword, "protected"),
        ["overridable"] = (SyntaxKind.OverridableKeyword, "overridable"),
        ["out"] = (SyntaxKind.OutKeyword, "out"),
        ["ref"] = (SyntaxKind.RefKeyword, "ref"),
        ["const"] = (SyntaxKind.ConstKeyword, "const"),
        ["get"] = (SyntaxKind.GetKeyword, "get"),
        ["set"] = (SyntaxKind.SetKeyword, "set"),
        ["void"] = (SyntaxKind.VoidKeyword, "void"),
        ["default"] = (SyntaxKind.DefaultKeyword, "default"),
        ["requires"] = (SyntaxKind.RequiresKeyword, "requires"),

        // Classic MIDL keywords
        ["switch"] = (SyntaxKind.SwitchKeyword, "switch"),
        ["case"] = (SyntaxKind.CaseKeyword, "case"),
        ["TRUE"] = (SyntaxKind.TrueKeyword, "TRUE"),
        ["FALSE"] = (SyntaxKind.FalseKeyword, "FALSE"),
        ["NULL"] = (SyntaxKind.NullKeyword, "NULL"),
        ["SAFEARRAY"] = (SyntaxKind.SafeArrayKeyword, "SAFEARRAY"),
        ["pipe"] = (SyntaxKind.PipeKeyword, "pipe"),
        ["HRESULT"] = (SyntaxKind.HResultKeyword, "HRESULT"),
#if WITH_COMPAT
        ["cpp_quote"] = (SyntaxKind.CppQuoteKeyword, "cpp_quote"),
        ["boolean"] = (SyntaxKind.BooleanKeyword, "boolean"),
        ["byte"] = (SyntaxKind.ByteKeyword, "byte"),
        ["short"] = (SyntaxKind.ShortKeyword, "short"),
        ["long"] = (SyntaxKind.LongKeyword, "long"),
        ["int"] = (SyntaxKind.IntKeyword, "int"),
        ["float"] = (SyntaxKind.FloatKeyword, "float"),
        ["double"] = (SyntaxKind.DoubleKeyword, "double"),
        ["hyper"] = (SyntaxKind.HyperKeyword, "hyper"),
        ["unsigned"] = (SyntaxKind.UnsignedKeyword, "unsigned"),
        ["signed"] = (SyntaxKind.SignedKeyword, "signed"),
        ["small"] = (SyntaxKind.SmallKeyword, "small"),
        ["wchar_t"] = (SyntaxKind.WcharTKeyword, "wchar_t"),
        ["handle_t"] = (SyntaxKind.HandleTKeyword, "handle_t"),
        ["error_status_t"] = (SyntaxKind.ErrorStatusTKeyword, "error_status_t"),
        ["importlib"] = (SyntaxKind.ImportlibKeyword, "importlib"),
#endif
        ["in"] = (SyntaxKind.InKeyword, "in"),
        ["retval"] = (SyntaxKind.RetvalKeyword, "retval"),
        ["char"] = (SyntaxKind.CharKeyword, "char"),
    };

    public static bool TryGetKeywordKind(string text, out SyntaxKind kind)
    {
        if (s_keywords.TryGetValue(text, out var entry))
        {
            kind = entry.Kind;
            return true;
        }
        kind = default;
        return false;
    }

    /// <summary>
    /// Looks up a keyword and returns the canonical (interned) text string,
    /// avoiding a duplicate string allocation.
    /// </summary>
    public static bool TryGetKeyword(string text, out SyntaxKind kind, out string canonicalText)
    {
        if (s_keywords.TryGetValue(text, out var entry))
        {
            kind = entry.Kind;
            canonicalText = entry.Text;
            return true;
        }
        kind = default;
        canonicalText = text;
        return false;
    }
}
