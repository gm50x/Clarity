using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Clarity.App.Controllers.v1;


[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class HelloWorldController(ILogger<HelloWorldController> _logger) : ControllerBase
{
    [HttpGet]
    public IActionResult GetHello([FromQuery] HelloWorldRequestQuery query)
    {
        var message = query.GreetingType is GreetingType.Informal ? $"Hi, {query.FullName ?? "World"}" : $"Hello, {query.FullName ?? "World"}";
        return Ok(new HelloWorldResponseBody(message, query.GreetingType));
    }

    [HttpPost]
    public IActionResult PostHello([FromBody] HelloWorldRequestBody body)
    {
        var message = body.GreetingType is GreetingType.Informal ? $"Hi, {body.FullName ?? "World"}" : $"Hello, {body.FullName ?? "World"}";
        return Ok(new HelloWorldResponseBody(message, body.GreetingType));
    }

    [HttpGet("logs")]
    public IActionResult ProduceLogs([FromQuery(Name = "count")] int count)
    {
        for (int i = 0; i < count; i++)
        {
            var logScope = new Dictionary<string, string> {
                { "TargetId", Guid.NewGuid().ToString()},
                {"TraceKey", $"TraceKey-#{i}"}
            };
            using (_logger.BeginScope(logScope))
            {
                _logger.LogInformation("Running for {id}", i);
            }
        }

        return Ok();
    }
}

public enum GreetingType
{
    None,
    Informal,
    Formal
}

public record HelloWorldRequestQuery([FromQuery(Name = "full_name")] string? FullName, [FromQuery(Name = "greeting_type")] GreetingType GreetingType);
public record HelloWorldRequestBody(string? FullName, GreetingType GreetingType);
public record HelloWorldResponseBody(string GreetingMessage, GreetingType GreetingType);
