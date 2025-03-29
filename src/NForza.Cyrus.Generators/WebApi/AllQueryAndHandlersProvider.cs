using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.WebApi;

public class AllQueryAndHandlersProvider : CyrusProviderBase<ImmutableArray<ISymbol>>
{
    public override IncrementalValueProvider<ImmutableArray<ISymbol>> GetProvider(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<GenerationConfig> configProvider)
    {
        var compilationProvider = context.CompilationProvider;

        var queriesAndQueryHandlerProvider = compilationProvider
            .SelectMany((compilation, _) =>
            {
                var allTypes = compilation.GetAllTypesFromCyrusAssemblies();

                var queries = allTypes
                    .Where(t => t.IsQuery())
                    .ToList();

                var queryHandlers = allTypes
                    .SelectMany(t => t.GetMembers().OfType<IMethodSymbol>()
                    .Where(m => m.IsQueryHandler()))
                    .ToList();

                return queries.Cast<ISymbol>().Concat(queryHandlers);
            })
           .Collect();

        return queriesAndQueryHandlerProvider;
    }
}
