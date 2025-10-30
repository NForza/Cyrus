using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using Cyrus;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Abstractions;
using NForza.Cyrus.Abstractions.Model;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Generators.SignalR;

namespace NForza.Cyrus.Generators.Model;

public class ModelGenerator : CyrusGeneratorBase
{
    public static JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = false };

    public override void GenerateSource(SourceProductionContext context, CyrusGenerationContext cyrusGenerationContext)
    {
        var model = new CyrusMetadata
        {
            Commands = GetModelTypeDefinitions(cyrusGenerationContext.All.CommandHandlers),
            Integers = GetInts(cyrusGenerationContext.All.IntValues),
            Doubles = GetDoubles(cyrusGenerationContext.All.DoubleValues),
            Guids = GetGuids(cyrusGenerationContext.All.GuidValues),
            Strings = GetStrings(cyrusGenerationContext.All.StringValues),
            Events = cyrusGenerationContext.All.Events.Select(e => e.GetModelTypeDefinition()),
            Queries = GetModelQueryDefinitions(cyrusGenerationContext.All.QueryHandlers),
            Endpoints = GetEndpoints(cyrusGenerationContext.All.QueryHandlers, cyrusGenerationContext.All.CommandHandlers),
            Hubs = GetHubs(cyrusGenerationContext.SignalRHubs),
            Models = GetModelTypeDefinitions(cyrusGenerationContext.All)
        };
        var modelJson = JsonSerializer.Serialize(model, ModelSerializerOptions.Default);
        var modelAttribute = new
        {
            Key = "cyrus-model",
            Value = modelJson.CompressToBase64()
        };
        var source = cyrusGenerationContext.LiquidEngine.Render(modelAttribute, "ModelAttribute");
        context.AddSource("cyrus-model.g.cs", source);
    }

    private IEnumerable<ModelTypeDefinition> GetModelTypeDefinitions(SolutionContext all)
    {
        var allModels = all.Commands
            .Concat(all.Events)
            .Concat(all.Queries)
            .Concat(all.ValueTypes);
        return allModels.Select(m => m.GetModelTypeDefinition());
    }

    private IEnumerable<ModelHubDefinition> GetHubs(ImmutableArray<SignalRHubClassDefinition> signalRHubs)
    {
        foreach (var signalRHub in signalRHubs)
        {
            var model = new
            {
                signalRHub.Name,
                signalRHub.Path,
                signalRHub.Commands,
                signalRHub.Queries,
                signalRHub.Events
            };

            yield return new ModelHubDefinition(model.Name, model.Path,
              model.Commands.Select(c => c.MethodName),
              model.Queries.Select(c => c.MethodName),
              model.Events.Select(e => e.Name));
        }
    }

    private IEnumerable<ModelEndpointDefinition> GetEndpoints(ImmutableArray<IMethodSymbol> queryHandlers, ImmutableArray<IMethodSymbol> commandHandlers)
    {
        var queryEndpoints = queryHandlers
            .Select(nts => (Type: (INamedTypeSymbol)nts.Parameters[0].Type, Verb: HttpVerb.Get, Route: nts.GetQueryRoute()))
            .Select(em => new ModelEndpointDefinition(em.Verb, em.Route, string.Empty, em.Type.Name));

        var commandEndpoints = commandHandlers
            .Select(nts => (Type: (INamedTypeSymbol)nts.Parameters[0].Type, Verb: (HttpVerb)Enum.Parse(typeof(HttpVerb), nts.GetCommandVerb()), Route: nts.GetCommandRoute()))
            .Select(em => new ModelEndpointDefinition(em.Verb, em.Route, em.Type.Name, ""));

        return queryEndpoints.Concat(commandEndpoints);
    }

    private IEnumerable<ModelQueryDefinition> GetModelQueryDefinitions(ImmutableArray<IMethodSymbol> queryHandlers)
    {
        return [.. queryHandlers.Select(c => new ModelQueryDefinition(c.GetQueryType().GetModelTypeDefinition(), c.GetQueryReturnType().GetModelTypeDefinition()))];
    }

    private IEnumerable<string> GetStrings(ImmutableArray<INamedTypeSymbol> stringValues) => stringValues.Select(s => s.Name);

    private IEnumerable<string> GetGuids(ImmutableArray<INamedTypeSymbol> guidValues) => guidValues.Select(g => g.Name);

    private IEnumerable<string> GetDoubles(ImmutableArray<INamedTypeSymbol> doubleValues) => doubleValues.Select(s => s.Name);

    private IEnumerable<string> GetInts(ImmutableArray<INamedTypeSymbol> intValues) => intValues.Select(i => i.Name);

    private static IEnumerable<ModelTypeDefinition> GetModelTypeDefinitions(ImmutableArray<IMethodSymbol> methodSymbols)
    {
        return [.. methodSymbols.Select(c => c.GetCommandType().GetModelTypeDefinition())];
    }
}