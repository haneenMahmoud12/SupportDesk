using System;
using System.Collections.Generic;
using System.Text;

namespace SupportDesk.Application.Models
{
    public class PagedResultModel<T> : ResponseModel
    {
        public IEnumerable<T> Items { get; init; } = [];
        public int TotalCount { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalPages { get; init; }
    }
}
