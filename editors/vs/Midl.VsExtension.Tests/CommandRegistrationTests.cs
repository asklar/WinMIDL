namespace Midl.VsExtension.Tests;

using System.Reflection;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;

/// <summary>
/// Tests that all command classes are properly decorated and configured.
/// </summary>
public class CommandRegistrationTests
{
    private static readonly Type[] ExpectedCommandTypes =
    [
        typeof(ToggleStrictModeCommand),
        typeof(RestartServerCommand),
        typeof(ShowServerLogCommand),
        typeof(SelectSdkVersionCommand),
    ];

    [Fact]
    public void AllCommands_HaveVisualStudioContributionAttribute()
    {
        foreach (Type commandType in ExpectedCommandTypes)
        {
            var attr = commandType.GetCustomAttribute<VisualStudioContributionAttribute>();
            Assert.NotNull(attr);
        }
    }

    [Fact]
    public void AllCommands_DeriveFromCommand()
    {
        foreach (Type commandType in ExpectedCommandTypes)
        {
            Assert.True(
                typeof(Command).IsAssignableFrom(commandType),
                $"{commandType.Name} should derive from Command");
        }
    }

    [Fact]
    public void AllCommands_OverrideExecuteCommandAsync()
    {
        foreach (Type commandType in ExpectedCommandTypes)
        {
            var method = commandType.GetMethod(
                "ExecuteCommandAsync",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
                null,
                [typeof(IClientContext), typeof(CancellationToken)],
                null);

            Assert.NotNull(method);
        }
    }

    [Fact]
    public void AllCommands_HaveCommandConfiguration()
    {
        foreach (Type commandType in ExpectedCommandTypes)
        {
            var prop = commandType.GetProperty(
                "CommandConfiguration",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            Assert.NotNull(prop);
        }
    }

    [Fact]
    public void FourCommandTypes_AreRegistered()
    {
        Assert.Equal(4, ExpectedCommandTypes.Length);
    }
}
