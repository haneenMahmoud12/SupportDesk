using System;
using System.Collections.Generic;
using System.Text;

namespace SupportDesk.Application.DTOs
{
    public class PagedRequestDTO
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortColumn { get; set; } = "Id";
        public string SortDirection { get; set; } = "asc";
        public string? Search { get; set; } = string.Empty;
    }
}
