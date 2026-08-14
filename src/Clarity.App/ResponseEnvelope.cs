namespace Clarity.App;

public class ResponseEnvelope<T>
    where T : class
{
    public required string Version { get; set; }
    public required T Data { get; set; }
}