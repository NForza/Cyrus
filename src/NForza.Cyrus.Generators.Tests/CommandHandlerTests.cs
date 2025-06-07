using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NForza.Cyrus.Abstractions;
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

    [Fact]
    public async Task Generating_CommandHandler_Invalid_IResult_Tuple_Element_Name_Should_Generate_Analyzer_Error()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;
                using Microsoft.AspNetCore.Http;

                namespace Test;
            
                [Command]
                public record CreateCustomerCommand(Guid Id);

                public class Customer
                {
                    [CommandHandler(Route = ""/"")]
                    public (IResult wrong_name, object Events) Handle(CreateCustomerCommand command)
                    {
                        return (Results.Accepted(), new object());
                    }
                }
            ";

        (var compilerOutput, var analyzerOutput, var generatedSyntaxTrees) =
            await new CyrusGeneratorTestBuilder()
            .WithSource(source)
            .LogGeneratedSource(outputWindow.WriteLine)
            .RunAsync();

        compilerOutput.Should().NotHaveErrors();
        analyzerOutput.Should().ContainDiagnostic(DiagnosticDescriptors.IResultTupleElementShouldBeCalledResult);
    }

    [Fact]
    public async Task Generating_CommandHandler_Invalid_Events_Tuple_Element_Name_Should_Generate_Analyzer_Error()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;
                using Microsoft.AspNetCore.Http;

                namespace Test;
            
                [Command]
                public record CreateCustomerCommand(Guid Id);

                public class Customer
                {
                    [CommandHandler(Route = ""/"")]
                    public (IResult Result, object wrong_name) Handle(CreateCustomerCommand command)
                    {
                        return (Results.Accepted(), new object());
                    }
                }
            ";

        (var compilerOutput, var analyzerOutput, var generatedSyntaxTrees) =
            await new CyrusGeneratorTestBuilder()
            .WithSource(source)
            .LogGeneratedSource(outputWindow.WriteLine)
            .RunAsync();

        compilerOutput.Should().NotHaveErrors();
        analyzerOutput.Should().ContainDiagnostic(DiagnosticDescriptors.MessagesTupleElementShouldBeCalledMessages);
    }

    [Fact]
    public async Task Generating_CommandHandler_Invalid_Single_Event_Tuple_Element_Name_Should_Generate_Analyzer_Error()
    {
        var source = @"
                using System;       
                using System.Collections.Generic;
                using NForza.Cyrus.Abstractions;
                using Microsoft.AspNetCore.Http;

                namespace Test;
            
                [Command]
                public record CreateCustomerCommand(Guid Id);

                public class Customer
                {
                    [CommandHandler(Route = ""/"")]
                    public (IResult Result, IEnumerable<object> wrong_name) Handle(CreateCustomerCommand command)
                    {
                        return (Results.Accepted(), [new object()]);
                    }
                }
            ";

        (var compilerOutput, var analyzerOutput, var generatedSyntaxTrees) =
            await new CyrusGeneratorTestBuilder()
            .WithSource(source)
            .LogGeneratedSource(outputWindow.WriteLine)
            .RunAsync();

        compilerOutput.Should().NotHaveErrors();
        analyzerOutput.Should().ContainDiagnostic(DiagnosticDescriptors.MessagesTupleElementShouldBeCalledMessages);
    }

    [Fact]
    public async Task CommandHandler_With_AggregateRoot_Parameter_But_Parameter_Has_No_AggregateRoot_Attribute_Should_Generate_Analyzer_Error()
    {
        var source = @"
                using System;       
                using System.Collections.Generic;
                using NForza.Cyrus.Abstractions;
                using Microsoft.AspNetCore.Http;

                namespace Test;
            
                [Command]
                public record CreateCustomerCommand(Guid Id);

                public class Customer { }

                public class CustomerHandler
                {
                    [CommandHandler(Route = ""/"")]
                    public (IResult Result, IEnumerable<object> Messages) Handle(CreateCustomerCommand command, Customer customer)
                    {
                        return (Results.Accepted(), [new object()]);
                    }
                }
            ";

        (var compilerOutput, var analyzerOutput, var generatedSyntaxTrees) =
            await new CyrusGeneratorTestBuilder()
            .WithSource(source)
            .LogGeneratedSource(outputWindow.WriteLine)
            .RunAsync();

        analyzerOutput.Should().ContainDiagnostic(DiagnosticDescriptors.CommandHandlerArgumentShouldBeAnAggregateRoot);
    }

    [Fact]
    public async Task CommandHandler_With_AggregateRoot_Parameter_But_Parameter_Has_No_AggregateRootID_On_A_Property_Should_Generate_Analyzer_Error()
    {
        var source = @"
                using System;       
                using System.Collections.Generic;
                using NForza.Cyrus.Abstractions;
                using Microsoft.AspNetCore.Http;

                namespace Test;
            
                [Command]
                public record CreateCustomerCommand(Guid Id);

                [AggregateRoot]
                public class Customer { }

                public class CustomerHandler
                {
                    [CommandHandler(Route = ""/"")]
                    public (IResult Result, IEnumerable<object> Messages) Handle(CreateCustomerCommand command, Customer customer)
                    {
                        return (Results.Accepted(), [new object()]);
                    }
                }
            ";

        (var compilerOutput, var analyzerOutput, var generatedSyntaxTrees) =
            await new CyrusGeneratorTestBuilder()
            .WithSource(source)
            .LogGeneratedSource(outputWindow.WriteLine)
            .RunAsync();

        analyzerOutput.Should().ContainDiagnostic(DiagnosticDescriptors.CommandForCommandHandlerShouldHaveAggregateRootIdProperty);
    }

    [Fact]
    public async Task CommandHandler_With_AggregateRoot_Parameter_And_Parameter_Has_AggregateRootID_On_A_Property_Should_Not_Generate_Analyzer_Error()
    {
        var source = @"
                using System;       
                using System.Collections.Generic;
                using NForza.Cyrus.Abstractions;
                using Microsoft.AspNetCore.Http;

                namespace Test;
            
                [Command]
                public record CreateCustomerCommand([property: AggregateRootId]Guid Id);

                [AggregateRoot]
                public class Customer 
                { 
                    [AggregateRootId]
                    public Guid CustomerId { get; set; }
                }

                public class CustomerHandler
                {
                    [CommandHandler(Route = ""/"")]
                    public (IResult Result, IEnumerable<object> Messages) Handle(CreateCustomerCommand command, Customer customer)
                    {
                        return (Results.Accepted(), [new object()]);
                    }
                }
            ";

        (var compilerOutput, var analyzerOutput, var generatedSyntaxTrees) =
            await new CyrusGeneratorTestBuilder()
            .WithSource(source)
            .LogGeneratedSource(outputWindow.WriteLine)
            .RunAsync();

        analyzerOutput.Should().BeEmpty();
    }

    [Fact]
    public async Task Async_CommandHandler_With_Result_And_Messages_Should_Generate_Valid_CommandDispatcher_Method()
    {
        var source = @"
                using System;
                using System.Collections.Generic;
                using System.Threading.Tasks;
                using Microsoft.AspNetCore.Http;
                using NForza.Cyrus.Abstractions;                

                namespace Test;

                [Command]
                public record struct NewTrackCommand(Guid TrackId);

                [Event]
                public record TrackCreatedEvent(Guid TrackId);

                public class NewTrackCommandHandler
                {
                    [CommandHandler]
                    public async Task<(IResult Result, IEnumerable<object> Messages)> Handle(NewTrackCommand command)
                    {
                        return (Results.Accepted(), [new TrackCreatedEvent(command.TrackId)]);
                    }
                }
            ";

        (var compilerOutput, var analyzerOutput, var generatedSyntaxTrees) =
            await new CyrusGeneratorTestBuilder()
            .WithSource(source)
            .LogGeneratedSource(outputWindow.WriteLine)
            .RunAsync();

        analyzerOutput.Should().BeEmpty();
        compilerOutput.Should().NotHaveErrors();
        generatedSyntaxTrees.Should().ContainSource("Handle(this ICommandDispatcher");
    }

    [Fact]
    public async Task Command_And_CommandHandler_Without_Namespace_Should_Generate_Valid_Code()
    {
        var source = @"
                using System;
                using System.Collections.Generic;
                using System.Threading.Tasks;
                using Microsoft.AspNetCore.Http;
                using NForza.Cyrus.Abstractions;                

                [Command]
                public record struct NewTrackCommand(Guid TrackId);

                public class NewTrackCommandHandler
                {
                    [CommandHandler]
                    public async Task<IResult> Handle(NewTrackCommand command) => Results.Accepted();
                }
            ";

        (var compilerOutput, var analyzerOutput, var generatedSyntaxTrees) =
            await new CyrusGeneratorTestBuilder()
            .WithSource(source)
            .LogGeneratedSource(outputWindow.WriteLine)
            .RunAsync();

        analyzerOutput.Should().BeEmpty();
        compilerOutput.Should().NotHaveErrors();
    }

}
