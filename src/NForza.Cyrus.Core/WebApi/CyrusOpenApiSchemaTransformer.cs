using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            }
            return Task.CompletedTask;
        }
    }
}
