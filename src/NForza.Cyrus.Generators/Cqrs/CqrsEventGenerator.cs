using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Model;

namespace NForza.Cyrus.Generators.Cqrs;

[Generator]
public class CqrsEventGenerator : CyrusGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);
        var configProvider = ConfigProvider(context);

        var incrementalValuesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => IsEvent(syntaxNode),
                transform: (context, _) => GetRecordSymbolFromContext(context));

        var allEventsProvider = incrementalValuesProvider.Combine(configProvider)
            .Where(x => x.Left is not null)
            .Select((x, _) => x.Left!)
            .Collect();

        var combinedProvider = allEventsProvider.Combine(context.CompilationProvider);

        context.RegisterSourceOutput(combinedProvider, (spc, eventsWithCompilation) =>
        {
            var (events, compilation) = eventsWithCompilation;
            if (events.Any())
            {
                string assemblyName = events.First().ContainingAssembly.Name;
                var eventModels = GetPartialModelClass(
                    assemblyName,
                    "Events",
                    "ModelTypeDefinition",
                    events.Select(e => ModelGenerator.ForNamedType(e, LiquidEngine)));
                spc.AddSource($"model-events.g.cs", SourceText.From(eventModels, Encoding.UTF8));
            }
        });
    }
}