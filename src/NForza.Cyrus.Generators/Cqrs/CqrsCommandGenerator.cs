using System;
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
        DebugThisGenerator(false);

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
                var sourceText = GenerateCommandDispatcherExtensionMethods(commandHandlerSymbols, compilation);
                if (!string.IsNullOrEmpty(sourceText))
                {
                    spc.AddSource($"CommandDispatcher.g.cs", SourceText.From(sourceText, Encoding.UTF8));
                }

#pragma warning disable RS1035 // Do not use APIs banned for analyzers
                var commandHandlerRegistrations = string.Join(Environment.NewLine, 
                        commandHandlerSymbols
                            .Select(ch => ch.ContainingType)
                            .Where(x => x != null)
                            .Distinct(SymbolEqualityComparer.Default)
                            .Select(cht => $" services.AddTransient<{cht.ToFullName()}>();"));
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
                var ctx = new
                {
                    Usings = new string[] { "NForza.Cyrus.Cqrs" },
                    Namespace = "CommandHandlers",
                    Name = "CommandHandlersRegistration",
                    Initializer = commandHandlerRegistrations
                };

                var fileContents = LiquidEngine.Render(ctx, "CyrusInitializer");
                spc.AddSource("CommandHandlerRegistration.g.cs", SourceText.From(fileContents, Encoding.UTF8));

                string assemblyName = commandHandlerSymbols.First().ContainingAssembly.Name;
                var commandSymbols = commandHandlerSymbols.Select(ch => (INamedTypeSymbol)ch.Parameters[0].Type);

                var commandModels = GetPartialModelClass(assemblyName, "Command", "Commands", "ModelTypeDefinition", commandSymbols.Select(cm => ModelGenerator.ForNamedType(cm, LiquidEngine)));
                spc.AddSource($"model-commands.g.cs", SourceText.From(commandModels, Encoding.UTF8));

                var referencedTypes = commandSymbols.SelectMany(cs => cs.GetReferencedTypes());
                var referencedTypeModels = GetPartialModelClass(assemblyName, "Command", "Models", "ModelTypeDefinition", referencedTypes.Select(cm => ModelGenerator.ForNamedType(cm, LiquidEngine)));
                spc.AddSource($"model-command-types.g.cs", SourceText.From(referencedTypeModels, Encoding.UTF8));
            }
        });
    }

    private string GenerateCommandDispatcherExtensionMethods(ImmutableArray<IMethodSymbol> handlers, Compilation compilation)
    {
        INamedTypeSymbol? taskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        if (taskSymbol == null)
        {
            return string.Empty;
        }

        var commands = handlers.Select(h => new
        {
            Handler = h,
            CommandType = h.Parameters[0].Type.ToFullName(),
            Name = h.Name,
            ReturnsVoid = h.ReturnsVoid,
            ReturnType = (INamedTypeSymbol)h.ReturnType,
            IsAsync = h.ReturnType.OriginalDefinition.Equals(taskSymbol, SymbolEqualityComparer.Default)
        }).ToList();

        var model = new
        {
            Commands = commands.Select(q => new
            {
                ReturnTypeOriginal = q.ReturnType,
                ReturnType = q.IsAsync ? q.ReturnType.TypeArguments[0].ToFullName() : q.ReturnType.ToFullName(),
                Invocation = q.Handler.GetCommandInvocation(variableName: "command", serviceProviderVariable: "commandDispatcher.ServiceProvider"),
                Name = q.Name,
                q.ReturnsVoid,
                q.CommandType,
                q.IsAsync
            }).ToList()
        };

        var resolvedSource = LiquidEngine.Render(model, "CommandDispatcherExtensions");

        return resolvedSource;
    }
}