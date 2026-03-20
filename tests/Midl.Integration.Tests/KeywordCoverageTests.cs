using Midl.Analysis;
using Midl.Analysis.Diagnostics;

namespace Midl.Integration.Tests;

/// <summary>
/// One test per MIDL3 construct — verifies parse + bind + no unexpected errors.
/// </summary>
public class KeywordCoverageTests
{
    private static void AssertCompiles(string source, CompilationMode mode = CompilationMode.Strict)
    {
        var compilation = CompilationHelper.Compile(source, mode);
        var errors = compilation.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Namespace()
    {
        AssertCompiles("namespace Foo.Bar.Baz { }");
    }

    [Fact]
    public void NestedNamespace()
    {
        AssertCompiles(@"
            namespace A
            {
                namespace B
                {
                    runtimeclass C { }
                }
            }");
    }

    [Fact]
    public void Runtimeclass()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                delegate void StatusHandler(Int32 code, String message);
                runtimeclass Full
                {
                    Full();
                    Full(String name);
                    void DoWork();
                    String Name { get; };
                    event StatusHandler Changed;
                }
            }");
    }

    [Fact]
    public void SealedRuntimeclass()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                runtimeclass Sealed
                {
                    Sealed();
                }
            }");
    }

    [Fact]
    public void UnsealedRuntimeclass()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                unsealed runtimeclass Base
                {
                    String Label { get; };
                }
            }");
    }

    [Fact]
    public void StaticRuntimeclass()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                static runtimeclass Util
                {
                    static void DoWork();
                    static Int32 Count { get; };
                }
            }");
    }

    [Fact]
    public void PartialRuntimeclass()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                partial runtimeclass Split
                {
                    void MethodA();
                }
                partial runtimeclass Split
                {
                    void MethodB();
                }
            }");
    }

    [Fact]
    public void Interface()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                interface IFoo
                {
                    void DoSomething();
                    String Name { get; };
                }
            }");
    }

    [Fact]
    public void InterfaceRequires()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                interface IBar { }
                interface IFoo requires IBar
                {
                    void DoSomething();
                }
            }");
    }

    [Fact]
    public void Delegate()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                delegate void Handler(Int32 x);
            }");
    }

    [Fact]
    public void Struct()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                struct Point
                {
                    Int32 X;
                    Int32 Y;
                };
            }");
    }

    [Fact]
    public void Enum()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                enum Color
                {
                    Red,
                    Green,
                    Blue,
                };
            }");
    }

    [Fact]
    public void FlagsEnum()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                [flags]
                enum Perms
                {
                    None = 0,
                    Read = 1,
                    Write = 2,
                };
            }");
    }

    [Fact]
    public void Apicontract()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                [contractversion(1)]
                apicontract TestContract {};
            }");
    }

    [Fact]
    public void Event()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                delegate void ChangeHandler(Object sender, Object args);
                runtimeclass Widget
                {
                    Widget();
                    event ChangeHandler Changed;
                }
            }");
    }

    [Fact]
    public void PropertyReadOnly()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                runtimeclass C
                {
                    String Name { get; };
                }
            }");
    }

    [Fact]
    public void PropertyReadWrite()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                runtimeclass C
                {
                    String Name;
                }
            }");
    }

    [Fact]
    public void PropertyExplicitReadWrite()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                runtimeclass C
                {
                    String Name { get; set; };
                }
            }");
    }

    [Fact]
    public void StaticMethod()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                runtimeclass C
                {
                    static void DoWork();
                }
            }");
    }

    [Fact]
    public void StaticProperty()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                runtimeclass C
                {
                    static Int32 Count { get; };
                }
            }");
    }

    [Fact]
    public void ProtectedMethod()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                unsealed runtimeclass C
                {
                    protected void OnDraw();
                }
            }");
    }

    [Fact]
    public void OverridableMethod()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                unsealed runtimeclass C
                {
                    overridable void OnResize();
                }
            }");
    }

    [Fact]
    public void Constructor()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                runtimeclass C
                {
                    C();
                }
            }");
    }

    [Fact]
    public void ParameterizedConstructor()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                runtimeclass C
                {
                    C(Int32 x, String y);
                }
            }");
    }

    [Fact]
    public void OutParam()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                runtimeclass C
                {
                    void GetSize(out Int32 width, out Int32 height);
                }
            }");
    }

    [Fact]
    public void RefParam()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                runtimeclass C
                {
                    Double Calc(ref Double value);
                }
            }");
    }

    [Fact]
    public void ArrayParam()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                runtimeclass C
                {
                    void SetBytes(UInt8[] data);
                }
            }");
    }

    [Fact]
    public void OutArrayParam()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                runtimeclass C
                {
                    void GetBytes(out UInt8[] data);
                }
            }");
    }

    [Fact]
    public void GenericType()
    {
        AssertCompiles(@"
            namespace Windows.Foundation.Collections
            {
                interface IVector { }
            }
            namespace TestApp
            {
                runtimeclass C
                {
                    Windows.Foundation.Collections.IVector<String> GetItems();
                }
            }");
    }

    [Fact]
    public void NestedGeneric()
    {
        AssertCompiles(@"
            namespace Windows.Foundation.Collections
            {
                interface IVector { }
            }
            namespace Windows.Foundation
            {
                interface IAsyncOperation { }
            }
            namespace TestApp
            {
                runtimeclass C
                {
                    Windows.Foundation.IAsyncOperation<Windows.Foundation.Collections.IVector<String>> GetItemsAsync();
                }
            }");
    }

    [Fact]
    public void VersionedBlock()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                apicontract TestContract {};
                runtimeclass C
                {
                    C();
                    [contract(TestApp.TestContract, 2)]
                    {
                        void NewMethod();
                    }
                }
            }");
    }

    [Fact]
    public void Import()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"midl_kw_import_{Guid.NewGuid()}");
        Directory.CreateDirectory(dir);
        try
        {
            var mainFile = Path.Combine(dir, "main.idl");
            var typesFile = Path.Combine(dir, "types.idl");
            File.WriteAllText(typesFile, "namespace Ext { runtimeclass Widget { } }");

            var mainSource = @"
                import ""types.idl"";
                namespace App
                {
                    runtimeclass Consumer
                    {
                        Ext.Widget GetWidget();
                    }
                }";
            File.WriteAllText(mainFile, mainSource);

            var compilation = CompilationHelper.Compile(
                mainSource, referencePaths: null, CompilationMode.Strict,
                includePaths: null, currentFilePath: mainFile);
            var errors = compilation.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList();
            Assert.Empty(errors);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void TransitiveImport()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"midl_kw_transitive_{Guid.NewGuid()}");
        Directory.CreateDirectory(dir);
        try
        {
            var cFile = Path.Combine(dir, "c.idl");
            File.WriteAllText(cFile, "namespace Core { runtimeclass Base { } }");

            var bFile = Path.Combine(dir, "b.idl");
            File.WriteAllText(bFile, @"
                import ""c.idl"";
                namespace Types { runtimeclass Widget { } }");

            var aFile = Path.Combine(dir, "a.idl");
            var aSource = @"
                import ""b.idl"";
                namespace App
                {
                    runtimeclass Consumer
                    {
                        Core.Base GetBase();
                        Types.Widget GetWidget();
                    }
                }";
            File.WriteAllText(aFile, aSource);

            var compilation = CompilationHelper.Compile(
                aSource, referencePaths: null, CompilationMode.Strict,
                includePaths: null, currentFilePath: aFile);
            var errors = compilation.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList();
            Assert.Empty(errors);

            Assert.NotNull(compilation.GetTypeByName("Core.Base"));
            Assert.NotNull(compilation.GetTypeByName("Types.Widget"));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void DeclareBlock()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                declare
                {
                    interface IFoo { };
                }
            }");
    }

    [Fact]
    public void CustomAttribute()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr
                {
                    Int32 Level;
                };
            }");
    }

    [Fact]
    public void AttributeUsage()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                [attributeusage(target_runtimeclass)]
                attribute MyAttr
                {
                    Int32 Level;
                };

                [MyAttr(42)]
                runtimeclass C
                {
                    C();
                }
            }");
    }

    [Fact]
    public void ContractAttribute()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                apicontract TestContract {};
                [contract(TestApp.TestContract, 1)]
                runtimeclass C
                {
                    C();
                }
            }");
    }

    [Fact]
    public void DeprecatedAttribute()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                apicontract TestContract {};
                [deprecated(""old"", deprecate, TestApp.TestContract, 2)]
                runtimeclass C
                {
                    C();
                }
            }");
    }

    [Fact]
    public void ThreadingAttribute()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                [threading(both)]
                runtimeclass C
                {
                    C();
                }
            }");
    }

    [Fact]
    public void MarshalingAttribute()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                [marshaling_behavior(agile)]
                runtimeclass C
                {
                    C();
                }
            }");
    }

    [Fact]
    public void MethodNameAttribute()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                runtimeclass C
                {
                    [method_name(""DoWorkFast"")]
                    void DoWork();
                }
            }");
    }

    [Fact]
    public void OverloadAttribute()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                runtimeclass C
                {
                    [overload(""DoWork"")]
                    void DoWork(Int32 x);
                }
            }");
    }

    [Fact]
    public void DefaultOverloadAttribute()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                runtimeclass C
                {
                    [default_overload]
                    void DoWork(Int32 x);
                }
            }");
    }

    [Fact]
    public void ReturnNameAttribute()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                runtimeclass C
                {
                    [return_name(""count"")]
                    Int32 GetCount();
                }
            }");
    }

    [Fact]
    public void RemoteAsync()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                runtimeclass C
                {
                    [remote_async]
                    void DoWorkAsync();
                }
            }");
    }

    [Fact]
    public void DefaultInterfaceAttribute()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                [default_interface]
                runtimeclass Empty
                {
                }
            }");
    }

    [Fact]
    public void CppQuote()
    {
        AssertCompiles(@"cpp_quote(""#include <windows.h>"")", CompilationMode.Compat);
    }

    [Fact]
    public void Coclass()
    {
        AssertCompiles(@"coclass Foo { };", CompilationMode.Compat);
    }

    [Fact]
    public void Typedef()
    {
        AssertCompiles(@"typedef int MYINT;", CompilationMode.Compat);
    }

    [Fact]
    public void ClassicInterface()
    {
        AssertCompiles(@"
            namespace TestApp
            {
                [object, uuid(00000000-0000-0000-0000-000000000000)]
                interface IFoo
                {
                    HRESULT Bar();
                }
            }", CompilationMode.Compat);
    }
}
