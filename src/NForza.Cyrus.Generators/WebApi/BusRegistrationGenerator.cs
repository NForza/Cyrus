using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
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
                    Usings = cyrusGenerationContext.GenerationConfig.EventBus == EventBusType.MassTransit ? ["NForza.Cyrus.Cqrs", "NForza.Cyrus.MassTransit"] : new string[] { "NForza.Cyrus.Cqrs" },
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
        sb.AppendLine(@$"services.AddTransient<IEventBus, {generationConfig.EventBus}EventBus>();");
        if (generationConfig.EventBus == EventBusType.MassTransit)
        {
            sb.AppendLine(@$" services.AddTransient<LocalEventBus>();");
        }

        return sb.ToString();
    }
}