using System.Collections.Generic;
using NForza.Cyrus.Abstractions.Model;

namespace NForza.Cyrus.TypescriptGenerate.Model;

public class CyrusMetadata: ICyrusModel
{
    public IEnumerable<string> Guids { get; } = [];
    public IEnumerable<string> Integers { get; } = [];
    public IEnumerable<string> Strings { get; } = [];
    public IEnumerable<ModelTypeDefinition> Events { get; } = [];
    public IEnumerable<ModelTypeDefinition> Commands { get; } = [];
    public IEnumerable<ModelTypeDefinition> Queries { get; } = [];
    public IEnumerable<ModelHubDefinition> Hubs { get; } = [];
}
