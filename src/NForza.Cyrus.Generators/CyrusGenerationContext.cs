using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.SignalR;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators;

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
        ImmutableArray<INamedTypeSymbol> allEvents,
        ImmutableArray<IMethodSymbol> eventHandlers,
        ImmutableArray<ISymbol> allQueriesAndHandlers,
        ImmutableArray<IMethodSymbol> validators,

        ImmutableArray<SignalRHubClassDefinition> signalRHubs,
        GenerationConfig generationConfig, 
        LiquidEngine liquidEngine)
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
        AllEvents = allEvents;
        AllQueriesAndHandlers = allQueriesAndHandlers;
        Validators = validators;
        SignalRHubs = signalRHubs;
        GenerationConfig = generationConfig;
        LiquidEngine = liquidEngine;
    }

    public Compilation Compilation { get; }
    public ImmutableArray<INamedTypeSymbol> GuidIds { get; }
    public ImmutableArray<INamedTypeSymbol> IntIds { get; set; }
    public ImmutableArray<INamedTypeSymbol> StringIds { get; }
    public ImmutableArray<INamedTypeSymbol> Commands { get; }
    public ImmutableArray<IMethodSymbol> CommandHandlers { get; }
    public ImmutableArray<ISymbol> AllCommandsAndHandlers { get; }
    public ImmutableArray<INamedTypeSymbol> Queries { get; }
    public ImmutableArray<IMethodSymbol> QueryHandlers { get; }
    public ImmutableArray<ISymbol> AllQueriesAndHandlers { get;  }
    public ImmutableArray<IMethodSymbol> Validators { get; }
    public ImmutableArray<INamedTypeSymbol> Events { get; }
    public ImmutableArray<INamedTypeSymbol> AllEvents { get; }
    public ImmutableArray<IMethodSymbol> EventHandlers { get; }
    public ImmutableArray<SignalRHubClassDefinition> SignalRHubs { get; }
    public GenerationConfig GenerationConfig { get; }
    public LiquidEngine LiquidEngine { get; }

    public ImmutableArray<INamedTypeSymbol> TypedIds => GuidIds.AddRange(IntIds).AddRange(StringIds);
    public ImmutableArray<INamedTypeSymbol> AllCommands => AllCommandsAndHandlers.OfType<INamedTypeSymbol>().ToImmutableArray();
    public ImmutableArray<IMethodSymbol> AllCommandHandlers => AllCommandsAndHandlers.OfType<IMethodSymbol>().ToImmutableArray();
    public ImmutableArray<IMethodSymbol> AllQueryHandlers => AllQueriesAndHandlers.OfType<IMethodSymbol>().ToImmutableArray();
    public ImmutableArray<IMethodSymbol> AllHandlers => AllCommandsAndHandlers.Concat(AllQueriesAndHandlers).OfType<IMethodSymbol>().ToImmutableArray();

}