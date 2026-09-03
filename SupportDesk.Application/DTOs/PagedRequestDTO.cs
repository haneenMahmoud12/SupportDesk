using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Application.DTOs;

public sealed class PagedRequestDTO
{
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;

    public string SortColumn { get; set; } = "Id";

    [RegularExpression("^(asc|desc)$", ErrorMessage = "Sort direction must be asc or desc.")]
    public string SortDirection { get; set; } = "asc";

    [StringLength(200)]
    public string? Search { get; set; }

    [RegularExpression("^(Open|InProgress|Closed)$", ErrorMessage = "Status must be Open, InProgress, or Closed.")]
    public string? Status { get; set; } = string.Empty;

    [RegularExpression("^(Low|Medium|High)$", ErrorMessage = "Priority must be Low, Medium, or High.")]
    public string? Priority { get; set; } = string.Empty;
}
