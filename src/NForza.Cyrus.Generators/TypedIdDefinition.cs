using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Abstractions.Generator;

internal record struct TypedIdDefinition(INamedTypeSymbol symbol, string Name, string Namespace);