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
}
