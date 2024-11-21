using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace NForza.Cqrs.Generator;

[Generator]
public class EndpointGroupGenerator : CqrsSourceGenerator, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);

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
                return (classDeclaration, semanticModel, compilation);
            })
            .Where(providerTuple =>
            {
                var (classDeclaration, semanticModel, compilation) = providerTuple;
                var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
                return classSymbol is not null
                       &&
                       IsDirectlyDerivedFrom(classDeclaration, "NForza.Cqrs.WebApi.EndpointGroup", semanticModel, compilation);
            })
            .Collect();

        context.RegisterSourceOutput(classesWithSemanticModel, (spc, classesPlusMetadata) =>
        {
            foreach (var endpointGroup in classesPlusMetadata)
            {
                (var classDeclaration, var semanticModel, _) = endpointGroup;
                var sourceText = GenerateEndpointGroupDeclarations(classDeclaration);
                spc.AddSource($"RegisterEndpointGroups.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            };
        });
    }

    private string GenerateEndpointGroupDeclarations(ClassDeclarationSyntax classDeclaration)
    {
        return "";
    }
}
