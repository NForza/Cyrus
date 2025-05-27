using FluentAssertions;
using NForza.Cyrus.Generators.Tests.Infra;
using Xunit.Abstractions;

namespace NForza.Cyrus.Generators.Tests;

public class AggregateRootTests(ITestOutputHelper outputWindow)
{
    [Fact]
    public async Task Marking_A_AggregateRoot_Should_Compile()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;
                using NForza.Cyrus.Aggregates;

                namespace Test;
            
                [AggregateRoot]
                public record Customer([property: AggregateRootId] Guid Id);
            ";

        (var compilerOutput, var analyzerOutput, var generatedSyntaxTrees) =
            await new CyrusGeneratorTestBuilder()
            .WithSource(source)
            .LogGeneratedSource(outputWindow.WriteLine)
            .RunAsync();

        compilerOutput.Should().NotHaveErrors();
        analyzerOutput.Should().BeEmpty();
    }
}