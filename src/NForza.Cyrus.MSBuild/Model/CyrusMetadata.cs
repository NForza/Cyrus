using System.Collections.Generic;
using NForza.Cyrus.Abstractions.Model;

namespace NForza.Cyrus.TypescriptGenerate.Model;

public class CyrusMetadata: ICyrusModel
{
    public IEnumerable<string> Guids { get; set; } = [];
    public IEnumerable<string> Integers { get; set; } = [];
    public IEnumerable<string> Strings { get; set; } = [];
    public IEnumerable<ModelTypeDefinition> Models { get; set; } = [];
    public IEnumerable<ModelTypeDefinition> Events { get; set; } = [];
    public IEnumerable<ModelTypeDefinition> Commands { get; set; } = [];
    public IEnumerable<ModelTypeDefinition> Queries { get; set; } = [];
    public IEnumerable<ModelHubDefinition> Hubs { get; set; } = [];
}
