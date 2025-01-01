using System.Text.Json;
using Function.Types;

namespace Function;

public static class Helpers
{
    public static async Task<string> SendGptPromptAsync(string text)
    {
        var context = await File.ReadAllTextAsync("/function/storage/gpt-settings/context.txt");
        var payload = new GptRequest
        {
            ModelUri = $"gpt://{Utils.FolderId}/yandexgpt-lite",
            CompletionOptions = new CompletionOptions(false, 0.6, "2000"),
            Messages = [new Message("system", context), new Message("user", text)]
        };

        var httpResponse = await Utils.SendHttpRequestToYandexService(
            HttpMethod.Post,
            "https://llm.api.cloud.yandex.net/foundationModels/v1/completion",
            [],
            JsonSerializer.Serialize(payload, Utils.CamelCaseOptions)
        );

        if (!httpResponse.IsSuccessStatusCode)
        {
            return "Я не смог подготовить ответ на экзаменационный вопрос.";
        }

        var dataAsString = await httpResponse.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(dataAsString);

        return data
            .GetProperty("result")
            .GetProperty("alternatives")
            .EnumerateArray()
            .FirstOrDefault()
            .GetProperty("message")
            .GetProperty("text")
            .GetString() ?? "Я не смог подготовить ответ на экзаменационный вопрос.";
    }

    public static async Task<string?> GetTextFromImageAsync(string fileId)
    {
        var filePath = await Utils.GetFilePathAsync(fileId);

        if (filePath is null) return null;

        var bytes = await Utils.DownloadFileAsync(filePath);

        if (bytes.Length == 0) return null;

        var imageAsString = Convert.ToBase64String(bytes);

        var payload = new OcrRequest(filePath.Split('.')[^1], ["*"], imageAsString);

        var httpResponse = await Utils.SendHttpRequestToYandexService(
            HttpMethod.Post,
            "https://ocr.api.cloud.yandex.net/ocr/v1/recognizeText",
            [("x-folder-id", Utils.FolderId)],
            JsonSerializer.Serialize(payload, Utils.CamelCaseOptions)
        );

        if (!httpResponse.IsSuccessStatusCode) return null;

        var dataAsString = await httpResponse.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(dataAsString);

        return data
            .GetProperty("result")
            .GetProperty("textAnnotation")
            .GetProperty("fullText")
            .GetString();
    }
}