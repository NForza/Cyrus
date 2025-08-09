using System;

namespace NForza.Cyrus.Abstractions.Model;

public class ModelTypeDefinition
{
    public ModelTypeDefinition(string name, string clrTypeName, ModelPropertyDefinition[] properties, string[] values, bool isCollection, bool isNullable)
    {
        Name = name;
        ClrTypeName = clrTypeName;
        Properties = properties;
        IsCollection = isCollection;
        IsNullable = isNullable;
        Values = values;
    }
    public string Name { get; set; } = string.Empty;
    public bool IsCollection { get; set; } = false;
    public bool IsNullable { get; set; } = false;

    public ModelPropertyDefinition[] Properties { get; set; } = [];
    public string[] Values { get; set; } = [];

    public string Channel { get; set; } = "";
    public string ClrTypeName { get; set; } = "";
}
