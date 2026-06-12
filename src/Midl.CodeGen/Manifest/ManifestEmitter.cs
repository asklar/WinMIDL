// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using Midl.Analysis;
using Midl.Analysis.Symbols;

namespace Midl.CodeGen.Manifest;

/// <summary>
/// Generates WinRT activation manifest fragments from .winmd metadata.
/// Supports SxS (.manifest) and AppX (.appxmanifest) formats.
/// </summary>
public class ManifestEmitter
{
    /// <summary>Describes an activatable runtime class for manifest generation.</summary>
    public record ActivatableClassInfo(string FullName, string ThreadingModel);

    /// <summary>
    /// Emit an SxS activation manifest fragment (for unpackaged apps).
    /// </summary>
    public string EmitSxs(string serverDll, IReadOnlyList<ActivatableClassInfo> classes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("""<?xml version="1.0" encoding="utf-8"?>""");
        sb.AppendLine("""<assembly xmlns="urn:schemas-microsoft-com:asm.v1" manifestVersion="1.0">""");
        sb.AppendLine($"""  <file name="{EscapeXml(serverDll)}">""");

        foreach (var cls in classes)
        {
            sb.AppendLine($"""    <activatableClass name="{EscapeXml(cls.FullName)}" threadingModel="{cls.ThreadingModel}" xmlns="urn:schemas-microsoft-com:winrt.v1" />""");
        }

        sb.AppendLine("  </file>");
        sb.AppendLine("</assembly>");
        return sb.ToString();
    }

    /// <summary>
    /// Emit an AppX/MSIX activation manifest fragment (for packaged apps).
    /// </summary>
    public string EmitAppx(string serverDll, IReadOnlyList<ActivatableClassInfo> classes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("""<?xml version="1.0" encoding="utf-8"?>""");
        sb.AppendLine("<Extensions>");
        sb.AppendLine("""  <Extension Category="windows.activatableClass.inProcessServer">""");
        sb.AppendLine("    <InProcessServer>");
        sb.AppendLine($"      <Path>{EscapeXml(serverDll)}</Path>");

        foreach (var cls in classes)
        {
            sb.AppendLine($"""      <ActivatableClass ActivatableClassId="{EscapeXml(cls.FullName)}" ThreadingModel="{cls.ThreadingModel}" />""");
        }

        sb.AppendLine("    </InProcessServer>");
        sb.AppendLine("  </Extension>");
        sb.AppendLine("</Extensions>");
        return sb.ToString();
    }

    /// <summary>
    /// Extract activatable runtime classes from a .winmd file.
    /// </summary>
    public static List<ActivatableClassInfo> ExtractFromWinMd(string winmdPath)
    {
        using var provider = new LazyWinMdProvider(winmdPath);
        return ExtractFromProvider(provider);
    }

    /// <summary>
    /// Extract activatable runtime classes from WinMD bytes.
    /// </summary>
    internal static List<ActivatableClassInfo> ExtractFromBytes(byte[] winmdBytes)
    {
        using var provider = new LazyWinMdProvider(winmdBytes);
        return ExtractFromProvider(provider);
    }

    private static List<ActivatableClassInfo> ExtractFromProvider(LazyWinMdProvider provider)
    {
        var result = new List<ActivatableClassInfo>();

        foreach (var typeName in provider.GetAllTypeNames().OrderBy(n => n, StringComparer.Ordinal))
        {
            var symbol = provider.TryResolve(typeName);
            if (symbol is not RuntimeClassSymbol rc)
                continue;

            // Skip static-only classes
            // When reading from WinMD, constructors appear as Members (ConstructorSymbol),
            // the default interface is indicated via Interfaces with IsDefaultInterface,
            // and HasDefaultActivation may be set from [activatable] attribute.
            bool hasConstructors = rc.Members.Any(m => m is ConstructorSymbol);
            bool hasDefaultInterface = rc.DefaultInterface != null
                || rc.Interfaces.Any(i => i.IsDefaultInterface);
            bool isActivatable = hasConstructors || rc.HasDefaultActivation || hasDefaultInterface;

            if (!isActivatable)
                continue;

            // Read threading model; default to "both"
            var threading = NormalizeThreadingModel(rc.ThreadingModel);
            result.Add(new ActivatableClassInfo(rc.FullName, threading));
        }

        return result;
    }

    internal static string NormalizeThreadingModel(string? threadingModel)
    {
        if (string.IsNullOrEmpty(threadingModel))
            return "both";

        return threadingModel.ToLowerInvariant() switch
        {
            "sta" => "STA",
            "mta" => "MTA",
            "both" => "both",
            _ => "both"
        };
    }

    private static string EscapeXml(string value)
    {
        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}
