using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Analyzers;

internal class CommandAnalyzer : CyrusAnalyzerBase
{
    public override void AnalyzeMethodSymbol(SymbolAnalysisContext context, IMethodSymbol methodSymbol)
    {
        var isCommandHandler = methodSymbol.IsCommandHandler();

        if (!isCommandHandler)
            return;

        var location = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax()?.GetLocation()
                       ?? Location.None;

        if (methodSymbol.Parameters.Length == 0)
        {
            location = methodSymbol.Parameters[0].Locations.FirstOrDefault()
                       ?? Location.None;
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.TooManyArgumentsForCommandHandler,
                location,
                methodSymbol.ToDisplayString());
            context.ReportDiagnostic(diagnostic);
            return;
        }

        var commandParam = methodSymbol.Parameters[0].Type;
        var hasCommandAttr = commandParam.GetAttributes()
            .Any(attr => attr.AttributeClass?.Name == "CommandAttribute"
                      || attr.AttributeClass?.ToDisplayString() == "NForza.Cyrus.Abstractions");

        if (!hasCommandAttr)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.MissingCommandAttribute,
                location,
                commandParam.Name);
            context.ReportDiagnostic(diagnostic);
        }

        var parameters = methodSymbol.Parameters.Skip(1).ToList();

        var aggregateRootFound = false;
        var cancellationTokenFound = false;
        foreach (var param in parameters)
        {
            location = param.Locations.FirstOrDefault() ?? Location.None;

            var isAggregateRoot = param.Type is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsAggregateRoot();
            if (isAggregateRoot)
            {
                if (aggregateRootFound)
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.CommandHandlerCantHaveMultipleAggregateRootParameters,
                        param.Locations.FirstOrDefault() ?? location,
                        methodSymbol.ToDisplayString());
                    context.ReportDiagnostic(diagnostic);
                    return;
                }
                aggregateRootFound = true;

                var firstParameter = methodSymbol.Parameters[0];
                var commandHasAggregateRootId = firstParameter.Type is INamedTypeSymbol parameterType && parameterType.GetAggregateRootIdProperty() != null;
                if (!commandHasAggregateRootId)
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.CommandForCommandHandlerShouldHaveAggregateRootIdProperty,
                        firstParameter.Locations.FirstOrDefault() ?? location,
                        methodSymbol.ToDisplayString());
                    context.ReportDiagnostic(diagnostic);
                    return;
                }
            }

            var cancellationToken = param.Type is INamedTypeSymbol cancellationTokenTypeSymbol && cancellationTokenTypeSymbol.IsCancellationToken();
            if (cancellationTokenFound)
            {
                if (cancellationTokenFound)
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.CommandHandlerCantHaveMultipleCancellationTokenParameters,
                        param.Locations.FirstOrDefault() ?? location,
                        methodSymbol.ToDisplayString());
                    context.ReportDiagnostic(diagnostic);
                    return;
                }
                cancellationTokenFound = true;
            }

            if (methodSymbol.Parameters.Length > 3)
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.TooManyArgumentsForCommandHandler,
                    location,
                    methodSymbol.ToDisplayString());
                context.ReportDiagnostic(diagnostic);
                return;
            }

            if (methodSymbol.ReturnType is INamedTypeSymbol t && t.IsTupleType)
            {
                foreach (var te in t.TupleElements)
                {
                    if (te.Type.ToDisplayString() == "Microsoft.AspNetCore.Http.IResult" &&
                        te.Name != "Result")
                    {
                        location = te.Locations.FirstOrDefault() ?? Location.None;
                        var diagnostic = Diagnostic.Create(
                            DiagnosticDescriptors.IResultTupleElementShouldBeCalledResult,
                            location,
                            te.ToDisplayString());
                        context.ReportDiagnostic(diagnostic);
                    }

                    if ((te.Type.ToDisplayString() == "object" || te.Type.ToDisplayString() == "System.Collections.Generic.IEnumerable<object>") &&
                        te.Name != "Messages")
                    {
                        location = te.Locations.FirstOrDefault() ?? Location.None;
                        var diagnostic = Diagnostic.Create(
                            DiagnosticDescriptors.MessagesTupleElementShouldBeCalledMessages,
                            location,
                            te.ToDisplayString());
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}