using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.TypedIds;

internal record struct TypedIdDefinition(INamedTypeSymbol symbol, string Name, string Namespace);