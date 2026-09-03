using SupportDesk.Application.DTOs;
using SupportDesk.Application.Interfaces.Repositories;
using SupportDesk.Application.Models;
using SupportDesk.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace SupportDesk.Application.Services.TicketCommentsService;

public sealed class TicketCommentsService(
    ITicketCommentsRepository commentsRepository,
    ITicketRepository ticketRepository,
    ILogger<TicketCommentsService> logger) : ITicketCommentsService
{
    public async Task<PagedResultModel<TicketCommentDTO>> GetAllTicketCommentsAsync(
        PagedRequestDTO request, long ticketId, string userId, bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        await EnsureTicketAccessAsync(ticketId, userId, isAdmin, cancellationToken);
        var (items, totalCount) = await commentsRepository.GetPagedByTicketIdAsync(
            ticketId, request.PageNumber, request.PageSize, request.Search,
            request.SortColumn, request.SortDirection, cancellationToken);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        return new PagedResultModel<TicketCommentDTO>
        {
            Succeeded = true,
            Items = items.Select(MapComment).ToArray(),
            TotalCount = totalCount,
            PageNumber = Math.Max(request.PageNumber, 1),
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<ResponseModel> SaveTicketCommentAsync(
        SaveTicketCommentDTO request, long ticketId, string userId, bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ArgumentException("Comment content is required.", nameof(request.Content));

        await EnsureTicketAccessAsync(ticketId, userId, isAdmin, cancellationToken);
        TicketComment comment;

        if (request.Id.HasValue)
        {
            comment = await commentsRepository.GetByIdAsync(request.Id.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Comment not found.");
            if (comment.IsDeleted || comment.TicketId != ticketId)
                throw new KeyNotFoundException("Comment not found on this ticket.");
            if (!isAdmin && comment.CreatedByUserId != userId)
                throw new UnauthorizedAccessException("You can only edit your own comments.");

            comment.UpdatedAt = DateTime.UtcNow;
            comment.UpdatedByUserId = userId;
            commentsRepository.Update(comment);
        }
        else
        {
            comment = new TicketComment
            {
                TicketId = ticketId,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId
            };
            await commentsRepository.AddAsync(comment, cancellationToken);
        }

        comment.Content = request.Content.Trim();
        await commentsRepository.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            request.Id.HasValue
                ? "Comment {CommentId} on ticket {TicketId} updated by user {UserId}"
                : "Comment {CommentId} added to ticket {TicketId} by user {UserId}",
            comment.Id,
            ticketId,
            userId);
        return new ResponseModel { Succeeded = true };
    }

    public async Task<ResponseModel> DeleteTicketCommentAsync(
        long id, long ticketId, string userId, bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        await EnsureTicketAccessAsync(ticketId, userId, isAdmin, cancellationToken);
        var comment = await commentsRepository.GetByIdAsync(id, cancellationToken);
        if (comment is null || comment.IsDeleted || comment.TicketId != ticketId)
            throw new KeyNotFoundException("Comment not found on this ticket.");
        if (!isAdmin && comment.CreatedByUserId != userId)
            throw new UnauthorizedAccessException("You can only delete your own comments.");

        comment.IsDeleted = true;
        comment.UpdatedAt = DateTime.UtcNow;
        comment.UpdatedByUserId = userId;
        commentsRepository.Update(comment);
        await commentsRepository.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Comment {CommentId} on ticket {TicketId} deleted by user {UserId}",
            comment.Id,
            ticketId,
            userId);
        return new ResponseModel { Succeeded = true };
    }

    private async Task EnsureTicketAccessAsync(
        long ticketId, string userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var ticket = await ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        if (ticket is null || ticket.IsDeleted)
            throw new KeyNotFoundException($"Ticket with ID {ticketId} was not found.");
        if (!isAdmin && ticket.CreatedByUserId != userId)
            throw new UnauthorizedAccessException("You can only access your own tickets.");
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
