using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Header;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Unit tests for RuntimeClass metadata comment blocks and RUNTIMECLASS_DEFINED guards.
/// </summary>
public partial class HeaderEmitterTests
{
    [Fact]
    public void RuntimeClassComment_EmitsClassNameLine()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                runtimeclass Widget
                {
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("Class TestApp.Widget", header);
    }

    [Fact]
    public void RuntimeClassComment_EmitsImplementedInterfaces()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                }
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("Class implements the following interfaces:", header);
        Assert.Contains("** Default Interface **", header);
    }

    [Fact]
    public void RuntimeClassComment_EmitsThreadingModel()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                runtimeclass Widget
                {
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("Class Threading Model:  Both Single and Multi Threaded Apartment", header);
    }

    [Fact]
    public void RuntimeClassComment_EmitsMarshalingBehavior()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                runtimeclass Widget
                {
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("Class Marshaling Behavior:  Agile - Class is agile", header);
    }

    [Fact]
    public void RuntimeClassComment_WrappedInBlockComment()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                runtimeclass Widget
                {
                };
            }");

        var header = _emitter.Emit(compilation);

        var classNameIdx = header.IndexOf("Class TestApp.Widget");
        Assert.True(classNameIdx >= 0, "Class name should be present in comment");

        var commentStart = header.LastIndexOf("/*", classNameIdx);
        Assert.True(commentStart >= 0, "Should have a block comment before class name");
        var commentEnd = header.IndexOf("*/", commentStart);
        Assert.True(commentEnd > commentStart, "Block comment should be closed");
        Assert.True(classNameIdx > commentStart && classNameIdx < commentEnd,
            "Class name should be inside the comment block");
    }

    [Fact]
    public void RuntimeClassComment_HasRuntimeClassDefinedGuard()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                runtimeclass Widget
                {
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("#ifndef RUNTIMECLASS_TestApp_Widget_DEFINED", header);
        Assert.Contains("#define RUNTIMECLASS_TestApp_Widget_DEFINED", header);
        Assert.Contains("#endif", header);
    }

    [Fact]
    public void RuntimeClassComment_ActivationStringInsideGuard()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                runtimeclass Widget
                {
                };
            }");

        var header = _emitter.Emit(compilation);

        var guardIdx = header.IndexOf("#ifndef RUNTIMECLASS_TestApp_Widget_DEFINED");
        var activationIdx = header.IndexOf("RuntimeClass_TestApp_Widget[]");
        Assert.True(guardIdx >= 0, "RUNTIMECLASS guard should be present");
        Assert.True(activationIdx >= 0, "Activation string should be present");
        Assert.True(activationIdx > guardIdx, "Activation string should be inside the guard");
    }

    [Fact]
    public void RuntimeClassComment_CommentBeforeGuard()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                runtimeclass Widget
                {
                };
            }");

        var header = _emitter.Emit(compilation);

        var commentIdx = header.IndexOf("Class TestApp.Widget");
        var guardIdx = header.IndexOf("#ifndef RUNTIMECLASS_TestApp_Widget_DEFINED");
        Assert.True(commentIdx >= 0, "Comment should be present");
        Assert.True(guardIdx >= 0, "Guard should be present");
        Assert.True(commentIdx < guardIdx, "Comment should appear before the guard");
    }

    [Fact]
    public void RuntimeClassComment_DeepNamespace()
    {
        var compilation = Compile(@"
            namespace MyCompany.MyApp.Widgets
            {
                runtimeclass Widget
                {
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("Class MyCompany.MyApp.Widgets.Widget", header);
        Assert.Contains("#ifndef RUNTIMECLASS_MyCompany_MyApp_Widgets_Widget_DEFINED", header);
        Assert.Contains("#define RUNTIMECLASS_MyCompany_MyApp_Widgets_Widget_DEFINED", header);
    }

    [Fact]
    public void RuntimeClassComment_ListsDefaultInterface()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                runtimeclass Widget
                {
                    Widget();
                    String Name { get; };
                }
            }");

        var header = _emitter.Emit(compilation);

        // The synthesized IWidget should be listed as default interface
        Assert.Contains("TestApp.IWidget ** Default Interface **", header);
    }
}
