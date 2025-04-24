using FluentAssertions;
using NForza.Cyrus.Generators.Analyzers;
using NForza.Cyrus.Generators.Tests.Infra;
using Xunit.Abstractions;

namespace NForza.Cyrus.Generators.Tests;

public class CommandHandlerTests(ITestOutputHelper outputWindow)
{
    [Fact]
    public async Task Generating_CommandHandler_Should_Compile_And_Generate_Sources()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;

                namespace Test;
            
                [Command]
                public record CreateCustomerCommand(Guid Id);

                public class Customer
                {
                    [CommandHandler]
                    public void Handle(CreateCustomerCommand command)
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
        generatedSyntaxTrees.Should().ContainSource("ServiceCollectionJsonConverterExtensions: ICyrusInitializer");
        generatedSyntaxTrees.Should().ContainSource("CommandDispatcherExtensions");
        generatedSyntaxTrees.Should().ContainSource("Handle(this ICommandDispatcher commandDispatcher, global::Test.CreateCustomerCommand command");
        generatedSyntaxTrees.Should().ContainSource("CommandHandlersRegistration");
        generatedSyntaxTrees.Should().ContainSource("AddTransient<global::Test.Customer>()");
    }

    [Fact]
    public async Task Generating_Static_CommandHandler_Should_Compile_And_Generate_Sources()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;

                namespace Test;
            
                [Command]
                public record CreateCustomerCommand(Guid Id);

                public static class Customer
                {
                    [CommandHandler]
                    public static void Handle(CreateCustomerCommand command)
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
        generatedSyntaxTrees.Should().ContainSource("ServiceCollectionJsonConverterExtensions: ICyrusInitializer");
        generatedSyntaxTrees.Should().ContainSource("CommandDispatcherExtensions");
        generatedSyntaxTrees.Should().ContainSource("Handle(this ICommandDispatcher commandDispatcher, global::Test.CreateCustomerCommand command");
        generatedSyntaxTrees.Should().NotContainSource("CommandHandlersRegistration");
        generatedSyntaxTrees.Should().NotContainSource("AddTransient<global::Test.Customer>()");
    }

    [Fact]
    public async Task Generating_CommandHandler_For_Type_Which_Is_Not_A_Command_Should_Return_Error()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;

                namespace Test;
            
                public record CreateCustomerCommand(Guid Id);

                public class Customer
                {
                    [CommandHandler]
                    public void Handle(CreateCustomerCommand command)
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
        analyzerOutput.Should().ContainError(DiagnosticDescriptors.MissingCommandAttribute);
    }

    [Fact]
    public async Task Generating_CommandHandler_Without_A_Route_Should_Not_Generate_MapPost()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;

                namespace Test;
            
                [Command]
                public record CreateCustomerCommand(Guid Id);

                public class Customer
                {
                    [CommandHandler]
                    public void Handle(CreateCustomerCommand command)
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
        generatedSyntaxTrees.Should().NotContainSource("MapPost");
    }

    [Fact]
    public async Task Generating_CommandHandler_With_A_Route_Should_Generate_MapPost()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;

                namespace Test;
            
                [Command]
                public record CreateCustomerCommand(Guid Id);

                public class Customer
                {
                    [CommandHandler(Route = ""/"")]
                    public void Handle(CreateCustomerCommand command)
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
        generatedSyntaxTrees.Should().ContainSource("MapPost");
    }

}
