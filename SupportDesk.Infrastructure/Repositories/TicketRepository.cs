using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Interfaces.Repositories;
using SupportDesk.Domain.Entities;
using SupportDesk.Domain.Enums;
using SupportDesk.Infrastructure.Data;

namespace SupportDesk.Infrastructure.Repositories;

public sealed class TicketRepository(AppDbContext context)
    : Repository<Ticket>(context), ITicketRepository
{
    public async Task<Ticket?> GetActiveByIdAsync(
        long id,
        CancellationToken cancellationToken = default) =>
        await Entities
            .AsNoTracking()
            .FirstOrDefaultAsync(
                ticket => ticket.Id == id && !ticket.IsDeleted,
                cancellationToken);

    public async Task<(IReadOnlyList<Ticket> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        string? sortColumn,
        string? sortDirection,
        string? userId = null,
        string? status = null,
        string? priority = null,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        IQueryable<Ticket> query = Entities
            .AsNoTracking()
            .Where(ticket => !ticket.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(ticket =>
                ticket.Title.Contains(term) || ticket.Description.Contains(term));
        }
        
        if(!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(ticket => ticket.CreatedByUserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(status)) {
            if (Enum.TryParse<TicketStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(ticket => ticket.Status == parsedStatus);
            }
        }

        if (!string.IsNullOrWhiteSpace(priority))
        {
            if (Enum.TryParse<TicketPriority>(priority, true, out var parsedPriority))
            {
                query = query.Where(ticket => ticket.Priority == parsedPriority);
            }
        }
        var descending = string.Equals(
            sortDirection,
            "desc",
            StringComparison.OrdinalIgnoreCase);

        query = (sortColumn?.ToLowerInvariant(), descending) switch
        {
            ("title", false) => query.OrderBy(ticket => ticket.Title),
            ("title", true) => query.OrderByDescending(ticket => ticket.Title),
            ("status", false) => query.OrderBy(ticket => ticket.Status),
            ("status", true) => query.OrderByDescending(ticket => ticket.Status),
            ("priority", false) => query.OrderBy(ticket => ticket.Priority),
            ("priority", true) => query.OrderByDescending(ticket => ticket.Priority),
            ("createdat", false) => query.OrderBy(ticket => ticket.CreatedAt),
            ("createdat", true) => query.OrderByDescending(ticket => ticket.CreatedAt),
            (_, true) => query.OrderByDescending(ticket => ticket.Id),
            _ => query.OrderBy(ticket => ticket.Id)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
