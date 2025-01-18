using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Cqrs.WebApi;

[Generator]
public class EndpointGroupGenerator : CyrusGeneratorBase, IIncrementalGenerator
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

        var endpointGroupModelProvider = allEndpointGroupsProvider
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
                       IsDirectlyDerivedFrom(classSymbol, "NForza.Cyrus.WebApi.EndpointGroup");
            })
            .Collect();

        var endpointGroupModelsAndConfigurationProvider = endpointGroupModelProvider.Combine(configurationProvider);

        context.RegisterSourceOutput(endpointGroupModelsAndConfigurationProvider, (spc, classesAndConfig) =>
        {
            var (endpointGroupModels, configuration) = classesAndConfig;

            var isWebApi = configuration.GenerationTarget.Contains(GenerationTarget.WebApi);

            if (isWebApi)
            {
                var sourceText = GenerateEndpointGroupDeclarations(endpointGroupModels);
                spc.AddSource($"RegisterEndpointGroups.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            }
        });
    }

    private string GenerateEndpointGroupDeclarations(IEnumerable<INamedTypeSymbol> classSymbols)
    {
        var sb = new StringBuilder();
        foreach (var classSymbol in classSymbols)
        {
            sb.AppendLine($"options.Services.AddEndpointGroup<{classSymbol.ToFullName()}>();");
        }

        var resolvedSource = TemplateEngine.ReplaceInResourceTemplate("RegisterEndpointGroups.cs", new Dictionary<string, string>
        {
            ["RegisterEndpointGroups"] = sb.ToString()
        });

        return resolvedSource;
    }
}
