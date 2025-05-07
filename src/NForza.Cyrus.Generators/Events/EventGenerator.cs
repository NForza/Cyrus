using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Model;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Events;

public class EventGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider)
    {
        var events = cyrusProvider.Events;
        if (events.Any())
        {
            string assemblyName = events.First().ContainingAssembly.Name;
            var eventModels = GetPartialModelClass(
                assemblyName,
                "Events",
                "Events",
                "ModelTypeDefinition",
                events.Select(e => ModelGenerator.ForNamedType(e, cyrusProvider.LiquidEngine)), cyrusProvider.LiquidEngine);
            spc.AddSource($"model-events.g.cs", SourceText.From(eventModels, Encoding.UTF8));

            var referencedTypes = events.SelectMany(cs => cs.GetReferencedTypes());
            var referencedTypeModels = GetPartialModelClass(assemblyName, "Events", "Models", "ModelTypeDefinition", referencedTypes.Select(cm => ModelGenerator.ForNamedType(cm, cyrusProvider.LiquidEngine)), cyrusProvider.LiquidEngine);
            spc.AddSource($"model-event-types.g.cs", SourceText.From(referencedTypeModels, Encoding.UTF8));
        }
    }
}