using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Cqrs.Generator.Config;
using NForza.Generators;

namespace NForza.Cyrus.Cqrs.Generator;

[Generator]
public class CqrsCommandDispatcherGenerator : CqrsSourceGenerator, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);
        var configProvider = ParseConfigFile<CqrsConfig>(context, "cqrsConfig.yaml");

        var incrementalValuesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => CouldBeCommandHandler(syntaxNode),
                transform: (context, _) => GetMethodSymbolFromContext(context));

        var recordStructsWithAttribute = incrementalValuesProvider.Combine(configProvider)
            .Where(x => {
                var (methodNode, config) = x;
                return IsCommandHandler(methodNode, config.Commands.HandlerName, config.Commands.Suffix);
             })
            .Select((x, _) => x.Left!)
            .Collect();

        var combinedProvider = context.CompilationProvider
            .Combine(recordStructsWithAttribute);

        var third = combinedProvider
            .Combine(configProvider);

        context.RegisterSourceOutput(third, (spc, source) =>
        {
            var ((compilation, recordSymbols), config) = source;
            var sourceText = GenerateCommandDispatcherExtensionMethods(compilation, recordSymbols);
                spc.AddSource($"CommandDispatcher.g.cs", SourceText.From(sourceText, Encoding.UTF8));
        });

        //IEnumerable<string> contractSuffix = configuration.Contracts;
        //var commandSuffix = configuration.Commands.Suffix;
        //var methodHandlerName = configuration.Commands.HandlerName;

       // GenerateCommandDispatcherExtensionMethods(context, handlers);
    }

    private string GenerateCommandDispatcherExtensionMethods(Compilation compilation, ImmutableArray<IMethodSymbol> handlers)
    {
        INamedTypeSymbol taskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        INamedTypeSymbol commandResultSymbol = compilation.GetTypeByMetadataName("NForza.Cyrus.Cqrs.CommandResult");
        var taskOfCommandResultSymbol = taskSymbol.Construct(commandResultSymbol);

        StringBuilder source = new();
        foreach (var handler in handlers)
        {
            var methodSymbol = handler;
            var parameterType = methodSymbol.Parameters[0].Type;
                source.Append($@"
    public static Task<CommandResult> Execute(this ICommandDispatcher dispatcher, {parameterType} command, CancellationToken cancellationToken = default) 
        => dispatcher.ExecuteInternalAsync(command, cancellationToken);");
        }

        var resolvedSource = TemplateEngine.ReplaceInResourceTemplate("CommandDispatcherExtensions.cs", new Dictionary<string, string>
        {
            ["ExecuteMethods"] = source.ToString()
        });

        return resolvedSource.ToString();
    }
}