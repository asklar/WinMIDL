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

    /// <summary>Generate an HTML class diagram with CSS Grid card layout.</summary>
    public static string GenerateHtml(Compilation compilation, string? title = null)
    {
        var sb = new StringBuilder();
        var diagramTitle = title ?? compilation.GlobalNamespace.Name;
        if (string.IsNullOrEmpty(diagramTitle))
            diagramTitle = "IDL";

        var allTypes = compilation.GetAllTypes().Where(t => !t.IsExternal).ToList();

        sb.Append(@"<!DOCTYPE html><html lang=""en""><head><meta charset=""UTF-8""><style>
*{box-sizing:border-box}
body{font-family:var(--vscode-font-family,'Segoe UI',sans-serif);background:var(--vscode-editor-background,#1e1e1e);color:var(--vscode-editor-foreground,#ccc);margin:0;padding:16px}
h1{font-size:16px;margin:0 0 14px;font-weight:400;opacity:.8}
.g{display:grid;grid-template-columns:repeat(auto-fill,minmax(250px,1fr));gap:10px;align-items:start}
.c{border:1px solid var(--vscode-panel-border,#444);border-radius:5px;overflow:hidden;background:var(--vscode-editorWidget-background,#252526)}
.ch{padding:6px 10px;font-weight:600;font-size:13px;display:flex;align-items:center;gap:6px}
.kb{font-size:10px;padding:1px 5px;border-radius:3px;font-weight:400;opacity:.7;border:1px solid currentColor}
.cb{padding:4px 10px;font-size:12px;font-family:var(--vscode-editor-font-family,'Cascadia Code',monospace);line-height:1.5}
.cf{padding:4px 10px;font-size:11px;border-top:1px solid var(--vscode-panel-border,#444);opacity:.7;line-height:1.5}
.ke{background:rgba(80,180,80,.12);border-bottom:2px solid rgba(80,180,80,.4)}
.ks{background:rgba(200,140,50,.12);border-bottom:2px solid rgba(200,140,50,.4)}
.kd{background:rgba(200,80,140,.12);border-bottom:2px solid rgba(200,80,140,.4)}
.ki{background:rgba(70,140,220,.12);border-bottom:2px solid rgba(70,140,220,.4)}
.kc{background:rgba(160,80,220,.12);border-bottom:2px solid rgba(160,80,220,.4)}
</style></head><body>");

        sb.Append($"<h1>{H(diagramTitle)}</h1><div class=\"g\">");

        foreach (var symbol in allTypes)
        {
            switch (symbol)
            {
                case EnumSymbol e: EmitEnumCard(sb, e); break;
                case StructSymbol s: EmitStructCard(sb, s); break;
                case DelegateSymbol d: EmitDelegateCard(sb, d); break;
                case InterfaceSymbol i when !i.IsSynthesized: EmitInterfaceCard(sb, i); break;
                case RuntimeClassSymbol rc: EmitRuntimeClassCard(sb, rc); break;
            }
        }

        sb.Append("</div></body></html>");
        return sb.ToString();
    }

    private static string H(string s) => s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

    private static void CardStart(StringBuilder sb, string kind, string kindCss, string name) =>
        sb.Append($"<div class=\"c\"><div class=\"ch {kindCss}\"><span class=\"kb\">{kind}</span>{H(name)}</div><div class=\"cb\">");

    private static void CardMember(StringBuilder sb, string text) =>
        sb.Append($"<div>{H(text)}</div>");

    private static void CardEnd(StringBuilder sb, List<(string rel, string target)>? rels = null)
    {
        sb.Append("</div>"); // close .cb
        if (rels is { Count: > 0 })
        {
            sb.Append("<div class=\"cf\">");
            foreach (var (rel, target) in rels)
            {
                var icon = rel switch { "extends" => "⬆", "implements" => "◆", "requires" => "▸", _ => "·" };
                sb.Append($"<div>{icon} {rel} <b>{H(target)}</b></div>");
            }
            sb.Append("</div>");
        }
        sb.Append("</div>"); // close .c
    }

    private static void EmitEnumCard(StringBuilder sb, EnumSymbol e)
    {
        CardStart(sb, "enum", "ke", e.Name);
        foreach (var m in e.EnumMembers) CardMember(sb, m.Name);
        CardEnd(sb);
    }

    private static void EmitStructCard(StringBuilder sb, StructSymbol s)
    {
        CardStart(sb, "struct", "ks", s.Name);
        foreach (var f in s.Fields) CardMember(sb, $"{f.Type.DisplayName} {f.Name}");
        CardEnd(sb);
    }

    private static void EmitDelegateCard(StringBuilder sb, DelegateSymbol d)
    {
        CardStart(sb, "delegate", "kd", d.Name);
        CardMember(sb, $"{d.ReturnType.DisplayName} Invoke({FormatParameters(d.Parameters)})");
        CardEnd(sb);
    }

    private static void EmitInterfaceCard(StringBuilder sb, InterfaceSymbol iface)
    {
        CardStart(sb, "interface", "ki", iface.Name);
        foreach (var m in iface.Members)
        {
            switch (m)
            {
                case MethodSymbol method:
                    CardMember(sb, $"{method.ReturnType.DisplayName} {method.Name}({FormatParameters(method.Parameters)}){(method.IsStatic ? " ⊛" : "")}");
                    break;
                case PropertySymbol prop:
                    CardMember(sb, $"{prop.Type.DisplayName} {prop.Name}{(prop.IsStatic ? " ⊛" : "")}");
                    break;
                case EventSymbol evt:
                    CardMember(sb, $"event {evt.DelegateType.DisplayName} {evt.Name}{(evt.IsStatic ? " ⊛" : "")}");
                    break;
            }
        }
        var rels = iface.RequiredInterfaces.Select(r => ("requires", r.Name)).ToList();
        CardEnd(sb, rels);
    }

    private static void EmitRuntimeClassCard(StringBuilder sb, RuntimeClassSymbol rc)
    {
        CardStart(sb, "class", "kc", rc.Name);
        foreach (var m in rc.Members)
        {
            switch (m)
            {
                case ConstructorSymbol ctor:
                    CardMember(sb, $"{rc.Name}({FormatParameters(ctor.Parameters)})");
                    break;
                case MethodSymbol method:
                    CardMember(sb, $"{method.ReturnType.DisplayName} {method.Name}({FormatParameters(method.Parameters)}){(method.IsStatic ? " ⊛" : "")}");
                    break;
                case PropertySymbol prop:
                    CardMember(sb, $"{prop.Type.DisplayName} {prop.Name}{(prop.IsStatic ? " ⊛" : "")}");
                    break;
                case EventSymbol evt:
                    CardMember(sb, $"event {evt.DelegateType.DisplayName} {evt.Name}{(evt.IsStatic ? " ⊛" : "")}");
                    break;
            }
        }
        var rels = new List<(string, string)>();
        if (rc.BaseClass is { } baseRef)
        {
            if (baseRef.IsDefaultInterface)
            {
                if (baseRef.ResolvedSymbol is not InterfaceSymbol { IsSynthesized: true })
                    rels.Add(("implements", baseRef.Name));
            }
            else
                rels.Add(("extends", baseRef.Name));
        }
        foreach (var iface in rc.Interfaces)
        {
            if (iface.ResolvedSymbol is not InterfaceSymbol { IsSynthesized: true })
                rels.Add(("implements", iface.Name));
        }
        CardEnd(sb, rels);
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
