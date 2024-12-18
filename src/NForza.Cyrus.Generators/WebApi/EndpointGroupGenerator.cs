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
public class EndpointGroupGenerator : GeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);

        var configurationProvider = ConfigProvider(context);

        var allClassesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax,
                transform: (context, _) => (ClassDeclarationSyntax)context.Node)
             .Where(static classDeclaration => classDeclaration.BaseList is not null);

        var classModels = allClassesProvider
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

        var classesModelsAndConfigurationProvider = classModels.Combine(configurationProvider);

        context.RegisterSourceOutput(classesModelsAndConfigurationProvider, (spc, classesAndConfig) =>
        {
            var (classesModels, configuration) = classesAndConfig;

            var isWebApi = configuration.GenerationTarget.Contains(GenerationTarget.WebApi);

            if (isWebApi && classesModels.Any())
            {
                var sourceText = GenerateEndpointGroupDeclarations(classesModels);
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
