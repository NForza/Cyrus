using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Cqrs;

[Generator]
public class CqrsCommandGenerator : CyrusGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);

        var configProvider = ConfigProvider(context);

        var commandHandlerProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => CouldBeCommandHandler(syntaxNode),
                transform: (context, _) => GetMethodSymbolFromContext(context));

        var recordStructsWithAttribute = commandHandlerProvider.Combine(configProvider)
            .Where(x =>
            {
                var (methodNode, config) = x;
                if (config == null || !config.GenerationTarget.Contains(GenerationTarget.Domain))
                    return false;
                return IsCommandHandler(methodNode, config.Commands.HandlerName, config.Commands.Suffix);
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
            var ((compilation, recordSymbols), config) = source;

            if (config != null && recordSymbols.Any())
            {
                var sourceText = GenerateCommandDispatcherExtensionMethods(compilation, recordSymbols);
                if (!string.IsNullOrEmpty(sourceText))
                {
                    spc.AddSource($"CommandDispatcher.g.cs", SourceText.From(sourceText, Encoding.UTF8));
                }

                string assemblyName = recordSymbols.First().ContainingAssembly.Name;
                var commandModels = GetPartialModelClass(assemblyName, "Commands", "ModelTypeDefinition", recordSymbols.Select(cm => ModelGenerator.ForNamedType((INamedTypeSymbol)cm.Parameters[0].Type, compilation)));
                spc.AddSource($"model-commands.g.cs", SourceText.From(commandModels, Encoding.UTF8));
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

        var resolvedSource = ScribanEngine.Render("CommandDispatcherExtensions", model);

        return resolvedSource.ToString();
    }
}