namespace SupportDesk.Domain.Entities;

public class TicketComment
{
    public long Id { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public long TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
    public string CreatedByUserId { get; set; } = null!;
}
