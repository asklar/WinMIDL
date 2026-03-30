using System.Text;
using System.Xml;
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
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            OmitXmlDeclaration = false,
            Encoding = Encoding.UTF8
        };

        var sb = new StringBuilder();
        using (var writer = XmlWriter.Create(sb, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("doc");

            writer.WriteStartElement("assembly");
            writer.WriteElementString("name", assemblyName);
            writer.WriteEndElement(); // assembly

            writer.WriteStartElement("members");

            foreach (var type in compilation.GetAllTypes())
            {
                if (type.IsExternal) continue;
                EmitTypeAndMembers(writer, type);
            }

            writer.WriteEndElement(); // members
            writer.WriteEndElement(); // doc
            writer.WriteEndDocument();
        }

        return sb.ToString();
    }

    private void EmitTypeAndMembers(XmlWriter writer, ISymbol type)
    {
        if (type.DocComment != null)
        {
            var prefix = GetMemberIdPrefix(type);
            writer.WriteStartElement("member");
            writer.WriteAttributeString("name", $"{prefix}:{type.FullName}");
            WriteDocCommentXml(writer, type.DocComment);
            writer.WriteEndElement();
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
            writer.WriteStartElement("member");
            writer.WriteAttributeString("name", memberId);
            WriteDocCommentXml(writer, member.DocComment);
            writer.WriteEndElement();
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

    private static void WriteDocCommentXml(XmlWriter writer, DocComment doc)
    {
        if (!string.IsNullOrEmpty(doc.RawXml))
        {
            // Write the raw XML content directly
            writer.WriteRaw("\n    " + doc.RawXml.Replace("\n", "\n    ") + "\n  ");
        }
        else
        {
            if (doc.Summary != null)
            {
                writer.WriteElementString("summary", doc.Summary);
            }
            foreach (var kvp in doc.Params)
            {
                writer.WriteStartElement("param");
                writer.WriteAttributeString("name", kvp.Key);
                writer.WriteString(kvp.Value);
                writer.WriteEndElement();
            }
            if (doc.Returns != null)
            {
                writer.WriteElementString("returns", doc.Returns);
            }
            if (doc.Remarks != null)
            {
                writer.WriteElementString("remarks", doc.Remarks);
            }
        }
    }
}
