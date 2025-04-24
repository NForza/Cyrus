using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.Events;

public class EventHandlerGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider, LiquidEngine liquidEngine)
    {
        var eventHandlers = cyrusProvider.EventHandlers;
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
                Usings = new string[] { "NForza.Cyrus.Cqrs" },
                Namespace = "EventHandlers",
                Name = "EventHandlersRegistration",
                Initializer = eventHandlerRegistrations
            };
            var fileContents = LiquidEngine.Render(ctx, "CyrusInitializer");
            spc.AddSource("EventHandlerRegistration.g.cs", SourceText.From(fileContents, Encoding.UTF8));
        }
    }
}