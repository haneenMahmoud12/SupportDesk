using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Application.DTOs;

public sealed class SaveTicketCommentDTO
{
    public long? Id { get; set; }

    [Required]
    [StringLength(4000, MinimumLength = 1)]
    public string Content { get; set; } = string.Empty;
}
