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
                using Microsoft.EntityFrameworkCore;

                namespace Test;
            
                [AggregateRoot]
                public record Customer([property: AggregateRootId] Guid Id);

                public class DemoContext : DbContext
                {
                    public DbSet<Customer> Customers { get; set; } = null!;
                }

                public class CyrusConfiguration: CyrusConfig
                {
                  public CyrusConfiguration()
                  {
                    UseEntityFrameworkPersistence<global::Test.DemoContext>();
                  }   
                }   
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