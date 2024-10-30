using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using NForza.Cqrs.Generator.Config;
using NForza.Generators;

#pragma warning disable RS1035 // Do not use banned APIs for analyzers

namespace NForza.Cqrs.Generator;

[Generator]
public class CqrsServiceCollectionGenerator : CqrsSourceGenerator, ISourceGenerator
{
    public override void Execute(GeneratorExecutionContext context)
    {
        DebugThisGenerator(false);

        var configuration = ParseConfigFile<CqrsConfig>(context, "cqrsConfig.yaml");

        IEnumerable<string> contractSuffix = configuration.Contracts;
        var commandSuffix = configuration.Commands.Suffix;
        var methodHandlerName = configuration.Commands.HandlerName;

        var commands = GetAllCommands(context.Compilation, contractSuffix, commandSuffix).ToList();
        var handlers = GetAllCommandHandlers(context, methodHandlerName, commands);

        GenerateServiceCollectionExtensions(context, handlers);
    }

    private void GenerateServiceCollectionExtensions(GeneratorExecutionContext context, List<IMethodSymbol> handlers)
    {
        StringBuilder source = new();
        foreach (var typeToRegister in handlers.Select(h => h.ContainingType).Distinct(SymbolEqualityComparer.Default))
        {
            source.Append($@"
            services.AddTransient<{typeToRegister.ToDisplayString()}>();");
        }
        string registerTypes = source.ToString();

        source.Clear();
        foreach (var handler in handlers)
        {
            var handlerReturnType = handler.ReturnType;
            var commandType = handler.Parameters[0].Type;
            var typeSymbol = handler.ContainingType;

            source.Append($@"
        handlers.AddHandler<{commandType}>((services, command) => services.GetRequiredService<{typeSymbol}>().Execute(({commandType})command));");
        }
        string registerHandlers = source.ToString();

        var replacements = new Dictionary<string, string>
        {
            ["RegisterTypes"] = registerTypes,
            ["RegisterHandlers"] = registerHandlers
        };

        var resolvedSource = TemplateEngine.ReplaceInResourceTemplate("ServiceCollectionExtensions.cs", replacements);
        context.AddSource($"ServiceCollectionExtensions.g.cs", resolvedSource);
    }
}