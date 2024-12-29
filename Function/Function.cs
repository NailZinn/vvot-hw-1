using System.Text.Json;
using Function.Types;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Function;

public class Handler
{
    private readonly TelegramBotClient _bot = new TelegramBotClient("");

    public async Task<Response> FunctionHandler(Request request)
    {
        var update = JsonSerializer.Deserialize<Update>(request.Body);

        if (update is null || update.Message is null) return new Response(400, "Некорректный запрос.");

        if (update.Message.Type is not MessageType.Text and not MessageType.Photo)
        {
            return new Response(200, JsonSerializer.Serialize(
                new TelegramAnswer
                {
                    Method = "sendMessage",
                    ChatId = update.Message.Chat.Id,
                    Text = "Я могу обработать только текстовое сообщение или фотографию."
                },
                Utils.KebabCaseOptions
            ));
        }

        var textMessage = "";
        if (update.Message.Type == MessageType.Text)
        {
            textMessage = update.Message.Text;
        }
        else if (update.Message.Type == MessageType.Photo)
        {
            var textFromImage = await Helpers.GetTextFromImage(update.Message.Photo![^1].FileId, _bot);
            if (textFromImage is null)
            {
                return new Response(200, JsonSerializer.Serialize(
                    new TelegramAnswer
                    {
                        Method = "sendMessage",
                        ChatId = update.Message.Chat.Id,
                        Text = "Я не могу обработать эту фотографию."
                    },
                    Utils.KebabCaseOptions
                ));
            }
        }

        if (textMessage == "/start" || textMessage == "/help")
        {
            return new Response(200, JsonSerializer.Serialize(
                new TelegramAnswer
                {
                    Method = "sendMessage",
                    ChatId = update.Message.Chat.Id,
                    Text = "Я помогу подготовить ответ на экзаменационный вопрос по дисциплине \"Операционные системы\".\nПришлите мне фотографию с вопросом или наберите его текстом."
                },
                Utils.KebabCaseOptions
            ));
        }

        var gptResponse = await Helpers.SendGptPromptAsync(textMessage!);
        return new Response(200, JsonSerializer.Serialize(
            new TelegramAnswer
            {
                Method = "sendMessage",
                ChatId = update.Message.Chat.Id,
                Text = gptResponse
            },
            Utils.KebabCaseOptions
        ));
    }
}