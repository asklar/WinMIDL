// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Midl.Analysis.Tests;

public class ClrProjectionMapTests
{
    [Theory]
    [InlineData("System.DateTimeOffset", "Windows.Foundation.DateTime")]
    [InlineData("System.Exception", "Windows.Foundation.HResult")]
    [InlineData("System.TimeSpan", "Windows.Foundation.TimeSpan")]
    [InlineData("System.Uri", "Windows.Foundation.Uri")]
    [InlineData("System.IDisposable", "Windows.Foundation.IClosable")]
    [InlineData("System.Collections.Generic.IList`1", "Windows.Foundation.Collections.IVector`1")]
    [InlineData("System.Collections.Generic.IDictionary`2", "Windows.Foundation.Collections.IMap`2")]
    [InlineData("System.ComponentModel.INotifyPropertyChanged", "Windows.UI.Xaml.Data.INotifyPropertyChanged")]
    [InlineData("System.Type", "Windows.UI.Xaml.Interop.TypeName")]
    [InlineData("System.Numerics.Vector3", "Windows.Foundation.Numerics.Vector3")]
    public void ClrProjectionMap_RoundTrips(string clrName, string winrtName)
    {
        // CLR → WinRT
        Assert.Equal(winrtName, ClrProjectionMap.ToWinRTName(clrName));
        // WinRT → CLR
        Assert.Equal(clrName, ClrProjectionMap.ToClrName(winrtName));
        // Predicate checks
        Assert.True(ClrProjectionMap.IsProjectedClrType(clrName));
        Assert.True(ClrProjectionMap.IsProjectedWinRTType(winrtName));
    }

    [Theory]
    [InlineData("System.String")]
    [InlineData("MyApp.MyType")]
    [InlineData("System.Int32")]
    public void ToWinRTName_ReturnsInputForNonProjectedTypes(string name)
    {
        Assert.Equal(name, ClrProjectionMap.ToWinRTName(name));
        Assert.False(ClrProjectionMap.IsProjectedClrType(name));
    }

    [Theory]
    [InlineData("System.String")]
    [InlineData("MyApp.MyType")]
    public void ToClrName_ReturnsInputForNonProjectedTypes(string name)
    {
        Assert.Equal(name, ClrProjectionMap.ToClrName(name));
        Assert.False(ClrProjectionMap.IsProjectedWinRTType(name));
    }

    [Fact]
    public void ProjectedTypeDefinitions_ContainsWindowsFoundation()
    {
        Assert.True(ClrProjectionMap.ProjectedTypeDefinitions.ContainsKey("Windows.Foundation"));
        var defs = ClrProjectionMap.ProjectedTypeDefinitions["Windows.Foundation"];
        Assert.Contains(defs, d => d.Contains("struct TimeSpan"));
        Assert.Contains(defs, d => d.Contains("struct DateTime"));
        Assert.Contains(defs, d => d.Contains("struct HResult"));
    }

    [Fact]
    public void ProjectedTypeDefinitions_ContainsNumerics()
    {
        Assert.True(ClrProjectionMap.ProjectedTypeDefinitions.ContainsKey("Windows.Foundation.Numerics"));
        var defs = ClrProjectionMap.ProjectedTypeDefinitions["Windows.Foundation.Numerics"];
        Assert.Contains(defs, d => d.Contains("struct Vector3"));
        Assert.Contains(defs, d => d.Contains("struct Matrix4x4"));
    }
}
