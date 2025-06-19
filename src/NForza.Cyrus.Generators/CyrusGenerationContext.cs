using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Aggregates;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.SignalR;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators;

public class CyrusGenerationContext
{
    public CyrusGenerationContext(
        Compilation compilation,
        ImmutableArray<INamedTypeSymbol> guidValues,
        ImmutableArray<INamedTypeSymbol> intValues,
        ImmutableArray<INamedTypeSymbol> doubleValues,
        ImmutableArray<INamedTypeSymbol> stringValues,
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
        ImmutableArray<AggregateRootDefinition> aggregateRoots,
        ImmutableArray<SignalRHubClassDefinition> signalRHubs,
        GenerationConfig generationConfig, 
        LiquidEngine liquidEngine,
        bool isTestProject)
    {
        Compilation = compilation;
        GuidValues = guidValues;
        IntValues = intValues;
        DoubleValues = doubleValues;
        StringValues = stringValues;
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
        AggregateRoots = aggregateRoots;
        SignalRHubs = signalRHubs;
        GenerationConfig = generationConfig;
        LiquidEngine = liquidEngine;
        IsTestProject = isTestProject;
    }

    public Compilation Compilation { get; }
    public ImmutableArray<INamedTypeSymbol> GuidValues { get; }
    public ImmutableArray<INamedTypeSymbol> IntValues { get; }
    public ImmutableArray<INamedTypeSymbol> DoubleValues { get; }
    public ImmutableArray<INamedTypeSymbol> StringValues { get; }
    public ImmutableArray<INamedTypeSymbol> Commands { get; }
    public ImmutableArray<IMethodSymbol> CommandHandlers { get; }
    public ImmutableArray<ISymbol> AllCommandsAndHandlers { get; }
    public ImmutableArray<INamedTypeSymbol> Queries { get; }
    public ImmutableArray<IMethodSymbol> QueryHandlers { get; }
    public ImmutableArray<ISymbol> AllQueriesAndHandlers { get;  }
    public ImmutableArray<IMethodSymbol> Validators { get; }
    public ImmutableArray<AggregateRootDefinition> AggregateRoots { get; }
    public ImmutableArray<INamedTypeSymbol> Events { get; }
    public ImmutableArray<INamedTypeSymbol> AllEvents { get; }
    public ImmutableArray<IMethodSymbol> EventHandlers { get; }
    public ImmutableArray<SignalRHubClassDefinition> SignalRHubs { get; }
    public GenerationConfig GenerationConfig { get; }
    public LiquidEngine LiquidEngine { get; }
    public bool IsTestProject { get; }

    public ImmutableArray<INamedTypeSymbol> ValueTypes => GuidValues.AddRange(IntValues).AddRange(StringValues).AddRange(DoubleValues);
    public ImmutableArray<INamedTypeSymbol> AllCommands => AllCommandsAndHandlers.OfType<INamedTypeSymbol>().ToImmutableArray();
    public ImmutableArray<IMethodSymbol> AllCommandHandlers => AllCommandsAndHandlers.OfType<IMethodSymbol>().ToImmutableArray();
    public ImmutableArray<IMethodSymbol> AllQueryHandlers => AllQueriesAndHandlers.OfType<IMethodSymbol>().ToImmutableArray();
    public ImmutableArray<IMethodSymbol> AllHandlers => AllCommandsAndHandlers.Concat(AllQueriesAndHandlers).OfType<IMethodSymbol>().ToImmutableArray();
}