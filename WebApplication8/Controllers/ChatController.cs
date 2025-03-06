using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Route("api/chat")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly GeminiService _geminiService;

    public ChatController(GeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message cannot be empty.");

        string response = await _geminiService.SendMessageAsync(request.Message);
        return Ok(new { Reply = response });
    }

    // Thêm phương thức GET
    [HttpGet]
    public IActionResult GetMessage()
    {
        return Ok(new { message = "GET method is working!" });
    }
}

public class ChatRequest
{
    public string Message { get; set; }
}
