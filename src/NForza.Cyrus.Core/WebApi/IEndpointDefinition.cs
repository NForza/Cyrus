
namespace NForza.Cyrus.WebApi
{
    public interface IEndpointDefinition
    {
        string EndpointPath { get; }
        Type EndpointType { get; init; }
        string GroupPath { get; }
        Type? InputMappingPolicyType { get; set; }
        string Method { get; }
        string Path { get; }
        string[] Tags { get; }
    }
}