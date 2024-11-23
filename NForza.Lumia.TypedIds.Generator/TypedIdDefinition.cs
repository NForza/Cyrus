using Microsoft.CodeAnalysis;

namespace NForza.Lumia.TypedIds.Generator;

internal record struct TypedIdDefinition(INamedTypeSymbol symbol, string Name, string Namespace);