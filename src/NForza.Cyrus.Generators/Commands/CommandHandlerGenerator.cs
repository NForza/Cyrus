using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Abstractions;
using NForza.Cyrus.Generators.Aggregates;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Commands;

public class CommandHandlerGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        if (cyrusGenerationContext.GenerationConfig != null && cyrusGenerationContext.CommandHandlers.Any())
        {
            var commandHandlers = cyrusGenerationContext.CommandHandlers;

            GenerateCommandDispatcherExtensionMethods(cyrusGenerationContext, spc);

            GenerateImplementedCommandHandlers(cyrusGenerationContext, spc);

            GenerateStepCommandHandlers(cyrusGenerationContext, spc);
        }
    }

    private void GenerateStepCommandHandlers(CyrusGenerationContext cyrusGenerationContext, SourceProductionContext spc)
    {
        var stepHandlers = cyrusGenerationContext.CommandHandlers
             .Where(ch => ch.IsPartialDefinition);

        foreach (var stepHandler in stepHandlers)
        {
            IEnumerable<CommandStep> steps = GetCommandHandlerSteps(stepHandler, cyrusGenerationContext).ToList();
            var model = new
            {
                Namespace = stepHandler.ContainingType.ContainingNamespace.ToDisplayString(),
                ClassName = stepHandler.ContainingType.Name,
                ResultType = stepHandler.ReturnsVoid ? "void" : stepHandler.ReturnType.ToFullName(),
                Arguments = stepHandler.Parameters.Select(p => new
                {
                    Type = p.Type.ToFullName(),
                    Name = p.Name
                }).ToList(),
                Steps = steps,
                ReturnVariable = GetLastStepToReturnResultOrDefault(stepHandler, steps),
                MethodName = stepHandler.Name,
            };
            var source = cyrusGenerationContext.LiquidEngine.Render(model, "StepCommand");
            spc.AddSource($"{stepHandler.ContainingType.Name}.g.cs", source);
        }
    }

    private string GetLastStepToReturnResultOrDefault(IMethodSymbol methodSymbol, IEnumerable<CommandStep> steps)
    {
        if (methodSymbol.ReturnsVoid)
            return string.Empty;
        foreach (var stepAttribute in steps.Reverse())
        {
            if (SymbolEqualityComparer.Default.Equals(stepAttribute.ReturnTypeSymbol, methodSymbol.ReturnType))
            {
                return stepAttribute.VariableName;
            }
        }
        throw new InvalidOperationException($"Cannot find a step that returns the method return type {methodSymbol.ReturnType.ToFullName()}");
    }

    private IEnumerable<CommandStep> GetCommandHandlerSteps(IMethodSymbol stepHandler, CyrusGenerationContext cyrusGenerationContext)
    {
        IEnumerable<AttributeData> stepAttributes = GetStepAttributes(stepHandler);

        var stepHandlerType = stepHandler.ContainingType;

        int stepIndex = 0;
        Dictionary<ITypeSymbol, string> availableArguments = new Dictionary<ITypeSymbol, string>(SymbolEqualityComparer.Default)
        {
            { stepHandler.Parameters[0].Type, stepHandler.Parameters[0].Name }
        };
        foreach (var stepAttribute in stepAttributes)
        {
            if (stepAttribute.ConstructorArguments.Length > 0)
            {
                var methodNameArgument = stepAttribute.ConstructorArguments[0];
                if (methodNameArgument.Value is string methodName)
                {
                    var methods = stepHandlerType.GetMembers(methodName)
                        .OfType<IMethodSymbol>()
                        .ToList();
                    if (methods.Count == 1)
                    {
                        string variableName = $"step{stepIndex++}Result";
                        IMethodSymbol methodSymbol = methods[0];
                        yield return new()
                        {
                            VariableName = variableName,
                            Name = methodName,
                            ReturnType = methodSymbol.ReturnType.ToFullName(),
                            ReturnTypeSymbol = methodSymbol.ReturnType,
                            IsVoid = methodSymbol.ReturnType.IsVoid(),
                            HasArgs = methodSymbol.Parameters.Length > 0,
                            Args = ResolveParameterArguments(methodSymbol.Parameters, availableArguments).ToList(),
                        };
                        if (!methodSymbol.ReturnType.IsVoid())
                        {
                            availableArguments.Remove(methodSymbol.ReturnType);
                            availableArguments.Add(methodSymbol.ReturnType, variableName);
                        }
                    }
                }
            }
        }
    }

    private static IEnumerable<AttributeData> GetStepAttributes(IMethodSymbol stepHandler)
    {
        return stepHandler.GetAttributes()
            .Where(attr => attr.AttributeClass != null && attr.AttributeClass.Name == "HandlerStepAttribute");
    }

    private IEnumerable<string> ResolveParameterArguments(ImmutableArray<IParameterSymbol> parameters, Dictionary<ITypeSymbol, string> availableArguments)
    {
        foreach (var param in parameters)
        {
            if (availableArguments.TryGetValue(param.Type, out var argName))
            {
                yield return argName;
            }
            else
            {
                throw new InvalidOperationException($"Cannot resolve argument of type {param.Type.ToFullName()} for parameter {param.Name}");
            }
        }
    }

    private static void GenerateImplementedCommandHandlers(CyrusGenerationContext cyrusGenerationContext, SourceProductionContext spc)
    {
        var implementedCommandHandlerRegistrations = string.Join("\n",
                cyrusGenerationContext.CommandHandlers
                    .Where(ch => !ch.IsPartialDefinition)
                    .Select(ch => ch.ContainingType)
                    .Where(x => x != null && !x.IsStatic)
                    .Distinct(SymbolEqualityComparer.Default)
                    .Select(cht => $" services.AddTransient<{cht!.ToFullName()}>();"));
        if (!string.IsNullOrEmpty(implementedCommandHandlerRegistrations))
        {
            var ctx = new
            {
                Usings = new string[] { "NForza.Cyrus.Cqrs", "NForza.Cyrus.Abstractions" },
                Namespace = "CommandHandlers",
                Name = "CommandHandlersRegistration",
                Initializer = implementedCommandHandlerRegistrations
            };

            var fileContents = cyrusGenerationContext.LiquidEngine.Render(ctx, "CyrusInitializer");
            spc.AddSource("CommandHandlerRegistration.g.cs", fileContents);
        }
    }

    private static void GenerateCommandDispatcherExtensionMethods(CyrusGenerationContext cyrusGenerationContext, SourceProductionContext spc)
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

        var resolvedSource = cyrusGenerationContext.LiquidEngine.Render(model, "CommandDispatcher");

        if (!string.IsNullOrEmpty(resolvedSource))
        {
            spc.AddSource($"CommandDispatcher.g.cs", resolvedSource);
        }
    }

    private static AggregateRootDefinition? FindAggregateRoot(IEnumerable<AggregateRootDefinition> aggregateRoots, ITypeSymbol? type)
        => aggregateRoots.FirstOrDefault(ard => SymbolEqualityComparer.Default.Equals(ard.Symbol, type));

    public class CommandStep
    {
        public string Name { get; set; } = string.Empty;
        public string ReturnType { get; internal set; } = string.Empty;
        public ITypeSymbol ReturnTypeSymbol { get; internal set; } = null!;
        public bool IsVoid { get; internal set; }
        public string VariableName { get; internal set; } = string.Empty;
        public bool HasArgs { get; internal set; }
        public List<string> Args { get; internal set; } = [];
    }
}