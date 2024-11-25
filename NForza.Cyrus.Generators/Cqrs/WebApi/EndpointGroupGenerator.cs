using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Cqrs.Generator;

namespace NForza.Cyrus.Generators.Cqrs.WebApi;

[Generator]
public class EndpointGroupGenerator : CqrsSourceGenerator, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(true);

        var msbuildProperties = context.AnalyzerConfigOptionsProvider
            .Select((options, cancellationToken) =>
            {
                if (options.GlobalOptions.TryGetValue("MSBuildProjectSdk", out var sdk))
                {
                    return sdk;
                }

                return null;
            });

        var compilationProvider = context.CompilationProvider;

        var allClassesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax,
                transform: (context, _) => (ClassDeclarationSyntax)context.Node)
             .Where(static classDeclaration => classDeclaration.BaseList is not null);

        var classesWithSemanticModel = allClassesProvider
            .Combine(context.CompilationProvider)
            .Select(static (pair, _) =>
            {
                var (classDeclaration, compilation) = pair;
                var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                var classSymbol = (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(classDeclaration)!;

                return (classSymbol, compilation);
            })
            .Where(providerTuple =>
            {
                var (classSymbol, compilation) = providerTuple;
                return classSymbol is not null
                       &&
                       IsDirectlyDerivedFrom(classSymbol, "NForza.Cyrus.Cqrs.WebApi.EndpointGroup");
            })
            .Collect();

        var classesWithSemanticModelAndMsBuildProperties = classesWithSemanticModel.Combine(msbuildProperties);

        context.RegisterSourceOutput(classesWithSemanticModelAndMsBuildProperties, (spc, classesPlusMetadataAndConfig) =>
        {
            var(classesPlusMetadata, msbuildProperties) = classesPlusMetadataAndConfig;

            if (msbuildProperties != null && msbuildProperties.Contains("Microsoft.NET.Sdk.Web"))
            {
                var sourceText = GenerateEndpointGroupDeclarations(
                    classesPlusMetadata.Select(cmd => cmd.classSymbol));
                spc.AddSource($"RegisterEndpointGroups.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            }
        });
    }

    private string GenerateEndpointGroupDeclarations(IEnumerable<INamedTypeSymbol> classSymbols)
    {
        var sb = new StringBuilder();
        foreach (var classSymbol in classSymbols)
        {
            sb.AppendLine($"options.Services.AddEndpointGroup<{classSymbol}>();");
        }

        var resolvedSource = TemplateEngine.ReplaceInResourceTemplate("RegisterEndpointGroups.cs", new Dictionary<string, string>
        {
            ["RegisterEndpointGroups"] = sb.ToString()
        });

        return resolvedSource;
    }
}
