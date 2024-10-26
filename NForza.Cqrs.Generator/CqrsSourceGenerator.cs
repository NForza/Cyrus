using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Generators;

#pragma warning disable RS1035 // Do not use banned APIs for analyzers

namespace NForza.Cqrs.Generator;

public abstract class CqrsSourceGenerator : GeneratorBase
{
    protected List<IMethodSymbol> GetAllCommandHandlers(GeneratorExecutionContext context, string methodHandlerName, List<INamedTypeSymbol> commands) =>
        context.Compilation
            .GetSymbolsWithName(s => s == methodHandlerName, SymbolFilter.Member)
            .OfType<IMethodSymbol>()
            .Where(m => m.Parameters.Length == 1 && commands.Contains(m.Parameters[0].Type, SymbolEqualityComparer.Default))
            .Where(m => ReturnsTaskOfCommandResult(context, m))
            .ToList();

    protected IEnumerable<INamedTypeSymbol> GetAllCommands(Compilation compilation, IEnumerable<string> contractProjectSuffixes, string commandSuffix)
    {
        static bool IsStruct(INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.IsValueType && typeSymbol.TypeKind == TypeKind.Struct;
        }

        var commandsInDomain = compilation.GetSymbolsWithName(s => s.EndsWith(commandSuffix), SymbolFilter.Type)
            .OfType<INamedTypeSymbol>()
            .Where(t => t.IsRecord)
            .ToList();

        foreach (var command in commandsInDomain)
        {
            yield return command;
        }

        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assemblySymbol
                ||
                assemblySymbol.Name.StartsWith("System")
                ||
                assemblySymbol.Name.StartsWith("Microsoft")
                ||
                !contractProjectSuffixes.Any(assemblySymbol.Name.EndsWith))
                continue;

            var allTypes = GetAllTypes(assemblySymbol.GlobalNamespace);

            foreach (var t in allTypes)
            {
                if (IsStruct(t))
                    yield return t;
            }
        }
    }

    private static bool ReturnsTaskOfCommandResult(GeneratorExecutionContext context, IMethodSymbol handlerType)
    {
        Debug.WriteLine($"Found handler: {handlerType.Name}");

        if (IsTaskOfCommandResult(handlerType.ReturnType, context.Compilation))
        {
            return true;
        }
        else
        {
            context.ReportDiagnostic(Diagnostic.Create(
                 new DiagnosticDescriptor(
                     "SG0001",
                     "Incorrect return type for command handler",
                     "Method {0} returns {1}. All methods must return Task<CommandResult>.",
                     "NForza.Cqrs",
                     DiagnosticSeverity.Error,
                     true), handlerType.Locations.FirstOrDefault(), handlerType.Name, handlerType.ReturnType.Name));
            return false;
        }
    }

    private static bool IsTaskOfCommandResult(ITypeSymbol returnType, Compilation compilation)
    {
        if (returnType is INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.IsGenericType &&
                    namedTypeSymbol.ConstructedFrom.ToDisplayString() == "System.Threading.Tasks.Task<TResult>")
            {

                var genericArguments = namedTypeSymbol.TypeArguments;
                if (genericArguments.Length == 1)
                {
                    var commandResultSymbol = compilation.GetTypeByMetadataName("NForza.Cqrs.CommandResult");
                    return SymbolEqualityComparer.Default.Equals(genericArguments[0], commandResultSymbol);
                }
            }
        }
        return false;
    }

    public override void Initialize(GeneratorInitializationContext context)
    {
    }
}