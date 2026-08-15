using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Text.Json;
using System.Text.Json.Serialization;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;
using MvcOptions = Microsoft.AspNetCore.Mvc.MvcOptions;

namespace Clarity.App.Configurations;

public static class SerializationConfigurations
{
    private static readonly JsonNamingPolicy _jsonNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    private static readonly JsonStringEnumConverter _jsonStringEnumConverter = new(JsonNamingPolicy.SnakeCaseLower);
    public static IServiceCollection ConfigureSerialization(this IServiceCollection services)
    {
        services.Configure<MvcJsonOptions>(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = _jsonNamingPolicy;
            options.JsonSerializerOptions.Converters.Add(_jsonStringEnumConverter);
        });

        services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = _jsonNamingPolicy;
            options.SerializerOptions.Converters.Add(_jsonStringEnumConverter);
        });

        services.Configure<MvcOptions>(options =>
        {
            var transformer = new EndpointNamingTransformer();
            var convention = new RouteTokenTransformerConvention(transformer);
            options.Conventions.Add(convention);
        });

        return services;
    }
    private class EndpointNamingTransformer : IOutboundParameterTransformer
    {
        private static readonly JsonNamingPolicy _endpointNamingPolicy = JsonNamingPolicy.KebabCaseLower;
        public string? TransformOutbound(object? value) =>
            value == null
                ? null
                : _endpointNamingPolicy.ConvertName(value.ToString()!);

    }
}
