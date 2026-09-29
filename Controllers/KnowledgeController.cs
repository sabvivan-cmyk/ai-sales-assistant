using AiSalesAssistant.Models;
using AiSalesAssistant.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiSalesAssistant.Controllers;

[ApiController]
[Route("api/knowledge")]
public sealed class KnowledgeController(IKnowledgeBaseService knowledgeBaseService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<KnowledgeBaseItem>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<KnowledgeBaseItem>> Get()
    {
        return Ok(knowledgeBaseService.GetItems());
    }
}
