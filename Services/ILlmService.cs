using AiSalesAssistant.Models;

namespace AiSalesAssistant.Services;

public interface ILlmService
{
    Task<AssistantResponse?> GenerateResponseAsync(
        string clientMessage,
        KnowledgeBaseItem relevantService,
        IReadOnlyList<KnowledgeBaseItem> allowedUpsellServices,
        CancellationToken cancellationToken = default);
}
