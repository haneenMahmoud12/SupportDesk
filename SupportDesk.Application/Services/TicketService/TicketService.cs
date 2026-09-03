using System;
using Microsoft.Extensions.Logging;
using SupportDesk.Application.DTOs;
using SupportDesk.Application.Exceptions;
using SupportDesk.Application.Interfaces.Repositories;
using SupportDesk.Application.Models;
using SupportDesk.Domain.Entities;
using SupportDesk.Domain.Enums;

namespace SupportDesk.Application.Services.TicketService;

public sealed class TicketService(ITicketRepository ticketRepository, ILogger<TicketService> logger) : ITicketService
{
    public async Task<TicketDTO?> GetTicketByIdAsync(
        long id, string userId, bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var ticket = await ticketRepository.GetActiveByIdAsync(id, cancellationToken);
        if (ticket is not null && !isAdmin && ticket.CreatedByUserId != userId)
            throw new UnauthorizedAccessException("You can only access your own tickets.");

        return ticket is null ? null : MapTicket(ticket);
    }

    public async Task<PagedResultModel<TicketDTO>> GetAllTicketsAsync(
        PagedRequestDTO request, CancellationToken cancellationToken = default) =>
        await GetPagedAsync(request, null, cancellationToken);

    public async Task<PagedResultModel<TicketDTO>> GetAllUserTicketsAsync(
        PagedRequestDTO request, string userId,
        CancellationToken cancellationToken = default) =>
        await GetPagedAsync(request, userId, cancellationToken);

    public async Task<ResponseModel> SaveTicketAsync(
        SaveTicketDTO request, string userId, bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        Ticket ticket;
        if (request.Id.HasValue)
        {
            ticket = await ticketRepository.GetByIdAsync(request.Id.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Ticket not found.");

            if (ticket.IsDeleted)
                throw new KeyNotFoundException("Ticket not found.");
            if (!isAdmin && ticket.CreatedByUserId != userId)
                throw new UnauthorizedAccessException("You can only update your own tickets.");

            ticket.UpdatedAt = DateTime.UtcNow;
            ticket.UpdatedByUserId = userId;
            ticketRepository.Update(ticket);
        }
        else
        {
            ticket = new Ticket
            {
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId,
                Status = TicketStatus.Open
            };
            await ticketRepository.AddAsync(ticket, cancellationToken);
        }

        if (!Enum.TryParse<TicketPriority>(request.Priority, true, out var priority))
            throw new BadRequestException("Invalid ticket priority.");

        ticket.Title = request.Title.Trim();
        ticket.Description = request.Description.Trim();
        ticket.Priority = priority;

        await ticketRepository.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            request.Id.HasValue ? "Ticket {TicketId} updated by user {UserId}" : "Ticket {TicketId} created by user {UserId}",
            ticket.Id,
            userId);
        return new ResponseModel { Succeeded = true };
    }

    public async Task<ResponseModel> UpdateTicketStatusAsync(
        long id, string status, string userId,
        CancellationToken cancellationToken = default)
    {
        var ticket = await ticketRepository.GetByIdAsync(id, cancellationToken);
        if (ticket is null || ticket.IsDeleted)
            throw new KeyNotFoundException("Ticket not found.");

        if (!Enum.TryParse<TicketStatus>(status, true, out var parsedStatus))
            throw new BadRequestException("Invalid ticket status.");

        ticket.Status = parsedStatus;
        ticket.UpdatedAt = DateTime.UtcNow;
        ticket.UpdatedByUserId = userId;
        ticketRepository.Update(ticket);
        await ticketRepository.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Ticket {TicketId} status changed to {Status} by user {UserId}",
            ticket.Id,
            ticket.Status,
            userId);
        return new ResponseModel { Succeeded = true };
    }

    public async Task<ResponseModel> DeleteTicketAsync(
        long id, string userId, bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var ticket = await ticketRepository.GetByIdAsync(id, cancellationToken);
        if (ticket is null || ticket.IsDeleted)
            throw new KeyNotFoundException("Ticket not found.");
        if (!isAdmin && ticket.CreatedByUserId != userId)
            throw new UnauthorizedAccessException("You can only delete your own tickets.");

        ticket.UpdatedAt = DateTime.UtcNow;
        ticket.UpdatedByUserId = userId;
        ticket.IsDeleted = true;
        ticketRepository.Update(ticket);
        await ticketRepository.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Ticket {TicketId} deleted by user {UserId}",
            ticket.Id,
            userId);
        return new ResponseModel { Succeeded = true };
    }

    private async Task<PagedResultModel<TicketDTO>> GetPagedAsync(
        PagedRequestDTO request, string? userId, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await ticketRepository.GetPagedAsync(
            request.PageNumber, request.PageSize, request.Search,
            request.SortColumn, request.SortDirection, userId,
            request.Status, request.Priority, cancellationToken);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        return new PagedResultModel<TicketDTO>
        {
            Succeeded = true,
            Items = items.Select(MapTicket).ToArray(),
            TotalCount = totalCount,
            PageNumber = Math.Max(request.PageNumber, 1),
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    private static TicketDTO MapTicket(Ticket ticket) => new()
    {
        Id = ticket.Id,
        Title = ticket.Title,
        Description = ticket.Description,
        Status = ticket.Status,
        Priority = ticket.Priority,
        CreatedAt = ticket.CreatedAt,
        CreatedByUserId = ticket.CreatedByUserId
    };
}
