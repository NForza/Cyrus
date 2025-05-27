using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Queries;

public class QueryHandlerProvider : CyrusProviderBase<ImmutableArray<IMethodSymbol>>
{
    public override IncrementalValueProvider<ImmutableArray<IMethodSymbol>> GetProvider(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<GenerationConfig> configProvider)
    {
        var incrementalValuesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => syntaxNode.IsQueryHandler(),
                transform: (context, _) => context.GetMethodSymbolFromContext());

        var allQueryHandlersProvider = incrementalValuesProvider
            .Where(x => x is not null)
            .Select((x, _) => x!)
            .Collect();

        return allQueryHandlersProvider;
    }
}
