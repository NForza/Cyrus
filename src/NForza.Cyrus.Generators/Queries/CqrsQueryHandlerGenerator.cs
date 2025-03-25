using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.Queries;

public class QueryHandlerGenerator : CyrusGeneratorBase
{
    override public void GenerateSource(SourceProductionContext context, CyrusGenerationContext cyrusProvider, LiquidEngine liquidEngine)
    {
        var queryHandlers = cyrusProvider.QueryHandlers;

        if (queryHandlers.Any())
        {
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            var queryHandlerRegistrations = string.Join(Environment.NewLine, queryHandlers
                .Select(x => x.ContainingType)
                .Where(x => x != null)
                .Distinct(SymbolEqualityComparer.Default)
                .Select(qht => $" services.AddTransient<{qht.ToFullName()}>();"));
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
            var ctx = new
            {
                Usings = new string[] { "NForza.Cyrus.Cqrs" },
                Namespace = "QueryHandlers",
                Name = "QueryHandlersRegistration",
                Initializer = queryHandlerRegistrations
            };
            var fileContents = LiquidEngine.Render(ctx, "CyrusInitializer");
            context.AddSource("QueryHandlerRegistration.g.cs", SourceText.From(fileContents, Encoding.UTF8));

            var sourceText = GenerateQueryProcessorExtensionMethods(queryHandlers, cyrusProvider.Compilation);
            context.AddSource($"QueryProcessor.g.cs", SourceText.From(sourceText, Encoding.UTF8));
        }
    }

    private string GenerateQueryProcessorExtensionMethods(ImmutableArray<IMethodSymbol> handlers, Compilation compilation)
    {
        INamedTypeSymbol? taskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        if (taskSymbol == null)
        {
            return string.Empty;
        }

        var queries = handlers.Select(h => new
        {
            Handler = h,
            QueryType = h.Parameters[0].Type.ToFullName(),
            h.Name,
            ReturnType = (INamedTypeSymbol)h.ReturnType,
            IsAsync = h.ReturnType.OriginalDefinition.Equals(taskSymbol, SymbolEqualityComparer.Default)
        }).ToList();

        var model = new
        {
            Queries = queries.Select(q => new
            {
                ReturnTypeOriginal = q.ReturnType,
                ReturnType = q.ReturnType.IsTaskType() ? q.ReturnType.TypeArguments[0].ToFullName() : q.ReturnType.ToFullName(),
                Invocation = q.Handler.GetQueryInvocation(serviceProviderVariable: "queryProcessor.ServiceProvider"),
                q.Name,
                q.QueryType,
                ReturnsTask = q.ReturnType.IsTaskType(),
            }).ToList()
        };

        var resolvedSource = LiquidEngine.Render(model, "QueryProcessorExtensions");

        return resolvedSource;
    }
}