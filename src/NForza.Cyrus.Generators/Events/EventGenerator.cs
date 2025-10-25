using System.Linq;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Events;

public class EventGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var events = cyrusGenerationContext.Events;
        if (events.Any())
        {
            string assemblyName = events.First().ContainingAssembly.Name;
        }
    }
}