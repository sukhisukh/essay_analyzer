using Microsoft.EntityFrameworkCore;
using EssayAnalyzer.Data;
using EssayAnalyzer.Models;

namespace EssayAnalyzer.Services
{
    public interface ILicenseService
    {
        Task<(School? school, string? error)> ValidateLicenseAsync(string licenseKey);
        Task<UsageSummary> GetUsageAsync(School school);
        Task LogCheckAsync(School school, string? teacherEmail, string? studentName, string? language, int errorsFound);
        Task<string> GenerateLicenseKeyAsync(string schoolName);
    }

    public class LicenseService : ILicenseService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<LicenseService> _logger;

        public LicenseService(ApplicationDbContext db, ILogger<LicenseService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<(School? school, string? error)> ValidateLicenseAsync(string licenseKey)
        {
            if (string.IsNullOrWhiteSpace(licenseKey))
                return (null, "License key is required.");

            var school = await _db.Schools
                .FirstOrDefaultAsync(s => s.LicenseKey == licenseKey);

            if (school == null)
                return (null, "Invalid license key. Please contact your administrator.");

            if (!school.IsActive)
                return (null, "This license has been deactivated. Please contact your administrator.");

            if (school.ExpiryDate.HasValue && school.ExpiryDate.Value < DateTime.UtcNow.Date)
                return (null, $"Your license expired on {school.ExpiryDate.Value:dd MMM yyyy}. Please renew to continue.");

            // Check monthly usage
            var usage = await GetUsageAsync(school);
            if (usage.LimitReached)
                return (null, $"Monthly limit of {usage.MonthlyLimit} checks reached ({usage.UsedThisMonth} used). " +
                              "Contact your administrator to increase the limit or wait until next month.");

            return (school, null);
        }

        public async Task<UsageSummary> GetUsageAsync(School school)
        {
            var now = DateTime.UtcNow;
            var usedThisMonth = await _db.SpellCheckLogs
                .CountAsync(l => l.SchoolId == school.Id
                              && l.Month == now.Month
                              && l.Year == now.Year);

            return new UsageSummary
            {
                UsedThisMonth = usedThisMonth,
                MonthlyLimit = school.MonthlyLimit,
                Remaining = Math.Max(0, school.MonthlyLimit - usedThisMonth),
                LimitReached = usedThisMonth >= school.MonthlyLimit
            };
        }

        public async Task LogCheckAsync(School school, string? teacherEmail, string? studentName,
            string? language, int errorsFound)
        {
            var now = DateTime.UtcNow;
            _db.SpellCheckLogs.Add(new SpellCheckLog
            {
                SchoolId = school.Id,
                LicenseKey = school.LicenseKey,
                TeacherEmail = teacherEmail,
                StudentName = studentName,
                Language = language,
                ErrorsFound = errorsFound,
                CheckedAt = now,
                Month = now.Month,
                Year = now.Year
            });
            await _db.SaveChangesAsync();
        }

        public async Task<string> GenerateLicenseKeyAsync(string schoolName)
        {
            // Generate: SCH-{ABBR}-{RANDOM}
            var abbr = new string(schoolName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w[0])
                .Take(4)
                .ToArray())
                .ToUpper();

            string key;
            do
            {
                var random = Guid.NewGuid().ToString("N")[..6].ToUpper();
                key = $"SCH-{abbr}-{random}";
            }
            while (await _db.Schools.AnyAsync(s => s.LicenseKey == key));

            return key;
        }
    }
}
