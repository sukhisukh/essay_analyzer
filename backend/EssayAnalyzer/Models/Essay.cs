using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Essays")]
public class Essay
{
    [Key]
    public int Id { get; set; }
    public string? EssayId { get; set; }
    public int Score { get; set; }
    public string? FullText { get; set; }
    public string? Assignment { get; set; }
    public string? PromptName { get; set; }
    public string? EconomicallyDisadvantaged { get; set; }
    public string? StudentDisabilityStatus { get; set; }
    public string? EllStatus { get; set; }
    public string? RaceEthnicity { get; set; }
    public string? Gender { get; set; }
    public DateTime? SubmittedAt { get; set; }
}