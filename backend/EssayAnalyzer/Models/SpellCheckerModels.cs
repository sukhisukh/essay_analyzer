using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EssayAnalyzer.Models
{
    // ── Database entities ────────────────────────────────────────

    [Table("Schools")]
    public class School
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string LicenseKey { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string AnthropicApiKey { get; set; } = string.Empty;

        public int MonthlyLimit { get; set; } = 500;
        public bool IsActive { get; set; } = true;
        public DateTime? ExpiryDate { get; set; }

        [MaxLength(200)]
        public string? ContactEmail { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<SpellCheckLog> Logs { get; set; } = new List<SpellCheckLog>();
    }

    [Table("SpellCheckLogs")]
    public class SpellCheckLog
    {
        public int Id { get; set; }
        public int SchoolId { get; set; }

        [MaxLength(50)]
        public string LicenseKey { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? TeacherEmail { get; set; }

        [MaxLength(200)]
        public string? StudentName { get; set; }

        [MaxLength(50)]
        public string? Language { get; set; }

        public int? ErrorsFound { get; set; }
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
        public int Month { get; set; }
        public int Year { get; set; }

        public School? School { get; set; }
    }

    // ── Request / Response DTOs ──────────────────────────────────

    public class SpellCheckRequest
    {
        [Required]
        public string LicenseKey { get; set; } = string.Empty;

        [Required]
        public string ImageBase64 { get; set; } = string.Empty;

        [Required]
        public string MimeType { get; set; } = "image/jpeg";

        [Required]
        public string Language { get; set; } = "Marathi";

        public string? TeacherEmail { get; set; }
        public string? StudentName { get; set; }

        // School dictionary injected by client
        public string[]? NeverFlagWords { get; set; }
        public AlwaysFlagPair[]? AlwaysFlagPairs { get; set; }
    }

    public class AlwaysFlagPair
    {
        public string Wrong { get; set; } = string.Empty;
        public string Correct { get; set; } = string.Empty;
    }

    public class SpellCheckResponse
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? OcrText { get; set; }
        public List<Correction> Corrections { get; set; } = new();
        public UsageSummary? Usage { get; set; }
    }

    public class Correction
    {
        public string Wrong { get; set; } = string.Empty;
        public string Correct { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int LineIndex { get; set; }
    }

    public class UsageSummary
    {
        public int UsedThisMonth { get; set; }
        public int MonthlyLimit { get; set; }
        public int Remaining { get; set; }
        public bool LimitReached { get; set; }
    }

    public class LicenseCheckResponse
    {
        public bool Valid { get; set; }
        public string? SchoolName { get; set; }
        public string? Error { get; set; }
        public UsageSummary? Usage { get; set; }
    }

    // ── Admin DTOs ───────────────────────────────────────────────

    public class CreateSchoolRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string AnthropicApiKey { get; set; } = string.Empty;

        public int MonthlyLimit { get; set; } = 500;
        public DateTime? ExpiryDate { get; set; }
        public string? ContactEmail { get; set; }
        public string? Notes { get; set; }
    }

    public class SchoolSummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LicenseKey { get; set; } = string.Empty;
        public int MonthlyLimit { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? ContactEmail { get; set; }
        public int UsedThisMonth { get; set; }
        public int Remaining { get; set; }
    }
}
