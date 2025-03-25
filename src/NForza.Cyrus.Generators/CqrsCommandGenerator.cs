using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Model;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Cqrs;

[Generator]
public class CyrusGenerator : CyrusSourceGeneratorBase, IIncrementalGenerator
{
    private CommandHandlerGenerator CommandHandlerGenerator { get; } = new CommandHandlerGenerator();

    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);

        var configProvider = ConfigProvider(context);

        var commandHandlerProvider = CommandHandlerGenerator.GetProvider(context, configProvider);

        var cyrusProvider = context.CompilationProvider
            .Combine(commandHandlerProvider)
            .Combine(configProvider)
            .Select((combinedProviders, _) =>
            {
                var ((compilation, commandhandlers), generationConfig) = combinedProviders;
                return new CyrusGenerationContext(compilation, commandhandlers, generationConfig);
            });

        context.RegisterSourceOutput(cyrusProvider, (spc, source) =>
        {
           CommandHandlerGenerator.GenerateSource(spc, source, LiquidEngine);            
        });
    }

}