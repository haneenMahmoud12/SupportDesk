using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Application.DTOs;
using SupportDesk.Application.Models;
using SupportDesk.Application.Services.TicketCommentsService;

namespace SupportDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets/{ticketId:long}/comments")]
public sealed class TicketCommentsController(
    ITicketCommentsService ticketCommentsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(long ticketId, [FromQuery] PagedRequestDTO request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await ticketCommentsService.GetAllTicketCommentsAsync(request, ticketId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(Failure(exception.Message));
        }
        catch (Exception)
        {
            return InternalServerError();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Save(SaveTicketCommentDTO comment, CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Unauthorized(Failure("The authenticated user ID is missing."));

            comment.CreatedByUserId = userId;
            var result = await ticketCommentsService.SaveTicketCommentAsync(
                comment, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(Failure(exception.Message));
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, Failure(exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(Failure(exception.Message));
        }
        catch (Exception)
        {
            return InternalServerError();
        }
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long ticketId, long id,CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Unauthorized(Failure("The authenticated user ID is missing."));

            var result = await ticketCommentsService.DeleteTicketCommentAsync(
                id, ticketId, userId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(Failure(exception.Message));
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, Failure(exception.Message));
        }
        catch (Exception)
        {
            return InternalServerError();
        }
    }

    private static ResponseModel Failure(string message) => new()
    {
        Succeeded = false,
        Errors = [message]
    };

    private ObjectResult InternalServerError() => StatusCode(
        StatusCodes.Status500InternalServerError,
        Failure("An unexpected error occurred."));
}
