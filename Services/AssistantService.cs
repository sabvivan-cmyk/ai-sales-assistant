using System.Text.RegularExpressions;
using AiSalesAssistant.Models;

namespace AiSalesAssistant.Services;

public sealed partial class AssistantService(IKnowledgeBaseService knowledgeBaseService) : IAssistantService
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "без", "вам", "ваш", "для", "его", "или", "как", "ли", "можно", "мы", "нам", "наш",
        "она", "они", "оно", "потом", "при", "про", "это", "этот", "хотим", "хочу"
    };

    public AssistantResponse CreateResponse(string message)
    {
        var knowledgeItems = knowledgeBaseService.GetItems();
        var messageTokens = Tokenize(message);

        var rankedItems = knowledgeItems
            .Select((item, index) => new RankedItem(item, CalculateScore(item, messageTokens), index))
            .Where(result => result.Score > 0)
            .OrderByDescending(result => result.Score)
            .ThenBy(result => result.Index)
            .ToArray();

        var relevantItem = rankedItems.FirstOrDefault()?.Item;
        if (relevantItem is null)
        {
            return new AssistantResponse
            {
                ClientReply = "Спасибо за обращение. В нашей базе знаний пока недостаточно информации, чтобы дать точный ответ. Менеджер уточнит детали и вернётся к вам с информацией.",
                ManagerSuggestion = "Релевантная услуга и обоснованная допродажа по текущей базе знаний не определены. Уточните потребность клиента."
            };
        }

        var upsellItem = FindUpsell(relevantItem, knowledgeItems, messageTokens);

        return new AssistantResponse
        {
            ClientReply = $"Здравствуйте! По вашему обращению подходит услуга «{relevantItem.Name}». {relevantItem.Description} Ориентировочная стоимость: {relevantItem.PriceDescription}. Менеджер уточнит детали и объём работ.",
            ManagerSuggestion = upsellItem is null
                ? "В базе знаний нет связанной услуги для обоснованной допродажи."
                : $"Можно предложить связанную услугу «{upsellItem.Name}». {upsellItem.Description} Ориентировочная стоимость: {upsellItem.PriceDescription}."
        };
    }

    private static KnowledgeBaseItem? FindUpsell(
        KnowledgeBaseItem relevantItem,
        IReadOnlyList<KnowledgeBaseItem> knowledgeItems,
        IReadOnlySet<string> messageTokens)
    {
        var itemsById = knowledgeItems.ToDictionary(item => item.Id, StringComparer.OrdinalIgnoreCase);

        return relevantItem.UpsellServiceIds
            .Select((id, index) => itemsById.TryGetValue(id, out var item)
                ? new RankedItem(item, CalculateScore(item, messageTokens), index)
                : null)
            .Where(result => result is not null)
            .OrderByDescending(result => result!.Score)
            .ThenBy(result => result!.Index)
            .Select(result => result!.Item)
            .FirstOrDefault();
    }

    private static int CalculateScore(KnowledgeBaseItem item, IReadOnlySet<string> messageTokens)
    {
        var nameTokens = Tokenize(item.Name);
        var descriptionTokens = Tokenize(item.Description);

        return messageTokens.Sum(token =>
            GetTokenScore(token, nameTokens, exactMatchScore: 4, partialMatchScore: 2)
            + GetTokenScore(token, descriptionTokens, exactMatchScore: 1, partialMatchScore: 1));
    }

    private static int GetTokenScore(
        string messageToken,
        IReadOnlySet<string> candidateTokens,
        int exactMatchScore,
        int partialMatchScore)
    {
        if (candidateTokens.Contains(messageToken))
        {
            return exactMatchScore;
        }

        return candidateTokens.Any(candidateToken =>
            messageToken.Length >= 5
            && candidateToken.Length >= 5
            && (messageToken.StartsWith(candidateToken, StringComparison.Ordinal)
                || candidateToken.StartsWith(messageToken, StringComparison.Ordinal)))
            ? partialMatchScore
            : 0;
    }

    private static HashSet<string> Tokenize(string text)
    {
        return WordRegex()
            .Matches(text.ToLowerInvariant())
            .Select(match => match.Value)
            .Where(word => word.Length >= 2 && !StopWords.Contains(word))
            .ToHashSet(StringComparer.Ordinal);
    }

    [GeneratedRegex(@"[\p{L}\p{N}]+", RegexOptions.CultureInvariant)]
    private static partial Regex WordRegex();

    private sealed record RankedItem(KnowledgeBaseItem Item, int Score, int Index);
}
