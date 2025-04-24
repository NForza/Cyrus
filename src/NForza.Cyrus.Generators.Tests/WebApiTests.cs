using FluentAssertions;
using NForza.Cyrus.Generators.Tests.Infra;
using Xunit.Abstractions;

namespace NForza.Cyrus.Generators.Tests;

public class WebApiTests(ITestOutputHelper outputWindow)
{
    [Fact]
    public async Task Generating_Command_With_Default_Value_The_Contract_Should_Also_Have_That_Default()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;
                using NForza.Cyrus.SignalR;         

                namespace Test;

                [Command] 
                public record NewCustomerCommand(int Id = 1);

                public static class CustomerCommandHandler
                {
                    [CommandHandler(Route = ""/"")]
                    public static void Handle(NewCustomerCommand cmd)
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
        generatedSyntaxTrees.Should().Contain("NewCustomerCommandContract");
        generatedSyntaxTrees.Should().ContainMatch("*Id*= 1;*");
    }
}
