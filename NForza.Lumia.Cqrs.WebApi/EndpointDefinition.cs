namespace NForza.Lumia.Cqrs.WebApi;

public record EndpointDefinition(Type EndpointType)
{
    public Type? InputMappingPolicyType { get; set; } = null;
    public string Method { get; internal set; } = string.Empty;
    public string EndpointPath { get; internal set; } = string.Empty;
    public string GroupPath { get; internal set; } = string.Empty;
    public string Path => string.IsNullOrWhiteSpace(GroupPath) ? EndpointPath : $"{GroupPath}/{EndpointPath}".Trim('/');
    public string[] Tags { get; internal set; } = ["API"];
}
