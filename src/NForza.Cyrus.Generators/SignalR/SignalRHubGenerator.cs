using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;

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
                    spc.AddSource($"{signalRModel.Symbol.Name}.g.cs", SourceText.From(sourceText, Encoding.UTF8));
                }
            }
        });
    }

    private string GenerateSignalRHubRegistration(System.Collections.Immutable.ImmutableArray<SignalRHubClassDefinition> signalRDefinitions)
    {
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
        string usings = string.Join(Environment.NewLine, signalRDefinitions.Select( cm => $"using {cm.Symbol.ContainingNamespace.ToFullName()};").Distinct());
#pragma warning restore RS1035 // Do not use APIs banned for analyzers

        var sb = new StringBuilder();
        foreach (var signalRDefinition in signalRDefinitions)
        {
            sb.AppendLine($"signalRHubDictionary.AddSignalRHub<{signalRDefinition.Symbol.ToFullName()}_Generated>({signalRDefinition.Path});");
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

    private string GenerateSignalRHub(SignalRHubClassDefinition classDefinition)
    {
        string commands = GenerateCommands(classDefinition.Commands);

        var replacements = new Dictionary<string, string>
        {
            ["Name"] = classDefinition.Symbol.Name,
            ["Namespace"] = classDefinition.Symbol.ContainingNamespace.ToDisplayString(),
            ["CommandMethods"] = commands,
            ["QueryMethods"] = "",
        };

        var source = TemplateEngine.ReplaceInResourceTemplate("SignalRHub.cs", replacements);

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
            SendEvents(result.Events);
        }}
    }}");
        }
        return sb.ToString();
    }
}

internal class SignalRCommand
{
    public string FullTypeName { get; internal set; }
    public SourceText MethodName { get; internal set; }
}

internal record SignalRHubClassDefinition
{
    IEnumerable<InvocationExpressionSyntax> GetMethodCallsOf(SyntaxNode node, string methodName)
    {
        IEnumerable<InvocationExpressionSyntax> methodCalls = node
                         .DescendantNodes()
                         .OfType<InvocationExpressionSyntax>();
        return methodCalls
                 .Where(methodCall =>
                 {
                     return methodCall.Expression switch
                     {
                         IdentifierNameSyntax identifierName => identifierName.Identifier.Text == methodName,
                         GenericNameSyntax genericNameInMember => genericNameInMember.Identifier.Text == methodName,
                         _ => false
                     };
                 });
    }

    void SetPath(INamedTypeSymbol symbol, BlockSyntax? constructorBody)
    {
        string? usePathArgument = constructorBody != null ? GetMethodCallsOf(constructorBody, "UsePath").FirstOrDefault()?.ArgumentList.Arguments.FirstOrDefault()?.ToString() : null;
        Path = usePathArgument ?? symbol.Name.ToLower();
    }

    public SignalRHubClassDefinition(ClassDeclarationSyntax declaration, INamedTypeSymbol symbol, SemanticModel semanticModel)
    {
        Declaration = declaration;
        Symbol = symbol;
        SemanticModel = semanticModel;
    }

    public SignalRHubClassDefinition Initialize()
    {
        var constructorBody = Declaration.DescendantNodes()
            .OfType<ConstructorDeclarationSyntax>()
            .Where(constructorDeclarationSyntax => constructorDeclarationSyntax.Body != null)
            .FirstOrDefault()?.Body;

        if (constructorBody != null)
        {
            SetPath(Symbol, constructorBody);
            SetCommands(Symbol, constructorBody);
        }
        return this;
    }

    private void SetCommands(INamedTypeSymbol symbol, BlockSyntax constructorBody)
    {
        var memberAccessExpressionSyntaxes = GetMethodCallsOf(constructorBody, "CommandMethodFor")
                .Select(ies => ies.Expression)
                .OfType<GenericNameSyntax>();
        var commands = memberAccessExpressionSyntaxes.Select(name => name.TypeArgumentList.Arguments.Single());
        Commands = commands.Select(genericArg =>
        {
            var symbol = SemanticModel.GetSymbolInfo(genericArg).Symbol!;
            return new SignalRCommand { MethodName = genericArg.GetText(), FullTypeName = symbol.ToFullName() };
        });
    }

    public string Path { get; private set; } = "";
    public ClassDeclarationSyntax Declaration { get; }
    public INamedTypeSymbol Symbol { get; }
    public SemanticModel SemanticModel { get; }
    public IEnumerable<SignalRCommand> Commands { get; internal set; } = [];
}