using System.Text.Json;
using Function.Types;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Function;

public class Handler
{
    public async Task<Response> FunctionHandler(Request request)
    {
        var body = request.body.Replace("\n", "");
        Console.WriteLine($"body is {body}");

        var update = JsonSerializer.Deserialize<Update>(body, Utils.SnakeCaseOptions);

        Console.WriteLine($"update is {JsonSerializer.Serialize(update)}");

        if (update is null || update.Message is null)
        {
            return new Response(200, "Некорректный запрос.");
        }

        Console.WriteLine($"Message Type is {update.Message.Type}");

        if (update.Message.Type is not MessageType.Text and not MessageType.Photo)
        {
            await Utils.SendMessageAsync(update.Message.Chat.Id, "Я могу обработать только текстовое сообщение или фотографию.");
            return new Response(200, "Некорректный запрос");
        }

        var textMessage = "";
        if (update.Message.Type == MessageType.Text)
        {
            textMessage = update.Message.Text;
        }
        else if (update.Message.Type == MessageType.Photo)
        {
            textMessage = await Helpers.GetTextFromImageAsync(update.Message.Photo![^1].FileId);
            if (textMessage is null)
            {
                await Utils.SendMessageAsync(update.Message.Chat.Id, "Я не могу обработать эту фотографию.");
                return new Response(200, "Некорректный запрос");
            }
        }

        if (textMessage == "/start" || textMessage == "/help")
        {
            Console.WriteLine($"text message is {textMessage}");
            await Utils.SendMessageAsync(update.Message.Chat.Id, "Я помогу подготовить ответ на экзаменационный вопрос по дисциплине \"Операционные системы\".\nПришлите мне фотографию с вопросом или наберите его текстом.");
            return new Response(200, "Ok");
        }

        var gptResponse = await Helpers.SendGptPromptAsync(textMessage!);
        await Utils.SendMessageAsync(update.Message.Chat.Id, gptResponse);
        return new Response(200, "Ok");
    }
}