namespace NForza.Cyrus.Abstractions.Model
{
    public class ModelHubDefinition : INamedModelType
    {
        public ModelHubDefinition(string name, string path, IEnumerable<string> commands, IEnumerable<ModelQueryDefinition> queries, IEnumerable<string> events)
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
        public IEnumerable<ModelQueryDefinition> Queries { get; }
        public IEnumerable<string> Events { get; }
    }
}
