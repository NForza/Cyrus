using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.Cqrs.WebApi.Policies;

public class DefaultCommandInputMappingPolicy(HttpContext httpContext) : InputMappingPolicy
{
    private static readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web);
    public override async Task<object> MapInputAsync(Type typeToCreate)
    {
        using var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8);
        var jsonText = await reader.ReadToEndAsync();

        return JsonSerializer.Deserialize(jsonText, typeToCreate, serializerOptions) ?? throw new InvalidOperationException($"Can't deserialize object to type {typeToCreate.FullName}");
    }
}
