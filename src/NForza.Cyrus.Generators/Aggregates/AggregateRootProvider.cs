using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Aggregates;

internal class AggregateRootProvider : CyrusProviderBase<ImmutableArray<AggregateRootDefinition>>
{
    public override IncrementalValueProvider<ImmutableArray<AggregateRootDefinition>> GetProvider(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<GenerationConfig> configProvider)
    {
        var provider = context.SyntaxProvider
                    .CreateSyntaxProvider(
                        predicate: (syntaxNode, _) => syntaxNode.IsAggregateRoot(),
                        transform: (context, _) => 
                            {
                                var classSymbol = context.GetClassSymbolFromContext();
                                var aggregateRootIdProperty = classSymbol?.GetAggregateRootIdProperty();
                                return new AggregateRootDefinition(classSymbol, aggregateRootIdProperty);
                            }
                        );

        var aggregateRootProvider = provider
            .Where(x => x != null && x.Symbol != null)
            .Select((x, _) => x!)
            .Collect();

        return aggregateRootProvider;
    }
}

public class AggregateRootDefinition(INamedTypeSymbol symbol, IPropertySymbol? aggregateRootIdProperty)
{
    public INamedTypeSymbol Symbol { get; } = symbol;
    public IPropertySymbol? AggregateRootProperty { get; } = aggregateRootIdProperty;
}
