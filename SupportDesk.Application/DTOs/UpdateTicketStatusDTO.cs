using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Application.DTOs;

public sealed class UpdateTicketStatusDTO
{
    [Required]
    [RegularExpression("^(Open|InProgress|Closed)$", ErrorMessage = "Status must be Open, InProgress, or Closed.")]
    public string Status { get; set; } = string.Empty;
}
