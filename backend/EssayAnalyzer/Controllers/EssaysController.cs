using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;

[ApiController]
[Route("api/[controller]")]
[EnableCors("AllowAll")]
public class EssaysController : ControllerBase
{
    private readonly EssayService _essayService;

    public EssaysController(EssayService essayService)
    {
        _essayService = essayService;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromBody] EssayRequest request)
    {
        // Manual CORS header as backup
        Response.Headers.Append("Access-Control-Allow-Origin", "*");
        
        if (string.IsNullOrWhiteSpace(request.EssayText))
            return BadRequest("Essay text is required.");

        if (request.EssayText.Split(' ').Length < 50)
            return BadRequest("Essay must be at least 50 words.");

        var result = await _essayService.AnalyzeAsync(request.EssayText);
        return Ok(result);
    }

    [HttpOptions("analyze")]    // ← handles preflight request
    public IActionResult PreflightAnalyze()
    {
        Response.Headers.Append("Access-Control-Allow-Origin", "*");
        Response.Headers.Append("Access-Control-Allow-Methods", "POST, OPTIONS");
        Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type");
        return Ok();
    }

    [HttpGet("stats")]
    public IActionResult GetStats()
    {
        return Ok(new { message = "Stats endpoint coming soon." });
    }
}