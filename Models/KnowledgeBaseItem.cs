namespace AiSalesAssistant.Models;

public sealed class KnowledgeBaseItem
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string PriceDescription { get; init; }

    public IReadOnlyList<string> UpsellServiceIds { get; init; } = [];
}
