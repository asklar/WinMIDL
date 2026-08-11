// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.CodeGen.Decompiler;
using Midl.CodeGen.WinMd;
using Midl.Lsp;
using Midl.Lsp.Handlers;

namespace Midl.Integration.Tests;

/// <summary>
/// F12 inside a decompiled WinMD view, on a type that lives in a <em>different</em> WinMD.
///
/// <c>HandleWinMdView</c> derives a single WinMD path from the view's URI and searches only
/// that file, so a reference to a type defined elsewhere — a Windows SDK type, in practice —
/// found nothing and navigation silently did not happen. Nearly every real WinMD references
/// SDK types, so this is the common case rather than an edge one.
///
/// The existing tests in <c>GotoDefinitionTests.WinMdView</c> all navigate within a single
/// WinMD, which is why they stayed green. They also each begin with a silent
/// <c>if (!File.Exists(sdkPath)) return;</c> against a hardcoded SDK build, so on a machine
/// without that exact build they pass while asserting nothing. These tests depend on nothing
/// outside the repository: both WinMDs are emitted by the test.
/// </summary>
public class GotoDefinitionCrossWinMdTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "midl-xwinmd-" + Guid.NewGuid().ToString("N")[..8]);

    public GotoDefinitionCrossWinMdTests() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        try { Directory.Delete(_dir, recursive: true); } catch { }
    }

    private string Emit(string name, string idl, params string[] references)
    {
        var compilation = CompilationHelper.Compile(idl, references.Length == 0 ? null : references,
            CompilationMode.Strict);
        var path = Path.Combine(_dir, name + ".winmd");
        new WinMdEmitter().Emit(compilation, path);
        Assert.True(File.Exists(path), $"failed to emit {name}.winmd");
        return path;
    }

    /// <summary>Defines the type the other WinMD refers to.</summary>
    private string EmitLibrary() => Emit("Contoso.Library", """
        namespace Contoso.Library
        {
            interface IStorable
            {
                void Store();
            }
        }
        """);

    /// <summary>Refers to a type from another WinMD, exactly as a real WinMD refers to SDK types.</summary>
    private string EmitConsumer(string libraryWinMd) => Emit("Contoso.App", """
        namespace Contoso.App
        {
            runtimeclass Widget
            {
                Widget();
                void Take(Contoso.Library.IStorable item);
            }
        }
        """, libraryWinMd);

    [Fact]
    public void F12OnATypeFromAnotherWinMd_NavigatesToThatWinMd()
    {
        var library = EmitLibrary();
        var consumer = EmitConsumer(library);

        var decompiled = new WinMdDecompiler().Decompile(consumer, "Contoso.App");
        var uri = "midl-winmd:///" + consumer.Replace('\\', '/') + "/Contoso.App";

        int idx = decompiled.IndexOf("IStorable", StringComparison.Ordinal);
        Assert.True(idx > 0,
            $"expected the decompiled view to name the type it derives from; got:\n{decompiled}");

        var lineStarts = PositionHelper.BuildLineStarts(decompiled);
        var (line, ch) = PositionHelper.OffsetToLineChar(lineStarts, idx);

        var result = GotoDefinitionHandler.HandleCore(decompiled, uri, line, ch,
            referencePaths: new[] { library });

        Assert.NotNull(result);
        var dict = Assert.IsType<Dictionary<string, object?>>(result!);
        var resultUri = dict["uri"]!.ToString()!;

        Assert.Contains("midl-winmd:", resultUri);
        Assert.Contains("Contoso.Library.winmd", resultUri);
    }

    /// <summary>
    /// Navigation within the same WinMD must keep working: the fallback may only apply when
    /// the type genuinely is not in the current file.
    /// </summary>
    [Fact]
    public void F12OnALocalType_StaysInTheSameWinMd()
    {
        var library = EmitLibrary();
        var local = Emit("Contoso.Local", """
            namespace Contoso.Local
            {
                interface IThing
                {
                    void Go();
                }
                runtimeclass Widget : IThing
                {
                    Widget();
                }
            }
            """);

        var decompiled = new WinMdDecompiler().Decompile(local, "Contoso.Local");
        var uri = "midl-winmd:///" + local.Replace('\\', '/') + "/Contoso.Local";

        int idx = decompiled.IndexOf("IThing", StringComparison.Ordinal);
        Assert.True(idx > 0, $"expected IThing in the decompiled view; got:\n{decompiled}");

        var lineStarts = PositionHelper.BuildLineStarts(decompiled);
        var (line, ch) = PositionHelper.OffsetToLineChar(lineStarts, idx);

        var result = GotoDefinitionHandler.HandleCore(decompiled, uri, line, ch,
            referencePaths: new[] { library });

        Assert.NotNull(result);
        var dict = Assert.IsType<Dictionary<string, object?>>(result!);
        var resultUri = dict["uri"]!.ToString()!;

        Assert.Contains("Contoso.Local.winmd", resultUri);
        Assert.DoesNotContain("Contoso.Library.winmd", resultUri);
    }

    /// <summary>
    /// The same namespace name can exist in two WinMDs. When the type is not in the file
    /// being viewed, navigation must go to the file that defines it rather than searching
    /// the current document for a name it does not contain.
    /// </summary>
    [Fact]
    public void F12OnATypeInTheSameNamespaceButAnotherWinMd_NavigatesToThatWinMd()
    {
        // Both WinMDs use the namespace Contoso.App; only the library defines IShared.
        var library = Emit("Contoso.Shared", """
            namespace Contoso.App
            {
                interface IShared
                {
                    void Share();
                }
            }
            """);

        var consumer = Emit("Contoso.App", """
            namespace Contoso.App
            {
                runtimeclass Widget
                {
                    Widget();
                    void Take(Contoso.App.IShared item);
                }
            }
            """, library);

        var decompiled = new WinMdDecompiler().Decompile(consumer, "Contoso.App");
        var uri = "midl-winmd:///" + consumer.Replace('\\', '/') + "/Contoso.App";

        int idx = decompiled.IndexOf("IShared", StringComparison.Ordinal);
        Assert.True(idx > 0, $"expected IShared in the decompiled view; got:\n{decompiled}");

        var lineStarts = PositionHelper.BuildLineStarts(decompiled);
        var (line, ch) = PositionHelper.OffsetToLineChar(lineStarts, idx);

        var result = GotoDefinitionHandler.HandleCore(decompiled, uri, line, ch,
            referencePaths: new[] { library });

        Assert.NotNull(result);
        var dict = Assert.IsType<Dictionary<string, object?>>(result!);
        var resultUri = dict["uri"]!.ToString()!;

        Assert.Contains("Contoso.Shared.winmd", resultUri);
    }

    /// <summary>
    /// With no references supplied there is nothing to fall back to, so the handler must
    /// return null rather than guess at a file.
    /// </summary>
    [Fact]
    public void F12OnATypeFromAnotherWinMd_WithoutReferences_ReturnsNull()
    {
        var library = EmitLibrary();
        var consumer = EmitConsumer(library);

        var decompiled = new WinMdDecompiler().Decompile(consumer, "Contoso.App");
        var uri = "midl-winmd:///" + consumer.Replace('\\', '/') + "/Contoso.App";

        int idx = decompiled.IndexOf("IStorable", StringComparison.Ordinal);
        Assert.True(idx > 0);

        var lineStarts = PositionHelper.BuildLineStarts(decompiled);
        var (line, ch) = PositionHelper.OffsetToLineChar(lineStarts, idx);

        var result = GotoDefinitionHandler.HandleCore(decompiled, uri, line, ch);

        Assert.Null(result);
    }
}
