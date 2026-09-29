using System.Text.Json;
using AiSalesAssistant.Models;

namespace AiSalesAssistant.Services;

public sealed class KnowledgeBaseService : IKnowledgeBaseService
{
    private readonly ILogger<KnowledgeBaseService> _logger;
    private readonly string _filePath;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public KnowledgeBaseService(
        IWebHostEnvironment environment,
        ILogger<KnowledgeBaseService> logger)
    {
        _logger = logger;
        _filePath = Path.Combine(environment.ContentRootPath, "Data", "knowledge-base.json");
    }

    public IReadOnlyList<KnowledgeBaseItem> GetItems()
    {
        if (!File.Exists(_filePath))
        {
            _logger.LogError("Knowledge base file was not found at {FilePath}", _filePath);
            return [];
        }

        try
        {
            using var stream = File.OpenRead(_filePath);
            return JsonSerializer.Deserialize<List<KnowledgeBaseItem>>(stream, _serializerOptions) ?? [];
        }
        catch (JsonException exception)
        {
            _logger.LogError(exception, "Knowledge base file contains invalid JSON: {FilePath}", _filePath);
            return [];
        }
        catch (IOException exception)
        {
            _logger.LogError(exception, "Knowledge base file could not be read: {FilePath}", _filePath);
            return [];
        }
        catch (UnauthorizedAccessException exception)
        {
            _logger.LogError(exception, "Access to the knowledge base file was denied: {FilePath}", _filePath);
            return [];
        }
    }
}
