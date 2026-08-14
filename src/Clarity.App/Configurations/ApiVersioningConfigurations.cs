using Asp.Versioning;

namespace Clarity.App.Configurations;

public static class ApiVersioningConfiguration
{
    public static void ConfigureApiVersioning(this IServiceCollection services)
    {
        services
            // 1. Add API Versioning and API Explorer Core Services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true; // Returns supported versions in response headers
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new QueryStringApiVersionReader("api-version"),
                    new HeaderApiVersionReader("X-API-Version"),
                    new MediaTypeApiVersionReader("ver")); // Reads version from route URL
            })
            // 2. Format the group name to generate individual documents (e.g., 'v1', 'v2')
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true; // Substitutes {version:apiVersion} parameter in docs
            })
            .AddMvc();
    }
}
