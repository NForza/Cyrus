using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Model;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Cqrs;

[Generator]
public class CqrsCommandGenerator : CyrusGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(true);

        var commandHandlerProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => syntaxNode.IsCommandHandler(),
                transform: (context, _) => context.GetMethodSymbolFromContext());

        var configProvider = ConfigProvider(context);
        var recordStructsWithAttribute = commandHandlerProvider.Combine(configProvider)
            .Where(x =>
            {
                var (methodNode, config) = x;
                if (config == null || !config.GenerationTarget.Contains(GenerationTarget.Domain))
                    return false;
                return true;
            })
            .Where(x => x.Left != null)
            .Select((x, _) => x.Left!)
            .Collect();

        var combinedProvider = context.CompilationProvider
            .Combine(recordStructsWithAttribute);

        var third = combinedProvider
            .Combine(configProvider);

        context.RegisterSourceOutput(third, (spc, source) =>
        {
            var ((compilation, commandHandlerSymbols), config) = source;

            if (config != null && commandHandlerSymbols.Any())
            {
                var sourceText = GenerateCommandDispatcherExtensionMethods(compilation, commandHandlerSymbols);
                if (!string.IsNullOrEmpty(sourceText))
                {
                    spc.AddSource($"CommandDispatcher.g.cs", SourceText.From(sourceText, Encoding.UTF8));
                }

                string assemblyName = commandHandlerSymbols.First().ContainingAssembly.Name;
                var commandSymbols = commandHandlerSymbols.Select(ch => (INamedTypeSymbol)ch.Parameters[0].Type);

                var commandModels = GetPartialModelClass(assemblyName, "Commands", "ModelTypeDefinition", commandSymbols.Select(cm => ModelGenerator.ForNamedType(cm, LiquidEngine)));
                spc.AddSource($"model-commands.g.cs", SourceText.From(commandModels, Encoding.UTF8));

                var referencedTypes = commandSymbols.SelectMany(cs => cs.GetReferencedTypes());
                var referencedTypeModels = GetPartialModelClass(assemblyName, "Models", "ModelTypeDefinition", referencedTypes.Select(cm => ModelGenerator.ForNamedType(cm, LiquidEngine)));
                spc.AddSource($"model-command-types.g.cs", SourceText.From(referencedTypeModels, Encoding.UTF8));
            }
        });
    }

    private string GenerateCommandDispatcherExtensionMethods(Compilation compilation, ImmutableArray<IMethodSymbol> handlers)
    {
        INamedTypeSymbol? taskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1")!;
        INamedTypeSymbol? commandResultSymbol = compilation.GetTypeByMetadataName("NForza.Cyrus.Cqrs.CommandResult");

        if (commandResultSymbol == null || taskSymbol == null)
            return string.Empty;

        var model = new
        {
            Types = handlers.Select(h => h.Parameters[0].Type.ToFullName()).ToList()
        };

        var resolvedSource = LiquidEngine.Render(model, "CommandDispatcherExtensions");

        return resolvedSource.ToString();
    }
}