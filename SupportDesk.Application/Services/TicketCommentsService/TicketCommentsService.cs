using SupportDesk.Application.DTOs;
using SupportDesk.Application.Interfaces.Repositories;
using SupportDesk.Application.Models;
using SupportDesk.Domain.Entities;

namespace SupportDesk.Application.Services.TicketCommentsService;

public sealed class TicketCommentsService(
    ITicketCommentsRepository ticketCommentsRepository,
    ITicketRepository ticketRepository) : ITicketCommentsService
{
    public async Task<PagedResultModel<TicketCommentDTO>> GetAllTicketCommentsAsync(
        PagedRequestDTO request,
        long ticketId,
        CancellationToken cancellationToken = default)
    {
        var ticket = await ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        if (ticket is null || ticket.IsDeleted)
            throw new KeyNotFoundException($"Ticket with ID {ticketId} was not found.");

        var (items, totalCount) = await ticketCommentsRepository.GetPagedByTicketIdAsync(
            ticketId, request.PageNumber, request.PageSize, request.Search,
            request.SortColumn, request.SortDirection, cancellationToken);

        var pageNumber = Math.Max(request.PageNumber, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        return new PagedResultModel<TicketCommentDTO>
        {
            Succeeded = true,
            Items = items.Select(MapComment).ToArray(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<ResponseModel> SaveTicketCommentAsync(
        SaveTicketCommentDTO request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ArgumentException("Comment content is required.", nameof(request.Content));

        TicketComment comment;
        if (request.Id.HasValue)
        {
            comment = await ticketCommentsRepository.GetByIdAsync(request.Id.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Comment not found.");

            if (comment.IsDeleted)
                throw new KeyNotFoundException("Comment not found.");
            if (comment.TicketId != request.TicketId)
                throw new KeyNotFoundException("Comment not found on this ticket.");
            if (comment.CreatedByUserId != request.CreatedByUserId)
                throw new UnauthorizedAccessException("You can only edit your own comments.");

            comment.UpdatedAt = DateTime.UtcNow;
            comment.UpdatedByUserId = request.CreatedByUserId;
            ticketCommentsRepository.Update(comment);
        }
        else
        {
            var ticket = await ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
            if (ticket is null || ticket.IsDeleted)
                throw new KeyNotFoundException($"Ticket with ID {request.TicketId} was not found.");

            comment = new TicketComment
            {
                TicketId = request.TicketId,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = request.CreatedByUserId
            };
            await ticketCommentsRepository.AddAsync(comment, cancellationToken);
        }

        comment.Content = request.Content.Trim();
        await ticketCommentsRepository.SaveChangesAsync(cancellationToken);
        return new ResponseModel { Succeeded = true };
    }

    public async Task<ResponseModel> DeleteTicketCommentAsync(
        long id,
        long ticketId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var comment = await ticketCommentsRepository.GetByIdAsync(id, cancellationToken);
        if (comment is null || comment.IsDeleted)
            throw new KeyNotFoundException("Comment not found.");
        if (comment.TicketId != ticketId)
            throw new KeyNotFoundException("Comment not found on this ticket.");
        if (comment.CreatedByUserId != userId)
            throw new UnauthorizedAccessException("You can only delete your own comments.");

        comment.IsDeleted = true;
        comment.UpdatedAt = DateTime.UtcNow;
        comment.UpdatedByUserId = userId;
        ticketCommentsRepository.Update(comment);
        await ticketCommentsRepository.SaveChangesAsync(cancellationToken);
        return new ResponseModel { Succeeded = true };
    }

    private static TicketCommentDTO MapComment(TicketComment comment) => new()
    {
        Id = comment.Id,
        Content = comment.Content,
        CreatedAt = comment.CreatedAt,
        UpdatedAt = comment.UpdatedAt,
        TicketId = comment.TicketId,
        CreatedByUserId = comment.CreatedByUserId,
        UpdatedByUserId = comment.UpdatedByUserId
    };
}
