using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Validators;

public class ValidatorGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        if (cyrusGenerationContext.Validators.Any())
        {
            var validators = cyrusGenerationContext.Validators;

            var validatorRegistrations = string.Join("\n",
                    cyrusGenerationContext.Validators
                        .Select(ch => ch.ContainingType)
                        .Where(x => x != null)
                        .Where(x => !x.IsStatic)
                        .Distinct(SymbolEqualityComparer.Default)
                        .Select(cht => $" services.AddTransient<{cht!.ToFullName()}>();"));
            if (!string.IsNullOrEmpty(validatorRegistrations))
            {
                var ctx = new
                {
                    Usings = new string[] { "NForza.Cyrus.Cqrs", "NForza.Cyrus.Abstractions" },
                    Namespace = "Validators",
                    Name = "ValidatorRegistration",
                    Initializer = validatorRegistrations
                };

                var fileContents = cyrusGenerationContext.LiquidEngine.Render(ctx, "CyrusInitializer");
                spc.AddSource("ValidatorRegistration.g.cs", fileContents);
            }
        }
    }
}