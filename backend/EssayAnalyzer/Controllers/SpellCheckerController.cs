using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EssayAnalyzer.Models;
using EssayAnalyzer.Services;

namespace EssayAnalyzer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpellCheckerController : ControllerBase
    {
        private readonly ILicenseService _licenseService;
        private readonly IAnthropicService _anthropicService;
        private readonly ILogger<SpellCheckerController> _logger;

        public SpellCheckerController(
            ILicenseService licenseService,
            IAnthropicService anthropicService,
            ILogger<SpellCheckerController> logger)
        {
            _licenseService = licenseService;
            _anthropicService = anthropicService;
            _logger = logger;
        }

        // POST /api/spellchecker/check
        [HttpPost("check")]
        public async Task<ActionResult<SpellCheckResponse>> Check([FromBody] SpellCheckRequest request)
        {
            var (school, licenseError) = await _licenseService.ValidateLicenseAsync(request.LicenseKey);
            if (school == null)
                return Ok(new SpellCheckResponse { Success = false, Error = licenseError });

            try
            {
                var labeledText = await _anthropicService.OcrImageAsync(
                    school.AnthropicApiKey, request.ImageBase64, request.MimeType, request.Language);

                var ocrText = System.Text.RegularExpressions.Regex
                    .Replace(labeledText, @"LINE_\d+:\s*", "").Trim();

                var corrections = await _anthropicService.SpellCheckAsync(
                    school.AnthropicApiKey, labeledText, request.Language,
                    request.NeverFlagWords, request.AlwaysFlagPairs);

                await _licenseService.LogCheckAsync(
                    school, request.TeacherEmail, request.StudentName,
                    request.Language, corrections.Count);

                var usage = await _licenseService.GetUsageAsync(school);

                return Ok(new SpellCheckResponse
                {
                    Success = true,
                    OcrText = ocrText,
                    Corrections = corrections,
                    Usage = usage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Spell check failed for school {SchoolId}", school.Id);
                return Ok(new SpellCheckResponse { Success = false, Error = ex.Message });
            }
        }

        // GET /api/spellchecker/license/{key}
        [HttpGet("license/{licenseKey}")]
        public async Task<ActionResult<LicenseCheckResponse>> CheckLicense(string licenseKey)
        {
            var (school, error) = await _licenseService.ValidateLicenseAsync(licenseKey);
            if (school == null)
                return Ok(new LicenseCheckResponse { Valid = false, Error = error });

            var usage = await _licenseService.GetUsageAsync(school);
            return Ok(new LicenseCheckResponse { Valid = true, SchoolName = school.Name, Usage = usage });
        }
    }

    // ── Admin Controller ──────────────────────────────────────────
    [ApiController]
    [Route("api/admin/schools")]
    public class SchoolsAdminController : ControllerBase
    {
        private readonly ILicenseService _licenseService;
        private readonly IConfiguration _config;
        private readonly EssayContext _db;

        public SchoolsAdminController(
            ILicenseService licenseService,
            IConfiguration config,
            EssayContext db)
        {
            _licenseService = licenseService;
            _config = config;
            _db = db;
        }

        private bool IsAuthorized()
        {
            Request.Headers.TryGetValue("X-Admin-Key", out var key);
            return key == _config["SpellChecker:AdminKey"];
        }

        // GET /api/admin/schools
        [HttpGet]
        public async Task<ActionResult<List<SchoolSummary>>> GetAllSchools()
        {
            if (!IsAuthorized()) return Unauthorized();

            var schools = await _db.Schools.ToListAsync();
            var result = new List<SchoolSummary>();

            foreach (var school in schools)
            {
                var usage = await _licenseService.GetUsageAsync(school);
                result.Add(new SchoolSummary
                {
                    Id = school.Id,
                    Name = school.Name,
                    LicenseKey = school.LicenseKey,
                    MonthlyLimit = school.MonthlyLimit,
                    IsActive = school.IsActive,
                    ExpiryDate = school.ExpiryDate,
                    ContactEmail = school.ContactEmail,
                    UsedThisMonth = usage.UsedThisMonth,
                    Remaining = usage.Remaining
                });
            }

            return Ok(result);
        }

        // POST /api/admin/schools
        [HttpPost]
        public async Task<ActionResult> CreateSchool([FromBody] CreateSchoolRequest request)
        {
            if (!IsAuthorized()) return Unauthorized();

            var licenseKey = await _licenseService.GenerateLicenseKeyAsync(request.Name);
            var school = new School
            {
                Name = request.Name,
                LicenseKey = licenseKey,
                AnthropicApiKey = request.AnthropicApiKey,
                MonthlyLimit = request.MonthlyLimit,
                IsActive = true,
                ExpiryDate = request.ExpiryDate,
                ContactEmail = request.ContactEmail,
                Notes = request.Notes
            };

            _db.Schools.Add(school);
            await _db.SaveChangesAsync();

            return Ok(new { school.Id, school.Name, school.LicenseKey, school.MonthlyLimit });
        }

        // PATCH /api/admin/schools/{id}/limit
        [HttpPatch("{id}/limit")]
        public async Task<ActionResult> UpdateLimit(int id, [FromBody] UpdateLimitRequest request)
        {
            if (!IsAuthorized()) return Unauthorized();
            var school = await _db.Schools.FindAsync(id);
            if (school == null) return NotFound();
            school.MonthlyLimit = request.NewLimit;
            school.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { school.Id, school.Name, school.MonthlyLimit });
        }

        // PATCH /api/admin/schools/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<ActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            if (!IsAuthorized()) return Unauthorized();
            var school = await _db.Schools.FindAsync(id);
            if (school == null) return NotFound();
            school.IsActive = request.IsActive;
            school.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { school.Id, school.Name, school.IsActive });
        }
    }

    public class UpdateLimitRequest { public int NewLimit { get; set; } }
    public class UpdateStatusRequest { public bool IsActive { get; set; } }
}
