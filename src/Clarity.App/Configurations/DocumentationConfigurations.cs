using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Reflection;

namespace Clarity.App.Configurations;

public static class DocumentationConfigurations
{
    public static void ConfigureDocumentation(this IApiVersioningBuilder builder, IConfiguration configuration)
    {
        var settings = configuration.GetSection("Documentation").Get<DocumentationSettings>()
            ?? throw new InvalidOperationException("Missing required \"Documentation\" settings");

        builder.AddOpenApi(options =>
        {
            options.Document.AddOperationTransformer<QueryParameterTransformer>();
            options.Document.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                // options.Description is the ApiVersionDescription for the current version
                document.Info.Title = $"{settings.Title} - {options.Description.GroupName}";
                document.Info.Description = settings.Description;
                document.Info.Version = options.Description.ApiVersion.ToString();
                return Task.CompletedTask;
            });
        });
    }

    public static void UseDocumentation(this WebApplication app)
    {
        app.MapOpenApi().WithDocumentPerVersion();
        app.MapScalarApiReference();
    }

    private class QueryParameterTransformer : IOpenApiOperationTransformer
    {
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            var parameters = operation.Parameters;

            foreach (var description in context.Description.ParameterDescriptions)
            {
                if (description.Source.Id != "Query"
                    || description.ModelMetadata?.ContainerType is not Type modelType
                    || description.Name is not string parameterName
                    || parameters is null)
                {
                    continue;
                }

                // Apply the [FromQuery(Name = "...")] mapping that the OpenAPI generator does not honor
                var customName = GetCustomName(modelType, parameterName);
                if (customName is null)
                {
                    continue;
                }

                var openApiParam = parameters.FirstOrDefault(p =>
                    p.In == ParameterLocation.Query
                    && string.Equals(p.Name, parameterName, StringComparison.OrdinalIgnoreCase));

                if (openApiParam is null)
                {
                    continue;
                }

                parameters[parameters.IndexOf(openApiParam)] = new OpenApiParameter
                {
                    Name = customName,
                    In = openApiParam.In,
                    Description = openApiParam.Description,
                    Required = openApiParam.Required,
                    Schema = openApiParam.Schema,
                    Style = openApiParam.Style,
                    Explode = openApiParam.Explode
                };
            }

            return Task.CompletedTask;
        }

        private static string? GetCustomName(Type modelType, string parameterName)
        {
            // 1. Check constructor parameters (records)
            foreach (var constructor in modelType.GetConstructors())
            {
                var parameter = constructor.GetParameters()
                    .FirstOrDefault(p => string.Equals(p.Name, parameterName, StringComparison.OrdinalIgnoreCase));

                var attribute = parameter?.GetCustomAttribute<FromQueryAttribute>();
                if (attribute is not null && !string.IsNullOrEmpty(attribute.Name))
                {
                    return attribute.Name;
                }
            }

            // 2. Check properties (classes)
            var property = modelType.GetProperty(parameterName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var propertyAttribute = property?.GetCustomAttribute<FromQueryAttribute>();
            if (propertyAttribute is not null && !string.IsNullOrEmpty(propertyAttribute.Name))
            {
                return propertyAttribute.Name;
            }

            return null;
        }
    }
}

public class DocumentationSettings
{
    public required string Title { get; set; }
    public required string Description { get; set; }
}
