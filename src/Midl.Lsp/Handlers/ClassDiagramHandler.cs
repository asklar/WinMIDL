using System.Text;
using Midl.Analysis;
using Midl.Analysis.Symbols;

namespace Midl.Lsp.Handlers;

public static class ClassDiagramHandler
{
    /// <summary>Generate Mermaid class diagram markdown from a compilation.</summary>
    public static string GenerateMermaid(Compilation compilation, string? title = null)
    {
        var sb = new StringBuilder();
        var diagramTitle = title ?? compilation.GlobalNamespace.Name;
        if (string.IsNullOrEmpty(diagramTitle))
            diagramTitle = "IDL";

        sb.AppendLine($"# Class Diagram: {diagramTitle}");
        sb.AppendLine();
        sb.AppendLine("```mermaid");
        sb.AppendLine("classDiagram");

        var allTypes = compilation.GetAllTypes().Where(t => !t.IsExternal).ToList();
        var relationships = new List<string>();

        foreach (var symbol in allTypes)
        {
            switch (symbol)
            {
                case RuntimeClassSymbol rc:
                    EmitRuntimeClass(sb, rc, relationships);
                    break;
                case InterfaceSymbol iface when !iface.IsSynthesized:
                    EmitInterface(sb, iface, relationships);
                    break;
                case EnumSymbol enumSym:
                    EmitEnum(sb, enumSym);
                    break;
                case StructSymbol structSym:
                    EmitStruct(sb, structSym);
                    break;
                case DelegateSymbol del:
                    EmitDelegate(sb, del);
                    break;
            }
        }

        foreach (var rel in relationships)
            sb.AppendLine(rel);

        sb.AppendLine("```");
        return sb.ToString();
    }

    /// <summary>Generate Graphviz DOT source for a class diagram.</summary>
    public static string GenerateDot(Compilation compilation)
    {
        var sb = new StringBuilder();
        var allTypes = compilation.GetAllTypes().Where(t => !t.IsExternal).ToList();
        var nameToId = new Dictionary<string, string>();
        var edges = new List<(string fromId, string targetName, string kind)>();
        int idx = 0;

        sb.AppendLine("digraph {");
        sb.AppendLine("  layout=fdp; bgcolor=transparent;");
        sb.AppendLine("  graph [pad=\"0.3\", K=\"1.8\", overlap=false, splines=true, start=42];");
        sb.AppendLine("  node [shape=plaintext, margin=\"0\"];");
        sb.AppendLine("  edge [fontname=\"Segoe UI\", fontsize=9, color=\"#888888\", penwidth=1.5];");

        foreach (var symbol in allTypes)
        {
            var nid = $"n{idx++}";
            nameToId[symbol.Name] = nid;

            switch (symbol)
            {
                case EnumSymbol e:
                    EmitDotNode(sb, nid, "enum", "#1e3a1e", "#8fbc8f",
                        e.Name, e.EnumMembers.Select(m => DotH(m.Name)).ToList());
                    break;
                case StructSymbol s:
                    EmitDotNode(sb, nid, "struct", "#3a2a1a", "#deb887",
                        s.Name, s.Fields.Select(f => $"{DotH(f.Type.DisplayName)} <FONT COLOR=\"#9cdcfe\"><B>{DotH(f.Name)}</B></FONT>").ToList());
                    break;
                case DelegateSymbol d:
                    EmitDotNode(sb, nid, "delegate", "#3a1a2a", "#f0b0c0",
                        d.Name, new List<string> { $"{DotH(d.ReturnType.DisplayName)} <FONT COLOR=\"#dcdcaa\"><B>Invoke</B></FONT>({DotH(FormatParameters(d.Parameters))})" });
                    break;
                case InterfaceSymbol iface when !iface.IsSynthesized:
                    EmitDotNode(sb, nid, "interface", "#1a2a3a", "#87ceeb",
                        iface.Name, FormatInterfaceMembers(iface));
                    foreach (var req in iface.RequiredInterfaces)
                        edges.Add((nid, req.Name, "requires"));
                    break;
                case RuntimeClassSymbol rc:
                    EmitDotNode(sb, nid, "class", "#2a1a3a", "#b39ddb",
                        rc.Name, FormatClassMembers(rc));
                    if (rc.BaseClass is { } baseRef)
                    {
                        if (baseRef.IsDefaultInterface)
                        {
                            if (baseRef.ResolvedSymbol is not InterfaceSymbol { IsSynthesized: true })
                                edges.Add((nid, baseRef.Name, "implements"));
                        }
                        else
                            edges.Add((nid, baseRef.Name, "extends"));
                    }
                    foreach (var iface in rc.Interfaces)
                    {
                        if (iface.ResolvedSymbol is not InterfaceSymbol { IsSynthesized: true })
                            edges.Add((nid, iface.Name, "implements"));
                    }
                    break;
            }
        }

        foreach (var (fromId, targetName, kind) in edges)
        {
            if (!nameToId.TryGetValue(targetName, out var toId)) continue;
            var (style, arrow) = kind switch
            {
                "extends" => ("solid", "empty"),
                "implements" => ("dashed", "empty"),
                "requires" => ("dashed", "open"),
                _ => ("solid", "normal")
            };
            sb.AppendLine($"  {fromId} -> {toId} [style={style}, arrowhead={arrow}, arrowsize=1.2, len=4];");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string DotH(string s) => s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

    private static void EmitDotNode(StringBuilder sb, string nid, string kindLabel, string headerBg, string kindColor, string name, List<string> members)
    {
        sb.Append($"  {nid} [label=<");
        sb.Append($"<TABLE BORDER=\"0\" CELLBORDER=\"1\" CELLSPACING=\"0\" CELLPADDING=\"8\" COLOR=\"#555555\">");
        sb.Append($"<TR><TD BGCOLOR=\"{headerBg}\" ALIGN=\"CENTER\">");
        sb.Append($"<FONT COLOR=\"{kindColor}\" FACE=\"Segoe UI\" POINT-SIZE=\"9\">&laquo;{kindLabel}&raquo;</FONT><BR/>");
        sb.Append($"<FONT COLOR=\"#e0e0e0\" FACE=\"Segoe UI\" POINT-SIZE=\"12\"><B>{DotH(name)}</B></FONT></TD></TR>");
        if (members.Count > 0)
        {
            sb.Append("<TR><TD BGCOLOR=\"#2d2d2d\" ALIGN=\"LEFT\"><FONT COLOR=\"#999999\" FACE=\"Consolas\" POINT-SIZE=\"10\">");
            foreach (var m in members)
                sb.Append($"{m}<BR ALIGN=\"LEFT\"/>");
            sb.Append("</FONT></TD></TR>");
        }
        sb.AppendLine("</TABLE>>];");
    }

    private static List<string> FormatInterfaceMembers(InterfaceSymbol iface)
    {
        var list = new List<string>();
        foreach (var m in iface.Members)
        {
            switch (m)
            {
                case MethodSymbol method:
                    list.Add($"{DotH(method.ReturnType.DisplayName)} <FONT COLOR=\"#dcdcaa\"><B>{DotH(method.Name)}</B></FONT>({DotH(FormatParameters(method.Parameters))})");
                    break;
                case PropertySymbol prop:
                    list.Add($"{DotH(prop.Type.DisplayName)} <FONT COLOR=\"#9cdcfe\"><B>{DotH(prop.Name)}</B></FONT>");
                    break;
                case EventSymbol evt:
                    list.Add($"event {DotH(evt.DelegateType.DisplayName)} <FONT COLOR=\"#c586c0\"><B>{DotH(evt.Name)}</B></FONT>");
                    break;
            }
        }
        return list;
    }

    private static List<string> FormatClassMembers(RuntimeClassSymbol rc)
    {
        var list = new List<string>();
        foreach (var m in rc.Members)
        {
            switch (m)
            {
                case ConstructorSymbol ctor:
                    list.Add($"<FONT COLOR=\"#dcdcaa\"><B>{DotH(rc.Name)}</B></FONT>({DotH(FormatParameters(ctor.Parameters))})");
                    break;
                case MethodSymbol method:
                    list.Add($"{DotH(method.ReturnType.DisplayName)} <FONT COLOR=\"#dcdcaa\"><B>{DotH(method.Name)}</B></FONT>({DotH(FormatParameters(method.Parameters))})");
                    break;
                case PropertySymbol prop:
                    list.Add($"{DotH(prop.Type.DisplayName)} <FONT COLOR=\"#9cdcfe\"><B>{DotH(prop.Name)}</B></FONT>");
                    break;
                case EventSymbol evt:
                    list.Add($"event {DotH(evt.DelegateType.DisplayName)} <FONT COLOR=\"#c586c0\"><B>{DotH(evt.Name)}</B></FONT>");
                    break;
            }
        }
        return list;
    }

    private static void EmitRuntimeClass(StringBuilder sb, RuntimeClassSymbol rc, List<string> relationships)
    {
        sb.AppendLine($"    class {rc.Name} {{");

        foreach (var member in rc.Members)
        {
            switch (member)
            {
                case ConstructorSymbol ctor:
                    var ctorParams = FormatParameters(ctor.Parameters);
                    sb.AppendLine($"        +{rc.Name}({ctorParams})");
                    break;
                case MethodSymbol method:
                    EmitMethod(sb, method);
                    break;
                case PropertySymbol prop:
                    EmitProperty(sb, prop);
                    break;
                case EventSymbol evt:
                    EmitEvent(sb, evt);
                    break;
            }
        }

        sb.AppendLine("    }");

        // BaseClass can be either a real base class (extends) or a default interface (implements)
        if (rc.BaseClass is { } baseRef)
        {
            if (baseRef.IsDefaultInterface)
                AddImplementsIfNotSynthesized(relationships, rc.Name, baseRef);
            else
                relationships.Add($"    {rc.Name} --|> {baseRef.Name} : extends");
        }

        foreach (var iface in rc.Interfaces)
            AddImplementsIfNotSynthesized(relationships, rc.Name, iface);
    }

    private static void AddImplementsIfNotSynthesized(List<string> relationships, string className, TypeRef ifaceRef)
    {
        if (ifaceRef.ResolvedSymbol is InterfaceSymbol { IsSynthesized: true })
            return;
        relationships.Add($"    {className} ..|> {ifaceRef.Name} : implements");
    }

    private static void EmitInterface(StringBuilder sb, InterfaceSymbol iface, List<string> relationships)
    {
        sb.AppendLine($"    class {iface.Name} {{");
        sb.AppendLine("        <<interface>>");

        foreach (var member in iface.Members)
        {
            switch (member)
            {
                case MethodSymbol method:
                    EmitMethod(sb, method);
                    break;
                case PropertySymbol prop:
                    EmitProperty(sb, prop);
                    break;
                case EventSymbol evt:
                    EmitEvent(sb, evt);
                    break;
            }
        }

        sb.AppendLine("    }");

        foreach (var req in iface.RequiredInterfaces)
            relationships.Add($"    {iface.Name} ..|> {req.Name} : requires");
    }

    private static void EmitEnum(StringBuilder sb, EnumSymbol enumSym)
    {
        sb.AppendLine($"    class {enumSym.Name} {{");
        sb.AppendLine("        <<enumeration>>");

        foreach (var member in enumSym.EnumMembers)
            sb.AppendLine($"        {member.Name}");

        sb.AppendLine("    }");
    }

    private static void EmitStruct(StringBuilder sb, StructSymbol structSym)
    {
        sb.AppendLine($"    class {structSym.Name} {{");
        sb.AppendLine("        <<struct>>");

        foreach (var field in structSym.Fields)
            sb.AppendLine($"        +{field.Type.DisplayName} {field.Name}");

        sb.AppendLine("    }");
    }

    private static void EmitDelegate(StringBuilder sb, DelegateSymbol del)
    {
        var returnType = del.ReturnType.DisplayName;
        var paramList = FormatParameters(del.Parameters);

        sb.AppendLine($"    class {del.Name} {{");
        sb.AppendLine("        <<delegate>>");
        sb.AppendLine($"        +{returnType} Invoke({paramList})");
        sb.AppendLine("    }");
    }

    private static void EmitMethod(StringBuilder sb, MethodSymbol method)
    {
        var returnType = method.ReturnType.DisplayName;
        var paramList = FormatParameters(method.Parameters);
        var staticSuffix = method.IsStatic ? "$" : "";
        sb.AppendLine($"        +{returnType} {method.Name}({paramList}){staticSuffix}");
    }

    private static void EmitProperty(StringBuilder sb, PropertySymbol prop)
    {
        var staticSuffix = prop.IsStatic ? "$" : "";
        sb.AppendLine($"        +{prop.Type.DisplayName} {prop.Name}{staticSuffix}");
    }

    private static void EmitEvent(StringBuilder sb, EventSymbol evt)
    {
        var staticSuffix = evt.IsStatic ? "$" : "";
        sb.AppendLine($"        +event {evt.DelegateType.DisplayName} {evt.Name}{staticSuffix}");
    }

    private static string FormatParameters(List<ParameterSymbol> parameters)
    {
        return string.Join(", ", parameters.Select(p => p.Type.DisplayName));
    }
}
