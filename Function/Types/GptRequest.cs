namespace Function.Types;

public class GptRequest
{
    public string ModelUri { get; set; } = default!;

    public CompletionOptions CompletionOptions { get; set; } = default!;

    public Message[] Messages { get; set; } = [];
}

public class CompletionOptions(bool stream, double temperature, string maxTokens)
{
    public bool Stream { get; set; } = stream;

    public double Temperature { get; set; } = temperature;

    public string MaxTokens { get; set; } = maxTokens;
}

public class Message(string role, string text)
{
    public string Role { get; set; } = role;

    public string Text { get; set; } = text;
}