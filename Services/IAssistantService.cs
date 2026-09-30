using AiSalesAssistant.Models;

namespace AiSalesAssistant.Services;

public interface IAssistantService
{
    Task<AssistantResponse> CreateResponseAsync(string message, CancellationToken cancellationToken = default);
}
