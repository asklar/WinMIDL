// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Midl.Analysis.Symbols;

public partial class RuntimeClassSymbol
{
    /// <summary>Explicit factory interface name from [constructor_name("...", guid)].</summary>
    public string? FactoryInterfaceName { get; set; }

    /// <summary>Explicit factory interface GUID from [constructor_name("...", guid)].</summary>
    public Guid? FactoryInterfaceGuid { get; set; }

    /// <summary>Explicit default instance interface name from [interface_name("...", guid)].</summary>
    public string? DefaultInterfaceName { get; set; }

    /// <summary>Explicit default instance interface GUID from [interface_name("...", guid)].</summary>
    public Guid? DefaultInterfaceGuid { get; set; }

    /// <summary>Explicit statics interface name from [static_name("...", guid)].</summary>
    public string? StaticsInterfaceName { get; set; }

    /// <summary>Explicit statics interface GUID from [static_name("...", guid)].</summary>
    public Guid? StaticsInterfaceGuid { get; set; }
}
