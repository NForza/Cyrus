using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.ValueTypes;

public class GuidValueProvider : CyrusProviderBase<ImmutableArray<INamedTypeSymbol>>
{
    public override IncrementalValueProvider<ImmutableArray<INamedTypeSymbol>> GetProvider(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<GenerationConfig> configProvider)
    {
        var incrementalValuesProvider = context.SyntaxProvider
                    .CreateSyntaxProvider(
                        predicate: (syntaxNode, _) => syntaxNode.IsRecordWithGuidValueAttribute(),
                        transform: (context, _) => context.GetRecordSymbolFromContext());

        var recordStructsWithAttribute = incrementalValuesProvider
            .Where(x => x is not null)
            .Select((x, _) => x!)
            .Collect();

        return recordStructsWithAttribute;
    }
}
