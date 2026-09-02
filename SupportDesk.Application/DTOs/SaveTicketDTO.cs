using SupportDesk.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportDesk.Application.DTOs
{
    public class SaveTicketDTO
    {
        public long? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string? CreatedByUserId { get; set; }
        public string? UpdatedByUserId { get; set; }
    }
}
