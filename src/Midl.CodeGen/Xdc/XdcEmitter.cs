using System.Text;
using Midl.Analysis;
using Midl.Analysis.Symbols;

namespace Midl.CodeGen.Xdc;

/// <summary>
/// Emits an XDC (XML Documentation Comments) file from a compilation.
/// Only members with doc comments are included.
/// </summary>
public class XdcEmitter
{
    /// <summary>
    /// Emit XDC XML for the compilation.
    /// </summary>
    public string Emit(Compilation compilation, string assemblyName)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<doc>");
        sb.AppendLine("  <assembly>");
        sb.Append("    <name>").Append(Escape(assemblyName)).AppendLine("</name>");
        sb.AppendLine("  </assembly>");
        sb.AppendLine("  <members>");

        foreach (var type in compilation.GetAllTypes())
        {
            if (type.IsExternal) continue;
            EmitTypeAndMembers(sb, type);
        }

        sb.AppendLine("  </members>");
        sb.AppendLine("</doc>");

        return sb.ToString();
    }

    private void EmitTypeAndMembers(StringBuilder sb, ISymbol type)
    {
        if (type.DocComment != null)
        {
            var prefix = GetMemberIdPrefix(type);
            sb.Append("    <member name=\"").Append(Escape($"{prefix}:{type.FullName}")).AppendLine("\">");
            WriteDocCommentXml(sb, type.DocComment);
            sb.AppendLine("    </member>");
        }

        // Emit members
        IReadOnlyList<ISymbol>? members = type switch
        {
            RuntimeClassSymbol rc => rc.Members,
            InterfaceSymbol iface => iface.Members,
            StructSymbol s => CombineStructMembers(s),
            EnumSymbol e => e.EnumMembers.Cast<ISymbol>().ToList(),
            DelegateSymbol => null,
            _ => null
        };

        if (members == null) return;

        foreach (var member in members)
        {
            if (member.DocComment == null) continue;

            var memberId = GetMemberId(member, type);
            sb.Append("    <member name=\"").Append(Escape(memberId)).AppendLine("\">");
            WriteDocCommentXml(sb, member.DocComment);
            sb.AppendLine("    </member>");
        }
    }

    private static IReadOnlyList<ISymbol> CombineStructMembers(StructSymbol s)
    {
        return s.Fields.Cast<ISymbol>().ToList();
    }

    private static string GetMemberIdPrefix(ISymbol type)
    {
        return type.Kind switch
        {
            SymbolKind.RuntimeClass => "T",
            SymbolKind.Interface => "T",
            SymbolKind.Struct => "T",
            SymbolKind.Enum => "T",
            SymbolKind.Delegate => "T",
            SymbolKind.ApiContract => "T",
            _ => "T"
        };
    }

    private static string GetMemberId(ISymbol member, ISymbol containingType)
    {
        var typeName = containingType.FullName;

        return member switch
        {
            MethodSymbol m => $"M:{typeName}.{m.Name}({FormatParams(m.Parameters)})",
            ConstructorSymbol c => $"M:{typeName}.{c.Name}({FormatParams(c.Parameters)})",
            PropertySymbol p => $"P:{typeName}.{p.Name}",
            EventSymbol e => $"E:{typeName}.{e.Name}",
            FieldSymbol f => $"F:{typeName}.{f.Name}",
            EnumMemberSymbol em => $"F:{typeName}.{em.Name}",
            _ => $"M:{typeName}.{member.Name}"
        };
    }

    private static string FormatParams(IReadOnlyList<ParameterSymbol> parameters)
    {
        if (parameters.Count == 0) return "";
        return string.Join(",", parameters.Select(p => p.Type?.Name ?? "Object"));
    }

    private static void WriteDocCommentXml(StringBuilder sb, DocComment doc)
    {
        if (!string.IsNullOrEmpty(doc.RawXml))
        {
            sb.Append("    ").Append(doc.RawXml.Replace("\n", "\n    ")).AppendLine();
        }
        else
        {
            if (doc.Summary != null)
                sb.Append("      <summary>").Append(Escape(doc.Summary)).AppendLine("</summary>");
            foreach (var kvp in doc.Params)
                sb.Append("      <param name=\"").Append(Escape(kvp.Key)).Append("\">").Append(Escape(kvp.Value)).AppendLine("</param>");
            if (doc.Returns != null)
                sb.Append("      <returns>").Append(Escape(doc.Returns)).AppendLine("</returns>");
            if (doc.Remarks != null)
                sb.Append("      <remarks>").Append(Escape(doc.Remarks)).AppendLine("</remarks>");
        }
    }

    private static string Escape(string text)
    {
        if (text.IndexOfAny(new[] { '&', '<', '>', '"' }) < 0)
            return text;
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }
}
