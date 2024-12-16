namespace NForza.Cyrus.WebApi;

public record EndpointDefinition<T> : IEndpointDefinition
{
    public Type EndpointType => typeof(T);
    public Type? InputMappingPolicyType { get; set; } = null;
    public string Method { get; set; } = string.Empty;
    public string EndpointPath { get; set; } = string.Empty;
    public string GroupPath { get; set; } = string.Empty;
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
    public string[] Tags { get; set; } = ["API"];
}
