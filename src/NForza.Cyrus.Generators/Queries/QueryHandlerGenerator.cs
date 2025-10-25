using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.Queries;

public class QueryHandlerGenerator : CyrusGeneratorBase
{
    override public void GenerateSource(SourceProductionContext context, CyrusGenerationContext cyrusGenerationContext)
    {
        var queryHandlers = cyrusGenerationContext.QueryHandlers;

        if (queryHandlers.Any())
        {
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            var queryHandlerRegistrations = string.Join(Environment.NewLine, queryHandlers
                .Select(x => x.ContainingType)
                .Where(x => x != null)
                .Where(x => !x.IsStatic)
                .Distinct(SymbolEqualityComparer.Default)
                .Select(qht => $" services.AddTransient<{qht!.ToFullName()}>();"));
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
            var ctx = new
            {
                Usings = new string[] { "NForza.Cyrus.Cqrs", "NForza.Cyrus.Abstractions" },
                Namespace = "QueryHandlers",
                Name = "QueryHandlersRegistration",
                Initializer = queryHandlerRegistrations
            };
            var fileContents = cyrusGenerationContext.LiquidEngine.Render(ctx, "CyrusInitializer");
            context.AddSource("QueryHandlerRegistration.g.cs", fileContents);

            var sourceText = GenerateQueryProcessorExtensionMethods(queryHandlers, cyrusGenerationContext.Compilation, cyrusGenerationContext.LiquidEngine);
            context.AddSource($"QueryProcessor.g.cs", sourceText);
        }
    }

    private string GenerateQueryProcessorExtensionMethods(ImmutableArray<IMethodSymbol> handlers, Compilation compilation, LiquidEngine liquidEngine)
    {
        INamedTypeSymbol? taskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        if (taskSymbol == null)
        {
            return string.Empty;
        }

        var model = new
        {
            Queries = handlers.Select(q => new
            {
                ReturnTypeOriginal = q.ReturnType,
                ReturnType = q.ReturnType.IsTaskType() ? ((INamedTypeSymbol)q.ReturnType).TypeArguments[0].ToFullName() : q.ReturnType.ToFullName(),
                Invocation = q.GetQueryInvocation(serviceProviderVariable: "queryProcessor.ServiceProvider"),
                q.Name,
                QueryType = q.Parameters[0].Type.ToFullName(),
                ReturnsTask = q.ReturnType.IsTaskType(),
            }).ToList()
        };

        var resolvedSource = liquidEngine.Render(model, "QueryProcessorExtensions");

        return resolvedSource;
    }
}