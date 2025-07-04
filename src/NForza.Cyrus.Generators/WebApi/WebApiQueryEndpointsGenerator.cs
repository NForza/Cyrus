﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.WebApi;

public class WebApiQueryEndpointsGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var config = cyrusGenerationContext.GenerationConfig;
        if (config != null && config.GenerationTarget.Contains(GenerationTarget.WebApi))
        {
            IEnumerable<IMethodSymbol> validators = cyrusGenerationContext.Validators;
            IEnumerable<IMethodSymbol> queryHandlers = 
                cyrusGenerationContext
                    .AllQueriesAndHandlers
                    .OfType<IMethodSymbol>()
                    .Where(h => h.HasQueryRoute());
            var contents = AddQueryHandlerMappings(spc, queryHandlers, validators, cyrusGenerationContext.LiquidEngine);

            if (!string.IsNullOrWhiteSpace(contents))
            {
                var ctx = new
                {
                    Usings = new string[] {
                        "System.Collections.Generic",
                        "System.Linq",
                        "System.Threading",
                        "Microsoft.AspNetCore.Builder",
                        "Microsoft.AspNetCore.Mvc",
                        "Microsoft.AspNetCore.Http",
                        "Microsoft.Extensions.DependencyInjection",
                    },
                    Namespace = "WebApiQueries",
                    Name = "Query",
                    StartupCommands = contents
                };

                var fileContents = cyrusGenerationContext.LiquidEngine.Render(ctx, "CyrusWebStartup");
                spc.AddSource("QueryHandlerMapping.g.cs", fileContents);
            }

            IEnumerable<INamedTypeSymbol> queries = cyrusGenerationContext.AllQueriesAndHandlers
                .Where(q => q.IsQuery())
                .OfType<INamedTypeSymbol>();

            AddHttpContextObjectFactoryMethodsRegistrations(spc, queries, cyrusGenerationContext.LiquidEngine);
            WebApiContractGenerator.GenerateContracts(queries, spc, cyrusGenerationContext.LiquidEngine);
        }
    }

    private void AddHttpContextObjectFactoryMethodsRegistrations(SourceProductionContext sourceProductionContext, IEnumerable<INamedTypeSymbol> queries, LiquidEngine liquidEngine)
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
        var httpContextObjectFactoryInitialization = liquidEngine.Render(model, "HttpContextObjectFactoryQuery");

        var initModel = new { 
            Namespace = "WebApi", 
            Name = "HttpContextObjectFactoryQueryInitializer",
            Usings = new string[] { "System.Linq", "NForza.Cyrus.Abstractions" },
            Initializer = httpContextObjectFactoryInitialization 
        };
        var source = liquidEngine.Render(initModel, "CyrusInitializer");
        sourceProductionContext.AddSource($"HttpContextObjectFactoryQueries.g.cs", source);
    }

    private string AddQueryHandlerMappings(SourceProductionContext sourceProductionContext, IEnumerable<IMethodSymbol> handlers, IEnumerable<IMethodSymbol> validators, LiquidEngine liquidEngine)
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
                ValidatorMethodIsStatic = validator?.IsStatic,
                ValidatorMethodName = validator?.Name,
                ValidatorClass = validator?.ContainingType?.ToFullName(),
                ReturnType = handler.ReturnsTask(out var taskResultType) ? taskResultType!.ToFullName() : handler.ReturnType.ToFullName(),
                Attributes = handler.GetAttributes().Select(a => a.ToNewInstanceString()).Where(a => a != null)
            };
            sb.AppendLine(liquidEngine.Render(query, "MapQuery"));
        }
        return sb.ToString().Trim();
    }
}