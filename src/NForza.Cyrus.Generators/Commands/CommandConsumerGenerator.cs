using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.Commands;

public class CommandConsumerGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var commandHandlers = cyrusGenerationContext.CommandHandlers;
        foreach (var commandHandler in commandHandlers)
        {
            var sourceText = GenerateCommandConsumer(commandHandler, cyrusGenerationContext.LiquidEngine);
            spc.AddSource($"{commandHandler.ContainingType.ToFullName().Replace("global::", "")}_CommandConsumer.g.cs", sourceText);
        }
    }

    private string GenerateCommandConsumer(IMethodSymbol commandHandler, LiquidEngine liquidEngine)
    {
        var model = new
        {
            Command = commandHandler.Parameters[0].Type.Name,
            Namespace = commandHandler.Parameters[0].Type.ContainingNamespace.GetNameOrEmpty(),
            HasResult = !commandHandler.ReturnsVoid
        };

        var resolvedSource = liquidEngine.Render(model, "CommandConsumer");
        return resolvedSource;
    }
}