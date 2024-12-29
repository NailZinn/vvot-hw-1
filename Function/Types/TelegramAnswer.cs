namespace Function.Types;

public class TelegramAnswer
{
    public string Method { get; set; } = default!;

    public long ChatId { get; set; }

    public string Text { get; set; } = default!;
}