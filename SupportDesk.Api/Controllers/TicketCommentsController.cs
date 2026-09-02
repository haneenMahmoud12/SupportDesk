using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Application.Constants;
using SupportDesk.Application.DTOs;
using SupportDesk.Application.Models;
using SupportDesk.Application.Services.TicketCommentsService;

namespace SupportDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets/{ticketId:long}/comments")]
public sealed class TicketCommentsController(ITicketCommentsService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        long ticketId, [FromQuery] PagedRequestDTO request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = CurrentUserId();
            if (userId is null) return Unauthorized(Failure("The authenticated user ID is missing."));
            return Ok(await service.GetAllTicketCommentsAsync(
                request, ticketId, userId, IsAdmin(), cancellationToken));
        }
        catch (KeyNotFoundException exception) { return NotFound(Failure(exception.Message)); }
        catch (UnauthorizedAccessException exception) { return Forbidden(exception.Message); }
        catch (Exception) { return InternalServerError(); }
    }

    [HttpPost]
    public async Task<IActionResult> Save(
        long ticketId, SaveTicketCommentDTO comment,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = CurrentUserId();
            if (userId is null) return Unauthorized(Failure("The authenticated user ID is missing."));
            return Ok(await service.SaveTicketCommentAsync(
                comment, ticketId, userId, IsAdmin(), cancellationToken));
        }
        catch (KeyNotFoundException exception) { return NotFound(Failure(exception.Message)); }
        catch (UnauthorizedAccessException exception) { return Forbidden(exception.Message); }
        catch (ArgumentException exception) { return BadRequest(Failure(exception.Message)); }
        catch (Exception) { return InternalServerError(); }
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(
        long ticketId, long id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = CurrentUserId();
            if (userId is null) return Unauthorized(Failure("The authenticated user ID is missing."));
            return Ok(await service.DeleteTicketCommentAsync(
                id, ticketId, userId, IsAdmin(), cancellationToken));
        }
        catch (KeyNotFoundException exception) { return NotFound(Failure(exception.Message)); }
        catch (UnauthorizedAccessException exception) { return Forbidden(exception.Message); }
        catch (Exception) { return InternalServerError(); }
    }

    private string? CurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
    private bool IsAdmin() => User.IsInRole(RoleNames.Admin);
    private ObjectResult Forbidden(string message) => StatusCode(
        StatusCodes.Status403Forbidden, Failure(message));
    private ObjectResult InternalServerError() => StatusCode(
        StatusCodes.Status500InternalServerError, Failure("An unexpected error occurred."));
    private static ResponseModel Failure(string message) => new()
    {
        Succeeded = false,
        Errors = [message]
    };
}
