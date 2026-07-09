using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

public class EssayService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly EssayContext _context;

    public EssayService(HttpClient httpClient, IConfiguration config, EssayContext context)
    {
        _httpClient = httpClient;
        _apiKey = config["Anthropic:ApiKey"];
        _context = context;
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    public async Task<EssayAnalysisResult> AnalyzeAsync(string essayText)
    {
        // ── Pull real stats from your Azure SQL dataset ──
        var totalEssays  = await _context.Essays.CountAsync();
        var avgScore     = await _context.Essays.AverageAsync(e => e.Score);
        var highCount    = await _context.Essays
                              .Where(e => e.Score >= 5)
                              .CountAsync();
        var highScorePct = totalEssays > 0 
                              ? (highCount * 100) / totalEssays 
                              : 0;
        var lowCount     = await _context.Essays
                              .Where(e => e.Score <= 2)
                              .CountAsync();
        var lowScorePct  = totalEssays > 0 
                              ? (lowCount * 100) / totalEssays 
                              : 0;

        var prompt = $$"""
            You are an expert writing coach for high school students.

            You have access to a real dataset of {{totalEssays}} student essays
            scored by expert human raters on a 1-6 scale.
            Here are the real statistics from that dataset:
            - Average score: {{avgScore:F1}} out of 6
            - Only {{highScorePct}}% of students score 5 or higher
            - {{lowScorePct}}% of students score 2 or lower

            Use these real statistics to calibrate your scoring so it matches
            genuine academic standards — not inflated or deflated scores.
            A score of 4 means genuinely above average. A score of 6 is rare.

            Analyze the following student essay using that calibration.
            Respond ONLY with a raw JSON object.
            Do NOT use markdown code blocks.
            Do NOT include backticks anywhere.
            Return ONLY the JSON object itself.

            Use this exact structure:
            {
              "overallScore": <integer 1-6>,
              "summary": "<2 sentence assessment that references how this compares to the average student>",
              "categories": [
                {"name": "Thesis & Argument", "score": <integer 1-4>, "feedback": "<specific actionable tip>"},
                {"name": "Evidence & Support", "score": <integer 1-4>, "feedback": "<specific actionable tip>"},
                {"name": "Organization & Flow", "score": <integer 1-4>, "feedback": "<specific actionable tip>"},
                {"name": "Grammar & Style", "score": <integer 1-4>, "feedback": "<specific actionable tip>"}
              ],
              "topStrength": "<one specific thing this student did well>",
              "topImprovement": "<the single most important thing to fix>",
              "benchmarkNote": "<one sentence comparing this essay to the {{avgScore:F1}} average score in the dataset>"
            }

            Student essay to analyze:
            {{essayText}}
        """;

        var body = new
        {
            model = "claude-sonnet-4-20250514",
            max_tokens = 1000,
            messages = new[] { new { role = "user", content = prompt } }
        };

        var response = await _httpClient.PostAsJsonAsync(
            "https://api.anthropic.com/v1/messages", body);

        // Log response for debugging
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new Exception($"Anthropic API error: {response.StatusCode} - {errorBody}");
        }

        var data = await response.Content.ReadFromJsonAsync<AnthropicResponse>();
        // Add this null check
        if (data?.Content == null || data.Content.Count == 0)
        {
            throw new Exception($"Anthropic API returned null or empty response. Status: {response.StatusCode}");
        }
        var rawText  = data.Content[0].Text;
        var cleanJson = rawText
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();

        var result = JsonSerializer.Deserialize<EssayAnalysisResult>(cleanJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result;
    }
}