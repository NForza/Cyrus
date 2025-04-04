using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using NForza.Cyrus.Abstractions;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NForza.Cyrus.WebApi;

public class ConfigureSwaggerOptions(TypedIdDictionary typedIds) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var typedId in typedIds)
        {
            options.MapType(typedId.Key, () => new OpenApiSchema() { Type = GetSwaggerType(typedId.Value), Format = typedId.Value == typeof(Guid) ? "uuid" : "" });
        }
        options.SchemaFilter<ContractSchemaFilter>();
    }

    private string GetSwaggerType(Type value) 
        => value switch
           {
               Type t when t == typeof(int) => "integer",
               _ => "string"
           };
}