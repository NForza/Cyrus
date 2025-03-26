using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Generators.Model;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.Generators.SignalR;

public class SignalRHubGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider, LiquidEngine liquidEngine)
    {
        var signalRHubs = cyrusProvider.SignalRHubs;
        var configuration = cyrusProvider.GenerationConfig;

        var isWebApi = configuration.GenerationTarget.Contains(GenerationTarget.WebApi);

        if (isWebApi)
        {
            var registration = GenerateSignalRHubRegistration(signalRHubs, liquidEngine);
            spc.AddSource($"RegisterSignalRHubs.g.cs", SourceText.From(registration, Encoding.UTF8));

            foreach (var signalRModel in signalRHubs)
            {
                var sourceText = GenerateSignalRHub(signalRModel, LiquidEngine);
                spc.AddSource($"{signalRModel.Symbol.Name}.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            }
            if (signalRHubs.Any())
            {
                string assemblyName = signalRHubs.First().Symbol.ContainingAssembly.Name;
                var commandModels = GetPartialModelClass(assemblyName, "SignalR", "Hubs", "ModelHubDefinition", signalRHubs.Select(e => ModelGenerator.ForHub(e, LiquidEngine)));
                spc.AddSource($"model-hubs.g.cs", SourceText.From(commandModels, Encoding.UTF8));
            }
        }
    }

    private string GenerateSignalRHubRegistration(ImmutableArray<SignalRHubClassDefinition> signalRDefinitions, LiquidEngine liquidEngine)
    {
        var usings = signalRDefinitions
            .Select(cm => cm.Symbol.ContainingNamespace)
            .Distinct(SymbolEqualityComparer.Default)
            .Select(u => u?.ToFullName())
            .Where(u => !string.IsNullOrWhiteSpace(u));

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

    private string GenerateSignalRHub(SignalRHubClassDefinition classDefinition, LiquidEngine liquidEngine)
    {
        string queries = GenerateQueries(classDefinition.Queries);

        var model = new
        {
            classDefinition.Symbol.Name,
            Namespace = classDefinition.Symbol.ContainingNamespace.ToDisplayString(),
            Commands = classDefinition.Commands.Select(c => new
            {
                c.MethodName,
                c.FullTypeName,
                c.Handler.ReturnType,
                ReturnsVoid = c.Handler.ReturnType.IsVoid(),
                Invocation = c.Handler.GetCommandInvocation(variableName: "command", serviceProviderVariable: "services"),
                ReturnsTask = c.Handler.ReturnType.IsTaskType()
            }),
            QueryMethods = queries,
        };

        var source = liquidEngine.Render(model, "SignalRHub");

        return source;
    }

    private static string GenerateCommands(IEnumerable<SignalRCommand> signalRCommands)
    {
        var sb = new StringBuilder();
        foreach (var command in signalRCommands)
        {
            sb.AppendLine(
    @$"public async Task {command.MethodName}({command.FullTypeName} command) 
    {{
        var result = await commandDispatcher.Execute(command);
        if (result.Succeeded)
        {{
            await SendEvents(result.Events);
        }}
    }}");
        }
        return sb.ToString();
    }

    private static string GenerateQueries(IEnumerable<SignalRQuery> signalRQueries)
    {
        var sb = new StringBuilder();
        foreach (var query in signalRQueries)
        {
            sb.AppendLine(
    @$"public async Task {query.MethodName}({query.FullTypeName} query) 
    {{
        var result = await queryProcessor.Query(query);
        await SendQueryResultReply(""{query.MethodName}"", result);
    }}");
        }
        return sb.ToString();
    }
}
