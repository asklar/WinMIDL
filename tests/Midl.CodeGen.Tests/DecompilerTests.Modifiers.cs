using System.Text.RegularExpressions;
using Midl.CodeGen.Decompiler;

namespace Midl.CodeGen.Tests;

public partial class DecompilerTests
{
    #region No-duplicate overridable/protected/static members

    [Fact]
    public void Decompile_OverridableMethod_NoDuplicate()
    {
        var source = @"
            namespace N {
                unsealed runtimeclass Widget {
                    void PublicMethod();
                    overridable void OnActivated();
                }
            }";
        var bytes = CompileAndEmit(source);
        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("overridable void OnActivated", idl);
        var count = Regex.Matches(idl, @"\bOnActivated\b").Count;
        Assert.Equal(1, count); // exactly one occurrence
    }

    [Fact]
    public void Decompile_ProtectedMethod_NoDuplicate()
    {
        var source = @"
            namespace N {
                unsealed runtimeclass Widget {
                    protected void Initialize();
                    void DoWork();
                }
            }";
        var bytes = CompileAndEmit(source);
        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("protected void Initialize", idl);
        var count = Regex.Matches(idl, @"\bInitialize\b").Count;
        Assert.Equal(1, count);
    }

    [Fact]
    public void Decompile_StaticMethod_NoDuplicate()
    {
        var source = @"
            namespace N {
                runtimeclass Widget {
                    void DoWork();
                    static void Reset();
                }
            }";
        var bytes = CompileAndEmit(source);
        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("static void Reset", idl);
        var count = Regex.Matches(idl, @"\bReset\b").Count;
        Assert.Equal(1, count);
    }

    [Fact]
    public void Decompile_OverridableProperty_NoDuplicate()
    {
        var source = @"
            namespace N {
                unsealed runtimeclass Widget {
                    String Name;
                    overridable String Label;
                }
            }";
        var bytes = CompileAndEmit(source);
        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("overridable String Label", idl);
        // Label should appear only in the overridable line (and nowhere else as a bare property)
        var count = Regex.Matches(idl, @"\bLabel\b").Count;
        Assert.Equal(1, count);
    }

    [Fact]
    public void Decompile_StaticProperty_NoDuplicate()
    {
        var source = @"
            namespace N {
                runtimeclass Widget {
                    String Name;
                    static String Default;
                }
            }";
        var bytes = CompileAndEmit(source);
        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("static String Default", idl);
        var count = Regex.Matches(idl, @"\bDefault\b").Count;
        Assert.Equal(1, count);
    }

    #endregion

    #region Version display

    [Fact]
    public void Decompile_VersionAttribute_RawValue()
    {
        // version(1) should display as version(1), not version(0.1)
        var source = @"
            namespace N {
                [version(2)]
                runtimeclass Widget {
                    void DoWork();
                }
            }";
        var bytes = CompileAndEmit(source);
        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("[version(2)]", idl);
        Assert.DoesNotContain("0.2", idl);
    }

    #endregion
}
