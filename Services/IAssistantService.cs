using AiSalesAssistant.Models;

namespace AiSalesAssistant.Services;

public interface IAssistantService
{
    AssistantResponse CreateResponse(string message);
}
