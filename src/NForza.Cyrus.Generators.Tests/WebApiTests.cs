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

    [Fact]
    public async Task Generating_Command_Contract_With_String_As_Argument_The_Contract_Should_Use_Bang_Operator()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;
                using NForza.Cyrus.SignalR;         

                namespace Test;

                [Command] 
                public record NewCustomerCommand(string name);

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
        generatedSyntaxTrees.Should().ContainMatch("*name!*");
    }


    [Fact]
    public async Task Generating_Command_Contract_With_Value_Type_As_Argument_The_Contract_Should_Not_Use_Bang_Operator()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;
                using NForza.Cyrus.SignalR;         

                namespace Test;

                [Command] 
                public record NewCustomerCommand(int val);

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
        generatedSyntaxTrees.Should().ContainMatch("*val.Value*");
    }

    [Fact]
    public async Task Generating_Command_Contract_With_Reference_Type_As_Argument_The_Contract_Should_Not_Use_Bang_Operator()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;
                using NForza.Cyrus.SignalR;         

                namespace Test;

                [Command] 
                public record NewCustomerCommand(object val);

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
        generatedSyntaxTrees.Should().ContainMatch("*val!*");
    }


    [Fact]
    public async Task Generating_Command_With_Property_In_Route_The_Contract_Should_Have_That_Property_As_Internal()
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
                    [CommandHandler(Route = ""/{Id}"")]
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
        generatedSyntaxTrees.Should().ContainMatch("*internal int? Id { get; set; } = 1;*");
    }

    [Fact]
    public async Task Generating_Command_Without_Property_In_Route_The_Contract_Should_Have_That_Property_As_Public()
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
        generatedSyntaxTrees.Should().ContainMatch("*public int? Id { get; set; } = 1;*");
    }

    [Fact]
    public async Task Generating_CommandHandler_With_Route_Should_Generate_Endpoint_In_Model()
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
        generatedSyntaxTrees.Should().Contain("new ModelEndpointDefinition(");
    }

}
