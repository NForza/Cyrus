namespace NForza.Cyrus.WebApi;

public record EndpointDefinition(Type EndpointType) : IEndpointDefinition
{
    public Type? InputMappingPolicyType { get; set; } = null;
    public string Method { get; internal set; } = string.Empty;
    public string EndpointPath { get; internal set; } = string.Empty;
    public string GroupPath { get; internal set; } = string.Empty;
    public string Path
    {
        get
        {
            var result = GroupPath.Trim('/');
            if (!string.IsNullOrEmpty(EndpointPath))
            {
                result += $"/{EndpointPath.Trim('/')}";
            }
            return result;
        }
    }
    public string[] Tags { get; internal set; } = ["API"];
}
