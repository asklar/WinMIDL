using Midl.Analysis;
using Midl.Lsp.Handlers;

namespace Midl.Integration.Tests;

public class ClassDiagramTests
{
    [Fact]
    public void SingleRuntimeClass_EmitsClassWithMembers()
    {
        var source = """
            namespace TestNs
            {
                runtimeclass Widget
                {
                    Widget();
                    Widget(String name);
                    String Name { get; set; };
                    void DoStuff(Int32 x);
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("class Widget {", mermaid);
        Assert.Contains("+Widget()", mermaid);
        Assert.Contains("+Widget(String)", mermaid);
        Assert.Contains("+String Name", mermaid);
        Assert.Contains("+void DoStuff(Int32)", mermaid);
        Assert.Contains("classDiagram", mermaid);
        Assert.Contains("```mermaid", mermaid);
    }

    [Fact]
    public void Interface_HasInterfaceStereotype()
    {
        var source = """
            namespace TestNs
            {
                interface IFoo
                {
                    String Name { get; };
                    void Execute();
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("class IFoo {", mermaid);
        Assert.Contains("<<interface>>", mermaid);
        Assert.Contains("+String Name", mermaid);
        Assert.Contains("+void Execute()", mermaid);
    }

    [Fact]
    public void Enum_HasEnumerationStereotype()
    {
        var source = """
            namespace TestNs
            {
                enum Color
                {
                    Red,
                    Green,
                    Blue
                };
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("class Color {", mermaid);
        Assert.Contains("<<enumeration>>", mermaid);
        Assert.Contains("Red", mermaid);
        Assert.Contains("Green", mermaid);
        Assert.Contains("Blue", mermaid);
    }

    [Fact]
    public void Struct_HasStructStereotype()
    {
        var source = """
            namespace TestNs
            {
                struct Point
                {
                    Int32 X;
                    Int32 Y;
                };
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("class Point {", mermaid);
        Assert.Contains("<<struct>>", mermaid);
        Assert.Contains("+Int32 X", mermaid);
        Assert.Contains("+Int32 Y", mermaid);
    }

    [Fact]
    public void Delegate_HasDelegateStereotype()
    {
        var source = """
            namespace TestNs
            {
                delegate void MyHandler(String message, Int32 code);
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("class MyHandler {", mermaid);
        Assert.Contains("<<delegate>>", mermaid);
        Assert.Contains("+void Invoke(String, Int32)", mermaid);
    }

    [Fact]
    public void Inheritance_EmitsExtendsArrow()
    {
        var source = """
            namespace TestNs
            {
                unsealed runtimeclass Base
                {
                    Base();
                }

                runtimeclass Derived : Base
                {
                    Derived();
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("class Base {", mermaid);
        Assert.Contains("class Derived {", mermaid);
        Assert.Contains("Derived --|> Base : extends", mermaid);
    }

    [Fact]
    public void InterfaceImplementation_EmitsImplementsArrow()
    {
        var source = """
            namespace TestNs
            {
                interface IWidget
                {
                    void Activate();
                }

                runtimeclass Widget : [default] IWidget
                {
                    Widget();
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("class Widget {", mermaid);
        Assert.Contains("class IWidget {", mermaid);
        Assert.Contains("Widget ..|> IWidget : implements", mermaid);
    }

    [Fact]
    public void MultipleInterfaces_EmitsMultipleArrows()
    {
        var source = """
            namespace TestNs
            {
                interface IFirst
                {
                    void A();
                }
                interface ISecond
                {
                    void B();
                }
                runtimeclass MyClass : [default] IFirst, ISecond
                {
                    MyClass();
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("MyClass ..|> IFirst : implements", mermaid);
        Assert.Contains("MyClass ..|> ISecond : implements", mermaid);
    }

    [Fact]
    public void InterfaceRequires_EmitsRequiresArrow()
    {
        var source = """
            namespace TestNs
            {
                interface IBase
                {
                    void BaseMethod();
                }

                interface IDerived requires IBase
                {
                    void DerivedMethod();
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("class IBase {", mermaid);
        Assert.Contains("class IDerived {", mermaid);
        Assert.Contains("IDerived ..|> IBase : requires", mermaid);
    }

    [Fact]
    public void StaticMembers_GetDollarSuffix()
    {
        var source = """
            namespace TestNs
            {
                runtimeclass Util
                {
                    static String Version { get; };
                    static Int32 Compute(Double input);
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("+String Version$", mermaid);
        Assert.Contains("+Int32 Compute(Double)$", mermaid);
    }

    [Fact]
    public void Events_ShownWithEventPrefix()
    {
        var source = """
            namespace TestNs
            {
                runtimeclass Button
                {
                    Button();
                    event Windows.Foundation.EventHandler<Object> Click;
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("+event", mermaid);
        Assert.Contains("Click", mermaid);
    }

    [Fact]
    public void EmptyNamespace_ProducesValidDiagram()
    {
        var source = """
            namespace TestNs
            {
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("# Class Diagram:", mermaid);
        Assert.Contains("```mermaid", mermaid);
        Assert.Contains("classDiagram", mermaid);
        Assert.DoesNotContain("class ", mermaid.Split("classDiagram")[1].Split("```")[0].Trim().Length > 0
            ? mermaid.Split("classDiagram")[1] : "no classes");
    }

    [Fact]
    public void EmptyNamespace_NoClassesInBody()
    {
        var source = """
            namespace TestNs
            {
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("```mermaid", mermaid);
        Assert.Contains("classDiagram", mermaid);
        // The body between classDiagram and closing ``` should be empty
        var body = mermaid.Split("classDiagram")[1].Split("```")[0].Trim();
        Assert.Empty(body);
    }

    [Fact]
    public void SynthesizedExcluded_NoIWidgetFactory()
    {
        var source = """
            namespace TestNs
            {
                runtimeclass Widget
                {
                    Widget();
                    Widget(String name);
                    void DoWork();
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("class Widget {", mermaid);
        Assert.DoesNotContain("IWidgetFactory", mermaid);
        Assert.DoesNotContain("IWidgetStatics", mermaid);
        // The synthesized default interface IWidget should also be excluded
        Assert.DoesNotContain("class IWidget {", mermaid);
    }

    [Fact]
    public void MultipleNamespaces_AllTypesShown()
    {
        var source = """
            namespace Alpha
            {
                enum Status { Active, Inactive };
            }
            namespace Beta
            {
                struct Vector { Single X; Single Y; };
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("class Status {", mermaid);
        Assert.Contains("class Vector {", mermaid);
        Assert.Contains("<<enumeration>>", mermaid);
        Assert.Contains("<<struct>>", mermaid);
    }

    [Fact]
    public void ConstructorParams_ShownCorrectly()
    {
        var source = """
            namespace TestNs
            {
                runtimeclass Service
                {
                    Service();
                    Service(String name, Int32 port, Boolean secure);
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("+Service()", mermaid);
        Assert.Contains("+Service(String, Int32, Boolean)", mermaid);
    }

    [Fact]
    public void PropertyGetSet_ShowsTypeAndName()
    {
        var source = """
            namespace TestNs
            {
                runtimeclass Config
                {
                    Config();
                    String Host { get; set; };
                    Int32 Port { get; };
                    Boolean Enabled { get; set; };
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("+String Host", mermaid);
        Assert.Contains("+Int32 Port", mermaid);
        Assert.Contains("+Boolean Enabled", mermaid);
    }

    [Fact]
    public void Title_UsedInHeader()
    {
        var source = """
            namespace TestNs
            {
                enum Mode { Fast, Slow };
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation, "My Custom Title");

        Assert.Contains("# Class Diagram: My Custom Title", mermaid);
    }

    [Fact]
    public void DefaultTitle_UsesNamespace()
    {
        var source = """
            namespace MyApp.Controls
            {
                enum State { On, Off };
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("# Class Diagram:", mermaid);
    }

    [Fact]
    public void MethodReturnTypes_ShownCorrectly()
    {
        var source = """
            namespace TestNs
            {
                interface ICalculator
                {
                    Int32 Add(Int32 a, Int32 b);
                    Double Divide(Double num, Double denom);
                    String Format();
                    void Reset();
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("+Int32 Add(Int32, Int32)", mermaid);
        Assert.Contains("+Double Divide(Double, Double)", mermaid);
        Assert.Contains("+String Format()", mermaid);
        Assert.Contains("+void Reset()", mermaid);
    }

    [Fact]
    public void FlagsEnum_TreatedAsEnumeration()
    {
        var source = """
            namespace TestNs
            {
                [flags]
                enum Permissions
                {
                    None = 0,
                    Read = 1,
                    Write = 2,
                    Execute = 4
                };
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("class Permissions {", mermaid);
        Assert.Contains("<<enumeration>>", mermaid);
        Assert.Contains("None", mermaid);
        Assert.Contains("Read", mermaid);
        Assert.Contains("Write", mermaid);
        Assert.Contains("Execute", mermaid);
    }

    [Fact]
    public void ComplexScenario_MultipleTypesAndRelationships()
    {
        var source = """
            namespace TestNs
            {
                enum Status { Active, Inactive };

                struct Config
                {
                    String Name;
                    Int32 Value;
                };

                delegate void StatusChanged(Status newStatus);

                interface IService
                {
                    Status CurrentStatus { get; };
                    void Start();
                    void Stop();
                }

                runtimeclass Service : [default] IService
                {
                    Service();
                    Service(Config config);
                    event StatusChanged StatusUpdated;
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        // All types present
        Assert.Contains("class Status {", mermaid);
        Assert.Contains("class Config {", mermaid);
        Assert.Contains("class StatusChanged {", mermaid);
        Assert.Contains("class IService {", mermaid);
        Assert.Contains("class Service {", mermaid);

        // Stereotypes
        Assert.Contains("<<enumeration>>", mermaid);
        Assert.Contains("<<struct>>", mermaid);
        Assert.Contains("<<delegate>>", mermaid);
        Assert.Contains("<<interface>>", mermaid);

        // Relationships
        Assert.Contains("Service ..|> IService : implements", mermaid);

        // Members
        Assert.Contains("+void Start()", mermaid);
        Assert.Contains("+void Stop()", mermaid);
        Assert.Contains("+Service()", mermaid);
        Assert.Contains("+Service(Config)", mermaid);
    }

    [Fact]
    public void MermaidOutput_IsWellFormed()
    {
        var source = """
            namespace TestNs
            {
                interface IWidget
                {
                    void Activate();
                }

                runtimeclass Widget : [default] IWidget
                {
                    Widget();
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        // Should start with markdown header
        Assert.StartsWith("# Class Diagram:", mermaid);

        // Should have exactly one mermaid code block
        var mermaidBlocks = mermaid.Split("```mermaid");
        Assert.Equal(2, mermaidBlocks.Length);

        // Should end with closing code fence
        Assert.EndsWith("```\r\n", mermaid.Replace("\n", "\r\n").TrimEnd() + "\r\n");
    }

    [Fact]
    public void DelegateWithReturnType_ShowsReturnType()
    {
        var source = """
            namespace TestNs
            {
                delegate Int32 ValueProducer();
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("class ValueProducer {", mermaid);
        Assert.Contains("<<delegate>>", mermaid);
        Assert.Contains("+Int32 Invoke()", mermaid);
    }

    [Fact]
    public void InterfaceWithMultipleMethods_AllShown()
    {
        var source = """
            namespace TestNs
            {
                interface IRepository
                {
                    void Add(String item);
                    void Remove(String item);
                    String Get(Int32 index);
                    Int32 Count { get; };
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("+void Add(String)", mermaid);
        Assert.Contains("+void Remove(String)", mermaid);
        Assert.Contains("+String Get(Int32)", mermaid);
        Assert.Contains("+Int32 Count", mermaid);
    }

    [Fact]
    public void StaticEvents_GetDollarSuffix()
    {
        var source = """
            namespace TestNs
            {
                runtimeclass AppEvents
                {
                    static event Windows.Foundation.EventHandler<Object> AppStarted;
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("AppStarted", mermaid);
        Assert.Contains("$", mermaid);
    }

    [Fact]
    public void InheritanceWithInterface_BothArrowTypes()
    {
        var source = """
            namespace TestNs
            {
                interface IActivatable
                {
                    void Activate();
                }

                unsealed runtimeclass BaseWidget
                {
                    BaseWidget();
                }

                runtimeclass FancyWidget : BaseWidget, [default] IActivatable
                {
                    FancyWidget();
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("FancyWidget --|> BaseWidget : extends", mermaid);
        Assert.Contains("FancyWidget ..|> IActivatable : implements", mermaid);
    }

    [Fact]
    public void StructWithMultipleFieldTypes()
    {
        var source = """
            namespace TestNs
            {
                struct Rectangle
                {
                    Single X;
                    Single Y;
                    Single Width;
                    Single Height;
                };
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("+Single X", mermaid);
        Assert.Contains("+Single Y", mermaid);
        Assert.Contains("+Single Width", mermaid);
        Assert.Contains("+Single Height", mermaid);
    }

    [Fact]
    public void EnumMembers_NoTypePrefix()
    {
        var source = """
            namespace TestNs
            {
                enum Direction
                {
                    North,
                    South,
                    East,
                    West
                };
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        // Enum members should be listed without type prefix (no +)
        var lines = mermaid.Split('\n');
        var memberLines = lines.Where(l => l.Trim() is "North" or "South" or "East" or "West").ToList();
        Assert.Equal(4, memberLines.Count);
    }

    [Fact]
    public void NoExternalTypes_OnlyUserDefined()
    {
        // Just user-defined types, no SDK references
        var source = """
            namespace TestNs
            {
                runtimeclass MyClass
                {
                    MyClass();
                    String Value { get; };
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source);
        var mermaid = ClassDiagramHandler.GenerateMermaid(compilation);

        Assert.Contains("class MyClass {", mermaid);
        // Should not include built-in types as classes
        Assert.DoesNotContain("class String {", mermaid);
        Assert.DoesNotContain("class Object {", mermaid);
    }
}
