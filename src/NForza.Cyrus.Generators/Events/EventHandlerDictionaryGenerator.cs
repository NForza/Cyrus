using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.Events;

public class EventHandlerDictionaryGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var config = cyrusGenerationContext.GenerationConfig;
        if (config.GenerationTarget.Contains(GenerationTarget.WebApi) || config.GenerationTarget.Contains(GenerationTarget.Domain))
        {
            var contents = CreateEventHandlerRegistrations(cyrusGenerationContext.EventHandlers, cyrusGenerationContext.Compilation, cyrusGenerationContext.LiquidEngine);

            if (!string.IsNullOrWhiteSpace(contents))
            {
                var ctx = new
                {
                    Usings = new string[] { "NForza.Cyrus.Cqrs" },
                    Namespace = "EventHandlers",
                    Name = "EventHandlersDictionary",
                    Initializer = contents
                };

                var fileContents = cyrusGenerationContext.LiquidEngine.Render(ctx, "CyrusInitializer");
                spc.AddSource("EventHandlerDictionary.g.cs", fileContents);
            }
        }
    }

    private string CreateEventHandlerRegistrations(IEnumerable<IMethodSymbol> eventHandlers, Compilation compilation, LiquidEngine liquidEngine)
    {
        INamedTypeSymbol? taskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        if (taskSymbol == null)
        {
            return string.Empty;
        }

        var model = new
        {
            EventHandlers = eventHandlers.Select(eventHandler =>
            {
                var eventType = eventHandler.Parameters[0].Type.ToFullName();
                var typeSymbol = eventHandler.ContainingType;
                var methodName = eventHandler.Name;
                var typeSymbolName = typeSymbol.ToFullName();
                var handlerName = $"{typeSymbol.Name}.{eventHandler.Name}({eventHandler.Parameters[0].Type.Name})";
                var returnType = (INamedTypeSymbol)eventHandler.ReturnType;
                var returnsTask = eventHandler.ReturnsTask();
                return new
                {
                    EventType = eventType,
                    HandlerName = handlerName,
                    eventHandler.IsStatic,
                    MethodName = methodName,
                    ReturnsTask = returnsTask,
                    TypeSymbolName = typeSymbolName
                };
            })
        };

        return liquidEngine.Render(model, "EventHandlerDictionary");
    }
}