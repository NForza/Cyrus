﻿using FluentAssertions;
using FluentAssertions.Collections;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Tests.Infra;

public static class SyntaxTreeAssertions
{
    public static void ContainSource(this GenericCollectionAssertions<SyntaxTree> syntaxTrees, string source)
    {
        syntaxTrees.Subject.Should().Contain(tree => tree.ToString().Contains(source), $"Expected generated syntax tree to contain source: {source}");
    }
    public static void NotContainSource(this GenericCollectionAssertions<SyntaxTree> syntaxTrees, string source)
    {
        syntaxTrees.Subject.Should().NotContain(tree => tree.ToString().Contains(source), $"Expected generated syntax tree not to contain source: {source}");
    }
}
