
using Clarity.App.Configurations;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Serilog;

namespace Clarity.App;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            StartApp(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
    public static void StartApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var configuration = builder.Configuration;

        builder.Host.UseSerilog((context, services, config) =>
        {
            config.ReadFrom.Configuration(configuration);
        });

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.ConfigureSerialization();
        builder.Services.ConfigureApiVersioning();
        builder.Services.ConfigureDocumentation(configuration);

        var app = builder.Build();

        app.UseSerilogRequestLogging();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDocumenation();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
