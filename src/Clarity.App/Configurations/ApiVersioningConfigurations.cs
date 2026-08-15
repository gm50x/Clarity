using Asp.Versioning;

namespace Clarity.App.Configurations;

public static class ApiVersioningConfigurations
{
    public static IApiVersioningBuilder ConfigureApiVersioning(this IServiceCollection services)
    {
        return services
            // 1. Add API Versioning and API Explorer Core Services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true; // Returns supported versions in response headers
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),               // Reads version from the URL segment
                    new QueryStringApiVersionReader("api-version"), // ?api-version=1.0
                    new HeaderApiVersionReader("X-API-Version"),    // X-API-Version: 1.0
                    new MediaTypeApiVersionReader("ver"));          // Content-Type: ...; ver=1.0
            })
            // 2. Format the group name to generate individual documents (e.g., 'v1', 'v2')
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true; // Substitutes {version:apiVersion} parameter in docs
            })
            .AddMvc(); // Opts MVC controllers into API versioning (AddApiExplorer no longer calls this implicitly)
    }
}
