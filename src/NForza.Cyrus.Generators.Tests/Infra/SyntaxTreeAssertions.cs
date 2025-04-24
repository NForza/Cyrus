using FluentAssertions;
using FluentAssertions.Collections;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Tests.Infra;

public static class SyntaxTreeAssertions
{
    public static void Contain(this GenericCollectionAssertions<SyntaxTree> syntaxTrees, string source)
    {
        syntaxTrees.Subject.Should().Contain(tree => tree.ToString().Contains(source), $"Expected generated syntax tree to contain source: {source}");
    }

    public static void MatchRegex(this GenericCollectionAssertions<SyntaxTree> syntaxTrees, string regex)
    {
        syntaxTrees.Subject.Should().Contain(tree => System.Text.RegularExpressions.Regex.IsMatch(tree.ToString(), regex), $"Expected generated syntax tree to match regex: {regex}");
    }

    public static void ContainMatch(this GenericCollectionAssertions<SyntaxTree> syntaxTrees, string wildcard)
    {
        var generatedLines =
        syntaxTrees
            .Subject
            .SelectMany(s => s.ToString().Split(Environment.NewLine));

        generatedLines
            .Should()
            .ContainMatch(wildcard, $"Expected generated syntax tree to match wildcard: {wildcard}");
    }

    public static void NotContainSource(this GenericCollectionAssertions<SyntaxTree> syntaxTrees, string source)
    {
        syntaxTrees.Subject.Should().NotContain(tree => tree.ToString().Contains(source), $"Expected generated syntax tree not to contain source: {source}");
    }
}
