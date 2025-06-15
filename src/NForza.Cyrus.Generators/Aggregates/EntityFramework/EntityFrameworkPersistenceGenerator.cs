using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Aggregates.EntityFramework;

public class EntityFrameworkPersistenceGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var config = cyrusGenerationContext.GenerationConfig;
        if (!string.IsNullOrEmpty(config.PersistenceContextType) &&  cyrusGenerationContext.GenerationConfig.GenerationTarget.Contains(GenerationTarget.WebApi) && cyrusGenerationContext.AggregateRoots.Any())
        {
            var sourceText = GenerateEntityFrameworkPersistenceInitializer(cyrusGenerationContext);
            spc.AddSource($"EntityFrameworkPersistenceInitializer.g.cs", sourceText);
        }
    }

    private string GenerateEntityFrameworkPersistenceInitializer(CyrusGenerationContext cyrusGenerationContext)
    {
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
        var persistenceRegistrations = string.Join(Environment.NewLine, cyrusGenerationContext.AggregateRoots.Select(v =>
                $"services.AddScoped<IAggregateRootPersistence<{v.Symbol.ToFullName()}, {v.AggregateRootProperty.Type.ToFullName()}>, EntityFrameworkPersistence<{v.Symbol.ToFullName()}, {v.AggregateRootProperty.Type.ToFullName()}, {cyrusGenerationContext.GenerationConfig.PersistenceContextType}>>();"));
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
        var ctx = new
        {
            Usings = new string[] { "NForza.Cyrus.EntityFramework", "NForza.Cyrus.Aggregates", "NForza.Cyrus.Abstractions"},
            Namespace = "Persistence",
            Name = "PersistenceRegistration",
            Initializer = persistenceRegistrations
        };

        var fileContents = cyrusGenerationContext.LiquidEngine.Render(ctx, "CyrusInitializer");
        return fileContents;
    }
}