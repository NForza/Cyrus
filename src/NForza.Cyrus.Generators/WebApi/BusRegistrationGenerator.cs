using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.WebApi;

[Generator]
public class BusRegistrationGenerator : CyrusGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);

        var configProvider = ConfigProvider(context);

        context.RegisterSourceOutput(configProvider, (sourceProductionContext, config) =>
        {
            if (config != null && config.GenerationTarget.Contains(GenerationTarget.WebApi))
            {
                var contents = AddBusRegistrations(config);

                if (!string.IsNullOrWhiteSpace(contents))
                {
                    var ctx = new
                    {
                        Usings = new string[] { "NForza.Cyrus.Cqrs" },
                        Namespace = "BusRegistration",
                        Name = "BusRegistration",
                        Initializer = contents
                    };

                    var fileContents = LiquidEngine.Render(ctx, "CyrusInitializer");
                    sourceProductionContext.AddSource(
                       "BusRegistration.g.cs",
                       SourceText.From(fileContents, Encoding.UTF8));
                }
            }
        });
    }

    private string AddBusRegistrations(GenerationConfig generationConfig)
    {
        return @$"services.AddTransient<IEventBus, {generationConfig.EventBus}EventBus>();";
    }
}