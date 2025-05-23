﻿using System.Collections.Generic;

namespace NForza.Cyrus.Abstractions.Model;

public class ModelHubDefinition 
{
    public ModelHubDefinition(string name, string path, IEnumerable<string> commands, IEnumerable<string> queries, IEnumerable<string> events)
    {
        Name = name;
        Path = path;
        Commands = commands;
        Queries = queries;
        Events = events;
    }
    public string Name { get; }
    public string Path { get; }
    public IEnumerable<string> Commands { get; }
    public IEnumerable<string> Queries { get; }
    public IEnumerable<string> Events { get; }
}
