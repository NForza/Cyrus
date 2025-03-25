using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Generators.MassTransit;

[Generator]
public class MassTransitConsumerGenerator : CyrusSourceGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);
        var configProvider = ConfigProvider(context);

        var eventHandlersProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => syntaxNode.IsEventHandler(),
                transform: (context, _) => context.GetMethodSymbolFromContext());

        var eentHandlersWithConfigProvider = eventHandlersProvider.Combine(configProvider)
            .Select((x, _) => x.Left!)
            .Collect();

        var combinedProvider = eentHandlersWithConfigProvider.Combine(context.CompilationProvider).Combine(configProvider);

        context.RegisterSourceOutput(combinedProvider, (spc, eventHandlersWithCompilation) =>
        {
            var ((queryHandlers, compilation), config) = eventHandlersWithCompilation;
            if (queryHandlers.Any())
            {
                if (config.EventBus == "MassTransit")
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

        var resolvedSource = LiquidEngine.Render(model, "EventConsumers");

        return resolvedSource;
    }
}