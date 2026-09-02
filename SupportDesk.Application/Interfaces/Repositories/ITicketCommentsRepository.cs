using SupportDesk.Domain.Entities;

namespace SupportDesk.Application.Interfaces.Repositories;

public interface ITicketCommentsRepository : IRepository<TicketComment>
{
    Task<(IReadOnlyList<TicketComment> Items, int TotalCount)> GetPagedByTicketIdAsync(
        long ticketId,
        int pageNumber,
        int pageSize,
        string? search,
        string? sortColumn,
        string? sortDirection,
        CancellationToken cancellationToken = default);
}
