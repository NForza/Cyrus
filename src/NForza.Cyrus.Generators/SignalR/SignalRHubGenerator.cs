using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Analyzers;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Model;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.SignalR;

public class SignalRHubGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var signalRHubs = cyrusGenerationContext.SignalRHubs.Select(h => h.Initialize(cyrusGenerationContext));

        ReportMissingHandlers(signalRHubs, spc);

        var configuration = cyrusGenerationContext.GenerationConfig;

        var isWebApi = configuration.GenerationTarget.Contains(GenerationTarget.WebApi);

        if (isWebApi)
        {
            var registration = GenerateSignalRHubRegistration(signalRHubs, cyrusGenerationContext.LiquidEngine);
            spc.AddSource($"RegisterSignalRHubs.g.cs", registration);

            foreach (var signalRModel in signalRHubs)
            {
                var sourceText = GenerateSignalRHub(signalRModel, cyrusGenerationContext.LiquidEngine);
                spc.AddSource($"{signalRModel.Symbol.Name}.g.cs", sourceText);
            }
            if (signalRHubs.Any())
            {
                string assemblyName = signalRHubs.First().Symbol.ContainingAssembly.Name;
                var commandModels = GetPartialModelClass(assemblyName, "SignalR", "Hubs", "ModelHubDefinition", signalRHubs.Select(e => ModelGenerator.ForHub(e, cyrusGenerationContext.LiquidEngine)), cyrusGenerationContext.LiquidEngine);
                spc.AddSource($"model-hubs.g.cs", commandModels);
            }
        }
    }

    private void ReportMissingHandlers(IEnumerable<SignalRHubClassDefinition> signalRHubs, SourceProductionContext spc)
    {
        foreach(var hub in signalRHubs)
        {
            foreach (var command in hub.Commands)
            {
                if (command.Handler == null)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.MissingHandler,
                        Location.None));
                    continue;
                }
                if (command.Handler.ReturnType == null)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.UnableToDetermineReturnType,
                        Location.None));
                }

            }
        }
    }

    private string GenerateSignalRHubRegistration(IEnumerable<SignalRHubClassDefinition> signalRDefinitions, LiquidEngine liquidEngine)
    {
        var usings = signalRDefinitions
            .Select(cm => cm.Symbol.ContainingNamespace)
            .Distinct(SymbolEqualityComparer.Default)
            .Select(u => u?.ToFullName())
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Concat(["Microsoft.Extensions.DependencyInjection", "NForza.Cyrus.Abstractions" ]);

        var hubs = signalRDefinitions
            .Select(s => new
            {
                Name = s.Symbol.ToFullName(),
                s.Path
            });

        var model = new
        {
            Hubs = hubs,
            Usings = usings,
        };

        var source = liquidEngine.Render(model, "RegisterSignalRHubs");

        return source;
    }

    static CommandAdapterMethod[] adapterMethodsWithEvents =
        [
                            CommandAdapterMethod.FromIResultAndMessages,
                            CommandAdapterMethod.FromIResultAndMessage
        ];

    private string GenerateSignalRHub(SignalRHubClassDefinition classDefinition, LiquidEngine liquidEngine)
    {
        var model = new
        {
            classDefinition.Symbol.Name,
            Namespace = classDefinition.Symbol.ContainingNamespace.ToDisplayString(),
            Commands = classDefinition.Commands
                .Select(c => new
                {
                    c.MethodName,
                    c.ClrTypeName,
                    c.Handler.ReturnType,
                    ReturnsVoid = c.Handler.ReturnType.IsVoid(),
                    Invocation = c.Handler.GetCommandInvocation(variableName: "command", serviceProviderVariable: "services"),
                    ReturnsTask = c.Handler.ReturnType.IsTaskType(),
                    ReturnsEvents = adapterMethodsWithEvents
                        .Contains(c.Handler.GetAdapterMethodName()),
                })
                .ToList(),
            Queries = classDefinition.Queries
                .Select(c => new
                {
                    c.MethodName,
                    c.ClrTypeName,
                })
                .ToList(),
            Events = classDefinition.Events
                .Select(c => new
                {
                    c.MethodName,
                    c.ClrTypeName,
                    c.Broadcast
                })
                .ToList(),
        };

        var source = liquidEngine.Render(model, "SignalRHub");

        return source;
    }
}
