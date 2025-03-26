using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.Generators.WebApi;

public class BusRegistrationGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider, LiquidEngine liquidEngine)
    {
        var config = cyrusProvider.GenerationConfig;
        if (config.GenerationTarget.Contains(GenerationTarget.WebApi))
        {
            var contents = AddBusRegistrations(config);
            if (!string.IsNullOrWhiteSpace(contents))
            {
                var ctx = new
                {
                    Usings = config.EventBus == "MassTransit"
                            ? 
                            [
                                "NForza.Cyrus.MassTransit",
                                "NForza.Cyrus.Cqrs"
                            ] 
                            : 
                            new string[] { "NForza.Cyrus.Cqrs" },
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