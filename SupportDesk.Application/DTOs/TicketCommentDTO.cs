using SupportDesk.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportDesk.Application.DTOs
{
    public class TicketCommentDTO
    {
        public long Id { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public long TicketId { get; set; }
        public string CreatedByUserId { get; set; } = null!;
        public string? UpdatedByUserId { get; set; }
    }
}
