using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EssayAnalyzer.Models;

namespace EssayAnalyzer.Services
{
    public interface IAnthropicService
    {
        Task<string> OcrImageAsync(string apiKey, string imageBase64, string mimeType, string language);
        Task<List<Correction>> SpellCheckAsync(string apiKey, string labeledText, string language,
            string[]? neverFlag, AlwaysFlagPair[]? alwaysFlag);
    }

    public class AnthropicService : IAnthropicService
    {
        private readonly HttpClient _http;
        private readonly ILogger<AnthropicService> _logger;
        private const string AnthropicUrl = "https://api.anthropic.com/v1/messages";
        private const string Model = "claude-opus-4-5";

        public AnthropicService(HttpClient http, ILogger<AnthropicService> logger)
        {
            _http = http;
            _logger = logger;
        }

        // ── Call 1: OCR only ─────────────────────────────────────
        public async Task<string> OcrImageAsync(string apiKey, string imageBase64, string mimeType, string language)
        {
            var prompt = $"You are an expert {language} handwriting reader. " +
                         "Transcribe this handwritten text EXACTLY as written. " +
                         "Copy every character, every matra, every mark. " +
                         "Do NOT correct or interpret anything. " +
                         "Label each line: LINE_1: LINE_2: etc. " +
                         "Respond with ONLY the transcription, nothing else.";

            var payload = new
            {
                model = Model,
                max_tokens = 1500,
                temperature = 0,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "image", source = new { type = "base64", media_type = mimeType, data = imageBase64 } },
                            new { type = "text", text = prompt }
                        }
                    }
                }
            };

            var raw = await CallAnthropicAsync(apiKey, payload);
            return raw;
        }

        // ── Call 2: Spell check on clean text ────────────────────
        public async Task<List<Correction>> SpellCheckAsync(string apiKey, string labeledText, string language,
            string[]? neverFlag, AlwaysFlagPair[]? alwaysFlag)
        {
            bool isIndic = new[] { "Marathi", "Hindi", "Tamil", "Telugu", "Kannada" }.Contains(language);
            var checks = isIndic
                ? $"For {language} check: vowel matra errors, wrong consonant substitutions, " +
                  "anusvara/visarga errors, conjunct consonant errors, word spacing errors, " +
                  "wrong verb form, wrong pronoun or postposition."
                : "Check for: spelling mistakes, wrong tense, wrong pronoun, grammar errors.";

            var dictNote = new StringBuilder();
            if (neverFlag?.Length > 0)
                dictNote.Append($" Do NOT flag these words (confirmed correct): {string.Join(", ", neverFlag)}.");
            if (alwaysFlag?.Length > 0)
                foreach (var pair in alwaysFlag)
                    dictNote.Append($" \"{pair.Wrong}\" should be \"{pair.Correct}\".");

            var promptText = $"You are a strict {language} language teacher. " +
                             $"{checks}{dictNote} " +
                             "Only flag GENUINE errors. Use 0-based line_index. " +
                             "Respond with ONLY a JSON array, no explanation, no markdown: " +
                             "[{\"wrong\":\"...\",\"correct\":\"...\",\"type\":\"...\",\"line_index\":0}] " +
                             "If no errors: []\n\nTEXT:\n" + labeledText;

            var payload = new
            {
                model = Model,
                max_tokens = 2000,
                temperature = 0,
                messages = new[]
                {
                    new { role = "user", content = new[] { new { type = "text", text = promptText } } }
                }
            };

            var raw = await CallAnthropicAsync(apiKey, payload);

            // Parse JSON response
            try
            {
                var cleaned = raw.Trim()
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Trim();

                // Find the JSON array
                var start = cleaned.IndexOf('[');
                var end = cleaned.LastIndexOf(']');
                if (start >= 0 && end > start)
                    cleaned = cleaned[start..(end + 1)];

                var corrections = JsonSerializer.Deserialize<List<Correction>>(cleaned,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return corrections ?? new List<Correction>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse corrections JSON: {Raw}", raw);
                return new List<Correction>();
            }
        }

        // ── Shared HTTP call ──────────────────────────────────────
        private async Task<string> CallAnthropicAsync(string apiKey, object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            var request = new HttpRequestMessage(HttpMethod.Post, AnthropicUrl)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("x-api-key", apiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");

            var response = await _http.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Anthropic API error {Status}: {Body}", response.StatusCode, responseBody);
                throw new Exception($"Anthropic API error ({(int)response.StatusCode}): {responseBody}");
            }

            using var doc = JsonDocument.Parse(responseBody);
            var text = doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;

            return text;
        }
    }
}
