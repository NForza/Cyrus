using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;

namespace NForza.Cyrus.Generators.WebApi;

public class BusRegistrationGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider)
    {
        if (cyrusProvider.GenerationConfig.GenerationTarget.Contains(GenerationTarget.WebApi))
        {
            var contents = AddBusRegistrations(cyrusProvider.GenerationConfig);

            if (!string.IsNullOrWhiteSpace(contents))
            {
                var ctx = new
                {
                    Usings = cyrusProvider.GenerationConfig.EventBus == EventBusType.MassTransit ? ["NForza.Cyrus.Cqrs", "NForza.Cyrus.MassTransit"] : new string[] { "NForza.Cyrus.Cqrs" },
                    Namespace = "BusRegistration",
                    Name = "BusRegistration",
                    Initializer = contents
                };

                var fileContents = cyrusProvider.LiquidEngine.Render(ctx, "CyrusInitializer");
                spc.AddSource(
                   "BusRegistration.g.cs",
                   SourceText.From(fileContents, Encoding.UTF8));
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