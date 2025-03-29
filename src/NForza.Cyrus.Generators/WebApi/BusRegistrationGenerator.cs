using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.WebApi;

public class BusRegistrationGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider, LiquidEngine liquidEngine)
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

                var fileContents = LiquidEngine.Render(ctx, "CyrusInitializer");
                spc.AddSource(
                   "BusRegistration.g.cs",
                   SourceText.From(fileContents, Encoding.UTF8));
            }
        }
    }

    private string AddBusRegistrations(GenerationConfig generationConfig)
    {
        return @$"services.AddTransient<IEventBus, {generationConfig.EventBus}EventBus>();";
    }
}