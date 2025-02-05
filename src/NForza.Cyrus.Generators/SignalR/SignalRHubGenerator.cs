using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Model;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.SignalR;

[Generator]
public class SignalRHubGenerator : CyrusGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);

        var configurationProvider = ConfigProvider(context);

        var allEndpointGroupsProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax,
                transform: (context, _) => (ClassDeclarationSyntax)context.Node)
             .Where(static classDeclaration => classDeclaration.BaseList is not null);

        var signalrHubModelProvider = allEndpointGroupsProvider
            .Combine(context.CompilationProvider)
            .Select(static (pair, _) =>
            {
                var (classDeclaration, compilation) = pair;
                var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                var classSymbol = (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(classDeclaration)!;

                SignalRHubClassDefinition definition = new SignalRHubClassDefinition(classDeclaration, classSymbol, semanticModel);
                return definition;
            })
            .Where(signalRHubClassDefinition =>
            {
                return signalRHubClassDefinition.Symbol is not null
                       &&
                       IsDirectlyDerivedFrom(signalRHubClassDefinition.Symbol, "NForza.Cyrus.SignalR.SignalRHub");
            })
            .Select((signalRHubClassDefinition, _) => signalRHubClassDefinition.Initialize())
            .Collect();

        var endpointGroupModelsAndConfigurationProvider = signalrHubModelProvider.Combine(configurationProvider).Combine(context.CompilationProvider);

        context.RegisterSourceOutput(endpointGroupModelsAndConfigurationProvider, (spc, classesAndConfig) =>
        {
            var ((signalRModels, configuration), compilation) = classesAndConfig;

            var isWebApi = configuration.GenerationTarget.Contains(GenerationTarget.WebApi);

            if (isWebApi)
            {
                var registration = GenerateSignalRHubRegistration(signalRModels);
                spc.AddSource($"RegisterSignalRHubs.g.cs", SourceText.From(registration, Encoding.UTF8));

                foreach (var signalRModel in signalRModels)
                {
                    var sourceText = GenerateSignalRHub(signalRModel, LiquidEngine);
                    spc.AddSource($"{signalRModel.Symbol.Name}.g.cs", SourceText.From(sourceText, Encoding.UTF8));
                }
                if (signalRModels.Any())
                {
                    string assemblyName = signalRModels.First().Symbol.ContainingAssembly.Name;
                    var commandModels = GetPartialModelClass(assemblyName, "Hubs", "ModelHubDefinition", signalRModels.Select(e => ModelGenerator.ForHub(e, LiquidEngine)));
                    spc.AddSource($"model-hubs.g.cs", SourceText.From(commandModels, Encoding.UTF8));
                }
            }
        });
    }

    private string GenerateSignalRHubRegistration(System.Collections.Immutable.ImmutableArray<SignalRHubClassDefinition> signalRDefinitions)
    {
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
        string usings = string.Join(Environment.NewLine, signalRDefinitions.Select(cm => $"using {cm.Symbol.ContainingNamespace.ToFullName()};").Distinct());
#pragma warning restore RS1035 // Do not use APIs banned for analyzers

        var sb = new StringBuilder();
        foreach (var signalRDefinition in signalRDefinitions)
        {
            sb.AppendLine($"signalRHubDictionary.AddSignalRHub<{signalRDefinition.Symbol.ToFullName()}_Generated>({signalRDefinition.Path});");
        }

        var model = new
        {
            RegisterSignalRHubs = sb.ToString(),
            Usings = usings,
            CommandMethods = "",
            QueryMethods = ""
        };

        var source = LiquidEngine.Render(model, "RegisterSignalRHubs");

        return source;
    }

    private string GenerateSignalRHub(SignalRHubClassDefinition classDefinition, LiquidEngine liquidEngine)
    {

        //this needs to be moved to the template
        string commands = GenerateCommands(classDefinition.Commands);
        string queries = GenerateQueries(classDefinition.Queries);

        var model = new
        {
            classDefinition.Symbol.Name,
            Namespace = classDefinition.Symbol.ContainingNamespace.ToDisplayString(),
            CommandMethods = commands,
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
