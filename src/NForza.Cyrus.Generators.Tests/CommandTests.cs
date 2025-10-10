using FluentAssertions;
using NForza.Cyrus.Generators.Analyzers;
using NForza.Cyrus.Generators.Tests.Infra;
using Xunit.Abstractions;

namespace NForza.Cyrus.Generators.Tests;

public class CommandTests(ITestOutputHelper outputWindow)
{
    [Fact]
    public async Task Defining_A_Command_As_A_Record_Struct_Should_Generate_Analyzer_Error()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;

                namespace Test;
            
                [Command]
                public record struct CreateCustomerCommand(Guid Id);

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

        analyzerOutput.Should().ContainDiagnostic(DiagnosticDescriptors.CommandsCantBeStructs);
    }

    [Fact]
    public async Task Defining_A_Command_As_A_Struct_Should_Generate_Analyzer_Error()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;

                namespace Test;
            
                [Command]
                public struct CreateCustomerCommand(Guid Id);

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

        analyzerOutput.Should().ContainDiagnostic(DiagnosticDescriptors.CommandsCantBeStructs);
    }
    [Fact]
    public async Task Defining_A_Command_As_A_Record_Should_Not_Generate_Analyzer_Error()
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

        analyzerOutput.Should().BeEmpty();
    }


    [Fact]
    public async Task Defining_A_Command_As_A_Class_Record_Should_Not_Generate_Analyzer_Error()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;

                namespace Test;
            
                [Command]
                public class record CreateCustomerCommand { }

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

        analyzerOutput.Should().BeEmpty();
    }
}
