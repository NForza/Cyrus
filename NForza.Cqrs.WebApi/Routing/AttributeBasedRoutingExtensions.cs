using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace NForza.Cqrs.WebApi.Routing;

public static class RouteExtensions
{
    public static void MapCqrsHandlers(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapCqrsQueryHandlers();
        endpoints.MapCqrsCommandHandlers();
    }

    public static void MapCqrsQueryHandlers(this IEndpointRouteBuilder endpoints,
        params Assembly[] resolveFromAssemblies)
    {
        //resolveFromAssemblies ??= AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsFrameworkAssembly()).ToArray();

        //var x = typeof(IQuery<>).Name;
        //var types = resolveFromAssemblies
        //    .SelectMany(s => s.GetTypes())
        //    .Where(p => p.GetInterface(x) != null);

        //var mapQueryExtensionMethod =
        //    typeof(IEndpointRouteBuilderExtensions).GetMethod("MapQueryInternal",
        //        BindingFlags.NonPublic | BindingFlags.Static);

        //foreach (var queryType in types)
        //{
        //    if (Attribute.GetCustomAttribute(queryType, typeof(GetAttribute)) is not GetAttribute get) continue;
        //    if (AlreadyRegistered(get.UrlPattern)) continue;

        //    var genericExtensionMethod = mapQueryExtensionMethod!.MakeGenericMethod(queryType);
        //    genericExtensionMethod.Invoke(null, [endpoints, get.UrlPattern, string.Empty, string.Empty, null]);
        //}

        //bool AlreadyRegistered(string getUrlPattern)
        //{
        //    return endpoints
        //        .DataSources
        //        .SelectMany(ds => ds.Endpoints)
        //        .Any(e => e is RouteEndpoint rep && rep.RoutePattern.RawText?.ToLowerInvariant() == getUrlPattern.ToLowerInvariant());

        //}
    }

    private static void CopyPropertiesFromRouteValues(object query, RouteValueDictionary routeValues) => query.GetType().GetProperties().ToList().ForEach(p =>
                                                                                                              {
                                                                                                                  if (routeValues.ContainsKey(p.Name) && p.GetSetMethod() != null)
                                                                                                                  {
                                                                                                                      p.SetValue(query, routeValues[p.Name]);
                                                                                                                  }
                                                                                                              });

    public static void MapCqrsCommandHandlers(this IEndpointRouteBuilder endpoints)
    {
        //var x = nameof(ICommand);
        //var types = AppDomain.CurrentDomain.GetAssemblies()
        //    .Where(a => !a.IsFrameworkAssembly())
        //    .SelectMany(s => s.GetTypes())
        //    .Where(p => p.GetInterface(x) != null);

        //foreach (var commandType in types)
        //{
        //    if (Attribute.GetCustomAttribute(commandType, typeof(PostAttribute)) is PostAttribute post)
        //    {
        //        endpoints.MapHttpVerbToCommandHandler(HttpMethods.Post, post.UrlPattern, commandType);
        //    }

        //    if (Attribute.GetCustomAttribute(commandType, typeof(PutAttribute)) is PutAttribute put)
        //    {
        //        endpoints.MapHttpVerbToCommandHandler(HttpMethods.Put, put.UrlPattern, commandType);
        //    }

        //    if (Attribute.GetCustomAttribute(commandType, typeof(DeleteAttribute)) is DeleteAttribute delete)
        //    {
        //        endpoints.MapHttpVerbToCommandHandler(HttpMethods.Delete, delete.UrlPattern, commandType);
        //    }
        //}
    }

    private static void MapHttpVerbToCommandHandler(this IEndpointRouteBuilder endpoints, string method,
        string urlPattern, Type commandType)
    {
//        var x = nameof(ICommand);
//        var customMapping = commandType.GetInterfaces()
//            .FirstOrDefault(i => i == typeof(ICustomRequestHandler));

//        RouteHandlerBuilder builder;
//        if (customMapping == null)
//        {
//            builder = endpoints.MapMethods(urlPattern, [method],
//                ([FromServices] ICommandDispatcher commands, JsonElement command) =>
//                {
//                    var interfaceType = commandType.GetInterface(x);
//                    var cmd = JsonSerializer.Deserialize(command.ToString(), commandType,
//                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

//                    CommandResult commandResult = commands.Execute(cmd.CastToReflected(interfaceType!));
//                });
//        }
//        else
//        {
//            var cmdInstance =
//                (ICustomRequestHandler)Activator.CreateInstance(commandType).CastToReflected(customMapping);
//            builder = endpoints.MapMethods(urlPattern, [method], cmdInstance.CreateRequestHandler());
//        }

//        builder.ApplyAttributeBasedAuthorization(commandType);

//        builder
//            .WithTags("Commands")
//            .Accepts(commandType, "application/json");
//    }
//}

//public static class RouteHandlerBuilderExtensions
//{
//    public static void ApplyAttributeBasedAuthorization(this RouteHandlerBuilder builder, Type cqrsType)
//    {
//        var allowAnonymous =
//            Attribute.GetCustomAttribute(cqrsType, typeof(AllowAnonymousAttribute)) is AllowAnonymousAttribute
//                anonymous;
//        if (allowAnonymous)
//            builder.AllowAnonymous();
    }
}

public static class ObjectExtensions
{
    public static T CastTo<T>(this object o) => (T)o;

    public static dynamic CastToReflected(this object? o, Type type)
    {
        var methodInfo =
            typeof(ObjectExtensions).GetMethod(nameof(CastTo), BindingFlags.Static | BindingFlags.Public);
        var genericArguments = new[] { type };
        var genericMethodInfo = methodInfo?.MakeGenericMethod(genericArguments);
        return genericMethodInfo?.Invoke(null, [o])!;
    }
}