using FluentAssertions;
using NForza.Cyrus.Generators.Tests.Infra;
using Xunit.Abstractions;

namespace NForza.Cyrus.Generators.Tests;

public class SignalRHubTests(ITestOutputHelper outputWindow)
{
    [Fact]
    public async Task Generating_Valid_Hub_Should_Compile_And_Generate_Sources()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;
                using NForza.Cyrus.SignalR;         

                namespace Test;

                [Command] 
                public record NewCustomerCommand(Guid Id);

                public static class CustomerCommandHandler
                {
                    [CommandHandler]
                    public static void Handle(NewCustomerCommand cmd)
                    {
                    }
                }

                public class CustomerHub : CyrusSignalRHub
                {
                    public CustomerHub()
                    {
                        UsePath(""/customers"");
                        Expose<NewCustomerCommand>();
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
        generatedSyntaxTrees.Should().ContainSource("app.MapHub<global::Test.CustomerHub_Generated>(\"/customers\")");
    }

    [Fact]
    public async Task Generating_Hub_With_Emit_Event_Should_Compile_And_Generate_Sources()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;
                using NForza.Cyrus.SignalR;         

                namespace Test;

                [Command] 
                public record NewCustomerCommand(Guid Id);

                [Event]
                public record CustomerCreatedEvent(Guid Id);

                public static class CustomerCommandHandler
                {
                    [CommandHandler]
                    public static void Handle(NewCustomerCommand cmd)
                    {
                    }
                }

                public class CustomerHub : CyrusSignalRHub
                {
                    public CustomerHub()
                    {
                        UsePath(""/customers"");
                        Expose<NewCustomerCommand>();
                        Emit<CustomerCreatedEvent>();
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
        generatedSyntaxTrees.Should().ContainSource("app.MapHub<global::Test.CustomerHub_Generated>(\"/customers\")");
        generatedSyntaxTrees.Should().ContainSource("(typeof(global::Test.CustomerCreatedEvent), false)");
    }

    [Fact]
    public async Task Generating_Hub_With_Broadcast_Event_Should_Compile_And_Generate_Sources()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;
                using NForza.Cyrus.SignalR;         

                namespace Test;

                [Command] 
                public record NewCustomerCommand(Guid Id);

                [Event]
                public record CustomerCreatedEvent(Guid Id);

                public static class CustomerCommandHandler
                {
                    [CommandHandler]
                    public static void Handle(NewCustomerCommand cmd)
                    {
                    }
                }

                public class CustomerHub : CyrusSignalRHub
                {
                    public CustomerHub()
                    {
                        UsePath(""/customers"");
                        Expose<NewCustomerCommand>();
                        Broadcast<CustomerCreatedEvent>();
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
        generatedSyntaxTrees.Should().ContainSource("app.MapHub<global::Test.CustomerHub_Generated>(\"/customers\")");
        generatedSyntaxTrees.Should().ContainSource("(typeof(global::Test.CustomerCreatedEvent), true)");
    }
}