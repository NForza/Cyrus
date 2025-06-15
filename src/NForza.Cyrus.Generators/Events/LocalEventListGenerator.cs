using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.Events;

public class LocalEventListGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var config = cyrusGenerationContext.GenerationConfig;
        if (config.GenerationTarget.Contains(GenerationTarget.WebApi))
        {
            var contents = CreateEventHandlerRegistrations(cyrusGenerationContext.AllEvents, cyrusGenerationContext.LiquidEngine);

            if (!string.IsNullOrWhiteSpace(contents))
            {
                var ctx = new
                {
                    Usings = new string[] { "NForza.Cyrus.Cqrs", "NForza.Cyrus.Abstractions" },
                    Namespace = "LocalEvents",
                    Name = "LocalEventListInitializer",
                    Initializer = contents
                };

                var fileContents = cyrusGenerationContext.LiquidEngine.Render(ctx, "CyrusInitializer");
                spc.AddSource("LocalEventList.g.cs", fileContents);
            }
        }
    }

    private string CreateEventHandlerRegistrations(IEnumerable<INamedTypeSymbol> events, LiquidEngine liquidEngine)
    {
        var model = new
        {
            LocalEvents = events.Where(e => e.IsLocalEvent())
                .Select(e => e.ToFullName())
                .ToList()
        };

        return liquidEngine.Render(model, "LocalEventList");
    }
}
