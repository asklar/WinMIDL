using System.Text;
using Midl.Analysis.Symbols;

namespace Midl.CodeGen.Header;

/// <summary>
/// Method parameter formatting helpers that match real midl.exe output:
/// multi-line params, no space after annotation close, separated pointer star,
/// 'result' for retval, propget/propput annotations.
/// </summary>
public partial class HeaderEmitter
{
    /// <summary>
    /// Formats a single parameter as midl.exe does:
    /// <c>/* [in] */Type name</c> or <c>/* [out] */Type * name</c>
    /// No space after */, pointer star separated from type.
    /// </summary>
    private static string FormatParam(ParameterSymbol p)
    {
        var cType = MapType(p.Type);
        if (p.Direction == ParameterDirection.Out)
            return $"/* [out] */{cType} * {p.Name}";
        else if (p.Direction == ParameterDirection.RefConst)
            return $"/* [in] */{cType} * {p.Name}";
        else
            return $"/* [in] */{cType} {p.Name}";
    }

    /// <summary>
    /// Emits a vtable method with multi-line parameter formatting matching real midl.exe.
    /// Used by EmitMethod, EmitPropertyMethods, EmitEventMethods, and EmitDelegateCpp.
    /// </summary>
    private static void EmitVtableMethod(
        StringBuilder sb,
        string methodName,
        List<ParameterSymbol> parameters,
        TypeRef returnType,
        string nsIndent,
        string? methodAnnotation = null)
    {
        var methodIndent = nsIndent + "    ";
        var paramIndent = methodIndent + "    ";

        var paramParts = new List<string>();
        foreach (var p in parameters)
            paramParts.Add(FormatParam(p));

        // If non-void return, add retval out param
        if (returnType.Name != "void")
        {
            var retType = MapType(returnType);
            paramParts.Add($"/* [out, retval] */{retType} * result");
        }

        var annotation = methodAnnotation != null ? $" {methodAnnotation}" : "";

        if (paramParts.Count == 0)
        {
            sb.AppendLine($"{methodIndent}virtual{annotation} HRESULT STDMETHODCALLTYPE {methodName}(void) = 0;");
        }
        else
        {
            sb.AppendLine($"{methodIndent}virtual{annotation} HRESULT STDMETHODCALLTYPE {methodName}(");
            for (int i = 0; i < paramParts.Count; i++)
            {
                var trailing = i < paramParts.Count - 1 ? "," : "";
                sb.AppendLine($"{paramIndent}{paramParts[i]}{trailing}");
            }
            sb.AppendLine($"{paramIndent}) = 0;");
        }
    }

    private static void EmitMethod(StringBuilder sb, MethodSymbol method, string nsIndent = "")
    {
        EmitVtableMethod(sb, method.Name, method.Parameters, method.ReturnType, nsIndent);
    }

    private static void EmitPropertyMethods(StringBuilder sb, PropertySymbol prop, string nsIndent = "")
    {
        var cType = MapType(prop.Type);
        var methodIndent = nsIndent + "    ";
        var paramIndent = methodIndent + "    ";

        if (prop.HasGetter)
        {
            sb.AppendLine($"{methodIndent}virtual /* [propget] */ HRESULT STDMETHODCALLTYPE get_{prop.Name}(");
            sb.AppendLine($"{paramIndent}/* [out, retval] */{cType} * value");
            sb.AppendLine($"{paramIndent}) = 0;");
        }
        if (prop.HasSetter)
        {
            sb.AppendLine($"{methodIndent}virtual /* [propput] */ HRESULT STDMETHODCALLTYPE put_{prop.Name}(");
            sb.AppendLine($"{paramIndent}/* [in] */{cType} value");
            sb.AppendLine($"{paramIndent}) = 0;");
        }
    }

    private static void EmitEventMethods(StringBuilder sb, EventSymbol evt, string nsIndent = "")
    {
        var delegateType = MapType(evt.DelegateType);
        var methodIndent = nsIndent + "    ";
        var paramIndent = methodIndent + "    ";

        // add_ method
        sb.AppendLine($"{methodIndent}virtual HRESULT STDMETHODCALLTYPE add_{evt.Name}(");
        sb.AppendLine($"{paramIndent}/* [in] */{delegateType} handler,");
        sb.AppendLine($"{paramIndent}/* [out, retval] */EventRegistrationToken * token");
        sb.AppendLine($"{paramIndent}) = 0;");

        // remove_ method
        sb.AppendLine($"{methodIndent}virtual HRESULT STDMETHODCALLTYPE remove_{evt.Name}(");
        sb.AppendLine($"{paramIndent}/* [in] */EventRegistrationToken token");
        sb.AppendLine($"{paramIndent}) = 0;");
    }
}
