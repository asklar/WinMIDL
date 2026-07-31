// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.CodeGen.Proxy;

namespace Midl.Integration.Tests;

/// <summary>
/// Integration tests for FactoryGenerator — compiles IDL source end-to-end
/// and verifies the generated factory + DLL export C++ code.
/// </summary>
public class FactoryGeneratorIntegrationTests
{
    private readonly FactoryGenerator _factoryGenerator = new();
    private readonly ProxyGenerator _proxyGenerator = new();
    private readonly StubGenerator _stubGenerator = new();

    private static Compilation Compile(string source) => CompilationHelper.Compile(source);

    [Fact]
    public void EndToEnd_SimpleInterface_GeneratesCompleteFactory()
    {
        var source = @"
            namespace Widgets
            {
                interface IWidget
                {
                    String Name { get; set; };
                    void DoWork(Int32 count);
                    Boolean IsActive();
                };
            }";

        var compilation = Compile(source);
        var output = _factoryGenerator.Generate(compilation);

        // Factory class
        Assert.Contains("class CPSFactory : public IPSFactoryBuffer {", output);

        // IPSFactoryBuffer plumbing
        Assert.Contains("QueryInterface(REFIID riid, void** ppv)", output);
        Assert.Contains("AddRef()", output);
        Assert.Contains("Release()", output);
        Assert.Contains("CreateProxy(", output);
        Assert.Contains("CreateStub(", output);

        // Correct proxy/stub references
        Assert.Contains("CIWidgetProxy", output);
        Assert.Contains("CStub___x_Widgets_IWidget", output);

        // DLL exports
        Assert.Contains("DllGetClassObject", output);
        Assert.Contains("DllCanUnloadNow", output);
        Assert.Contains("DllRegisterServer", output);
        Assert.Contains("DllUnregisterServer", output);
    }

    [Fact]
    public void EndToEnd_MultipleInterfaces_FactoryHandlesAll()
    {
        var source = @"
            namespace TestApp
            {
                interface IAlpha
                {
                    void DoAlpha();
                };

                interface IBeta
                {
                    void DoBeta();
                };

                interface IGamma
                {
                    void DoGamma(Int32 value);
                };
            }";

        var compilation = Compile(source);
        var output = _factoryGenerator.Generate(compilation);

        // CreateProxy handles all three
        Assert.Contains("CIAlphaProxy", output);
        Assert.Contains("CIBetaProxy", output);
        Assert.Contains("CIGammaProxy", output);

        // CreateStub handles all three
        Assert.Contains("CStub___x_TestApp_IAlpha", output);
        Assert.Contains("CStub___x_TestApp_IBeta", output);
        Assert.Contains("CStub___x_TestApp_IGamma", output);

        // All IIDs referenced
        Assert.Contains("IID___x_TestApp_IAlpha", output);
        Assert.Contains("IID___x_TestApp_IBeta", output);
        Assert.Contains("IID___x_TestApp_IGamma", output);
    }

    [Fact]
    public void EndToEnd_DeepNamespace_ManglesCorrectly()
    {
        var source = @"
            namespace Company.Product.Feature
            {
                interface IService
                {
                    void Execute();
                };
            }";

        var compilation = Compile(source);
        var output = _factoryGenerator.Generate(compilation);

        Assert.Contains("__x_Company_Product_Feature_IService", output);
        Assert.Contains("CIServiceProxy", output);
        Assert.Contains("CStub___x_Company_Product_Feature_IService", output);
    }

    [Fact]
    public void EndToEnd_SkipsExternalInterfaces()
    {
        var source = @"
            namespace TestApp
            {
                interface ILocal
                {
                    void DoLocal();
                };
            }";

        var compilation = Compile(source);
        var output = _factoryGenerator.Generate(compilation);

        Assert.Contains("CILocalProxy", output);
        Assert.DoesNotContain("CIInspectableProxy", output);
    }

    [Fact]
    public void EndToEnd_NoInterfaces_ProducesEmptyOutput()
    {
        var source = @"
            namespace TestApp
            {
                enum Color { Red, Green, Blue };
                struct Point { Single X; Single Y; };
            }";

        var compilation = Compile(source);
        var output = _factoryGenerator.Generate(compilation);

        Assert.DoesNotContain("class CPSFactory", output);
        Assert.Contains("auto-generated", output);
    }

    [Fact]
    public void EndToEnd_FullProxyFile_AllPartsCoherent()
    {
        var source = @"
            namespace Widgets
            {
                delegate void ClickHandler(Object sender, Int32 args);

                interface IButton
                {
                    String Label { get; set; };
                    event ClickHandler Click;
                    void Press();
                };
            }";

        var compilation = Compile(source);

        // Generate all three parts
        var proxyOutput = _proxyGenerator.Generate(compilation);
        var stubOutput = _stubGenerator.Generate(compilation);
        var factoryOutput = _factoryGenerator.Generate(compilation);

        // Proxy generates the class referenced by factory
        Assert.Contains("class CIButtonProxy", proxyOutput);
        Assert.Contains("CIButtonProxy", factoryOutput);

        // Stub generates the class referenced by factory
        Assert.Contains("class CStub___x_Widgets_IButton", stubOutput);
        Assert.Contains("CStub___x_Widgets_IButton", factoryOutput);

        // Factory uses the same IID as proxy and stub
        Assert.Contains("IID___x_Widgets_IButton", proxyOutput);
        Assert.Contains("IID___x_Widgets_IButton", stubOutput);
        Assert.Contains("IID___x_Widgets_IButton", factoryOutput);
    }

    [Fact]
    public void EndToEnd_DefFile_MatchesCppExports()
    {
        var source = @"
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                };
            }";

        var compilation = Compile(source);
        var cppOutput = _factoryGenerator.Generate(compilation);
        var defOutput = _factoryGenerator.GenerateDefFile(compilation, "testproxy");

        // Every export in the .def file should exist in the .cpp output
        Assert.Contains("DllGetClassObject", cppOutput);
        Assert.Contains("DllGetClassObject", defOutput);
        Assert.Contains("DllCanUnloadNow", cppOutput);
        Assert.Contains("DllCanUnloadNow", defOutput);
        Assert.Contains("DllRegisterServer", cppOutput);
        Assert.Contains("DllRegisterServer", defOutput);
        Assert.Contains("DllUnregisterServer", cppOutput);
        Assert.Contains("DllUnregisterServer", defOutput);
    }

    [Fact]
    public void EndToEnd_DllRegisterServer_RegistersAllInterfaces()
    {
        var source = @"
            namespace TestApp
            {
                interface IFirst
                {
                    void DoFirst();
                };
                interface ISecond
                {
                    void DoSecond();
                };
            }";

        var compilation = Compile(source);
        var output = _factoryGenerator.Generate(compilation);

        // DllRegisterServer should register both IIDs
        var regIdx = output.IndexOf("DllRegisterServer()");
        var unregIdx = output.IndexOf("DllUnregisterServer()");
        var regBody = output[regIdx..unregIdx];

        Assert.Contains("IID___x_TestApp_IFirst", regBody);
        Assert.Contains("IID___x_TestApp_ISecond", regBody);
    }
}
