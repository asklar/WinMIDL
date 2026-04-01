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
