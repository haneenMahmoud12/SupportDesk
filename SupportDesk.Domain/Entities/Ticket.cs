using SupportDesk.Domain.Enums;

namespace SupportDesk.Domain.Entities;

public class Ticket
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedByUserId { get; set; } = null!;
}
