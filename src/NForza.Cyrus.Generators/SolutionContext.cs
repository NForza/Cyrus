using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators;

public class SolutionContext(CyrusGenerationContext cyrusGenerationContext, ImmutableArray<ISymbol> allCommandsAndHandlers, ImmutableArray<ISymbol> allQueriesAndHandlers, ImmutableArray<INamedTypeSymbol> allValueTypes)
{
    public ImmutableArray<INamedTypeSymbol> ValueTypes => allValueTypes;
    public ImmutableArray<INamedTypeSymbol> GuidValues => allValueTypes.Where(v => v.IsGuidValue()).ToImmutableArray();
    public ImmutableArray<INamedTypeSymbol> StringValues => allValueTypes.Where(v => v.IsStringValue()).ToImmutableArray();
    public ImmutableArray<INamedTypeSymbol> DoubleValues => allValueTypes.Where(v => v.IsDoubleValue()).ToImmutableArray();
    public ImmutableArray<INamedTypeSymbol> IntValues => allValueTypes.Where(v => v.IsIntValue()).ToImmutableArray();
    public ImmutableArray<INamedTypeSymbol> Commands => allCommandsAndHandlers.OfType<INamedTypeSymbol>().ToImmutableArray();
    public ImmutableArray<IMethodSymbol> CommandHandlers => allCommandsAndHandlers.OfType<IMethodSymbol>().ToImmutableArray();
    public ImmutableArray<IMethodSymbol> QueryHandlers => allQueriesAndHandlers.OfType<IMethodSymbol>().ToImmutableArray();
    public ImmutableArray<INamedTypeSymbol> Queries => allQueriesAndHandlers.OfType<INamedTypeSymbol>().ToImmutableArray();
    public ImmutableArray<IMethodSymbol> Handlers => allCommandsAndHandlers
            .Concat(allQueriesAndHandlers)
            .OfType<IMethodSymbol>().ToImmutableArray();
}