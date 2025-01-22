
namespace NForza.Cyrus.WebApi;

public interface IEndpointDefinition
{
    string EndpointPath { get; set; }
    Type EndpointType { get; }
    string GroupPath { get; set; }
    Type? InputMappingPolicyType { get; set; }
    string Method { get; set; }
    string Path { get; }
    string[] Tags { get; set; }
}