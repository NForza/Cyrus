using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Cqrs;

[Generator]
public class CqrsQueryHandlerGenerator : CyrusGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);

        var incrementalValuesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => syntaxNode.IsQueryHandler(),
                transform: (context, _) => context.GetMethodSymbolFromContext());

        var allQueryHandlersProvider = incrementalValuesProvider
            .Where(x => x is not null)
            .Select((x, _) => x!)
            .Collect();

        var combinedProvider = allQueryHandlersProvider.Combine(context.CompilationProvider);

        context.RegisterSourceOutput(combinedProvider, (spc, queryHandlersWithCompilation) =>
        {
            var (queryHandlers, compilation) = queryHandlersWithCompilation;
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
                spc.AddSource("QueryHandlerRegistration.g.cs", SourceText.From(fileContents, Encoding.UTF8));

                var sourceText = GenerateQueryProcessorExtensionMethods(queryHandlers, compilation);
                spc.AddSource($"QueryProcessor.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            }
        });
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
            Name = h.Name,
            ReturnType = (INamedTypeSymbol)h.ReturnType,
            IsAsync = h.ReturnType.OriginalDefinition.Equals(taskSymbol, SymbolEqualityComparer.Default)
        }).ToList();

        var model = new
        {
            Queries = queries.Select(q => new
            {
                ReturnTypeOriginal = q.ReturnType,
                ReturnType = q.IsAsync ? q.ReturnType.TypeArguments[0].ToFullName() : q.ReturnType.ToFullName(),
                Invocation = q.Handler.GetQueryInvocation(serviceProviderVariable: "queryProcessor.ServiceProvider"),
                Name = q.Name,
                q.QueryType,
                q.IsAsync
            }).ToList()
        };

        var resolvedSource = LiquidEngine.Render(model, "QueryProcessorExtensions");

        return resolvedSource;
    }
}