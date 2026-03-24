using Midl.Analysis;
using Midl.CodeGen.Header;

namespace Midl.Integration.Tests;

/// <summary>
/// Integration tests verifying header emission for enum fixtures with explicit values.
/// Compiles real IDL fixture files and checks the generated header output.
/// </summary>
public class HeaderParityEnumTests
{
    private static readonly string FixturesDir = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "fixtures", "valid");

    private readonly HeaderEmitter _emitter = new();

    private Compilation CompileFixture(string fixtureFileName)
    {
        var idlPath = Path.Combine(FixturesDir, fixtureFileName);
        Assert.True(File.Exists(idlPath), $"Fixture not found: {idlPath}");
        var source = File.ReadAllText(idlPath);
        return CompilationHelper.Compile(source);
    }

    [Fact]
    public void EnumVariations_ExplicitValues_EmittedCorrectly()
    {
        var compilation = CompileFixture("parity_enum_variations.idl");
        var header = _emitter.Emit(compilation);

        // SimpleSequential — auto-values
        Assert.Contains("SimpleSequential_First = 0,", header);
        Assert.Contains("SimpleSequential_Second = 1,", header);
        Assert.Contains("SimpleSequential_Third = 2,", header);
        Assert.Contains("SimpleSequential_Fourth = 3,", header);

        // ExplicitValues — non-sequential explicit
        Assert.Contains("ExplicitValues_None = 0,", header);
        Assert.Contains("ExplicitValues_Low = 10,", header);
        Assert.Contains("ExplicitValues_Medium = 100,", header);
        Assert.Contains("ExplicitValues_High = 1000,", header);
        Assert.Contains("ExplicitValues_Max = 10000,", header);

        // PermissionFlags — hex flags as decimal
        Assert.Contains("PermissionFlags_None = 0,", header);
        Assert.Contains("PermissionFlags_Read = 1,", header);
        Assert.Contains("PermissionFlags_Write = 2,", header);
        Assert.Contains("PermissionFlags_Execute = 4,", header);
        Assert.Contains("PermissionFlags_Delete = 8,", header);
        Assert.Contains("PermissionFlags_Admin = 16,", header);
        Assert.Contains("PermissionFlags_All = 31,", header);
    }

    [Fact]
    public void EnumsFlags_ExplicitValues_EmittedCorrectly()
    {
        var compilation = CompileFixture("parity_enums_flags.idl");
        var header = _emitter.Emit(compilation);

        // Color — sequential auto-values
        Assert.Contains("Color_Red = 0,", header);
        Assert.Contains("Color_Green = 1,", header);
        Assert.Contains("Color_Blue = 2,", header);

        // Priority — explicit sequential values
        Assert.Contains("Priority_Lowest = 0,", header);
        Assert.Contains("Priority_Low = 1,", header);
        Assert.Contains("Priority_Normal = 2,", header);
        Assert.Contains("Priority_High = 3,", header);
        Assert.Contains("Priority_Highest = 4,", header);
        Assert.Contains("Priority_Critical = 5,", header);

        // FileAccess flags — hex values emitted as decimal
        Assert.Contains("FileAccess_Read = 1,", header);
        Assert.Contains("FileAccess_Write = 2,", header);
        Assert.Contains("FileAccess_Execute = 4,", header);
        Assert.Contains("FileAccess_ReadWrite = 3,", header);
        Assert.Contains("FileAccess_All = 7,", header);

        // DayOfWeek flags — larger hex values as decimal
        Assert.Contains("DayOfWeek_None = 0,", header);
        Assert.Contains("DayOfWeek_Monday = 1,", header);
        Assert.Contains("DayOfWeek_Friday = 16,", header);
        Assert.Contains("DayOfWeek_Weekday = 31,", header);
        Assert.Contains("DayOfWeek_Weekend = 96,", header);
        Assert.Contains("DayOfWeek_All = 127,", header);

        // Planet — explicit starting at 1
        Assert.Contains("Planet_Mercury = 1,", header);
        Assert.Contains("Planet_Neptune = 8,", header);
    }
}
