using SupportDesk.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportDesk.Application.DTOs
{
    public class TicketDTO
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public TicketStatus Status { get; set; }
        public TicketPriority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserId { get; set; } = null!;
    }
}
