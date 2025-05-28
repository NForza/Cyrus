using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Aggregates;
using NForza.Cyrus.Generators.Model;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.Commands;

public class CommandHandlerGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        if (cyrusGenerationContext.GenerationConfig != null && cyrusGenerationContext.CommandHandlers.Any())
        {
            var commandHandlers = cyrusGenerationContext.CommandHandlers;

            var sourceText = GenerateCommandDispatcherExtensionMethods(cyrusGenerationContext, cyrusGenerationContext.LiquidEngine);
            if (!string.IsNullOrEmpty(sourceText))
            {
                spc.AddSource($"CommandDispatcher.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            }

#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            var commandHandlerRegistrations = string.Join(Environment.NewLine,
                    cyrusGenerationContext.CommandHandlers
                        .Select(ch => ch.ContainingType)
                        .Where(x => x != null && !x.IsStatic)
                        .Distinct(SymbolEqualityComparer.Default)
                        .Select(cht => $" services.AddTransient<{cht.ToFullName()}>();"));
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
            if (!string.IsNullOrEmpty(commandHandlerRegistrations))
            {
                var ctx = new
                {
                    Usings = new string[] { "NForza.Cyrus.Cqrs" },
                    Namespace = "CommandHandlers",
                    Name = "CommandHandlersRegistration",
                    Initializer = commandHandlerRegistrations
                };

                var fileContents = cyrusGenerationContext.LiquidEngine.Render(ctx, "CyrusInitializer");
                spc.AddSource("CommandHandlerRegistration.g.cs", SourceText.From(fileContents, Encoding.UTF8));
            }

            string assemblyName = commandHandlers.First().ContainingAssembly.Name;
            var commandSymbols = commandHandlers.Select(ch => (INamedTypeSymbol)ch.Parameters[0].Type);

            var commandModels = GetPartialModelClass(assemblyName, "Command", "Commands", "ModelTypeDefinition", commandSymbols.Select(cm => ModelGenerator.ForNamedType(cm, cyrusGenerationContext.LiquidEngine)), cyrusGenerationContext.LiquidEngine);
            spc.AddSource($"model-commands.g.cs", SourceText.From(commandModels, Encoding.UTF8));

            var referencedTypes = commandSymbols.SelectMany(cs => cs.GetReferencedTypes());
            var referencedTypeModels = GetPartialModelClass(assemblyName, "Command", "Models", "ModelTypeDefinition", referencedTypes.Select(cm => ModelGenerator.ForNamedType(cm, cyrusGenerationContext.LiquidEngine)), cyrusGenerationContext.LiquidEngine);
            spc.AddSource($"model-command-types.g.cs", SourceText.From(referencedTypeModels, Encoding.UTF8));
        }
    }

    private static string GenerateCommandDispatcherExtensionMethods(CyrusGenerationContext cyrusGenerationContext, LiquidEngine liquidEngine)
    {
        var commands = cyrusGenerationContext.CommandHandlers.Select(h => new
        {
            Handler = h,
            AggregateRoot = h.Parameters.Length == 2 ? FindAggregateRoot(cyrusGenerationContext.AggregateRoots, h.Parameters[1].Type) : null,
            CommandType = h.Parameters[0].Type.ToFullName(),
            h.Name,
            h.ReturnsVoid,
            ReturnType = (INamedTypeSymbol)h.ReturnType,
            ReturnsTask = h.ReturnType.IsTaskType()
        }).ToList();

        var model = new
        {
            Commands = commands.Select(cmd => new
            {
                ReturnTypeOriginal = cmd.ReturnType,
                ReturnType = cmd.ReturnsTask ? cmd.ReturnType.TypeArguments[0].ToFullName() : cmd.ReturnType.ToFullName(),
                Invocation = cmd.Handler.GetCommandInvocation(variableName: "command", serviceProviderVariable: "commandDispatcher.Services", aggregateRootVariableName: cmd.AggregateRoot != null ? "aggregateRoot" : null),
                cmd.Name,
                cmd.ReturnsVoid,
                cmd.CommandType,
                cmd.ReturnsTask,
                cmd.AggregateRoot,
                AggregateRootType = cmd.AggregateRoot?.Symbol?.ToFullName() ?? "",
                AggregateRootIdPropertyType = cmd.AggregateRoot?.AggregateRootProperty?.Type.ToFullName() ?? "",
                AggregateRootIdPropertyName = cmd.AggregateRoot?.AggregateRootProperty?.Name ?? "",
            }).ToList()
        };

        var resolvedSource = liquidEngine.Render(model, "CommandDispatcher");

        return resolvedSource;
    }

    private static AggregateRootDefinition FindAggregateRoot(IEnumerable<AggregateRootDefinition> aggregateRoots, ITypeSymbol type) 
        => aggregateRoots.FirstOrDefault(ard => SymbolEqualityComparer.Default.Equals(ard.Symbol, type));
}