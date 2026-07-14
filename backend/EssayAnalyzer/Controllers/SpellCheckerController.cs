using Microsoft.AspNetCore.Mvc;
using EssayAnalyzer.Models;
using EssayAnalyzer.Services;
using Microsoft.EntityFrameworkCore;


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

        // ── POST /api/spellchecker/check ─────────────────────────
        // Main endpoint: validate license, run OCR, run spell check, log usage
        [HttpPost("check")]
        public async Task<ActionResult<SpellCheckResponse>> Check([FromBody] SpellCheckRequest request)
        {
            // 1. Validate license and check usage
            var (school, licenseError) = await _licenseService.ValidateLicenseAsync(request.LicenseKey);
            if (school == null)
            {
                return Ok(new SpellCheckResponse
                {
                    Success = false,
                    Error = licenseError
                });
            }

            try
            {
                // 2. OCR — read the handwriting
                _logger.LogInformation("OCR started for school {SchoolId}, student {Student}",
                    school.Id, request.StudentName);

                var labeledText = await _anthropicService.OcrImageAsync(
                    school.AnthropicApiKey,
                    request.ImageBase64,
                    request.MimeType,
                    request.Language);

                var ocrText = System.Text.RegularExpressions.Regex
                    .Replace(labeledText, @"LINE_\d+:\s*", "")
                    .Trim();

                // 3. Spell check — find errors in transcription
                var corrections = await _anthropicService.SpellCheckAsync(
                    school.AnthropicApiKey,
                    labeledText,
                    request.Language,
                    request.NeverFlagWords,
                    request.AlwaysFlagPairs);

                // 4. Log this check (after success)
                await _licenseService.LogCheckAsync(
                    school,
                    request.TeacherEmail,
                    request.StudentName,
                    request.Language,
                    corrections.Count);

                // 5. Return results with updated usage
                var usage = await _licenseService.GetUsageAsync(school);

                _logger.LogInformation("Spell check complete: {Errors} errors found for {Student}",
                    corrections.Count, request.StudentName);

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
                return Ok(new SpellCheckResponse
                {
                    Success = false,
                    Error = $"Spell check failed: {ex.Message}"
                });
            }
        }

        // ── GET /api/spellchecker/license/{key} ──────────────────
        // Quick license validation — used on extension startup to verify key is valid
        [HttpGet("license/{licenseKey}")]
        public async Task<ActionResult<LicenseCheckResponse>> CheckLicense(string licenseKey)
        {
            var (school, error) = await _licenseService.ValidateLicenseAsync(licenseKey);

            if (school == null)
                return Ok(new LicenseCheckResponse { Valid = false, Error = error });

            var usage = await _licenseService.GetUsageAsync(school);
            return Ok(new LicenseCheckResponse
            {
                Valid = true,
                SchoolName = school.Name,
                Usage = usage
            });
        }
    }

    // ── Admin Controller ──────────────────────────────────────────
    // Protected with an admin key — only you can access this
    [ApiController]
    [Route("api/admin/schools")]
    public class SchoolsAdminController : ControllerBase
    {
        private readonly ILicenseService _licenseService;
        private readonly IConfiguration _config;
        private readonly Data.ApplicationDbContext _db;

        public SchoolsAdminController(
            ILicenseService licenseService,
            IConfiguration config,
            Data.ApplicationDbContext db)
        {
            _licenseService = licenseService;
            _config = config;
            _db = db;
        }

        // Simple admin key check
        private bool IsAuthorized()
        {
            Request.Headers.TryGetValue("X-Admin-Key", out var key);
            return key == _config["SpellChecker:AdminKey"];
        }

        // ── GET /api/admin/schools ────────────────────────────────
        [HttpGet]
        public async Task<ActionResult<List<SchoolSummary>>> GetAllSchools()
        {
            if (!IsAuthorized()) return Unauthorized();

            var now = DateTime.UtcNow;
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

        // ── POST /api/admin/schools ───────────────────────────────
        [HttpPost]
        public async Task<ActionResult<School>> CreateSchool([FromBody] CreateSchoolRequest request)
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

        // ── PATCH /api/admin/schools/{id}/limit ──────────────────
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

        // ── PATCH /api/admin/schools/{id}/status ─────────────────
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

