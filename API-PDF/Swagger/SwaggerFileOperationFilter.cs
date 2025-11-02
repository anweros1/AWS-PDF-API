using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API_PDF;

/// <summary>
/// Swagger operation filter to support file uploads in Swagger UI
/// </summary>
public class SwaggerFileOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParameters = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile) || 
                       p.ParameterType == typeof(IFormFileCollection) ||
                       p.ParameterType == typeof(IEnumerable<IFormFile>))
            .ToList();

        if (!fileParameters.Any())
            return;

        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>(),
                        Required = new HashSet<string>()
                    }
                }
            }
        };

        var schema = operation.RequestBody.Content["multipart/form-data"].Schema;

        foreach (var fileParameter in fileParameters)
        {
            schema.Properties[fileParameter.Name!] = new OpenApiSchema
            {
                Type = "string",
                Format = "binary"
            };
            schema.Required.Add(fileParameter.Name!);
        }

        // Add other form parameters
        var otherParameters = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType != typeof(IFormFile) && 
                       p.ParameterType != typeof(IFormFileCollection) &&
                       p.ParameterType != typeof(IEnumerable<IFormFile>) &&
                       p.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.FromFormAttribute), false).Any())
            .ToList();

        foreach (var param in otherParameters)
        {
            schema.Properties[param.Name!] = new OpenApiSchema
            {
                Type = "string"
            };
        }
    }
}
