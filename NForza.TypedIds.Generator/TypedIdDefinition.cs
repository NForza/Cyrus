using Microsoft.CodeAnalysis;

namespace NForza.TypedIds.Generator;

internal record struct TypedIdDefinition(INamedTypeSymbol symbol, string Name, string Namespace);