﻿using FluentAssertions;
using NForza.Cyrus.Generators.Analyzers;
using NForza.Cyrus.Generators.Tests.Infra;
using Xunit.Abstractions;

namespace NForza.Cyrus.Generators.Tests;

public class QueryHandlerTests(ITestOutputHelper outputWindow)
{
    [Fact]
    public async Task Generating_QueryHandler_Should_Compile_And_Generate_Sources()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;

                namespace Test;
            
                [Query]
                public record GetCustomerByIdQuery(Guid Id);

                public static class CustomerQuery
                {
                    [QueryHandler]
                    public static string Handle(GetCustomerByIdQuery query)
                    {
                        return query.Id.ToString();
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
        generatedSyntaxTrees.Should().Contain("ServiceCollectionJsonConverterExtensions: ICyrusInitializer");
        generatedSyntaxTrees.Should().Contain("Task<string> Query(this IQueryProcessor queryProcessor, global::Test.GetCustomerByIdQuery query");
        generatedSyntaxTrees.Should().Contain("QueryHandlersRegistration");
        generatedSyntaxTrees.Should().NotContainSource("AddTransient<global::Test.CustomerQuery>()");
    }

    [Fact]
    public async Task Generating_QueryHandler_For_Type_Which_Is_Not_A_Query_Should_Return_Error()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;

                namespace Test;
            
                public record GetCustomerByIdQuery(Guid Id);

                public static class CustomerQuery
                {
                    [QueryHandler]
                    public static string Handle(GetCustomerByIdQuery query)
                    {
                        return query.Id.ToString();
                    }
                }
            ";

        (var compilerOutput, var analyzerOutput, var generatedSyntaxTrees) =
            await new CyrusGeneratorTestBuilder()
            .WithSource(source)
            .LogGeneratedSource(outputWindow.WriteLine)
            .RunAsync();

        compilerOutput.Should().NotHaveErrors();
        analyzerOutput.Should().ContainError(DiagnosticDescriptors.MissingQueryAttribute);
    }

    [Fact]
    public async Task Generating_QueryHandler_Without_A_Route_Should_Not_Generate_MapGet()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;

                namespace Test;
            
                [Query]
                public record GetCustomerByIdQuery(Guid Id);

                public static class CustomerQuery
                {
                    [QueryHandler]
                    public static string Handle(GetCustomerByIdQuery query)
                    {
                        return query.Id.ToString();
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
        generatedSyntaxTrees.Should().NotContainSource("MapGet");
    }

    [Fact]
    public async Task Generating_QueryHandler_With_A_Route_Should_Generate_MapGet()
    {
        var source = @"
                using System;       
                using NForza.Cyrus.Abstractions;

                namespace Test;
            
                [Query]
                public record GetCustomerByIdQuery(Guid Id);

                public static class CustomerQuery
                {
                    [QueryHandler(Route = ""/"")]
                    public static string Handle(GetCustomerByIdQuery query)
                    {
                        return query.Id.ToString();
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
        generatedSyntaxTrees.Should().Contain("MapGet");
    }

}
