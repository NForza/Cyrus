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
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider)
    {
        var config = cyrusProvider.GenerationConfig;
        if (!string.IsNullOrEmpty(config.PersistenceContextType) && cyrusProvider.AggregateRoots.Any())
        {
            var sourceText = GenerateEntityFrameworkPersistenceInitializer(config.PersistenceContextType, cyrusProvider);
            spc.AddSource($"EntityFrameworkPersistenceInitializer.g.cs", SourceText.From(sourceText, Encoding.UTF8));
        }
    }

    private string GenerateEntityFrameworkPersistenceInitializer(string entityFrameworkPersistenceType, CyrusGenerationContext cyrusGenerationContext)
    {
        //var persistenceRegistrations = string.Join(Environment.NewLine, cyrusGenerationContext.AggregateRoots.Select(v =>
        //    $"services.AddTransient<IAggregateRootPersistence, EntityFrameworkPersistence<{v.ToFullName()}>();"));
        var persistenceRegistrations = "";
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