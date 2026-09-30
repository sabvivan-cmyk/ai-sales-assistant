namespace AiSalesAssistant.Models;

public sealed class AssistantResponse
{
    public required string ClientReply { get; init; }

    public required string ManagerSuggestion { get; init; }
}
