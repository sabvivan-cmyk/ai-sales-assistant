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
    public async Task<ActionResult<AssistantResponse>> Post(
        [FromBody] AssistantRequest request,
        CancellationToken cancellationToken)
    {
        var response = await assistantService.CreateResponseAsync(request.Message, cancellationToken);
        return Ok(response);
    }
}
