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
            RequestUri = new Uri($"https://api.telegram.org/bot{TgBotToken}/sendMessage")
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
}