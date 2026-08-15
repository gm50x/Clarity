using Asp.Versioning;
using Scalar.AspNetCore;

namespace Clarity.App.Configurations;

public static class DocumentationConfigurations
{
    public static void ConfigureDocumentation(this IApiVersioningBuilder builder, IConfiguration configuration)
    {
        var settings = configuration.GetSection("Documentation").Get<DocumentationSettings>()
            ?? throw new InvalidOperationException("Missing required \"Documentation\" settings");

        builder.AddOpenApi(options =>
        {
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
}

