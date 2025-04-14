using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.SignalR;

namespace NForza.Cyrus.Generators
{
    public class CyrusGenerationContext
    {
        public CyrusGenerationContext(
            Compilation compilation,
            ImmutableArray<INamedTypeSymbol> guidIds,
            ImmutableArray<INamedTypeSymbol> intIds,
            ImmutableArray<INamedTypeSymbol> stringIds,
            ImmutableArray<INamedTypeSymbol> commands,
            ImmutableArray<IMethodSymbol> commandHandlers,
            ImmutableArray<ISymbol> allCommandsAndHandlers,
            ImmutableArray<INamedTypeSymbol> queries,
            ImmutableArray<IMethodSymbol> queryHandlers,
            ImmutableArray<INamedTypeSymbol> events,
            ImmutableArray<IMethodSymbol> eventHandlers,
            ImmutableArray<ISymbol> allQueriesAndHandlers,
            ImmutableArray<IMethodSymbol> validators,
            ImmutableArray<SignalRHubClassDefinition> signalRHubs,
            GenerationConfig generationConfig)
        {
            Compilation = compilation;
            GuidIds = guidIds;
            IntIds = intIds;
            StringIds = stringIds;
            Commands = commands;
            CommandHandlers = commandHandlers;
            AllCommandsAndHandlers = allCommandsAndHandlers;
            Queries = queries;
            QueryHandlers = queryHandlers;
            EventHandlers = eventHandlers;
            Events = events;
            AllQueriesAndHandlers = allQueriesAndHandlers;
            Validators = validators;
            SignalRHubs = signalRHubs;
            GenerationConfig = generationConfig;
        }

        public Compilation Compilation { get; private set; }
        public ImmutableArray<INamedTypeSymbol> GuidIds { get; private set; }
        public ImmutableArray<INamedTypeSymbol> IntIds { get; private set; }
        public ImmutableArray<INamedTypeSymbol> StringIds { get; private set; }
        public ImmutableArray<INamedTypeSymbol> TypedIds => GuidIds.AddRange(IntIds).AddRange(StringIds);   
        public ImmutableArray<INamedTypeSymbol> Commands { get; private set; }
        public ImmutableArray<IMethodSymbol> CommandHandlers { get; private set; }
        public ImmutableArray<ISymbol> AllCommandsAndHandlers { get; private set; }
        public ImmutableArray<INamedTypeSymbol> AllCommands => AllCommandsAndHandlers.OfType<INamedTypeSymbol>().ToImmutableArray();
        public ImmutableArray<INamedTypeSymbol> Queries { get; private set; }
        public ImmutableArray<IMethodSymbol> QueryHandlers { get; private set; }
        public ImmutableArray<ISymbol> AllQueriesAndHandlers { get; private set; }
        public ImmutableArray<IMethodSymbol> Validators { get; private set; }
        public ImmutableArray<INamedTypeSymbol> Events { get; private set; }
        public ImmutableArray<IMethodSymbol> EventHandlers { get; private set; }
        public ImmutableArray<SignalRHubClassDefinition> SignalRHubs { get; private set; }
        public GenerationConfig GenerationConfig { get; private set; }
    }
}