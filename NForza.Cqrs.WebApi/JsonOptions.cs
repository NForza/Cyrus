using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace NForza.Cqrs.WebApi;

public class JsonOptionsConfigurator(IEnumerable<JsonConverter> jsonConverters) : IConfigureOptions<JsonOptions>
{
    public void Configure(JsonOptions options)
    {
        foreach (var jsonConverter in jsonConverters)
        {
            options.SerializerOptions.Converters.Add(jsonConverter);
        }
    }
}