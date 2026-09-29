using AiSalesAssistant.Models;

namespace AiSalesAssistant.Services;

public interface IKnowledgeBaseService
{
    IReadOnlyList<KnowledgeBaseItem> GetItems();
}
