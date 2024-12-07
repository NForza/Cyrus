using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Cqrs.Generator.Config;
using NForza.Cyrus.Generators.Cqrs;
using NForza.Generators;

namespace NForza.Cyrus.Cqrs.Generator;

[Generator]
public class CqrsCommandDispatcherGenerator : GeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(true);

        var configProvider = ConfigFileProvider(context);

        var commandHandlerProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => CouldBeCommandHandler(syntaxNode),
                transform: (context, _) => GetMethodSymbolFromContext(context));

        var recordStructsWithAttribute = commandHandlerProvider.Combine(configProvider)
            .Where(x => {
                var (methodNode, config) = x;
                if (!config.GenerationType.Contains("domain"))
                    return false;
                return config != null && IsCommandHandler(methodNode, config.Commands.HandlerName, config.Commands.Suffix);
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

            if (config != null && recordSymbols.Any())
            {
                var sourceText = GenerateCommandDispatcherExtensionMethods(compilation, recordSymbols);
                if (!string.IsNullOrEmpty(sourceText))
                {
                    spc.AddSource($"CommandDispatcher.g.cs", SourceText.From(sourceText, Encoding.UTF8));
                }
            }
        });
    }



    private string GenerateCommandDispatcherExtensionMethods(Compilation compilation, ImmutableArray<IMethodSymbol> handlers)
    {
        INamedTypeSymbol? taskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1")!;
        INamedTypeSymbol? commandResultSymbol = compilation.GetTypeByMetadataName("NForza.Cyrus.Cqrs.CommandResult");

        if (commandResultSymbol == null || taskSymbol == null)
            return string.Empty;

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