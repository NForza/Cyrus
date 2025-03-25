using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Config;

namespace NForza.Cyrus.Generators
{
    public class CyrusGenerationContext
    {
        public CyrusGenerationContext(
            Compilation compilation,
            ImmutableArray<INamedTypeSymbol> commands,
            ImmutableArray<IMethodSymbol> commandHandlers,
            ImmutableArray<INamedTypeSymbol> queries,
            ImmutableArray<IMethodSymbol> queryHandlers,
            ImmutableArray<INamedTypeSymbol> events,
            ImmutableArray<IMethodSymbol> eventHandlers,
            GenerationConfig generationConfig)
        {
            Compilation = compilation;
            Commands = commands;
            CommandHandlers = commandHandlers;
            Queries = queries;
            QueryHandlers = queryHandlers;
            EventHandlers = eventHandlers;
            Events = events;
            GenerationConfig = generationConfig;
        }

        public Compilation Compilation { get; private set; }
        public ImmutableArray<INamedTypeSymbol> Commands { get; private set; }
        public ImmutableArray<IMethodSymbol> CommandHandlers { get; private set; }
        public ImmutableArray<INamedTypeSymbol> Queries { get; private set; }
        public ImmutableArray<IMethodSymbol> QueryHandlers { get; private set; }
        public ImmutableArray<INamedTypeSymbol> Events { get; private set; }
        public ImmutableArray<IMethodSymbol> EventHandlers { get; private set; }
        public GenerationConfig GenerationConfig { get; private set; }
    }
}