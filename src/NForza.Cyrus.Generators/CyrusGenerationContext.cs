using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Config;

namespace NForza.Cyrus.Generators.Cqrs
{
    internal class CyrusGenerationContext
    {
        public CyrusGenerationContext(Compilation compilation, ImmutableArray<IMethodSymbol> commandHandlers, GenerationConfig generationConfig)
        {
            Compilation = compilation;
            CommandHandlers = commandHandlers;
            GenerationConfig = generationConfig;
        }

        public Compilation Compilation { get; private set; }
        public ImmutableArray<IMethodSymbol> CommandHandlers { get ; private set; }
        public GenerationConfig GenerationConfig { get; private set; }
    }
}