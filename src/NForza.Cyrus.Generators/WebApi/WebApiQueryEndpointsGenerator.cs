using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.WebApi;

public class WebApiQueryEndpointsGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider, LiquidEngine liquidEngine)
    {
        var config = cyrusProvider.GenerationConfig;
        if (config != null && config.GenerationTarget.Contains(GenerationTarget.WebApi))
        {
            IEnumerable<IMethodSymbol> validators = cyrusProvider.Validators;
            var contents = AddQueryHandlerMappings(spc, cyrusProvider.AllQueriesAndHandlers.OfType<IMethodSymbol>(), validators);

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

            IEnumerable<INamedTypeSymbol> queries = cyrusProvider.AllQueriesAndHandlers
                .Where(q => q.IsQuery())
                .OfType<INamedTypeSymbol>();

            AddHttpContextObjectFactoryMethodsRegistrations(spc, queries);
            WebApiContractGenerator.GenerateContracts(queries, spc, LiquidEngine);
        }
    }

    private void AddHttpContextObjectFactoryMethodsRegistrations(SourceProductionContext sourceProductionContext, IEnumerable<INamedTypeSymbol> queries)
    {
        var model = new
        {
            Queries = queries.Select(q =>
                new
                {
                    q.Name,
                    TypeName = q.ToFullName(),
                    Properties = q.GetPublicProperties().Select(p => new { p.Name, Type = p.Type.ToFullName() })
                })
        };
        var httpContextObjectFactoryInitialization = LiquidEngine.Render(model, "HttpContextObjectFactoryQuery");

        var initModel = new { Namespace = "WebApi", Name = "HttpContextObjectFactoryQueryInitializer", Initializer = httpContextObjectFactoryInitialization };
        var source = LiquidEngine.Render(initModel, "CyrusInitializer");
        sourceProductionContext.AddSource($"HttpContextObjectFactoryQueries.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private string AddQueryHandlerMappings(SourceProductionContext sourceProductionContext, IEnumerable<IMethodSymbol> handlers, IEnumerable<IMethodSymbol> validators)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var handler in handlers)
        {
            IMethodSymbol validator = validators.FirstOrDefault(v => v.Parameters[0].Type.ToFullName() == handler.Parameters[0].Type.ToFullName());
            var query = new
            {
                Path = TypeAnnotations.AugmentRouteWithTypeAnnotations(handler.GetQueryRoute(), handler.Parameters[0].Type),
                Query = handler.Parameters[0].Type.ToFullName(),
                ReturnsTask = handler.ReturnType.IsTaskType(),
                QueryInvocation = handler.GetQueryInvocation(),
                ValidatorMethod = validator,
                ValidatorMethodIsStatic = validator.IsStatic,
                ValidatorMethodName = validator?.Name,
                ValidatorClass = validator?.ContainingType?.ToFullName(),
                ReturnType = handler.ReturnsTask(out var taskResultType) ? taskResultType!.ToFullName() : handler.ReturnType.ToFullName(),
                Attributes = handler.GetAttributes().Select(a => a.ToNewInstanceString()).Where(a => a != null)
            };
            sb.AppendLine(LiquidEngine.Render(query, "MapQuery"));
        }
        return sb.ToString().Trim();
    }
}