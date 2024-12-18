using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;

namespace NForza.Cyrus.WebApi;

public static class RouteHandlerBuilderExtensions
{
    public static RouteHandlerBuilder WithSwaggerParameters(this RouteHandlerBuilder builder, string path)
      => builder.WithOpenApi(operation =>
         {
             foreach (var param in RouteParameterDiscovery.FindAllParametersInRoute(path))
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
