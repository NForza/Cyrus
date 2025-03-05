using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.WebApi.Policies;

public class DefaultCommandInputMappingPolicy(IHttpContextAccessor httpContextAccessor, IHttpContextObjectFactory cqrsFactory)
{
    private static readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web);
    public async Task<object?> MapInputAsync(Type typeToCreate)
    {
        HttpContext? httpContext = httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            throw new InvalidOperationException("HttpContext is not available");
        }

        using var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8);
        var jsonText = await reader.ReadToEndAsync();

        return string.IsNullOrEmpty(jsonText) ? cqrsFactory.CreateFromHttpContext(typeToCreate, httpContext) : JsonSerializer.Deserialize(jsonText, typeToCreate, serializerOptions) ?? throw new InvalidOperationException($"Can't deserialize object to type {typeToCreate.FullName}");
    }
}
