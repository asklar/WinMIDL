// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Verifies that generated proxy/stub C++ code contains zero TODO placeholders
/// across all supported type patterns.
/// </summary>
public class NoTodoTests
{
    private readonly ProxyGenerator _proxyGenerator = new();
    private readonly StubGenerator _stubGenerator = new();

    private static Compilation Compile(string source) => CompilationHelper.Compile(source);

    [Fact]
    public void GeneratedProxyCode_ContainsNoTodoComments()
    {
        var idl = @"
            namespace TestApp {
                enum Color { Red, Green, Blue }
                struct Point { Single X; Single Y; }
                delegate void SimpleHandler();
                delegate void StringHandler(String value);

                interface ICallback {
                    void OnComplete();
                    void OnError(String message);
                }

                interface IWidget {
                    // Primitives
                    Int32 GetCount();
                    void SetCount(Int32 value);
                    String GetName();
                    void SetName(String value);
                    Boolean IsEnabled();
                    Double GetRatio();
                    Guid GetId();

                    // Enum
                    Color GetColor();
                    void SetColor(Color c);

                    // Struct
                    Point GetPosition();
                    void SetPosition(Point p);

                    // Interface pointer
                    ICallback GetCallback();
                    void SetCallback(ICallback cb);

                    // Delegate
                    EventRegistrationToken AddHandler(StringHandler handler);
                    void RemoveHandler(EventRegistrationToken token);

                    // Object
                    Object GetValue();
                    void SetValue(Object val);

                    // Event
                    event SimpleHandler Click;
                }
            }";
        var compilation = Compile(idl);
        var proxyOutput = _proxyGenerator.Generate(compilation);

        Assert.DoesNotContain("// TODO", proxyOutput);
    }

    [Fact]
    public void GeneratedStubCode_ContainsNoTodoComments()
    {
        var idl = @"
            namespace TestApp {
                enum Color { Red, Green, Blue }
                struct Point { Single X; Single Y; }
                delegate void SimpleHandler();
                delegate void StringHandler(String value);

                interface ICallback {
                    void OnComplete();
                    void OnError(String message);
                }

                interface IWidget {
                    // Primitives
                    Int32 GetCount();
                    void SetCount(Int32 value);
                    String GetName();
                    void SetName(String value);
                    Boolean IsEnabled();
                    Double GetRatio();
                    Guid GetId();

                    // Enum
                    Color GetColor();
                    void SetColor(Color c);

                    // Struct
                    Point GetPosition();
                    void SetPosition(Point p);

                    // Interface pointer
                    ICallback GetCallback();
                    void SetCallback(ICallback cb);

                    // Delegate
                    EventRegistrationToken AddHandler(StringHandler handler);
                    void RemoveHandler(EventRegistrationToken token);

                    // Object
                    Object GetValue();
                    void SetValue(Object val);

                    // Event
                    event SimpleHandler Click;
                }
            }";
        var compilation = Compile(idl);
        var stubOutput = _stubGenerator.Generate(compilation);

        Assert.DoesNotContain("// TODO", stubOutput);
    }
}
