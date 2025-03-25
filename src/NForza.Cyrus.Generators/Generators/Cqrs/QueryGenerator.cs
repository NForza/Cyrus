﻿using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Generators.Model;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.Generators.Cqrs;

public class QueryGenerator : CyrusGeneratorBase<ImmutableArray<INamedTypeSymbol>>
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

    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider, LiquidEngine liquidEngine)
    {
        var queries = cyrusProvider.Queries;
        if (queries.Any())
        {
            string assemblyName = queries.First().ContainingAssembly.Name;
            var eventModels = GetPartialModelClass(
                assemblyName,
                "Queries",
                "Queries",
                "ModelTypeDefinition",
                queries.Select(e => ModelGenerator.ForNamedType(e, LiquidEngine)));
            spc.AddSource($"model-queries.g.cs", SourceText.From(eventModels, Encoding.UTF8));

            var referencedTypes = queries.SelectMany(cs => cs.GetReferencedTypes());
            var referencedTypeModels = GetPartialModelClass(assemblyName, "Queries", "Models", "ModelTypeDefinition", referencedTypes.Select(cm => ModelGenerator.ForNamedType(cm, LiquidEngine)));
            spc.AddSource($"model-event-types.g.cs", SourceText.From(referencedTypeModels, Encoding.UTF8));
        }
    }
}