using Midl.Analysis;
using Midl.CodeGen.Header;

namespace Midl.CodeGen.Tests;

public partial class HeaderEmitterTests
{
    private readonly HeaderEmitter _emitter = new();

    private static Compilation Compile(string source) => CompilationHelper.Compile(source);

    [Fact]
    public void EmitEnumHeader()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                enum Color
                {
                    Red,
                    Green,
                    Blue
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("enum Color", header);
        Assert.Contains("Color_Red = 0,", header);
        Assert.Contains("Color_Green = 1,", header);
        Assert.Contains("Color_Blue = 2,", header);
    }

    [Fact]
    public void EmitStructHeader()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                struct Point
                {
                    Int32 X;
                    Int32 Y;
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("struct Point", header);
        Assert.Contains("int X;", header);
        Assert.Contains("int Y;", header);
    }

    [Fact]
    public void EmitInterfaceHeader()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IGreeter
                {
                    String Name { get; set; };
                    void SayHello(Int32 times);
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("MIDL_INTERFACE(", header);
        Assert.Contains("IGreeter : public IInspectable", header);
        Assert.Contains("get_Name(", header);
        Assert.Contains("/* [propget] */", header);
        Assert.Contains("/* [out, retval] */HSTRING * value", header);
        Assert.Contains("put_Name(", header);
        Assert.Contains("/* [propput] */", header);
        Assert.Contains("/* [in] */HSTRING value", header);
        Assert.Contains("/* [in] */int times", header);
    }

    [Fact]
    public void EmitRuntimeClassActivationString()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                runtimeclass Widget
                {
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("RuntimeClass_TestApp_Widget", header);
        Assert.Contains("L\"TestApp.Widget\"", header);
        Assert.Contains("__declspec(selectany)", header);
    }

    [Fact]
    public void EmitIncludeGuard()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                enum Dummy { A };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("#pragma once", header);
        Assert.Contains("#ifndef __TestApp_h__", header);
        Assert.Contains("#define __TestApp_h__", header);
        Assert.Contains("#endif // __TestApp_h__", header);
    }

    [Fact]
    public void EmitTypeMapping()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                struct AllTypes
                {
                    Int16 a;
                    Int32 b;
                    Int64 c;
                    UInt8 d;
                    UInt16 e;
                    UInt32 f;
                    UInt64 g;
                    Single h;
                    Double i;
                    Boolean j;
                    Char k;
                    String l;
                    Guid m;
                };
            }");

        var header = _emitter.Emit(compilation);

        Assert.Contains("short a;", header);
        Assert.Contains("int b;", header);
        Assert.Contains("__int64 c;", header);
        Assert.Contains("::byte d;", header);
        Assert.Contains("unsigned short e;", header);
        Assert.Contains("unsigned int f;", header);
        Assert.Contains("unsigned __int64 g;", header);
        Assert.Contains("float h;", header);
        Assert.Contains("double i;", header);
        Assert.Contains("::boolean j;", header);
        Assert.Contains("wchar_t k;", header);
        Assert.Contains("HSTRING l;", header);
        Assert.Contains("GUID m;", header);
    }

    [Fact]
    public void EmitFullFile()
    {
        var compilation = Compile(@"
            namespace MyCompany.MyApp
            {
                enum Status
                {
                    Active,
                    Inactive
                };

                interface IWidget
                {
                    String Name { get; };
                    void DoWork(Int32 count);
                };

                runtimeclass Widget
                {
                };
            }");

        var header = _emitter.Emit(compilation);

        // Include guard
        Assert.Contains("#pragma once", header);
        Assert.Contains("#ifndef __MyCompany_MyApp_h__", header);

        // Enum
        Assert.Contains("enum Status", header);
        Assert.Contains("Status_Active = 0,", header);

        // Interface
        Assert.Contains("MIDL_INTERFACE(", header);
        Assert.Contains("IWidget : public IInspectable", header);
        Assert.Contains("get_Name(", header);
        Assert.Contains("/* [propget] */", header);
        Assert.Contains("/* [out, retval] */HSTRING * value", header);
        Assert.Contains("/* [in] */int count", header);

        // Runtime class activation string
        Assert.Contains("RuntimeClass_MyCompany_MyApp_Widget", header);
        Assert.Contains("L\"MyCompany.MyApp.Widget\"", header);

        // Closing guard
        Assert.Contains("#endif // __MyCompany_MyApp_h__", header);
    }
}
