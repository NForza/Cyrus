using System.Linq;
using Microsoft.CodeAnalysis;
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
        }
    }
}