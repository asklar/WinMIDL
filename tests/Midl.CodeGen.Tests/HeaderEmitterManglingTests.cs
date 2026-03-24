using Midl.CodeGen.Header;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Unit tests for HeaderEmitter.MangleName — verifies the C-separator mangling
/// that real midl.exe uses between namespace segments.
/// </summary>
public partial class HeaderEmitterManglingTests
{
    [Theory]
    [InlineData("Foo.IBar", "__x_Foo_CIBar")]
    [InlineData("A.B.C.IFoo", "__x_A_CB_CC_CIFoo")]
    [InlineData("IFoo", "__x_IFoo")]
    [InlineData("TestApp.Parity.IBasicWidget", "__x_TestApp_CParity_CIBasicWidget")]
    [InlineData("Windows.Foundation.TypedEventHandler", "__x_Windows_CFoundation_CTypedEventHandler")]
    public void MangleName_ProducesCorrectSeparators(string fullName, string expected)
    {
        var result = HeaderEmitter.MangleName(fullName);
        Assert.Equal(expected, result);
    }
}
