using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Cqrs.WebApi;

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
            SetEvents(Symbol, constructorBody);
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
            return new SignalRCommand { MethodName = genericArg.GetText().ToString(), Name = symbol.Name, FullTypeName = symbol.ToFullName() };
        });
    }

    private void SetEvents(INamedTypeSymbol symbol, BlockSyntax constructorBody)
    {
        var memberAccessExpressionSyntaxes = GetMethodCallsOf(constructorBody, "PublishesEventToCaller")
                .Select(ies => ies.Expression)
                .OfType<GenericNameSyntax>();
        var callerEventTypes = memberAccessExpressionSyntaxes.Select(name => name.TypeArgumentList.Arguments.Single());
        var callerEvents = callerEventTypes.Select(genericArg =>
        {
            var symbol = SemanticModel.GetSymbolInfo(genericArg).Symbol!;
            return new SignalREvent { MethodName = genericArg.GetText().ToString(), Name = symbol.Name, FullTypeName = symbol.ToFullName(), IsBroadcast = false };
        });

        memberAccessExpressionSyntaxes = GetMethodCallsOf(constructorBody, "PublishesEventToAll")
                .Select(ies => ies.Expression)
                .OfType<GenericNameSyntax>();
        var broadcastEventTypes = memberAccessExpressionSyntaxes.Select(name => name.TypeArgumentList.Arguments.Single());
        var broadcastEvents = broadcastEventTypes.Select(genericArg =>
        {
            var symbol = SemanticModel.GetSymbolInfo(genericArg).Symbol!;
            return new SignalREvent { MethodName = genericArg.GetText().ToString(), Name = symbol.Name, FullTypeName = symbol.ToFullName(), IsBroadcast = true };
        });

        Events = callerEvents.Concat(broadcastEvents);
    }


    public string Path { get; private set; } = "";
    public ClassDeclarationSyntax Declaration { get; }
    public INamedTypeSymbol Symbol { get; }
    public SemanticModel SemanticModel { get; }
    public IEnumerable<SignalRCommand> Commands { get; internal set; } = [];
    public IEnumerable<SignalREvent> Events { get; internal set; } = [];
    public string Name => Declaration?.Identifier.Text ?? "";
}