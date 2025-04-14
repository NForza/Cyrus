using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.WebApi;

public class ValidatorProvider : CyrusProviderBase<ImmutableArray<IMethodSymbol>>
{
    public override IncrementalValueProvider<ImmutableArray<IMethodSymbol>> GetProvider(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<GenerationConfig> configProvider)
    {
        var compilationProvider = context.CompilationProvider;

        var validatorProvider = compilationProvider
            .SelectMany((compilation, _) =>
            {
                var typesFromAssemblies = compilation.GetAllTypesFromCyrusAssemblies();

                var validatorsFromAssemblies = typesFromAssemblies.SelectMany(t => t.GetMembers().OfType<IMethodSymbol>().Where(m => m.IsValidator()));

                return validatorsFromAssemblies;
            })
           .Collect();
        return validatorProvider;
    }

}
