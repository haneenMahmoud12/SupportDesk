using SupportDesk.Domain.Entities;

namespace SupportDesk.Application.Interfaces.Repositories;

public interface ITicketRepository : IRepository<Ticket>
{
    Task<Ticket?> GetActiveByIdAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Ticket> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        string? sortColumn,
        string? sortDirection,
        CancellationToken cancellationToken = default);
}
