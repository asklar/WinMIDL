// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using Midl.Syntax;

namespace Midl.Integration.Tests;

public class FixtureAstTests
{
    private static string FixturesDir
    {
        get
        {
            var assemblyDir = Path.GetDirectoryName(typeof(FixtureAstTests).Assembly.Location)!;
            return Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "..", "..", "fixtures"));
        }
    }

    public static IEnumerable<object[]> GetTreeFixtures()
    {
        var dir = Path.Combine(FixturesDir, "valid");
        if (!Directory.Exists(dir)) return [];
        return Directory.GetFiles(dir, "*.expected.tree")
            .Select(f =>
            {
                // e.g. simple_namespace.expected.tree → simple_namespace.idl
                var baseName = Path.GetFileNameWithoutExtension(f); // simple_namespace.expected
                baseName = Path.GetFileNameWithoutExtension(baseName); // simple_namespace
                var idlPath = Path.Combine(Path.GetDirectoryName(f)!, baseName + ".idl");
                return new object[] { baseName, idlPath };
            });
    }

    [Theory]
    [MemberData(nameof(GetTreeFixtures))]
    public void AstMatchesExpected(string name, string idlPath)
    {
        var source = File.ReadAllText(idlPath);
        var treePath = Path.Combine(
            Path.GetDirectoryName(idlPath)!,
            name + ".expected.tree");
        Assert.True(File.Exists(treePath), $"Missing tree file: {treePath}");
        var expectedTree = File.ReadAllText(treePath);

        var lexer = new Midl.Lexer.Lexer(source);
        var tokens = lexer.Lex();
        var parser = new Midl.Parser.Parser(tokens);
        var tree = parser.Parse();

        var actualTree = DumpTree(tree, 0);

        // Normalize whitespace for comparison
        var expectedLines = expectedTree.Split('\n')
            .Select(l => l.TrimEnd())
            .Where(l => l.Length > 0)
            .ToList();
        var actualLines = actualTree.Split('\n')
            .Select(l => l.TrimEnd())
            .Where(l => l.Length > 0)
            .ToList();

        // Structural subset check: every expected line appears in actual
        foreach (var expectedLine in expectedLines)
        {
            var trimmedExpected = expectedLine.Trim();
            Assert.True(
                actualLines.Any(a => a.TrimStart() == expectedLine.TrimStart() ||
                    a.Contains(trimmedExpected)),
                $"[{name}] Expected tree line not found: '{expectedLine}'\n" +
                $"Actual tree:\n{actualTree}");
        }
    }

    private static string DumpTree(GreenNode node, int indent)
    {
        var sb = new StringBuilder();
        var prefix = new string(' ', indent);

        if (node is SyntaxToken token)
        {
            if (token.Kind == SyntaxKind.MissingToken)
                return "";
            if (token.Text.Length > 0)
                sb.AppendLine($"{prefix}{token.Kind} \"{token.Text}\"");
            else
                sb.AppendLine($"{prefix}{token.Kind}");
        }
        else
        {
            sb.AppendLine($"{prefix}{node.Kind}");
            for (int i = 0; i < node.SlotCount; i++)
            {
                var child = node.GetSlot(i);
                if (child != null)
                    sb.Append(DumpTree(child, indent + 2));
            }
        }
        return sb.ToString();
    }
}
