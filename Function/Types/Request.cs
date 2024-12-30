namespace Function.Types;

public class Request
{
    public string httpMethod { get; set; } = default!;

    public string body { get; set; } = default!;
}