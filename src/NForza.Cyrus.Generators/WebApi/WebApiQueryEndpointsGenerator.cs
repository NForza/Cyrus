using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
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
                var contents = AddQueryHandlerRegistrations(spc, queriesAndHandlers.OfType<IMethodSymbol>());

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

                AddQueryFactoryMethodsRegistrations(spc, queries);
            }
        });
    }

    private void AddQueryFactoryMethodsRegistrations(SourceProductionContext sourceProductionContext, IEnumerable<INamedTypeSymbol> queries)
    {
        string queryFactoryRegistrations = GenerateQueryFactoryExtensionMethods(queries);

        queryFactoryRegistrations = $@"HttpContextObjectFactory x = new HttpContextObjectFactory();
             {queryFactoryRegistrations}
             services.AddSingleton<IHttpContextObjectFactory>(x);";
        var source = LiquidEngine.Render(new { Namespace = "WebApi", Name = "QueryFactoryRegistration", Initializer = queryFactoryRegistrations }, "CyrusInitializer");
        sourceProductionContext.AddSource($"QueryFactoryMethodRegistrations.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private string GenerateQueryFactoryExtensionMethods(IEnumerable<INamedTypeSymbol> queries)
    {
        StringBuilder source = new();
        foreach (var query in queries)
        {
            var queryTypeName = query.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            source.Append($@"    x.Register<{queryTypeName}>(ctx => {GetConstructionExpression(query)}");
        }

        return source.ToString();
    }

    private string GetConstructionExpression(INamedTypeSymbol query)
    {
        static IEnumerable<IPropertySymbol> GetPublicProperties(INamedTypeSymbol namedTypeSymbol)
        {
            return namedTypeSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.DeclaredAccessibility == Accessibility.Public);
        }

        var queryTypeName = query.ToFullName();
        var ctor = new StringBuilder(@$"new {queryTypeName}");
        var constructorProperties = GenerateConstructorParameters(query, ctor);
        var propertiesToInitialize = GetPublicProperties(query).Where(p => !constructorProperties.Contains(p.Name)).ToList();
        if (propertiesToInitialize.Count > 0)
        {
            ctor.Append("{");
            var propertyInitializer = new List<string>();
            foreach (var prop in propertiesToInitialize)
            {
                propertyInitializer.Add(@$"{prop.Name} = ({prop.Type.ToFullName()})x.GetPropertyValue(""{prop.Name}"", ctx, typeof({prop.Type}))");
            }
            ctor.Append(string.Join(", ", propertyInitializer));
            ctor.Append("}");
        }
        ctor.AppendLine(");");
        return ctor.ToString();
    }

    private List<string> GenerateConstructorParameters(INamedTypeSymbol query, StringBuilder ctor)
    {
        var constructorWithLeastParameters = query.Constructors
                .Where(c => c.DeclaredAccessibility == Accessibility.Public)
                .OrderBy(c => c.Parameters.Length)
                .FirstOrDefault();
        if (constructorWithLeastParameters == null)
        {
            return [];
        }
        ctor.Append("(");

        var result = new List<string>();
        var firstParam = constructorWithLeastParameters.Parameters.FirstOrDefault();
        foreach (var param in constructorWithLeastParameters.Parameters)
        {
            if (!param.Equals(firstParam, SymbolEqualityComparer.Default))
            {
                ctor.Append(", ");
            }
            ctor.Append($"({param.Type.ToFullName()})x.GetPropertyValue(\"{param.Name}\", ctx, typeof({param.Type.ToFullName()}))");
            result.Add(param.Name);
        }
        ctor.Append(")");
        return result;
    }

    private string AddQueryHandlerRegistrations(SourceProductionContext sourceProductionContext, IEnumerable<IMethodSymbol> handlers)
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