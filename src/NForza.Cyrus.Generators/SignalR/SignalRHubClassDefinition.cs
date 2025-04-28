using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.SignalR;

public record SignalRHubClassDefinition
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
        Path = usePathArgument != null ? usePathArgument.Trim('\"') : symbol.Name.ToLower();
    }

    public SignalRHubClassDefinition(ClassDeclarationSyntax declaration, INamedTypeSymbol symbol, SemanticModel semanticModel)
    {
        Declaration = declaration;
        Symbol = symbol;
        SemanticModel = semanticModel;
    }

    public SignalRHubClassDefinition Initialize(CyrusGenerationContext cyrusProvider)
    {
        var constructorBody = Declaration.DescendantNodes()
            .OfType<ConstructorDeclarationSyntax>()
            .Where(constructorDeclarationSyntax => constructorDeclarationSyntax.Body != null)
            .FirstOrDefault()?.Body;

        if (constructorBody != null)
        {
            SetPath(Symbol, constructorBody);
            SetCommands(Symbol, constructorBody, cyrusProvider);
            SetQueries(Symbol, constructorBody, cyrusProvider);
            SetEvents(Symbol, constructorBody);
        }
        return this;
    }

    private void SetCommands(INamedTypeSymbol symbol, BlockSyntax constructorBody, CyrusGenerationContext cyrusProvider)
    {
        var memberAccessExpressionSyntaxes = GetMethodCallsOf(constructorBody, "Expose")
                .Select(ies => ies.Expression)
                .OfType<GenericNameSyntax>();
        var commands = memberAccessExpressionSyntaxes
            .Select(name => name.TypeArgumentList.Arguments.Single());
        Commands = commands
            .Select(genericArg =>
            {
                var commandSymbol = (ITypeSymbol?)SemanticModel.GetSymbolInfo(genericArg).Symbol;
                if (commandSymbol == null || !commandSymbol.IsCommand())
                    return null;

                return new SignalRCommand
                {
                    MethodName = genericArg.GetText().ToString(),
                    Name = symbol.Name,
                    Handler = GetCommandHandler(commandSymbol, cyrusProvider),
                    FullTypeName = commandSymbol.ToFullName()
                };
            })
            .Where(command => command != null)
            .Cast<SignalRCommand>();
    }

    private void SetQueries(INamedTypeSymbol symbol, BlockSyntax constructorBody, CyrusGenerationContext cyrusProvider)
    {
        var memberAccessExpressionSyntaxes = GetMethodCallsOf(constructorBody, "Expose")
                .Select(ies => ies.Expression)
                .OfType<GenericNameSyntax>();
        var queries = memberAccessExpressionSyntaxes.Select(name => name.TypeArgumentList.Arguments.Single());

        IEnumerable<SignalRQuery> signalRQueries = queries
            .Select(genericArg =>
            {
                var querySymbol = (ITypeSymbol?)SemanticModel.GetSymbolInfo(genericArg).Symbol;
                if (querySymbol == null || !querySymbol.IsQuery())
                    return null;
                ITypeSymbol? returnType = GetReturnTypeOfQuery(querySymbol, cyrusProvider);
                (bool isCollection, ITypeSymbol? collectionType) = returnType?.IsCollection() ?? (false, null);
                var isNullable = returnType?.IsNullable();
                ITypeSymbol? queryReturnType = isCollection ? collectionType : returnType;
                var propertyModelsOfReturnType = queryReturnType!.GetPropertyModels();
                return
                    new SignalRQuery
                    {
                        MethodName = genericArg.GetText().ToString(),
                        Name = querySymbol.Name,
                        FullTypeName = querySymbol.ToFullName(),
                        ReturnType = new(queryReturnType!.Name, propertyModelsOfReturnType, [], isCollection, isNullable ?? false) //, [])
                    };
            })
            .Where(query => query != null)
            .Cast<SignalRQuery>();

        Queries = signalRQueries
            .Where(query => query != null)
            .Cast<SignalRQuery>();
    }

    private ITypeSymbol? GetReturnTypeOfQuery(ITypeSymbol symbol, CyrusGenerationContext cyrusProvider)
    {
        var queryHandler = cyrusProvider.AllQueryHandlers.FirstOrDefault(handler => SymbolEqualityComparer.Default.Equals(handler.Parameters[0].Type, symbol));
        if (queryHandler != null)
        {
            return queryHandler.GetQueryReturnType();
        }

        return null;
    }

    private IMethodSymbol? GetCommandHandler(ITypeSymbol symbol, CyrusGenerationContext cyrusProvider)
        => cyrusProvider.AllCommandHandlers.FirstOrDefault(handler => SymbolEqualityComparer.Default.Equals(handler.Parameters[0].Type, symbol));

    private void SetEvents(INamedTypeSymbol symbol, BlockSyntax constructorBody)
    {
        var memberAccessExpressionSyntaxes = GetMethodCallsOf(constructorBody, "Emit")
                .Select(ies => ies.Expression)
                .OfType<GenericNameSyntax>();
        var callerEventTypes = memberAccessExpressionSyntaxes.Select(name => name.TypeArgumentList.Arguments.Single());
        var callerEvents = callerEventTypes.Select(genericArg =>
        {
            var symbol = SemanticModel.GetSymbolInfo(genericArg).Symbol!;
            return new SignalREvent { MethodName = genericArg.GetText().ToString(), Name = symbol.Name, FullTypeName = symbol.ToFullName(), Broadcast = false };
        });

        memberAccessExpressionSyntaxes = GetMethodCallsOf(constructorBody, "Broadcast")
                .Select(ies => ies.Expression)
                .OfType<GenericNameSyntax>();
        var broadcastEventTypes = memberAccessExpressionSyntaxes.Select(name => name.TypeArgumentList.Arguments.Single());
        var broadcastEvents = broadcastEventTypes.Select(genericArg =>
        {
            var symbol = SemanticModel.GetSymbolInfo(genericArg).Symbol!;
            return new SignalREvent { MethodName = genericArg.GetText().ToString(), Name = symbol.Name, FullTypeName = symbol.ToFullName(), Broadcast = true };
        });

        Events = callerEvents.Concat(broadcastEvents);
    }


    public string Path { get; private set; } = "";
    public ClassDeclarationSyntax Declaration { get; }
    public INamedTypeSymbol Symbol { get; }
    public SemanticModel SemanticModel { get; }
    public IEnumerable<SignalRCommand> Commands { get; internal set; } = [];
    public IEnumerable<SignalREvent> Events { get; internal set; } = [];
    public IEnumerable<SignalRQuery> Queries { get; internal set; } = [];
    public string Name => Declaration?.Identifier.Text ?? "";
}
