using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Clarity.App.Controllers.v2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class HelloWorldController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHello([FromQuery] HelloWorldRequestQuery query)
    {
        var message = $"Hello, {query.FullName ?? "World"}";
        return Ok(new HelloWorldResponseBody(message));
    }

    [HttpPost]
    public IActionResult PostHello([FromBody] HelloWorldRequestBody body)
    {
        var message = $"Hello, {body.FullName ?? "World"}";
        return Ok(new HelloWorldResponseBody(message));
    }
}

public record HelloWorldRequestQuery([FromQuery(Name = "full_name")] string? FullName);
public record HelloWorldRequestBody(string? FullName);
public record HelloWorldResponseBody(string GreetingMessage);
