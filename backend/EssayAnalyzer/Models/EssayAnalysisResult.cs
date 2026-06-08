public class EssayAnalysisResult
{
    public int OverallScore { get; set; }
    public string? Summary { get; set; }
    public List<CategoryFeedback>? Categories { get; set; }
    public string? TopStrength { get; set; }
    public string? TopImprovement { get; set; }
    public string? BenchmarkNote { get; set; }
}

public class CategoryFeedback
{
    public string? Name { get; set; }
    public int Score { get; set; }
    public string? Feedback { get; set; }
    
}