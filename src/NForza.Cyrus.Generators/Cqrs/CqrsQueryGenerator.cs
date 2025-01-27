using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace NForza.Cyrus.Generators.Cqrs;

[Generator]
public class CqrsQueryGenerator : CyrusGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);
        var configProvider = ConfigProvider(context);

        var incrementalValuesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => IsQuery(syntaxNode),
                transform: (context, _) => GetRecordSymbolFromContext(context));

        var allEventsProvider = incrementalValuesProvider.Combine(configProvider)
            .Where(x => x.Left is not null)
            .Select((x, _) => x.Left!)
            .Collect();

        var combinedProvider = allEventsProvider.Combine(context.CompilationProvider);

        context.RegisterSourceOutput(combinedProvider, (spc, eventsWithCompilation) =>
        {
            var (events, compilation) = eventsWithCompilation;
            if (events.Any())
            {
                string assemblyName = events.First().ContainingAssembly.Name;
                var eventModels = GetPartialModelClass(
                    assemblyName,
                    "Queries",
                    "ModelTypeDefinition",
                    events.Select(qh => ModelGenerator.ForNamedType(qh, compilation)));
                spc.AddSource($"model-queries.g.cs", SourceText.From(eventModels, Encoding.UTF8));
            }
        });
    }
}