using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Commands
{
    internal class CommandHandlerProvider : CyrusProviderBase<ImmutableArray<IMethodSymbol>>
    {
        public override IncrementalValueProvider<ImmutableArray<IMethodSymbol>> GetProvider(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<GenerationConfig> configProvider)
        {
            var provider = context.SyntaxProvider
                        .CreateSyntaxProvider(
                            predicate: (syntaxNode, _) => syntaxNode.IsCommandHandler(),
                            transform: (context, _) => context.GetMethodSymbolFromContext());

            var commandHandlerProvider = provider.Combine(configProvider)
                .Where(x =>
                {
                    var (methodNode, config) = x;
                    if (config == null || !config.GenerationTarget.Contains(GenerationTarget.Domain))
                        return false;
                    return true;
                })
                .Where(x => x.Left != null)
                .Select((x, _) => x.Left!)
                .Collect();

            return commandHandlerProvider;
        }
    }
}