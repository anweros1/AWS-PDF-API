using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API_PDF;

/// <summary>
/// Swagger operation filter to add X-Username header to all endpoints
/// </summary>
public class SwaggerHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        // Add X-Username header to all operations
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Username",
            In = ParameterLocation.Header,
            Description = "Username for logging and tracking",
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Default = new Microsoft.OpenApi.Any.OpenApiString("TestUser")
            }
        });
    }
}
