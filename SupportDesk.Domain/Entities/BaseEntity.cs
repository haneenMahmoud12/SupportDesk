using System;
using System.Collections.Generic;
using System.Text;

namespace SupportDesk.Domain.Entities
{
    public abstract class BaseEntity
    {
        public long Id { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required string CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedByUserId { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
