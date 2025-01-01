using System.Text;
using System.Text.Json;

namespace Function;

public static class Utils
{
    public static readonly JsonSerializerOptions CamelCaseOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static readonly JsonSerializerOptions SnakeCaseOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public static readonly string FolderId = Environment.GetEnvironmentVariable("FOLDER_ID")!;

    public static readonly string IamToken = Environment.GetEnvironmentVariable("IAM_TOKEN")!;

    public static readonly string TgBotToken = Environment.GetEnvironmentVariable("TG_BOT_TOKEN")!;

    private static readonly string TgBotUrlFormat = $"https://api.telegram.org{{0}}/bot{TgBotToken}{{1}}";

    public static async Task<HttpResponseMessage> SendHttpRequestToYandexService(
        HttpMethod method, string uri, List<(string Name, string Value)> headers, string payload)
    {
        using var httpClient = new HttpClient();
        using var httpRequest = new HttpRequestMessage
        {
            Method = method,
            RequestUri = new Uri(uri)
        };
        httpRequest.Headers.Add("Authorization", $"Bearer {IamToken}");
        headers.ForEach(x => httpRequest.Headers.Add(x.Name, x.Value));
        httpRequest.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        return await httpClient.SendAsync(httpRequest);
    }

    public static async Task SendMessageAsync(long chatId, string text)
    {
        using var httpClient = new HttpClient();
        using var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(string.Format(TgBotUrlFormat, "", "/sendMessage"))
        };
        httpRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["chat_id"] = chatId.ToString(),
            ["text"] = text
        });

        Console.WriteLine(await httpRequest.Content.ReadAsStringAsync());

        var httpResponse = await httpClient.SendAsync(httpRequest);
        Console.WriteLine(httpResponse.StatusCode);
        Console.WriteLine(await httpResponse.Content.ReadAsStringAsync());
    }

    public static async Task<string?> GetFilePathAsync(string fileId)
    {
        using var httpClient = new HttpClient();
        using var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(string.Format(TgBotUrlFormat, "", "/getFile"))
        };
        httpRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["file_id"] = fileId
        });

        var httpResponse = await httpClient.SendAsync(httpRequest);

        if (!httpResponse.IsSuccessStatusCode) return null;

        var dataAsString = await httpResponse.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<JsonElement>(dataAsString)
            .GetProperty("result")
            .GetProperty("file_path")
            .GetString();
    }

    public static async Task<byte[]> DownloadFileAsync(string filePath)
    {
        using var httpClient = new HttpClient();
        using var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(string.Format(TgBotUrlFormat, "/file", $"/{filePath}"))
        };

        var httpResponse = await httpClient.SendAsync(httpRequest);

        if (!httpResponse.IsSuccessStatusCode) return [];

        var stream = await httpResponse.Content.ReadAsStreamAsync();
        var memory = new MemoryStream();
        await stream.CopyToAsync(memory);

        return memory.ToArray();
    }
}