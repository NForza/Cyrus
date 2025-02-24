using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using NForza.Cyrus.Cqrs;

namespace NForza.Cyrus.WebApi;

public static class RouteHandlerBuilderExtensions
{
    public static RouteHandlerBuilder WithSwaggerParameters(this RouteHandlerBuilder builder, string route)
      => builder.WithOpenApi(operation =>
         {
             foreach (var param in RouteParameterDiscovery.FindAllParametersInRoute(route))
             {
                 operation.Parameters.Add(
                       new OpenApiParameter()
                       {
                           Name = param.Name,
                           In = ParameterLocation.Path,
                           Required = true,
                           Schema = new OpenApiSchema() { Type = param.Type, Format = param.Format }
                       });
             }
             return operation;
         });
}
