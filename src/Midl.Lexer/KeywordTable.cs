namespace Midl.Lexer;

using Midl.Syntax;

/// <summary>
/// Maps keyword strings to their corresponding <see cref="SyntaxKind"/>.
/// </summary>
public static class KeywordTable
{
    private static readonly Dictionary<string, SyntaxKind> s_keywords = new()
    {
        // MIDL3 type keywords
        ["Boolean"] = SyntaxKind.BooleanKeyword,
        ["Char"] = SyntaxKind.CharKeyword,
        ["Int16"] = SyntaxKind.Int16Keyword,
        ["Int32"] = SyntaxKind.Int32Keyword,
        ["Int64"] = SyntaxKind.Int64Keyword,
        ["UInt8"] = SyntaxKind.UInt8Keyword,
        ["UInt16"] = SyntaxKind.UInt16Keyword,
        ["UInt32"] = SyntaxKind.UInt32Keyword,
        ["UInt64"] = SyntaxKind.UInt64Keyword,
        ["Single"] = SyntaxKind.SingleKeyword,
        ["Double"] = SyntaxKind.DoubleKeyword,
        ["String"] = SyntaxKind.StringKeyword,
        ["Guid"] = SyntaxKind.GuidKeyword,
        ["Object"] = SyntaxKind.ObjectKeyword,

        // Declaration keywords
        ["namespace"] = SyntaxKind.NamespaceKeyword,
        ["runtimeclass"] = SyntaxKind.RuntimeclassKeyword,
        ["interface"] = SyntaxKind.InterfaceKeyword,
        ["delegate"] = SyntaxKind.DelegateKeyword,
        ["struct"] = SyntaxKind.StructKeyword,
        ["enum"] = SyntaxKind.EnumKeyword,
        ["attribute"] = SyntaxKind.AttributeKeyword,
        ["apicontract"] = SyntaxKind.ApicontractKeyword,
        ["event"] = SyntaxKind.EventKeyword,
        ["import"] = SyntaxKind.ImportKeyword,
        ["declare"] = SyntaxKind.DeclareKeyword,
#if WITH_COMPAT
        ["coclass"] = SyntaxKind.CoclassKeyword,
        ["library"] = SyntaxKind.LibraryKeyword,
        ["module"] = SyntaxKind.ModuleKeyword,
        ["typedef"] = SyntaxKind.TypedefKeyword,
        ["union"] = SyntaxKind.UnionKeyword,
        ["dispinterface"] = SyntaxKind.DispinterfaceKeyword,
#endif
        ["contract"] = SyntaxKind.ContractKeyword,

        // Modifier keywords
        ["static"] = SyntaxKind.StaticKeyword,
        ["partial"] = SyntaxKind.PartialKeyword,
        ["unsealed"] = SyntaxKind.UnsealedKeyword,
        ["protected"] = SyntaxKind.ProtectedKeyword,
        ["overridable"] = SyntaxKind.OverridableKeyword,
        ["out"] = SyntaxKind.OutKeyword,
        ["ref"] = SyntaxKind.RefKeyword,
        ["const"] = SyntaxKind.ConstKeyword,
        ["get"] = SyntaxKind.GetKeyword,
        ["set"] = SyntaxKind.SetKeyword,
        ["void"] = SyntaxKind.VoidKeyword,
        ["default"] = SyntaxKind.DefaultKeyword,
        ["requires"] = SyntaxKind.RequiresKeyword,

        // Classic MIDL keywords
        ["switch"] = SyntaxKind.SwitchKeyword,
        ["case"] = SyntaxKind.CaseKeyword,
        ["TRUE"] = SyntaxKind.TrueKeyword,
        ["FALSE"] = SyntaxKind.FalseKeyword,
        ["NULL"] = SyntaxKind.NullKeyword,
        ["SAFEARRAY"] = SyntaxKind.SafeArrayKeyword,
        ["pipe"] = SyntaxKind.PipeKeyword,
        ["HRESULT"] = SyntaxKind.HResultKeyword,
#if WITH_COMPAT
        ["cpp_quote"] = SyntaxKind.CppQuoteKeyword,
        ["boolean"] = SyntaxKind.BooleanKeyword,
        ["byte"] = SyntaxKind.ByteKeyword,
        ["short"] = SyntaxKind.ShortKeyword,
        ["long"] = SyntaxKind.LongKeyword,
        ["int"] = SyntaxKind.IntKeyword,
        ["float"] = SyntaxKind.FloatKeyword,
        ["double"] = SyntaxKind.DoubleKeyword,
        ["hyper"] = SyntaxKind.HyperKeyword,
        ["unsigned"] = SyntaxKind.UnsignedKeyword,
        ["signed"] = SyntaxKind.SignedKeyword,
        ["small"] = SyntaxKind.SmallKeyword,
        ["wchar_t"] = SyntaxKind.WcharTKeyword,
        ["handle_t"] = SyntaxKind.HandleTKeyword,
        ["error_status_t"] = SyntaxKind.ErrorStatusTKeyword,
        ["importlib"] = SyntaxKind.ImportlibKeyword,
#endif
        ["in"] = SyntaxKind.InKeyword,
        ["retval"] = SyntaxKind.RetvalKeyword,
        ["char"] = SyntaxKind.CharKeyword,
    };

    public static bool TryGetKeywordKind(string text, out SyntaxKind kind)
    {
        return s_keywords.TryGetValue(text, out kind);
    }
}
