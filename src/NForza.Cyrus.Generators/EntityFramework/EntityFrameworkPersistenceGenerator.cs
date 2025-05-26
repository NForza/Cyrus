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
        if (!string.IsNullOrEmpty(config.PersistenceContextType))
        {
                var sourceText = GenerateEntityFrameworkPersistenceInitializer(config.PersistenceContextType, cyrusProvider.LiquidEngine);
                spc.AddSource($"EntityFrameworkPersistenceInitializer.g.cs", SourceText.From(sourceText, Encoding.UTF8));
        }
    }

    private string GenerateEntityFrameworkPersistenceInitializer(string entityFrameworkPersistenceType, LiquidEngine liquidEngine)
    {
        return "";
    }
}