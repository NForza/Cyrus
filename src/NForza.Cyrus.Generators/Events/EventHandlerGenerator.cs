using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Events;

public class EventHandlerGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var eventHandlers = cyrusGenerationContext.EventHandlers;
        if (eventHandlers.Any())
        {
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            var eventHandlerRegistrations = string.Join(Environment.NewLine, eventHandlers
                .Select(x => x.ContainingType)
                .Where(x => x != null)
                .Where(x => !x.IsStatic)
                .Distinct(SymbolEqualityComparer.Default)
                .Select(eht => $" services.AddTransient<{eht.ToFullName()}>();"));
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
            var ctx = new
            {
                Usings = new string[] { "NForza.Cyrus.Cqrs", "NForza.Cyrus.Abstractions" },
                Namespace = "EventHandlers",
                Name = "EventHandlersRegistration",
                Initializer = eventHandlerRegistrations
            };
            var fileContents = cyrusGenerationContext.LiquidEngine.Render(ctx, "CyrusInitializer");
            spc.AddSource("EventHandlerRegistration.g.cs", fileContents);
        }
    }
}