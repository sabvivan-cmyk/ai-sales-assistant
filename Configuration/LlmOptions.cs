namespace AiSalesAssistant.Configuration;

public sealed class LlmOptions
{
    public const string SectionName = "Llm";

    public string Model { get; init; } = "openrouter/free";

    public string BaseUrl { get; init; } = "https://openrouter.ai/api/v1";

    public string ApiKey { get; init; } = string.Empty;

    public int TimeoutSeconds { get; init; } = 30;
}
