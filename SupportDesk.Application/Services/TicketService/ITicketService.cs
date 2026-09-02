using SupportDesk.Application.DTOs;
using SupportDesk.Application.Models;

namespace SupportDesk.Application.Services.TicketService;

public interface ITicketService
{
    Task<TicketDTO?> GetTicketByIdAsync(
        long id, string userId, bool isAdmin,
        CancellationToken cancellationToken = default);

    Task<PagedResultModel<TicketDTO>> GetAllTicketsAsync(
        PagedRequestDTO request, CancellationToken cancellationToken = default);

    Task<PagedResultModel<TicketDTO>> GetAllUserTicketsAsync(
        PagedRequestDTO request, string userId,
        CancellationToken cancellationToken = default);

    Task<ResponseModel> SaveTicketAsync(
        SaveTicketDTO ticket, string userId, bool isAdmin,
        CancellationToken cancellationToken = default);

    Task<ResponseModel> UpdateTicketStatusAsync(
        long id, string status, string userId,
        CancellationToken cancellationToken = default);

    Task<ResponseModel> DeleteTicketAsync(
        long id, string userId, bool isAdmin,
        CancellationToken cancellationToken = default);
}
