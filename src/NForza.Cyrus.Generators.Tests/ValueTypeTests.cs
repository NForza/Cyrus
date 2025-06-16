using FluentAssertions;
using NForza.Cyrus.Generators.Analyzers;
using NForza.Cyrus.Generators.Tests.Infra;
using Xunit.Abstractions;

namespace NForza.Cyrus.Generators.Tests;

public class ValueTypeTests(ITestOutputHelper outputWindow)
{
    [Fact]
    public async Task Generating_GuidValue_Should_Compile_And_Generate_Partial_Record_Struct_And_Converters()
    {
        var source = @"
                using NForza.Cyrus.Abstractions;

                namespace Test;
            
                [GuidValue]
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
        generatedSyntaxTrees.Should().ContainSource("partial record struct CustomerId(Guid Value): IGuidValueType, IComparable<CustomerId>, IComparable");
        generatedSyntaxTrees.Should().ContainSource("JsonConverter<CustomerId>");
        generatedSyntaxTrees.Should().ContainSource("CustomerIdTypeConverter");
    }

    [Fact]
    public async Task Generating_GuidValue_For_Struct_Which_Is_Not_A_Record_Should_Not_Generate_Analyzer_Error()
    {
        var source = @"
                using NForza.Cyrus.Abstractions;

                namespace Test;
            
                [GuidValue]
                public partial struct CustomerId; 
            ";

        (var compilerOutput, var analyzerOutput, var generatedSyntaxTrees) =
            await new CyrusGeneratorTestBuilder()
            .WithSource(source)
            .LogGeneratedSource(outputWindow.WriteLine)
            .RunAsync();

        analyzerOutput.Should().HaveErrorCount(1);
        analyzerOutput.Should().ContainError(DiagnosticDescriptors.ValueTypeMustBeARecordStruct);
    }

    [Fact]
    public async Task Generating_GuidValue_For_Struct_In_Global_Namespace_Should_Not_Generate_Compiler_Error()
    {
        var source = @"
                using NForza.Cyrus.Abstractions;

                [GuidValue]
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
    public async Task Generating_StringValue_For_Struct_In_Global_Namespace_Should_Not_Generate_Compiler_Error()
    {
        var source = @"
                using NForza.Cyrus.Abstractions;

                [StringValue]
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
    public async Task Generating_IntValue_For_Struct_In_Global_Namespace_Should_Not_Generate_Compiler_Error()
    {
        var source = @"
                using NForza.Cyrus.Abstractions;

                [IntValue]
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