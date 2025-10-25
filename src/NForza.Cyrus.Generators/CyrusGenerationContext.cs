using System.Collections.Immutable;
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
        ImmutableArray<INamedTypeSymbol> allValueTypes,
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
        this.allValueTypes = allValueTypes;
        Commands = commands;
        CommandHandlers = commandHandlers;
        this.allCommandsAndHandlers = allCommandsAndHandlers;
        Queries = queries;
        QueryHandlers = queryHandlers;
        EventHandlers = eventHandlers;
        Events = events;
        AllEvents = allEvents;
        this.allQueriesAndHandlers = allQueriesAndHandlers;
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

    private ImmutableArray<INamedTypeSymbol> allValueTypes;

    public ImmutableArray<INamedTypeSymbol> Commands { get; }
    public ImmutableArray<IMethodSymbol> CommandHandlers { get; }
    private ImmutableArray<ISymbol> allCommandsAndHandlers;
    public ImmutableArray<INamedTypeSymbol> Queries { get; }
    public ImmutableArray<IMethodSymbol> QueryHandlers { get; }
    private ImmutableArray<ISymbol> allQueriesAndHandlers;
    public ImmutableArray<IMethodSymbol> Validators { get; }
    public ImmutableArray<AggregateRootDefinition> AggregateRoots { get; }
    public ImmutableArray<INamedTypeSymbol> Events { get; }
    public ImmutableArray<INamedTypeSymbol> AllEvents { get; }
    public ImmutableArray<IMethodSymbol> EventHandlers { get; }
    public ImmutableArray<INamedTypeSymbol> ValueTypes => GuidValues.AddRange(IntValues).AddRange(StringValues).AddRange(DoubleValues);
    public ImmutableArray<SignalRHubClassDefinition> SignalRHubs { get; }
    public GenerationConfig GenerationConfig { get; }
    public LiquidEngine LiquidEngine { get; }
    public bool IsTestProject { get; }

    public SolutionContext All { get => new SolutionContext(allCommandsAndHandlers, allQueriesAndHandlers, allValueTypes); }
}