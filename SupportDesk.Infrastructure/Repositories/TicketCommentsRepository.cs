using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Interfaces.Repositories;
using SupportDesk.Domain.Entities;
using SupportDesk.Infrastructure.Data;

namespace SupportDesk.Infrastructure.Repositories;

public sealed class TicketCommentsRepository(AppDbContext context)
    : Repository<TicketComment>(context), ITicketCommentsRepository
{
    public async Task<(IReadOnlyList<TicketComment> Items, int TotalCount)>
        GetPagedByTicketIdAsync(
            long ticketId,
            int pageNumber,
            int pageSize,
            string? search,
            string? sortColumn,
            string? sortDirection,
            CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        IQueryable<TicketComment> query = Entities
            .AsNoTracking()
            .Where(comment => comment.TicketId == ticketId && !comment.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(comment => comment.Content.Contains(term));
        }

        var descending = string.Equals(
            sortDirection,
            "desc",
            StringComparison.OrdinalIgnoreCase);

        query = (sortColumn?.ToLowerInvariant(), descending) switch
        {
            ("content", false) => query.OrderBy(comment => comment.Content),
            ("content", true) => query.OrderByDescending(comment => comment.Content),
            ("updatedat", false) => query.OrderBy(comment => comment.UpdatedAt),
            ("updatedat", true) => query.OrderByDescending(comment => comment.UpdatedAt),
            ("createdat", false) => query.OrderBy(comment => comment.CreatedAt),
            ("createdat", true) => query.OrderByDescending(comment => comment.CreatedAt),
            (_, false) => query.OrderBy(comment => comment.Id),
            _ => query.OrderByDescending(comment => comment.Id)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
