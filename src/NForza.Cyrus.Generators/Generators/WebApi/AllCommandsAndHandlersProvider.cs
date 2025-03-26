using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Generators.WebApi;

public class AllCommandsAndHandlersProvider : CyrusProviderBase<ImmutableArray<ISymbol>>
{
    public override IncrementalValueProvider<ImmutableArray<ISymbol>> GetProvider(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<GenerationConfig> configProvider)
    {
        var compilationProvider = context.CompilationProvider;

        var typesFromReferencedAssemblyProvider = compilationProvider
            .SelectMany((compilation, _) =>
            {
                var typesFromAssemblies = compilation.GetAllTypesFromCyrusAssemblies();

                var commandsFromAssemblies = typesFromAssemblies
                    .Where(t => t.IsCommand())
                    .ToList();

                var commandHandlersFromAssemblies = typesFromAssemblies.SelectMany(t => t.GetMembers().OfType<IMethodSymbol>().Where(m => m.IsCommandHandler()));

                var csharpCompilation = (CSharpCompilation)compilation;
                var typesInCompilation = csharpCompilation.GetAllTypesFromCyrusAssemblies();
                var commandsInCompilation = typesInCompilation.Where(t => t.IsCommand())
                    .ToList();
                var commandHandlersInCompilation = typesInCompilation.SelectMany(t => t.GetMembers().OfType<IMethodSymbol>().Where(m => m.IsCommandHandler()));

                var allCommands = typesFromAssemblies
                    .Union(typesInCompilation, SymbolEqualityComparer.Default)
                    .OfType<INamedTypeSymbol>()
                    .Where(t => t.IsCommand())
                    .Cast<ISymbol>();
                var allCommandHandlers = commandHandlersFromAssemblies
                    .Union(commandHandlersInCompilation, SymbolEqualityComparer.Default)
                    .Cast<ISymbol>();
                return allCommands.Cast<ISymbol>().Concat(allCommandHandlers);
            })
           .Collect();
        return typesFromReferencedAssemblyProvider;
    }

}
