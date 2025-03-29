using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.WebApi;

public class EventHandlerDictionaryGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider, LiquidEngine liquidEngine)
    {
        var config = cyrusProvider.GenerationConfig;
        if (config.GenerationTarget.Contains(GenerationTarget.WebApi))
        {
            var contents = CreateEventHandlerRegistrations(cyrusProvider.EventHandlers, cyrusProvider.Compilation);

            if (!string.IsNullOrWhiteSpace(contents))
            {
                var ctx = new
                {
                    Usings = new string[] { "NForza.Cyrus.Cqrs" },
                    Namespace = "EventHandlers",
                    Name = "EventHandlersDictionary",
                    Initializer = contents
                };

                var fileContents = LiquidEngine.Render(ctx, "CyrusInitializer");
                spc.AddSource(
                   "EventHandlerDictionary.g.cs",
                   SourceText.From(fileContents, Encoding.UTF8));
            }
        }
    }

    private string CreateEventHandlerRegistrations(IEnumerable<IMethodSymbol> eventHandlers, Compilation compilation)
    {
        INamedTypeSymbol? taskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        if (taskSymbol == null)
        {
            return string.Empty;
        }

        StringBuilder source = new();

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
                var isAsync = returnType.OriginalDefinition.Equals(taskSymbol, SymbolEqualityComparer.Default);
                return new
                {
                    EventType = eventType,
                    HandlerName = handlerName,
                    eventHandler.IsStatic,
                    MethodName = methodName,
                    TypeSymbolName = typeSymbolName
                };
            })
        };

        return LiquidEngine.Render(model, "EventHandlerDictionary");
    }
}