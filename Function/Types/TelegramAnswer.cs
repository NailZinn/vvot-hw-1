namespace Function.Types;

public class TelegramAnswer(long chatId, string text)
{
    public long ChatId { get; set; } = chatId;

    public string Text { get; set; } = text;
}