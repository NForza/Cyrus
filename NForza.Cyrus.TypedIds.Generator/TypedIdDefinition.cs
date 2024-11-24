using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.TypedIds.Generator;

internal record struct TypedIdDefinition(INamedTypeSymbol symbol, string Name, string Namespace);