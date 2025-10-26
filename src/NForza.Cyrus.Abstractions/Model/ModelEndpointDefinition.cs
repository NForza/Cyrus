namespace NForza.Cyrus.Abstractions.Model;

public class ModelEndpointDefinition

{
    public ModelEndpointDefinition(HttpVerb httpVerb, string route, string commandName, string queryName)
    {
        HttpVerb = httpVerb;
        Route = route;
        CommandName = commandName;
        QueryName = queryName;
    }

    public HttpVerb HttpVerb { get; set; }
    public string Route { get; set; }
    public string CommandName { get; set; } = null;
    public string QueryName { get; set; } = null;
}
