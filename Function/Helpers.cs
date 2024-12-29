using System.Net.Http.Json;
using System.Text.Json;
using Function.Types;
using Microsoft.Net.Http.Headers;
using Telegram.Bot;

namespace Function;

public static class Helpers
{
    public static async Task<string> SendGptPromptAsync(string text)
    {
        var context = await File.ReadAllTextAsync("/function/storage/gpt-settings/context.txt");
        var payload = new GptRequest
        {
            ModelUri = $"gpt:{Utils.FolderId}/yandexgpt-lite",
            CompletionOptions = new CompletionOptions(false, 0.6, "2000"),
            Messages = [new Message("system", context), new Message("user", text)]
        };

        var httpResponse = await Utils.SendHttpRequestToYandexService(
            HttpMethod.Post,
            "https://llm.api.cloud.yandex.net/foundationModels/v1/completion",
            [(HeaderNames.ContentType, "application/json")],
            JsonSerializer.Serialize(payload, Utils.CamelCaseOptions)
        );

        if (!httpResponse.IsSuccessStatusCode)
        {
            return "Я не смог подготовить ответ на экзаменационный вопрос.";
        }

        var data = await httpResponse.Content.ReadFromJsonAsync<JsonElement>();

        return data
            .GetProperty("result")
            .GetProperty("alternatives")
            .EnumerateArray()
            .FirstOrDefault()
            .GetProperty("message")
            .GetProperty("text")
            .GetString() ?? "Я не смог подготовить ответ на экзаменационный вопрос.";
    }

    public static async Task<string?> GetTextFromImage(string fileId, TelegramBotClient bot)
    {
        await using var stream = File.Create("./file.temp");
        var file = await bot.GetInfoAndDownloadFile(fileId, stream);

        Console.WriteLine(file.FilePath);

        var memory = new MemoryStream();
        stream.CopyTo(memory);

        var bytes = memory.ToArray();
        var imageAsString = Convert.ToBase64String(bytes);

        var payload = new OcrRequest(file.FilePath!.Split('.')[^1], ["*"], imageAsString);

        var httpResponse = await Utils.SendHttpRequestToYandexService(
            HttpMethod.Post,
            "https://ocr.api.cloud.yandex.net/ocr/v1/recognizeText",
            [("x-folder-id", Utils.FolderId)],
            JsonSerializer.Serialize(payload, Utils.CamelCaseOptions)
        );

        if (!httpResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var data = await httpResponse.Content.ReadFromJsonAsync<JsonElement>();

        return data
            .GetProperty("fullText")
            .GetString();
    }
}