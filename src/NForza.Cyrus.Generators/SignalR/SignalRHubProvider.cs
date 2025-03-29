using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.SignalR;

public class SignalRHubProvider : CyrusProviderBase<ImmutableArray<SignalRHubClassDefinition>>
{
    public override IncrementalValueProvider<ImmutableArray<SignalRHubClassDefinition>> GetProvider(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<GenerationConfig> configProvider)
    {
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
            .Where(signalRHubClassDefinition => signalRHubClassDefinition.Symbol.IsDirectlyDerivedFrom("NForza.Cyrus.SignalR.SignalRHub"))
            .Select((signalRHubClassDefinition, _) => signalRHubClassDefinition.Initialize())
            .Collect();

        return signalrHubModelProvider;
    }
}
