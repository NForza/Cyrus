using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.EntityFramework;

public class EntityFrameworkPersistenceGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var config = cyrusGenerationContext.GenerationConfig;
        if (!string.IsNullOrEmpty(config.PersistenceContextType) && cyrusGenerationContext.AggregateRoots.Any())
        {
            var sourceText = GenerateEntityFrameworkPersistenceInitializer(cyrusGenerationContext);
            spc.AddSource($"EntityFrameworkPersistenceInitializer.g.cs", SourceText.From(sourceText, Encoding.UTF8));
        }
    }

    private string GenerateEntityFrameworkPersistenceInitializer(CyrusGenerationContext cyrusGenerationContext)
    {
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
        var persistenceRegistrations = string.Join(Environment.NewLine, cyrusGenerationContext.AggregateRoots.Select(v =>
                $"services.AddTransient<IAggregateRootPersistence, EntityFrameworkPersistence<{v.Symbol.ToFullName()}, {v.AggregateRootProperty.ToFullName()}, {cyrusGenerationContext.GenerationConfig.PersistenceContextType}>();"));
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
        var ctx = new
        {
            Usings = new string[] { },
            Namespace = "Persistence",
            Name = "PersistenceRegistration",
            Initializer = persistenceRegistrations
        };

        var fileContents = cyrusGenerationContext.LiquidEngine.Render(ctx, "CyrusInitializer");
        return fileContents;
    }
}