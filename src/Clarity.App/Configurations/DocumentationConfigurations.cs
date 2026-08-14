using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Reflection;

namespace Clarity.App.Configurations;

public static class OpenApiConfiguration
{
    public static void ConfigureDocumentation(this IServiceCollection services, IConfiguration configuration)
    {
        using var sp = services.BuildServiceProvider();

        var settings = configuration.GetSection("Documentation").Get<DocumentationSettings>()
            ?? throw new InvalidOperationException("Missing required \"Documentation\" settings");

        foreach (var description in GetDescriptions(sp))
        {
            services.AddOpenApi(description.GroupName, options =>
            {
                options.AddOperationTransformer<QueryParameterTransformer>();
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Info.Title = $"{settings.Title} - {description.GroupName}";
                    document.Info.Description = settings.Description;
                    document.Info.Version = description.ApiVersion.ToString();
                    return Task.CompletedTask;
                });
            });
        }
    }

    public static void UseDocumenation(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
        app.UseSwaggerUI(options =>
        {
            foreach (var description in GetDescriptions(app.Services))
            {
                options.SwaggerEndpoint($"/openapi/{description.GroupName}.json", description.GroupName.ToUpperInvariant());
            }
        });
    }

    private static IEnumerable<ApiVersionDescription> GetDescriptions(IServiceProvider sp)
    {
        var descriptionProvider = sp.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in descriptionProvider.ApiVersionDescriptions)
        {
            yield return description;
        }
    }

    private class QueryParameterTransformer : IOpenApiOperationTransformer
    {
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            var parameterDescriptions = context.Description.ParameterDescriptions;

            foreach (var description in parameterDescriptions)
            {
                // 1. Identify complex query bound parameters
                if (description.Source.Id == "Query" && description.ModelMetadata?.ContainerType is Type modelType)
                {
                    string? customName = null;

                    // 2. Strategy A: Check Constructor Parameters (For Records)
                    var constructor = modelType.GetConstructors().FirstOrDefault();
                    if (constructor != null)
                    {
                        var constructorParam = constructor.GetParameters()
                            .FirstOrDefault(p => string.Equals(p.Name, description.Name, StringComparison.OrdinalIgnoreCase));

                        var fromQueryAttr = constructorParam?.GetCustomAttribute<FromQueryAttribute>();
                        if (fromQueryAttr != null && !string.IsNullOrEmpty(fromQueryAttr.Name))
                        {
                            customName = fromQueryAttr.Name;
                        }
                    }

                    // 3. Strategy B: Check Properties (For Standard Classes, if Record check yielded nothing)
                    if (customName == null)
                    {
                        var property = modelType.GetProperty(description.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        var fromQueryAttr = property?.GetCustomAttribute<FromQueryAttribute>();
                        if (fromQueryAttr != null && !string.IsNullOrEmpty(fromQueryAttr.Name))
                        {
                            customName = fromQueryAttr.Name;
                        }
                    }

                    // 4. Swap parameter object if a custom name was mapped
                    if (customName != null)
                    {
                        var openApiParam = operation.Parameters?
                            .FirstOrDefault(p => p.In == ParameterLocation.Query && string.Equals(p.Name, description.Name, StringComparison.OrdinalIgnoreCase));

                        if (openApiParam != null)
                        {
                            var index = operation.Parameters?.IndexOf(openApiParam);

                            if (index == null) continue;

                            var overriddenParam = new OpenApiParameter
                            {
                                Name = customName, // <-- Injected name
                                In = openApiParam.In,
                                Description = openApiParam.Description,
                                Required = openApiParam.Required,
                                Schema = openApiParam.Schema,
                                Style = openApiParam.Style,
                                Explode = openApiParam.Explode
                            };

                            operation.Parameters?[index.Value] = overriddenParam;
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}

public class DocumentationSettings
{
    public required string Title { get; set; }
    public required string Description { get; set; }
}