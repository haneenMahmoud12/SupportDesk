using SupportDesk.Application.DTOs;
using SupportDesk.Application.Models;

namespace SupportDesk.Application.Services.TicketCommentsService;

public interface ITicketCommentsService
{
    Task<PagedResultModel<TicketCommentDTO>> GetAllTicketCommentsAsync(
        PagedRequestDTO request,
        long ticketId,
        CancellationToken cancellationToken = default);

    Task<ResponseModel> SaveTicketCommentAsync(
        SaveTicketCommentDTO ticketComment,
        CancellationToken cancellationToken = default);

    Task<ResponseModel> DeleteTicketCommentAsync(
        long id,
        long ticketId,
        string userId,
        CancellationToken cancellationToken = default);
}
