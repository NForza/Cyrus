﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Config;

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
            ImmutableArray<INamedTypeSymbol> queries,
            ImmutableArray<IMethodSymbol> queryHandlers,
            ImmutableArray<INamedTypeSymbol> events,
            ImmutableArray<IMethodSymbol> eventHandlers,
            GenerationConfig generationConfig)
        {
            Compilation = compilation;
            GuidIds = guidIds;
            IntIds = intIds;
            StringIds = stringIds;
            Commands = commands;
            CommandHandlers = commandHandlers;
            Queries = queries;
            QueryHandlers = queryHandlers;
            EventHandlers = eventHandlers;
            Events = events;
            GenerationConfig = generationConfig;
        }

        public Compilation Compilation { get; private set; }
        public ImmutableArray<INamedTypeSymbol> GuidIds { get; private set; }
        public ImmutableArray<INamedTypeSymbol> IntIds { get; private set; }
        public ImmutableArray<INamedTypeSymbol> StringIds { get; private set; }
        public ImmutableArray<INamedTypeSymbol> TypedIds => GuidIds.AddRange(IntIds).AddRange(StringIds);   
        public ImmutableArray<INamedTypeSymbol> Commands { get; private set; }
        public ImmutableArray<IMethodSymbol> CommandHandlers { get; private set; }
        public ImmutableArray<INamedTypeSymbol> Queries { get; private set; }
        public ImmutableArray<IMethodSymbol> QueryHandlers { get; private set; }
        public ImmutableArray<INamedTypeSymbol> Events { get; private set; }
        public ImmutableArray<IMethodSymbol> EventHandlers { get; private set; }
        public GenerationConfig GenerationConfig { get; private set; }
    }
}