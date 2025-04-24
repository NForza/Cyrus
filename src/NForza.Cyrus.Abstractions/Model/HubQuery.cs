using System;

namespace NForza.Cyrus.Abstractions.Model;

public class HubQuery
{
    public string Name { get; set; } = string.Empty;
    public ModelTypeDefinition ReturnType { get; set; } = new ModelTypeDefinition(string.Empty, Array.Empty<ModelPropertyDefinition>(), Array.Empty<string>(), false, false); 
}