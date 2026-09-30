using AiSalesAssistant.Models;
using AiSalesAssistant.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiSalesAssistant.Controllers;

[ApiController]
[Route("api/assistant")]
public sealed class AssistantController(IAssistantService assistantService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<AssistantResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<AssistantResponse> Post([FromBody] AssistantRequest request)
    {
        return Ok(assistantService.CreateResponse(request.Message));
    }
}
