using Midl.Analysis;
using Midl.CodeGen.Xdc;

namespace Midl.CodeGen.Tests;

public class XdcEmitterTests
{
    [Fact]
    public void EmitXdc_TypeWithSummary()
    {
        var compilation = CompilationHelper.Compile("""
            namespace MyLib
            {
                /// <summary>A widget.</summary>
                runtimeclass Widget
                {
                    void Do();
                }
            }
            """);

        var emitter = new XdcEmitter();
        var xml = emitter.Emit(compilation, "MyLib");

        Assert.Contains("<name>MyLib</name>", xml);
        Assert.Contains("T:MyLib.Widget", xml);
        Assert.Contains("A widget.", xml);
    }

    [Fact]
    public void EmitXdc_MultipleTypesWithDocs()
    {
        var compilation = CompilationHelper.Compile("""
            namespace MyLib
            {
                /// <summary>A widget.</summary>
                runtimeclass Widget
                {
                    void Do();
                }

                /// <summary>Options for stuff.</summary>
                enum Options
                {
                    None,
                    All
                };
            }
            """);

        var emitter = new XdcEmitter();
        var xml = emitter.Emit(compilation, "MyLib");

        Assert.Contains("T:MyLib.Widget", xml);
        Assert.Contains("T:MyLib.Options", xml);
        Assert.Contains("A widget.", xml);
        Assert.Contains("Options for stuff.", xml);
    }

    [Fact]
    public void EmitXdc_MethodWithParams()
    {
        var compilation = CompilationHelper.Compile("""
            namespace MyLib
            {
                runtimeclass Widget
                {
                    /// <summary>Does stuff.</summary>
                    /// <param name="count">The count.</param>
                    /// <returns>A result.</returns>
                    Int32 DoStuff(Int32 count);
                }
            }
            """);

        var emitter = new XdcEmitter();
        var xml = emitter.Emit(compilation, "MyLib");

        Assert.Contains("M:MyLib.Widget.DoStuff(Int32)", xml);
        Assert.Contains("Does stuff.", xml);
        Assert.Contains("count", xml);
    }

    [Fact]
    public void EmitXdc_TypeWithoutDocs_NotInOutput()
    {
        var compilation = CompilationHelper.Compile("""
            namespace MyLib
            {
                runtimeclass Widget
                {
                    void Do();
                }
            }
            """);

        var emitter = new XdcEmitter();
        var xml = emitter.Emit(compilation, "MyLib");

        Assert.DoesNotContain("T:MyLib.Widget", xml);
    }

    [Fact]
    public void EmitXdc_PropertyWithDoc()
    {
        var compilation = CompilationHelper.Compile("""
            namespace MyLib
            {
                runtimeclass Widget
                {
                    /// <summary>The name.</summary>
                    String Name;
                }
            }
            """);

        var emitter = new XdcEmitter();
        var xml = emitter.Emit(compilation, "MyLib");

        Assert.Contains("P:MyLib.Widget.Name", xml);
        Assert.Contains("The name.", xml);
    }

    [Fact]
    public void EmitXdc_EventWithDoc()
    {
        var compilation = CompilationHelper.Compile("""
            namespace MyLib
            {
                runtimeclass Widget
                {
                    /// <summary>Raised on click.</summary>
                    event Windows.Foundation.EventHandler<Object> Clicked;
                }
            }
            """);

        var emitter = new XdcEmitter();
        var xml = emitter.Emit(compilation, "MyLib");

        Assert.Contains("E:MyLib.Widget.Clicked", xml);
        Assert.Contains("Raised on click.", xml);
    }
}
