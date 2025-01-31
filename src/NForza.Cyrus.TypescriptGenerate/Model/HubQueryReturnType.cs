using System.Collections.Generic;

namespace NForza.Cyrus.TypescriptGenerate.Model;

public class HubQueryReturnType : ITypeWithProperties, ITypeDefinition
{
    public string Name { get => Type; set => Type = value; }
    public string Type { get; set; } = string.Empty;
    public bool IsCollection { get; set; }
    public bool IsNullable { get; set; }
    public Property[] Properties { get; set ; } = [];
    public SupportType[] SupportTypes { get; set; } = [];
}