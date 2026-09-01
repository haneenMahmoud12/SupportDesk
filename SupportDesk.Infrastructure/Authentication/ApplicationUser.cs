using Microsoft.AspNetCore.Identity;
using SupportDesk.Domain.Entities;

namespace SupportDesk.Infrastructure.Authentication;

public class ApplicationUser : IdentityUser
{
    public ICollection<Ticket> CreatedTickets { get; set; } = [];
    public ICollection<TicketComment> CreatedComments { get; set; } = [];
}
