﻿using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Model;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Events;

public class EventGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var events = cyrusGenerationContext.Events;
        if (events.Any())
        {
            string assemblyName = events.First().ContainingAssembly.Name;
            var eventModels = GetPartialModelClass(
                assemblyName,
                "Events",
                "Events",
                "ModelTypeDefinition",
                events.Select(e => ModelGenerator.ForNamedType(e, cyrusGenerationContext.LiquidEngine)), cyrusGenerationContext.LiquidEngine);
            spc.AddSource($"model-events.g.cs", eventModels);

            var referencedTypes = events.SelectMany(cs => cs.GetReferencedTypes());
            var referencedTypeModels = GetPartialModelClass(assemblyName, "Events", "Models", "ModelTypeDefinition", referencedTypes.Select(cm => ModelGenerator.ForNamedType(cm, cyrusGenerationContext.LiquidEngine)), cyrusGenerationContext.LiquidEngine);
            spc.AddSource($"model-event-types.g.cs", referencedTypeModels);
        }
    }
}