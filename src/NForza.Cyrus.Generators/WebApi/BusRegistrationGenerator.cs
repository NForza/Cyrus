using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Config;

namespace NForza.Cyrus.Generators.WebApi;

public class BusRegistrationGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        if (cyrusGenerationContext.GenerationConfig.GenerationTarget.Contains(GenerationTarget.WebApi))
        {
            var contents = AddBusRegistrations(cyrusGenerationContext.GenerationConfig);

            if (!string.IsNullOrWhiteSpace(contents))
            {
                var ctx = new
                {
                    Usings = new List<string> { "NForza.Cyrus.Cqrs", "NForza.Cyrus.Abstractions", "NForza.Cyrus.MassTransit" },
                    Namespace = "BusRegistration",
                    Name = "BusRegistration",
                    Initializer = contents
                };

                var fileContents = cyrusGenerationContext.LiquidEngine.Render(ctx, "CyrusInitializer");
                spc.AddSource("BusRegistration.g.cs", fileContents);
            }
        }
    }

    private string AddBusRegistrations(GenerationConfig generationConfig)
    {
        StringBuilder sb = new();
        sb.AppendLine(@$"services.AddTransient<IMessageBus, MassTransitMessageBus>();");
        return sb.ToString();
    }
}