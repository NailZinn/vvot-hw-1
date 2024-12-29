namespace Function.Types;

public class Request
{
    public string HttpMethod { get; set; } = default!;

    public string Body { get; set; } = default!;
}