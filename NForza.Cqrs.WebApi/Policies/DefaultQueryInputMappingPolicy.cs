using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace NForza.Cqrs.WebApi.Policies;

public class DefaultQueryInputMappingPolicy(HttpContext httpContext) : InputMappingPolicy
{
    private static readonly Dictionary<Type, Func<string, object>> TypeConverters = new()
    {
        { typeof(Guid), s => Guid.Parse(s) },
        { typeof(int), s => int.Parse(s) },
        { typeof(long), s => long.Parse(s) },
        { typeof(string), s => s }
    };

    public override Task<object> MapInputAsync(Type typeToCreate)
    {
        static ConstructorInfo? FindConstructorForQuery(Type queryType)
        {
            ConstructorInfo[] constructorInfos = queryType.GetConstructors();
            if (constructorInfos.Length > 1)
                throw new ArgumentException($"More than one constructor found for query {queryType.Name}.");
            return constructorInfos.FirstOrDefault();
        }

        var constructor = FindConstructorForQuery(typeToCreate);
        if (constructor == null)
            return Task.FromResult(Activator.CreateInstance(typeToCreate)!);
        List<object?> parameters = [];
        foreach (var parameter in constructor.GetParameters())
            if (httpContext.Request.RouteValues.TryGetValue(parameter.Name!, out var value))
                parameters.Add(value == null ? null : TypeConverters[parameter.ParameterType](value.ToString()!));
        object? result = constructor.Invoke([.. parameters]);
        return Task.FromResult(result);
    }
}
