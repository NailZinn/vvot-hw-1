using System.Text;
using System.Text.Json;
using Microsoft.Net.Http.Headers;

namespace Function;

public static class Utils
{
    public static readonly JsonSerializerOptions CamelCaseOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static readonly JsonSerializerOptions KebabCaseOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower
    };

    public static readonly string FolderId = Environment.GetEnvironmentVariable("FOLDER_ID")!;

    public static readonly string IamToken = Environment.GetEnvironmentVariable("IAM_TOKEN")!;

    public static async Task<HttpResponseMessage> SendHttpRequestToYandexService(
        HttpMethod method, string uri, List<(string Name, string Value)> headers, string payload)
    {
        using var httpClient = new HttpClient();
        using var httpRequest = new HttpRequestMessage
        {
            Method = method,
            RequestUri = new Uri(uri)
        };
        httpRequest.Headers.Add(HeaderNames.Authorization, $"Bearer {IamToken}");
        headers.ForEach(x => httpRequest.Headers.Add(x.Name, x.Value));
        httpRequest.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        return await httpClient.SendAsync(httpRequest);
    }
}