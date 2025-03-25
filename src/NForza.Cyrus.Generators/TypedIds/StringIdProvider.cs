using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Generators.TypedIds;

public class StringIdProvider : CyrusProviderBase<ImmutableArray<INamedTypeSymbol>>
{
    public override IncrementalValueProvider<ImmutableArray<INamedTypeSymbol>> GetProvider(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<GenerationConfig> configProvider)
    {
        var incrementalValuesProvider = context.SyntaxProvider
                    .CreateSyntaxProvider(
                        predicate: (syntaxNode, _) => syntaxNode.IsRecordWithStringIdAttribute(),
                        transform: (context, _) => context.GetNamedTypeSymbolFromContext());

        var recordStructsWithAttribute = incrementalValuesProvider
            .Where(x => x is not null)
            .Select((x, _) => x!)
            .Collect();
        return recordStructsWithAttribute;
    }
}
