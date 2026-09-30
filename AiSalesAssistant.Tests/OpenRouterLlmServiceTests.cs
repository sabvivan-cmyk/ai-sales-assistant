using System.Net;
using System.Text;
using AiSalesAssistant.Configuration;
using AiSalesAssistant.Models;
using AiSalesAssistant.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AiSalesAssistant.Tests;

public sealed class OpenRouterLlmServiceTests
{
    [Fact]
    public async Task GenerateResponseAsync_ReturnsNullForInvalidStructuredContent()
    {
        const string openRouterEnvelope = """
            {
              "choices": [
                {
                  "message": {
                    "content": "this is not JSON"
                  }
                }
              ]
            }
            """;
        using var httpClient = new HttpClient(new StubHttpMessageHandler(openRouterEnvelope))
        {
            BaseAddress = new Uri("https://openrouter.example/api/v1/")
        };
        var service = new OpenRouterLlmService(
            httpClient,
            Options.Create(new LlmOptions
            {
                ApiKey = "test-key-not-a-real-secret",
                BaseUrl = "https://openrouter.example/api/v1",
                Model = "openrouter/free"
            }),
            NullLogger<OpenRouterLlmService>.Instance);

        var result = await service.GenerateResponseAsync(
            "Нужен интернет-магазин",
            CreateKnowledgeItem(),
            []);

        Assert.Null(result);
    }

    private static KnowledgeBaseItem CreateKnowledgeItem()
    {
        return new KnowledgeBaseItem
        {
            Id = "online-store",
            Name = "Разработка интернет-магазина",
            Description = "Интернет-магазин с каталогом и корзиной.",
            PriceDescription = "от 450 000 ₽"
        };
    }

    private sealed class StubHttpMessageHandler(string responseBody) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }
}
