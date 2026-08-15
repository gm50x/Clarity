using Serilog;

namespace Clarity.App.Configurations;

public static class LoggerConfigurations
{
    public static void ConfigureBootstrapLogger()
    {
        Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();
    }

    public static void BootstrapApp(Action<string[]> bootstrapFunc, string[] args)
    {
        try
        {
            bootstrapFunc.Invoke(args);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            Environment.ExitCode = 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static void ConfigureLogger(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, config) =>
        {
            config.ReadFrom.Configuration(context.Configuration);
        });
    }
}
