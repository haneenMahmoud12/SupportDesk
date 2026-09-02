using SupportDesk.Domain.Enums;

namespace SupportDesk.Domain.Entities;

public class Ticket : BaseEntity
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public ICollection<TicketComment> Comments { get; set; } = [];
}
