using System.ComponentModel.DataAnnotations;

namespace AiSalesAssistant.Models;

public sealed class AssistantRequest
{
    public const int MaxMessageLength = 2000;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Message is required.")]
    [StringLength(MaxMessageLength, ErrorMessage = "Message must not exceed 2000 characters.")]
    public string Message { get; init; } = string.Empty;
}
