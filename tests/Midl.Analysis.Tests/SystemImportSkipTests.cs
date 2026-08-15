// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.Analysis.Diagnostics;

namespace Midl.Analysis.Tests;

/// <summary>
/// A WinRT IDL frequently imports "Windows.Foundation.idl", which transitively imports the
/// classic COM/OLE system IDLs (oaidl.idl, objidl.idl, wtypes.idl, ...). Those use classic
/// RPC grammar (encapsulated unions, wire types) WinMIDL does not parse. Their types are
/// provided as built-ins or via referenced winmds, so the compiler skips textually compiling
/// them rather than failing the whole build.
/// </summary>
public class SystemImportSkipTests
{
    private static (string dir, string primary) WritePair(string classicFileName, string classicBody)
    {
        var dir = Path.Combine(Path.GetTempPath(), "winmidl_sysimport_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, classicFileName), classicBody);
        var primary = Path.Combine(dir, "primary.idl");
        File.WriteAllText(primary, $"import \"{classicFileName}\";\nnamespace N {{ enum E {{ A, B }}; }}\n");
        return (dir, primary);
    }

    [Fact]
    public void ImportingAClassicSystemIdl_IsSkipped_NotParsed()
    {
        // Encapsulated non-discriminated union — classic RPC grammar WinMIDL does not parse.
        const string classic = "typedef union _wireVARIANT switch(unsigned long sf) u {\n" +
                               "    case 1: long lVal;\n" +
                               "    case 2: double dblVal;\n" +
                               "} VARIANTUNION;\n";
        var (dir, primary) = WritePair("oaidl.idl", classic);
        try
        {
            var source = File.ReadAllText(primary);
            var c = CompilationHelper.Compile(source, referencePaths: null, mode: CompilationMode.Compat,
                includePaths: new[] { dir }, currentFilePath: primary);

            // The classic file was skipped, so neither "not found" (MIDL1047) nor any parse
            // error from its unparseable body appears.
            Assert.DoesNotContain(c.Diagnostics, d => d.Code == "MIDL1047");
            Assert.DoesNotContain(c.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ANonSystemImportThatDoesNotExist_StillReportsNotFound()
    {
        // The skip must not swallow a genuine missing-import for a user file.
        const string source = "import \"myhelpers.idl\";\nnamespace N { enum E { A, B }; }\n";
        var c = CompilationHelper.Compile(source, referencePaths: null, mode: CompilationMode.Compat,
            includePaths: null, currentFilePath: @"C:\fake\primary.idl");

        Assert.Contains(c.Diagnostics, d => d.Code == "MIDL1047" && d.Message.Contains("myhelpers.idl"));
    }
}
