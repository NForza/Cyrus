using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using NForza.Cyrus.Abstractions;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NForza.Cyrus.Cqrs;

public class ConfigureSwaggerOptions(TypedIdDictionary typedIds) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var typedId in typedIds)
        {
            options.MapType(typedId.Key, () => new OpenApiSchema() { Type = "string", Format = typedId.Value == typeof(Guid) ? "uuid" : "" });
        }
    }
}