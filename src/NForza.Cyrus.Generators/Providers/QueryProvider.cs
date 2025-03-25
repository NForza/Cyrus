﻿using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Generators.Cqrs;

public class QueryProvider : CyrusProviderBase<ImmutableArray<INamedTypeSymbol>>
{
    public override IncrementalValueProvider<ImmutableArray<INamedTypeSymbol>> GetProvider(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<GenerationConfig> configProvider)
    {
        var queryProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => syntaxNode.IsQuery(),
                transform: (context, _) => context.GetRecordSymbolFromContext());

        var allQueriesProvider = queryProvider
            .Where(x => x is not null)
            .Select((x, _) => x!)
            .Collect();
        return allQueriesProvider;
    }
}
