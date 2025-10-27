using FluentAssertions;
using NForza.Cyrus.Abstractions;
using NForza.Cyrus.Generators.Tests.Infra;
using Xunit.Abstractions;

namespace NForza.Cyrus.Generators.Tests;

public class ModelTests(ITestOutputHelper outputWindow)
{
    [Fact]
    public async Task Generating_Command_With_Route_Should_Generate_Model_With_Command()
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

        var model =  generatedSyntaxTrees.GetGeneratedModel();
        model.Should().NotBeNull();
        model.Commands.Should().ContainSingle(cd => cd.Name == "NewCustomerCommand");
        model.Endpoints.Should().ContainSingle(ed => ed.Route == "/" && ed.CommandName == "NewCustomerCommand" && ed.HttpVerb == HttpVerb.Post);
    }

    [Fact]
    public async Task Generating_Query_With_Route_Should_Generate_Model_With_Query()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;
                using NForza.Cyrus.SignalR;         

                namespace Test;

                [Query] 
                public record CustomerQuery(int Id);

                public static class CustomerQueryHandler
                {
                    [QueryHandler(Route = ""/query"")]
                    public static int Handle(CustomerQuery qry)
                    {
                        return qry.Id;      
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

        var model = generatedSyntaxTrees.GetGeneratedModel();
        model.Should().NotBeNull();
        model.Queries.Should().ContainSingle(cd => cd.Name == "CustomerQuery");
        model.Endpoints.Should().ContainSingle(ed => ed.Route == "/query" && ed.QueryName == "CustomerQuery" && ed.HttpVerb == HttpVerb.Get);
    }
}
