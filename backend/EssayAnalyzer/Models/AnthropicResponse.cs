public class AnthropicResponse
{
    public List<ContentBlock>? Content { get; set; }
}

public class ContentBlock
{
    public string? Type { get; set; }
    public string? Text { get; set; }
}