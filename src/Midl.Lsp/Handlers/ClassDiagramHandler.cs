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

        // Categorize types by kind
        var enums = new List<EnumSymbol>();
        var structs = new List<StructSymbol>();
        var delegates = new List<DelegateSymbol>();
        var interfaces = new List<InterfaceSymbol>();
        var classes = new List<RuntimeClassSymbol>();

        foreach (var symbol in allTypes)
        {
            switch (symbol)
            {
                case RuntimeClassSymbol rc: classes.Add(rc); break;
                case InterfaceSymbol iface when !iface.IsSynthesized: interfaces.Add(iface); break;
                case EnumSymbol e: enums.Add(e); break;
                case StructSymbol s: structs.Add(s); break;
                case DelegateSymbol d: delegates.Add(d); break;
            }
        }

        int totalTypes = enums.Count + structs.Count + delegates.Count + interfaces.Count + classes.Count;
        bool useGroups = totalTypes > 4;

        if (useGroups)
        {
            EmitNamespaceGroup(sb, "Enumerations", enums, (s, e) => EmitEnum(s, e));
            EmitNamespaceGroup(sb, "Structures", structs, (s, st) => EmitStruct(s, st));
            EmitNamespaceGroup(sb, "Delegates", delegates, (s, d) => EmitDelegate(s, d));
            EmitNamespaceGroup(sb, "Interfaces", interfaces, (s, i) => EmitInterface(s, i, relationships));
            EmitNamespaceGroup(sb, "Classes", classes, (s, rc) => EmitRuntimeClass(s, rc, relationships));
        }
        else
        {
            foreach (var e in enums) EmitEnum(sb, e);
            foreach (var s in structs) EmitStruct(sb, s);
            foreach (var d in delegates) EmitDelegate(sb, d);
            foreach (var i in interfaces) EmitInterface(sb, i, relationships);
            foreach (var rc in classes) EmitRuntimeClass(sb, rc, relationships);
        }

        foreach (var rel in relationships)
            sb.AppendLine(rel);

        sb.AppendLine("```");
        return sb.ToString();
    }

    private static void EmitNamespaceGroup<T>(StringBuilder sb, string groupName, List<T> items, Action<StringBuilder, T> emitter)
    {
        if (items.Count == 0) return;
        sb.AppendLine($"    namespace {groupName} {{");
        foreach (var item in items) emitter(sb, item);
        sb.AppendLine("    }");
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
