using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Application.DTOs;
using SupportDesk.Application.Constants;
using SupportDesk.Application.Models;
using SupportDesk.Application.Services.TicketService;

namespace SupportDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets")]
public sealed class TicketsController(ITicketService ticketService) : ControllerBase
{
    [HttpGet("ticket/{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await ticketService.GetTicketByIdAsync(id, cancellationToken);
            if (ticket is null)
            {
                return NotFound(Failure($"Ticket with ID {id} was not found."));
            }

            return Ok(new ResponseModel<TicketDTO>
            {
                Succeeded = true,
                Data = ticket
            });
        }
        catch (Exception)
        {
            return InternalServerError();
        }
    }

    [HttpGet("tickets")]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequestDTO request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await ticketService.GetAllTicketsAsync(request, cancellationToken));
        }
        catch (Exception)
        {
            return InternalServerError();
        }
    }

    [HttpPost("saveTicket")]
    public async Task<IActionResult> Save(SaveTicketDTO ticket, CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                return Unauthorized(Failure("The authenticated user ID is missing."));
            }

            ticket.CreatedByUserId = userId;
            if (ticket.Id.HasValue)
            {
                ticket.UpdatedByUserId = userId;
            }

            return Ok(await ticketService.SaveTicketAsync(ticket, cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(Failure(exception.Message));
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

    [HttpDelete("delete/{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                return Unauthorized(Failure("The authenticated user ID is missing."));
            }

            var result = await ticketService.DeleteTicketAsync(
                id,
                userId,
                cancellationToken);

            return result.Succeeded ? Ok(result) : NotFound(result);
        }
        catch (Exception)
        {
            return InternalServerError();
        }
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPatch("{id:long}/status")]
    public async Task<IActionResult> UpdateStatus(
        long id,
        UpdateTicketStatusDTO request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Unauthorized(Failure("The authenticated user ID is missing."));

            var result = await ticketService.UpdateTicketStatusAsync(
                id,
                request.Status,
                userId,
                cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(Failure(exception.Message));
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

    private ObjectResult InternalServerError() => StatusCode(StatusCodes.Status500InternalServerError, Failure("An unexpected error occurred."));

    private static ResponseModel Failure(string message) => new()
    {
        Succeeded = false,
        Errors = [message]
    };
}
