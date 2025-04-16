using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace NForza.Cyrus.WebApi
{
    internal class CyrusOpenApiSchemaTransformer : IOpenApiSchemaTransformer
    {
        public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
        {
            if (context.JsonTypeInfo.Type.Name.EndsWith("Contract"))
            {
                schema.Title = context.JsonTypeInfo.Type.Name[..^8];

                var requiredProperties = new HashSet<string>();
                var type = context.JsonTypeInfo.Type;

                foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    var isRequired = Attribute.IsDefined(prop, typeof(RequiredAttribute));
                    var jsonName = prop.Name;

                    if (isRequired)
                        requiredProperties.Add(jsonName);

                    var camelCasedName = char.ToLowerInvariant(jsonName[0]) + jsonName[1..];

                    if (schema.Properties.TryGetValue(camelCasedName, out var propSchema))
                    {
                        propSchema.Nullable = !isRequired;
                    }
                }

                if (requiredProperties.Count > 0)
                {
                    schema.Required = requiredProperties;
                }

            }
            return Task.CompletedTask;
        }
    }
}
