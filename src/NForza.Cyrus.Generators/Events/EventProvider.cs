using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Events;

public class EventProvider : CyrusProviderBase<ImmutableArray<INamedTypeSymbol>>
{
    public override IncrementalValueProvider<ImmutableArray<INamedTypeSymbol>> GetProvider(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<GenerationConfig> configProvider)
    {
        var incrementalValuesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => syntaxNode.IsEvent(),
                transform: (context, _) => context.GetNamedTypeSymbolFromContext());

        var allEventsProvider = incrementalValuesProvider.Combine(configProvider)
            .Where(x => x.Left is not null)
            .Select((x, _) => x.Left!)
            .Collect();

        return allEventsProvider;
    }
}
