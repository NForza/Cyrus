using FluentAssertions;
using NForza.Cyrus.Generators.Analyzers;
using NForza.Cyrus.Generators.Tests.Infra;
using Xunit.Abstractions;

namespace NForza.Cyrus.Generators.Tests;

public class TypedIdTests(ITestOutputHelper outputWindow)
{
    [Fact]
    public async Task Generating_GuidId_Should_Compile_And_Generate_Partial_Record_Struct_And_Converters()
    {
        var source = @"
                using NForza.Cyrus.Abstractions;

                namespace Test;
            
                [GuidId]
                public partial record struct CustomerId; 
            ";

        (var compilerOutput, var analyzerOutput, var generatedSyntaxTrees) = 
            await new CyrusGeneratorTestBuilder()
            .WithSource(source)
            .LogGeneratedSource(outputWindow.WriteLine)
            .RunAsync();

        compilerOutput.Should().NotHaveErrors();
        analyzerOutput.Should().BeEmpty();

        generatedSyntaxTrees.Should().NotBeEmpty();
        generatedSyntaxTrees.Should().ContainSource("partial record struct CustomerId(Guid Value): IGuidId, IComparable<CustomerId>, IComparable");
        generatedSyntaxTrees.Should().ContainSource("JsonConverter<CustomerId>");
        generatedSyntaxTrees.Should().ContainSource("CustomerIdTypeConverter");
    }

    [Fact]
    public async Task Generating_GuidId_For_Struct_Which_Is_Not_A_Record_Should_Not_Generate_Analyzer_Error()
    {
        var source = @"
                using NForza.Cyrus.Abstractions;

                namespace Test;
            
                [GuidId]
                public partial struct CustomerId; 
            ";

        (var compilerOutput, var analyzerOutput, var generatedSyntaxTrees) =
            await new CyrusGeneratorTestBuilder()
            .WithSource(source)
            .LogGeneratedSource(outputWindow.WriteLine)
            .RunAsync();

        analyzerOutput.Should().HaveErrorCount(1);
        analyzerOutput.Should().ContainError(DiagnosticDescriptors.TypedIdMustBeARecordStruct);
    }

    [Fact]
    public async Task Generating_GuidId_For_Struct_In_Global_Namespace_Should_Not_Generate_Compiler_Error()
    {
        var source = @"
                using NForza.Cyrus.Abstractions;

                [GuidId]
                public partial record struct CustomerId; 
            ";

        (var compilerOutput, var analyzerOutput, var generatedSyntaxTrees) =
            await new CyrusGeneratorTestBuilder()
            .WithSource(source)
            .LogGeneratedSource(outputWindow.WriteLine)
            .RunAsync();

        analyzerOutput.Should().NotHaveErrors();
        compilerOutput.Should().NotHaveErrors();
    }

    [Fact]
    public async Task Generating_StringId_For_Struct_In_Global_Namespace_Should_Not_Generate_Compiler_Error()
    {
        var source = @"
                using NForza.Cyrus.Abstractions;

                [StringId]
                public partial record struct Name; 
            ";

        (var compilerOutput, var analyzerOutput, var generatedSyntaxTrees) =
            await new CyrusGeneratorTestBuilder()
            .WithSource(source)
            .LogGeneratedSource(outputWindow.WriteLine)
            .RunAsync();

        analyzerOutput.Should().NotHaveErrors();
        compilerOutput.Should().NotHaveErrors();
    }

    [Fact]
    public async Task Generating_IntId_For_Struct_In_Global_Namespace_Should_Not_Generate_Compiler_Error()
    {
        var source = @"
                using NForza.Cyrus.Abstractions;

                [IntId]
                public partial record struct Amount; 
            ";

        (var compilerOutput, var analyzerOutput, var generatedSyntaxTrees) =
            await new CyrusGeneratorTestBuilder()
            .WithSource(source)
            .LogGeneratedSource(outputWindow.WriteLine)
            .RunAsync();

        analyzerOutput.Should().NotHaveErrors();
        compilerOutput.Should().NotHaveErrors();
    }
}