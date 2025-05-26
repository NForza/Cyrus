using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Aggregates;

internal class AggregateRootProvider : CyrusProviderBase<ImmutableArray<INamedTypeSymbol>>
{
    public override IncrementalValueProvider<ImmutableArray<INamedTypeSymbol>> GetProvider(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<GenerationConfig> configProvider)
    {
        var provider = context.SyntaxProvider
                    .CreateSyntaxProvider(
                        predicate: (syntaxNode, _) => syntaxNode.IsAggregateRoot(),
                        transform: (context, _) => context.GetNamedTypeSymbolFromContext());

        var aggregateRootProvider = provider.Combine(configProvider)
            .Where(x =>
            {
                var (methodNode, config) = x;
                if (config == null || !config.GenerationTarget.Contains(GenerationTarget.Domain))
                    return false;
                return true;
            })
            .Where(x => x.Left != null)
            .Select((x, _) => x.Left!)
            .Collect();

        return aggregateRootProvider;
    }
}