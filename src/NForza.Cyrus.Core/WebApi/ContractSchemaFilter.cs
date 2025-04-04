using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NForza.Cyrus.WebApi
{
    internal class ContractSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var name = context.Type.Name;
            if (name.EndsWith("Contract"))
            {
                schema.Title = name[..^8];
            }
        }
    }
}