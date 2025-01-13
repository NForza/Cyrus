using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Generators;

namespace NForza.Cyrus.Generators.Cqrs.WebApi;

[Generator]
public class SignalRHubGenerator : GeneratorBase, IIncrementalGenerator
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

                return (classSymbol);
            })
            .Where(classSymbol =>
            {
                return classSymbol is not null
                       &&
                       IsDirectlyDerivedFrom(classSymbol, "NForza.Cyrus.SignalR.SignalRHub");
            })
            .Collect();

        var endpointGroupModelsAndConfigurationProvider = signalrHubModelProvider.Combine(configurationProvider);

        context.RegisterSourceOutput(endpointGroupModelsAndConfigurationProvider, (spc, classesAndConfig) =>
        {
            var (signalRModels, configuration) = classesAndConfig;

            var isWebApi = configuration.GenerationTarget.Contains(GenerationTarget.WebApi);

            if (isWebApi)
            {
                var registration = GenerateSignalRHubRegistration(signalRModels);
                spc.AddSource($"RegisterSignalRHubs.g.cs", SourceText.From(registration, Encoding.UTF8));

                foreach (var signalRModel in signalRModels)
                {
                    var sourceText = GenerateSignalRHub(signalRModel);
                    spc.AddSource($"{signalRModel.Name}.g.cs", SourceText.From(sourceText, Encoding.UTF8));
                }
            }
        });
    }

    private string GenerateSignalRHubRegistration(System.Collections.Immutable.ImmutableArray<INamedTypeSymbol> signalRModels)
    {
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
        string usings = string.Join(Environment.NewLine, signalRModels.Select( cm => $"using {cm.ContainingNamespace.ToFullName()};").Distinct());
#pragma warning restore RS1035 // Do not use APIs banned for analyzers

        var sb = new StringBuilder();
        foreach (var classSymbol in signalRModels)
        {
            sb.AppendLine($"signalRHubDictionary.AddSignalRHub<{classSymbol.ToFullName()}_Generated>(\"{classSymbol.Name}\");");
        }

        var replacements = new Dictionary<string, string>
        {
            ["RegisterSignalRHubs"] = sb.ToString(),
            ["Usings"] = usings,
            ["CommandMethods"] = "",
            ["QueryMethods"] = "",
        };

        var source = TemplateEngine.ReplaceInResourceTemplate("RegisterSignalRHubs.cs", replacements);

        return source;
    }

    private string GenerateSignalRHub(INamedTypeSymbol classSymbol)
    {
        var sb = new StringBuilder();

        var replacements = new Dictionary<string, string>
        {
            ["Name"] = classSymbol.Name,
            ["Namespace"] = classSymbol.ContainingNamespace.ToDisplayString(),
            ["CommandMethods"] = "",
            ["QueryMethods"] = "",
        };

        var source = TemplateEngine.ReplaceInResourceTemplate("SignalRHub.cs", replacements);

        return source;
    }
}
