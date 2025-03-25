using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Config;

namespace NForza.Cyrus.Generators;

public abstract class CyrusProviderBase<T>
{
    public abstract IncrementalValueProvider<T> GetProvider(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<GenerationConfig> configProvider);
}
