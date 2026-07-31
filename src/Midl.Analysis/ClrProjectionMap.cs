// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Midl.Analysis;

/// <summary>
/// Maps between CLR-projected type names and their original WinRT type names.
/// .NET's MetadataReader de-projects WinRT types to their CLR equivalents.
/// This map allows reverse-mapping back to WinRT names for decompilation and display.
/// Source: https://github.com/microsoft/CsWinRT/blob/master/src/WinRT.Runtime/Projections.cs
/// </summary>
public static class ClrProjectionMap
{
    /// <summary>CLR full name → WinRT full name</summary>
    private static readonly Dictionary<string, string> ClrToWinRT = new(StringComparer.Ordinal)
    {
        // Fundamental types
        ["System.DateTimeOffset"] = "Windows.Foundation.DateTime",
        ["System.Exception"] = "Windows.Foundation.HResult",
        ["System.TimeSpan"] = "Windows.Foundation.TimeSpan",
        ["System.Uri"] = "Windows.Foundation.Uri",
        ["System.Nullable`1"] = "Windows.Foundation.IReference`1",
        ["System.EventHandler`1"] = "Windows.Foundation.EventHandler`1",

        // Numerics
        ["System.Numerics.Matrix3x2"] = "Windows.Foundation.Numerics.Matrix3x2",
        ["System.Numerics.Matrix4x4"] = "Windows.Foundation.Numerics.Matrix4x4",
        ["System.Numerics.Plane"] = "Windows.Foundation.Numerics.Plane",
        ["System.Numerics.Quaternion"] = "Windows.Foundation.Numerics.Quaternion",
        ["System.Numerics.Vector2"] = "Windows.Foundation.Numerics.Vector2",
        ["System.Numerics.Vector3"] = "Windows.Foundation.Numerics.Vector3",
        ["System.Numerics.Vector4"] = "Windows.Foundation.Numerics.Vector4",

        // Collections
        ["System.Collections.Generic.KeyValuePair`2"] = "Windows.Foundation.Collections.IKeyValuePair`2",
        ["System.Collections.Generic.IEnumerable`1"] = "Windows.Foundation.Collections.IIterable`1",
        ["System.Collections.Generic.IEnumerator`1"] = "Windows.Foundation.Collections.IIterator`1",
        ["System.Collections.Generic.IList`1"] = "Windows.Foundation.Collections.IVector`1",
        ["System.Collections.Generic.IReadOnlyList`1"] = "Windows.Foundation.Collections.IVectorView`1",
        ["System.Collections.Generic.IDictionary`2"] = "Windows.Foundation.Collections.IMap`2",
        ["System.Collections.Generic.IReadOnlyDictionary`2"] = "Windows.Foundation.Collections.IMapView`2",

        // IDisposable → IClosable
        ["System.IDisposable"] = "Windows.Foundation.IClosable",

        // XAML Data (Windows.UI.Xaml flavor)
        ["System.ComponentModel.INotifyPropertyChanged"] = "Windows.UI.Xaml.Data.INotifyPropertyChanged",
        ["System.ComponentModel.PropertyChangedEventArgs"] = "Windows.UI.Xaml.Data.PropertyChangedEventArgs",
        ["System.ComponentModel.PropertyChangedEventHandler"] = "Windows.UI.Xaml.Data.PropertyChangedEventHandler",
        ["System.Windows.Input.ICommand"] = "Windows.UI.Xaml.Input.ICommand",
        ["System.Collections.IEnumerable"] = "Windows.UI.Xaml.Interop.IBindableIterable",
        ["System.Collections.IList"] = "Windows.UI.Xaml.Interop.IBindableVector",
        ["System.Collections.Specialized.INotifyCollectionChanged"] = "Windows.UI.Xaml.Interop.INotifyCollectionChanged",
        ["System.Collections.Specialized.NotifyCollectionChangedAction"] = "Windows.UI.Xaml.Interop.NotifyCollectionChangedAction",
        ["System.Collections.Specialized.NotifyCollectionChangedEventArgs"] = "Windows.UI.Xaml.Interop.NotifyCollectionChangedEventArgs",
        ["System.Collections.Specialized.NotifyCollectionChangedEventHandler"] = "Windows.UI.Xaml.Interop.NotifyCollectionChangedEventHandler",

        // Type itself
        ["System.Type"] = "Windows.UI.Xaml.Interop.TypeName",
    };

    /// <summary>WinRT full name → CLR full name (reverse mapping)</summary>
    private static readonly Dictionary<string, string> WinRTToClr =
        ClrToWinRT.ToDictionary(kv => kv.Value, kv => kv.Key, StringComparer.Ordinal);

    /// <summary>
    /// If the given type name is a CLR-projected name, returns the WinRT name.
    /// Otherwise returns the input unchanged.
    /// </summary>
    public static string ToWinRTName(string clrTypeName)
    {
        return ClrToWinRT.TryGetValue(clrTypeName, out var winrtName) ? winrtName : clrTypeName;
    }

    /// <summary>
    /// If the given type name is a WinRT name with a CLR projection, returns the CLR name.
    /// Otherwise returns the input unchanged.
    /// </summary>
    public static string ToClrName(string winrtTypeName)
    {
        return WinRTToClr.TryGetValue(winrtTypeName, out var clrName) ? clrName : winrtTypeName;
    }

    /// <summary>Returns true if this CLR type name has a WinRT projection.</summary>
    public static bool IsProjectedClrType(string clrTypeName) => ClrToWinRT.ContainsKey(clrTypeName);

    /// <summary>Returns true if this WinRT type name is projected to CLR.</summary>
    public static bool IsProjectedWinRTType(string winrtTypeName) => WinRTToClr.ContainsKey(winrtTypeName);

    /// <summary>
    /// Returns well-known WinRT struct definitions for types that are CLR-projected
    /// and thus invisible in .NET's MetadataReader.
    /// Keyed by namespace, returns IDL-formatted type definitions.
    /// </summary>
    public static IReadOnlyDictionary<string, string[]> ProjectedTypeDefinitions { get; } = new Dictionary<string, string[]>
    {
        ["Windows.Foundation"] = new[]
        {
            "struct DateTime { Int64 UniversalTime; };",
            "struct TimeSpan { Int64 Duration; };",
            "struct HResult { Int32 Value; };",
            "struct EventRegistrationToken { Int64 Value; };",
            "struct Point { Single X; Single Y; };",
            "struct Size { Single Width; Single Height; };",
            "struct Rect { Single X; Single Y; Single Width; Single Height; };",
        },
        ["Windows.Foundation.Numerics"] = new[]
        {
            "struct Vector2 { Single X; Single Y; };",
            "struct Vector3 { Single X; Single Y; Single Z; };",
            "struct Vector4 { Single X; Single Y; Single Z; Single W; };",
            "struct Matrix3x2 { Single M11; Single M12; Single M21; Single M22; Single M31; Single M32; };",
            "struct Matrix4x4 { Single M11; Single M12; Single M13; Single M14; Single M21; Single M22; Single M23; Single M24; Single M31; Single M32; Single M33; Single M34; Single M41; Single M42; Single M43; Single M44; };",
            "struct Quaternion { Single X; Single Y; Single Z; Single W; };",
            "struct Plane { Vector3 Normal; Single D; };",
        },
    };
}
