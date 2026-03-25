using Midl.Analysis;
using Midl.CodeGen.Header;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Unit tests for forward declaration placement and boilerplate whitespace fixes.
/// </summary>
public partial class HeaderEmitterTests
{
    [Fact]
    public void ForwardDecl_InsideCppGuard()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                };
            }");

        var header = _emitter.Emit(compilation);

        var fwdIdx = header.IndexOf("/* Forward Declarations */");
        var cppGuard = header.IndexOf("#if defined(__cplusplus) && !defined(CINTERFACE)");
        Assert.True(fwdIdx >= 0, "Forward declarations section should be present");
        Assert.True(cppGuard >= 0, "C++ guard should be present");
        Assert.True(fwdIdx > cppGuard, "Forward declarations should appear INSIDE the C++ guard");
    }

    [Fact]
    public void ForwardDecl_BeforeInterfaceDefinitions()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                };
            }");

        var header = _emitter.Emit(compilation);

        var fwdIdx = header.IndexOf("/* Forward Declarations */");
        var ifaceDef = header.IndexOf("#ifndef ____x_TestApp_CIWidget_INTERFACE_DEFINED__");
        Assert.True(fwdIdx >= 0, "Forward declarations should be present");
        Assert.True(ifaceDef >= 0, "Interface definition should be present");
        Assert.True(fwdIdx < ifaceDef, "Forward declarations should appear before interface definitions");
    }

    [Fact]
    public void Boilerplate_BlankLineAfterPragmaWarning_MatchesMidl()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                enum Color { Red, Green, Blue };
            }");

        var header = _emitter.Emit(compilation);

        // Real midl.exe has a blank line after "#pragma warning( disable: 4049 )..."
        // before the next comment about rpcndr.h
        var pragmaIdx = header.IndexOf("#pragma warning( disable: 4049 )");
        Assert.True(pragmaIdx >= 0, "Pragma warning should be present");

        // Find the next line after pragma warning
        var nextLineStart = header.IndexOf('\n', pragmaIdx) + 1;
        var nextLine = header.Substring(nextLineStart, header.IndexOf('\n', nextLineStart) - nextLineStart).Trim();

        // The next line should be blank (matching real midl.exe)
        Assert.Equal("", nextLine);
    }
}
