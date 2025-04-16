using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;

namespace NForza.Cyrus.Generators.Tests
{
    public class GeneratorTest
    {
        [Fact]
        public void Generating_GuidId_Should_Compile_And_Generate_Partial_Record_Struct()
        {
            var source = @"
                using NForza.Cyrus.Abstractions;

                namespace Demo;
            
                [GuidId]
                public partial record struct CustomerId; 
            ";

            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            var referencedAssemblies = new string[]
            {
                typeof(IServiceCollection).Assembly.Location
            };

            var trustedAssemblies = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
                .Split(Path.PathSeparator)
                .Concat(referencedAssemblies)
                .Distinct()
                .Select(path => MetadataReference.CreateFromFile(path))
                .ToList();

            var compilation = CSharpCompilation.Create(
                "TestAssembly",
                [syntaxTree],
                trustedAssemblies,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            IIncrementalGenerator generator = new CyrusGenerator(); 

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

            var compileDiagnostics = outputCompilation.GetDiagnostics();
            var errors = compileDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
            errors.Should().HaveCount(0);

            var generatedCode = outputCompilation.SyntaxTrees
                .Skip(1) 
                .Select(tree => tree.ToString())
                .FirstOrDefault();

            Assert.NotNull(generatedCode);
            generatedCode.Should().Contain("partial record struct CustomerId(Guid Value): IGuidId, IComparable<CustomerId>, IComparable");
        }
    }
}