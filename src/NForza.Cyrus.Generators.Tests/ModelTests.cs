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

    [Fact]
    public async Task Generating_GuidValue_Should_Not_Generate_GuidValue_In_Models()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;
                using NForza.Cyrus.SignalR;         

                namespace Test;

                [GuidValue] 
                public partial record struct CustomerId;

                [Query] 
                public record CustomerQuery(CustomerId Id);

                public static class CustomerQueryHandler
                {
                    [QueryHandler(Route = ""/query"")]
                    public static CustomerId Handle(CustomerQuery qry)
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
        model.Guids.Should().ContainSingle(g => g == "CustomerId");
        model.Models.Should().NotContain(md => md.Name == "CustomerId");
    }

    [Fact]
    public async Task Generating_Query_Should_Generate_Query_ReturnType_In_Model()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;
                using NForza.Cyrus.SignalR;         

                namespace Test;

                [GuidValue] 
                public partial record struct CustomerId;

                public record Customer(CustomerId Id, string Name);

                [Query] 
                public record CustomerQuery(CustomerId Id);

                public static class CustomerQueryHandler
                {
                    [QueryHandler(Route = ""/query"")]
                    public static Customer Handle(CustomerQuery qry)
                    {
                        return new Customer(qry.Id, """");      
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
        model.Guids.Should().ContainSingle(g => g == "CustomerId");
        model.Models.Should().Contain(md => md.Name == "Customer");
        model.Models.Should().NotContain(md => md.Name == "CustomerId");
    }
}
