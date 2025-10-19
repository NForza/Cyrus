using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Aggregates;
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
                spc.AddSource($"CommandDispatcher.g.cs", sourceText);
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
                    Usings = new string[] { "NForza.Cyrus.Cqrs", "NForza.Cyrus.Abstractions" },
                    Namespace = "CommandHandlers",
                    Name = "CommandHandlersRegistration",
                    Initializer = commandHandlerRegistrations
                };

                var fileContents = cyrusGenerationContext.LiquidEngine.Render(ctx, "CyrusInitializer");
                spc.AddSource("CommandHandlerRegistration.g.cs", fileContents);
            }
        }
    }

    private static string GenerateCommandDispatcherExtensionMethods(CyrusGenerationContext cyrusGenerationContext, LiquidEngine liquidEngine)
    {
        var commands = cyrusGenerationContext.CommandHandlers.Select(h =>
        {
            ITypeSymbol? aggregateRootSymbol = h.Parameters.Skip(1).FirstOrDefault(p => p.Type.IsAggregateRoot())?.Type;
            return new
            {
                Handler = h,
                AggregateRoot = FindAggregateRoot(cyrusGenerationContext.AggregateRoots, aggregateRootSymbol),
                CommandType = h.Parameters[0].Type.ToFullName(),
                CommandAggregateRootIdPropertyName = h.Parameters[0].Type is INamedTypeSymbol namedTypeSymbol ? namedTypeSymbol.GetAggregateRootIdProperty()?.Name ?? null : null,
                h.Name,
                h.ReturnsVoid,
                ReturnType = (INamedTypeSymbol)h.ReturnType,
                ReturnsTask = h.ReturnType.IsTaskType()
            };
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
                RequiresAsync = cmd.ReturnsTask || cmd.AggregateRoot != null,
                AggregateRootType = cmd.AggregateRoot?.Symbol?.ToFullName() ?? "",
                AggregateRootIdPropertyType = cmd.AggregateRoot?.AggregateRootProperty?.Type.ToFullName() ?? "",
                AggregateRootIdPropertyName = cmd.CommandAggregateRootIdPropertyName,
            }).ToList()
        };

        var resolvedSource = liquidEngine.Render(model, "CommandDispatcher");

        return resolvedSource;
    }

    private static AggregateRootDefinition? FindAggregateRoot(IEnumerable<AggregateRootDefinition> aggregateRoots, ITypeSymbol? type) 
        => aggregateRoots.FirstOrDefault(ard => SymbolEqualityComparer.Default.Equals(ard.Symbol, type));
}