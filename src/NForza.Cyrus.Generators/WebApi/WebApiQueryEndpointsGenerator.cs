using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.WebApi;

[Generator]
public class QueryEndpointsGenerator : CyrusGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(true);

        var configProvider = ConfigProvider(context);

        var compilationProvider = context.CompilationProvider;

        var queriesAndQueryHandlerProvider = compilationProvider
            .SelectMany((compilation, _) =>
            {
                var allTypes = compilation.GetAllTypesFromCyrusAssemblies();

                var queries = allTypes
                    .Where(t => t.IsQuery())
                    .ToList();

                var queryHandlers = allTypes
                    .SelectMany(t => t.GetMembers().OfType<IMethodSymbol>()
                    .Where(m => m.IsQueryHandler()))
                    .ToList();

                return queries.Cast<ISymbol>().Concat(queryHandlers);
            })
           .Collect();

        var combinedProvider = context
            .CompilationProvider
            .Combine(queriesAndQueryHandlerProvider)
            .Combine(configProvider);

        context.RegisterSourceOutput(combinedProvider, (spc, source) =>
        {
            var ((compilation, queriesAndHandlers), config) = source;

            if (config != null && config.GenerationTarget.Contains(GenerationTarget.WebApi))
            {
                var contents = AddQueryHandlerMappings(spc, queriesAndHandlers.OfType<IMethodSymbol>());

                if (!string.IsNullOrWhiteSpace(contents))
                {
                    var ctx = new
                    {
                        Usings = new string[] {
                            "Microsoft.AspNetCore.Mvc",
                            "Microsoft.AspNetCore.Http"
                    },
                        Namespace = "WebApiQueries",
                        Name = "Query",
                        StartupCommands = contents
                    };

                    var fileContents = LiquidEngine.Render(ctx, "CyrusWebStartup");
                    spc.AddSource(
                       "QueryHandlerMapping.g.cs",
                       SourceText.From(fileContents, Encoding.UTF8));
                }

                IEnumerable<INamedTypeSymbol> queries = queriesAndHandlers
                    .Where(q => q.IsQuery())
                    .OfType<INamedTypeSymbol>();

                AddHttpContextObjectFactoryMethodsRegistrations(spc, queries);          
                WebApiContractGenerator.GenerateContracts(queries, spc, LiquidEngine);
            }
        });
    }

    private void AddHttpContextObjectFactoryMethodsRegistrations(SourceProductionContext sourceProductionContext, IEnumerable<INamedTypeSymbol> queries)
    {
        var model = new 
        { 
            Queries = queries.Select( q => 
                new 
                { 
                    TypeName = q.ToFullName(),
                    Properties = q.GetPublicProperties().Select(p => new { Name = p.Name, Type = p.Type.ToFullName() })
                }) };
        var httpContextObjectFactoryInitialization = LiquidEngine.Render(model, "HttpContextObjectFactoryQuery");

        var initModel = new { Namespace = "WebApi", Name = "HttpContextObjectFactoryQueryInitializer", Initializer = httpContextObjectFactoryInitialization };
        var source = LiquidEngine.Render(initModel, "CyrusInitializer");
        sourceProductionContext.AddSource($"HttpContextObjectFactoryQueries.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private string AddQueryHandlerMappings(SourceProductionContext sourceProductionContext, IEnumerable<IMethodSymbol> handlers)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var handler in handlers)
        {
            var query = new
            {
                Path = handler.GetQueryHandlerRoute(),
                Query = handler.Parameters[0].Type.ToFullName(),
                IsAsync = handler.IsAsync(),
                QueryInvocation = handler.GetQueryInvocation()
            };
            sb.AppendLine(LiquidEngine.Render(query, "MapQuery"));
        }
        return sb.ToString().Trim();
    }
}