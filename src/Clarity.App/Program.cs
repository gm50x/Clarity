using Clarity.App.Configurations;
using Serilog;

namespace Clarity.App;

public class Program
{
    public static void Main(string[] args)
    {
        LoggerConfigurations.ConfigureBootstrapLogger();
        LoggerConfigurations.BootstrapApp(StartApp, args);
    }

    private static void StartApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var configuration = builder.Configuration;

        builder.ConfigureLogger();

        // Add services to the container.

        builder.Services.AddControllers();
        builder.Services.ConfigureSerialization();
        builder.Services.ConfigureApiVersioning()
            .ConfigureDocumentation(configuration);

        var app = builder.Build();

        app.UseSerilogRequestLogging();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDocumentation();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
