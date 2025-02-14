using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Model;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Cqrs;

[Generator]
public class CqrsQueryGenerator : CyrusGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);

        var incrementalValuesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => syntaxNode.IsQuery(),
                transform: (context, _) => context.GetRecordSymbolFromContext());

        var allQueriesProvider = incrementalValuesProvider
            .Where(x => x is not null)
            .Select((x, _) => x!)
            .Collect();

        var combinedProvider = allQueriesProvider.Combine(context.CompilationProvider);

        context.RegisterSourceOutput(combinedProvider, (spc, eventsWithCompilation) =>
        {
            var (queries, compilation) = eventsWithCompilation;
            if (queries.Any())
            {
                string assemblyName = queries.First().ContainingAssembly.Name;
                var eventModels = GetPartialModelClass(
                    assemblyName,
                    "Queries",
                    "ModelTypeDefinition",
                    queries.Select(e => ModelGenerator.ForNamedType(e, LiquidEngine)));
                spc.AddSource($"model-queries.g.cs", SourceText.From(eventModels, Encoding.UTF8));
            }
        });
    }
}