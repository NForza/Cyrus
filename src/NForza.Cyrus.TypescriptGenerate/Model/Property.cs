using System.Collections.Generic;

namespace NForza.Cyrus.TypescriptGenerate.Model;

public class Property : ITypeDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsCollection { get; set; } = false;
    public bool IsNullable { get; set; } = false;
    public IEnumerable<ITypeDefinition> SupportTypes { get; set; } = [];
}