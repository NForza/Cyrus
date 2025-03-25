using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Generators.Model;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.Generators.Cqrs;

public class EventGenerator : CyrusGeneratorBase<ImmutableArray<INamedTypeSymbol>>
{
    public override IncrementalValueProvider<ImmutableArray<INamedTypeSymbol>> GetProvider(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<GenerationConfig> configProvider)
    {
        var incrementalValuesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => syntaxNode.IsEvent(),
                transform: (context, _) => context.GetRecordSymbolFromContext());

        var allEventsProvider = incrementalValuesProvider.Combine(configProvider)
            .Where(x => x.Left is not null)
            .Select((x, _) => x.Left!)
            .Collect();

        return allEventsProvider;
    }

    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider, LiquidEngine liquidEngine)
    {
        var events = cyrusProvider.Events;
        if (events.Any())
        {
            string assemblyName = events.First().ContainingAssembly.Name;
            var eventModels = GetPartialModelClass(
                assemblyName,
                "Events",
                "Events",
                "ModelTypeDefinition",
                events.Select(e => ModelGenerator.ForNamedType(e, LiquidEngine)));
            spc.AddSource($"model-events.g.cs", SourceText.From(eventModels, Encoding.UTF8));

            var referencedTypes = events.SelectMany(cs => cs.GetReferencedTypes());
            var referencedTypeModels = GetPartialModelClass(assemblyName, "Events", "Models", "ModelTypeDefinition", referencedTypes.Select(cm => ModelGenerator.ForNamedType(cm, LiquidEngine)));
            spc.AddSource($"model-event-types.g.cs", SourceText.From(referencedTypeModels, Encoding.UTF8));
        }
    }
}