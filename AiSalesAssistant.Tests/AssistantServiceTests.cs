using AiSalesAssistant.Models;
using AiSalesAssistant.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace AiSalesAssistant.Tests;

public sealed class AssistantServiceTests
{
    [Fact]
    public async Task CreateResponseAsync_ReturnsSuccessfulLlmResponse()
    {
        var expected = new AssistantResponse
        {
            ClientReply = "Подготовим интернет-магазин и уточним требования.",
            ManagerSuggestion = "Предложите интеграцию с CRM для передачи заказов."
        };
        var llmService = new StubLlmService { Response = expected };
        var service = CreateService(llmService);

        var actual = await service.CreateResponseAsync("Хотим интернет-магазин с подключением к CRM");

        Assert.Same(expected, actual);
        Assert.Equal(1, llmService.CallCount);
        Assert.Equal("online-store", llmService.RelevantService?.Id);
        Assert.Equal("crm-integration", llmService.AllowedUpsellServices.First().Id);
    }

    [Fact]
    public async Task CreateResponseAsync_DoesNotCallLlmForUnknownRequest()
    {
        var llmService = new StubLlmService();
        var service = CreateService(llmService);

        var actual = await service.CreateResponseAsync(
            "Проводите ли вы обучение сотрудников работе с бухгалтерской программой?");

        Assert.Equal(0, llmService.CallCount);
        Assert.Contains("недостаточно информации", actual.ClientReply);
        Assert.Contains("не определены", actual.ManagerSuggestion);
    }

    [Fact]
    public async Task CreateResponseAsync_ReturnsKnowledgeFallbackWhenLlmFails()
    {
        var llmService = new StubLlmService
        {
            Exception = new HttpRequestException("Simulated OpenRouter failure")
        };
        var service = CreateService(llmService);

        var actual = await service.CreateResponseAsync("Нужен интернет-магазин");

        Assert.Contains("Разработка интернет-магазина", actual.ClientReply);
        Assert.Contains("Личный кабинет клиента", actual.ManagerSuggestion);
    }

    private static AssistantService CreateService(StubLlmService llmService)
    {
        return new AssistantService(
            new StubKnowledgeBaseService(CreateKnowledgeBase()),
            llmService,
            NullLogger<AssistantService>.Instance);
    }

    private static IReadOnlyList<KnowledgeBaseItem> CreateKnowledgeBase()
    {
        return
        [
            new KnowledgeBaseItem
            {
                Id = "online-store",
                Name = "Разработка интернет-магазина",
                Description = "Интернет-магазин с каталогом, корзиной и онлайн-оплатой.",
                PriceDescription = "от 450 000 ₽",
                UpsellServiceIds = ["customer-account", "crm-integration"]
            },
            new KnowledgeBaseItem
            {
                Id = "customer-account",
                Name = "Личный кабинет клиента",
                Description = "Профиль клиента и история заказов.",
                PriceDescription = "от 180 000 ₽"
            },
            new KnowledgeBaseItem
            {
                Id = "crm-integration",
                Name = "Интеграция с CRM",
                Description = "Передача заказов и клиентских данных в CRM.",
                PriceDescription = "от 90 000 ₽"
            }
        ];
    }

    private sealed class StubKnowledgeBaseService(IReadOnlyList<KnowledgeBaseItem> items)
        : IKnowledgeBaseService
    {
        public IReadOnlyList<KnowledgeBaseItem> GetItems() => items;
    }

    private sealed class StubLlmService : ILlmService
    {
        public AssistantResponse? Response { get; init; }

        public Exception? Exception { get; init; }

        public int CallCount { get; private set; }

        public KnowledgeBaseItem? RelevantService { get; private set; }

        public IReadOnlyList<KnowledgeBaseItem> AllowedUpsellServices { get; private set; } = [];

        public Task<AssistantResponse?> GenerateResponseAsync(
            string clientMessage,
            KnowledgeBaseItem relevantService,
            IReadOnlyList<KnowledgeBaseItem> allowedUpsellServices,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            RelevantService = relevantService;
            AllowedUpsellServices = allowedUpsellServices;

            return Exception is null
                ? Task.FromResult(Response)
                : Task.FromException<AssistantResponse?>(Exception);
        }
    }
}
