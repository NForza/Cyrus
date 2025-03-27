using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Generators.SignalR;

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
            SetQueries(Symbol, constructorBody);
        }
        return this;
    }

    private void SetCommands(INamedTypeSymbol symbol, BlockSyntax constructorBody)
    {
        var memberAccessExpressionSyntaxes = GetMethodCallsOf(constructorBody, "Command")
                .Select(ies => ies.Expression)
                .OfType<GenericNameSyntax>();
        var commands = memberAccessExpressionSyntaxes.Select(name => name.TypeArgumentList.Arguments.Single());
        Commands = commands.Select(genericArg =>
        {
            var symbol = (SemanticModel.GetSymbolInfo(genericArg).Symbol as ITypeSymbol)!;
            return new SignalRCommand
            {
                MethodName = genericArg.GetText().ToString(),
                Name = symbol.Name,
                Handler = GetCommandHandler(symbol) ?? throw new InvalidCastException("Can't find return type for " + symbol.ToFullName()),
                FullTypeName = symbol.ToFullName()
            };
        });
    }

    private void SetQueries(INamedTypeSymbol symbol, BlockSyntax constructorBody)
    {
        var memberAccessExpressionSyntaxes = GetMethodCallsOf(constructorBody, "Query")
                .Select(ies => ies.Expression)
                .OfType<GenericNameSyntax>();
        var queries = memberAccessExpressionSyntaxes.Select(name => name.TypeArgumentList.Arguments.Single());

        Queries = queries.Select(genericArg =>
        {
            var symbol = (ITypeSymbol)SemanticModel.GetSymbolInfo(genericArg).Symbol!;
            ITypeSymbol returnType = GetReturnTypeOfQuery(symbol) ?? throw new InvalidCastException("Can't find return type for " + symbol.ToFullName());
            (bool isCollection, ITypeSymbol? collectionType) = returnType?.IsCollection() ?? (false, null);
            var isNullable = returnType?.IsNullable();
            ITypeSymbol? queryReturnType = isCollection ? collectionType : returnType;
            var propertyModelsOfReturnType = queryReturnType!.GetPropertyModels();
            return
                new SignalRQuery
                {
                    MethodName = genericArg.GetText().ToString(),
                    Name = symbol.Name,
                    FullTypeName = symbol.ToFullName(),
                    ReturnType = new(queryReturnType!.Name, propertyModelsOfReturnType, [], isCollection, isNullable ?? false) //, [])
                };
        });
    }

    private ITypeSymbol? GetReturnTypeOfQuery(ITypeSymbol symbol)
    {
        Compilation compilation = SemanticModel.Compilation;
        foreach (var reference in compilation.References)
        {
            var assemblySymbol = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
            if (assemblySymbol == null)
                continue;

            foreach (var type in assemblySymbol.GlobalNamespace.GetAllTypes())
            {
                foreach (var method in type.GetMembers().OfType<IMethodSymbol>())
                {
                    if (method.Parameters.Length == 1 && method.IsQueryHandler() &&
                        SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, symbol))
                    {
                        var returnType = method.ReturnType;
                        if (returnType is INamedTypeSymbol namedType &&
                            namedType.IsGenericType &&
                            (namedType.ConstructedFrom.ToDisplayString() == "System.Threading.Tasks.Task<TResult>" ||
                             namedType.ConstructedFrom.ToDisplayString() == "System.Threading.Tasks.ValueTask<TResult>"))
                        {
                            return namedType.TypeArguments[0];
                        }

                        return returnType;
                    }
                }
            }
        }

        return null;
    }

    private IMethodSymbol? GetCommandHandler(ITypeSymbol symbol)
    {
        Compilation compilation = SemanticModel.Compilation;
        foreach (var reference in compilation.References)
        {
            var assemblySymbol = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
            if (assemblySymbol == null)
                continue;

            foreach (var type in assemblySymbol.GlobalNamespace.GetAllTypes())
            {
                foreach (var method in type.GetMembers().OfType<IMethodSymbol>())
                {
                    if (method.Parameters.Length == 1 && method.IsCommandHandler() &&
                        SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, symbol))
                    {
                        return method;
                    }
                }
            }
        }

        return null;
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
    public IEnumerable<SignalRQuery> Queries { get; internal set; } = [];
    public string Name => Declaration?.Identifier.Text ?? "";
}
