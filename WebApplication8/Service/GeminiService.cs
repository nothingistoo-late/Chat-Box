using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class GeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GoogleGemini:ApiKey"];
        _model = configuration["GoogleGemini:Model"];
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

        // In phản hồi từ API để kiểm tra
        Console.WriteLine("Response từ API Gemini: " + responseString);

        // Kiểm tra nếu API trả về lỗi HTTP
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Lỗi API Gemini: {response.StatusCode}, Nội dung: {responseString}");
        }

        // Phân tích JSON trả về
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
