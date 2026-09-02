namespace SupportDesk.Domain.Entities;

public class TicketComment : BaseEntity
{
    public string Content { get; set; } = null!;
    public long TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
}
