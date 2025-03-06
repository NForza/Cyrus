using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.WebApi;

[Generator]
public class EventHandlerDictionaryGenerator : CyrusGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(true);

        var configProvider = ConfigProvider(context);

        var compilationProvider = context.CompilationProvider;

        var eventHandlerProvider = compilationProvider
            .SelectMany((compilation, _) =>
            {
                var allTypes = compilation.GetAllTypesFromCyrusAssemblies();

                var eventHandlers = allTypes
                    .SelectMany(t => t.GetMembers().OfType<IMethodSymbol>())
                    .Where(t => t.IsEventHandler())
                    .ToList();

                return eventHandlers;
            })
           .Collect();

        var combinedProvider = context
            .CompilationProvider
            .Combine(eventHandlerProvider)
            .Combine(configProvider);

        context.RegisterSourceOutput(combinedProvider, (spc, source) =>
        {
            var ((compilation, eventHandlers), config) = source;

            if (config != null && config.GenerationTarget.Contains(GenerationTarget.WebApi))
            {
                var contents = CreateEventHandlerRegistrations(eventHandlers, compilation);

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
        });
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
                    IsStatic = eventHandler.IsStatic,
                    MethodName = methodName,
                    TypeSymbolName = typeSymbolName
                };
            })
        };

        return LiquidEngine.Render(model, "EventHandlerDictionary");
    }
}