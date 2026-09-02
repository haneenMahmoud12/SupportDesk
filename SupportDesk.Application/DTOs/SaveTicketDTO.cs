using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Application.DTOs;

public sealed class SaveTicketDTO
{
    public long? Id { get; set; }

    [Required, StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(4000, MinimumLength = 3)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Low|Medium|High)$", ErrorMessage = "Priority must be Low, Medium, or High.")]
    public string Priority { get; set; } = string.Empty;
}
