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
        var compilationProvider = context.CompilationProvider;

        var aggregateRootsFromReferencedAssembliesProvider = compilationProvider
            .SelectMany((compilation, _) =>
            {
                var typesFromAssemblies = compilation.GetAllTypesFromCyrusAssemblies();

                var aggregateFromAssemblies = typesFromAssemblies
                    .Where(t => t.IsAggregateRoot())
                    .ToList();
                return aggregateFromAssemblies
                    .Select(t => new AggregateRootDefinition(t, t.GetAggregateRootIdProperty()));
            })
           .Collect();

        return aggregateRootsFromReferencedAssembliesProvider;
    }
}

public class AggregateRootDefinition(INamedTypeSymbol? symbol, IPropertySymbol? aggregateRootIdProperty)
{
    public INamedTypeSymbol? Symbol { get; } = symbol;
    public IPropertySymbol? AggregateRootProperty { get; } = aggregateRootIdProperty;
}
