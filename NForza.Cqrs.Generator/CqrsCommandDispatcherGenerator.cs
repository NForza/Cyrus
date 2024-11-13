using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NForza.Cqrs.Generator.Config;
using NForza.Generators;

#pragma warning disable RS1035 // Do not use banned APIs for analyzers

namespace NForza.Cqrs.Generator;

[Generator]
public class CqrsCommandDispatcherGenerator : CqrsSourceGenerator, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var configProvider = ParseConfigFile<CqrsConfig>(context, "cqrsConfig.yaml");

        var incrementalValuesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => IsCommandHandler(syntaxNode),
                transform: (context, _) => GetMethodSymbolFromContext(context));

        var recordStructsWithAttribute = incrementalValuesProvider
            .Where(x => x is not null)
            .Select((x, _) => x!)
            .Collect();

        var combinedProvider = context.CompilationProvider
            .Combine(recordStructsWithAttribute);

        var third = combinedProvider
            .Combine(configProvider);


        context.RegisterSourceOutput(combinedProvider, (spc, source) =>
        {
            var (compilation, recordSymbols) = source;
            var sourceText = GenerateCommandDispatcherExtensionMethods(compilation, recordSymbols);
                spc.AddSource($"CommandDispatcher.g.cs", SourceText.From(sourceText, Encoding.UTF8));
        });

        //IEnumerable<string> contractSuffix = configuration.Contracts;
        //var commandSuffix = configuration.Commands.Suffix;
        //var methodHandlerName = configuration.Commands.HandlerName;

       // GenerateCommandDispatcherExtensionMethods(context, handlers);
    }

    private bool IsCommandHandler(SyntaxNode syntaxNode)
     => syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax
        &&
        methodDeclarationSyntax.Identifier.Text.EndsWith("Execute")
        &&
        methodDeclarationSyntax.ParameterList.Parameters.Count == 1
        &&
        GetTypeName(methodDeclarationSyntax.ParameterList.Parameters[0].Type).EndsWith("Command");

    private string GenerateCommandDispatcherExtensionMethods(Compilation compilation, ImmutableArray<IMethodSymbol> handlers)
    {
        INamedTypeSymbol taskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        INamedTypeSymbol commandResultSymbol = compilation.GetTypeByMetadataName("NForza.Cqrs.CommandResult");
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