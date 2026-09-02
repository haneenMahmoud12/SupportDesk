using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Application.DTOs;

public sealed class UpdateTicketStatusDTO
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
