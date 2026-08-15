using Clarity.App.Configurations;

namespace Clarity.App;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var configuration = builder.Configuration;

        builder.Services.AddControllers();
        builder.Services.ConfigureSerialization();
        builder.Services.ConfigureApiVersioning()
            .ConfigureDocumentation(configuration);

        var app = builder.Build();

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
