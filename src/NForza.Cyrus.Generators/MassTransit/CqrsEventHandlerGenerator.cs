using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.MassTransit;

[Generator]
public class MassTransitConsumerGenerator : CyrusGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);
        var configProvider = ConfigProvider(context);

        var incrementalValuesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => CouldBeEventHandler(syntaxNode),
                transform: (context, _) => GetMethodSymbolFromContext(context));

        var allEventHandlersProvider = incrementalValuesProvider.Combine(configProvider)
            .Where(x =>
            {
                var (methodNode, config) = x;
                return IsEventHandler(methodNode, config.Events.HandlerName, config.Events.Suffix);
            })
            .Select((x, _) => x.Left!)
            .Collect();

        var combinedProvider = allEventHandlersProvider.Combine(context.CompilationProvider).Combine(configProvider);

        context.RegisterSourceOutput(combinedProvider, (spc, eventHandlersWithCompilation) =>
        {
            var ((queryHandlers, compilation), config) = eventHandlersWithCompilation;
            if (queryHandlers.Any())
            {
                if (config.Events.Bus == "MassTransit")
                {
                    var sourceText = GenerateEventConsumers(queryHandlers);
                    spc.AddSource($"EventConsumers.g.cs", SourceText.From(sourceText, Encoding.UTF8));
                }
            }
        });
    }

    private string GenerateEventConsumers(ImmutableArray<IMethodSymbol> handlers)
    {
        StringBuilder source = new();
        var model = new
        {
            Consumers = handlers.Select(h => new { h.Parameters[0].Type.Name, FullName = h.Parameters[0].Type.ToFullName() })
        };

        var resolvedSource = ScribanEngine.Render("EventConsumers", model);

        return resolvedSource;
    }
}