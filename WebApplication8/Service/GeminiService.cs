using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DotNetEnv;
using Microsoft.Extensions.Configuration;

//Env.Load(); // Load biến môi trường

public class GeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        // Lấy API key từ biến môi trường (file .env)
        _apiKey = Environment.GetEnvironmentVariable("GOOGLE_GEMINI_API_KEY");
        _model = Environment.GetEnvironmentVariable("GOOGLE_GEMINI_MODEL");

        if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_model))
        {
            throw new InvalidOperationException("API Key hoặc Model không được tìm thấy trong biến môi trường!");
        }

    }

    public async Task<string> SendMessageAsync(string message)
    {
        string apiUrl = $"https://generativelanguage.googleapis.com/v1/models/{_model}:generateContent?key={_apiKey}";

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = message } } }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(apiUrl, content);
        var responseString = await response.Content.ReadAsStringAsync();

        Console.WriteLine("Response từ API Gemini: " + responseString);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Lỗi API Gemini: {response.StatusCode}, Nội dung: {responseString}");
        }

        using JsonDocument doc = JsonDocument.Parse(responseString);
        if (!doc.RootElement.TryGetProperty("candidates", out JsonElement candidates) ||
            candidates.GetArrayLength() == 0 ||
            !candidates[0].TryGetProperty("content", out JsonElement contentElement) ||
            !contentElement.TryGetProperty("parts", out JsonElement parts) ||
            parts.GetArrayLength() == 0 ||
            !parts[0].TryGetProperty("text", out JsonElement textElement))
        {
            throw new KeyNotFoundException("Phản hồi API không có dữ liệu hợp lệ!");
        }

        return textElement.GetString();
    }
}
