using FluentAssertions;
using NForza.Cyrus.Generators.Analyzers;
using NForza.Cyrus.Generators.Tests.Infra;
using Xunit.Abstractions;

namespace NForza.Cyrus.Generators.Tests;

public class EventHandlerTests(ITestOutputHelper outputWindow)
{
    [Fact]
    public async Task Generating_EventHandler_Should_Compile_And_Generate_Sources()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;

                namespace Test;
            
                [Event]
                public record CustomerCreatedEvent(Guid Id);

                public static class CustomerCreatedEventHandler
                {
                    [EventHandler]
                    public static void Handle(CustomerCreatedEvent @event)
                    {
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

        generatedSyntaxTrees.Should().NotBeEmpty();
        generatedSyntaxTrees.Should().ContainSource("AddEventHandler<global::Test.CustomerCreatedEvent>");
    }

    [Fact]
    public async Task Generating_Event_And_EventHandler_Without_Namespace_Should_Generate_Valid_Source()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;

                [Event]
                public record CustomerCreatedEvent(Guid Id);

                public static class CustomerCreatedEventHandler
                {
                    [EventHandler]
                    public static void Handle(CustomerCreatedEvent @event)
                    {
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

        generatedSyntaxTrees.Should().NotBeEmpty();
    }
}
