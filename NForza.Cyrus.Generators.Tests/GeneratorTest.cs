using FluentAssertions;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Tests;

public class GeneratorTest
{
    [Fact]
    public async Task Generating_GuidId_Should_Compile_And_Generate_Partial_Record_Struct()
    {
        var source = @"
                using NForza.Cyrus.Abstractions;

                namespace Demo;
            
                [GuidId]
                public partial record struct CustomerId; 
            ";

        (var compilerOutput, var analyzerOutput, var generatedCode) = await new CyrusGeneratorTestBuilder()
            .WithSource(source)
            .RunAsync();

        compilerOutput.Where(d => d.Severity == DiagnosticSeverity.Error).Should().HaveCount(0);
        analyzerOutput.Should().HaveCount(0);

        generatedCode.Should().NotBeNullOrWhiteSpace();
        generatedCode.Should().Contain("partial record struct CustomerId(Guid Value): IGuidId, IComparable<CustomerId>, IComparable");
    }

    [Fact]
    public async Task Generating_GuidId_For_Struct_Which_Is_Not_A_Record_Should_Not_Compile()
    {
        var source = @"
                using NForza.Cyrus.Abstractions;

                namespace Demo;
            
                [GuidId]
                public partial struct CustomerId; 
            ";

        (var compilerOutput, var analyzerOutput, var generatedCode) =await new CyrusGeneratorTestBuilder()
            .WithSource(source)
            .RunAsync();

        analyzerOutput.Where(d => d.Severity == DiagnosticSeverity.Error).Should().HaveCount(1);
        analyzerOutput.First().Descriptor.Should().Be(DiagnosticDescriptors.TypedIdMustBeARecordStruct);
    }
}