// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis.Symbols;

namespace Midl.Analysis;

/// <summary>
/// Walks all RuntimeClassSymbols after binding and synthesizes the required
/// exclusive WinRT interfaces (instance, factory, statics, protected, overrides).
/// </summary>
public partial class InterfaceSynthesizer
{
    public void Synthesize(Compilation compilation)
    {
        foreach (var type in compilation.GetAllTypes().ToList())
        {
            if (type is RuntimeClassSymbol rc)
            {
                SynthesizeForClass(rc);
            }
        }
    }

    private void SynthesizeForClass(RuntimeClassSymbol rc)
    {
        var ns = rc.ContainingSymbol as NamespaceSymbol;

        // Collect members by category
        var instanceMembers = new List<ISymbol>();
        var staticMembers = new List<ISymbol>();
        var protectedMembers = new List<ISymbol>();
        var overridableMembers = new List<ISymbol>();
        var parameterizedCtors = new List<ConstructorSymbol>();
        bool hasDefaultCtor = false;

        foreach (var member in rc.Members)
        {
            switch (member)
            {
                case ConstructorSymbol ctor:
                    if (ctor.Parameters.Count == 0)
                        hasDefaultCtor = true;
                    else
                        parameterizedCtors.Add(ctor);
                    break;

                case MethodSymbol method:
                    if (method.IsProtected)
                        protectedMembers.Add(method);
                    else if (method.IsOverridable)
                        overridableMembers.Add(method);
                    else if (method.IsStatic)
                        staticMembers.Add(method);
                    else
                        instanceMembers.Add(method);
                    break;

                case PropertySymbol prop:
                    if (prop.IsProtected)
                        protectedMembers.Add(prop);
                    else if (prop.IsOverridable)
                        overridableMembers.Add(prop);
                    else if (prop.IsStatic)
                        staticMembers.Add(prop);
                    else
                        instanceMembers.Add(prop);
                    break;

                case EventSymbol evt:
                    if (evt.IsProtected)
                        protectedMembers.Add(evt);
                    else if (evt.IsOverridable)
                        overridableMembers.Add(evt);
                    else if (evt.IsStatic)
                        staticMembers.Add(evt);
                    else
                        instanceMembers.Add(evt);
                    break;
            }
        }

        // Rule A: Instance member interface
        if (instanceMembers.Count > 0)
        {
            var ifaceName = rc.DefaultInterfaceName != null
                ? ExtractSimpleName(rc.DefaultInterfaceName)
                : GetUniqueName($"I{rc.Name}", ns);
            var iface = CreateExclusiveInterface(ifaceName, ns, rc.Name, instanceMembers);
            if (rc.DefaultInterfaceGuid != null)
                iface.Uuid = rc.DefaultInterfaceGuid;
            rc.DefaultInterface = iface;
            rc.ImplementedInterfaces.Add(iface);
        }

        // Rule B: Factory interface (parameterized constructors)
        if (parameterizedCtors.Count > 0)
        {
            var ifaceName = rc.FactoryInterfaceName != null
                ? ExtractSimpleName(rc.FactoryInterfaceName)
                : GetUniqueName($"I{rc.Name}Factory", ns);
            var factoryMembers = new List<ISymbol>();
            foreach (var ctor in parameterizedCtors)
            {
                // Convert constructor to a factory method named CreateInstance
                var factoryMethod = new MethodSymbol(
                    ctor.Parameters.Count == 1 ? "CreateInstance" : $"CreateInstance",
                    null);
                factoryMethod.ReturnType = new TypeRef(rc.Name) { ResolvedSymbol = rc };
                factoryMethod.Parameters.AddRange(ctor.Parameters);
                factoryMembers.Add(factoryMethod);
            }
            var iface = CreateExclusiveInterface(ifaceName, ns, rc.Name, factoryMembers);
            if (rc.FactoryInterfaceGuid != null)
                iface.Uuid = rc.FactoryInterfaceGuid;
            rc.FactoryInterface = iface;
            rc.ImplementedInterfaces.Add(iface);
        }

        // Rule C: Default activation
        if (hasDefaultCtor)
        {
            rc.HasDefaultActivation = true;
        }

        // Rule D: Static interface(s)
        if (staticMembers.Count > 0)
        {
            var ifaceName = rc.StaticsInterfaceName != null
                ? ExtractSimpleName(rc.StaticsInterfaceName)
                : GetUniqueName($"I{rc.Name}Statics", ns);
            var iface = CreateExclusiveInterface(ifaceName, ns, rc.Name, staticMembers);
            if (rc.StaticsInterfaceGuid != null)
                iface.Uuid = rc.StaticsInterfaceGuid;
            rc.StaticsInterfaces.Add(iface);
            rc.ImplementedInterfaces.Add(iface);
        }

        // Rule E: Protected interface
        // Original midl emits IClassProtected with ProtectedAttribute even on sealed classes
        if (protectedMembers.Count > 0)
        {
            var ifaceName = GetUniqueName($"I{rc.Name}Protected", ns);
            var iface = CreateExclusiveInterface(ifaceName, ns, rc.Name, protectedMembers);
            rc.ProtectedInterface = iface;
            rc.ImplementedInterfaces.Add(iface);
        }

        // Rule F: Overridable interface
        // Original midl emits IClassOverrides with OverridableAttribute even on sealed classes
        if (overridableMembers.Count > 0)
        {
            var ifaceName = GetUniqueName($"I{rc.Name}Overrides", ns);
            var iface = CreateExclusiveInterface(ifaceName, ns, rc.Name, overridableMembers);
            rc.OverridesInterface = iface;
            rc.ImplementedInterfaces.Add(iface);
        }

        // Rule G: Default interface selection — first instance interface becomes default
        if (rc.DefaultInterface == null && rc.ImplementedInterfaces.Count > 0)
        {
            // Find the first non-factory, non-statics interface
            rc.DefaultInterface = rc.ImplementedInterfaces.FirstOrDefault(
                i => i != rc.FactoryInterface && !rc.StaticsInterfaces.Contains(i));
        }

        // Rule H-ext: Handle [default_interface] attribute on empty classes
        if (rc.IsDefaultInterface && rc.DefaultInterface == null)
        {
            var ifaceName = GetUniqueName($"I{rc.Name}", ns);
            var iface = CreateExclusiveInterface(ifaceName, ns, rc.Name, new List<ISymbol>());
            rc.DefaultInterface = iface;
            rc.ImplementedInterfaces.Add(iface);
        }
    }

    private static InterfaceSymbol CreateExclusiveInterface(
        string name, NamespaceSymbol? ns, string className, List<ISymbol> members)
    {
        var iface = new InterfaceSymbol(name, ns);
        iface.IsExclusive = true;
        // Store the FQN for ExclusiveTo attribute (original midl uses FQN)
        var fqn = ns != null && ns.FullName.Length > 0 ? $"{ns.FullName}.{className}" : className;
        iface.ExclusiveToClass = fqn;
        iface.IsSynthesized = true;

        foreach (var member in members)
        {
            iface.AddMember(member);
        }

        // Add to namespace so it's discoverable
        ns?.AddMember(iface);

        return iface;
    }

    /// <summary>
    /// Rule H: If the candidate name collides with an existing type in the namespace,
    /// append 2, 3, 4... until unique.
    /// </summary>
    private static string GetUniqueName(string candidate, NamespaceSymbol? ns)
    {
        if (ns == null) return candidate;

        var existingNames = new HashSet<string>(
            ns.Members.Select(m => m.Name), StringComparer.Ordinal);

        if (!existingNames.Contains(candidate))
            return candidate;

        for (int i = 2; ; i++)
        {
            var numbered = $"{candidate}{i}";
            if (!existingNames.Contains(numbered))
                return numbered;
        }
    }

    /// <summary>
    /// Extracts the simple (unqualified) name from a possibly fully-qualified name.
    /// E.g. "Namespace.IFooFactory" → "IFooFactory".
    /// </summary>
    private static string ExtractSimpleName(string qualifiedName)
    {
        var lastDot = qualifiedName.LastIndexOf('.');
        return lastDot >= 0 ? qualifiedName.Substring(lastDot + 1) : qualifiedName;
    }
}
