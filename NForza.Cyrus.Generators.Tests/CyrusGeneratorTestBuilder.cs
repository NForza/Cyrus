using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using NForza.Cyrus.Generators.Analyzers;

namespace NForza.Cyrus.Generators.Tests
{
    internal class CyrusGeneratorTestBuilder
    {
        private string? source;

        public async Task<(ImmutableArray<Diagnostic> compilerOutput, ImmutableArray<Diagnostic> analyzerOutput, IEnumerable<SyntaxTree> generatedSource)> RunAsync()
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new InvalidOperationException("Source code is not set.");
            }

            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            var trustedAssemblies = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
                .Split(Path.PathSeparator)
                .Concat([typeof(IServiceCollection).Assembly.Location])
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

            GeneratorDriver driver = CSharpGeneratorDriver.Create([generator]);

            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            var compilationWithAnalyzers = outputCompilation.WithAnalyzers([new CyrusAnalyzer()]);

            var compileDiagnostics = outputCompilation.GetDiagnostics();
            var analyzerDiagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

            var generatedSyntaxTrees = outputCompilation.SyntaxTrees
                .Skip(1)
                .ToList();

            return (compileDiagnostics, analyzerDiagnostics, generatedSyntaxTrees);
        }

        internal CyrusGeneratorTestBuilder WithSource(string source)
        {
            this.source = source;
            return this;
        }
    }
}